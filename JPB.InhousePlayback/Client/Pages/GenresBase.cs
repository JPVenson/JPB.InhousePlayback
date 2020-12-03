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
	public class GenresBase : ComponentBase
	{
		public GenresBase()
		{
			Genre = new List<Genre>();
			LastPlayed = new List<PlaybackWithTitle>();
		}

		[Inject]
		public HttpService HttpService { get; set; }
		[Inject]
		public BreadcrumbService BreadcrumbService { get; set; }
		[Inject]
		public NavigationManager NavigationManager { get; set; }
		[Inject]
		public UserManagerService UserManagerService { get; set; }
		
		public IList<Genre> Genre { get; set; }
		public IList<PlaybackWithTitle> LastPlayed { get; set; }

		protected override async Task OnInitializedAsync()
		{
			BreadcrumbService.Genre = null;
			BreadcrumbService.Season = null;
			BreadcrumbService.Title = null;
			await BreadcrumbService.OnChanged();

			Genre = (await HttpService.GenreApiAccess.GetAll()).Object;
			LastPlayed = (await HttpService.TitlesApiAccess.GetLastPlayed(UserManagerService.CurrentUser.UserId))
				.Object;
		}

		public async Task SetGenre(Genre genre)
		{
			await BreadcrumbService.InitWithGenre(genre, false);
			NavigationManager.NavigateTo("/seasons/" + genre.GenreId);
		}
	}
}
