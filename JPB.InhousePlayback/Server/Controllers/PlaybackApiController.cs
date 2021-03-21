using System.IO;
using System.Linq;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Util.BufferedFileStreamResult;
using JPB.InhousePlayback.Shared.ApiModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace JPB.InhousePlayback.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class PlaybackApiController : ControllerBase
	{
		private readonly DbService _db;
		private readonly BufferedFileStreamResultExecutor _executor;

		public PlaybackApiController(DbService db, BufferedFileStreamResultExecutor executor)
		{
			_db = db;
			_executor = executor;
		}
		
		[HttpGet]
		[Route("StartPlayback")]
		[AllowAnonymous]
		public IActionResult BeginPlayback(int titleId)
		{
			var streamId = _executor.GetStreamId(titleId);
			return Ok(new StreamIdModel()
			{
				Id = streamId
			});
		}
		
		[HttpPost]
		[Route("EndPlayback")]
		[AllowAnonymous]
		public IActionResult EndPlayback(string streamId)
		{
			_executor.EndPlayback(streamId);
			return Ok();
		}

		[HttpGet]
		[Route("Playback")]
		[AllowAnonymous]
		public IActionResult Playback(string streamId)
		{
			//var fileStreamResult = File(new FileStream(title.Location, FileMode.Open, FileAccess.Read),
			//	MimeTypes.GetMimeType(title.Location), true);

			var streamIdObject = _executor.GetStreamId(streamId);

			var fileStreamResult = new BufferedFileStreamActionResult(streamIdObject.Location,
				streamIdObject.Id);
			fileStreamResult.EnableRangeProcessing = true;
			return fileStreamResult;
		}

		[HttpPost]
		[Route("OnPlayback")]
		public IActionResult OnPlayback(int userId, int titleId, int position)
		{
			var playback = _db.Query().Select.Table<Playback>().Where
				.Column(f => f.IdTitle).Is.EqualsTo(titleId)
				.And
				.Column(f => f.IdUser).Is.EqualsTo(userId)
				.LimitBy(1)
				.FirstOrDefault();

			if (playback == null)
			{
				playback = new Playback();
				playback.IdTitle = titleId;
				playback.IdUser = userId;
				playback.Position = position;
				_db.Insert(playback);
			}
			else
			{
				_db.Query().Update.Table<Playback>()
					.Set
					.Column(f => f.Position).Value(position)
					.Where
					.PrimaryKey().Is.EqualsTo(playback.PlaybackId)
					.ExecuteNonQuery();
			}

			return Ok();
		}
	}
}