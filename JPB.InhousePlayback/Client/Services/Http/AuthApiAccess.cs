using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.ApiModel;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class AuthApiAccess : AccessBase
	{
		public AuthApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("AuthoriseApi"))
		{
		}

		public async Task<ApiResult<AppUser>> Me()
		{
			return await Get<AppUser>(BuildUrl(base.Url + "Me"));
		}

		public async Task<ApiResult<LoginResult>> Login(string username, string password)
		{
			return await Post<LoginModel, LoginResult>(BuildUrl(base.Url + "Login"), new LoginModel
			{
				Username = username,
				Password = password
			});
		}
	}
}