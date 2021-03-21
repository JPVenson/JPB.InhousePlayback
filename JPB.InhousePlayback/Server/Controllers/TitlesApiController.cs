using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using JPB.DataAccess.Contacts;
using JPB.DataAccess.Query.Contracts;
using JPB.DataAccess.Query.Operators;
using JPB.DataAccess.Query.QueryItems;
using JPB.DataAccess.QueryFactory;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Services.Thumbnail;
using JPB.InhousePlayback.Server.Services.TitleEnumeration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace JPB.InhousePlayback.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class TitlesApiController : ControllerBase
	{
		private readonly DbService _db;
		private readonly IFileSystem _fileSystem;
		private readonly ThumbnailService _thumbnailService;

		public TitlesApiController(DbService db, IFileSystem fileSystem, ThumbnailService thumbnailService)
		{
			_db = db;
			_fileSystem = fileSystem;
			_thumbnailService = thumbnailService;
		}

		[HttpPost]
		[Route("Update")]
		[Authorize(Roles = "Admin")]
		public ActionResult Update(Title data)
		{
			_db.Update(data);
			return Ok();
		}

		[HttpGet]
		[Route("Thumbnail")]
		[AllowAnonymous]
		public IActionResult GetThumbnail(int titleId)
		{
			var title = _db.SelectSingle<Title>(titleId);
			var thumbnailPath = _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(title.Location),
				"Thumbnails",
				_fileSystem.Path.GetFileName(title.Location) + ".thumb.jpeg");
			if (_fileSystem.File.Exists(thumbnailPath))
			{
				return File(System.IO.File.ReadAllBytes(thumbnailPath), MimeTypes.GetMimeType(thumbnailPath));
			}

			return Redirect(GenreApiController.DefaultCover);
		}

		[HttpGet]
		[Route("ThumbnailPreview")]
		[AllowAnonymous]
		public async Task<IActionResult> GetThumbnail(int titleId, bool fromEnd, int position = 0)
		{
			var title = _db.SelectSingle<Title>(titleId);
			var generateThumbnail = await _thumbnailService.GetThumbnail(title, fromEnd, position);
			return File(generateThumbnail, MimeTypes.GetMimeType(".jpg"));
		}

		[HttpGet]
		[Route("GetNextTitle")]
		public IActionResult GetNextTitle(int titleId)
		{
			var title = _db.SelectSingle<Title>(titleId);

			var nextTitle = _db.Query().Select.Table<Title>()
				.Where
				.Column(f => f.IdSeason).Is.EqualsTo(title.IdSeason)
				.And
				.Column(f => f.OrderNo).Is.BiggerThen(title.OrderNo)
				.Order.By(e => e.OrderNo)
				.LimitBy(1)
				.FirstOrDefault();

			if (nextTitle == null)
			{
				var season = _db.SelectSingle<Season>(title.IdSeason);
				var nextSeason = _db.Query().Select.Table<Season>()
					.Where
					.Column(f => f.IdGenre).Is.EqualsTo(season.IdGenre)
					.And
					.Column(f => f.OrderNo).Is.BiggerThen(season.OrderNo)
					.Order.By(f => f.OrderNo)
					.LimitBy(1)
					.FirstOrDefault();

				if (nextSeason != null)
				{
					nextTitle = _db.Query().Select.Table<Title>()
						.Where
						.Column(f => f.IdSeason).Is.EqualsTo(nextSeason.SeasonId)
						.Order.By(f => f.OrderNo)
						.LimitBy(1)
						.FirstOrDefault();
				}
			}

			return Ok(nextTitle);
		}
		
		[HttpPost]
		[Route("Delete")]
		[Authorize(Roles = "Admin")]
		public ActionResult Delete(int titleId)
		{
			_db.Query().Delete<Playback>()
				.Where
				.Column(f => f.IdTitle).Is.EqualsTo(titleId)
				.ExecuteNonQuery();
			_db.Query().Delete<Title>()
				.Where
				.Column(f => f.TitleId).Is.EqualsTo(titleId)
				.ExecuteNonQuery();
			return Ok();
		}


		[HttpGet]
		[Route("GetAll")]
		public ActionResult GetAll(int seasonId, int userId)
		{
			return Ok(_db.Query().Select.Table<Title>()
				.Join(e => e.Playback, JoinMode.Left, f => f.And.Column(e => e.Playback.Type.IdUser).Is.EqualsTo(userId))
				.Where
				.Column(f => f.IdSeason).Is.EqualsTo(seasonId).ToArray());
		}

		[HttpGet]
		[Route("GetSingle")]
		public ActionResult GetSingle(int titleId)
		{
			return Ok(_db.SelectSingle<Title>(titleId));
		}

		[HttpGet]
		[Route("GetSingleWithPlaybackStatus")]
		public ActionResult GetSingleWithPlaybackStatus(int titleId, int userId)
		{
			return Ok(_db.Query().Select.Table<Title>()
				.Join(e => e.Playback, JoinMode.Left, f => f.And.Column(e => e.Playback.Type.IdUser).Is.EqualsTo(userId))
				.Where
				.PrimaryKey().Is.EqualsTo(titleId).FirstOrDefault()
			);
		}

		[HttpGet]
		[Route("GetLastPlayed")]
		public ActionResult GetLastPlayed(int userId)
		{
			var query = _db.Query().Select.Table<Playback>()
				.Join(e => e.Title.Season.Genre)
				.Where
				.Column(f => f.IdUser).Is.EqualsTo(userId).ToArray();

			return Ok(query.GroupBy(e => e.Title.Season.IdGenre)
				.SelectMany(f =>
				{
					return new Playback[]
						{
							f.OrderByDescending(e => e.Title.Season.OrderNo)
								.ThenBy(e => e.Title.OrderNo)
								.First(),
							f.OrderBy(e => e.PlaybackId).Last()
						};
				}));
		}
	}
}