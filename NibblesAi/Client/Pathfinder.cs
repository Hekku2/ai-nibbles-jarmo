using System;
using Functional.Maybe;

namespace Client
{
    /// <summary>
    /// Path finder implementation in C#, modified from http://www.policyalmanac.org/games/aStarTutorial.htm
    /// </summary>
    public class Pathfinder
    {
        

        private readonly Int64 _mapWidth;
        private readonly Int64 _mapHeight;

        private const int NotStarted = 0;
        private const int Found = 1;

        private PathfindingBinaryHeap _heap;

        private readonly PathfindingState[,] _locations; 
        private int _pathLength;
        private const int NonDiagonalMovementCost = 10;
        private const int DiagonalMovementCost = 14;

        public Pathfinder(Int64 mapWidth, Int64 mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _locations = InitializeLocations(_mapWidth + 1, mapHeight + 1);
            _heap = new PathfindingBinaryHeap(_mapWidth * _mapHeight);
        }

        private static PathfindingState[,] InitializeLocations(long width, long height)
        {
            var locations = new PathfindingState[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    locations[x, y] = new PathfindingState(x, y);
                }
            }
            return locations;
        }

        public Maybe<Direction> FindPath(Location startPoint, Location target, Location[] blockedLocations)
        {
            _heap.Clear();
            if (startPoint.X == target.X && startPoint.Y == target.Y || !IsInGamefield(target.X, target.Y))
                return Maybe<Direction>.Nothing;

            ResetWhichList();

            _pathLength = NotStarted;
            _locations[startPoint.X, startPoint.Y].MovementCostFromStart = 0;

            //4.Add the starting location to the open list of squares to be checked.
            _heap.Add(_locations[startPoint.X, startPoint.Y]);

            //5.Do the following until a path is found or deemed nonexistent.
            int path;
            do
            {
                //6.If the open list is not empty, take the first cell off of the list. This is the lowest F cost cell on the open list.
                if (_heap.HasItems())
                {
                    //7. Pop the first item off the open list.
                    var parent = _heap.Pop();
                    parent.Status = ListStatus.Closed;

                    // 7.Check the adjacent squares. Add these adjacent child squares to the open list
                    //	for later consideration if appropriate
                    for (var yCoordinate = parent.Location.Y - 1; yCoordinate <= parent.Location.Y + 1; yCoordinate++)
                    {
                        for (var xCoordinate = parent.Location.X - 1; xCoordinate <= parent.Location.X + 1; xCoordinate++)
                        {
                            //	If not off the map (do this first to avoid array out-of-bounds errors)
                            if (!IsInGamefield(xCoordinate, yCoordinate) || _locations[xCoordinate, yCoordinate].Status == ListStatus.Closed)
                                continue;

                            if (!CanBeTraversed(blockedLocations, xCoordinate, yCoordinate) || !IsCornerWalkable(blockedLocations, xCoordinate, parent.Location.X, yCoordinate, parent.Location.Y))
                                continue;

                            //	If not already on the open list, add it to the open list.			
                            if (_locations[xCoordinate, yCoordinate].Status != ListStatus.Open)
                            {
                                //Figure out its H and F costs and parent
                                var newItem = _locations[xCoordinate, yCoordinate];
                                newItem.MovementCostFromStart = newItem.MovementCostFromStart + CalculateAddedCost(xCoordinate, parent.Location.X, yCoordinate, parent.Location.Y);
                                newItem.EstimatedMovementCostToTarget = 10 * (Math.Abs(xCoordinate - target.X) + Math.Abs(yCoordinate - target.Y));
                                newItem.FCost = newItem.MovementCostFromStart + newItem.EstimatedMovementCostToTarget;
                                newItem.Parent = parent;
                                newItem.Status = ListStatus.Open;
                                _heap.Add(newItem);
                            }
                            else
                            {
                                var calculatedCostFromStart = parent.MovementCostFromStart + CalculateAddedCost(xCoordinate, parent.Location.X, yCoordinate, parent.Location.Y);
                                if (calculatedCostFromStart >= _locations[xCoordinate, yCoordinate].MovementCostFromStart)
                                    continue;

                                var considered = _locations[xCoordinate, yCoordinate];
                                considered.Parent = parent;
                                considered.MovementCostFromStart = calculatedCostFromStart;
                                _heap.CalculateNewFCostAndSort(xCoordinate, yCoordinate, considered.MovementCostFromStart);
                            }
                        }
                    }
                }
                else
                {
                    //9.If open list is empty then there is no path.
                    return Maybe<Direction>.Nothing;
                }

                //If target is added to open list then path has been found.
                if (_locations[target.X, target.Y].Status == ListStatus.Open)
                {
                    path = Found;
                    break;
                }

            }
            while (true);//Do until path is found or deemed nonexistent

            if (path != Found) return Maybe<Direction>.Nothing;

            //  a.Working backwards from the target to the starting location by checking
            //	each cell's parent, figure out the length of the path.
            var pathX = target.X;
            var pathY = target.Y;
            do
            {
                //Look up the parent of the current cell.
                var parent = _locations[pathX, pathY].Parent;
                pathX = parent.Location.X;
                pathY = parent.Location.Y;

                //Figure out the path length
                _pathLength++;
            }
            while (pathX != startPoint.X || pathY != startPoint.Y);

            return startPoint.GetDirection(GetFirstStepOfPath(startPoint, target)).ToMaybe();
        }

        private Location GetFirstStepOfPath(Location startPoint, Location target)
        {
            var pathX = target.X;
            var pathY = target.Y;

            var cellPosition = _pathLength * 2;
            Location final;
            do
            {
                cellPosition = cellPosition - 2;
                final = new Location(pathX, pathY);

                var parent = _locations[pathX, pathY].Parent;
                pathX = parent.Location.X;
                pathY = parent.Location.Y;
            }
            while (pathX != startPoint.X || pathY != startPoint.Y);

            return final;
        }

        private bool IsCornerWalkable(Location[] blockedLocations, long xCoordinate, long parentXval, long yCoordinate, long parentYval)
        {
            if (xCoordinate == parentXval - 1)
            {
                if (yCoordinate == parentYval - 1)
                {
                    if (!CanBeTraversed(blockedLocations, parentXval - 1, parentYval) ||
                        !CanBeTraversed(blockedLocations, parentXval, parentYval - 1))
                        return false;
                }
                else if (yCoordinate == parentYval + 1)
                {
                    if (!CanBeTraversed(blockedLocations, parentXval, parentYval + 1) ||
                        !CanBeTraversed(blockedLocations, parentXval - 1, parentYval))
                        return false;
                }
            }
            else if (xCoordinate == parentXval + 1)
            {
                if (yCoordinate == parentYval - 1)
                {
                    if (!CanBeTraversed(blockedLocations, parentXval, parentYval - 1) ||
                        !CanBeTraversed(blockedLocations, parentXval + 1, parentYval))
                        return false;
                }
                else if (yCoordinate == parentYval + 1)
                {
                    if (!CanBeTraversed(blockedLocations, parentXval + 1, parentYval) ||
                        !CanBeTraversed(blockedLocations, parentXval, parentYval + 1))
                        return false;
                }
            }
            return true;
        }



        private void ResetWhichList()
        {
            for (var x = 0; x < _mapWidth; x++)
                for (var y = 0; y < _mapHeight; y++)
                    _locations[x, y].Status = ListStatus.None;
        }

        private static int CalculateAddedCost(long xCoordinate, long parentXval, long yCoordinate, long parentYval)
        {
            if (Math.Abs(xCoordinate - parentXval) == 1 && Math.Abs(yCoordinate - parentYval) == 1)
                return DiagonalMovementCost;	
            return NonDiagonalMovementCost;
        }

        private bool CanBeTraversed(Location[] blockedLocations, long x, long y)
        {
            foreach (var location in blockedLocations)
            {
                if (location.X == x && location.Y == y)
                    return false;
            }
            return true;
        }

        private bool IsInGamefield(long x, long y)
        {
            return x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight;
        }
    }
}
