using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Server.Auth;
using JPB.InhousePlayback.Server.Services.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JPB.InhousePlayback.Server.Services.Database
{
	public class DbInitService : IRequireInit
	{
		private readonly DbService _db;
		private readonly IConfiguration _config;
		private readonly UserManager<AppUser> _appUserManager;

		public DbInitService(DbService db, IConfiguration config, IServiceProvider serviceCollection)
		{
			_db = db;
			_config = config;
			using (var scope = serviceCollection.CreateScope())
			{
				_appUserManager = scope.ServiceProvider.GetService(typeof(UserManager<AppUser>)) as UserManager<AppUser>;	
			}
		}

		public void Init()
		{
			_config["NeedsInit"] = BuildDatabase().ToString();
		}

		public static string GetFileName()
		{
			var sb = new StringBuilder();
			foreach (var table in Tables.Yield())
			{
				sb.Append(table);
			}
			var computeHash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(sb.ToString()));
			var smHash = computeHash.Select(f => (int)f).Aggregate((e, f) => e + f) % byte.MaxValue;
			return "Playback." + smHash + ".db";
		}

		public bool BuildDatabase()
		{
			var hash = File.Exists("dbstate") ? File.ReadAllText("dbstate") : "";
			if (hash != GetFileName())
			{
				File.Delete("Playback.db");
				foreach (var table in Tables.Yield())
				{
					_db.ExecuteGenericCommand(table);
				}

				foreach (var seed in Seeds.Yield(_appUserManager))
				{
					_db.ExecuteGenericCommand(seed);
				}

				File.WriteAllText("dbstate", GetFileName());
				return true;
			}

			return false;
		}
	}
}