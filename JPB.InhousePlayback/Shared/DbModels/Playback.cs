namespace JPB.InhousePlayback.Shared.DbModels
{
	public class Playback
	{
		public int PlaybackId  { get; set; }
		public int IdTitle  { get; set; }
		public int IdUser  { get; set; }
		public int Position  { get; set; }
	}

	public class PlaybackWithTitle : Playback
	{
		public TitleWithSeason Title { get; set; }
	}
}