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
	public class SeasonsApiController : ControllerBase
	{
		private readonly DbService _db;

		public SeasonsApiController(DbService db)
		{
			_db = db;
		}
		
		[HttpGet]
		[Route("GetSingle")]
		public ActionResult GetSingle(int seasonId)
		{
			return Ok(_db.SelectSingle<Season>(seasonId));
		}
		
		[HttpPost]
		[Route("Update")]
		[Authorize(Roles = "Admin")]
		public ActionResult Update(Season data)
		{
			_db.Update(data);
			return Ok();
		}

		[HttpGet]
		[Route("GetAll")]
		public ActionResult GetAll(int genreId)
		{
			var seasons = _db.Query().Select.Table<Season>().Where
				.Column(f => f.IdGenre).Is.EqualsTo(genreId).ToArray();
			return Ok(seasons);
		}

		
		[HttpGet]
		[Route("GetImage")]
		[AllowAnonymous]
		public IActionResult GetImage(int seasonId)
		{
			var genre = _db.SelectSingle<Season>(seasonId);
			if (genre == null)
			{
				return Redirect(GenreApiController.DefaultCover);
			}
			var imagePath = Path.Combine(genre.Location, "icon.jpeg");
			if (!System.IO.File.Exists(imagePath))
			{
				imagePath = Path.Combine(genre.Location, "icon.jpg");
				if (!System.IO.File.Exists(imagePath))
				{
					return Redirect(GenreApiController.DefaultCover);
				}
			}

			return File(System.IO.File.ReadAllBytes(imagePath), MimeTypes.GetMimeType(".jpeg"));
		}
	}
}