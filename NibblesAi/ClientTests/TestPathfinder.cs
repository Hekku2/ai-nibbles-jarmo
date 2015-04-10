using System;
using System.Linq;
using Client;
using NUnit.Framework;

namespace ClientTests
{
    [TestFixture]
    public class TestPathfinder
    {
        private const long MaxSize = 2000;
        private readonly bool[,] _nothing = new bool[MaxSize, MaxSize];

        [Test]
        public void TestFindPathReturnsCorrectDirectionWithTrivialScenario()
        {
            var pathFinder = new Pathfinder(2, 1);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(0, 0), new Location(1, 0), _nothing).Value);
            Assert.AreEqual(Direction.Left, pathFinder.FindPath(new Location(1, 0), new Location(0, 0), _nothing).Value);

            pathFinder = new Pathfinder(1, 2);
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 0), new Location(0, 1), _nothing).Value);
            Assert.AreEqual(Direction.Down, pathFinder.FindPath(new Location(0, 1), new Location(0, 0), _nothing).Value);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfWayIsBlocked()
        {
            var pathFinder = new Pathfinder(3, 1);
            var block = GenerateBlockingListFromLocations(new[] { new Location(1, 0) }, pathFinder);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(1, 0), block).HasValue);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(2, 0), block).HasValue);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfTargetIsNotOnPlayfield()
        {
            var pathFinder = new Pathfinder(2, 2);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(3, 0), _nothing).HasValue);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(0, 3), _nothing).HasValue);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfSourceIsOnTarget()
        {
            var pathFinder = new Pathfinder(1, 1);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(0, 0), _nothing).HasValue);
        }

        [Test]
        public void TestFindPathCanGoAroundObject()
        {
            var pathFinder = new Pathfinder(5, 2);
            /*******
             *s*   *
             *   *t*
             ******/
            var blocks = GenerateBlockingListFromLocations(new[]
            {
                new Location(1, 0), 
                new Location(3, 1)
            }, pathFinder);
            var target = new Location(4, 1);
            
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 0), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(0, 1), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(1, 1), target, blocks).Value);
            Assert.AreEqual(Direction.Down, pathFinder.FindPath(new Location(2, 1), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(2, 0), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(3, 0), target, blocks).Value);
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(4, 0), target, blocks).Value);
        }

        [Test]
        public void TestFindPathVerticalUTurn()
        {
            var pathFinder = new Pathfinder(3, 3);
            /*****
             *s*t*
             * * *
             *   *
             *****/
            var blocks = GenerateBlockingListFromLocations(new[]
            {
                new Location(1, 0), 
                new Location(1, 1)
            }, pathFinder);

            var target = new Location(2, 0);
            
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 0), target, blocks).Value);
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 1), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(0, 2), target, blocks).Value);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(1, 2), target, blocks).Value);
            Assert.AreEqual(Direction.Down, pathFinder.FindPath(new Location(2, 2), target, blocks).Value);
            Assert.AreEqual(Direction.Down, pathFinder.FindPath(new Location(2, 1), target, blocks).Value);
        }

        [Test]
        public void TestCornerCase()
        {
            var blocks = new[]
            {
                new Location(1, 0), 
                new Location(0, 1)
            };
            /****
             *s**
             **t*
             ****/
            var target = new Location(1, 1);
            var pathFinder = new Pathfinder(2, 2);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), target, GenerateBlockingListFromLocations(blocks, pathFinder)).HasValue);
        }

        [Test]
        public void TestCornerCaseWithRouteVertical()
        {
            var blocks = new[]
            {
                new Location(1, 0)
            };
            /****
             *s**
             * t*
             ****/
            var target = new Location(1, 1);
            var pathFinder = new Pathfinder(2, 2);
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 0), target, GenerateBlockingListFromLocations(blocks, pathFinder)).Value);
        }

        [Test]
        public void TestCornerCaseWithRouteHorisontal()
        {
            var blocks = new[]
            {
                new Location(0, 1)
            };
            /****
             *s *
             **t*
             ****/
            var target = new Location(1, 1);
            var pathFinder = new Pathfinder(2, 2);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(0, 0), target, GenerateBlockingListFromLocations(blocks, pathFinder)).Value);
        }

        [Test]
        public void TestLongRoadToRight()
        {
            const int length = 1000;
            var blocks = Enumerable.Range(1, length).Where(n => n % 3 == 0).Select(n => new Location(n, n % 2)).ToArray();
            var start = new Location(0, 0);
            var target = new Location(length - 1, 0);

            var startTime = DateTime.UtcNow;
            var pathFinder = new Pathfinder(length, 2);
            var blocklist = GenerateBlockingListFromLocations(blocks, pathFinder);
            var result = pathFinder.FindPath(start, target, blocklist);
            while (result.HasValue && result.Value != Direction.Left)
            {
                start = start + result.Value.LocationDelta();
                result = pathFinder.FindPath(start, target, blocklist);
            }
            Assert.IsTrue(start == target, "When route is traversed, start should be same as target.");
            var endTime = DateTime.UtcNow;
            Assert.IsTrue(endTime - startTime < TimeSpan.FromSeconds(4), "Duration: {0}", endTime - startTime);
        }

        [Test]
        public void TestLongRoadToLeft()
        {
            const int length = 1000;
            var blocks = Enumerable.Range(1, length).Where(n => n % 3 == 0).Select(n => new Location(n, n % 2)).ToArray();
            var start = new Location(length-1, 0);
            var target = new Location(0, 0);

            var startTime = DateTime.UtcNow;
            var pathFinder = new Pathfinder(length, 2);
            var blocklist = GenerateBlockingListFromLocations(blocks, pathFinder);
            var result = pathFinder.FindPath(start, target, blocklist);
            while (result.HasValue && result.Value != Direction.Right)
            {
                start = start + result.Value.LocationDelta();
                result = pathFinder.FindPath(start, target, blocklist);
            }
            Assert.IsTrue(start == target, string.Format("When route is traversed, start should be same as target. Current position {0}, target was {1}", start, target));
            var endTime = DateTime.UtcNow;
            Assert.IsTrue(endTime - startTime < TimeSpan.FromSeconds(4), "Duration: {0}", endTime - startTime);
        }

        [Test]
        public void TestLongRoadBottom()
        {
            const int length = 1000;
            var blocks = Enumerable.Range(1, length).Where(n => n % 3 == 0).Select(n => new Location(n % 2, n)).ToArray();
            var start = new Location(0, length - 1);
            var target = new Location(0, 0);

            var startTime = DateTime.UtcNow;
            var pathFinder = new Pathfinder(2, length);
            var blocklist = GenerateBlockingListFromLocations(blocks, pathFinder);
            var result = pathFinder.FindPath(start, target, blocklist);
            while (result.HasValue && result.Value != Direction.Up)
            {
                start = start + result.Value.LocationDelta();
                result = pathFinder.FindPath(start, target, blocklist);
            }
            Assert.IsTrue(start == target, string.Format("When route is traversed, start should be same as target. Current position {0}, target was {1}", start, target));
            var endTime = DateTime.UtcNow;
            Assert.IsTrue(endTime - startTime < TimeSpan.FromSeconds(4), "Duration: {0}", endTime - startTime);
        }

        [Test]
        public void TestLongRoadTop()
        {
            const int length = 1000;
            var blocks = Enumerable.Range(1, length).Where(n => n % 3 == 0).Select(n => new Location(n % 2, n)).ToArray();
            var start = new Location(0, 0);
            var target = new Location(0, length - 1);

            var startTime = DateTime.UtcNow;
            var pathFinder = new Pathfinder(2, length);
            var blocklist = GenerateBlockingListFromLocations(blocks, pathFinder);
            var result = pathFinder.FindPath(start, target, blocklist);
            while (result.HasValue)
            {
                Assert.IsFalse(result.Value == Direction.Down, "Direction should not be down");
                start = start + result.Value.LocationDelta();
                result = pathFinder.FindPath(start, target, blocklist);
            }
            Assert.IsTrue(start == target, string.Format("When route is traversed, start should be same as target. Current position {0}, target was {1}", start, target));
            var endTime = DateTime.UtcNow;
            Assert.IsTrue(endTime - startTime < TimeSpan.FromSeconds(4), "Duration: {0}", endTime - startTime);
        }

        [Test]
        public void TestWiderArea()
        {
            /******
             *   s*
             *    *
             *t   *
             ******/
            const int size = 20;
            var start = new Location(size - 1, 0);
            var target = new Location(0, size - 1);
            var pathFinder = new Pathfinder(size, size);
            var result = pathFinder.FindPath(start, target, _nothing);
            while (result.HasValue)
            {
                Assert.IsFalse(result.Value == Direction.Right, "Direction should not be right");
                Assert.IsFalse(result.Value == Direction.Down, "Direction should not be down");
                start = start + result.Value.LocationDelta();
                result = pathFinder.FindPath(start, target, _nothing);
            }
            Assert.IsTrue(start == target, string.Format("When route is traversed, start should be same as target. Current position {0}, target was {1}", start, target));
        }

        private static bool[,] GenerateBlockingListFromLocations(Location[] locations, Pathfinder finder)
        {
            var blocked = new bool[finder.MapWidth, finder.MapHeight];
            foreach (var location in locations)
            {
                blocked[location.X, location.Y] = true;
            }
            return blocked;
        }
    }
}
