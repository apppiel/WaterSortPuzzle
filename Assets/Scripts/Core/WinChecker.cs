namespace WaterSortPuzzle.Core
{
    // 승리 조건을 판정하는 유틸리티 클래스.
    // 인스턴스 없이 IsWon 메서드만 호출하면 된다.
    public static class WinChecker
    {
        // 모든 튜브가 비어 있거나, 가득 차고 단일 색인 상태면 승리.
        // 하나라도 "부분적으로 채워진" 튜브가 있으면 아직 미완성.
        public static bool IsWon(Board board)
        {
            for (int i = 0; i < board.TubeCount; i++)
            {
                Tube tube = board.GetTube(i);

                // 비어 있는 튜브 → OK
                // 가득 차고 단일 색(IsComplete) → OK
                // 그 외(부분 채움, 혼합 색) → 아직 미완성
                if (!tube.IsEmpty && !tube.IsComplete)
                    return false;
            }

            return true;
        }
    }
}
