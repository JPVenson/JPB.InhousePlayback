using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class UserApiAccess : AccessBase
	{
		public UserApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("UserApi"))
		{
		}

		public async Task<ApiResult<User[]>> GetAll()
		{
			return await Get<User[]>(BuildUrl(base.Url + "GetUsers"));
		}

		public async Task<ApiResult<User>> Create(User user)
		{
			return await Post<User, User>(BuildUrl(base.Url + "AddUser"), user);
		}
	}
}
