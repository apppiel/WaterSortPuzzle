namespace WaterSortPuzzle.Core
{
    public readonly struct Move
    {
        public readonly int From;
        public readonly int To;
        public readonly int Count;
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
