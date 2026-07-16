using UnityEngine;
using DG.Tweening;

namespace WaterSortPuzzle.Game
{
    // 첫 이동을 손 아이콘으로 유도하는 튜토리얼.
    // GameManager 가 Level 1 진입 시 생성한다. 첫 pour 성공 시 손이 사라지고
    // PlayerPrefs 에 완료 플래그를 저장해 이후 실행에서는 다시 뜨지 않는다.
    //
    // World space SpriteRenderer 를 쓰므로 튜브(같은 World space)와 좌표계가 통일된다.
    // TubeView 의 sortingOrder 는 최대 20 이라 100 을 주면 안전하게 위에 렌더링됨.
    public class TutorialManager : MonoBehaviour
    {
        // 완료 플래그 저장 키
        private const string TutorialDoneKey = "tutorial_done";

        // 이미 튜토리얼을 봤는지 여부 (GameManager 가 스폰 전 확인)
        public static bool IsDone => PlayerPrefs.GetInt(TutorialDoneKey, 0) == 1;

        // 손 아이콘 SpriteRenderer (자식 GameObject 로 생성)
        private SpriteRenderer _hand;
        // Pulse 트윈 (완료 시 Kill 하려고 참조 보관)
        private Tween _pulseTween;
        // 손 위치를 옮길 대상 튜브 Transform
        private Transform _toTube;
        // 유저가 정확히 이 인덱스의 튜브를 눌렀을 때만 손을 이동시킨다.
        // 예전엔 아무 튜브나 누르면 손이 움직여 "왜 저기로 가지?" 하는 혼란이 있었다.
        private int _fromIdx;

        // 상태 머신
        //   0 = FROM 튜브 클릭 대기 (손이 FROM 위에서 pulse)
        //   1 = 유저가 첫 튜브 선택 → 손이 TO 로 이동, pour 대기
        //   2 = pour 완료, 손 사라짐
        private int _state;

        // 손 크기 (튜브 대비 적절한 비율)
        private const float HandScale = 0.6f;
        // 튜브 좌표에서 손 위치 오프셋 (아래쪽 + 살짝 오른쪽에 배치)
        private static readonly Vector3 HandOffset = new Vector3(0.35f, -1.1f, 0f);

        // 튜토리얼 시작. GameManager 에서 호출.
        //   handSprite : 손 스프라이트 (Cursor_Hand3 등)
        //   fromIdx    : 유저가 처음 눌러야 할 튜브의 인덱스 (매칭 검증용)
        //   fromTube   : 그 튜브의 Transform (손 초기 위치)
        //   toTube     : 유저가 두 번째로 눌러야 할 튜브의 Transform
        public void StartTutorial(Sprite handSprite, int fromIdx, Transform fromTube, Transform toTube)
        {
            if (handSprite == null || fromTube == null || toTube == null)
            {
                Debug.LogWarning("[TutorialManager] 필수 인자 누락 → 튜토리얼 스킵");
                Destroy(gameObject);
                return;
            }

            _fromIdx = fromIdx;
            _toTube  = toTube;

            // 자식 GameObject 에 SpriteRenderer 를 붙여 손 아이콘 생성
            var handGo = new GameObject("TutorialHand");
            handGo.transform.SetParent(transform);
            _hand = handGo.AddComponent<SpriteRenderer>();
            _hand.sprite = handSprite;
            _hand.sortingOrder = 100; // 튜브/세그먼트(최대 20) 위

            _hand.transform.position   = fromTube.position + HandOffset;
            _hand.transform.localScale = Vector3.one * HandScale;

            // Tap 느낌의 pulse: 살짝 커졌다 작아지기를 무한 반복
            Vector3 baseScale = _hand.transform.localScale;
            _pulseTween = _hand.transform
                .DOScale(baseScale * 0.82f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // GameManager 에서 튜브가 처음 선택됐을 때 호출.
        // 힌트 튜브(_fromIdx)를 정확히 눌렀을 때만 손을 TO 로 이동시킨다.
        // 다른 튜브를 누르면 손은 그대로 pulse 중 → 유저가 "아, 저기 눌러야 하는구나" 인지.
        public void NotifyTubeSelected(int tubeIdx)
        {
            if (_state != 0 || _hand == null) return;
            if (tubeIdx != _fromIdx) return;
            _state = 1;

            _hand.transform.DOMove(_toTube.position + HandOffset, 0.5f)
                .SetEase(Ease.OutQuad);
        }

        // GameManager 에서 pour 성공 시 호출.
        // 손 페이드아웃 + 완료 플래그 저장 + 오브젝트 정리.
        public void NotifyPourSucceeded()
        {
            if (_state == 2 || _hand == null) return;
            _state = 2;

            _pulseTween?.Kill();

            // 페이드아웃 후 자기 자신 파괴.
            // 이 프로젝트의 DOTween 은 SpriteRenderer 용 DOFade/DOColor 확장이 없어
            // DOTween.To 로 color 프로퍼티를 직접 tween.
            Color start = _hand.color;
            Color end   = new Color(start.r, start.g, start.b, 0f);
            DOTween.To(() => _hand.color, c => _hand.color = c, end, 0.3f)
                .OnComplete(() =>
                {
                    if (this != null) Destroy(gameObject);
                });

            // 완료 플래그 저장 → 다음 실행부터는 IsDone == true
            PlayerPrefs.SetInt(TutorialDoneKey, 1);
            PlayerPrefs.Save();
        }

        // 오브젝트 파괴 시 트윈 정리 (씬 전환·강제 종료 대비)
        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }
    }
}
