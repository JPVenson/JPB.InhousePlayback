using JPB.InhousePlayback.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace JPB.InhousePlayback.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UserApiController : ControllerBase
	{
		private readonly DbService _db;

		public UserApiController(DbService db)
		{
			_db = db;
		}

		[HttpGet]
		[Route("GetUsers")]
		public ActionResult GetUsers()
		{
			return Ok(_db.Select<Users>());
		}

		[HttpPost]
		[Route("AddUser")]
		public ActionResult AddUser(Users user)
		{
			user.IdAccount = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
			return Ok(_db.InsertWithSelect(user));
		}
	}
}
