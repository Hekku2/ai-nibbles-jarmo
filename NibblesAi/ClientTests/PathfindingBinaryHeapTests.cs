using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using NUnit.Framework;

namespace ClientTests
{
    [TestFixture]
    public class PathfindingBinaryHeapTests
    {
        [Test]
        public void TestPopReturnsItemWithLowestFCost()
        {
            var highest = new PathfindingState(0, 0)
            {
                FCost = 10
            };
            var lowest = new PathfindingState(0, 0)
            {
                FCost = 5
            };
            var middle = new PathfindingState(0, 0)
            {
                FCost = 7
            };
            var heap = new PathfindingBinaryHeap(2);
            heap.Add(highest);
            heap.Add(lowest);
            heap.Add(middle);

            Assert.AreEqual(lowest.FCost, heap.Pop().FCost);
            Assert.AreEqual(middle.FCost, heap.Pop().FCost);
            Assert.AreEqual(highest.FCost, heap.Pop().FCost);
            Assert.IsFalse(heap.HasItems(), "Heap should be empty.");
        }

        [Test]
        public void TestHasItemsReturnsFalseAfterCreation()
        {
            Assert.IsFalse(new PathfindingBinaryHeap(20).HasItems());
        }

        [Test]
        public void TestCalculateNewFCostAndSort()
        {
            var heap = new PathfindingBinaryHeap(3);
            var highest = new PathfindingState(0, 0)
            {
                FCost = 10
            };
            var lowest = new PathfindingState(2, 2)
            {
                FCost = 5
            };
            var middle = new PathfindingState(1, 1)
            {
                FCost = 7
            };
            heap.Add(highest);
            heap.Add(lowest);
            heap.Add(middle);
            
            heap.CalculateNewFCostAndSort(1, 1, 1);
            Assert.AreEqual(1, heap.Pop().FCost);
        }
    }
}
