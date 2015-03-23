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
    }
}
