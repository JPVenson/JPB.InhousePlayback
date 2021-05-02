using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.TitleEnumeration;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using MediaToolkit.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Operators;

namespace JPB.InhousePlayback.Server.Services.Thumbnail
{
	public class ThumbnailService : IRequireInit
	{
		private readonly IConfiguration _config;
		private readonly IFileSystem _fs;
		private readonly IMediaToolkitService _mediaToolkitService;
		private readonly IServiceProvider _serviceProvider;
		private readonly IHostApplicationLifetime _livetimeService;
		private readonly ILogger<ThumbnailService> _logger;
		private string _thumbnailBasePath;

		public ThumbnailService(IConfiguration config, IFileSystem fs,
			IMediaToolkitService mediaToolkitService,
			IServiceProvider serviceProvider,
			IHostApplicationLifetime livetimeService,
			ILogger<ThumbnailService> logger)
		{
			_config = config;
			_fs = fs;
			_mediaToolkitService = mediaToolkitService;
			_serviceProvider = serviceProvider;
			_livetimeService = livetimeService;
			_logger = logger;
			CurrentGenerations = new ConcurrentDictionary<CurrentGeneration, Task<byte[]>>();
		}

		private ConcurrentDictionary<CurrentGeneration, Task<byte[]>> CurrentGenerations { get; set; }

		private readonly struct CurrentGeneration : IEquatable<CurrentGeneration>
		{
			public CurrentGeneration(int titleId, int position)
			{
				TitleId = titleId;
				Position = position;
			}

			public int TitleId { get; }
			public int Position { get; }

			public bool Equals(CurrentGeneration other)
			{
				return TitleId == other.TitleId && Position == other.Position;
			}

			public override bool Equals(object obj)
			{
				return obj is CurrentGeneration other && Equals(other);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(TitleId, Position);
			}
		}

		public async Task<byte[]> CreateThumbnail(Title title, int position)
		{
			return await CurrentGenerations.GetOrAdd(new CurrentGeneration(title.TitleId, position), (key) =>
			{
				return CreateThumbnailImpl(title, position);
			});
		}

		private Task<byte[]> CreateThumbnailImpl(Title title, int position)
		{
			return Task.Run(async () =>
			{
				var time = TimeSpan.FromSeconds(position);
				var inputFilePath = TitleEnumerationService.GetUncPath(title.Location);
				var getThumbnailResult = await _mediaToolkitService.ExecuteAsync(new FfTaskGetThumbnail(inputFilePath,
					new GetThumbnailOptions()
					{
						SeekSpan = time,
						OutputFormat = OutputFormat.Image2,
						PixelFormat = PixelFormat.Rgba,
					}));

				var fs = new MemoryStream();
				using (var img = new MagickImage(getThumbnailResult.ThumbnailData))
				{
					img.Resize(512, 0);
					await img.WriteAsync(fs);
				}

				fs.Position = 0;
				return fs.ToArray();
			});
		}

		public async Task<string> CreateThumbnail(Title title, bool fromEnd, int position)
		{
			var posInStream = fromEnd ? title.PlaybackLength.Value - position : position;
			var cachedThumbnailPath = _fs.Path.Combine(_thumbnailBasePath, title.TitleId.ToString());

			if (!_fs.Directory.Exists(cachedThumbnailPath))
			{
				_fs.Directory.CreateDirectory(cachedThumbnailPath);
			}

			cachedThumbnailPath = _fs.Path.Combine(cachedThumbnailPath, posInStream.ToString() + ".jpg");
			if (!_fs.File.Exists(cachedThumbnailPath))
			{
				await _fs.File.WriteAllBytesAsync(cachedThumbnailPath, await CreateThumbnailImpl(title, posInStream));
			}
			return cachedThumbnailPath;
		}

		public async Task<Stream> GetThumbnail(Title title, bool fromEnd, int position)
		{
			return _fs.FileStream.Create(await CreateThumbnail(title, fromEnd, position), FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		private readonly struct PreviewGenTile
		{
			public PreviewGenTile(int position, Title title)
			{
				Position = position;
				Title = title;
			}

			public int Position { get; }
			public Title Title { get; }
		}

		public void StartPreviewThumbGeneration()
		{
			var metaDb = _serviceProvider.GetService<DbService>();
			var titles = metaDb.Select<Title>();
			var previewGenTiles = new ConcurrentBag<PreviewGenTile>(titles.SelectMany(e =>
			{
				var tiles = new List<PreviewGenTile>();
				for (int i = 0; i < e.PlaybackLength / 10; i++)
				{
					var position = 10 * i;
					tiles.Add(new PreviewGenTile((int)position, e));
				}

				return tiles;
			}));

			async void PullTitle()
			{
				while (previewGenTiles.TryTake(out var title) && !_livetimeService.ApplicationStopping.IsCancellationRequested)
				{
					if (previewGenTiles.Count % 10 == 0)
					{
						_logger.LogInformation($"Thumbnail Generation at {previewGenTiles.Count}");
					}

					try
					{
						await this.CreateThumbnail(title.Title, false, title.Position);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "Could not create Thumbnail");
					}
				}
			}

			var pipelines = new Thread[Environment.ProcessorCount / 2 < 1 ? 1 : Environment.ProcessorCount / 2];
			for (int index = 0; index < pipelines.Length; index++)
			{
				var pipline = pipelines[index] = new Thread(PullTitle);
				pipline.SetApartmentState(ApartmentState.MTA);
				pipline.Priority = ThreadPriority.BelowNormal;
				pipline.Start();
			}
		}

		public void Init()
		{
			_thumbnailBasePath = _config["thumbnailPath"];
			if (!_fs.Directory.Exists(_thumbnailBasePath))
			{
				_fs.Directory.CreateDirectory(_thumbnailBasePath);
			}

			//StartPreviewThumbGeneration();
		}
	}
}
