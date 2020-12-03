using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Blazored.Video;
using Blazored.Video.Support;
using JPB.InhousePlayback.Client.Components.Fullscreen;
using JPB.InhousePlayback.Client.Services.Breadcrumb;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Timer = System.Timers.Timer;

namespace JPB.InhousePlayback.Client.Pages
{
	public class TitlesBase : ComponentBase
	{
		public TitlesBase()
		{
			Titles = new List<TitleWithPlayback>();
		}

		[Inject]
		public HttpService HttpService { get; set; }

		[Inject]
		public UserManagerService UserManagerService { get; set; }
		
		[Inject]
		public BreadcrumbService BreadcrumbService { get; set; }

		[Parameter]
		public string SeasonId { get; set; }
		
		public IList<TitleWithPlayback> Titles { get; set; }

		protected override async Task OnParametersSetAsync()
		{
			var seasonId = int.Parse(SeasonId);
			var titles = (await HttpService.TitlesApiAccess.GetAll(seasonId, UserManagerService.CurrentUser.UserId)).Object;
			Titles = new List<TitleWithPlayback>(titles);
		}
	}
}
