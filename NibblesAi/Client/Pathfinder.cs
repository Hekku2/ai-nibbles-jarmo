using System;
using Functional.Maybe;

namespace Client
{
    /// <summary>
    /// Path finder implementation in C#, modified from http://www.policyalmanac.org/games/aStarTutorial.htm
    /// </summary>
    public class Pathfinder
    {
        private const int Players = 1;
        private readonly Int64 _mapWidth;
        private readonly Int64 _mapHeight;

        private const int NotStarted = 0;
        private const int Found = 1;

        private readonly int[] _openList; //1 dimensional array holding ID# of open list items
        private readonly ListStatus[,] _whichList;
        private readonly Location[] _open;

        private readonly Location[,] _parent; //2D array of parents
        private readonly long[] _fcost;	//1d array to store F cost of a cell on the open list
        private readonly long[,] _movementCostFromStart; 	//2d array to store G cost for each cell.
        private readonly long[] _estimatedMovementCostToTarget;	//1d array to store H cost of a cell on the open list
        private readonly int[] _pathLength;
        private const int NonDiagonalMovementCost = 10;
        private const int DiagonalMovementCost = 14;

        public Pathfinder(Int64 mapWidth, Int64 mapHeight)
        {
            _mapWidth = mapWidth;
            _mapHeight = mapHeight;
            _openList = new int[_mapWidth * _mapHeight + 2];
            _whichList = new ListStatus[_mapWidth + 1, mapHeight + 1];
            _open = new Location[_mapWidth * _mapHeight + 2];

            _parent = InitializeParents(_mapWidth + 1, mapHeight + 1);
            _fcost = new long[_mapWidth * _mapHeight + 2];
            _movementCostFromStart = new long[_mapWidth + 1, mapHeight + 1];
            _estimatedMovementCostToTarget = new long[_mapWidth * _mapHeight + 2];
            _pathLength = new int[Players];
        }

        private static Location[,] InitializeParents(long width, long height)
        {
            var parents = new Location[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    parents[x,y] = new Location(0, 0);
                }
            }
            return parents;
        }

        public Maybe<Direction> FindPath(Location startPoint, Location target, Location[] blockedLocations)
        {
            if (startPoint.X == target.X && startPoint.Y == target.Y || !IsInGamefield(target.X, target.Y))
                return Maybe<Direction>.Nothing;

            ResetWhichList();

            _pathLength[0] = NotStarted;
            _movementCostFromStart[startPoint.X, startPoint.Y] = 0;
            var newOpenListItemId = 0;

            //4.Add the starting location to the open list of squares to be checked.
            var numberOfOpenListItems = 1;
            _openList[1] = 1;//assign it as the top (and currently only) item in the open list, which is maintained as a binary heap (explained below)
            _open[1] = startPoint;

            //5.Do the following until a path is found or deemed nonexistent.
            int path;
            do
            {
                //6.If the open list is not empty, take the first cell off of the list. This is the lowest F cost cell on the open list.
                if (numberOfOpenListItems != 0)
                {
                    //7. Pop the first item off the open list.
                    var parent = _open[_openList[1]];
                    _whichList[parent.X, parent.Y] = ListStatus.Closed;
                    numberOfOpenListItems = numberOfOpenListItems - 1;
                    OrderBinaryHeap(numberOfOpenListItems+1);

                    // 7.Check the adjacent squares. Add these adjacent child squares to the open list
                    //	for later consideration if appropriate (see various if statements
                    //	below).
                    for (var yCoordinate = parent.Y - 1; yCoordinate <= parent.Y + 1; yCoordinate++)
                    {
                        for (var xCoordinate = parent.X - 1; xCoordinate <= parent.X + 1; xCoordinate++)
                        {
                            //	If not off the map (do this first to avoid array out-of-bounds errors)
                            if (!IsInGamefield(xCoordinate, yCoordinate) || _whichList[xCoordinate, yCoordinate] == ListStatus.Closed)
                                continue;

                            if (!CanBeTraversed(blockedLocations, xCoordinate, yCoordinate) || !IsCornerWalkable(blockedLocations, xCoordinate, parent.X, yCoordinate, parent.Y))
                                continue;

                            //	If not already on the open list, add it to the open list.			
                            if (_whichList[xCoordinate, yCoordinate] != ListStatus.Open)
                            {
                                //Create a new open list item in the binary heap.
                                newOpenListItemId = newOpenListItemId + 1; //each new item has a unique ID #
                                var m = numberOfOpenListItems + 1;
                                _openList[m] = newOpenListItemId;//place the new open list item (actually, its ID#) at the bottom of the heap
                                _open[newOpenListItemId] = new Location(xCoordinate, yCoordinate);
                                _movementCostFromStart[xCoordinate, yCoordinate] = _movementCostFromStart[parent.X, parent.Y] + CalculateAddedCost(xCoordinate, parent.X, yCoordinate, parent.Y);

                                //Figure out its H and F costs and parent
                                _estimatedMovementCostToTarget[_openList[m]] = 10 * (Math.Abs(xCoordinate - target.X) + Math.Abs(yCoordinate - target.Y));
                                _fcost[_openList[m]] = _movementCostFromStart[xCoordinate, yCoordinate] + _estimatedMovementCostToTarget[_openList[m]];

                                _parent[xCoordinate, yCoordinate] = parent;

                                //Move the new open list item to the proper place in the binary heap.
                                //Starting at the bottom, successively compare to parent items,
                                //swapping as needed until the item finds its place in the heap
                                //or bubbles all the way to the top (if it has the lowest F cost).
                                while (m != 1) //While item hasn't bubbled to the top (m=1)	
                                {
                                    //Check if child's F cost is < parent's F cost. If so, swap them.	
                                    if (_fcost[_openList[m]] <= _fcost[_openList[m / 2]])
                                    {
                                        var temp = _openList[m / 2];
                                        _openList[m / 2] = _openList[m];
                                        _openList[m] = temp;
                                        m = m / 2;
                                    }
                                    else
                                        break;
                                }
                                numberOfOpenListItems = numberOfOpenListItems + 1;//add one to the number of items in the heap

                                //Change whichList to show that the new item is on the open list.
                                _whichList[xCoordinate, yCoordinate] = ListStatus.Open;
                            }
                            else
                            {
                                var calculatedCostFromSTart = _movementCostFromStart[parent.X, parent.Y] + CalculateAddedCost(xCoordinate, parent.X, yCoordinate, parent.Y);	
                                if (calculatedCostFromSTart >= _movementCostFromStart[xCoordinate, yCoordinate]) continue;
                                
                                _parent[xCoordinate, yCoordinate] = parent;
                                _movementCostFromStart[xCoordinate, yCoordinate] = calculatedCostFromSTart;//change the G cost			

                                for (var x = 1; x <= numberOfOpenListItems; x++) //look for the item in the heap
                                {
                                    if (_open[_openList[x]].X != xCoordinate || _open[_openList[x]].Y != yCoordinate)
                                        continue;

                                    _fcost[_openList[x]] = _movementCostFromStart[xCoordinate, yCoordinate] + _estimatedMovementCostToTarget[_openList[x]];//change the F cost

                                    //See if changing the F score bubbles the item up from it's current location in the heap
                                    var m = x;
                                    while (m != 1) //While item hasn't bubbled to the top (m=1)	
                                    {
                                        //Check if child is < parent. If so, swap them.	
                                        if (_fcost[_openList[m]] < _fcost[_openList[m / 2]])
                                        {
                                            var temp = _openList[m / 2];
                                            _openList[m / 2] = _openList[m];
                                            _openList[m] = temp;
                                            m = m / 2;
                                        }
                                        else
                                            break;
                                    }
                                    break;
                                }
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
                if (_whichList[target.X, target.Y] == ListStatus.Open)
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
                var parent = _parent[pathX, pathY];
                pathX = parent.X;
                pathY = parent.Y;

                //Figure out the path length
                _pathLength[0] = _pathLength[0] + 1;
            }
            while (pathX != startPoint.X || pathY != startPoint.Y);

            return startPoint.GetDirection(GetFirstStepOfPath(startPoint, target)).ToMaybe();
        }

        private Location GetFirstStepOfPath(Location startPoint, Location target)
        {
            var pathX = target.X;
            var pathY = target.Y;

            var cellPosition = _pathLength[0] * 2;
            Location final;
            do
            {
                cellPosition = cellPosition - 2;
                final = new Location(pathX, pathY);

                var parent = _parent[pathX, pathY];
                pathX = parent.X;
                pathY = parent.Y;
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
            _openList[1] = _openList[newItemIndex];

            //	Repeat the following until the new item in slot #1 sinks to its proper spot in the heap.
            var v = 1;
            do
            {
                var u = v;
                if (2 * u + 1 <= orderedItemsLastIndex) //if both children exist
                {
                    //Check if the F cost of the parent is greater than each child.
                    //Select the lowest of the two children.
                    if (_fcost[_openList[u]] >= _fcost[_openList[2*u]])
                        v = 2*u;
                    if (_fcost[_openList[v]] >= _fcost[_openList[2*u + 1]])
                        v = 2*u + 1;
                }
                else
                {
                    if (2 * u <= orderedItemsLastIndex) //if only child #1 exists
                    {
                        //Check if the F cost of the parent is greater than child #1	
                        if (_fcost[_openList[u]] >= _fcost[_openList[2*u]])
                            v = 2*u;
                    }
                }

                if (u != v) //if parent's F is > one of its children, swap them
                {
                    var temp = _openList[u];
                    _openList[u] = _openList[v];
                    _openList[v] = temp;
                }
                else
                    break; //otherwise, exit loop
            } while (false); //reorder the binary heap
        }

        private void ResetWhichList()
        {
            for (var x = 0; x < _mapWidth; x++)
                for (var y = 0; y < _mapHeight; y++)
                    _whichList[x, y] = ListStatus.None;
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
