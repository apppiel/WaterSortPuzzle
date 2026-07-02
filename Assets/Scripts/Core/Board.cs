using System;
using System.Collections.Generic;

namespace WaterSortPuzzle.Core
{
    /// <summary>
    /// 현재 게임 상태를 나타낸다.
    /// 튜브 집합을 보유하고, 붓기(TryPour)와 되돌리기(TryUndo)를 담당한다.
    /// </summary>
    public class Board
    {
        // 모든 튜브 배열
        private readonly Tube[] _tubes;

        // Undo를 위한 이동 히스토리 스택 (가장 최근 이동이 top)
        private readonly Stack<Move> _history = new();

        /// <summary>튜브 수</summary>
        public int TubeCount => _tubes.Length;

        /// <summary>되돌릴 수 있는 이동이 있으면 true</summary>
        public bool CanUndo => _history.Count > 0;

        public Board(Tube[] tubes)
        {
            if (tubes == null || tubes.Length == 0)
                throw new ArgumentException("튜브가 최소 하나 이상 있어야 합니다.");
            _tubes = tubes;
        }

        /// <summary>인덱스로 튜브를 가져온다.</summary>
        public Tube GetTube(int index) => _tubes[index];

        /// <summary>
        /// from 튜브에서 to 튜브로 물을 붓는다.
        /// 규칙을 검증하고, 이동 가능하면 세그먼트를 옮기고 옮긴 수를 반환.
        /// 이동 불가 시 0 반환.
        /// </summary>
        public int TryPour(int from, int to)
        {
            // 같은 튜브는 이동 불가
            if (from == to) return 0;

            Tube src = _tubes[from];
            Tube dst = _tubes[to];

            // 출발 튜브가 비어 있으면 이동 불가
            if (src.IsEmpty) return 0;

            // 도착 튜브가 해당 색을 받을 수 없으면 이동 불가
            if (!dst.CanAccept(src.TopColor)) return 0;

            int color = src.TopColor;

            // 연속 세그먼트 수와 도착 튜브 빈 칸 중 적은 만큼만 이동
            int count = Math.Min(src.TopRunLength, dst.FreeSpace);

            // 세그먼트를 하나씩 옮긴다
            for (int i = 0; i < count; i++)
            {
                src.Pop();
                dst.Push(color);
            }

            // 이동 기록을 히스토리에 저장 (Undo용)
            _history.Push(new Move(from, to, count, color));
            return count;
        }

        /// <summary>
        /// 마지막 이동을 되돌린다.
        /// 히스토리가 없으면 false 반환.
        /// </summary>
        public bool TryUndo()
        {
            if (!CanUndo) return false;

            Move move = _history.Pop();

            // 역방향으로 세그먼트를 돌려놓는다 (to → from)
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
