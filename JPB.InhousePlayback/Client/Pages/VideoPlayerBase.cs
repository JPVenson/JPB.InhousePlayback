using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Blazored.Video;
using Blazored.Video.Support;
using JPB.InhousePlayback.Client.Components.Fullscreen;
using JPB.InhousePlayback.Client.Components.VideoEx;
using JPB.InhousePlayback.Client.Services.Breadcrumb;
using JPB.InhousePlayback.Client.Services.Http;
using JPB.InhousePlayback.Client.Services.UserManager;
using JPB.InhousePlayback.Client.Util;
using JPB.InhousePlayback.Shared.DbModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MimeKit;

namespace JPB.InhousePlayback.Client.Pages
{
	public class VideoPlayerBase : ComponentBase, IDisposable
	{
		public VideoPlayerBase()
		{
			Timer ??= new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
			Timer.AutoReset = true;
			Timer.Elapsed += Timer_Elapsed;

			VideoStateOptions = new Dictionary<VideoEvents, VideoStateOptions>();
			var videoStateOptions = new VideoStateOptions { CurrentTime = true, CurrentSrc = true, Duration = true, Volume = true };
			VideoStateOptions[VideoEvents.TimeUpdate] = videoStateOptions;
			VideoStateOptions[VideoEvents.Play] = videoStateOptions;
			VideoStateOptions[VideoEvents.Pause] = videoStateOptions;
			VideoStateOptions[VideoEvents.VolumeChange] = videoStateOptions;
		}

		[Inject]
		public IJSRuntime JsRuntime { get; set; }

		public FullscreenComponent FullscreenComponent { get; set; }

		[Parameter]
		public string TitleId { get; set; }

		[Parameter]
		public string Position { get; set; }
		
		[Inject]
		public HttpService HttpService { get; set; }

		[Inject]
		public UserManagerService UserManagerService { get; set; }

		[Inject]
		public BreadcrumbService BreadcrumbService { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		public VideoExComponent BlazoredVideo { get; set; }
		public Dictionary<VideoEvents, VideoStateOptions> VideoStateOptions { get; set; }
		
		public Title CurrentPlayback { get; set; }
		public Playback LastPlayback { get; set; }

		public Timer Timer { get; set; }

		public double LastKnownPosition { get; set; }
		public double LastUpdatePosition { get; set; }

		public Title NextTitle { get; set; }

		private bool _showTitleInCurrentPlayback;
		public bool ShowTitleInCurrentPlayback
		{
			get { return _showTitleInCurrentPlayback; }
			set
			{
				_showTitleInCurrentPlayback = value;
				//if (value)
				//{
				//	TitleShown = DateTime.Now.AddSeconds(4);
				//}
			}
		}

		public DateTime TitleShown { get; set; }
		public bool ShowNextTitle { get; set; }
		public DateTime? ShowVolume { get; set; }

		public NextTitleBox NextTitleBox { get; set; }
		public double Volume { get; set; }

		protected override async Task OnParametersSetAsync()
		{
			var titleId = int.Parse(TitleId);
			var title = (await HttpService.TitlesApiAccess.GetSingleWithPlaybackStatus(titleId, UserManagerService.CurrentUser.UserId)).Object;
			if (title != null)
			{
				var at = 0;
				if (Position != null)
				{
					at = int.Parse(Position);
				}
				await StartPlaybackOf(title, at);
			}
		}

		public string GetPoster()
		{
			return $"/api/TitlesApi/ThumbnailPreview?titleId={CurrentPlayback.TitleId}&" +
			       $"fromEnd=false&" +
			       $"position={GetImgMarker.GetStartMarker(CurrentPlayback, BreadcrumbService.Season, BreadcrumbService.Genre) ?? 10}";
		}

		public async Task OnFullscreenRequest()
		{
			await FullscreenComponent.RequestFullscreen();
		}

		public void ContinuePlayback(int position)
		{
			BlazoredVideo.CurrentTime = position;
			LastPlayback = null;
		}

		public async Task OnFullscreenChanged()
		{
			var vid = BlazoredVideo;
			if (vid == null)
			{
				return;
			}
			//Timer.Stop();
			if (await FullscreenComponent.HasFullscreen())
			{
				ShowTitleInCurrentPlayback = true;
				await vid.StartPlayback();
			}
			else
			{
				var fullscreenElement = await FullscreenComponent.GetFullscreenElement();
				if (fullscreenElement != FullscreenComponent.ContainerId)
				{
					await FullscreenComponent.ExitFullscreen();
				}
			}
		}

		public async Task StartPlaybackOf(TitleWithPlayback title, int startFrom = 0)
		{
			await BreadcrumbService.InitWithTitle(title, false);
			if (CurrentPlayback?.TitleId != title.TitleId)
			{
				if (CurrentPlayback != null)
				{
					await HttpService.PlaybackApiAccess.UpdatePlayback(CurrentPlayback.TitleId,
						UserManagerService.CurrentUser.UserId, (int)LastKnownPosition);
					LastKnownPosition = 0;
					await BlazoredVideo.PausePlayback();
					CurrentPlayback = null;
					//base.StateHasChanged();
					//await Task.Yield();
				}

				CurrentPlayback = title;
				LastPlayback = title.Playback?.FirstOrDefault();
				TitleId = CurrentPlayback.TitleId.ToString();
				ShowTitleInCurrentPlayback = true;
				NextTitle = null;
				
				base.StateHasChanged();
				await Task.Yield();
				await GetNextTitle();
				await BlazoredVideo.ReloadControl();
				await BlazoredVideo.SetMediaData(new MediaData()
				{
					Title = CurrentPlayback.Name,
					Album = BreadcrumbService.Season.Name,
					Artist = BreadcrumbService.Genre.Name,
					Artwork = new MediaDataArtwork[]
					{
						new MediaDataArtwork()
						{
							Src = GetPoster(),
							Type = MimeTypes.GetMimeType("jpeg"),
							Sizes = "96x96"
						}, 
					}
				});
			}

			//if (!await FullscreenComponent.HasFullscreen())
			//{
			//	await FullscreenComponent.RequestFullscreen();
			//}

			if (startFrom != 0)
			{
				StateHasChanged();
				await Task.Yield();
				BlazoredVideo.CurrentTime = startFrom;
			}

			await BlazoredVideo.StartPlayback();
		}

		public void Dispose()
		{
			Timer.Stop();
			Timer?.Dispose();
			NextTitleBox?.Dispose();
			FullscreenComponent?.Dispose();
		}

		public void ShowControlsExtChanged(bool status)
		{
			ShowTitleInCurrentPlayback = status;
			if (!ShowTitleInCurrentPlayback)
			{
				LastPlayback = null;
			}
			StateHasChanged();
		}

		public async void OnVolumeChanged(VideoState obj)
		{
			Volume = obj.Volume;
			ShowVolume = DateTime.Now.AddSeconds(5);
			StateHasChanged();
		}

		public async void OnTimeUpdate(VideoState obj)
		{
			LastKnownPosition = obj.CurrentTime;
			if (ShowVolume < DateTime.Now)
			{
				ShowVolume = null;
				StateHasChanged();
			}

			var shouldDisplayAt = (obj.Duration - EvalShowNextTitleHint(CurrentPlayback).TotalSeconds);

			if (ShowNextTitle == false && LastKnownPosition >= shouldDisplayAt && obj.Duration > 0)
			{
				Console.WriteLine("Show Box: LAST" + LastKnownPosition + "|" + shouldDisplayAt);
				await DisplayNextTitleBox();
			}
			else if (ShowNextTitle && LastKnownPosition <= shouldDisplayAt)
			{
				await StopNextTitleBox();
			}
		}

		private async Task StopNextTitleBox()
		{
			ShowNextTitle = false;
			if (NextTitleBox != null)
			{
				if (NextTitleBox.Display)
				{
					await NextTitleBox.Hide();
				}
			}

			StateHasChanged();
		}

		public TimeSpan EvalShowNextTitleHint(Title title)
		{
			//return TimeSpan.FromSeconds(2000);
			if (title.OffsetEnd.HasValue)
			{
				return TimeSpan.FromSeconds(title.OffsetEnd.Value);
			}
			if (BreadcrumbService.Season.OffsetEnd.HasValue)
			{
				return TimeSpan.FromSeconds(BreadcrumbService.Season.OffsetEnd.Value);
			}
			if (BreadcrumbService.Genre.OffsetEnd.HasValue)
			{
				return TimeSpan.FromSeconds(BreadcrumbService.Genre.OffsetEnd.Value);
			}

			return TimeSpan.FromSeconds(10);
		}

		public async void OnPlaybackEnded(VideoState obj)
		{
			if (obj.Duration == obj.CurrentTime)
			{
				await DisplayNextTitleBox();
			}
		}

		private async Task GetNextTitle()
		{
			if (NextTitle != null && CurrentPlayback != null)
			{
				return;
			}
			NextTitle = (await HttpService.TitlesApiAccess.GetNextTitle(CurrentPlayback.TitleId)).Object;
			StateHasChanged();
			await Task.Yield();
		}

		public async Task DisplayNextTitleBox()
		{
			ShowNextTitle = true;
			StateHasChanged();
			await Task.Yield();

			if (NextTitleBox.Display)
			{
				return;
			}

			NextTitleBox.NextTitle = NextTitle;
			NextTitleBox.NextTitlePlayback = async () =>
			{
				await GoToNextTitle();
			};
			NextTitleBox.StartTimer();
			await NextTitleBox.Show();
		}

		public async Task GoToNextTitle()
		{
			if (NextTitle == null)
			{
				await GetNextTitle();
			}

			if (NextTitle == null)
			{
				return;
			}
			NavigationManager.NavigateTo("/Player/" + NextTitle.TitleId);

			//if (NextTitle.IdSeason == CurrentPlayback.IdSeason)
			//{
			//	await StartPlaybackOf(NextTitle);
			//	base.StateHasChanged();
			//	await Task.Yield();
			//	await BlazoredVideo.StartPlayback();
			//}
			//else
			//{
			//	NavigationManager.NavigateTo("/Player/" + NextTitle.TitleId);
			//}

			await StopNextTitleBox();
		}

		public async void OnPlayerPlayed(VideoState obj)
		{
			LastKnownPosition = obj.CurrentTime;
			await UpdatePlayback();
			Timer.Start();
			ShowTitleInCurrentPlayback = true;
		}

		private async Task UpdatePlayback()
		{
			if (LastKnownPosition == LastUpdatePosition || CurrentPlayback == null)
			{
				return;
			}
			//if(CurrentPlayback is )
			//var playback = CurrentPlayback.Playback?.FirstOrDefault();
			//if (playback != null)
			//{
			//	playback.Position = (long)LastKnownPosition;
			//	StateHasChanged();
			//}
			await HttpService.PlaybackApiAccess.UpdatePlayback(CurrentPlayback.TitleId,
				UserManagerService.CurrentUser.UserId, (int)LastKnownPosition);
			LastUpdatePosition = LastKnownPosition;
		}

		public async void OnPlayerPaused(VideoState obj)
		{
			ShowTitleInCurrentPlayback = true;
			LastKnownPosition = obj.CurrentTime;
			await UpdatePlayback();
			Timer.Stop();
			StateHasChanged();

			if (NextTitleBox != null && NextTitleBox.Display)
			{
				NextTitleBox.StopTimer();
			}
		}

		private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			await UpdatePlayback();
		}
	}
}
