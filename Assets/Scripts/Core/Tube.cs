using System;

namespace WaterSortPuzzle.Core
{
    public class Tube
    {
        private readonly int[] _segments;
        private int _count;

        public int Capacity { get; }
        public int Count => _count;
        public bool IsEmpty => _count == 0;
        public bool IsFull => _count == Capacity;
        public int FreeSpace => Capacity - _count;

        public int TopColor => _count > 0 ? _segments[_count - 1] : -1;

        public int TopRunLength
        {
            get
            {
                if (_count == 0) return 0;
                int color = _segments[_count - 1];
                int run = 0;
                for (int i = _count - 1; i >= 0 && _segments[i] == color; i--)
                    run++;
                return run;
            }
        }

        public bool IsComplete => IsFull && TopRunLength == Capacity;

        public Tube(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
            _segments = new int[capacity];
        }

        // bottomToTop: index 0 = 가장 아래 세그먼트
        public Tube(int capacity, int[] bottomToTop) : this(capacity)
        {
            if (bottomToTop.Length > capacity)
                throw new ArgumentException("초기 세그먼트가 용량을 초과합니다.");
            for (int i = 0; i < bottomToTop.Length; i++)
                _segments[i] = bottomToTop[i];
            _count = bottomToTop.Length;
        }

        public bool CanAccept(int color) => !IsFull && (IsEmpty || TopColor == color);

        public int GetSegment(int index) => _segments[index];

        internal void Push(int color) => _segments[_count++] = color;

        internal int Pop() => _segments[--_count];
    }
}
