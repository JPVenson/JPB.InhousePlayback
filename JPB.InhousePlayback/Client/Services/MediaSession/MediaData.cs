using System.Text.Json.Serialization;

namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	public class MediaData
	{
		[JsonPropertyName("title")]
		public string Title { get; set; }
		[JsonPropertyName("artist")]
		public string Artist { get; set; }
		[JsonPropertyName("album")]
		public string Album { get; set; }
		
		[JsonPropertyName("artwork")]
		public MediaDataArtwork[] Artworks { get; set; }
	}
}
