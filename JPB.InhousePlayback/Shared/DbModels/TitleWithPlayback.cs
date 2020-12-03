namespace JPB.InhousePlayback.Shared.DbModels
{
	public class TitleWithPlayback : Title
	{
		public Playback[] Playback { get; set; }
	}
}