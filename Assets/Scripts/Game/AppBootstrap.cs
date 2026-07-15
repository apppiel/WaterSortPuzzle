using Firebase;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace WaterSortPuzzle.Game
{
    // 앱 부팅 시점에 Firebase 관련 초기 설정을 한 번만 수행한다.
    //   - Firebase.CheckAndFixDependenciesAsync 로 SDK 준비 확인
    //   - Crashlytics: 잡히지 않은 C# 예외까지 fatal 크래시로 리포트하도록 설정
    //     (Unity 기본은 unhandled exception 을 non-fatal 로 리포트. 이 옵션을 켜면
    //      실제 크래시처럼 Firebase Console 최상단 지표에 반영됨)
    //   - Analytics: SDK 로드만으로 자동 이벤트(session_start, first_open 등) 수집 시작.
    //     별도 초기화 코드 불필요. 커스텀 이벤트는 각 관리자에서 FirebaseAnalytics.LogEvent 호출.
    //
    // RuntimeInitializeOnLoadMethod(BeforeSceneLoad) 로 첫 씬 로드 전에 실행되므로,
    // Loading 씬을 건너뛰고 특정 씬을 바로 Play 해도 부트 코드가 걸린다.
    // RewardManager 도 Firebase.CheckAndFixDependenciesAsync 를 호출하는데, 이 호출은
    // 두 번째부터는 캐시된 결과를 즉시 반환하므로 중복 오버헤드 없음.
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                    Debug.Log("[AppBootstrap] Firebase + Crashlytics 준비 완료");
                }
                else
                {
                    Debug.LogError("[AppBootstrap] Firebase 초기화 실패: " + task.Result);
                }
            });
        }
    }
}
