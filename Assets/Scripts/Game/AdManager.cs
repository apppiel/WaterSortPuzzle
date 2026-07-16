using System;
using UnityEngine;
using GoogleMobileAds.Api;

namespace WaterSortPuzzle.Game
{
    // AdMob 광고(전면·보상형)를 관리하는 싱글턴.
    // GameManager.Start()에서 생성되며 씬 전환 후에도 유지된다.
    public class AdManager : MonoBehaviour
    {
        // 싱글턴 인스턴스
        public static AdManager Instance { get; private set; }

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

        // 보상형 광고 종료 시 실행할 콜백 (리셋 등).
        // OnAdFullScreenContentClosed 가 일부 실기(갤럭시 등)에서 발화 안 되는 경우가 있어
        // OnApplicationFocus 백업 경로에서도 이 콜백을 발화한다. 둘 중 먼저 오는 쪽에서
        // 한 번만 실행되도록 발화 후 null 로 비운다 (idempotent).
        private Action _pendingRewardedCallback;
        // 보상형 광고가 실제로 화면에 올라와 앱 포커스를 뺏은 상태인지.
        // focus loss 를 관찰한 후에만 focus return 을 "광고 닫힘" 신호로 신뢰한다 (스퓨리어스 방지).
        private bool _rewardedAdOpened;

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

            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
            });
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
        // onRewarded: 광고가 닫히면 호출 (리셋 실행)
        // onFailed: 광고가 없거나 로드 중일 때 호출
        //
        // 콜백 발화 경로 (둘 중 먼저 오는 쪽이 승자, idempotent):
        //   1) AdMob OnAdFullScreenContentClosed — 정석 경로
        //   2) OnApplicationFocus 백업 — 일부 실기(갤럭시 확인)에서 (1)이 아예 안 오는 케이스 우회
        //      광고가 뜨면 앱이 포커스 잃음(false) → 광고 닫히면 포커스 복귀(true) OS 이벤트를 활용.
        //      이건 반드시 옴.
        //
        // 정책: 광고가 닫히면 유저가 스킵했든 끝까지 봤든 무조건 onRewarded 호출.
        //   HandleReset 의 "광고 실패 → 리셋 실행" 원칙과 통일 —
        //   유저의 클릭 의도가 광고 시청 여부보다 우선한다.
        public void ShowRewarded(Action onRewarded, Action onFailed = null)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                // pending 콜백 세팅. FirePendingRewardedCallback 가 호출되면 이걸 실행하고 null 로 비운다.
                _pendingRewardedCallback = () =>
                {
                    // AdMob이 광고 중 일시정지한 게임을 반드시 재개한다
                    UnityEngine.Time.timeScale = 1f;
                    LoadRewarded();
                    onRewarded?.Invoke();
                };
                _rewardedAdOpened = false;

                void OnClosed()
                {
                    _rewardedAd.OnAdFullScreenContentClosed -= OnClosed;
                    FirePendingRewardedCallback();
                }
                _rewardedAd.OnAdFullScreenContentClosed += OnClosed;
                // AdMob API 요구사항: Show 는 반드시 Action<Reward> 콜백 인자를 받는다 (미사용)
                _rewardedAd.Show(_ => { });
            }
            else
            {
                Debug.LogWarning("보상형 광고가 준비되지 않았습니다.");
                onFailed?.Invoke();
                LoadRewarded();
            }
        }

        // pending 콜백을 딱 한 번만 실행한다. AdMob close 이벤트 or focus 복귀 중 먼저 오는 쪽에서 호출.
        private void FirePendingRewardedCallback()
        {
            if (_pendingRewardedCallback == null) return;
            var cb = _pendingRewardedCallback;
            _pendingRewardedCallback = null;
            _rewardedAdOpened = false;
            cb();
        }

        // OS 레벨 포커스 이벤트 — AdMob close 이벤트가 안 오는 실기 대응 백업 경로.
        // 광고 표시 → 앱 포커스 상실 → 광고 닫힘 → 앱 포커스 복귀 흐름을 활용해
        // pending 콜백을 발화한다. focus loss 를 실제로 관찰한 뒤(_rewardedAdOpened == true)의
        // focus return 만 신뢰해서 알림 팝업 등 스퓨리어스 focus 이벤트로 오발화하지 않도록.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (_pendingRewardedCallback == null) return;

            if (!hasFocus)
            {
                _rewardedAdOpened = true;
            }
            else if (_rewardedAdOpened)
            {
                FirePendingRewardedCallback();
            }
        }
    }
}
