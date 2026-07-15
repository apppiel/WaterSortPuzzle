using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace WaterSortPuzzle.Game
{
    // 마지막 레벨(100) 클리어 시 인증코드를 발급하고 Firestore에 저장한다.
    // NO.1(NANA_puzzle)의 RewardManager와 동일한 스키마를 사용:
    //   - rewards/{deviceId}   : 기기당 1개 (중복 발급 방지 목적)
    //   - code_index/{code}    : 코드로 역조회하기 위한 인덱스 (CS 대응용)
    // Firebase 프로젝트는 게임별로 분리(nana-no2). NO.1과 데이터가 섞이지 않는다.
    // UI는 이 컴포넌트에 붙지 않는다. OnCodeIssued 이벤트를 구독해서 팝업이 뜨는 구조.
    public class RewardManager : MonoBehaviour
    {
        // 코드 발급 결과 이벤트. 다음 세션에서 uGUI 팝업이 이걸 구독한다.
        //   code   : 발급된 XXXX-XXXX 형식 문자열 (진행 중일 땐 빈 문자열)
        //   status : 유저에게 보여줄 상태 메시지
        //   isReissue : true면 기존 발급 코드 재사용 (같은 기기가 다시 클리어)
        public event Action<string, string, bool> OnCodeIssued;

        private FirebaseFirestore _db;        // Firestore 인스턴스 (초기화 후 세팅)
        private bool _firebaseReady;          // 의존성 체크 완료 여부

        // Firebase 초기화를 앱 시작 시점에 미리 시작.
        // CheckAndFixDependenciesAsync 콜백은 non-main thread에서 올 수 있으므로
        // ContinueWithOnMainThread로 메인 스레드에서 처리해야 UnityException을 피한다.
        private void Awake()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    _db = FirebaseFirestore.DefaultInstance;
                    _firebaseReady = true;
                    Debug.Log("[RewardManager] Firebase Firestore 준비 완료");
                }
                else
                {
                    Debug.LogError("[RewardManager] Firebase 초기화 실패: " + task.Result);
                }
            });
        }

        // 마지막 레벨 클리어 시 GameManager에서 호출.
        // 같은 기기에 이미 발급 이력이 있으면 그 코드를 재사용, 없으면 새로 발급.
        public void IssueCode()
        {
            // 기기 고유 ID를 문서 키로 → 같은 기기는 항상 같은 코드
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            if (!_firebaseReady)
            {
                // Firebase 초기화 실패/네트워크 문제 시에도 코드는 보여줌 (스크린샷용).
                // 다만 서버 저장이 안 됐음을 유저에게 알림.
                string offlineCode = GenerateCode();
                OnCodeIssued?.Invoke(offlineCode, "네트워크 오류 - 코드를 메모해 두세요", false);
                Debug.LogWarning("[RewardManager] Firebase 미준비 상태에서 코드 발급: " + offlineCode);
                return;
            }

            OnCodeIssued?.Invoke("", "코드 생성 중...", false);

            _db.Collection("rewards").Document(deviceId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully && task.Result.Exists)
                {
                    // 같은 기기가 이미 코드 받은 적 있음 → 같은 코드 재사용
                    string existing = task.Result.GetValue<string>("code");
                    OnCodeIssued?.Invoke(existing, "이미 발급된 코드입니다", true);
                    Debug.Log("[RewardManager] 기존 코드 재사용: " + existing);
                    return;
                }

                // 새 코드 생성 후 두 컬렉션에 저장
                string newCode = GenerateCode();

                var rewardData = new Dictionary<string, object>
                {
                    { "code",      newCode },
                    { "claimed",   false },
                    { "deviceId",  deviceId },
                    { "createdAt", FieldValue.ServerTimestamp }
                };
                var indexData = new Dictionary<string, object>
                {
                    { "deviceId",  deviceId },
                    { "createdAt", FieldValue.ServerTimestamp }
                };

                var rewardsTask = _db.Collection("rewards").Document(deviceId).SetAsync(rewardData);
                var indexTask   = _db.Collection("code_index").Document(newCode).SetAsync(indexData);

                Task.WhenAll(rewardsTask, indexTask).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsCompletedSuccessfully)
                    {
                        OnCodeIssued?.Invoke(newCode, "코드가 발급되었습니다!", false);
                        Debug.Log("[RewardManager] 새 코드 발급 성공: " + newCode);
                    }
                    else
                    {
                        // 저장 실패해도 코드는 유저에게 보여줌 (스크린샷 등으로 메모 가능)
                        OnCodeIssued?.Invoke(newCode, "저장 실패 - 코드를 메모해 두세요", false);
                        Debug.LogError("[RewardManager] Firestore 저장 실패: " + saveTask.Exception);
                    }
                });
            });
        }

        // 예: A3K9-XZ21 형식. 헷갈리는 문자(0,O,1,I) 제외한 32자 charset 사용.
        // 8자리 → 32^8 ≈ 1.1조 조합. 실전 스케일에서 충돌 확률 무시 가능.
        private string GenerateCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = new System.Random();
            char[] code = new char[9];
            for (int i = 0; i < 4; i++) code[i] = chars[rng.Next(chars.Length)];
            code[4] = '-';
            for (int i = 5; i < 9; i++) code[i] = chars[rng.Next(chars.Length)];
            return new string(code);
        }
    }
}
