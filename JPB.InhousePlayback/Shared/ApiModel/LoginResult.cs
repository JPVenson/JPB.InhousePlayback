using System;
using System.Collections.Generic;
using System.Text;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Shared.ApiModel
{
	public class LoginResult
	{
		public string Token { get; set; }
		public AppUser User { get; set; }
	}
}
