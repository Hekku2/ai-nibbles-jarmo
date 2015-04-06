namespace Client
{
    public enum Direction
    {
        Up = 2,
        Down = 1,
        Left = 3,
        Right = 4
    }

    public static class DirectionExtensions
    {
        public static Location LocationDelta(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Location(0, 1);
                case Direction.Down:
                    return new Location(0, -1);
                case Direction.Right:
                    return  new Location(1, 0);
                case Direction.Left:
                    return new Location(-1, 0);
            }
            return new Location(0, 0);
        }
    }
}
