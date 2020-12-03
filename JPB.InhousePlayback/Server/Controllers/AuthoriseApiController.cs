using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JPB.InhousePlayback.Server.Auth;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using JPB.InhousePlayback.Server.Settings;
using JPB.InhousePlayback.Shared.ApiModel;
using JPB.InhousePlayback.Shared.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JPB.InhousePlayback.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthoriseApiController : ControllerBase
	{
		private readonly DbService _db;
		private readonly AppUserSignInManager _signInManager;
		private readonly IUserStore<AppUser> _userStore;
		private readonly IRoleStore<AppRole> _roleStore;
		private readonly IOptions<TokenSettings> _tokenSettings;
		private readonly UserManager<AppUser> _appUserManager;

		public AuthoriseApiController(DbService db, AppUserSignInManager signInManager, 
			IUserStore<AppUser> userStore, 
			IRoleStore<AppRole> roleStore, 
			IOptions<TokenSettings> tokenSettings,
			UserManager<AppUser> appUserManager)
		{
			_db = db;
			_signInManager = signInManager;
			_userStore = userStore;
			_roleStore = roleStore;
			_tokenSettings = tokenSettings;
			_appUserManager = appUserManager;
		}

		[Authorize]
		[HttpGet]
		[Route("Me")]
		public IActionResult Me()
		{
			var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userdata = _db.SelectSingle<AppUser>(username);
			return Ok(userdata);
		}

		//[Authorize(Roles = "Admin")]
		//[HttpGet]
		//[Route("AddUser")]
		//public IActionResult AddUser(CreateUserModel user)
		//{
		//	var appUser = new AppUser()
		//	{
		//		Username = user.Username,
		//		NormUsername = user.Username.ToUpper(),
		//		IdRole = user.IdRole
		//	};
		//	appUser.Password = _appUserManager.PasswordHasher.HashPassword(appUser, user.Password);
		//	_db.Insert(appUser);
		//	return Ok();
		//}

		[HttpPost]
		[Route("Login")]
		public async Task<ActionResult> Login([FromBody]LoginModel loginModel)
		{
			var passwordSignInAsync = await _signInManager.PasswordSignInAsync(loginModel.Username, loginModel.Password, true, false);
			if (passwordSignInAsync.Succeeded)
			{
				var result = new LoginResult();
				var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Value.Key));
				var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

				//var user = _db.Query().Select.Table<AppUser>()
				//	.Where
				//	.Column(f => f.NormUsername).Is.EqualsTo(loginModel.Username.ToUpper())
				//	.FirstOrDefault();

				var user = await _userStore.FindByNameAsync(loginModel.Username.ToUpper(), CancellationToken.None);

				var claims = new List<Claim>()
				{
					new Claim(ClaimTypes.NameIdentifier, user.AppUserId.ToString()),
					new Claim(ClaimTypes.Name, user.Username),
					new Claim(ClaimTypes.Role, user.AppRole.RoleName)
				};
				
				var jwtToken = new JwtSecurityToken(
					issuer: _tokenSettings.Value.Issuer,
					audience: _tokenSettings.Value.Audience,
					expires: DateTime.Now.AddYears(200),
					signingCredentials: credentials,
					claims: claims
				);
				string token = JwtCoder.EncodeToken(jwtToken);
				result.Token = token;

				return Ok(result);
			}

			return Unauthorized();
		}

		[HttpPost]
		[Route("Logout")]
		public async Task<ActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return Ok();
		}
	}
}