using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class TitlesApiAccess : AccessBase
	{
		public TitlesApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("TitlesApi"))
		{
		}

		public async Task<ApiResult<TitleWithPlayback[]>> GetAll(int seasonId, int userId)
		{
			return await Get<TitleWithPlayback[]>(BuildUrl(base.Url + "GetAll", new
			{
				seasonId,
				userId
			}));
		}

		public async Task<ApiResult<Title>> GetSingle(int titleId)
		{
			return await Get<Title>(BuildUrl(base.Url + "GetSingle", new
			{
				titleId
			}));
		}

		public async Task<ApiResult> Update(Title title)
		{
			return await Post(BuildUrl(base.Url + "Update"), title);
		}
		
		public async Task<ApiResult<PlaybackWithTitle[]>> GetLastPlayed(int userId)
		{
			return await Get<PlaybackWithTitle[]>(BuildUrl(base.Url + "GetLastPlayed", new
			{
				userId
			}));
		}

		public async Task<ApiResult<Title>> GetNextTitle(int titleId)
		{
			return await Get<Title>(BuildUrl(base.Url + "GetNextTitle", new
			{
				titleId
			}));
		}

		public async Task<ApiResult<TitleWithPlayback>> GetSingleWithPlaybackStatus(int titleId, int userId)
		{
			return await Get<TitleWithPlayback>(BuildUrl(base.Url + "GetSingleWithPlaybackStatus", new
			{
				titleId,
				userId
			}));
		}
	}
}