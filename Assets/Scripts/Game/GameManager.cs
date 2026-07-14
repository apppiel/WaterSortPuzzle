using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

        // 인스펙터에서 드래그로 연결하는 튜브 스프라이트 (Assets/Sprites/Tube.png)
        [SerializeField] private Sprite tubeSprite;

        // 세그먼트를 병 모양으로 클리핑하는 마스크 스프라이트 (Assets/Sprites/TubeMask.png)
        [SerializeField] private Sprite _tubeMask;

        // 인스펙터에서 드래그로 연결하는 한글 TMP 폰트 에셋 (NanumSquareRoundEB SDF)
        [SerializeField] private TMP_FontAsset koreanFont;

        // 현재 레벨 번호를 표시하는 텍스트 (인스펙터에서 연결)
        [SerializeField] private TextMeshProUGUI _levelText;

        // 효과음 재생 담당 (인스펙터에서 연결)
        [SerializeField] private AudioManager _audioManager;

        // 리셋 버튼 (인스펙터에서 연결) — 첫 이동 전까지 비활성화
        [SerializeField] private Button _resetButton;

        // Undo 버튼 (인스펙터에서 연결) — 횟수 소진 시 비활성화
        [SerializeField] private Button _undoButton;

        // 메뉴(뒤로가기) 버튼 (인스펙터에서 연결) — 눌리면 MenuPopup 표시
        [SerializeField] private Button _menuButton;

        // 레벨당 Undo 최대 횟수
        private const int MaxUndoCount = 3;

        // 남은 Undo 횟수
        private int _remainingUndos;

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

        // 메뉴 팝업이 열려있는 동안 게임 입력을 막기 위한 플래그
        private bool _isPaused;

        // 1x1 흰색 스프라이트 (TubeView 세그먼트·하이라이트용)
        private Sprite _square;

        // 클리어 팝업 UI
        private ClearPopup _clearPopup;

        // 메뉴 팝업 UI
        private MenuPopup _menuPopup;

        // 튜브 4개 이하: 한 줄. 5개 이상: 두 줄로 전환하는 기준 (상용 게임 표준)
        private const int TwoRowThreshold = 5;

        // 씬 시작 시 한 번 호출된다. LevelData로 Board와 TubeView를 생성한다.
        private void Start()
        {
            // AdManager 싱글턴이 없으면 생성한다 (첫 게임 씬 진입 시 한 번만)
            if (AdManager.Instance == null)
            {
                var adGo = new GameObject("AdManager");
                adGo.AddComponent<AdManager>();
            }
            // 레벨선택 씬에서 저장한 레벨 번호를 읽어온다. 없으면 0번(첫 레벨)
            _currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);

            // 레벨 데이터가 아예 없으면 메인 메뉴로 복귀 (인스펙터 설정 누락 등)
            if (_levels == null || _levels.Length == 0)
            {
                Debug.LogError("LevelData가 설정되지 않았습니다. 메인 메뉴로 복귀합니다.");
                SceneLoader.LoadMainMenu();
                return;
            }
            // 선택된 인덱스가 범위를 벗어나면 레벨 선택으로 복귀
            // (LevelSelect의 팬텀 버튼·저장 데이터 오염 등으로 유저가 갇히지 않도록 방어)
            if (_currentLevelIndex >= _levels.Length)
            {
                Debug.LogWarning($"선택된 레벨({_currentLevelIndex + 1})이 존재하지 않습니다. 레벨 선택으로 복귀합니다.");
                SceneLoader.LoadLevelSelect();
                return;
            }
            _levelData = _levels[_currentLevelIndex];

            // 레벨 텍스트 업데이트 (1부터 시작)
            if (_levelText != null)
                _levelText.text = $"Level {_currentLevelIndex + 1}";

            var tubes = BuildTubes();       // Core 데이터 생성
            _board = new Board(tubes);      // 게임 상태 초기화
            _tubeViews = BuildViews(tubes); // 화면 오브젝트 생성
            BuildClearPopup();              // 클리어 팝업 생성 (평소엔 숨김)
            BuildMenuPopup();               // 메뉴 팝업 생성 (평소엔 숨김)

            // 이동 전까지 리셋 버튼 비활성화
            if (_resetButton != null)
                _resetButton.interactable = false;

            // Undo 횟수 초기화
            _remainingUndos = MaxUndoCount;
        }

        // 매 프레임 호출된다. 터치 및 마우스 클릭 입력을 처리한다.
        private void Update()
        {
            // 클리어 후, 애니메이션 중, 메뉴 팝업 중에는 입력 무시
            if (_gameOver || _isAnimating || _isPaused) return;

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
                _audioManager?.PlaySelect();
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

                // Core에서 규칙 검증 + 이동
                int moved = _board.TryPour(from, index);

                if (moved > 0)
                {
                    // 첫 이동 성공 시 리셋 버튼 활성화
                    if (_resetButton != null)
                        _resetButton.interactable = true;
                    PlayPourAnimation(from, index);
                }
                else
                {
                    // 실패 피드백: 대상 튜브를 좌우로 흔들고 실패음 재생.
                    // 예전엔 아무 반응 없이 선택만 풀려서 "탭이 씹혔나?" 하는 혼란이 있었다.
                    _tubeViews[index].PlayFailShake();
                    _audioManager?.PlayFail();
                }
            }
        }

        // HUD의 메뉴(뒤로가기) 버튼에서 호출한다.
        // MenuPopup을 표시하고 게임 입력을 잠근다. 유저가 "계속하기" 누르면 재개.
        public void HandleOpenMenu()
        {
            if (_isAnimating || _gameOver || _isPaused) return;
            _isPaused = true;
            _menuPopup.Show();
        }

        // HUD의 리셋 버튼에서 호출한다.
        // 보상형 광고를 시청하면 현재 레벨을 처음부터 다시 시작한다.
        public void HandleReset()
        {
            if (_isAnimating || _gameOver || _isPaused) return;

            var ad = AdManager.Instance;
            if (ad != null)
            {
                ad.ShowRewarded(
                    onRewarded: () => SceneLoader.LoadGame(_currentLevelIndex),
                    onFailed:   () => Debug.Log("보상형 광고 미준비 — 리셋 불가")
                );
            }
        }

        // HUD의 Undo 버튼에서 호출한다. 최대 3회까지 되돌릴 수 있다.
        public void HandleUndo()
        {
            // 클리어 후에는 Undo 금지 (승리 상태에서 되돌리면 팝업/상태가 어긋남)
            if (_gameOver || _isAnimating || _isPaused || _remainingUndos <= 0) return;
            if (!_board.TryUndo()) return;

            _remainingUndos--;

            // 횟수 소진 시 버튼 비활성화
            if (_undoButton != null)
                _undoButton.interactable = _remainingUndos > 0;

            // 어느 튜브가 바뀌었는지 모르므로 전체 갱신
            foreach (var view in _tubeViews)
                view.Refresh();
        }

        // 붓기 애니메이션: 출발 병이 이동·기울어서 도착 병에 붓고 돌아온다.
        private void PlayPourAnimation(int from, int to)
        {
            _isAnimating = true;

            Vector3 destPos = _tubeViews[to].transform.position;

            _tubeViews[from].PlayPourTo(
                destTubePos: destPos,
                onPoured: () =>
                {
                    _audioManager?.PlayPour(); // 병이 기울어져 실제로 붓는 순간에 재생
                    _tubeViews[from].Refresh();
                    _tubeViews[to].Refresh();
                },
                onDone: () =>
                {
                    _isAnimating = false;

                    if (WinChecker.IsWon(_board))
                    {
                        _gameOver = true;
                        LevelProgressManager.SaveClear(_currentLevelIndex);
                        _audioManager?.PlayClear();
                        PlayClearSequence();
                    }
                }
            );
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

        // 메뉴 팝업 오브젝트를 생성하고 초기화한다.
        // 레벨선택 버튼 → 즉시 레벨선택 씬 이동 (광고 없음)
        // 계속하기 버튼 → 팝업 닫고 게임 재개 (Hide 애니메이션 완료 후 콜백)
        private void BuildMenuPopup()
        {
            var go = new GameObject("MenuPopup");
            _menuPopup = go.AddComponent<MenuPopup>();
            _menuPopup.Init(
                onLevelSelect: () => SceneLoader.LoadLevelSelect(),
                onContinue:    () => _isPaused = false,
                font: koreanFont
            );
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
                    Action navigate = nextIndex < _levels.Length
                        ? (Action)(() => SceneLoader.LoadGame(nextIndex))
                        : SceneLoader.LoadLevelSelect;

                    // 전면 광고 시청 후 다음 씬으로 이동 (광고 없으면 바로 이동)
                    var ad = AdManager.Instance;
                    if (ad != null)
                        ad.ShowInterstitial(navigate);
                    else
                        navigate();
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
        // 튜브가 4개 이하면 한 줄, 5개 이상이면 위/아래 두 줄로 배치한다(TwoRowThreshold 참조).
        // 3개 이상일 때 홀짝 인덱스에 Y 오프셋을 줘서 지그재그 배치로 심심함을 줄인다.
        private TubeView[] BuildViews(Tube[] tubes)
        {
            _square = CreateSquareSprite();
            Color[] palette = _levelData.palette;

            int   count   = tubes.Length;
            bool  twoRows = count >= TwoRowThreshold;

            // 두 줄일 때 튜브를 더 작게 그려 화면에 여유 있게 들어오도록 조정
            float segSize = twoRows ? 0.58f : 0.7f;
            float spacing = twoRows ? 0.92f : 1.1f;

            // 튜브가 12개 이상이면 큰 행이 카메라 가로를 넘어감 → 자동 축소.
            // 큰 행 폭 = (rowCount-1) * spacing + segSize * 1.1 (튜브 자체 폭 근사)
            // 이 값이 TargetRowWidth 를 넘으면 세그먼트 크기와 간격을 비례 축소.
            if (twoRows)
            {
                int   biggestRow      = count - count / 2; // 홀수면 위 행이 더 큼
                const float TargetRowWidth = 5.2f;         // 카메라 가로 안전 폭 (1080x1920 기준 여유 포함)
                float ratio           = spacing / segSize; // 원래 비율 유지 (~1.586)
                float requiredWidth   = (biggestRow - 1) * spacing + segSize * 1.1f;
                if (requiredWidth > TargetRowWidth)
                {
                    segSize = TargetRowWidth / ((biggestRow - 1) * ratio + 1.1f);
                    spacing = segSize * ratio;
                }
            }

            // 지그재그 높이 차: 한 줄은 55%, 두 줄은 행 자체가 변화를 주므로 적용 안 함
            float stagger = twoRows ? 0f : (_levelData.tubeCapacity - 1) * segSize * 0.55f;

            var views = new TubeView[count];

            if (!twoRows)
            {
                // 한 줄 배치 (3개 이상이면 지그재그)
                float totalWidth = (count - 1) * spacing;
                // 지그재그로 위쪽 병이 HUD와 붙지 않도록 중심을 살짝 아래로 내린다
                float centerY    = -(_levelData.tubeCapacity - 1) * segSize * 0.5f - stagger * 0.3f;
                for (int i = 0; i < count; i++)
                {
                    // 3개 이상일 때 짝수 인덱스는 살짝 아래, 홀수 인덱스는 살짝 위
                    float yOff = (count >= 3) ? (i % 2 == 0 ? -stagger : stagger) : 0f;
                    views[i] = SpawnTubeView(i, tubes[i], palette, segSize,
                        new Vector3(-totalWidth / 2f + i * spacing, centerY + yOff, 0f));
                }
            }
            else
            {
                // 두 줄 배치: 아래 줄 → 위 줄 순으로 인덱스 배정
                int bottomCount = count / 2;
                int topCount    = count - bottomCount;

                // 행 간격: 튜브 시각적 높이 + 여백
                float tubeVisualHeight = (_levelData.tubeCapacity - 1) * segSize;
                float rowGap      = tubeVisualHeight + segSize * 2.8f;
                float baseCenterY = -(_levelData.tubeCapacity - 1) * segSize * 0.5f;
                float bottomY     = baseCenterY - rowGap * 0.5f;
                float topY        = baseCenterY + rowGap * 0.5f;

                // 아래 줄 (지그재그)
                float bottomWidth = (bottomCount - 1) * spacing;
                for (int i = 0; i < bottomCount; i++)
                {
                    float yOff = i % 2 == 0 ? -stagger : stagger;
                    views[i] = SpawnTubeView(i, tubes[i], palette, segSize,
                        new Vector3(-bottomWidth / 2f + i * spacing, bottomY + yOff, 0f));
                }

                // 위 줄 (지그재그 — 아래 줄과 위상 반전해서 더 자연스럽게)
                float topWidth = (topCount - 1) * spacing;
                for (int i = 0; i < topCount; i++)
                {
                    int   idx  = bottomCount + i;
                    float yOff = i % 2 == 0 ? stagger : -stagger; // 반전
                    views[idx] = SpawnTubeView(idx, tubes[idx], palette, segSize,
                        new Vector3(-topWidth / 2f + i * spacing, topY + yOff, 0f));
                }
            }

            return views;
        }

        // 튜브 GameObject를 생성하고 TubeView를 초기화해 반환한다.
        private TubeView SpawnTubeView(int index, Tube tube, Color[] palette, float segSize, Vector3 pos)
        {
            var go = new GameObject($"Tube{index}");
            go.transform.position = pos;
            var view = go.AddComponent<TubeView>();
            view.Init(index, tube, palette, _square, tubeSprite, _tubeMask, segSize);
            return view;
        }

        // 1x1 흰색 Sprite를 코드로 생성한다. (하이라이트용)
        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

    }
}
