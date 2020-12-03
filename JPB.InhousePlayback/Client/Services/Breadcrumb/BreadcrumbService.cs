using System;
using System.Text;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Services.Breadcrumb
{
	public class BreadcrumbService
	{
		private readonly HttpService _httpService;
		private readonly IJSRuntime _jsRuntime;

		public BreadcrumbService(HttpService httpService, IJSRuntime jsRuntime)
		{
			_httpService = httpService;
			_jsRuntime = jsRuntime;
		}

		public event EventHandler Changed;

		public Genre Genre { get; set; }
		public Season Season { get; set; }
		public Title Title { get; set; }

		public async Task InitWithGenre(Genre genre, bool load)
		{
			if (Genre?.GenreId == genre.GenreId)
			{
				return;
			}

			if (load)
			{
				Genre = (await _httpService.GenreApiAccess.GetSingle(genre.GenreId)).Object;
			}
			else
			{
				Genre = genre;
			}
			await OnChanged();
		}

		public async Task InitWithSeason(Season season, bool load)
		{
			if (Season?.SeasonId == season.SeasonId)
			{
				return;
			}
			if (load)
			{
				Season = (await _httpService.SeasonApiAccess.GetSingle(season.SeasonId)).Object;
			}
			else
			{
				Season = season;
			}

			await OnChanged();
			await InitWithGenre(new Genre() { GenreId = Season.IdGenre }, true);
		}

		public async Task InitWithTitle(Title title, bool load)
		{
			if (Title?.TitleId == title.TitleId)
			{
				return;
			}

			if (load)
			{
				Title = (await _httpService.TitlesApiAccess.GetSingle(title.TitleId)).Object;
			}
			else
			{
				Title = title;
			}

			await OnChanged();
			await InitWithSeason(new Season() { SeasonId = Title.IdSeason }, true);
		}

		public virtual async Task OnChanged()
		{
			Changed?.Invoke(this, EventArgs.Empty);
			var title = new StringBuilder();
			if (Genre != null)
			{
				title.Append(Genre.Name);
				if (Season != null)
				{
					title.Append("-");
					title.Append(Season.Name);
					if (Title != null)
					{
						title.Append("-");
						title.Append(Title.Name);
					}
				}
			}

			await _jsRuntime.InvokeVoidAsync("setTitle", title.ToString());
		}
	}
}
