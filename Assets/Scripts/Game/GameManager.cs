using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using WaterSortPuzzle.Core;
using WaterSortPuzzle.Data;
using WaterSortPuzzle.UI;

namespace WaterSortPuzzle.Game
{
    // 게임 전체 흐름을 제어하는 MonoBehaviour.
    // LevelData를 읽어 Board(게임 상태)와 TubeView(화면)를 만들고,
    // 입력을 받아 Pour/Undo를 실행하고, 승리를 감지한다.
    public class GameManager : MonoBehaviour
    {
        // 인스펙터에서 드래그로 연결하는 전체 레벨 데이터 배열
        [SerializeField] private LevelData[] _levels;

        // 현재 플레이 중인 레벨 데이터 (Start에서 _levels[선택된 인덱스]로 설정됨)
        private LevelData _levelData;

        // 현재 레벨 번호 (0부터 시작)
        private int _currentLevelIndex;

        // 인스펙터에서 드래그로 연결하는 튜브 스프라이트 (Assets/Sprites/Frame 1.png)
        [SerializeField] private Sprite tubeSprite;

        // 인스펙터에서 드래그로 연결하는 한글 TMP 폰트 에셋 (NanumSquareRoundEB SDF)
        [SerializeField] private TMP_FontAsset koreanFont;


        // Core 게임 상태 (튜브 집합 + 이동 히스토리)
        private Board _board;

        // 화면에 보이는 튜브 뷰 배열
        private TubeView[] _tubeViews;

        // 현재 선택된 튜브 인덱스. 아무것도 선택 안 됐으면 -1
        private int _selectedIndex = -1;

        // 클리어 후 입력을 막기 위한 플래그
        private bool _gameOver;

        // 애니메이션 재생 중 입력을 막기 위한 플래그
        private bool _isAnimating;

        // 1x1 흰색 스프라이트 (TubeView와 애니메이션 오브젝트 모두 사용)
        private Sprite _square;

        // 클리어 팝업 UI
        private ClearPopup _clearPopup;

        // 세그먼트 한 칸의 월드 유닛 크기 (이 값 하나로 전체 크기 조절)
        private const float SegmentSize = 0.5f;

        // 튜브 사이 간격 (월드 유닛)
        private const float TubeSpacing = 0.8f;

        // 붓기 애니메이션 총 시간 (초)
        private const float PourDuration = 0.35f;

        // 씬 시작 시 한 번 호출된다. LevelData로 Board와 TubeView를 생성한다.
        private void Start()
        {
            // 레벨선택 씬에서 저장한 레벨 번호를 읽어온다. 없으면 0번(첫 레벨)
            _currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);

            // 유효한 인덱스인지 확인 후 레벨 데이터 설정
            if (_levels == null || _levels.Length == 0 || _currentLevelIndex >= _levels.Length)
            {
                Debug.LogError("LevelData가 설정되지 않았거나 인덱스가 범위를 벗어났습니다.");
                return;
            }
            _levelData = _levels[_currentLevelIndex];

            var tubes = BuildTubes();       // Core 데이터 생성
            _board = new Board(tubes);      // 게임 상태 초기화
            _tubeViews = BuildViews(tubes); // 화면 오브젝트 생성
            BuildClearPopup();              // 클리어 팝업 생성 (평소엔 숨김)
        }

        // 매 프레임 호출된다. 터치 및 마우스 클릭 입력을 처리한다.
        private void Update()
        {
            // 클리어 후 또는 애니메이션 중에는 입력 무시
            if (_gameOver || _isAnimating) return;

            // Pointer는 마우스와 터치스크린을 모두 추상화한다.
            // 에디터에서는 마우스, 모바일에서는 터치로 자동 전환된다.
            var pointer = Pointer.current;
            if (pointer == null) return;

            // 이번 프레임에 화면을 눌렀을 때
            if (pointer.press.wasPressedThisFrame)
            {
                // 스크린 좌표를 월드 좌표로 변환
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(pointer.position.ReadValue());

                // 클릭/터치한 위치에 Collider2D가 있는지 확인
                var hit = Physics2D.OverlapPoint(worldPos);

                // TubeClickTarget 컴포넌트가 있는 Collider면 튜브 클릭으로 처리
                if (hit != null && hit.TryGetComponent<TubeClickTarget>(out var target))
                    HandleTubeClicked(target.TubeIndex);
            }
        }

        // 튜브 클릭 시 호출된다.
        // 첫 클릭: 튜브 선택 / 두 번째 클릭: 붓기 시도 / 같은 튜브 재클릭: 선택 해제
        private void HandleTubeClicked(int index)
        {
            if (_selectedIndex == -1)
            {
                // 아무것도 선택 안 된 상태: 빈 튜브는 선택 불가
                if (_board.GetTube(index).IsEmpty) return;

                // 튜브 선택
                _selectedIndex = index;
                _tubeViews[index].SetSelected(true);
            }
            else if (_selectedIndex == index)
            {
                // 같은 튜브를 다시 클릭하면 선택 해제
                _tubeViews[index].SetSelected(false);
                _selectedIndex = -1;
            }
            else
            {
                int from = _selectedIndex;
                _tubeViews[from].SetSelected(false);
                _selectedIndex = -1;

                // 붓기 전에 애니메이션에 필요한 정보를 미리 저장한다
                // (TryPour 이후에는 Board 상태가 바뀌어 원래 값을 알 수 없음)
                int colorId = _board.GetTube(from).TopColor;
                Color animColor = _levelData.palette[colorId];
                Vector3 startPos = _tubeViews[from].GetSlotWorldPos(_board.GetTube(from).Count - 1);

                // Core에서 규칙 검증 + 이동 (Board 상태가 여기서 바뀜)
                int moved = _board.TryPour(from, index);

                if (moved > 0)
                {
                    // 붓기 후 도착 튜브의 새 맨 위 슬롯 위치
                    Vector3 endPos = _tubeViews[index].GetSlotWorldPos(_board.GetTube(index).Count - 1);

                    // 애니메이션 재생 (완료 후 Refresh와 승리 판정)
                    PlayPourAnimation(from, index, animColor, startPos, endPos);
                }
            }
        }

        // HUD의 Undo 버튼에서 호출한다. Board의 마지막 이동을 되돌리고 모든 뷰를 갱신한다.
        public void HandleUndo()
        {
            // 애니메이션 중에는 Undo 불가
            if (_isAnimating) return;
            if (!_board.TryUndo()) return;

            // 어느 튜브가 바뀌었는지 모르므로 전체 갱신
            foreach (var view in _tubeViews)
                view.Refresh();
        }

        // 세그먼트가 출발 튜브에서 도착 튜브로 날아가는 애니메이션을 재생한다.
        // 완료 후 양쪽 TubeView를 갱신하고 승리 조건을 확인한다.
        private void PlayPourAnimation(int from, int to, Color color, Vector3 startPos, Vector3 endPos)
        {
            _isAnimating = true;

            // 이동할 세그먼트를 표현하는 임시 오브젝트 생성
            var animGo = new GameObject("PourAnim");
            var sr = animGo.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = color;
            sr.sortingOrder = 20; // 튜브 배경(−10)과 슬롯(10)보다 앞에 렌더링
            animGo.transform.position = startPos;
            animGo.transform.localScale = Vector3.one * SegmentSize * 0.88f;

            // 포물선 궤적을 위한 중간 지점 (두 튜브 중간에서 위로 올라감)
            Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * 0.8f;

            // DOTween 시퀀스: 위로 올라갔다가 → 도착 위치로 내려옴
            DOTween.Sequence()
                .Append(animGo.transform.DOMove(midPos, PourDuration * 0.5f).SetEase(Ease.OutQuad))
                .Append(animGo.transform.DOMove(endPos, PourDuration * 0.5f).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    Destroy(animGo);                   // 임시 오브젝트 제거
                    _tubeViews[from].Refresh();        // 출발 튜브 화면 갱신
                    _tubeViews[to].Refresh();          // 도착 튜브 화면 갱신
                    _isAnimating = false;              // 입력 잠금 해제

                    // 승리 조건 확인
                    if (WinChecker.IsWon(_board))
                    {
                        _gameOver = true;
                        LevelProgressManager.SaveClear(_currentLevelIndex); // 클리어 저장
                        PlayClearSequence();
                    }
                });
        }

        // 클리어 연출을 재생한다.
        // 튜브들이 파도처럼 통통 튄 뒤 클리어 팝업이 등장한다.
        private void PlayClearSequence()
        {
            // 튜브마다 0.08초 시차로 바운스 → 왼쪽에서 오른쪽으로 파도치는 느낌
            for (int i = 0; i < _tubeViews.Length; i++)
                _tubeViews[i].PlayClearBounce(i * 0.08f);

            // 튜브 애니메이션이 끝날 때쯤 팝업 등장
            float popupDelay = _tubeViews.Length * 0.08f + 0.4f;
            DOVirtual.DelayedCall(popupDelay, () => _clearPopup.Show());
        }

        // 클리어 팝업 오브젝트를 생성하고 초기화한다.
        private void BuildClearPopup()
        {
            var go = new GameObject("ClearPopup");
            _clearPopup = go.AddComponent<ClearPopup>();
            _clearPopup.Init(
                // 다시하기: 현재 레벨을 처음부터 다시 시작
                onRetry: () => SceneLoader.LoadGame(_currentLevelIndex),
                // 다음 레벨: 마지막 레벨이면 레벨선택으로, 아니면 다음 레벨로 이동
                onNext: () =>
                {
                    int nextIndex = _currentLevelIndex + 1;
                    if (nextIndex < _levels.Length)
                        SceneLoader.LoadGame(nextIndex);
                    else
                        SceneLoader.LoadLevelSelect();
                },
                font: koreanFont
            );
        }

        // ── 초기화 헬퍼 ──────────────────────────────────────────

        // LevelData의 TubeInitData 배열을 읽어 Core Tube 배열을 만든다.
        private Tube[] BuildTubes()
        {
            var tubes = new Tube[_levelData.tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                // segments가 null이면 빈 배열로 처리 (빈 튜브)
                int[] segs = _levelData.tubes[i].segments ?? System.Array.Empty<int>();
                tubes[i] = new Tube(_levelData.tubeCapacity, segs);
            }
            return tubes;
        }

        // Tube 배열로 TubeView 오브젝트를 생성하고 화면 중앙에 배치한다.
        private TubeView[] BuildViews(Tube[] tubes)
        {
            _square = CreateSquareSprite(); // 필드에 저장 (애니메이션에서도 재사용)
            Color[] palette = _levelData.palette;

            // 전체 튜브를 화면 가로 중앙에 배치
            float totalWidth = (tubes.Length - 1) * TubeSpacing;
            float startX = -totalWidth / 2f;

            // 튜브 세로 중앙 정렬
            float centerY = -(_levelData.tubeCapacity - 1) * SegmentSize * 0.5f;

            var views = new TubeView[tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                var go = new GameObject($"Tube{i}");
                go.transform.position = new Vector3(startX + i * TubeSpacing, centerY, 0f);

                // TubeView 컴포넌트를 추가하고 초기화
                var view = go.AddComponent<TubeView>();
                view.Init(i, tubes[i], palette, _square, tubeSprite, SegmentSize, HandleTubeClicked);
                views[i] = view;
            }
            return views;
        }

        // 1x1 흰색 Sprite를 코드로 생성한다.
        // SpriteRenderer.color로 색을 입힐 수 있어서 에셋 없이 색 사각형을 만들 수 있다.
        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white); // 픽셀 하나를 흰색으로
            tex.Apply();                      // GPU에 업로드
            // Rect(0,0,1,1): 텍스처 전체 영역 사용
            // Vector2(0.5, 0.5): 피벗(기준점)을 중앙으로
            // 1f: Pixels Per Unit = 1 (1픽셀 = 1월드유닛)
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
