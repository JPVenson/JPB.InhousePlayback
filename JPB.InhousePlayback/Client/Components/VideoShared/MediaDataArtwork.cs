using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JPB.InhousePlayback.Client.Components.VideoShared
{
	public class MediaDataArtwork
	{
		[JsonPropertyName("src")]
		public string Src { get; set; }
		[JsonPropertyName("sizes")]
		public string Sizes { get; set; }
		[JsonPropertyName("type")]
		public string Type { get; set; }
	}
}
