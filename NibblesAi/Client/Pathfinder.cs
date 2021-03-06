﻿using System;
using Functional.Maybe;

namespace Client
{
    /// <summary>
    /// Path finder implementation in C#, modified from http://www.policyalmanac.org/games/aStarTutorial.htm
    /// </summary>
    public class Pathfinder
    {
        public Int64 MapWidth { get; private set; }
        public Int64 MapHeight { get; private set; }

        private const int NotStarted = 0;
        private const int Found = 1;

        private readonly PathfindingBinaryHeap _heap;

        private readonly PathfindingState[,] _locations; 
        private int _pathLength;
        private const int NonDiagonalMovementCost = 10;
        private const int DiagonalMovementCost = 14;

        public Pathfinder(Int64 mapWidth, Int64 mapHeight)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            _locations = InitializeLocations(MapWidth + 1, mapHeight + 1);
            _heap = new PathfindingBinaryHeap(MapWidth * MapHeight);
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

        public Maybe<Direction> FindPath(Location startPoint, Location target, bool[,] blockedLocations)
        {
            _heap.Clear();
            if (startPoint == target || !IsInGamefield(target))
                return Maybe<Direction>.Nothing;

            ResetPathdinfing();

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
                    foreach (var location in AdjacentLocations(parent.Location))
                    {
                        //	If not off the map (do this first to avoid array out-of-bounds errors)
                        if (!IsInGamefield(location) || _locations[location.X, location.Y].Status == ListStatus.Closed)
                            continue;

                        if (!CanBeTraversed(blockedLocations, location.X, location.Y) || !IsCornerWalkable(blockedLocations, location, parent.Location))
                            continue;

                        //	If not already on the open list, add it to the open list.			
                        if (_locations[location.X, location.Y].Status != ListStatus.Open)
                        {
                            //Figure out its H and F costs and parent
                            var newItem = _locations[location.X, location.Y];
                            newItem.MovementCostFromStart = newItem.MovementCostFromStart + CalculateAddedCost(location, parent.Location);
                            newItem.EstimatedMovementCostToTarget = NonDiagonalMovementCost * (Math.Abs(location.X - target.X) + Math.Abs(location.Y - target.Y));
                            newItem.FCost = newItem.MovementCostFromStart + newItem.EstimatedMovementCostToTarget;
                            newItem.Parent = parent;
                            newItem.Status = ListStatus.Open;
                            _heap.Add(newItem);
                        }
                        else
                        {
                            AdjustLocationIfCloserFromStart(parent, location);
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
            var pathStep = target;
            do
            {
                //Look up the parent of the current cell.
                var parent = _locations[pathStep.X, pathStep.Y].Parent;
                pathStep = parent.Location;

                //Figure out the path length
                _pathLength++;
            }
            while (pathStep != startPoint);

            return startPoint.GetDirection(GetFirstStepOfPath(startPoint, target)).ToMaybe();
        }

        private void AdjustLocationIfCloserFromStart(PathfindingState parent, Location location)
        {
            var calculatedCostFromStart = parent.MovementCostFromStart + CalculateAddedCost(location, parent.Location);
            if (calculatedCostFromStart >= _locations[location.X, location.Y].MovementCostFromStart)
                return;

            var considered = _locations[location.X, location.Y];
            considered.Parent = parent;
            considered.MovementCostFromStart = calculatedCostFromStart;
            _heap.CalculateNewFCostAndSort(location, considered.MovementCostFromStart);
        }

        private Location[] AdjacentLocations(Location location)
        {
            var locations = new Location[9];

            var i = 0;
            for (var yCoordinate = location.Y - 1; yCoordinate <= location.Y + 1; yCoordinate++)
            {
                for (var xCoordinate = location.X - 1; xCoordinate <= (location.X + 1); xCoordinate++)
                {
                    locations[i] = new Location(xCoordinate, yCoordinate);
                    i++;
                }
            }
            
            return locations;
        }

        private Location GetFirstStepOfPath(Location startPoint, Location target)
        {
            var path = target;

            var cellPosition = _pathLength * 2;
            Location final;
            do
            {
                cellPosition = cellPosition - 2;
                final = new Location(path.X, path.Y);

                var parent = _locations[path.X, path.Y].Parent;
                path = parent.Location;
            }
            while (path != startPoint);

            return final;
        }

        private static bool IsCornerWalkable(bool[,] blockedLocations, Location location, Location parent)
        {
            if (location.X == parent.X - 1)
            {
                if (location.Y == parent.Y - 1)
                {
                    if (!CanBeTraversed(blockedLocations, parent.X - 1, parent.Y) ||
                        !CanBeTraversed(blockedLocations, parent.X, parent.Y - 1))
                        return false;
                }
                else if (location.Y == parent.Y + 1)
                {
                    if (!CanBeTraversed(blockedLocations, parent.X, parent.Y + 1) ||
                        !CanBeTraversed(blockedLocations, parent.X - 1, parent.Y))
                        return false;
                }
            }
            else if (location.X == parent.X + 1)
            {
                if (location.Y == parent.Y - 1)
                {
                    if (!CanBeTraversed(blockedLocations, parent.X, parent.Y - 1) ||
                        !CanBeTraversed(blockedLocations, parent.X + 1, parent.Y))
                        return false;
                }
                else if (location.Y == parent.Y + 1)
                {
                    if (!CanBeTraversed(blockedLocations, parent.X + 1, parent.Y) ||
                        !CanBeTraversed(blockedLocations, parent.X, parent.Y + 1))
                        return false;
                }
            }
            return true;
        }

        private void ResetPathdinfing()
        {
            foreach (var location in _locations)
            {
                location.Status = ListStatus.None;
                location.Parent = null;
                location.FCost = 0;
                location.EstimatedMovementCostToTarget = 0;
                location.MovementCostFromStart = 0;
            }
        }

        private static int CalculateAddedCost(Location location, Location parent)
        {
            if (location.X - parent.X != 0 && location.Y - parent.Y != 0)
                return DiagonalMovementCost;	
            return NonDiagonalMovementCost;
        }

        private static bool CanBeTraversed(bool[,] blockedLocations, long x, long y)
        {
            return !blockedLocations[x, y];
        }

        private bool IsInGamefield(Location location)
        {
            return location.X >= 0 && location.X < MapWidth && location.Y >= 0 && location.Y < MapHeight;
        }
    }
}
