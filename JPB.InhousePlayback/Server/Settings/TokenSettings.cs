﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.InhousePlayback.Server.Settings
{
	public class TokenSettings
	{
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string Key { get; set; }
	}
}
