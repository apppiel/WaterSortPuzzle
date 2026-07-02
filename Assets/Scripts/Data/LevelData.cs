using System;
using UnityEngine;

namespace WaterSortPuzzle.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "WaterSortPuzzle/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int tubeCapacity = 4;
        public Color[] palette;
        public TubeInitData[] tubes;
    }

    [Serializable]
    public struct TubeInitData
    {
        // ColorId 배열, 아래(index 0)부터 위 순서. 빈 칸은 배열 길이로 표현.
        public int[] segments;
    }
}
