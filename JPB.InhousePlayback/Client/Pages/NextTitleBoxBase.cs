using System;
using System.Threading.Tasks;
using System.Timers;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Pages
{
	public class NextTitleBoxBase : ComponentBase, IDisposable
	{
		public NextTitleBoxBase()
		{
			Timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
			Timer.Elapsed += Timer_Elapsed;
			Timer.AutoReset = true;
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (!TimeUntilAutoStart.HasValue)
			{
				return;
			}

			TimeUntilAutoStart -= TimeSpan.FromSeconds(1);
			StateHasChanged();
			if (TimeUntilAutoStart.Value.TotalSeconds <= 0)
			{
				ElapsedDone();
			}
		}

		public void ElapsedDone()
		{
			NextTitlePlayback();
			StopTimer();
		}


		public Action NextTitlePlayback { get; set; }
		public bool Display { get; private set; }
		public Title NextTitle { get; set; }

		[Inject]
		public IJSRuntime JsRuntime { get; set; }

		public TimeSpan? TimeUntilAutoStart { get; set; } = DefaultWaitTime;
		public static readonly TimeSpan DefaultWaitTime = TimeSpan.FromSeconds(10);

		public Timer Timer { get; set; }

		public async Task OnDisplayed()
		{
			await JsRuntime.InvokeVoidAsync("BlazoredFocusApi.focusByName", "nextTitleBtn");
		}

		public async Task Show()
		{
			Display = true;
			StateHasChanged();
			await Task.Yield();
			await OnDisplayed();
		}

		public async Task Hide()
		{
			StopTimer();
			await Task.CompletedTask;
		}
		
		public void StartTimer()
		{
			Display = true;
			TimeUntilAutoStart = DefaultWaitTime;
			Timer.Start();
		}

		public void StopTimer()
		{
			TimeUntilAutoStart = null;
			Timer.Stop();
			StateHasChanged();
		}

		public void Dispose()
		{
			Timer?.Dispose();
		}
	}
}