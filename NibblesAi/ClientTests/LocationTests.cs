using Client;
using NUnit.Framework;

namespace ClientTests
{
    [TestFixture]
    public class LocationTests
    {
        readonly Location _location = new Location(0, 0);
        readonly Location _sameLocation = new Location(0, 0);
        readonly Location _differentY = new Location(0, 1);
        readonly Location _differentX = new Location(1, 0);
        readonly Location _bothDifferent = new Location(1, 1);

        [Test]
        public void TestEquality()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(_location == _location);
            Assert.IsFalse(_location == null);
            Assert.IsFalse(null == _location);
            Assert.IsTrue(_location == _sameLocation);
            Assert.IsFalse(_location == _differentY);
            Assert.IsFalse(_location == _differentX);
            Assert.IsFalse(_location == _bothDifferent);
        }

        [Test]
        public void TestInequality()
        {
            // ReSharper disable once EqualExpressionComparison
            Assert.IsFalse(_location != _location);
            Assert.IsTrue(null != _location);
            Assert.IsTrue(_location != null);
            Assert.IsFalse(_location != _sameLocation);
            Assert.IsTrue(_location != _differentY);
            Assert.IsTrue(_location != _differentX);
            Assert.IsTrue(_location != _bothDifferent);
        }
    }
}
