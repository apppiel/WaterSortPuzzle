// 게임 설정값을 PlayerPrefs에 저장하고 불러오는 정적 클래스.
// 씬이 바뀌어도 설정이 유지된다.

using UnityEngine;

namespace WaterSortPuzzle.UI
{
    public static class SettingsManager
    {
        private const string KeySFX = "SFX_Enabled"; // PlayerPrefs 키

        // 효과음 켜짐 여부. 기본값 true (1)
        public static bool SFXEnabled
        {
            get => PlayerPrefs.GetInt(KeySFX, 1) == 1;
            set
            {
                PlayerPrefs.SetInt(KeySFX, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
