namespace Client
{
    public class PathfindingState
    {
        public PathfindingState Parent { get; set; }
        public Location Location { get; set; }
        public long FCost { get; set; }
        public long MovementCostFromStart { get; set; }
        public long EstimatedMovementCostToTarget { get; set; }
        public ListStatus Status { get; set; }

        public PathfindingState(long x, long y)
        {
            Location = new Location(x, y);
            Status = ListStatus.None;
        }
    }
}
