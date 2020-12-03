using System.Threading.Tasks;

namespace JPB.InhousePlayback.Client.Services.UserManager
{
	public interface IRequireInitAsync
	{
		Task Init();
	}
}