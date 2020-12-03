﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.Video;
using Blazored.Video.Support;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Components.VideoEx
{
	public partial class VideoExComponentBase
	{
		[Inject] ILoggerFactory LoggerFactory { get; set; }
		[Inject] 
		protected IJSRuntime JS { get; set; }

		/// <summary>
		/// Allows you to put the same content inside this component as you would 
		/// do for the html <code>video</code> element
		/// </summary>
		[Parameter] public RenderFragment ChildContent { get; set; }

		/// <summary>
		/// Capture all standard html attributes supplied by the developer
		/// </summary>
		[Parameter(CaptureUnmatchedValues = true)]
		public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

		/// <summary>
		/// Supply a dictionary describing what data should be returned for each event
		/// The default is to return just the event name.
		/// <code>
		/// VideoEventOptions=@(new VideoStateOptions() { CurrentTime = true })
		///	</code>
		/// </summary>
		[Parameter] public Dictionary<VideoEvents, VideoStateOptions> VideoEventOptions { get; set; }

		protected string UniqueKey = Guid.NewGuid().ToString("N");
#pragma warning disable CS0649
#pragma warning disable CS0414
		protected ElementReference videoRef;
		protected bool Configured = false;
#pragma warning restore CS0414
#pragma warning restore CS0649
		private readonly JsonSerializerOptions serializationOptions = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };

		/// <summary>
		/// Configure the events and the data payload requested
		/// TODO: consider if this can/should happen in OnSetParameters() so they can change?
		/// </summary>
		protected override void OnInitialized()
		{
			if (Attributes.TryGetValue("id", out object Id))
			{
				UniqueKey = Id.ToString();
			}
		}
		
		private async Task ConfigureEvents()
		{

			var registerAllEvents = RegisterEventFired;

			if (registerAllEvents || RegisterAbort)
			{
				await Implement(VideoEvents.Abort);
			}
			if (registerAllEvents || RegisterCanPlay)
			{
				await Implement(VideoEvents.CanPlay);
			}
			if (registerAllEvents || RegisterCanPlayThrough)
			{
				await Implement(VideoEvents.CanPlayThrough);
			}
			if (registerAllEvents || RegisterDurationChange)
			{
				await Implement(VideoEvents.DurationChange);
			}
			if (registerAllEvents || RegisterEmptied)
			{
				await Implement(VideoEvents.Emptied);
			}
			if (registerAllEvents || RegisterEnded)
			{
				await Implement(VideoEvents.Ended);
			}
			if (registerAllEvents || RegisterError)
			{
				await Implement(VideoEvents.Error);
			}
			if (registerAllEvents || RegisterLoadedData)
			{
				await Implement(VideoEvents.LoadedData);
			}
			if (registerAllEvents || RegisterLoadedMetadata)
			{
				await Implement(VideoEvents.LoadedMetadata);
			}
			if (registerAllEvents || RegisterLoadStart)
			{
				await Implement(VideoEvents.LoadStart);
			}
			if (registerAllEvents || RegisterPause)
			{
				await Implement(VideoEvents.Pause);
			}
			if (registerAllEvents || RegisterPlay)
			{
				await Implement(VideoEvents.Play);
			}
			if (registerAllEvents || RegisterPlaying)
			{
				await Implement(VideoEvents.Playing);
			}
			if (registerAllEvents || RegisterProgress)
			{
				await Implement(VideoEvents.Progress);
			}
			if (registerAllEvents || RegisterRateChange)
			{
				await Implement(VideoEvents.RateChange);
			}
			if (registerAllEvents || RegisterSeeked)
			{
				await Implement(VideoEvents.Seeked);
			}
			if (registerAllEvents || RegisterSeeking)
			{
				await Implement(VideoEvents.Seeking);
			}
			if (registerAllEvents || RegisterStalled)
			{
				await Implement(VideoEvents.Stalled);
			}
			if (registerAllEvents || RegisterSuspend)
			{
				await Implement(VideoEvents.Suspend);
			}
			if (registerAllEvents || RegisterTimeUpdate)
			{
				await Implement(VideoEvents.TimeUpdate);
			}
			if (registerAllEvents || RegisterVolumeChange)
			{
				await Implement(VideoEvents.VolumeChange);
			}
			if (registerAllEvents || RegisterWaiting)
			{
				await Implement(VideoEvents.Waiting);
			}
		}

		/// <summary>
		/// This is where we generate the markup required to make events work.
		/// TODO: Move the JS code to a script and make a simple call here?
		/// </summary>
		/// <param name="eventName">The DOM event name e.g. play, pause etc</param>
		/// <param name="payloadName">In case someone wants to change it. Don't though.</param>
		/// <returns>A string containing JS code for the event handler</returns>
		async Task Implement(VideoEvents eventName)
		{
			VideoStateOptions options = default;
			VideoEventOptions?.TryGetValue(eventName, out options);
			await JS.InvokeVoidAsync("Blazored.registerCustomEventHandler", videoRef, eventName.ToString().ToLower(), options.GetPayload());
		}

		/// <summary>
		/// Normally a <pre>video</pre> element doesn't have a <pre>change</pre> event.
		/// We force one and a use it as a proxy for all the Media Events.
		/// </summary>
		/// <param name="args">The event args - Value contains our JSON</param>
		protected virtual void OnChange(ChangeEventArgs args)
		{
			var ThisEvent = args?.Value?.ToString();
			VideoEventData videoData = new VideoEventData();
			try
			{
				videoData = JsonSerializer.Deserialize<VideoEventData>(ThisEvent, serializationOptions);
			}
			catch (Exception ex)
			{
				LoggerFactory
					.CreateLogger(nameof(VideoExComponentBase))
					.LogError(ex, "Failed to convert the JSON: {0}", ThisEvent);
			}

			switch (videoData.EventName)
			{
				case VideoEvents.Abort:
					Abort?.Invoke(videoData.State);
					AbortEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.CanPlay:
					CanPlay?.Invoke(videoData.State);
					CanPlayEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.CanPlayThrough:
					CanPlayThrough?.Invoke(videoData.State);
					CanPlayThroughEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.DurationChange:
					DurationChange?.Invoke(videoData.State);
					DurationChangeEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Emptied:
					Emptied?.Invoke(videoData.State);
					EmptiedEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Ended:
					Ended?.Invoke(videoData.State);
					EndedEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Error:
					Error?.Invoke(videoData.State);
					ErrorEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.LoadedData:
					LoadedData?.Invoke(videoData.State);
					LoadedDataEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.LoadedMetadata:
					LoadedMetadata?.Invoke(videoData.State);
					LoadedMetadataEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.LoadStart:
					LoadStart?.Invoke(videoData.State);
					LoadStartEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Pause:
					Pause?.Invoke(videoData.State);
					PauseEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Play:
					Play?.Invoke(videoData.State);
					PlayEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Playing:
					Playing?.Invoke(videoData.State);
					PlayingEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Progress:
					Progress?.Invoke(videoData.State);
					ProgressEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.RateChange:
					RateChange?.Invoke(videoData.State);
					RateChangeEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Seeked:
					Seeking?.Invoke(videoData.State);
					SeekingEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Seeking:
					Seeking?.Invoke(videoData.State);
					SeekingEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Stalled:
					Stalled?.Invoke(videoData.State);
					StalledEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Suspend:
					Suspend?.Invoke(videoData.State);
					SuspendEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.TimeUpdate:
					TimeUpdate?.Invoke(videoData.State);
					TimeUpdateEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.VolumeChange:
					VolumeChange?.Invoke(videoData.State);
					VolumeChangeEvent.InvokeAsync(videoData.State);
					break;
				case VideoEvents.Waiting:
					Waiting?.Invoke(videoData.State);
					WaitingEvent.InvokeAsync(videoData.State);
					break;
				default:
					break;
			}
			// Here is our catch-all event handler call!
			EventFired?.Invoke(videoData);
		}
		bool RegisterEventFired => EventFiredEventRequired || EventFiredRequired;
		bool RegisterAbort => AbortEventRequired || AbortRequired;
		bool RegisterCanPlay => CanPlayEventRequired || CanPlayRequired;
		bool RegisterCanPlayThrough => CanPlayThroughEventRequired || CanPlayThroughRequired;
		bool RegisterDurationChange => DurationChangeEventRequired || DurationChangeRequired;
		bool RegisterEmptied => EmptiedEventRequired || EmptiedRequired;
		bool RegisterEnded => EndedEventRequired || EndedRequired;
		bool RegisterError => ErrorEventRequired || ErrorRequired;
		bool RegisterLoadedData => LoadedDataEventRequired || LoadedDataRequired;
		bool RegisterLoadedMetadata => LoadedMetadataEventRequired || LoadedMetadataRequired;
		bool RegisterLoadStart => LoadStartEventRequired || LoadStartRequired;
		bool RegisterPause => PauseEventRequired || PauseRequired;
		bool RegisterPlay => PlayEventRequired || PlayRequired;
		bool RegisterPlaying => PlayingEventRequired || PlayingRequired;
		bool RegisterProgress => ProgressEventRequired || ProgressRequired;
		bool RegisterRateChange => RateChangeEventRequired || RateChangeRequired;
		bool RegisterSeeked => SeekedEventRequired || SeekedRequired;
		bool RegisterSeeking => SeekingEventRequired || SeekingRequired;
		bool RegisterStalled => StalledEventRequired || StalledRequired;
		bool RegisterSuspend => SuspendEventRequired || SuspendRequired;
		bool RegisterTimeUpdate => TimeUpdateEventRequired || TimeUpdateRequired;
		bool RegisterVolumeChange => VolumeChangeEventRequired || VolumeChangeRequired;
		bool RegisterWaiting => WaitingEventRequired || WaitingRequired;
	}
}