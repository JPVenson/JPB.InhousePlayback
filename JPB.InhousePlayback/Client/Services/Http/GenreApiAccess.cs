using System.Net.Http;
using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.Http.Base;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Services.Http
{
	public class GenreApiAccess : AccessBase
	{
		public GenreApiAccess(HttpClient httpClient) : base(httpClient, BuildApi("GenreApi"))
		{
		}

		public async Task<ApiResult<Genre[]>> GetAll()
		{
			return await Get<Genre[]>(BuildUrl(base.Url + "GetAll"));
		}

		public async Task<ApiResult<Genre>> GetSingle(int genreId)
		{
			return await Get<Genre>(BuildUrl(base.Url + "GetSingle", new
			{
				genreId
			}));
		}

		public async Task<ApiResult> Update(Genre title)
		{
			return await Post(BuildUrl(base.Url + "Update"), title);
		}
	}
}