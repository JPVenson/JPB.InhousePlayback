using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;

namespace JPB.InhousePlayback.Client.Pages.Admin
{
	public class UpdateDataBase : ComponentBase
	{
		public UpdateDataBase()
		{
			Genre = new List<GenreData>();
		}

		[Inject]
		public HttpService HttpService { get; set; }

		public IList<GenreData> Genre { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var genres = (await HttpService.GenreApiAccess.GetAll()).Object;
			for (var index = 0; index < genres.Length; index++)
			{
				var genre = genres[index];
				Genre.Add(new GenreData(genre));
			}
		}

		public async Task ExpandGenre(GenreData genre)
		{
			if (!genre.Children.Any())
			{
				var seasons = (await HttpService.SeasonApiAccess.GetAll(genre.Data.GenreId)).Object.OrderBy(e => e.OrderNo).ToArray();
				for (var index = 0; index < seasons.Length; index++)
				{
					var season = seasons[index];
					var seasonData = new SeasonData(season);
					seasonData.Data.OrderNo = index;
					genre.Children.Add(seasonData);
				}

				genre.Collapsed = false;
			}
			else
			{
				genre.Collapsed = !genre.Collapsed;
			}
		}

		public async Task ExpandSeason(SeasonData season)
		{
			if (!season.Children.Any())
			{
				var titles = (await HttpService.TitlesApiAccess.GetAll(season.Data.SeasonId, 0)).Object.OrderBy(e => e.OrderNo).ToArray();
				for (var index = 0; index < titles.Length; index++)
				{
					var title = titles[index];
					var titleData = new TitleData(title);
					titleData.Data.OrderNo = index;
					season.Children.Add(titleData);
				}

				season.Collapsed = false;
			}
			else
			{
				season.Collapsed = !season.Collapsed;
			}
		}

		public async Task UpdateAll()
		{
			foreach (var genreData in Genre.ToArray())
			{
				if (CanUpdate(genreData))
				{
					await Update(genreData);
					if (genreData.MarkedAsDeleted)
					{
						Genre.Remove(genreData);
						continue;
					}
					genreData.Hash = genreData.Data.GetHashCode();
				}

				foreach (var genreDataChild in genreData.Children.ToArray())
				{
					if (CanUpdate(genreDataChild))
					{
						await Update(genreDataChild);
						if (genreDataChild.MarkedAsDeleted)
						{
							genreData.Children.Remove(genreDataChild);
							continue;
						}
						genreDataChild.Hash = genreDataChild.Data.GetHashCode();
					}

					foreach (var titleData in genreDataChild.Children.ToArray())
					{
						if (CanUpdate(titleData))
						{
							await Update(titleData);
							if (titleData.MarkedAsDeleted)
							{
								genreDataChild.Children.Remove(titleData);
								continue;
							}
							titleData.Hash = titleData.Data.GetHashCode();
						}
					}
				}
			}
			StateHasChanged();
		}

		public async Task Update(TitleData data)
		{
			if (data.MarkedAsDeleted)
			{
				await HttpService.TitlesApiAccess.Delete(data.Data.TitleId);
				return;
			}

			await HttpService.TitlesApiAccess.Update(data.Data);
		}

		public async Task Update(SeasonData data)
		{
			if (data.MarkedAsDeleted)
			{
				await HttpService.SeasonApiAccess.Delete(data.Data.SeasonId);
				return;
			}
			await HttpService.SeasonApiAccess.Update(data.Data);
		}

		public async Task Update(GenreData data)
		{
			if (data.MarkedAsDeleted)
			{
				await HttpService.GenreApiAccess.Delete(data.Data.GenreId);
				return;
			}
			await HttpService.GenreApiAccess.Update(data.Data);
		}

		public bool CanUpdate(TitleData data)
		{
			return data.Hash != data.Data.GetHashCode() || data.MarkedAsDeleted;
		}

		public bool CanUpdate(SeasonData data)
		{
			return data.Hash != data.Data.GetHashCode() || data.MarkedAsDeleted;
		}

		public bool CanUpdate(GenreData data)
		{
			return data.Hash != data.Data.GetHashCode() || data.MarkedAsDeleted;
		}

		public void MoveSeason(GenreData genre, SeasonData season, bool direction)
		{
			var orderedChildren = genre.Children.OrderBy(e => e.Data.OrderNo).ToList();
			var indexOfSeason = orderedChildren.IndexOf(season);
			var targetIndex = direction ? indexOfSeason - 1 : indexOfSeason + 1;
			var toSwap = orderedChildren[targetIndex];
			toSwap.Data.OrderNo = indexOfSeason;
			season.Data.OrderNo = targetIndex;
			StateHasChanged();
		}

		public void MoveTitle(SeasonData season, TitleData title, bool direction)
		{
			var targetIndex = direction ? title.Data.OrderNo.Value - 1 : title.Data.OrderNo.Value + 1;
			var toSwap = season.Children.First(e => e.Data.OrderNo == targetIndex);
			toSwap.Data.OrderNo = title.Data.OrderNo;
			title.Data.OrderNo = targetIndex;
			StateHasChanged();
		}
	}

	public class GenreData
	{
		public GenreData(Genre data)
		{
			Data = data;
			Hash = Data.GetHashCode();
			Children = new List<SeasonData>();
		}

		public Genre Data { get; }
		public IList<SeasonData> Children { get; }
		public bool Collapsed { get; set; }
		public bool MarkedAsDeleted { get; set; }
		public int Hash { get; set; }
	}

	public class SeasonData
	{
		public SeasonData(Season data)
		{
			Data = data;
			Hash = Data.GetHashCode();
			Children = new List<TitleData>();
		}

		public int Hash { get; set; }

		public Season Data { get; }
		public IList<TitleData> Children { get; }
		public bool Collapsed { get; set; }
		public bool MarkedAsDeleted { get; set; }
	}

	public class TitleData
	{
		public TitleData(Title title)
		{
			Data = title;
			Hash = Data.GetHashCode();
		}

		public int Hash { get; set; }
		public bool MarkedAsDeleted { get; set; }
		public Title Data { get; set; }
	}
}
