using System;
using UnityEngine;

namespace WaterSortPuzzle.Data
{
    /// <summary>
    /// 레벨 하나의 초기 설정 데이터.
    /// ScriptableObject라서 Unity 에디터에서 에셋으로 만들고 인스펙터에서 편집할 수 있다.
    /// Assets/Levels/ 폴더에 저장한다.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "WaterSortPuzzle/LevelData")]
    public class LevelData : ScriptableObject
    {
        /// <summary>튜브 하나당 최대 세그먼트 수 (모든 튜브 동일)</summary>
        public int tubeCapacity = 4;

        /// <summary>
        /// ColorId → 실제 Unity Color 매핑 배열.
        /// palette[0] = ColorId 0의 색, palette[1] = ColorId 1의 색, ...
        /// 반드시 Alpha를 1(255)로 설정할 것. 기본값이 Alpha=0이라 투명하게 보임.
        /// </summary>
        public Color[] palette;

        /// <summary>
        /// 각 튜브의 초기 세그먼트 배열.
        /// 빈 튜브는 segments를 비워두면 된다.
        /// </summary>
        public TubeInitData[] tubes;
    }

    /// <summary>
    /// 튜브 하나의 초기 세그먼트 데이터.
    /// [Serializable]이 있어야 Unity 인스펙터에서 편집 가능하다.
    /// </summary>
    [Serializable]
    public struct TubeInitData
    {
        /// <summary>
        /// ColorId 배열. index 0이 가장 아래 세그먼트.
        /// 예) [0, 1, 2, 0] = 아래부터 빨강·파랑·노랑·빨강
        /// </summary>
        public int[] segments;
    }
}
