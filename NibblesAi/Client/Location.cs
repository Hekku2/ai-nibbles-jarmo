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
    }
}
