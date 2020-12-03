using System;

namespace JPB.InhousePlayback.Shared.DbModels
{
	public class Genre : IEquatable<Genre>
	{
		public int GenreId { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public Nullable<Int32> PlaybackLength  { get; set; }
		public Nullable<Int32> OffsetStart  { get; set; }
		public Nullable<Int32> OffsetEnd  { get; set; }

		public bool Equals(Genre other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return GenreId == other.GenreId && Name == other.Name && Location == other.Location && PlaybackLength == other.PlaybackLength && OffsetStart == other.OffsetStart && OffsetEnd == other.OffsetEnd;
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

			return Equals((Genre) obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(GenreId, Name, Location, PlaybackLength, OffsetStart, OffsetEnd);
		}
	}
}