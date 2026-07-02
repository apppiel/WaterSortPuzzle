using System;
using UnityEngine;
using WaterSortPuzzle.Core;

namespace WaterSortPuzzle.Game
{
    public class TubeView : MonoBehaviour
    {
        private Tube _tube;
        private Color[] _palette;
        private SpriteRenderer[] _slots;
        private SpriteRenderer _background;

        private static readonly Color EmptySlotColor = new(0.85f, 0.85f, 0.85f, 0.4f);
        private static readonly Color BackgroundNormal = new(0.15f, 0.15f, 0.15f, 0.6f);
        private static readonly Color BackgroundSelected = new(1f, 0.9f, 0.2f, 0.8f);

        public void Init(int index, Tube tube, Color[] palette, Sprite square, float segmentSize, Action<int> onClicked)
        {
            _tube = tube;
            _palette = palette;

            float s = segmentSize;

            // 배경
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform, false);
            bgGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);
            bgGo.transform.localScale = new Vector3(s * 1.1f, tube.Capacity * s + s * 0.2f, 1f);
            _background = bgGo.AddComponent<SpriteRenderer>();
            _background.sprite = square;
            _background.color = BackgroundNormal;
            _background.sortingOrder = -10;

            // 클릭 감지용 콜라이더
            bgGo.AddComponent<BoxCollider2D>();
            var clickTarget = bgGo.AddComponent<TubeClickTarget>();
            clickTarget.TubeIndex = index;
            clickTarget.OnClicked = onClicked;

            // 세그먼트 슬롯
            _slots = new SpriteRenderer[tube.Capacity];
            for (int i = 0; i < tube.Capacity; i++)
            {
                var slotGo = new GameObject($"Slot{i}");
                slotGo.transform.SetParent(transform, false);
                slotGo.transform.localPosition = new Vector3(0f, i * s, 0f);
                slotGo.transform.localScale = new Vector3(s * 0.88f, s * 0.88f, 1f);
                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sortingOrder = 10;
                _slots[i] = sr;
            }

            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].color = i < _tube.Count ? _palette[_tube.GetSegment(i)] : EmptySlotColor;
        }

        public void SetSelected(bool selected)
        {
            _background.color = selected ? BackgroundSelected : BackgroundNormal;
        }
    }
}
