using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using JPB.InhousePlayback.Client.Pages.Auth;
using JPB.InhousePlayback.Client.Services.Breadcrumb;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.LocalStorage;
using JPB.InhousePlayback.Client.Services.MyMediaSession;
using JPB.InhousePlayback.Client.Services.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace JPB.InhousePlayback.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");
			builder.Services.AddSingleton(sp => new HttpClient
			{
				BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
			});
			builder.Services.AddSingleton<HttpService>();
			builder.Services.AddSingleton<UserManagerService>();
			builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();
			builder.Services.AddSingleton<PlayblackLocalStorageService>();
			builder.Services.AddSingleton<BreadcrumbService>();
			builder.Services.AddSingleton<CurrentUserStore>();
			builder.Services.AddScoped<AuthenticationStateProvider, ClientAuthenticationStateProvider>();
			builder.Services.AddScoped<ClientAuthenticationStateProvider>();
			builder.Services.AddSingleton<MyMediaSessionService>();
			//builder.Services.AddMediaSessionService();
			//builder.Services.AddScoped<IAuthorizationPolicyProvider, ClientAuthPolicy>();
			//builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
			builder.Services.AddAuthorizationCore();
			
			var app = builder.Build();
			foreach (IRequireInit builderService in builder.Services
				.Where(e => typeof(IRequireInit).IsAssignableFrom(e.ServiceType) && e.Lifetime == ServiceLifetime.Singleton)
				.Select(e => app.Services.GetService(e.ServiceType)))
			{
				builderService.Init();
			}
			foreach (IRequireInitAsync builderService in builder.Services
				.Where(e => typeof(IRequireInitAsync).IsAssignableFrom(e.ServiceType) && e.Lifetime == ServiceLifetime.Singleton)
				.Select(e => app.Services.GetService(e.ServiceType)))
			{
				await builderService.Init();
			}

			await app.RunAsync();
		}
	}
}
