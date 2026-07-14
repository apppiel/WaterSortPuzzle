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
        private string InterstitialAdUnitId => Debug.isDebugBuild ? TestInterstitialId : RealInterstitialId;
        private string RewardedAdUnitId     => Debug.isDebugBuild ? TestRewardedId     : RealRewardedId;

        private InterstitialAd _interstitialAd;
        private RewardedAd     _rewardedAd;

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

            InterstitialAd.Load(InterstitialAdUnitId, new AdRequest(),
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

            RewardedAd.Load(RewardedAdUnitId, new AdRequest(),
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
        // onRewarded: 광고가 닫힌 후 보상이 확정되었을 때 호출 (리셋 실행)
        // onFailed: 광고가 없거나 로드 중일 때 호출
        public void ShowRewarded(Action onRewarded, Action onFailed = null)
        {
            if (_rewardedAd != null && _rewardedAd.CanShowAd())
            {
                bool earned = false;

                void OnClosed()
                {
                    _rewardedAd.OnAdFullScreenContentClosed -= OnClosed;
                    // AdMob이 광고 중 일시정지한 게임을 반드시 재개한다
                    UnityEngine.Time.timeScale = 1f;
                    LoadRewarded();
                    if (earned) onRewarded?.Invoke();
                }

                _rewardedAd.OnAdFullScreenContentClosed += OnClosed;
                // 보상 획득 시점(광고 완료)에 플래그만 세운다 — 실제 실행은 광고 닫힌 후
                _rewardedAd.Show(_ => earned = true);
            }
            else
            {
                Debug.LogWarning("보상형 광고가 준비되지 않았습니다.");
                onFailed?.Invoke();
                LoadRewarded();
            }
        }
    }
}
