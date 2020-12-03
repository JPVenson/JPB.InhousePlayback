using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JPB.InhousePlayback.Client.Services.Http.Base
{
	public static class HttpErrorExtentions
	{
		private class ErrorMessageClass
		{
			[JsonProperty("message")]
			public string Message { get; set; }
		}

		public static async Task<string> ObtainMessage(this HttpContent content)
		{
			var error = await content.ReadAsStringAsync();
			try
			{
				if (string.IsNullOrWhiteSpace(error))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<ErrorMessageClass>(error).Message;
			}
			catch
			{
				return error;
			}
		}
	}
}