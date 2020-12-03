using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JPB.InhousePlayback.Shared.DbModels;

namespace JPB.InhousePlayback.Client.Util
{
	public static class GetImgMarker
	{
		public static int? GetEndMarker(Title title, Season season, Genre genre)
		{
			if (title?.OffsetEnd != null)
			{
				return title.OffsetEnd.Value;
			}
			if (season?.OffsetEnd != null)
			{
				return season.OffsetEnd.Value;
			}
			if (genre?.OffsetEnd != null)
			{
				return genre.OffsetEnd.Value;
			}
			return null;
		}
		public static int? GetStartMarker(Title title, Season season, Genre genre)
		{
			if (title?.OffsetStart != null)
			{
				return title.OffsetStart.Value;
			}
			if (season?.OffsetStart != null)
			{
				return season.OffsetStart.Value;
			}
			if (genre?.OffsetStart != null)
			{
				return genre.OffsetStart.Value;
			}
			return null;
		}
	}
}
