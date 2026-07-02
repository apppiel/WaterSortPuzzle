namespace WaterSortPuzzle.Core
{
    public static class WinChecker
    {
        // 모든 튜브가 비어 있거나, 가득 차고 단일 색인 상태면 승리
        public static bool IsWon(Board board)
        {
            for (int i = 0; i < board.TubeCount; i++)
            {
                Tube tube = board.GetTube(i);
                if (!tube.IsEmpty && !tube.IsComplete)
                    return false;
            }
            return true;
        }
    }
}
