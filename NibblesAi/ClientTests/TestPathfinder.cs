using Client;
using NUnit.Framework;

namespace ClientTests
{
    [TestFixture]
    public class TestPathfinder
    {
        [Test]
        public void TestFindPathReturnsCorrectDirectionWithTrivialScenario()
        {
            var pathFinder = new Pathfinder(2, 1);
            Assert.AreEqual(Direction.Right, pathFinder.FindPath(new Location(0, 0), new Location(1, 0), new Location[0]).Value);
            Assert.AreEqual(Direction.Left, pathFinder.FindPath(new Location(1, 0), new Location(0, 0), new Location[0]).Value);

            pathFinder = new Pathfinder(1, 2);
            Assert.AreEqual(Direction.Up, pathFinder.FindPath(new Location(0, 0), new Location(0, 1), new Location[0]).Value);
            Assert.AreEqual(Direction.Down, pathFinder.FindPath(new Location(0, 1), new Location(0, 0), new Location[0]).Value);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfWayIsBlocked()
        {
            var pathFinder = new Pathfinder(3, 1);
            var block = new Location(1, 0);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(1, 0), new[] { block }).HasValue);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(2, 0), new[] { block }).HasValue);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfTargetIsNotOnPlayfield()
        {
            var pathFinder = new Pathfinder(2, 2);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(3, 0), new Location[0]).HasValue);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(0, 3), new Location[0]).HasValue);
        }

        [Test]
        public void TestFindPathReturnsEmptyDirectionIfSourceIsOnTarget()
        {
            var pathFinder = new Pathfinder(1, 1);
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), new Location(0, 0), new Location[0]).HasValue);
        }

        [Test]
        public void TestFindPathCanGoAroundObject()
        {
            /*******
             *s*   *
             *   *t*
             ******/
            var blocks = new[]
            {
                new Location(1, 0), 
                new Location(3, 1)
            };
            var target = new Location(4, 1);
            var pathFinder = new Pathfinder(5, 2);
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
            /*****
             *s*t*
             * * *
             *   *
             *****/
            var blocks = new[]
            {
                new Location(1, 0), 
                new Location(1, 1)
            };

            var target = new Location(2, 0);
            var pathFinder = new Pathfinder(3, 3);
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
            Assert.IsFalse(pathFinder.FindPath(new Location(0, 0), target, blocks).HasValue);
        }
    }
}
