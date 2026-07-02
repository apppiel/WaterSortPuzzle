using NUnit.Framework;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Tests
{
    public class TubeTests
    {
        [Test]
        public void NewTube_IsEmpty()
        {
            var tube = new Tube(4);
            Assert.IsTrue(tube.IsEmpty);
            Assert.AreEqual(0, tube.Count);
            Assert.AreEqual(-1, tube.TopColor);
        }

        [Test]
        public void TopRunLength_ConsecutiveSameColor()
        {
            var tube = new Tube(4, new[] { 1, 2, 2, 2 });
            Assert.AreEqual(3, tube.TopRunLength);
        }

        [Test]
        public void TopRunLength_SingleTop()
        {
            var tube = new Tube(4, new[] { 1, 2, 2, 3 });
            Assert.AreEqual(1, tube.TopRunLength);
        }

        [Test]
        public void CanAccept_EmptyTube_AcceptsAnyColor()
        {
            var tube = new Tube(4);
            Assert.IsTrue(tube.CanAccept(1));
            Assert.IsTrue(tube.CanAccept(99));
        }

        [Test]
        public void CanAccept_SameTopColor_Accepts()
        {
            var tube = new Tube(4, new[] { 1, 2 });
            Assert.IsTrue(tube.CanAccept(2));
        }

        [Test]
        public void CanAccept_DifferentTopColor_Rejects()
        {
            var tube = new Tube(4, new[] { 1, 2 });
            Assert.IsFalse(tube.CanAccept(1));
        }

        [Test]
        public void CanAccept_FullTube_Rejects()
        {
            var tube = new Tube(2, new[] { 1, 1 });
            Assert.IsFalse(tube.CanAccept(1));
        }

        [Test]
        public void IsComplete_FullSingleColor()
        {
            var tube = new Tube(3, new[] { 2, 2, 2 });
            Assert.IsTrue(tube.IsComplete);
        }

        [Test]
        public void IsComplete_NotFull_ReturnsFalse()
        {
            var tube = new Tube(4, new[] { 2, 2, 2 });
            Assert.IsFalse(tube.IsComplete);
        }

        [Test]
        public void IsComplete_MixedColor_ReturnsFalse()
        {
            var tube = new Tube(3, new[] { 1, 2, 2 });
            Assert.IsFalse(tube.IsComplete);
        }
    }
}
