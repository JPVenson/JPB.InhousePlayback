using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
	public class GenreApiController : ControllerBase
	{
		private readonly DbService _db;

		public GenreApiController(DbService db)
		{
			_db = db;
		}
		
		[HttpGet]
		[Route("GetAll")]
		public ActionResult GetAll()
		{
			return Ok(_db.Select<Genre>());
		}
		
		[HttpGet]
		[Route("GetSingle")]
		public ActionResult GetSingle(int genreId)
		{
			return Ok(_db.SelectSingle<Genre>(genreId));
		}
		
		[HttpPost]
		[Route("Update")]
		[Authorize(Roles = "Admin")]
		public ActionResult Update(Genre data)
		{
			_db.Update(data);
			return Ok();
		}

		public static string DefaultCover { get; set; } = "https://image.freepik.com/free-icon/dvd-sign_318-51393.jpg";
		
		[HttpGet]
		[Route("GetImage")]
		[AllowAnonymous]
		public IActionResult GetImage(int genreId)
		{
			var genre = _db.SelectSingle<Genre>(genreId);
			if (genre == null)
			{
				return Redirect(DefaultCover);
			}
			var imagePath = Path.Combine(genre.Location, "icon.jpeg");
			if (!System.IO.File.Exists(imagePath))
			{
				imagePath = Path.Combine(genre.Location, "icon.jpg");
				if (!System.IO.File.Exists(imagePath))
				{
					return Redirect(DefaultCover);
				}
			}

			return File(System.IO.File.ReadAllBytes(imagePath), MimeTypes.GetMimeType(".jpeg"));
		}
	}
}
