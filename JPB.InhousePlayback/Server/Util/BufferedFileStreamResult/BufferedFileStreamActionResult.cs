using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.Media;
using MediaToolkit;
using MediaToolkit.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MimeKit;
using Newtonsoft.Json;

namespace JPB.InhousePlayback.Server.Util.BufferedFileStreamResult
{
	public class BufferedFileStreamActionResult : FileResult, IDisposable
	{
		public string FileName { get; }
		public string StreamId { get;  }

		public BufferedFileStreamActionResult(string fileName, string streamId) : base(MimeTypes.GetMimeType(fileName))
		{
			FileName = fileName;
			StreamId = streamId;
		}

		public override Task ExecuteResultAsync(ActionContext context)
		{
			var executor = context.HttpContext.RequestServices.GetRequiredService<BufferedFileStreamResultExecutor>();
			return executor.ExecuteAsync(context, this);
		}

		public void Dispose()
		{

		}
	}

	public class StreamId
	{
		public StreamId(string id, string location)
		{
			Id = id;
			Location = location;
		}


		public string Id { get; set; }
		public string Location { get; set; }
	}

	public class BufferedFileStreamResultExecutor : FileStreamResultExecutor, IActionResultExecutor<BufferedFileStreamActionResult>
	{
		static BufferedFileStreamResultExecutor()
		{
			var rand = new Random();
			var key = new byte[1280];
			for (int i = 0; i < key.Length; i++)
			{
				key[i] = (byte) rand.Next(0, byte.MaxValue);
			}
			_privateStreamKey = GetKey(key);
		}

		private readonly DbService _db;

		public BufferedFileStreamResultExecutor(ILoggerFactory loggerFactory, DbService db) : base(loggerFactory)
		{
			_db = db;
			Streams = new ConcurrentDictionary<string, BufferData>();
		}

		public IDictionary<string, BufferData> Streams { get; set; }

		public string GetStreamId(int titleId)
		{
			var title = _db.SelectSingle<Title>(titleId);
			var streamId =
				new StreamId(Guid.NewGuid().ToString(), title.Location);

			return Encrypt(JsonConvert.SerializeObject(streamId));
		}

		public StreamId GetStreamId(string streamId)
		{
			streamId = Decrypt(streamId);
			return JsonConvert.DeserializeObject<StreamId>(streamId);
		}

		private static byte[] _privateStreamKey;

		private static byte[] GetKey(byte[] password)
		{
			using (var md5 = MD5.Create())
			{
				return md5.ComputeHash(password);
			}
		}

		/// <span class="code-SummaryComment"><summary></span>
		/// Encrypt a string.
		/// <span class="code-SummaryComment"></summary></span>
		/// <span class="code-SummaryComment"><param name="originalString">The original string.</param></span>
		/// <span class="code-SummaryComment"><returns>The encrypted string.</returns></span>
		/// <span class="code-SummaryComment"><exception cref="ArgumentNullException">This exception will be </span>
		/// thrown when the original string is null or empty.<span class="code-SummaryComment"></exception></span>
		public static string Encrypt(string toEncrypt)
		{
			using (var aes = Aes.Create())
			using (var encryptor = aes.CreateEncryptor(_privateStreamKey, _privateStreamKey))
			{
				var plainText = Encoding.UTF8.GetBytes(toEncrypt);
				var data = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
				var base64String = Convert.ToBase64String(data, 0, data.Length);
				return base64String;
			}
		}

		/// <span class="code-SummaryComment"><summary></span>
		/// Decrypt a crypted string.
		/// <span class="code-SummaryComment"></summary></span>
		/// <span class="code-SummaryComment"><param name="cryptedString">The crypted string.</param></span>
		/// <span class="code-SummaryComment"><returns>The decrypted string.</returns></span>
		/// <span class="code-SummaryComment"><exception cref="ArgumentNullException">This exception will be thrown </span>
		/// when the crypted string is null or empty.<span class="code-SummaryComment"></exception></span>
		public static string Decrypt(string encryptedData)
		{
			var data = Convert.FromBase64String(encryptedData);
			using (var aes = Aes.Create())
			using (var encryptor = aes.CreateDecryptor(_privateStreamKey, _privateStreamKey))
			{
				var decryptedBytes = encryptor
					.TransformFinalBlock(data, 0, data.Length);
				return Encoding.UTF8.GetString(decryptedBytes);
			}
		}

		public async Task ExecuteAsync(ActionContext context, BufferedFileStreamActionResult result)
		{
			if (!Streams.TryGetValue(result.FileName, out var buffer) || buffer == null)
			{
				buffer = new BufferData(result.StreamId,
					new FileStream(result.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize));
				Streams[result.FileName] = buffer;
			}
			buffer.LastAccess = DateTime.Now;

			var fileLength = buffer.SourceStream.Length;

			var (range, rangeLength, serveBody) = SetHeadersAndLog(
				context,
				result,
				fileLength,
				result.EnableRangeProcessing,
				result.LastModified,
				result.EntityTag);

			if (!serveBody)
			{
				return;
			}

			var outputStream = context.HttpContext.Response.Body;
			try
			{
				if (range == null)
				{
					await StreamCopyOperation.CopyToAsync(buffer.AccessStream, outputStream, count: null, bufferSize: BufferSize, cancel: context.HttpContext.RequestAborted);
				}
				else
				{
					buffer.AccessStream.Seek(range.From.Value, SeekOrigin.Begin);
					await StreamCopyOperation.CopyToAsync(buffer.AccessStream, outputStream, rangeLength, BufferSize, context.HttpContext.RequestAborted);
				}
			}
			catch (OperationCanceledException)
			{
				// Don't throw this exception, it's most likely caused by the client disconnecting.
				// However, if it was cancelled for any other reason we need to prevent empty responses.
				context.HttpContext.Abort();
			}
			//using (var sourceStream = buffer.AccessStream.CreateViewStream(range.From ?? 0, range.To ?? fileLength, MemoryMappedFileAccess.Read))
			//{
			//}
		}

		public void EndPlayback(string streamId)
		{
			var streamIdObject = GetStreamId(streamId);
			Streams.Remove(streamIdObject.Id);
		}
	}

	public class BufferData
	{
		public BufferData(string streamId, FileStream sourceStream)
		{
			StreamId = streamId;
			SourceStream = sourceStream;
			DateOfCreation = DateTime.Now;
			var sourceStreamLength = sourceStream.Length > 5000000 ? sourceStream.Length : 5000000;
			AccessStream = new BufferedStream(sourceStream, 50000000);
		}

		public string StreamId { get; private set; }
		public DateTime DateOfCreation { get; private set; }
		public DateTime LastAccess { get; set; }

		public FileStream SourceStream { get; set; }
		public BufferedStream AccessStream { get; set; }

		
	}


	//public class BufferData : IDisposable
	//{
	//	public BufferData()
	//	{
	//		BufferedRanges = new ConcurrentDictionary<Range, BufferedStreamRange>();
	//	}

	//	public string StreamId { get; set; }
	//	public FileStream Stream { get; set; }

	//	public class BufferedStreamRange
	//	{
	//		public BufferedStreamRange(Range range)
	//		{
	//			Range = range;
	//			Buffer = new MemoryStream();
	//		}

	//		public Range Range { get; private set; }
	//		private MemoryStream Buffer { get; }

	//		public Memory<byte> Read()
	//		{
	//			return new Memory<byte>(Buffer.GetBuffer(), 0, (int) Buffer.Length);
	//		}

	//		public Memory<byte> Read(int startInBuffer, int endInBuffer)
	//		{
	//			return new Memory<byte>(Buffer.GetBuffer(), startInBuffer, endInBuffer);
	//		}

	//		public void Write(Span<byte> length)
	//		{
	//			Buffer.Write(length);
	//			Range = new Range(Range.Start, Range.End.Value + length.Length);
	//		}
	//	}

	//	public ConcurrentDictionary<Range, BufferedStreamRange> BufferedRanges { get; private set; }

	//	public bool ContainsRange(Range outer, Range inner)
	//	{
	//		return inner.Start.Value >= outer.Start.Value && inner.End.Value <= outer.End.Value;
	//	}

	//	public void Add()

	//	public Memory<byte> GetOrStitchRange(Range range)
	//	{
	//		var completeRange = BufferedRanges.FirstOrDefault(e => ContainsRange(e.Key, range));
	//		if (completeRange.Equals(default))
	//		{
	//			//try stitch range
	//			return null;
	//		}

	//		//check for perfect fit
	//		if (range.Equals(completeRange.Key))
	//		{
	//			return completeRange.Value.Read();
	//		}

	//		var startInBuffer = range.Start.Value - completeRange.Key.Start.Value;
	//		var endInBuffer = completeRange.Key.End.Value - range.End.Value;
	//		return completeRange.Value.Read(startInBuffer, endInBuffer);
	//	}

	//	public void Dispose()
	//	{
	//		Stream?.Dispose();
	//	}
	//}
}
