using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	public static class MediaSessionServiceExtensions
	{
		/// <summary>
		///		Adds the MediaSessionService to the service collection
		/// </summary>
		/// <param name="serviceCollection"></param>
		/// <returns></returns>
		public static IServiceCollection AddMediaSessionService(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<MediaSessionService>();
			return serviceCollection;
		}
	}

	/// <summary>
	///		service for using the [experimental] Media Session Api of an browser
	/// </summary>
	public class MediaSessionService : IDisposable, IAsyncDisposable
	{
		private readonly IJSRuntime _jsRuntime;
		private readonly DotNetObjectReference<MediaSessionService> _that;

		public MediaSessionService(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
			_that = DotNetObjectReference.Create(this);
			ServiceKey = Guid.NewGuid().ToString("N");
		}

		private bool? _isSupported;

		protected string ServiceKey { get; set; }

		/// <summary>
		///		Returns ether true if the MediaSession is supported in this browser or false if not
		/// </summary>
		/// <returns></returns>
		public async ValueTask<bool> IsSupported()
		{
			if (_isSupported.HasValue)
			{
				return _isSupported.Value;
			}

			_isSupported = await _jsRuntime.InvokeAsync<bool>("BlazoredMediaData.isSupported");
			return _isSupported.Value;
		}

		/// <summary>
		///		Sets the current MediaData
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public async ValueTask SetMediaMetaData(MediaData data)
		{
			await _jsRuntime.InvokeVoidAsync("BlazoredMediaData.setMediaData", data);
		}

		/// <summary>
		///		Gets the current MediaData
		/// </summary>
		/// <returns></returns>
		public async ValueTask<MediaData> GetMediaMetaData()
		{
			return await _jsRuntime.InvokeAsync<MediaData>("BlazoredMediaData.getMediaData");
		}

		/// <summary>
		///		Sets the current playback position and speed of the media currently being presented.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public async ValueTask SetMediaPositionData(MediaPositionStateData data)
		{
			await _jsRuntime.InvokeVoidAsync("BlazoredMediaData.setMediaPositionData", data);
		}
		
		/// <summary>
		///		Indicates whether the current media session is playing. Valid values are none, paused, or playing.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public async ValueTask SetMediaPlaybackState(MediaSessionPlaybackState data)
		{
			await _jsRuntime.InvokeVoidAsync("BlazoredMediaData.setMediaPlaybackState", data.ToString());
		}

		/// <summary>
		///		Indicates whether the current media session is playing. Valid values are none, paused, or playing.
		/// </summary>
		/// <returns></returns>
		public async ValueTask<MediaSessionPlaybackState> GetMediaPlaybackState()
		{
			return await _jsRuntime.InvokeAsync<MediaSessionPlaybackState>("BlazoredMediaData.getMediaPlaybackState");
		}

		/// <summary>
		///		Will be triggered if any registered event was called
		/// </summary>
		public event EventHandler<MediaSessionEventData> AnyEvent;

		/// <summary>
		///		Begins (or resumes) playback of the media.
		/// </summary>
		public event EventHandler<MediaSessionEventData> Play;
		/// <summary>
		///		Pauses playback of the media.
		/// </summary>
		public event EventHandler<MediaSessionEventData> Pause;
		/// <summary>
		///		Halts playback entirely.
		/// </summary>
		public event EventHandler<MediaSessionEventData> Stop;
		/// <summary>
		///		Seeks backward through the media from the current position. The <seealso cref="MediaSessionEventData.SeekOffset"/> property specifies the amount of time to seek backward.
		/// </summary>
		public event EventHandler<MediaSessionEventData> SeekBackward;
		/// <summary>
		/// Seeks forward from the current position through the media. The <seealso cref="MediaSessionEventData.SeekOffset"/> property specifies the amount of time to seek forward.
		/// </summary>
		public event EventHandler<MediaSessionEventData> SeekForward;
		/// <summary>
		///		Moves the playback position to the specified time within the media. The time to which to seek is specified in the <seealso cref="MediaSessionEventData.SeekTime"/>. If you intend to perform multiple seekto operations in rapid succession, you can also specify the <seealso cref="MediaSessionEventData.SeekFast"/> property with a value of true. This lets the browser know it can take steps to optimize repeated operations, and is likely to result in improved performance.
		/// </summary>
		public event EventHandler<MediaSessionEventData> SeekTo;
		/// <summary>
		///		Moves back to the previous track.
		/// </summary>
		public event EventHandler<MediaSessionEventData> PreviousTrack;
		/// <summary>
		///		Advances playback to the next track.
		/// </summary>
		public event EventHandler<MediaSessionEventData> NextTrack;
		/// <summary>
		///		Skips past the currently playing advertisement or commercial. This action may or may not be available, depending on the platform and user agent, or may be disabled due to subscription level or other circumstances.
		/// </summary>
		public event EventHandler<MediaSessionEventData> SkipAd;

		[JSInvokable("BlazoredMediaData_eventCallback")]
		public virtual void ActionCallback(MediaSessionEventData eventData)
		{
			Console.WriteLine("MS_" + eventData.ActionType);
			if (string.Equals(eventData.ActionType, "Play", StringComparison.InvariantCultureIgnoreCase))
			{
				Play?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "Pause", StringComparison.InvariantCultureIgnoreCase))
			{
				Pause?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "Stop", StringComparison.InvariantCultureIgnoreCase))
			{
				Stop?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "SeekBackward", StringComparison.InvariantCultureIgnoreCase))
			{
				SeekBackward?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "SeekForward", StringComparison.InvariantCultureIgnoreCase))
			{
				SeekForward?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "SeekTo", StringComparison.InvariantCultureIgnoreCase))
			{
				SeekTo?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "PreviousTrack", StringComparison.InvariantCultureIgnoreCase))
			{
				PreviousTrack?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "NextTrack", StringComparison.InvariantCultureIgnoreCase))
			{
				NextTrack?.Invoke(this, eventData);
			}
			else if (string.Equals(eventData.ActionType, "SkipAd", StringComparison.InvariantCultureIgnoreCase))
			{
				SkipAd?.Invoke(this, eventData);
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}

			AnyEvent?.Invoke(this, eventData);
		}

		/// <summary>
		///		Registers this instance to be receiving the given events
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		protected virtual async ValueTask ActivateEvents(MediaSessionActionType[] types)
		{
			await _jsRuntime.InvokeVoidAsync("BlazoredMediaData.setActionHandler",
				_that,
				ServiceKey,
				types.Select(f => f.ToString().ToLower()).ToArray()
			);
		}

		/// <summary>
		///		Registers this instance to receive all possible media events
		/// </summary>
		/// <returns></returns>
		public virtual async ValueTask ActivateEvents()
		{
			await ActivateEvents(new[]
			{
				MediaSessionActionType.Play,
				MediaSessionActionType.Pause,
				MediaSessionActionType.Stop,
				MediaSessionActionType.SeekBackward,
				MediaSessionActionType.SeekForward,
				MediaSessionActionType.SeekTo,
				MediaSessionActionType.PreviousTrack,
				MediaSessionActionType.NextTrack,
				MediaSessionActionType.SkipAd
			});
		}

		/// <summary>
		///		Unregisters this instance to be receiving the given events
		/// </summary>
		/// <param name="types"></param>
		/// <returns></returns>
		protected virtual async ValueTask DeactivateEvents(MediaSessionActionType[] types)
		{
			await _jsRuntime.InvokeVoidAsync("BlazoredMediaData.removeActionHandler",
				ServiceKey,
				types.Select(f => f.ToString().ToLower()).ToArray()
			);
		}
		
		/// <summary>
		///		Unregisters all events from this instance
		/// </summary>
		/// <returns></returns>
		public virtual async ValueTask DeactivateEvents()
		{
			var types = new[]
			{
				MediaSessionActionType.Play,
				MediaSessionActionType.Pause,
				MediaSessionActionType.Stop,
				MediaSessionActionType.SeekBackward,
				MediaSessionActionType.SeekForward,
				MediaSessionActionType.SeekTo,
				MediaSessionActionType.PreviousTrack,
				MediaSessionActionType.NextTrack,
				MediaSessionActionType.SkipAd
			};
			await DeactivateEvents(types);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			DisposeAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync()
		{
			if (await IsSupported())
			{
				await DeactivateEvents();
			}
			_that.Dispose();
		}
	}
}