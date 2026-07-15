// 레벨 진행 상태를 저장하고 불러오는 유틸리티 클래스
// PlayerPrefs를 사용해 클리어 여부를 기기에 저장한다.
// static 클래스라 어디서든 LevelProgressManager.IsClear(index) 형태로 사용 가능하다.
//
// 재설치 감지: 앱을 삭제 후 재설치하면 Android auto-backup이 PlayerPrefs를 복원해
// 진행도가 남아있을 수 있다. NO.1과 동일하게 firstInstallTime을 비교해 재설치를 감지하고
// PlayerPrefs를 초기화한다 ("삭제 = 처음부터" 유저 기대 부합, 개발 중 테스트도 편함).

using UnityEngine;

namespace WaterSortPuzzle.UI
{
    public static class LevelProgressManager
    {
        // PlayerPrefs 키 prefix. "Level_0_Clear", "Level_1_Clear" 형태로 저장된다.
        private const string ClearKeyPrefix = "Level_";
        private const string ClearKeySuffix = "_Clear";

        // 재설치 감지용 마지막 firstInstallTime 저장 키
        private const string InstallTimeKey = "InstallTime";

        // 앱 시작 시 첫 씬 로드 전에 자동 실행. Loading 씬을 거치지 않고
        // 개발 중 특정 씬을 바로 Play해도 재설치 체크가 보장된다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CheckReinstallOnAppStart()
        {
            ClearPrefsIfReinstalled();
        }

        // Android 재설치 감지: firstInstallTime이 달라지면 새로 설치된 것 → PlayerPrefs 초기화.
        // Google 자동 백업이 PlayerPrefs를 복원해도 firstInstallTime은 새 값이므로 정확히 감지됨.
        // Editor에선 아무 것도 안 함 (테스트 데이터 유지).
        private static void ClearPrefsIfReinstalled()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            long currentInstallTime = GetFirstInstallTime();
            long storedInstallTime  = long.Parse(PlayerPrefs.GetString(InstallTimeKey, "0"));

            if (currentInstallTime != storedInstallTime)
            {
                Debug.Log($"[LevelProgressManager] 재설치 감지 (저장={storedInstallTime}, 현재={currentInstallTime}). PlayerPrefs 전체 초기화.");
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetString(InstallTimeKey, currentInstallTime.ToString());
                PlayerPrefs.Save();
            }
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android PackageManager를 통해 앱의 firstInstallTime을 조회한다.
        // 이 값은 재설치할 때마다 새 값이 되며 Google auto-backup으로도 복원되지 않는다.
        private static long GetFirstInstallTime()
        {
            try
            {
                using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var pm       = activity.Call<AndroidJavaObject>("getPackageManager");
                string    pkg      = activity.Call<string>("getPackageName");
                using var info     = pm.Call<AndroidJavaObject>("getPackageInfo", pkg, 0);
                return info.Get<long>("firstInstallTime");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[LevelProgressManager] firstInstallTime 조회 실패: " + e.Message);
                return 0L;
            }
        }
#endif

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
