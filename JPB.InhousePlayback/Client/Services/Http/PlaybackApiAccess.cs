using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.ApiModel;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class PlaybackApiAccess : AccessBase
	{
		public PlaybackApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("PlaybackApi"))
		{
		}

		public async Task<ApiResult> UpdatePlayback(int titleId, int userId, int position)
		{
			return await Post(BuildUrl(base.Url + "OnPlayback", new
			{
				titleId,
				userId,
				position
			}));
		}

		public async Task<ApiResult<StreamIdModel>> GetStreamId(int titleId)
		{
			return await Get<StreamIdModel>(BuildUrl(Url + "StartPlayback", new { titleId }));
		}

		public async Task EndStream(string streamId)
		{
			await Post(BuildUrl(Url + "EndPlayback", new { streamId }));
		}
	}
}