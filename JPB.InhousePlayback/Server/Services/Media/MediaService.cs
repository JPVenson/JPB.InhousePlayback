using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MediaToolkit.Services;
using MediaToolkit.Tasks;
using Microsoft.Extensions.Configuration;
using Format = FFMpegCore.Format;

namespace JPB.InhousePlayback.Server.Services.Media
{
	public class MediaService
	{
		private readonly IFileSystem _fileSystem;

		public MediaService(IConfiguration configuration, IMediaToolkitService mediaToolkitService, IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
			MediaToolkitService = mediaToolkitService;
			FfmpegPath = configuration["ffpmegPath"];
			GlobalFFOptions.Configure(new FFOptions
			{
				BinaryFolder = Path.GetDirectoryName(FfmpegPath),
				TemporaryFilesFolder = "/tmp"
			});
		}

		public IMediaToolkitService MediaToolkitService { get; set; }

		public string FfmpegPath { get; set; }

		public async Task<double> GetMovieLength(string filename)
		{
			var mediaAnalysis = await FFProbe.AnalyseAsync(filename);
			return mediaAnalysis.Duration.TotalSeconds;

			//var result = await MediaToolkitService.ExecuteAsync(new FfTaskGetMetadata(filename));
			//return result.Metadata.Streams.Select(e => double.Parse(e.Duration)).Max();
		}

		public async Task<string> Convert(string fileName)
		{
			var targetFile = Path.ChangeExtension(fileName, ".m4v");
			if (File.Exists(targetFile))
			{
				return targetFile;
			}

			var length = await GetMovieLength(fileName);

			//await FFMpegArguments.FromFileInput(fileName)
			//	.OutputToFile(targetFile, true, options => options.ForceFormat("m4v"))
			//	.NotifyOnProgress(e =>
			//	{
			//		Console.WriteLine($"Progress: {e}/100");
			//	}, TimeSpan.FromSeconds(length))
			//	.ProcessAsynchronously();
			//return targetFile;

			var sourceFile = new MediaFile(fileName);
			var target = new MediaFile(Path.ChangeExtension(fileName, ".m4k"));
			if (File.Exists(target.Filename))
			{
				return target.Filename;
			}

			using (var engine = new Engine(FfmpegPath, _fileSystem))
			{
				engine.Convert(sourceFile, target, new ConversionOptions()
				{
					
				});
			}

			return target.Filename;
		}
	}
}
