using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.InhousePlayback.Client.Util
{
	public static class TimespanHumanizer
	{
		public static string Humanize(TimeSpan timespan)
		{
			if (timespan.TotalHours > 1)
			{
				return $"{(int) timespan.TotalHours}:{timespan.Minutes}:{timespan.Seconds}";
			}
			if (timespan.TotalMinutes > 1)
			{
				return $"{(int)timespan.TotalMinutes}:{timespan.Seconds}";
			}

			return timespan.TotalSeconds.ToString();
		}
	}
}
