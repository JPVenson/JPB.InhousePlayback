using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class HttpService
	{
		private readonly HttpClient _client;

		public HttpService(HttpClient client, NavigationManager navigationManager)
		{
			_client = client;

			var accessesors = new List<AccessBase>();
			foreach (var propertyInfo in GetType().GetProperties().Where(e => typeof(AccessBase).IsAssignableFrom(e.PropertyType)))
			{
				var instance = Activator.CreateInstance(propertyInfo.PropertyType, client) as AccessBase;
				accessesors.Add(instance);
				propertyInfo.SetValue(this, instance);
			}

			//IHubConnectionBuilder connectionBuilder = new HubConnectionBuilder();
			//foreach (var accessesor in accessesors)
			//{
			//	foreach (var accessesorSingalRRoute in accessesor.SingalRRoutes)
			//	{
			//		connectionBuilder = connectionBuilder.WithUrl(navigationManager.ToAbsoluteUri(accessesorSingalRRoute));
			//	}
			//}

			//HubConnection = connectionBuilder.Build();

			//foreach (var accessesor in accessesors)
			//{
			//	accessesor.AddSignalRHandlers(HubConnection);
			//}

			//HubConnection.StartAsync();
		}

		public HubConnection HubConnection { get; set; }

		public UserApiAccess UserApiAccess { get; set; }
		public GenreApiAccess GenreApiAccess { get; set; }
		public SeasonApiAccess SeasonApiAccess { get; set; }
		public TitlesApiAccess TitlesApiAccess { get; set; }
		public PlaybackApiAccess PlaybackApiAccess { get; set; }
		public AuthApiAccess AuthApiAccess { get; set; }

		public void SetToken(string tokenToken)
		{
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", tokenToken);
			//_client.DefaultRequestHeaders.TryAddWithoutValidation("Authentification", "Bearer " + tokenToken);
		}
	}
}
