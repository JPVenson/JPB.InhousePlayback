using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using JPB.InhousePlayback.Shared.ApiModel;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.LocalStorage
{
	public class PlayblackLocalStorageService
	{
		private readonly ILocalStorageService  _localStorageService;

		public PlayblackLocalStorageService(ILocalStorageService  localStorageService)
		{
			_localStorageService = localStorageService;
		}

		public async Task<User> GetLastUser()
		{
			return await _localStorageService.GetItemAsync<User>("Playback.LastUser");
		}

		public async Task SetLastUser(User user)
		{
			await _localStorageService.SetItemAsync("Playback.LastUser", user);
		}

		public async Task<LoginResult> GetToken()
		{
			return await _localStorageService.GetItemAsync<LoginResult>("Playback.LoginToken");
		}

		public async Task SetToken(LoginResult user)
		{
			await _localStorageService.SetItemAsync("Playback.LoginToken", user);
			if (user == null)
			{
				await SetLastUser(null);
			}
		}
	}
}
