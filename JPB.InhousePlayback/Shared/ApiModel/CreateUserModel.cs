using System;
using System.Collections.Generic;
using System.Text;

namespace JPB.InhousePlayback.Shared.ApiModel
{
	public class CreateUserModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public int IdRole { get; set; }
	}

	public class StreamIdModel
	{
		public string Id { get; set; }
	}
}
