using System;

namespace JPB.InhousePlayback.Shared.DbModels
{
	public class Title : IEquatable<Title>
	{
		public int TitleId { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public int IdSeason { get; set; }

		public Nullable<Int32> OrderNo { get; set; }
		public Nullable<Int32> PlaybackLength { get; set; }
		public Nullable<Int32> OffsetStart { get; set; }
		public Nullable<Int32> OffsetEnd { get; set; }

		public bool Equals(Title other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return TitleId == other.TitleId && Name == other.Name && Location == other.Location && IdSeason == other.IdSeason && OrderNo == other.OrderNo && PlaybackLength == other.PlaybackLength && OffsetStart == other.OffsetStart && OffsetEnd == other.OffsetEnd;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((Title)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(TitleId, Name, Location, IdSeason, OrderNo, PlaybackLength, OffsetStart, OffsetEnd);
		}
	}

	public class TitleWithSeason : Title
	{
		public SeasonWithGenre Season { get; set; }
	}
}