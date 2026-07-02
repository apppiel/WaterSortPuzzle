using UnityEngine;
using UnityEngine.InputSystem;
using WaterSortPuzzle.Core;
using WaterSortPuzzle.Data;

namespace WaterSortPuzzle.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;

        private Board _board;
        private TubeView[] _tubeViews;
        private int _selectedIndex = -1;
        private bool _gameOver;

        private const float SegmentSize = 0.5f;   // 세그먼트 한 칸의 world unit 크기
        private const float TubeSpacing = 0.8f;

        private void Start()
        {
            var tubes = BuildTubes();
            _board = new Board(tubes);
            _tubeViews = BuildViews(tubes);
        }

        private void Update()
        {
            if (_gameOver) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                var hit = Physics2D.OverlapPoint(worldPos);
                if (hit != null && hit.TryGetComponent<TubeClickTarget>(out var target))
                    HandleTubeClicked(target.TubeIndex);
            }

            if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
                HandleUndo();
        }

        private void HandleTubeClicked(int index)
        {
            if (_selectedIndex == -1)
            {
                if (_board.GetTube(index).IsEmpty) return;
                _selectedIndex = index;
                _tubeViews[index].SetSelected(true);
            }
            else if (_selectedIndex == index)
            {
                _tubeViews[index].SetSelected(false);
                _selectedIndex = -1;
            }
            else
            {
                int from = _selectedIndex;
                _tubeViews[from].SetSelected(false);
                _selectedIndex = -1;

                int moved = _board.TryPour(from, index);
                if (moved > 0)
                {
                    _tubeViews[from].Refresh();
                    _tubeViews[index].Refresh();

                    if (WinChecker.IsWon(_board))
                    {
                        _gameOver = true;
                        Debug.Log("클리어!");
                    }
                }
            }
        }

        private void HandleUndo()
        {
            if (!_board.TryUndo()) return;
            foreach (var view in _tubeViews)
                view.Refresh();
        }

        // ── 초기화 헬퍼 ──────────────────────────────────────────

        private Tube[] BuildTubes()
        {
            var tubes = new Tube[levelData.tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                int[] segs = levelData.tubes[i].segments ?? System.Array.Empty<int>();
                tubes[i] = new Tube(levelData.tubeCapacity, segs);
            }
            return tubes;
        }

        private TubeView[] BuildViews(Tube[] tubes)
        {
            Sprite square = CreateSquareSprite();
            Color[] palette = levelData.palette;

            float totalWidth = (tubes.Length - 1) * TubeSpacing;
            float startX = -totalWidth / 2f;
            float centerY = -(levelData.tubeCapacity - 1) * SegmentSize * 0.5f;

            var views = new TubeView[tubes.Length];
            for (int i = 0; i < tubes.Length; i++)
            {
                var go = new GameObject($"Tube{i}");
                go.transform.position = new Vector3(startX + i * TubeSpacing, centerY, 0f);
                var view = go.AddComponent<TubeView>();
                view.Init(i, tubes[i], palette, square, SegmentSize, HandleTubeClicked);
                views[i] = view;
            }
            return views;
        }

        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
