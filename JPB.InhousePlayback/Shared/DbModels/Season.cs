using System;

namespace JPB.InhousePlayback.Shared.DbModels
{
	public class Season : IEquatable<Season>
	{
		public int SeasonId { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public int OrderNo { get; set; }
		
		public Nullable<Int32> PlaybackLength  { get; set; }
		public Nullable<Int32> OffsetStart  { get; set; }
		public Nullable<Int32> OffsetEnd  { get; set; }
		public int IdGenre { get; set; }

		public bool Equals(Season other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return SeasonId == other.SeasonId && Name == other.Name && Location == other.Location && OrderNo == other.OrderNo && PlaybackLength == other.PlaybackLength && OffsetStart == other.OffsetStart && OffsetEnd == other.OffsetEnd && IdGenre == other.IdGenre;
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

			return Equals((Season) obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(SeasonId, Name, Location, OrderNo, PlaybackLength, OffsetStart, OffsetEnd, IdGenre);
		}
	}

	public class SeasonWithGenre : Season
	{
		public Genre Genre { get; set; }
	}
}