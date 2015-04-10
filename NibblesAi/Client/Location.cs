using System;

namespace Client
{
    public class Location
    {
        public Int64 X { get; set; }
        public Int64 Y { get; set; }

        public Location(Int64 x, Int64 y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("({0}:{1})", X, Y);
        }

        public Direction GetDirection(Location target)
        {
            if (target.X > X)
                return Direction.Right;
            if (target.X < X)
                return Direction.Left;
            if (target.Y > Y)
                return Direction.Up;
            return Direction.Down;
        }

        public static bool operator ==(Location location1, Location location2)
        {
            if ((object) location1 == null && (object) location2 == null)
                return true;
            if ((object) location1 == null || (object) location2 == null)
                return false;

            return location1.X == location2.X && location1.Y == location2.Y;
        }

        public static bool operator !=(Location location1, Location location2)
        {
            return !(location1 == location2);
        }

        public static Location operator +(Location location1, Location location2)
        {
            return new Location(location1.X + location2.X, location1.Y+ location2.Y);
        }

        protected bool Equals(Location other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Location)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }
}
