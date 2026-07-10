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

        // 1x1 흰색 스프라이트 (TubeView 세그먼트·하이라이트용)
        private Sprite _square;

        // 클리어 팝업 UI
        private ClearPopup _clearPopup;

        // 튜브 6개 이하: 한 줄. 6개 초과: 두 줄로 전환하는 기준
        private const int TwoRowThreshold = 6;

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

            // 레벨 텍스트 업데이트 (1부터 시작)
            if (_levelText != null)
                _levelText.text = $"Level {_currentLevelIndex + 1}";

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

                // TryPour 이후 Board 상태가 바뀌므로 색을 미리 저장한다
                Color pourColor = _levelData.palette[_board.GetTube(from).TopColor];

                // Core에서 규칙 검증 + 이동
                int moved = _board.TryPour(from, index);

                if (moved > 0)
                    PlayPourAnimation(from, index);
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
        // 튜브가 6개 이하면 한 줄, 7개 이상이면 위/아래 두 줄로 배치한다.
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

        // 튜브 GameObejct를 생성하고 TubeView를 초기화해 반환한다.
        private TubeView SpawnTubeView(int index, Tube tube, Color[] palette, float segSize, Vector3 pos)
        {
            var go = new GameObject($"Tube{index}");
            go.transform.position = pos;
            var view = go.AddComponent<TubeView>();
            view.Init(index, tube, palette, _square, tubeSprite, _tubeMask, segSize, HandleTubeClicked);
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
