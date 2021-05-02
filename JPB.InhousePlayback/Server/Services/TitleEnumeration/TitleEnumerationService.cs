using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Server.Config.Models;
using JPB.InhousePlayback.Server.Hubs.Access;
using JPB.InhousePlayback.Server.Hubs.Impl;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.Media;
using JPB.InhousePlayback.Shared.ApiModel;
using MediaToolkit.Core;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JPB.InhousePlayback.Server.Services.TitleEnumeration
{
	public class TitleEnumerationService : IRequireInitAsync
	{
		private readonly DbService _db;
		private readonly IConfiguration _config;
		private readonly MediaService _mediaToolkitService;
		private readonly IFileSystem _fileSystem;
		private readonly ILogger<TitleEnumerationService> _logger;
		private readonly SetupHubAccess _setupHub;

		public TitleEnumerationService(DbService db,
			IConfiguration config,
			MediaService mediaToolkitService,
			IFileSystem fileSystem,
			ILogger<TitleEnumerationService> logger,
			SetupHubAccess setupHub)
		{
			_db = db;
			_config = config;
			_mediaToolkitService = mediaToolkitService;
			_fileSystem = fileSystem;
			_logger = logger;
			_setupHub = setupHub;
		}

		public bool IsEnumerating { get; set; }
		public static TitlePathModel[] Paths { get; set; }

		public async Task SyncTitles()
		{
			IsEnumerating = true;
			Paths = _config.GetSection("Paths").Get<TitlePathModel[]>();
			foreach (var path in Paths)
			{
				await Sync(EnumerateGenres(path).ToArray(), _logger, new Progress<SyncProgress>(syncProgress =>
				{
					_setupHub.SendTitleEnumerationProgress(syncProgress);
				}));
			}

			IsEnumerating = false;
		}

		private class GenresEnumeration
		{
			public string Location { get; set; }
			public IList<SeasonsEnumeration> Seasons { get; set; }
			public TitlePathModel Origin { get; set; }
		}

		private class SeasonsEnumeration
		{
			public string Location { get; set; }
			public IList<TitleEnumeration> Titles { get; set; }
		}

		private class TitleEnumeration
		{
			public string Location { get; set; }
		}

		private IEnumerable<GenresEnumeration> EnumerateGenres(TitlePathModel titlePathModel)
		{
			foreach (var subDirectory in _fileSystem.Directory.EnumerateDirectories(titlePathModel.Path))
			{
				yield return new GenresEnumeration()
				{
					Origin = titlePathModel,
					Location = BuildUri(titlePathModel, subDirectory),
					Seasons = EnumerateSeasons(titlePathModel, subDirectory).ToArray()
				};
			}
		}

		private IEnumerable<SeasonsEnumeration> EnumerateSeasons(TitlePathModel titlePathModel, string directory)
		{
			foreach (var subDirectory in _fileSystem.Directory.EnumerateDirectories(directory))
			{
				yield return new SeasonsEnumeration()
				{
					Location = BuildUri(titlePathModel, subDirectory),
					Titles = EnumerateTitles(titlePathModel, subDirectory).ToArray()
				};
			}
		}

		private IEnumerable<TitleEnumeration> EnumerateTitles(TitlePathModel titlePathModel, string directory)
		{
			var patterns = new string[]
			{
				"*.m4v"
			};
			foreach (var subDirectory in patterns.SelectMany(e => _fileSystem.Directory.EnumerateFiles(directory, e)))
			{
				yield return new TitleEnumeration()
				{
					Location = BuildUri(titlePathModel, subDirectory),
				};
			}
		}

		private static string BuildUri(TitlePathModel pathModel, string location)
		{
			return $"{pathModel.Id}://{location.Remove(0, pathModel.Path.Length + 1)}";
		}

		public static string GetUncPath(string location)
		{
			var parts = location.Split("://");
			var model = Paths.First(e => e.Id == parts[0]);
			return model.Path + "\\" + parts[1];
		}

		private async Task Sync(IList<GenresEnumeration> genres, ILogger logger, IProgress<SyncProgress> onProgress)
		{
			var maxProgress = genres.Count +
							  genres.SelectMany(e => e.Seasons).Count() +
							  genres.SelectMany(e => e.Seasons.SelectMany(f => f.Titles)).Count();

			var progress = 0;
			_db.Database.Connect();
			foreach (var genresEnumeration in genres)
			{
				var genre = _db.Query().Select.Table<Genre>()
					.Where
					.Column(f => f.Location).Is.EqualsTo(genresEnumeration.Location)
					.FirstOrDefault();

				if (genre == null)
				{
					genre = new Genre();
					genre.Location = genresEnumeration.Location;
					genre.Name = _fileSystem.Path.GetFileName(genresEnumeration.Location);
					genre = _db.InsertWithSelect(genre);
					logger.LogInformation("Created Genre " + genre.Name);
				}

				onProgress.Report(new SyncProgress(genre.Name, ++progress, maxProgress));
				foreach (var seasonsEnumeration in genresEnumeration.Seasons)
				{
					var season = _db.Query().Select.Table<Season>()
						.Where
						.Column(f => f.Location).Is.EqualsTo(seasonsEnumeration.Location)
						.FirstOrDefault();
					if (season == null)
					{
						season = new Season();
						season.Location = seasonsEnumeration.Location;
						season.Name = _fileSystem.Path.GetFileName(seasonsEnumeration.Location);
						season.IdGenre = genre.GenreId;
						var lastSeason = _db.Query().Select.Table<Season>()
							.Where
							.Column(f => f.IdGenre).Is.EqualsTo(genre.GenreId)
							.Order.By(f => f.OrderNo).Descending
							.LimitBy(1)
							.FirstOrDefault();
						season.OrderNo = (lastSeason?.OrderNo ?? -1) + 1;
						_db.Insert(season);
						logger.LogInformation("\t Created Season " + season.Name);
					}
					onProgress.Report(new SyncProgress(genre.Name, season.Name, ++progress, maxProgress));

					var titleIndex = 0;
					foreach (var titleEnumeration in seasonsEnumeration.Titles)
					{
						var title = _db.Query().Select.Table<Title>()
							.Where.Column(f => f.Location).Is.EqualsTo(titleEnumeration.Location)
							.FirstOrDefault();
						if (title == null)
						{
							title = new Title();
							title.Location = titleEnumeration.Location;
							title.Name = _fileSystem.Path.GetFileNameWithoutExtension(title.Location);
							title.OrderNo = titleIndex;
							var lastTitle = _db.Query().Select.Table<Title>()
								.Where
								.Column(f => f.IdSeason).Is.EqualsTo(season.SeasonId)
								.Order.By(f => f.OrderNo).Descending
								.LimitBy(1)
								.FirstOrDefault();
							season.OrderNo = (lastTitle?.OrderNo ?? -1) + 1;
							title.IdSeason = season.SeasonId;
							title.PlaybackLength = 0;
							onProgress.Report(new SyncProgress(genre.Name, season.Name, title.Name, ++progress, maxProgress));

							try
							{
								title.PlaybackLength = (int?)await _mediaToolkitService.GetMovieLength(GetUncPath(title.Location));
							}
							catch (Exception e)
							{
								logger.LogError(e, "Could not obtain Metadata or Thumbnail from title");
								continue;
							}

							_db.Insert(title);
							logger.LogInformation("\t\t Created Title " + title.Name);
						}
						else
						{
							onProgress.Report(new SyncProgress(genre.Name, season.Name, title.Name, ++progress, maxProgress));
						}

						titleIndex++;
					}
				}
			}

			_db.Database.CloseConnection();
		}
		/*
		private async Task SyncDirectory(string path)
		{
			var generes = _fileSystem.Directory.EnumerateDirectories(path);
			_db.Database.Connect();
			foreach (var genreLocation in generes)
			{
				var genre = _db.Query().Select.Table<Genre>()
					.Where
					.Column(f => f.Location).Is.EqualsTo(genreLocation)
					.FirstOrDefault();

				if (genre == null)
				{
					genre = new Genre();
					genre.Location = genreLocation;
					genre.Name = _fileSystem.Path.GetFileName(genreLocation);
					genre = _db.InsertWithSelect(genre);
					_logger.LogInformation("Created Genre " + genre.Name);
				}

				var array = _fileSystem.Directory.EnumerateDirectories(genreLocation)
					.OrderBy(e => _fileSystem.DirectoryInfo.FromDirectoryName(e).LastWriteTime)
					.ToArray();

				var seasons = new List<Season>();
				for (var i = 0; i < array.Length; i++)
				{
					var seasonsLocation = array[i];
					var season = _db.Query().Select.Table<Season>()
						.Where
						.Column(f => f.Location).Is.EqualsTo(seasonsLocation)
						.FirstOrDefault();
					if (season == null)
					{
						season = new Season();
						season.Location = seasonsLocation;
						season.Name = _fileSystem.Path.GetFileName(seasonsLocation);
						season.IdGenre = genre.GenreId;
						var lastSeason = _db.Query().Select.Table<Season>()
							.Where
							.Column(f => f.IdGenre).Is.EqualsTo(genre.GenreId)
							.Order.By(f => f.OrderNo).Descending
							.LimitBy(1)
							.FirstOrDefault();
						season.OrderNo = (lastSeason?.OrderNo ?? -1) + 1;
						seasons.Add(season = _db.InsertWithSelect(season));
						_logger.LogInformation("\t Created Season " + season.Name);
					}
					else
					{
						seasons.Add(season);
					}

					var titles = patterns.SelectMany(f => _fileSystem.Directory.EnumerateFiles(seasonsLocation, f))
						.OrderBy(e => _fileSystem.FileInfo.FromFileName(e).LastWriteTime)
						.ToArray();
					var titleEntites = new List<Title>();
					for (var index = 0; index < titles.Length; index++)
					{
						var titleLocation = titles[index];

						if (Path.GetExtension(titleLocation) != ".m4v")
						{
							_logger.LogWarning($"Not acceptable format. Try to convert file '{titleLocation}'");
							var oldFile = titleLocation;
							titleLocation = await _mediaToolkitService.Convert(titleLocation);
						}

						var title = _db.Query().Select.Table<Title>()
							.Where.Column(f => f.Location).Is.EqualsTo(titleLocation)
							.FirstOrDefault();
						if (title == null)
						{
							title = new Title();
							title.Location = titleLocation;
							title.Name = _fileSystem.Path.GetFileNameWithoutExtension(title.Location);
							title.OrderNo = index;
							var lastTitle = _db.Query().Select.Table<Title>()
								.Where
								.Column(f => f.IdSeason).Is.EqualsTo(season.SeasonId)
								.Order.By(f => f.OrderNo).Descending
								.LimitBy(1)
								.FirstOrDefault();
							season.OrderNo = (lastTitle?.OrderNo ?? -1) + 1;
							title.IdSeason = season.SeasonId;
							title.PlaybackLength = 0;

							try
							{
								title.PlaybackLength = (int?) await _mediaToolkitService.GetMovieLength(title.Location);
							}
							catch (Exception e)
							{
								_logger.LogError(e, "Could not obtain Metadata or Thumbnail from title");
								continue;
							}

							titleEntites.Add(_db.InsertWithSelect(title));
							_logger.LogInformation("\t\t Created Title " + title.Name);
						}
						else
						{
							titleEntites.Add(title);
						}
					}

					_db.Query().Update.Table<Season>()
						.Set
						.Column(f => f.PlaybackLength).Value(titleEntites.Select(f => f.PlaybackLength ?? 0).Sum())
						.Where
						.PrimaryKey().Is.EqualsTo(season.SeasonId)
						.ExecuteNonQuery();
				}


				_db.Query().Update.Table<Genre>()
					.Set
					.Column(f => f.PlaybackLength).Value(seasons.Select(f => f.PlaybackLength ?? 0).Sum())
					.Where
					.PrimaryKey().Is.EqualsTo(genre.GenreId)
					.ExecuteNonQuery();
			}

			_db.Database.CloseConnection();
		}
		*/
		public async Task Init()
		{
#pragma warning disable 4014
			Task.Run(async () =>
#pragma warning restore 4014
			{
				await SyncTitles();
			});
			
			if (this._config["NeedsInit"] == "True")
			{
			}
		}
	}
}
