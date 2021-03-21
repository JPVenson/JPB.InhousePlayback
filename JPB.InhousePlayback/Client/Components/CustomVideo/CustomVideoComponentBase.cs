using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Video;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Components.CustomVideo
{
	public class CustomVideoComponentBase : BlazoredVideo
	{
		public string ContainerId { get; set; } = Guid.NewGuid().ToString("N");

		[Parameter]
		public EventCallback FullscreenRequest { get; set; }
		[Parameter]
		public EventCallback<bool> ShowControlsChanged { get; set; }
		
		[Parameter]
		public Func<string> GetPoster { get; set; }
		[Parameter]
		public bool ShowControls { get; set; }


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

		public void Dispose()
		{
			this.PausePlayback();
		}
	}
}
