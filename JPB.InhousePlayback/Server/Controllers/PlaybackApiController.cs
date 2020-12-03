using System.IO;
using System.Linq;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
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

		public PlaybackApiController(DbService db)
		{
			_db = db;
		}

		[HttpGet]
		[Route("Playback")]
		[AllowAnonymous]
		public FileStreamResult Playback(int titleId)
		{
			var title = _db.SelectSingle<Title>(titleId);
			var fileStreamResult = File(new FileStream(title.Location, FileMode.Open, FileAccess.Read),
				MimeTypes.GetMimeType(title.Location), true);
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