using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.LocalStorage;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;

namespace JPB.InhousePlayback.Client.Services.UserManager
{
	public class UserManagerService : IRequireInitAsync
	{
		private readonly PlayblackLocalStorageService _playblackLocalStorageService;
		private readonly NavigationManager _navigationManager;

		public UserManagerService(PlayblackLocalStorageService playblackLocalStorageService, NavigationManager navigationManager)
		{
			_playblackLocalStorageService = playblackLocalStorageService;
			_navigationManager = navigationManager;
		}

		public async Task Init()
		{
			CurrentUser = await _playblackLocalStorageService.GetLastUser();
		}

		public User CurrentUser { get; private set; }

		public async Task SetCurrentUser(User user)
		{
			CurrentUser = user;
			await _playblackLocalStorageService.SetLastUser(user);
		}

		public async Task Logoff()
		{
			CurrentUser = null;
			await _playblackLocalStorageService.SetLastUser(null);
			_navigationManager.NavigateTo("/users");
		}
	}
}
