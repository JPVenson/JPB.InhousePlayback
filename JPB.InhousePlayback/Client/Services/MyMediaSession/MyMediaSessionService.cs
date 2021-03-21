using System.Threading.Tasks;
using JPB.InhousePlayback.Client.Services.MediaSession;
using Microsoft.JSInterop;

namespace JPB.InhousePlayback.Client.Services.MyMediaSession
{
	public class MyMediaSessionService : MediaSessionService
	{
		public MyMediaSessionService(IJSRuntime jsRuntime) : base(jsRuntime)
		{
		}

		public ValueTask ActivateEventsCore()
		{
			return base.ActivateEvents(new MediaSessionActionType[]
			{
				MediaSessionActionType.Play,
				MediaSessionActionType.Pause,
				MediaSessionActionType.Stop,
				MediaSessionActionType.SeekBackward,
				MediaSessionActionType.SeekForward,
				MediaSessionActionType.SeekTo,
			});
		}

		public override ValueTask ActivateEvents()
		{
			return base.ActivateEvents(new MediaSessionActionType[]
			{
				MediaSessionActionType.Play,
				MediaSessionActionType.Pause,
				MediaSessionActionType.Stop,
				MediaSessionActionType.SeekBackward,
				MediaSessionActionType.SeekForward,
				MediaSessionActionType.SeekTo,
				MediaSessionActionType.PreviousTrack,
				MediaSessionActionType.NextTrack,
			});
		}
	}
}
