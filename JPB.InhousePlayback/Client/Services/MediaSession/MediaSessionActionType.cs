using System;

namespace JPB.InhousePlayback.Client.Services.MediaSession
{
	[Flags]
	public enum MediaSessionActionType
	{
		Play,
		Pause,
		Stop,
		SeekBackward,
		SeekForward,
		SeekTo,
		PreviousTrack,
		NextTrack,
		SkipAd
	}
}
