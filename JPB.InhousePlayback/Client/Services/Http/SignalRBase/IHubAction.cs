using System.Threading.Tasks;

namespace JPB.InhousePlayback.Client.Services.Http.SignalRBase
{
	public interface IHubAction
	{
		void RegisterSubscription();
		Task Register(params object[] args);
	}
}