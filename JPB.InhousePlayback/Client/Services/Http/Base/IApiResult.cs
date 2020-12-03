using System.Net;

namespace JPB.InhousePlayback.Client.Services.Http.Base
{
	public interface IApiResult
	{
		HttpStatusCode StatusCode { get; }
		bool Success { get; }
		string StatusMessage { get; }
	}
}