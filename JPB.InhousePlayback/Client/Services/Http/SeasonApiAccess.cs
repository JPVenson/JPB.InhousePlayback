using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class SeasonApiAccess : AccessBase
	{
		public SeasonApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("SeasonsApi"))
		{
		}

		public async Task<ApiResult<Season[]>> GetAll(int genreId)
		{
			return await Get<Season[]>(BuildUrl(base.Url + "GetAll", new
			{
				genreId
			}));
		}

		public async Task<ApiResult<Season>> GetSingle(int seasonId)
		{
			return await Get<Season>(BuildUrl(base.Url + "GetSingle", new
			{
				seasonId
			}));
		}

		public async Task<ApiResult> Update(Season title)
		{
			return await Post(BuildUrl(base.Url + "Update"), title);
		}

		public async Task<ApiResult> Delete(int seasonId)
		{
			return await Post(BuildUrl(base.Url + "Delete", new { seasonId }));
		}
	}
}