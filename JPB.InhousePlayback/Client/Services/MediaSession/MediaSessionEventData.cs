using System.Text.Json.Serialization;

namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	public class MediaSessionEventData
	{
		/// <summary>
		/// A Media Session action type string taken from the MediaSessionActionType enumerated type, indicating which type of action needs to be performed.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaSessionAction</remarks>
		/// </summary>
		[JsonPropertyName("action")]
		public string ActionType { get; set; }

		/// <summary>
		///	An seekto action may optionally include this property, which is a Boolean value indicating whether or not to perform a "fast" seek. A "fast" seek is a seek being performed in a rapid sequence, such as when fast-forwarding or reversing through the media, rapidly skipping through it. This property can be used to indicate that you should use the shortest possible method to seek the media. fastSeek is not included on the final action in the seek sequence in this situation.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaSessionActionDetails/fastSeek</remarks>
		/// </summary>
		public bool SeekFast { get; set; }

		/// <summary>
		/// If the action is either seekforward or seekbackward and this property is present, it is a floating point value which indicates the number of seconds to move the play position forward or backward. If this property isn't present, those actions should choose a reasonable default distance to skip forward or backward (such as 7 or 10 seconds).
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaSessionActionDetails/seekOffset</remarks>
		/// </summary>
		public double? SeekOffset { get; set; }

		/// <summary>
		/// If the action is seekto, this property must be present and must be a floating-point value indicating the absolute time within the media to move the playback position to, where 0 indicates the beginning of the media. This property is not present for other action types.
		/// <remarks>https://developer.mozilla.org/en-US/docs/Web/API/MediaSessionActionDetails/seekTime</remarks>
		/// </summary>
		public double? SeekTime { get; set; }
	}
}