using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Server.Hubs.Access;
using JPB.InhousePlayback.Server.Hubs.Impl;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.Media;
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

		public TitleEnumerationService(DbService db,
			IConfiguration config,
			MediaService mediaToolkitService,
			IFileSystem fileSystem,
			ILogger<TitleEnumerationService> logger)
		{
			_db = db;
			_config = config;
			_mediaToolkitService = mediaToolkitService;
			_fileSystem = fileSystem;
			_logger = logger;
		}

		public bool IsEnumerating { get; set; }

		public async Task SyncTitles()
		{
			IsEnumerating = true;
			var paths = _config.GetSection("Paths").Get<string[]>();
			foreach (var path in paths)
			{
				await SyncDirectory(path);
			}

			IsEnumerating = false;
		}

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

					var patterns = new string[]
					{
						"*.m4v"
					};

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

		public async Task Init()
		{
			await SyncTitles();
			if (this._config["NeedsInit"] == "True")
			{
			}
		}
	}
}
