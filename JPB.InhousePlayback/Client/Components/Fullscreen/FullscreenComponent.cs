using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Components.Fullscreen
{
	public class FullscreenComponentBase : ComponentBase, IDisposable
	{
		public FullscreenComponentBase()
		{
			ContainerId = Guid.NewGuid().ToString("N");
		}

		[Inject]
		public IJSRuntime JsRuntime { get; set; }
		public ElementReference Container { get; set; }
		
		[Parameter] 
		public RenderFragment ChildContent { get; set; }

		public string ContainerId { get; set; }

		[Parameter]
		public EventCallback OnFullscreenChanged { get; set; }

		public string EventKey { get; set; }

		public async Task RequestFullscreen()
		{
			await JsRuntime.InvokeVoidAsync("fullscreenBlazorApi.requestFullscreen", Container);
		}

		public async Task ExitFullscreen()
		{
			await JsRuntime.InvokeVoidAsync("fullscreenBlazorApi.exitFullscreen");
		}

		protected override async Task OnInitializedAsync()
		{
			EventKey = await JsRuntime.InvokeAsync<string>("fullscreenBlazorApi.subscribeToFullscreenChangedEvent", DotNetObjectReference.Create(this));
		}

		[JSInvokable("onFullscreenChangedCallback")]
		public void OnFullscreenChangedCallback()
		{
			OnFullscreenChanged.InvokeAsync(null);
		}

		public async Task<bool> HasFullscreen()
		{
			return await JsRuntime.InvokeAsync<bool>("fullscreenBlazorApi.hasFullscreen", Container);
		}

		public async Task<string> GetFullscreenElement()
		{
			return await JsRuntime.InvokeAsync<string>("fullscreenBlazorApi.getFullscreenElement");
		}

		public void Dispose()
		{
			JsRuntime.InvokeVoidAsync("fullscreenBlazorApi.unsubscribeToFullscreenChangedEvent", EventKey);
		}
	}
}
