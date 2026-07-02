using System;
using System.Collections.Generic;

namespace WaterSortPuzzle.Core
{
    public class Board
    {
        private readonly Tube[] _tubes;
        private readonly Stack<Move> _history = new();

        public int TubeCount => _tubes.Length;
        public bool CanUndo => _history.Count > 0;

        public Board(Tube[] tubes)
        {
            if (tubes == null || tubes.Length == 0)
                throw new ArgumentException("튜브가 최소 하나 이상 있어야 합니다.");
            _tubes = tubes;
        }

        public Tube GetTube(int index) => _tubes[index];

        // 성공 시 옮긴 세그먼트 수 반환, 이동 불가 시 0 반환
        public int TryPour(int from, int to)
        {
            if (from == to) return 0;

            Tube src = _tubes[from];
            Tube dst = _tubes[to];

            if (src.IsEmpty) return 0;
            if (!dst.CanAccept(src.TopColor)) return 0;

            int color = src.TopColor;
            int count = Math.Min(src.TopRunLength, dst.FreeSpace);

            for (int i = 0; i < count; i++)
            {
                src.Pop();
                dst.Push(color);
            }

            _history.Push(new Move(from, to, count, color));
            return count;
        }

        public bool TryUndo()
        {
            if (!CanUndo) return false;

            Move move = _history.Pop();
            Tube src = _tubes[move.To];
            Tube dst = _tubes[move.From];

            for (int i = 0; i < move.Count; i++)
            {
                src.Pop();
                dst.Push(move.Color);
            }

            return true;
        }
    }
}
