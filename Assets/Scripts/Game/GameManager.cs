using UnityEngine;
using UnityEngine.InputSystem;
using WaterSortPuzzle.Core;
using WaterSortPuzzle.Data;

namespace WaterSortPuzzle.Game
{
    // 게임 전체 흐름을 제어하는 MonoBehaviour.
    // LevelData를 읽어 Board(게임 상태)와 TubeView(화면)를 만들고,
    // 입력을 받아 Pour/Undo를 실행하고, 승리를 감지한다.
    public class GameManager : MonoBehaviour
    {
        // 인스펙터에서 드래그로 연결하는 레벨 데이터
        [SerializeField] private LevelData levelData;

        // Core 게임 상태 (튜브 집합 + 이동 히스토리)
        private Board _board;

        // 화면에 보이는 튜브 뷰 배열
        private TubeView[] _tubeViews;

        // 현재 선택된 튜브 인덱스. 아무것도 선택 안 됐으면 -1
        private int _selectedIndex = -1;

        // 클리어 후 입력을 막기 위한 플래그
        private bool _gameOver;

        // 세그먼트 한 칸의 월드 유닛 크기 (이 값 하나로 전체 크기 조절)
        private const float SegmentSize = 0.5f;

        // 튜브 사이 간격 (월드 유닛)
        private const float TubeSpacing = 0.8f;

        // 씬 시작 시 한 번 호출된다. LevelData로 Board와 TubeView를 생성한다.
        private void Start()
        {
            var tubes = BuildTubes();       // Core 데이터 생성
            _board = new Board(tubes);      // 게임 상태 초기화
            _tubeViews = BuildViews(tubes); // 화면 오브젝트 생성
        }

        // 매 프레임 호출된다. 마우스 클릭과 키보드 입력을 처리한다.
        private void Update()
        {
            // 클리어 후에는 입력 무시
            if (_gameOver) return;

            // 마우스가 없는 환경(모바일 등)에서는 스킵
            if (Mouse.current == null) return;

            // 마우스 왼쪽 버튼을 이번 프레임에 눌렀을 때
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 스크린 좌표를 월드 좌표로 변환
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                // 클릭한 위치에 Collider2D가 있는지 확인
                var hit = Physics2D.OverlapPoint(worldPos);

                // TubeClickTarget 컴포넌트가 있는 Collider면 튜브 클릭으로 처리
                if (hit != null && hit.TryGetComponent<TubeClickTarget>(out var target))
                    HandleTubeClicked(target.TubeIndex);
            }

            // Z 키: Undo
            if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
                HandleUndo();
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
                // 다른 튜브 클릭: 선택된 튜브 → 클릭한 튜브로 붓기 시도
                int from = _selectedIndex;
                _tubeViews[from].SetSelected(false); // 먼저 하이라이트 해제
                _selectedIndex = -1;

                int moved = _board.TryPour(from, index); // Core에서 규칙 검증 + 이동
                if (moved > 0)
                {
                    // 이동이 일어난 튜브만 화면 갱신
                    _tubeViews[from].Refresh();
                    _tubeViews[index].Refresh();

                    // 승리 조건 확인
                    if (WinChecker.IsWon(_board))
                    {
                        _gameOver = true;
                        Debug.Log("클리어!");
                    }
                }
            }
        }

        // Z 키 입력 시 호출된다. Board의 마지막 이동을 되돌리고 모든 뷰를 갱신한다.
        private void HandleUndo()
        {
            if (!_board.TryUndo()) return;

            // 어느 튜브가 바뀌었는지 모르므로 전체 갱신
            foreach (var view in _tubeViews)
                view.Refresh();
        }

        // LevelData의 TubeInitData 배열을 읽어 Core Tube 배열을 만든다.
        private Tube[] BuildTubes()
        {
            var tubes = new Tube[levelData.tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                // segments가 null이면 빈 배열로 처리 (빈 튜브)
                int[] segs = levelData.tubes[i].segments ?? System.Array.Empty<int>();
                tubes[i] = new Tube(levelData.tubeCapacity, segs);
            }
            return tubes;
        }

        // Tube 배열로 TubeView 오브젝트를 생성하고 화면 중앙에 배치한다.
        private TubeView[] BuildViews(Tube[] tubes)
        {
            Sprite square = CreateSquareSprite(); // 프로그래매틱 1x1 흰색 스프라이트
            Color[] palette = levelData.palette;

            // 전체 튜브를 화면 가로 중앙에 배치
            float totalWidth = (tubes.Length - 1) * TubeSpacing;
            float startX = -totalWidth / 2f;

            // 튜브 세로 중앙 정렬
            float centerY = -(levelData.tubeCapacity - 1) * SegmentSize * 0.5f;

            var views = new TubeView[tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                var go = new GameObject($"Tube{i}");
                go.transform.position = new Vector3(startX + i * TubeSpacing, centerY, 0f);

                // TubeView 컴포넌트를 추가하고 초기화
                var view = go.AddComponent<TubeView>();
                view.Init(i, tubes[i], palette, square, SegmentSize, HandleTubeClicked);
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
