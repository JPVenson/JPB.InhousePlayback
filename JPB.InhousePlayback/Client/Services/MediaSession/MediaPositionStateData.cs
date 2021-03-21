namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	public class MediaPositionStateData
	{
		/// <summary>
		///		A floating-point value giving the total duration of the current media in seconds. This should always be a positive number, with positive infinity (Infinity) indicating media without a defined end, such as a live stream.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaPositionState/duration</remarks>
		/// </summary>
		public double Duration { get; set; }
		/// <summary>
		///		A floating-point value indicating the rate at which the media is being played, as a ratio relative to its normal playback speed. Thus, a value of 1 is playing at normal speed, 2 is playing at double speed, and so forth. Negative values indicate that the media is playing in reverse; -1 indicates playback at the normal speed but backward, -2 is double speed in reverse, and so on.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaPositionState/playbackRate</remarks>
		/// </summary>
		public double PlaybackRate { get; set; }
		/// <summary>
		///		A floating-point value indicating the last reported playback position of the media in seconds. This must always be a positive value.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaPositionState/position</remarks>
		/// </summary>
		public double Position { get; set; }
	}
}