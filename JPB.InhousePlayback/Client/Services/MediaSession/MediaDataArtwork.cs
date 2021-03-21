using System.Text.Json.Serialization;

namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	public class MediaDataArtwork
	{
		[JsonPropertyName("src")]
		public string Source { get; set; }
		[JsonPropertyName("sizes")]
		public string Sizes { get; set; }
		[JsonPropertyName("type")]
		public string Type { get; set; }
	}
}