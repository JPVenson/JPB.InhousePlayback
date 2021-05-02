using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;

namespace JPB.InhousePlayback.Client.Services.Http.SignalRBase
{
	public class ActionCallback : IActionCallback
	{
		private readonly Action _callback;

		public ActionCallback(Action callback)
		{
			_callback = callback;
		}

		public void Invoke()
		{
			_callback();
		}

		public IDisposable RegisterCallback(HubConnection connection, string actionName)
		{
			return connection.On(actionName, _callback);
		}
	}
}