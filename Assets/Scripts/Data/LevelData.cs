using System;
using UnityEngine;

namespace WaterSortPuzzle.Data
{
    // 레벨 하나의 초기 설정 데이터.
    // ScriptableObject라서 Unity 에디터에서 에셋으로 만들고 인스펙터에서 편집할 수 있다.
    // 우클릭 → Create → WaterSortPuzzle → LevelData 로 생성.
    [CreateAssetMenu(fileName = "LevelData", menuName = "WaterSortPuzzle/LevelData")]
    public class LevelData : ScriptableObject
    {
        // 튜브 하나당 최대 세그먼트 수 (모든 튜브 동일)
        public int tubeCapacity = 4;

        // ColorId → 실제 Unity Color 매핑 배열.
        // palette[0] = ColorId 0의 색, palette[1] = ColorId 1의 색, ...
        // ※ 반드시 Alpha를 1(255)로 설정할 것. 기본값이 Alpha=0 이라 투명하게 보임.
        public Color[] palette;

        // 각 튜브의 초기 세그먼트 배열. 빈 튜브는 segments를 비워두면 된다.
        public TubeInitData[] tubes;
    }

    // 튜브 하나의 초기 세그먼트 데이터.
    // [Serializable]이 있어야 Unity 인스펙터에서 편집 가능하다.
    [Serializable]
    public struct TubeInitData
    {
        // ColorId 배열. index 0이 가장 아래 세그먼트.
        // 예) [0, 1, 2, 0] = 아래부터 빨강·파랑·노랑·빨강
        public int[] segments;
    }
}
