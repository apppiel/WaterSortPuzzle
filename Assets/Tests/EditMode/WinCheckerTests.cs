using NUnit.Framework;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Tests
{
    public class WinCheckerTests
    {
        [Test]
        public void IsWon_AllComplete_ReturnsTrue()
        {
            var board = new Board(new[]
            {
                new Tube(3, new[] { 1, 1, 1 }),
                new Tube(3, new[] { 2, 2, 2 }),
            });
            Assert.IsTrue(WinChecker.IsWon(board));
        }

        [Test]
        public void IsWon_AllEmpty_ReturnsTrue()
        {
            var board = new Board(new[] { new Tube(3), new Tube(3) });
            Assert.IsTrue(WinChecker.IsWon(board));
        }

        [Test]
        public void IsWon_MixedColors_ReturnsFalse()
        {
            var board = new Board(new[]
            {
                new Tube(3, new[] { 1, 2, 1 }),
                new Tube(3),
            });
            Assert.IsFalse(WinChecker.IsWon(board));
        }

        [Test]
        public void IsWon_NotFull_ReturnsFalse()
        {
            var board = new Board(new[]
            {
                new Tube(3, new[] { 1, 1 }),
                new Tube(3),
            });
            Assert.IsFalse(WinChecker.IsWon(board));
        }

        [Test]
        public void IsWon_CompleteAndEmpty_ReturnsTrue()
        {
            var board = new Board(new[]
            {
                new Tube(3, new[] { 1, 1, 1 }),
                new Tube(3),
            });
            Assert.IsTrue(WinChecker.IsWon(board));
        }
    }
}
