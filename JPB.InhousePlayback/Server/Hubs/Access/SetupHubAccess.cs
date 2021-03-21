using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Server.Hubs.Impl;
using Microsoft.AspNetCore.SignalR;

namespace JPB.InhousePlayback.Server.Hubs.Access
{
	public class SetupHubAccess
	{
		private readonly IHubContext<SetupHub> _hub;

		public SetupHubAccess(IHubContext<SetupHub> hub)
		{
			_hub = hub;
		}

		public Task SendSetupProgress(string taskName, int progress, int maxProgress)
		{
			return _hub.Clients.All.SendAsync("OnProgress", taskName, progress, maxProgress);
		}
	}
}
