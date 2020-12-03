using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.LocalStorage;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Shared.ApiModel;
using JPB.InhousePlayback.Shared.DbModels;
using JPB.InhousePlayback.Shared.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JPB.InhousePlayback.Client.Pages.Auth
{
	public class LoginBase : ComponentBase
	{
		[Inject]
		public HttpService HttpService { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Inject]
		public AuthenticationStateProvider ClientAuthenticationStateProvider { get; set; }

		[Parameter]
		public string ReturnUrl { get; set; }

		public string Username { get; set; }
		public string Password { get; set; }

		protected override async Task OnParametersSetAsync()
		{
			var state = await ClientAuthenticationStateProvider.GetAuthenticationStateAsync();
			if (state.User.Identity.IsAuthenticated)
			{
				NavigationManager.NavigateTo("/Genres");
			}
		}

		public async Task Login()
		{
			var apiResult = await HttpService.AuthApiAccess.Login(Username, Password);
			Console.WriteLine("OnLogin" + apiResult.Success);
			if (apiResult.Success)
			{
				await (ClientAuthenticationStateProvider as ClientAuthenticationStateProvider).SetState(apiResult.Object, true);
				NavigationManager.NavigateTo("/Genres");
			}
		}
	}

	public class CurrentUserStore : IRequireInitAsync
	{
		private readonly HttpService _httpService;
		public PlayblackLocalStorageService PlayblackLocalStorageService { get; }

		public CurrentUserStore(PlayblackLocalStorageService playblackLocalStorageService, HttpService httpService)
		{
			_httpService = httpService;
			PlayblackLocalStorageService = playblackLocalStorageService;
		}

		public async Task Init()
		{
			CurrentToken = await PlayblackLocalStorageService.GetToken();
			if (CurrentToken != null)
			{
				_httpService.SetToken(CurrentToken.Token);
			}
		}

		public LoginResult CurrentToken { get; set; }

		public async Task SetToken(LoginResult state)
		{
			CurrentToken = state;
			await PlayblackLocalStorageService.SetToken(state);
		}

		public LoginResult GetAccount()
		{
			return CurrentToken;
		}
	}

	public class ClientAuthenticationStateProvider : AuthenticationStateProvider
	{
		private readonly CurrentUserStore _store;
		private readonly HttpService _httpService;
		private readonly NavigationManager _navigationManager;

		public ClientAuthenticationStateProvider(CurrentUserStore store, HttpService httpService, NavigationManager navigationManager)
		{
			_store = store;
			_httpService = httpService;
			_navigationManager = navigationManager;
		}
		
		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var token = _store.GetAccount();

			if (token != null)
			{
				var tokenValue = JwtCoder.DecodeToken(token.Token);
				foreach (var tokenValueClaim in tokenValue.Claims)
				{
					Console.WriteLine("Claims: " + tokenValueClaim.Type + ":" + tokenValueClaim.Value);
				}

				var claimsIdentity = new ClaimsIdentity(tokenValue.Claims, "bearer", ClaimTypes.NameIdentifier, ClaimTypes.Role);
				return new AuthenticationState(new ClaimsPrincipal(claimsIdentity));
			}
			
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], null)));
		}
		
		public async Task SetState(LoginResult state, bool invokeEvent = false)
		{
			_httpService.SetToken(state?.Token);
			if (state != null)
			{
				var userCall = (await  _httpService.AuthApiAccess.Me());
				if (!userCall.Success)
				{
					await _store.SetToken(null);
				}
				else
				{
					state.User = userCall.Object;
					await _store.SetToken(state);
				}
			}
			else
			{
				await _store.SetToken(null);
			}

			if (invokeEvent)
			{
				base.NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
			}
		}

		public async Task Logoff()
		{
			Console.WriteLine("LOGOFF");
			await SetState(null, true);
			_navigationManager.NavigateTo("/", true);
		}
	}
}
