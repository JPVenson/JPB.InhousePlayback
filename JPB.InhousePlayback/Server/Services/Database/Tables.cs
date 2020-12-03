using System.Collections.Generic;
using System.Text;
using JPB.InhousePlayback.Server.Auth;
using JPB.InhousePlayback.Server.Services.Database.Models;
using Microsoft.AspNetCore.Identity;

namespace JPB.InhousePlayback.Server.Services.Database
{
	public static class Seeds
	{
		public static IEnumerable<string> Yield(UserManager<AppUser> userManager)
		{
			yield return SeedAdminRole();
			yield return SeedUserRole();
			yield return SeedAdminAccount(userManager);
			yield return SeedViewerAccount(userManager);
		}

		public static string SeedAdminRole()
		{
			return $"INSERT INTO AppRole(RoleName, NormRoleName) VALUES ('Admin', 'ADMIN');";
		}

		public static string SeedUserRole()
		{
			return $"INSERT INTO AppRole(RoleName, NormRoleName) VALUES ('User', 'USER');";
		}

		public static string SeedAdminAccount(UserManager<AppUser> appUserManager)
		{
			return $"INSERT INTO AppUser(Username, NormUsername, IdRole, Password) " +
			       $"VALUES ('Admin', 'ADMIN', (SELECT AppRoleId FROM AppRole WHERE AppRole.RoleName = 'Admin'), '{appUserManager.PasswordHasher.HashPassword(new AppUser(), "AdminPw")}');";
		}

		public static string SeedViewerAccount(UserManager<AppUser> appUserManager)
		{
			return $"INSERT INTO AppUser(Username, NormUsername, IdRole, Password) " +
			       $"VALUES ('Viewer', 'VIEWER', (SELECT AppRoleId FROM AppRole WHERE AppRole.RoleName = 'User'), '{appUserManager.PasswordHasher.HashPassword(new AppUser(), "2020Sucks")}');";
		}
	}

	public static class Tables
	{
		public static IEnumerable<string> Yield()
		{
			yield return Accounts();
			yield return Roles();
			yield return Users();
			yield return Genre();
			yield return Seasons();
			yield return Titles();
			yield return Playbacks();
		}

		public static string Accounts()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE AppUser ( ");
			sb.AppendLine($" AppUserId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" Username TEXT NOT NULL,");
			sb.AppendLine($" NormUsername TEXT NOT NULL,");
			sb.AppendLine($" Password TEXT NOT NULL,");
			sb.AppendLine($" IdRole INTEGER NOT NULL,");
			sb.AppendLine($" FOREIGN KEY (IdRole) REFERENCES AppRole(AppRoleId) ");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Roles()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE AppRole ( ");
			sb.AppendLine($" AppRoleId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" RoleName TEXT NOT NULL,");
			sb.AppendLine($" NormRoleName TEXT NOT NULL");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Users()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE Users ( ");
			sb.AppendLine($" UserId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" Username TEXT,");
			sb.AppendLine($" IdAccount INTEGER,");
			sb.AppendLine($" FOREIGN KEY (IdAccount) REFERENCES AppUser(AppUserId) ");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Genre()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE Genre (");
			sb.AppendLine($" GenreId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" Name TEXT,");
			sb.AppendLine($" Location TEXT,");
			sb.AppendLine($" PlaybackLength INTEGER, ");
			sb.AppendLine($" OffsetStart INTEGER, ");
			sb.AppendLine($" OffsetEnd INTEGER ");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Seasons()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE Season (");
			sb.AppendLine($" SeasonId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" Name TEXT,");
			sb.AppendLine($" Location TEXT,");
			sb.AppendLine($" PlaybackLength INTEGER, ");
			sb.AppendLine($" OffsetStart INTEGER, ");
			sb.AppendLine($" OffsetEnd INTEGER, ");
			sb.AppendLine($" OrderNo INTEGER NOT NULL,");
			sb.AppendLine($" IdGenre INTEGER NOT NULL, ");
			sb.AppendLine($" FOREIGN KEY (IdGenre) REFERENCES Genre(GenreId) ");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Titles()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE Title (");
			sb.AppendLine($" TitleId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, ");
			sb.AppendLine($" Name TEXT, ");
			sb.AppendLine($" Location TEXT, ");
			sb.AppendLine($" OrderNo INTEGER, ");
			sb.AppendLine($" PlaybackLength INTEGER, ");
			sb.AppendLine($" OffsetStart INTEGER, ");
			sb.AppendLine($" OffsetEnd INTEGER, ");
			sb.AppendLine($" IdSeason INTEGER NOT NULL, ");
			sb.AppendLine($" FOREIGN KEY (IdSeason) REFERENCES Season(SeasonId) ");
			sb.AppendLine(");");
			return sb.ToString();
		}

		public static string Playbacks()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE Playback (");
			sb.AppendLine($" PlaybackId INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,");
			sb.AppendLine($" IdTitle INTEGER NOT NULL, ");
			sb.AppendLine($" IdUser INTEGER NOT NULL, ");
			sb.AppendLine($" Position INTEGER NOT NULL, ");
			sb.AppendLine($" FOREIGN KEY (IdTitle) REFERENCES Title(TitleId), ");
			sb.AppendLine($" FOREIGN KEY (IdUser) REFERENCES Users(UserId) ");
			sb.AppendLine(");");
			return sb.ToString();
		}
	}
}