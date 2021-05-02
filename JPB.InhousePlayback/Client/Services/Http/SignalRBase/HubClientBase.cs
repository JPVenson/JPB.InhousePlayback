using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace JPB.InhousePlayback.Client.Services.Http.SignalRBase
{
	public abstract class HubClientBase
	{
		public HttpService Client { get; private set; }

		public HubClientBase(string hubName, HttpService client)
		{
			Client = client;
			HubName = hubName + "Hub";
			HubActions = new Dictionary<string, IHubAction>();
		}

		public void RegisterCallbacks(HubConnection hubConnection)
		{
			HubProxy = hubConnection;
			foreach (var hubInfos in GetType()
				.GetProperties()
				.Where(e => typeof(IHubAction).IsAssignableFrom(e.PropertyType))
				.Select(f => f.GetValue(this) as IHubAction))
			{
				hubInfos.RegisterSubscription();
			}
		}

		public async Task StartCallbacks(HubConnection hubConnection)
		{
			foreach (var hubInfos in GetType()
				.GetProperties()
				.Where(e => typeof(IHubAction).IsAssignableFrom(e.PropertyType))
				.Select(f => f.GetValue(this) as IHubAction))
			{
				await hubInfos.Register();
			}
		}

		public virtual void CustomRegisterCallbacks(HubConnection hubConnection) { }
		public virtual void CustomStartCallbacks(HubConnection hubConnection) { }

		public string HubName { get; }
		public HubConnection HubProxy { get; private set; }

		public IDictionary<string, IHubAction> HubActions { get; set; }
	}
}