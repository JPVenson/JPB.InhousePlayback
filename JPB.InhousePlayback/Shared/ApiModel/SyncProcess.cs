namespace JPB.InhousePlayback.Shared.ApiModel
{
	public class SyncProgress
	{
		public SyncProgress(string genre, string season, string title, int progress, int maxProcess)
		{
			Genre = genre;
			Season = season;
			Title = title;
			Progress = progress;
			MaxProcess = maxProcess;
		}
		public SyncProgress(string genre, string season, int progress, int maxProcess)
			: this(genre, season, null, progress, maxProcess)
		{
		}
		public SyncProgress(string genre, int progress, int maxProcess)
			: this(genre, null, null, progress, maxProcess)
		{
		}

		public string Season { get; set; }
		public string Genre { get; set; }
		public string Title { get; set; }
		public int Progress { get; set; }
		public int MaxProcess { get; set; }
	}
}