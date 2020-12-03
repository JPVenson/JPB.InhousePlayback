using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Json;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace JPB.InhousePlayback.Client.Services.Http.Base
{
	public class AccessBase
	{
		private JsonSerializerSettings _jsonSerializerSettings;

		public AccessBase(HttpClient httpClient, string url)
		{
			HttpClient = httpClient;
			Url = url;
			
			_jsonSerializerSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			};
			SingalRRoutes = new List<string>();
		}

		public HttpClient HttpClient { get; }
		public string Url { get; }

		public List<string> SingalRRoutes { get; set; }

		public virtual void AddSignalRHandlers(HubConnection connection)
		{

		}

		public async Task<ApiResult<T>> Get<T>(string url
			//, object body = null
		)
		{
			HttpResponseMessage content = null;
			try
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
				//if (body != null)
				//{
				//	requestMessage.Content = new ObjectContent(body.GetType(), body, new JsonMediaTypeFormatter()
				//	{
				//		SerializerSettings = _jsonSerializerSettings
				//	});
				//}

				using (content = await HttpClient
					.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead)
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult<T>(content.StatusCode, false, 
							await content.Content.ObtainMessage(), null);
					}

					return new ApiResult<T>(content.StatusCode, content.IsSuccessStatusCode,
						await content.Content.ReadFromJsonAsync<T>(), content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult<T>(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<T> GetValue<T>(string url)
		{
			return (await Get<T>(url)).Object;
		}

		public async Task<ApiResult> Post<T>(string url, T data)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await HttpClient
					.PostAsync(url, new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter()
					{
						SerializerSettings = _jsonSerializerSettings
					}))
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}

					return new ApiResult(content.StatusCode, content.IsSuccessStatusCode, content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<ApiResult> Post(string url)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await HttpClient.PostAsync(url, new MultipartContent())
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}
					return new ApiResult(content.StatusCode, content.IsSuccessStatusCode, content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<ApiResult<TE>> Post<T, TE>(string url, T data)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await HttpClient.PostAsync(url, new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter()
					{
						SerializerSettings = _jsonSerializerSettings
					}))
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult<TE>(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}
					return new ApiResult<TE>(content.StatusCode,
						content.IsSuccessStatusCode,
						await content.Content.ReadAsAsync<TE>(),
						content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult<TE>(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public static string EncodeUrlString(object data)
		{
			if (data is string dataString)
			{
				return HttpUtility.UrlPathEncode(dataString);
			}

			var values = data.GetType().GetProperties()
				.Select(e => new Tuple<string, object>(e.Name, e.GetMethod.Invoke(data, null)))
				.Where(e => e.Item2 != null);
			return values
				.Select(e => Uri.EscapeDataString(e.Item1) + "=" + Uri.EscapeDataString(e.Item2.ToString()))
				.Aggregate((e, f) => e + "&" + f);
		}

		public static string BuildUrl(string basePath, object data = null)
		{
			if (data == null)
			{
				return basePath;
			}

			return basePath + "?" + EncodeUrlString(data);
		}

		protected static string BuildApi(string apiName)
		{
			return "/api/" + apiName + "/";
		}
	}
}