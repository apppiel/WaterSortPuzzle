using System;
using UnityEngine;
using DG.Tweening;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Game
{
    // 튜브 하나의 시각적 표현을 담당하는 MonoBehaviour.
    // Core의 Tube 데이터를 읽어 화면에 색 세그먼트를 렌더링한다.
    // 게임 로직은 없고 표현만 한다.
    public class TubeView : MonoBehaviour
    {
        // Core 데이터: 이 뷰가 표현하는 튜브
        private Tube _tube;

        // ColorId → Unity Color 팔레트 (LevelData에서 전달받음)
        private Color[] _palette;

        // 각 세그먼트 슬롯의 SpriteRenderer 배열 (index 0 = 가장 아래)
        private SpriteRenderer[] _slots;

        // 선택 하이라이트 SpriteRenderer (병 뒤에서 노랗게 빛남)
        private SpriteRenderer _selectionHighlight;

        // 빈 슬롯 색 (연한 회색 반투명)
        private static readonly Color EmptySlotColor = new(0.85f, 0.85f, 0.85f, 0.4f);

        // 선택됐을 때 하이라이트 색 (노란색 반투명)
        private static readonly Color HighlightSelected = new(1f, 0.9f, 0.2f, 0.5f);

        // 선택 안 됐을 때 하이라이트 색 (완전 투명)
        private static readonly Color HighlightNormal = new(0f, 0f, 0f, 0f);

        // 튜브 뷰를 초기화한다. GameManager가 Start()에서 호출한다.
        // index: 튜브 인덱스 (클릭 감지용)
        // tube: Core 튜브 데이터
        // palette: ColorId → Color 팔레트 배열
        // square: 1x1 흰색 스프라이트 (슬롯과 하이라이트에 사용)
        // bottleSprite: 병 PNG 스프라이트 (가장 위에 올라감)
        // segmentSize: 세그먼트 한 칸의 월드 유닛 크기
        // onClicked: 튜브 클릭 시 호출할 콜백
        public void Init(int index, Tube tube, Color[] palette, Sprite square, Sprite tubeSprite, float segmentSize, Action<int> onClicked)
        {
            _tube = tube;
            _palette = palette;

            float s = segmentSize;

            // ── 클릭 감지 영역 (투명, 보이지 않음) ─────────────
            // BoxCollider2D만 붙이고 SpriteRenderer는 없음
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform, false);
            bgGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);
            bgGo.transform.localScale = new Vector3(s * 1.1f, tube.Capacity * s + s * 0.2f, 1f);
            bgGo.AddComponent<BoxCollider2D>();

            var clickTarget = bgGo.AddComponent<TubeClickTarget>();
            clickTarget.TubeIndex = index;
            clickTarget.OnClicked = onClicked;

            // ── 선택 하이라이트 (병 뒤에서 빛나는 효과) ────────
            var hlGo = new GameObject("SelectionHighlight");
            hlGo.transform.SetParent(transform, false);
            hlGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);
            hlGo.transform.localScale = new Vector3(s * 1.3f, tube.Capacity * s + s * 0.4f, 1f);
            _selectionHighlight = hlGo.AddComponent<SpriteRenderer>();
            _selectionHighlight.sprite = square;
            _selectionHighlight.color = HighlightNormal; // 평소엔 투명
            _selectionHighlight.sortingOrder = -10;      // 슬롯보다 뒤

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
                sr.sortingOrder = 10; // 하이라이트보다 앞, 병보다 뒤

                _slots[i] = sr;
            }

            // ── 튜브 스프라이트 (세그먼트 위에 올라가 유리 테두리 효과) ──
            var tubeGo = new GameObject("TubeSprite");
            tubeGo.transform.SetParent(transform, false);

            // 튜브 스프라이트의 픽셀 크기로 비율 계산 → 슬롯 너비에 맞게 스케일
            float nativeW = tubeSprite.rect.width / tubeSprite.pixelsPerUnit;
            float nativeH = tubeSprite.rect.height / tubeSprite.pixelsPerUnit;
            float tScale  = (s * 1.1f) / nativeW; // 가로를 슬롯 너비에 맞춤

            // 스프라이트 중심을 세그먼트 영역 중앙에 정렬
            float tubeH = nativeH * tScale;
            tubeGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);
            tubeGo.transform.localScale     = new Vector3(tScale, tScale, 1f);

            var tubeSr = tubeGo.AddComponent<SpriteRenderer>();
            tubeSr.sprite       = tubeSprite;
            tubeSr.sortingOrder = 20; // 세그먼트(10)보다 앞 → 유리 테두리가 물 위에 렌더링

            // 초기 색 반영
            Refresh();
        }

        // index번 슬롯의 월드 좌표를 반환한다. (애니메이션 시작/끝 위치 계산용)
        public Vector3 GetSlotWorldPos(int index) => _slots[index].transform.position;

        // Core Tube 데이터를 읽어 각 슬롯의 색을 업데이트한다.
        // Pour나 Undo 후에 GameManager가 호출한다.
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

        // 선택 상태를 시각적으로 표시한다.
        // selected=true면 병 뒤에서 노란 빛이 남, false면 투명.
        public void SetSelected(bool selected)
        {
            _selectionHighlight.color = selected ? HighlightSelected : HighlightNormal;
        }

        // 클리어 시 통통 튀는 애니메이션을 재생한다.
        // delay를 튜브마다 다르게 주면 왼쪽부터 오른쪽으로 파도치는 효과가 난다.
        public void PlayClearBounce(float delay)
        {
            transform.DOPunchScale(Vector3.one * 0.28f, 0.5f, 5, 0.5f)
                     .SetDelay(delay);
        }
    }
}
