using System;
using UnityEngine;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Game
{
    /// <summary>
    /// 튜브 하나의 시각적 표현을 담당하는 MonoBehaviour.
    /// Core의 Tube 데이터를 읽어 화면에 색 세그먼트를 렌더링한다.
    /// 게임 로직은 없고 표현만 한다.
    /// </summary>
    public class TubeView : MonoBehaviour
    {
        // Core 데이터: 이 뷰가 표현하는 튜브
        private Tube _tube;

        // ColorId → Unity Color 팔레트 (LevelData에서 전달받음)
        private Color[] _palette;

        // 각 세그먼트 슬롯의 SpriteRenderer 배열 (index 0 = 가장 아래)
        private SpriteRenderer[] _slots;

        // 튜브 배경 SpriteRenderer (선택 하이라이트에 사용)
        private SpriteRenderer _background;

        // 빈 슬롯 색 (연한 회색 반투명)
        private static readonly Color EmptySlotColor = new(0.85f, 0.85f, 0.85f, 0.4f);

        // 선택 안 됐을 때 배경 색 (어두운 회색 반투명)
        private static readonly Color BackgroundNormal = new(0.15f, 0.15f, 0.15f, 0.6f);

        // 선택됐을 때 배경 색 (노란색)
        private static readonly Color BackgroundSelected = new(1f, 0.9f, 0.2f, 0.8f);

        /// <summary>
        /// 튜브 뷰를 초기화한다. GameManager가 Start()에서 호출한다.
        /// </summary>
        /// <param name="index">튜브 인덱스 (클릭 감지용)</param>
        /// <param name="tube">Core 튜브 데이터</param>
        /// <param name="palette">ColorId → Color 팔레트 배열</param>
        /// <param name="square">1x1 흰색 스프라이트 (색 입혀서 사각형으로 사용)</param>
        /// <param name="segmentSize">세그먼트 한 칸의 월드 유닛 크기</param>
        /// <param name="onClicked">튜브 클릭 시 호출할 콜백</param>
        public void Init(int index, Tube tube, Color[] palette, Sprite square, float segmentSize, Action<int> onClicked)
        {
            _tube = tube;
            _palette = palette;

            float s = segmentSize; // 가독성을 위해 짧게 alias

            // ── 배경 오브젝트 생성 ──────────────────────────────
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform, false); // 이 TubeView의 자식으로 추가

            // 배경 위치: 세그먼트 전체의 중앙
            bgGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);

            // 배경 크기: 세그먼트 전체 높이보다 약간 크게
            bgGo.transform.localScale = new Vector3(s * 1.1f, tube.Capacity * s + s * 0.2f, 1f);

            _background = bgGo.AddComponent<SpriteRenderer>();
            _background.sprite = square;
            _background.color = BackgroundNormal;
            _background.sortingOrder = -10; // 슬롯보다 뒤에 렌더링

            // 클릭 감지를 위한 BoxCollider2D (크기는 transform.localScale에 맞춰짐)
            bgGo.AddComponent<BoxCollider2D>();

            // 어느 튜브인지 식별하기 위한 컴포넌트
            var clickTarget = bgGo.AddComponent<TubeClickTarget>();
            clickTarget.TubeIndex = index;
            clickTarget.OnClicked = onClicked;

            // ── 세그먼트 슬롯 생성 ──────────────────────────────
            _slots = new SpriteRenderer[tube.Capacity];
            for (int i = 0; i < tube.Capacity; i++)
            {
                var slotGo = new GameObject($"Slot{i}");
                slotGo.transform.SetParent(transform, false);

                // 슬롯 위치: 아래(i=0)부터 위(i=Capacity-1)로 쌓임
                slotGo.transform.localPosition = new Vector3(0f, i * s, 0f);

                // 슬롯 크기: 세그먼트 크기보다 약간 작게 (슬롯 사이 여백 생김)
                slotGo.transform.localScale = new Vector3(s * 0.88f, s * 0.88f, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sortingOrder = 10; // 배경보다 앞에 렌더링

                _slots[i] = sr;
            }

            // 초기 색 반영
            Refresh();
        }

        /// <summary>
        /// Core Tube 데이터를 읽어 각 슬롯의 색을 업데이트한다.
        /// Pour나 Undo 후에 GameManager가 호출한다.
        /// </summary>
        public void Refresh()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                // 세그먼트가 있으면 팔레트 색, 없으면 빈 슬롯 색
                _slots[i].color = i < _tube.Count
                    ? _palette[_tube.GetSegment(i)]
                    : EmptySlotColor;
            }
        }

        /// <summary>
        /// 선택 상태를 시각적으로 표시한다.
        /// selected=true면 배경을 노랗게, false면 원래 색으로.
        /// </summary>
        public void SetSelected(bool selected)
        {
            _background.color = selected ? BackgroundSelected : BackgroundNormal;
        }
    }
}
