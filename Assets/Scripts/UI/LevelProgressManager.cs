// 레벨 진행 상태를 저장하고 불러오는 유틸리티 클래스
// PlayerPrefs를 사용해 클리어 여부를 기기에 저장한다.
// static 클래스라 어디서든 LevelProgressManager.IsClear(index) 형태로 사용 가능하다.

using UnityEngine;

namespace WaterSortPuzzle.UI
{
    public static class LevelProgressManager
    {
        // PlayerPrefs 키 prefix. "Level_0_Clear", "Level_1_Clear" 형태로 저장된다.
        private const string ClearKeyPrefix = "Level_";
        private const string ClearKeySuffix = "_Clear";

        // 해당 레벨을 클리어했는지 반환한다
        public static bool IsClear(int levelIndex)
        {
            return PlayerPrefs.GetInt(GetKey(levelIndex), 0) == 1;
        }

        // 해당 레벨을 플레이할 수 있는지 반환한다
        // 0번 레벨은 항상 열려있고, 나머지는 이전 레벨 클리어 시 해금된다
        public static bool IsUnlocked(int levelIndex)
        {
            if (levelIndex == 0) return true;
            return IsClear(levelIndex - 1);
        }

        // 해당 레벨 클리어를 저장한다
        public static void SaveClear(int levelIndex)
        {
            PlayerPrefs.SetInt(GetKey(levelIndex), 1);
            PlayerPrefs.Save(); // 즉시 디스크에 기록
        }

        // PlayerPrefs 키 문자열을 만든다
        private static string GetKey(int levelIndex)
        {
            return ClearKeyPrefix + levelIndex + ClearKeySuffix;
        }
    }
}
