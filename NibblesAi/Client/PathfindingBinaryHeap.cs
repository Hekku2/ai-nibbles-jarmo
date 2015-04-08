using System;

namespace Client
{
    public class PathfindingBinaryHeap
    {
        private int _numberOfItems;
        private int _newOpenListItemId;
        private const int BinaryHeapTopIndex = 1;
        private readonly int[] _binaryHeapIdsOfOpenListItems;
        private readonly PathfindingState[] _open;

        public PathfindingBinaryHeap(Int64 size)
        {
            _open = new PathfindingState[size + 2];
            _binaryHeapIdsOfOpenListItems = new int[size + 2];

            _binaryHeapIdsOfOpenListItems[BinaryHeapTopIndex] = 1;
        }

        public void Add(PathfindingState value)
        {
            
            _newOpenListItemId = _newOpenListItemId + 1; //each new item has a unique ID #
            var i = ++_numberOfItems;
            _binaryHeapIdsOfOpenListItems[i] = _newOpenListItemId;//place the new open list item (actually, its ID#) at the bottom of the heap
            _open[_newOpenListItemId] = value;

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
        }

        public bool HasItems()
        {
            return _numberOfItems > 0;
        }

        private PathfindingState LowestFCost()
        {
            return _open[_binaryHeapIdsOfOpenListItems[BinaryHeapTopIndex]];
        }

        public void CalculateNewFCostAndSort(long x, long y, long movementCostFromStart)
        {
            for (var i = BinaryHeapTopIndex; i <= _numberOfItems; i++) //look for the item in the heap
            {
                var location = _open[_binaryHeapIdsOfOpenListItems[i]];
                if (location.Location.X != x || location.Location.Y != y)
                    continue;

                _open[_binaryHeapIdsOfOpenListItems[i]].FCost = movementCostFromStart + _open[_binaryHeapIdsOfOpenListItems[i]].EstimatedMovementCostToTarget;
                SortWithNewFCost(i);
                break;
            }
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

        public PathfindingState Pop()
        {
            var item = LowestFCost();
            _numberOfItems = _numberOfItems - 1;
            OrderBinaryHeap(_numberOfItems);
            return item;
        }

        private void OrderBinaryHeap(int newItemIndex)
        {
            var orderedItemsLastIndex = newItemIndex;
            _binaryHeapIdsOfOpenListItems[BinaryHeapTopIndex] = _binaryHeapIdsOfOpenListItems[newItemIndex + 1];

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
                        v = 2 * u;
                    if (_open[_binaryHeapIdsOfOpenListItems[v]].FCost >= _open[_binaryHeapIdsOfOpenListItems[2 * u + 1]].FCost)
                        v = 2 * u + 1;
                }
                else
                {
                    if (2 * u <= orderedItemsLastIndex) //if only child #1 exists
                    {
                        //Check if the F cost of the parent is greater than child #1	
                        if (_open[_binaryHeapIdsOfOpenListItems[u]].FCost >= _open[_binaryHeapIdsOfOpenListItems[2 * u]].FCost)
                            v = 2 * u;
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

        public void Clear()
        {
            _newOpenListItemId = 0;
            _numberOfItems = 0;
        }
    }
}
