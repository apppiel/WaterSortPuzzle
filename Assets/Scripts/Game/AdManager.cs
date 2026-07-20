using System;
using UnityEngine;
using GoogleMobileAds.Api;
#if UNITY_IOS
using System.Runtime.InteropServices;
using AOT;
#endif

namespace WaterSortPuzzle.Game
{
    // AdMob 광고(전면·보상형)를 관리하는 싱글턴.
    // GameManager.Start()에서 생성되며 씬 전환 후에도 유지된다.
    public class AdManager : MonoBehaviour
    {
        // 싱글턴 인스턴스
        public static AdManager Instance { get; private set; }

#if UNITY_IOS
        // ATTBridge.mm 의 네이티브 함수. iOS 14+ 광고 personalization 승인 요청.
        // Apple 정책상 AdMob 초기화 전에 반드시 호출되어야 한다.
        private delegate void ATTCallback(int status);
        [DllImport("__Internal")]
        private static extern void _RequestATTPermission(ATTCallback callback);

        // P/Invoke 콜백은 인스턴스 캡처가 불가하므로 static 인스턴스 참조로 우회.
        private static AdManager _iosInstance;

        [MonoPInvokeCallback(typeof(ATTCallback))]
        private static void OnATTComplete(int status)
        {
            // ATT 결과와 무관하게 AdMob 초기화는 진행 (거부돼도 non-personalized 광고는 서빙됨)
            MobileAds.Initialize(_ =>
            {
                _iosInstance.LoadInterstitial();
                _iosInstance.LoadRewarded();
            });
        }
#endif

        // Google 공식 테스트 광고 ID (Development Build 전용)
        private const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        private const string TestRewardedId     = "ca-app-pub-3940256099942544/5224354917";

#if UNITY_ANDROID
        // Android 실제 광고 단위 ID
        private const string RealInterstitialId = "ca-app-pub-3079888946602647/4715758711";
        private const string RealRewardedId     = "ca-app-pub-3079888946602647/2117515532";
#elif UNITY_IOS
        // iOS 실제 광고 단위 ID
        private const string RealInterstitialId = "ca-app-pub-3079888946602647/5277463126";
        private const string RealRewardedId     = "ca-app-pub-3079888946602647/8786865662";
#else
        private const string RealInterstitialId = "unused";
        private const string RealRewardedId     = "unused";
#endif

        // Development Build이면 테스트 ID, 릴리즈면 실제 ID 사용
        // MobileAds.Initialize 콜백은 non-main thread에서 실행돼 Debug.isDebugBuild를 직접 부를 수 없다.
        // Awake(메인 스레드)에서 캐싱해두고 이후에는 필드만 참조한다.
        private string _interstitialAdUnitId;
        private string _rewardedAdUnitId;

        private InterstitialAd _interstitialAd;
        private RewardedAd     _rewardedAd;

        // 전면 광고 게이팅: N번 요청마다 한 번만 실제 표시.
        // 매 레벨 클리어마다 광고 뜨면 짜증나므로 3라운드마다 한 번씩만 노출.
        // 앱 실행 세션 내에서 유지되며(DontDestroyOnLoad), 앱 재시작 시 0으로 리셋 → 초반 몇 판은 광고 없이 편하게.
        private int _interstitialGateCount = 0;
        private const int InterstitialInterval = 3;

        // 싱글턴 설정 및 AdMob 초기화
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 메인 스레드에서 광고 단위 ID를 결정해 캐싱
            _interstitialAdUnitId = Debug.isDebugBuild ? TestInterstitialId : RealInterstitialId;
            _rewardedAdUnitId     = Debug.isDebugBuild ? TestRewardedId     : RealRewardedId;

#if UNITY_IOS && !UNITY_EDITOR
            // iOS 실기: ATT 권한 팝업 먼저, 응답 후 콜백에서 AdMob 초기화.
            // Editor 는 네이티브 함수 없으므로 아래 else 브랜치로 폴백해 EntryPointNotFoundException 회피.
            _iosInstance = this;
            _RequestATTPermission(OnATTComplete);
#else
            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
            });
#endif
        }

        // 전면 광고를 미리 로드한다.
        private void LoadInterstitial()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;

            InterstitialAd.Load(_interstitialAdUnitId, new AdRequest(),
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogWarning($"전면 광고 로드 실패: {error.GetMessage()}");
                        return;
                    }
                    _interstitialAd = ad;
                });
        }

        // 보상형 광고를 미리 로드한다.
        private void LoadRewarded()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;

            RewardedAd.Load(_rewardedAdUnitId, new AdRequest(),
                (RewardedAd ad, LoadAdError error) =>
                {
                    if (error != null)
                    {
                        Debug.LogWarning($"보상형 광고 로드 실패: {error.GetMessage()}");
                        return;
                    }
                    _rewardedAd = ad;
                });
        }

        // 전면 광고를 게이팅해서 표시한다 (매 N번 요청마다 한 번만 실제 표시).
        // GameManager 의 "다음 레벨" 흐름처럼 매번 호출되는 지점에서 이걸 사용.
        // 게이팅에 걸린 경우 광고 없이 onClosed 만 즉시 호출.
        public void ShowInterstitialGated(Action onClosed)
        {
            _interstitialGateCount++;
            if (_interstitialGateCount < InterstitialInterval)
            {
                onClosed?.Invoke();
                return;
            }
            _interstitialGateCount = 0;
            ShowInterstitial(onClosed);
        }

        // 전면 광고를 표시한다.
        // 광고가 준비되지 않거나 표시 실패해도 onClosed를 반드시 호출해 흐름을 끊지 않는다.
        public void ShowInterstitial(Action onClosed)
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                void OnClosed()
                {
                    _interstitialAd.OnAdFullScreenContentClosed -= OnClosed;
                    _interstitialAd.OnAdFullScreenContentFailed -= OnFailed;
                    LoadInterstitial();
                    onClosed?.Invoke();
                }
                void OnFailed(AdError _)
                {
                    _interstitialAd.OnAdFullScreenContentClosed -= OnClosed;
                    _interstitialAd.OnAdFullScreenContentFailed -= OnFailed;
                    LoadInterstitial();
                    onClosed?.Invoke();
                }
                _interstitialAd.OnAdFullScreenContentClosed += OnClosed;
                _interstitialAd.OnAdFullScreenContentFailed += OnFailed;
                _interstitialAd.Show();
            }
            else
            {
                // 광고 없으면 바로 다음 동작 실행
                onClosed?.Invoke();
                LoadInterstitial();
            }
        }

        // 보상형 광고를 표시한다.
        // onRewarded: 리셋 등 유저 액션. **광고보다 먼저** 즉시 실행됨.
        //
        // 순서를 "리셋 → 광고" 로 뒤집은 이유:
        //   실기(갤럭시)에서 AdMob OnAdFullScreenContentClosed 가 발화 안 되는 케이스가
        //   있어 광고 close 이벤트에 리셋을 걸면 두 번째 클릭에서만 리셋되는 버그 발생.
        //   OnApplicationFocus 백업 경로도 잡히지 않는 실기가 있음.
        //   → 이벤트 의존 자체를 없애고, 유저 액션은 즉시 완료. 광고는 그 다음에 표시.
        //   UX 관점에선 "광고 봤더니 새 판이더라" 로 자연스러움.
        //   광고 수익도 유지됨 (Show 는 여전히 호출).
        public void ShowRewarded(Action onRewarded)
        {
            // 1. 유저 액션 즉시 실행 (씬 리로드는 이번 프레임 끝에 실제 반영)
            onRewarded?.Invoke();

            // 2. 그 위에 광고 표시. AdMob 이벤트에 의존하지 않으니 콜백 트래킹 불필요.
            //    광고 닫히면 다음 광고 로드만 하면 됨.
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                var localAd = _rewardedAd;
                void OnClosed()
                {
                    localAd.OnAdFullScreenContentClosed -= OnClosed;
                    UnityEngine.Time.timeScale = 1f;
                    LoadRewarded();
                }
                localAd.OnAdFullScreenContentClosed += OnClosed;
                // AdMob API 요구사항: Show 는 반드시 Action<Reward> 콜백 인자를 받는다 (미사용)
                localAd.Show(_ => { });
            }
            else
            {
                // 광고 준비 안 됐어도 리셋은 이미 위에서 실행됨. 여기선 다음 광고 로드만.
                LoadRewarded();
            }
        }
    }
}
