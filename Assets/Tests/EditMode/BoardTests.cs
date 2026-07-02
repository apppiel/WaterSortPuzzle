using NUnit.Framework;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Tests
{
    public class BoardTests
    {
        // 기본 붓기: src 맨 위 색을 dst로 이동
        [Test]
        public void TryPour_BasicMove_MovesSegments()
        {
            var src = new Tube(4, new[] { 1, 2 });
            var dst = new Tube(4, new[] { 2 });
            var board = new Board(new[] { src, dst });

            int moved = board.TryPour(0, 1);

            Assert.AreEqual(1, moved);
            Assert.AreEqual(1, src.Count);
            Assert.AreEqual(2, dst.Count);
        }

        // 연속 세그먼트를 한 번에 이동
        [Test]
        public void TryPour_RunOfSameColor_MovesAll()
        {
            var src = new Tube(4, new[] { 1, 2, 2 });
            var dst = new Tube(4);
            var board = new Board(new[] { src, dst });

            int moved = board.TryPour(0, 1);

            Assert.AreEqual(2, moved);
            Assert.AreEqual(1, src.Count);
            Assert.AreEqual(2, dst.Count);
        }

        // dst 빈 공간이 부족하면 허용 범위만큼만 이동
        [Test]
        public void TryPour_LimitedByDstFreeSpace()
        {
            var src = new Tube(4, new[] { 1, 2, 2, 2 });
            var dst = new Tube(4, new[] { 2, 2 });
            var board = new Board(new[] { src, dst });

            int moved = board.TryPour(0, 1);

            Assert.AreEqual(2, moved);
            Assert.AreEqual(2, src.Count);
            Assert.AreEqual(4, dst.Count);
        }

        // 색이 달라서 이동 불가
        [Test]
        public void TryPour_ColorMismatch_Returns0()
        {
            var src = new Tube(4, new[] { 1, 3 });
            var dst = new Tube(4, new[] { 2 });
            var board = new Board(new[] { src, dst });

            int moved = board.TryPour(0, 1);

            Assert.AreEqual(0, moved);
        }

        // 같은 튜브로 붓기 불가
        [Test]
        public void TryPour_SameIndex_Returns0()
        {
            var tube = new Tube(4, new[] { 1, 2 });
            var board = new Board(new[] { tube, new Tube(4) });

            int moved = board.TryPour(0, 0);

            Assert.AreEqual(0, moved);
        }

        // 빈 튜브에서 붓기 불가
        [Test]
        public void TryPour_EmptySource_Returns0()
        {
            var src = new Tube(4);
            var dst = new Tube(4, new[] { 1 });
            var board = new Board(new[] { src, dst });

            int moved = board.TryPour(0, 1);

            Assert.AreEqual(0, moved);
        }

        // Undo: 이동을 되돌림
        [Test]
        public void TryUndo_ReversesLastMove()
        {
            var src = new Tube(4, new[] { 1, 2 });
            var dst = new Tube(4);
            var board = new Board(new[] { src, dst });

            board.TryPour(0, 1);
            bool undone = board.TryUndo();

            Assert.IsTrue(undone);
            Assert.AreEqual(2, src.Count);
            Assert.AreEqual(0, dst.Count);
        }

        // Undo: 히스토리 없을 때 false
        [Test]
        public void TryUndo_NothingToUndo_ReturnsFalse()
        {
            var board = new Board(new[] { new Tube(4), new Tube(4) });
            Assert.IsFalse(board.TryUndo());
        }

        // Undo: 여러 번 되돌리기
        [Test]
        public void TryUndo_MultipleMovesUndone()
        {
            var src = new Tube(4, new[] { 1 });
            var mid = new Tube(4);
            var dst = new Tube(4);
            var board = new Board(new[] { src, mid, dst });

            board.TryPour(0, 1); // src → mid
            board.TryPour(1, 2); // mid → dst
            board.TryUndo();     // mid ← dst
            board.TryUndo();     // src ← mid

            Assert.AreEqual(1, src.Count);
            Assert.AreEqual(0, mid.Count);
            Assert.AreEqual(0, dst.Count);
        }
    }
}
