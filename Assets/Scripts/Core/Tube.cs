using System;

namespace WaterSortPuzzle.Core
{
    /// <summary>
    /// 튜브 하나를 나타낸다.
    /// 세그먼트(색 조각)를 아래부터 쌓는 스택 구조.
    /// ColorId(정수)로 색을 표현하고, UnityEngine을 참조하지 않는 순수 C# 클래스.
    /// </summary>
    public class Tube
    {
        // 세그먼트를 저장하는 배열. index 0 = 가장 아래, index Count-1 = 가장 위
        private readonly int[] _segments;

        // 현재 쌓인 세그먼트 수
        private int _count;

        /// <summary>튜브의 최대 수용 칸 수</summary>
        public int Capacity { get; }

        /// <summary>현재 쌓인 세그먼트 수</summary>
        public int Count => _count;

        /// <summary>비어 있으면 true</summary>
        public bool IsEmpty => _count == 0;

        /// <summary>꽉 찼으면 true</summary>
        public bool IsFull => _count == Capacity;

        /// <summary>남은 빈 칸 수</summary>
        public int FreeSpace => Capacity - _count;

        /// <summary>
        /// 맨 위 세그먼트의 ColorId. 비어 있으면 -1 반환.
        /// </summary>
        public int TopColor => _count > 0 ? _segments[_count - 1] : -1;

        /// <summary>
        /// 맨 위부터 연속으로 같은 색인 세그먼트 수.
        /// 예) [R, G, G, G] → 3
        /// </summary>
        public int TopRunLength
        {
            get
            {
                if (_count == 0) return 0;

                int color = _segments[_count - 1]; // 맨 위 색
                int run = 0;

                // 위에서 아래로 내려가며 같은 색이 몇 개 연속인지 셈
                for (int i = _count - 1; i >= 0 && _segments[i] == color; i--)
                    run++;

                return run;
            }
        }

        /// <summary>
        /// 클리어 상태인지 확인.
        /// 꽉 차 있고 전부 같은 색이면 true.
        /// </summary>
        public bool IsComplete => IsFull && TopRunLength == Capacity;

        /// <summary>빈 튜브를 만든다.</summary>
        public Tube(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
            _segments = new int[capacity];
        }

        /// <summary>
        /// 초기 세그먼트가 채워진 튜브를 만든다.
        /// bottomToTop: index 0이 가장 아래 세그먼트.
        /// </summary>
        public Tube(int capacity, int[] bottomToTop) : this(capacity)
        {
            if (bottomToTop.Length > capacity)
                throw new ArgumentException("초기 세그먼트가 용량을 초과합니다.");

            for (int i = 0; i < bottomToTop.Length; i++)
                _segments[i] = bottomToTop[i];

            _count = bottomToTop.Length;
        }

        /// <summary>
        /// 이 튜브가 해당 색을 받을 수 있는지 확인.
        /// 비어 있거나, 맨 위 색이 같고 자리가 있으면 true.
        /// </summary>
        public bool CanAccept(int color) => !IsFull && (IsEmpty || TopColor == color);

        /// <summary>index 위치의 세그먼트 ColorId를 반환한다. (0 = 가장 아래)</summary>
        public int GetSegment(int index) => _segments[index];

        /// <summary>맨 위에 색을 추가한다. (Board에서만 호출)</summary>
        internal void Push(int color) => _segments[_count++] = color;

        /// <summary>맨 위 색을 꺼내어 반환한다. (Board에서만 호출)</summary>
        internal int Pop() => _segments[--_count];
    }
}
