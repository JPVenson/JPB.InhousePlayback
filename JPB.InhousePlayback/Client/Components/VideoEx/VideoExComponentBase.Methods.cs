using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.Video;
using Blazored.Video.Support;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace JPB.InhousePlayback.Client.Components.VideoEx
{
	public partial class VideoExComponentBase
	{
		public async Task StartPlayback()
		{
			await JS.InvokeVoidAsync("Blazored.invoke", videoRef, "play");
		}

		public async Task PausePlayback()
		{
			await JS.InvokeVoidAsync("Blazored.invoke", videoRef, "pause");
		}

		public async Task ReloadControl()
		{
			await JS.InvokeVoidAsync("Blazored.invoke", videoRef, "load");
		}

		public async Task<bool> CanPlayMediaType(string mediaType)
		{
			return await JS.InvokeAsync<bool>("Blazored.invoke", videoRef, "canPlayType", mediaType);
		}
	}
}