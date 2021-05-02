using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;

namespace JPB.InhousePlayback.Client.Pages
{
	public class SelectUserBase : ComponentBase
	{
		public SelectUserBase()
		{
			Users = new List<User>();
		}

		[Inject]
		public HttpService HttpService { get; set; }
		[Inject]
		public NavigationManager NavigationManager { get; set; }
		[Inject]
		public UserManagerService UserManagerService { get; set; }
		
		public IList<User> Users { get; set; }
		public User ToBeAdded { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var users = (await HttpService.UserApiAccess.GetAll()).Object;
			Users = new List<User>(users);
		}

		public async Task AddUser()
		{
			var usr = (await HttpService.UserApiAccess.Create(ToBeAdded)).Object;
			Users.Add(usr);
			ToBeAdded = null;
		}

		public async Task SetUser(User user)
		{
			await UserManagerService.SetCurrentUser(user);
			NavigationManager.NavigateTo("/genres");
		}
	}
}
