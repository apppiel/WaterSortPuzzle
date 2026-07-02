namespace WaterSortPuzzle.Core
{
    // 한 번의 붓기 이동을 기록하는 구조체.
    // Undo 기능을 위해 Board의 히스토리 스택에 쌓인다.
    // struct(구조체)라서 힙 할당 없이 값으로 복사된다.
    public readonly struct Move
    {
        // 출발 튜브 인덱스
        public readonly int From;

        // 도착 튜브 인덱스
        public readonly int To;

        // 옮긴 세그먼트 수
        public readonly int Count;

        // 옮긴 색의 ColorId (Undo 시 역방향으로 돌려놓기 위해 저장)
        public readonly int Color;

        public Move(int from, int to, int count, int color)
        {
            From = from;
            To = to;
            Count = count;
            Color = color;
        }
    }
}
