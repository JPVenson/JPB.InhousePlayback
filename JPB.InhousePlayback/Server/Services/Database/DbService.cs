using System;
using System.Threading.Tasks;
using JPB.DataAccess.DbInfoConfig;
using JPB.DataAccess.Manager;
using JPB.DataAccess.SqLite;
using Microsoft.Extensions.Configuration;
using MimeKit.Cryptography;

namespace JPB.InhousePlayback.Server.Services.Database
{
	public class DbService : DbAccessLayer
	{
		public DbService(IConfiguration config, DbConfig dbConfig) : base(new SqLiteStrategy($"Data Source=Playback.db;"), dbConfig)
		{
			
		}
	}
}
