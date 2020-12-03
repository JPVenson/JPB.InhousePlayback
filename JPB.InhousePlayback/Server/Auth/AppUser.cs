using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JPB.DataAccess.Query.Operators;
using JPB.InhousePlayback.Server.Services.Database;
using JPB.InhousePlayback.Server.Services.Database.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JPB.InhousePlayback.Server.Auth
{
	//public class AppUser
	//{
	//	public string AppUserId { get; set; }
	//	public string Username { get; set; }
	//	public string NormUsername { get; set; }
	//	public string Password { get; set; }
	//}

	//public class AppRole
	//{
	//	public string RoleId { get; set; }
	//	public string RoleName { get; set; }
	//	public string NormRoleName { get; set; }
	//}

	public class AppUserSignInManager : SignInManager<AppUser>
	{
		public AppUserSignInManager(UserManager<AppUser> userManager, 
			IHttpContextAccessor contextAccessor, 
			IUserClaimsPrincipalFactory<AppUser> claimsFactory,
			IOptions<IdentityOptions> optionsAccessor,
			ILogger<SignInManager<AppUser>> logger, 
			IAuthenticationSchemeProvider schemes, 
			IUserConfirmation<AppUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
		{
		}
	}

	public class AppUserStore : IUserStore<AppUser>, IUserPasswordStore<AppUser>
	{
		private readonly DbService _db;

		public AppUserStore(DbService db)
		{
			_db = db;
		}
		public void Dispose()
		{
		}

		public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
		{
			_db.Insert(user);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
		{
			return IdentityResult.Success;
		}

		public async Task<AppUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			return _db.SelectSingle<AppUser>(userId);
		}

		public async Task<AppUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			var findByNameAsync = _db.Query().Select.Table<AppUser>()
				.Join(e => e.AppRole, JoinMode.Left)
				.Where
				.Column(f => f.NormUsername).Is.EqualsTo(normalizedUserName)
				.FirstOrDefault();
			return findByNameAsync;
		}

		public async Task<string> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken)
		{
			return user.NormUsername;
		}

		public async Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken)
		{
			return user.AppUserId.ToString();
		}

		public async Task<string> GetUserNameAsync(AppUser user, CancellationToken cancellationToken)
		{
			return user.Username;
		}

		public async Task SetNormalizedUserNameAsync(AppUser user, string normalizedName, CancellationToken cancellationToken)
		{
			user.NormUsername = normalizedName;
		}

		public async Task SetUserNameAsync(AppUser user, string userName, CancellationToken cancellationToken)
		{
			user.Username = userName;
		}

		public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
		{
			_db.Update(user);
			return IdentityResult.Success;
		}

		public async Task<string> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
		{
			return user.Password;
		}

		public async Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
		{
			return user.Password != null;
		}

		public async Task SetPasswordHashAsync(AppUser user, string passwordHash, CancellationToken cancellationToken)
		{
			user.Password = passwordHash;
		}
	}

	public class AppRoleStore : IRoleStore<AppRole>
	{
		private readonly DbService _db;

		public AppRoleStore(DbService db)
		{
			_db = db;
		}

		public void Dispose()
		{
		}

		public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
		{
			_db.Insert(role);
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
		{
			return IdentityResult.Success;
		}

		public async Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			await Task.CompletedTask;
			return _db.SelectSingle<AppRole>(roleId);
		}

		public async Task<AppRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			return _db.Query().Select.Table<AppRole>()
				.Where
				.Column(f => f.RoleName).Is.EqualsTo(normalizedRoleName)
				.FirstOrDefault();
		}

		public async Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
		{
			return role.RoleName;
		}

		public async Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
		{
			return role.AppRoleId.ToString();
		}

		public async Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
		{
			return role.RoleName;
		}

		public async Task SetNormalizedRoleNameAsync(AppRole role, string normalizedName, CancellationToken cancellationToken)
		{
			role.RoleName = normalizedName;
		}

		public async Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancellationToken)
		{
			role.RoleName = roleName;
		}

		public async Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
		{
			_db.Update(role);
			return IdentityResult.Success;
		}
	}

	public class AppUserManager : UserManager<AppUser>
	{
		public AppUserManager(IUserStore<AppUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<AppUser> passwordHasher, IEnumerable<IUserValidator<AppUser>> userValidators, IEnumerable<IPasswordValidator<AppUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<AppUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
		{
		}
	}

	public class AppRoleManager : RoleManager<AppRole>
	{
		public AppRoleManager(IRoleStore<AppRole> store, IEnumerable<IRoleValidator<AppRole>> roleValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<AppRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
		{
		}
	}
}
