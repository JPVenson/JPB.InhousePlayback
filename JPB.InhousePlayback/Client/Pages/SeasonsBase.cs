using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Breadcrumb;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;

namespace JPB.InhousePlayback.Client.Pages
{
	public class SeasonsBase : ComponentBase
	{
		public SeasonsBase()
		{
			Seasons = new List<Season>();
		}

		[Inject]
		public HttpService HttpService { get; set; }

		[Inject]
		public BreadcrumbService BreadcrumbService { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Parameter]
		public string GenreId { get; set; }
		
		public IList<Season> Seasons { get; set; }
		
		protected override async Task OnParametersSetAsync()
		{
			var genreId = int.Parse(GenreId);
			
			BreadcrumbService.Title = null;
			await BreadcrumbService.InitWithGenre(new Genre(){GenreId = genreId}, true);
			var seasons = (await HttpService.SeasonApiAccess.GetAll(genreId)).Object;
			Seasons = new List<Season>(seasons);
		}
		
		public async Task SetSeason(Season season)
		{
			await BreadcrumbService.InitWithSeason(season, false);
			NavigationManager.NavigateTo("/titles/" + season.SeasonId);
		}
	}
}
