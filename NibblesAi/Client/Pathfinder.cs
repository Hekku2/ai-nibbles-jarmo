using System;
using Functional.Maybe;

namespace Client
{
    /// <summary>
    /// Path finder implementation in C#, modified from http://www.policyalmanac.org/games/aStarTutorial.htm
    /// </summary>
    public class Pathfinder
    {
        private const int BinaryHeapTopIndex = 1;

        private readonly Int64 _mapWidth;
        private readonly Int64 _mapHeight;

        private const int NotStarted = 0;
        private const int Found = 1;

        private readonly int[] _binaryHeapIdsOfOpenListItems; //1 dimensional array holding ID# of open list items;
        private readonly PathfindingState[] _open;

        private readonly PathfindingState[,] _locations; 
        private int _pathLength;
        private int _numberOfOpenListItems;
        private const int NonDiagonalMovementCost = 10;
        private const int DiagonalMovementCost = 14;

        public Pathfinder(Int64 mapWidth, Int64 mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _binaryHeapIdsOfOpenListItems = new int[_mapWidth * _mapHeight + 2];
            _open = new PathfindingState[_mapWidth * _mapHeight + 2];
            _locations = InitializeLocations(_mapWidth + 1, mapHeight + 1);
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
            if (startPoint.X == target.X && startPoint.Y == target.Y || !IsInGamefield(target.X, target.Y))
                return Maybe<Direction>.Nothing;

            ResetWhichList();

            _pathLength = NotStarted;
            _locations[startPoint.X, startPoint.Y].MovementCostFromStart = 0;
            var newOpenListItemId = 0;

            //4.Add the starting location to the open list of squares to be checked.
            var numberOfOpenListItems = 1;
            _binaryHeapIdsOfOpenListItems[BinaryHeapTopIndex] = 1;//assign it as the top (and currently only) item in the open list, which is maintained as a binary heap (explained below)
            _open[1] = _locations[startPoint.X, startPoint.Y];

            //5.Do the following until a path is found or deemed nonexistent.
            int path;
            do
            {
                //6.If the open list is not empty, take the first cell off of the list. This is the lowest F cost cell on the open list.
                if (numberOfOpenListItems != 0)
                {
                    //7. Pop the first item off the open list.
                    var parent = _open[_binaryHeapIdsOfOpenListItems[1]];
                    parent.Status = ListStatus.Closed;
                    numberOfOpenListItems = numberOfOpenListItems - 1;
                    OrderBinaryHeap(numberOfOpenListItems+1);

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
                                //Create a new open list item in the binary heap.
                                newOpenListItemId = newOpenListItemId + 1; //each new item has a unique ID #
                                var i = numberOfOpenListItems + 1;
                                _binaryHeapIdsOfOpenListItems[i] = newOpenListItemId;//place the new open list item (actually, its ID#) at the bottom of the heap
                                _open[newOpenListItemId] = _locations[xCoordinate, yCoordinate];

                                _locations[xCoordinate, yCoordinate].MovementCostFromStart = _open[newOpenListItemId].MovementCostFromStart + CalculateAddedCost(xCoordinate, parent.Location.X, yCoordinate, parent.Location.Y);

                                //Figure out its H and F costs and parent
                                _open[_binaryHeapIdsOfOpenListItems[i]].EstimatedMovementCostToTarget = 10 * (Math.Abs(xCoordinate - target.X) + Math.Abs(yCoordinate - target.Y));
                                _open[_binaryHeapIdsOfOpenListItems[i]].FCost = _locations[xCoordinate, yCoordinate].MovementCostFromStart + _open[_binaryHeapIdsOfOpenListItems[i]].EstimatedMovementCostToTarget;

                                _open[newOpenListItemId].Parent = parent;

                                //Move the new open list item to the proper place in the binary heap.
                                //Starting at the bottom, successively compare to parent items,
                                //swapping as needed until the item finds its place in the heap
                                //or bubbles all the way to the top (if it has the lowest F cost).
                                while (i != BinaryHeapTopIndex) //While item hasn't bubbled to the top (m=1)	
                                {
                                    //Check if child's F cost is < parent's F cost. If so, swap them.	
                                    if (_open[_binaryHeapIdsOfOpenListItems[i]].FCost <= _open[_binaryHeapIdsOfOpenListItems[i / 2]].FCost)
                                    {
                                        var temp = _binaryHeapIdsOfOpenListItems[i / 2];
                                        _binaryHeapIdsOfOpenListItems[i / 2] = _binaryHeapIdsOfOpenListItems[i];
                                        _binaryHeapIdsOfOpenListItems[i] = temp;
                                        i = i / 2;
                                    }
                                    else
                                        break;
                                }
                                _numberOfOpenListItems = numberOfOpenListItems = numberOfOpenListItems + 1;

                                //Change whichList to show that the new item is on the open list.
                                _locations[xCoordinate, yCoordinate].Status = ListStatus.Open;
                            }
                            else
                            {
                                
                                var calculatedCostFromStart = parent.MovementCostFromStart + CalculateAddedCost(xCoordinate, parent.Location.X, yCoordinate, parent.Location.Y);
                                if (calculatedCostFromStart >= _locations[xCoordinate, yCoordinate].MovementCostFromStart)
                                    continue;

                                var considered = _locations[xCoordinate, yCoordinate];
                                considered.Parent = parent;
                                considered.MovementCostFromStart = calculatedCostFromStart;	

                                var index = FindIndexInOpenList(xCoordinate, yCoordinate);
                                if (!index.HasValue)
                                    continue;

                                _open[_binaryHeapIdsOfOpenListItems[index.Value]].FCost = considered.MovementCostFromStart + _open[_binaryHeapIdsOfOpenListItems[index.Value]].EstimatedMovementCostToTarget;

                                //See if changing the F score bubbles the item up from it's current location in the heap
                                SortWithNewFCost(index.Value);
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

        private void SortWithNewFCost(int indexOfChangedItem)
        {
            var currentIndex = indexOfChangedItem;
            while (currentIndex != 1) //While item hasn't bubbled to the top (m=1)	
            {
                //Check if child is < parent. If so, swap them.	

                if (_open[_binaryHeapIdsOfOpenListItems[currentIndex]].FCost < _open[_binaryHeapIdsOfOpenListItems[currentIndex / 2]].FCost)
                {
                    var temp = _binaryHeapIdsOfOpenListItems[currentIndex / 2];
                    _binaryHeapIdsOfOpenListItems[currentIndex / 2] = _binaryHeapIdsOfOpenListItems[currentIndex];
                    _binaryHeapIdsOfOpenListItems[currentIndex] = temp;
                    currentIndex = currentIndex / 2;
                }
                else
                    break;
            }
        }

        private Maybe<int> FindIndexInOpenList(long x, long y)
        {
            for (var i = BinaryHeapTopIndex; i <= _numberOfOpenListItems; i++) //look for the item in the heap
            {
                var location = _open[_binaryHeapIdsOfOpenListItems[i]];
                if (location.Location.X != x || location.Location.Y != y)
                    continue;

                return i.ToMaybe();
            }
            return Maybe<int>.Nothing;
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

        private void OrderBinaryHeap(int newItemIndex)
        {
            var orderedItemsLastIndex = newItemIndex - 1;
            _binaryHeapIdsOfOpenListItems[1] = _binaryHeapIdsOfOpenListItems[newItemIndex];

            //	Repeat the following until the new item in slot #1 sinks to its proper spot in the heap.
            var v = 1;
            do
            {
                var u = v;
                if (2 * u + 1 <= orderedItemsLastIndex) //if both children exist
                {
                    //Check if the F cost of the parent is greater than each child. 2 * u + 1
                    //Select the lowest of the two children.

                    if (_open[_binaryHeapIdsOfOpenListItems[u]].FCost >= _open[_binaryHeapIdsOfOpenListItems[2 * u]].FCost)
                        v = 2*u;
                    if (_open[_binaryHeapIdsOfOpenListItems[v]].FCost >= _open[_binaryHeapIdsOfOpenListItems[2 * u + 1]].FCost)
                        v = 2*u + 1;
                }
                else
                {
                    if (2 * u <= orderedItemsLastIndex) //if only child #1 exists
                    {
                        //Check if the F cost of the parent is greater than child #1	
                        if (_open[_binaryHeapIdsOfOpenListItems[u]].FCost >= _open[_binaryHeapIdsOfOpenListItems[2 * u]].FCost)
                            v = 2*u;
                    }
                }

                if (u != v) //if parent's F is > one of its children, swap them
                {
                    var temp = _binaryHeapIdsOfOpenListItems[u];
                    _binaryHeapIdsOfOpenListItems[u] = _binaryHeapIdsOfOpenListItems[v];
                    _binaryHeapIdsOfOpenListItems[v] = temp;
                }
                else
                    break; //otherwise, exit loop
            } while (false); //reorder the binary heap
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
