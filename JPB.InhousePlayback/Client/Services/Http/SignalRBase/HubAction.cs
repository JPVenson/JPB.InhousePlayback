using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.InhousePlayback.Client.Services.Http.SignalRBase
{
	public class HubAction : IHubAction
	{
		public string ActionName { get; }
		private readonly HubClientBase _client;

		public HubAction(HubClientBase client, string actionName)
		{
			ActionName = actionName;
			_client = client;
		}

		public void RegisterSubscription()
		{
			
		}

		public async Task Register(params object[] args)
		{
			await _client.HubProxy.SendCoreAsync("Register" + ActionName, args).ConfigureAwait(false);
		}

		public async Task UnRegister(params object[] args)
		{
			await _client.HubProxy.SendCoreAsync("UnRegister" + ActionName, args);
		}

		public async Task Send(params object[] args)
		{
			await _client.HubProxy.SendCoreAsync("Send" + ActionName, args);
		}

		public async Task<T> Send<T>(params object[] args)
		{
			return (T) (await _client.HubProxy.InvokeCoreAsync("Send" + ActionName, typeof(T), args));
		}
	}

	//public class HubAction<TActionType> : IDisposable, 
	//	IHubAction where TActionType : IActionCallback
	//{
	//	private readonly HubClientBase _client;
	//	public IDisposable Subscription { get; private set; }
	//	public IList<TActionType> Actions { get; }

	//	public HubAction(string actionName, HubClientBase client)
	//	{
	//		Actions = new List<TActionType>();
	//		_client = client;
	//		ActionName = actionName;
	//	}

	//	public void RegisterSubscription()
	//	{

	//	}

	//	public string ActionName { get; set; }

	//	public async Task Register(params object[] args)
	//	{
	//		await _client.HubProxy.Invoke("Register" + ActionName, args).ConfigureAwait(false);
	//	}

	//	public async Task UnRegister(params object[] args)
	//	{
	//		await _client.HubProxy.Invoke("UnRegister" + ActionName, args);
	//	}
		
	//	public async Task Send(params object[] args)
	//	{
	//		await _client.HubProxy.Invoke("Send" + ActionName, args);
	//	}

	//	public async Task<T> Send<T>(params object[] args)
	//	{
	//		return await _client.HubProxy.Invoke<T>("Send" + ActionName, args);
	//	}

	//	public void Dispose()
	//	{
	//		Subscription.Dispose();
	//	}

	//	public void Add(TActionType actionCallback, IDispatchedDisposable source)
	//	{
	//		Actions.Add(actionCallback);
	//		if (source != null)
	//		{
	//			source.OnDisposables.Add(() =>
	//			{
	//				Remove(actionCallback);
	//			});
	//		}
	//	}

	//	public void Remove(TActionType actionCallback)
	//	{
	//		Actions.Remove(actionCallback);
	//	}
	//}
	
	//public interface IDispatchedDisposable : IDisposable
	//{
	//	ICollection<Action> OnDisposables { get; }
	//}
}