using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blazored.Video;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Components.VideoEx
{
	public partial class VideoExComponentBase : ComponentBase, IDisposable
	{
		public VideoExComponentBase()
		{

		}

		public string ContainerId { get; set; } = Guid.NewGuid().ToString("N");

		[Parameter]
		public EventCallback FullscreenRequest { get; set; }
		[Parameter]
		public EventCallback<bool> ShowControlsChanged { get; set; }
		[Parameter]
		public bool ShowControls { get; set; }

		[Parameter]
		public Func<string> GetPoster { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);

			if (firstRender)
			{
				await JS.InvokeVoidAsync("VideoEx", new
				{
					id = ContainerId,
					that = DotNetObjectReference.Create(this)
				});
			}
			if (firstRender)
			{
				await ConfigureEvents();
			}
		}

		[JSInvokable("onFullscreenRequestButton")]
		public async Task OnFullscreenRequestButton()
		{
			await FullscreenRequest.InvokeAsync(null);
		}

		[JSInvokable("onControlsStatusChange")]
		public async Task OnControlsStatusChange(bool showControls)
		{
			ShowControls = showControls;
			await ShowControlsChanged.InvokeAsync(showControls);
		}

		public async Task SetMediaData(MediaData mediaData)
		{
			await JS.InvokeVoidAsync("Blazored.setMediaData", mediaData);
		}

		public void Dispose()
		{
			this.PausePlayback();
		}
	}

	public class MediaData
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }
		[JsonPropertyName("artist")]
		public string Artist { get; set; }
		[JsonPropertyName("album")]
		public string Album { get; set; }
		
		[JsonPropertyName("artwork")]
		public MediaDataArtwork[] Artwork { get; set; }
	}

	public class MediaDataArtwork
	{
		[JsonPropertyName("src")]
		public string Src { get; set; }
		[JsonPropertyName("sizes")]
		public string Sizes { get; set; }
		[JsonPropertyName("type")]
		public string Type { get; set; }
	}
}
