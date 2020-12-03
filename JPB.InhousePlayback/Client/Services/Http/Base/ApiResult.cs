using System.Net;
using System.Runtime.ExceptionServices;

namespace JPB.InhousePlayback.Client.Services.Http.Base
{
	public struct ApiResult<T> : IApiResult
	{
		public ApiResult(HttpStatusCode statusCode, bool success, string statusMessage, ExceptionDispatchInfo exception)
		{
			StatusCode = statusCode;
			Success = success;
			StatusMessage = statusMessage;
			Exception = exception;
			Object = default(T);
		}

		public ApiResult(HttpStatusCode statusCode, bool success, T o, string statusMessage)
		{
			StatusCode = statusCode;
			Success = success;
			Object = o;
			StatusMessage = statusMessage;
			Exception = null;
		}

		public T Object { get; private set; }
		public HttpStatusCode StatusCode { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }
		public ExceptionDispatchInfo Exception { get; }

		public ApiResult<T> UnpackOrThrow()
		{
			if (Success)
			{
				return this;
			}

			Exception?.Throw();
			return this;
		}
	}

	public struct ApiResult : IApiResult
	{
		public ApiResult(HttpStatusCode statusCode, bool success, string statusMessage,
			ExceptionDispatchInfo exception = null)
		{
			StatusCode = statusCode;
			Success = success;
			StatusMessage = statusMessage;
			Exception = exception;
		}

		public HttpStatusCode StatusCode { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }
		public ExceptionDispatchInfo Exception { get; }

		public ApiResult UnpackOrThrow()
		{
			if (Success)
			{
				return this;
			}

			Exception?.Throw();
			return this;
		}
	}
}