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

        // 각 슬롯 상단에 얹히는 얇은 하이라이트 선 (물 표면 반사 효과)
        private SpriteRenderer[] _highlights;

        // 선택 하이라이트 SpriteRenderer (병 뒤에서 노랗게 빛남)
        private SpriteRenderer _selectionHighlight;

        // 선택 해제 시 돌아올 원래 월드 위치
        private Vector3 _basePosition;

        // 선택 시 위로 올라가는 거리 (월드 유닛)
        private const float SelectLiftY = 0.22f;

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
        // square: 1x1 흰색 스프라이트 (하이라이트·애니메이션에 사용)
        // tubeSprite: 병 PNG 스프라이트 (가장 위에 올라가 유리 테두리 효과)
        // tubeMask: 병 내부 마스크 스프라이트 (세그먼트를 병 모양으로 클리핑)
        // segmentSize: 세그먼트 한 칸의 월드 유닛 크기
        // onClicked: 튜브 클릭 시 호출할 콜백
        public void Init(int index, Tube tube, Color[] palette, Sprite square, Sprite tubeSprite, Sprite tubeMask, float segmentSize, Action<int> onClicked)
        {
            _tube = tube;
            _palette = palette;

            // GameManager가 위치를 설정한 뒤 Init을 호출하므로 여기서 기준 위치를 기억한다
            _basePosition = transform.position;

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

            // ── SpriteMask: 세그먼트를 병 내부 모양으로 클리핑 ──
            // 마스크 스프라이트의 알파값이 1인 영역에서만 세그먼트가 보인다.
            var maskGo = new GameObject("TubeMask");
            maskGo.transform.SetParent(transform, false);
            maskGo.transform.localPosition = new Vector3(0f, (tube.Capacity - 1) * s * 0.5f, 0f);
            float maskNativeW = tubeMask.rect.width / tubeMask.pixelsPerUnit;
            float maskScale = (s * 1.1f) / maskNativeW;
            maskGo.transform.localScale = new Vector3(maskScale, maskScale, 1f);
            var spriteMask = maskGo.AddComponent<SpriteMask>();
            spriteMask.sprite = tubeMask;

            // ── 세그먼트 슬롯 + 하이라이트 생성 ────────────────
            _slots      = new SpriteRenderer[tube.Capacity];
            _highlights = new SpriteRenderer[tube.Capacity];
            for (int i = 0; i < tube.Capacity; i++)
            {
                var slotGo = new GameObject($"Slot{i}");
                slotGo.transform.SetParent(transform, false);

                // 슬롯 위치: 아래(i=0)부터 위(i=Capacity-1)로 쌓임
                slotGo.transform.localPosition = new Vector3(0f, i * s, 0f);

                // 슬롯 너비를 병 내부보다 넉넉하게, 높이는 살짝 겹쳐 틈 제거
                slotGo.transform.localScale = new Vector3(s * 1.5f, s * 1.02f, 1f);

                var sr = slotGo.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sortingOrder = 10;
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _slots[i] = sr;

                // 슬롯 상단에 얇은 흰색 반투명 선 → 물 표면 반사 효과
                var shineGo = new GameObject($"Highlight{i}");
                shineGo.transform.SetParent(transform, false);
                // 슬롯 상단 끝에 위치 (슬롯 중심 + 높이의 절반 - 선 두께 절반)
                shineGo.transform.localPosition = new Vector3(0f, i * s + s * 0.44f, 0f);
                shineGo.transform.localScale     = new Vector3(s * 1.5f, s * 0.1f, 1f);

                var hlSr = shineGo.AddComponent<SpriteRenderer>();
                hlSr.sprite = square;
                hlSr.color  = new Color(1f, 1f, 1f, 0f); // 평소엔 투명
                hlSr.sortingOrder = 11;                    // 슬롯(10) 위
                hlSr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                _highlights[i] = hlSr;
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
                bool filled = i < _tube.Count;

                // 세그먼트가 있으면 팔레트 색, 없으면 빈 슬롯 색
                _slots[i].color = filled
                    ? _palette[_tube.GetSegment(i)]
                    : EmptySlotColor;

                // 같은 색 연속 구간의 최상단에만 하이라이트 표시
                // 바로 위 슬롯과 색이 다르거나(경계), 최상단 채워진 슬롯일 때만 선이 보인다
                bool isTopOfColorRun = filled &&
                    (i + 1 >= _tube.Count || _tube.GetSegment(i) != _tube.GetSegment(i + 1));

                _highlights[i].color = isTopOfColorRun
                    ? new Color(1f, 1f, 1f, 0.5f)
                    : new Color(1f, 1f, 1f, 0f);
            }
        }

        // 선택 상태를 시각적으로 표시한다.
        // selected=true면 위로 올라오며 하이라이트, false면 원위치로 복귀.
        public void SetSelected(bool selected)
        {
            _selectionHighlight.color = selected ? HighlightSelected : HighlightNormal;

            // 선택 시 살짝 위로 올라옴 (OutBack → 약간 튀는 느낌)
            // 해제 시 부드럽게 원래 위치로 복귀
            float targetY = selected ? _basePosition.y + SelectLiftY : _basePosition.y;
            transform.DOMoveY(targetY, 0.18f)
                     .SetEase(selected ? Ease.OutBack : Ease.OutQuad);
        }

        // 붓기 애니메이션: 위로 올린 뒤 옆으로 이동해 기울여서 붓고 원위치로 돌아온다.
        // onPoured : 기울기가 완성돼 붓는 타이밍 (출발·도착 튜브 Refresh 용)
        // onDone   : 병이 완전히 원위치로 복귀한 뒤 (입력 잠금 해제·승리 판정 용)
        public void PlayPourTo(Vector3 destTubePos, System.Action onPoured, System.Action onDone)
        {
            // 병 스프라이트 기준 입구까지의 높이 (기울였을 때 입구 위치 계산용)
            const float NeckHeight  = 2.59f;
            const float PourAngle   = 65f;   // 붓기 각도 — 너무 크면 병끼리 겹침
            const float LiftHeight  = 2.2f;  // 올리는 높이 — 높을수록 충돌 여유 생김

            // 오른쪽이면 시계방향(-), 왼쪽이면 반시계방향(+)
            float direction = (destTubePos.x > _basePosition.x) ? 1f : -1f;
            float zAngle    = -direction * PourAngle;

            // 기울였을 때 입구가 대상 병 위에 오도록 X 위치를 계산
            float mouthReach = NeckHeight * Mathf.Sin(PourAngle * Mathf.Deg2Rad);

            // 대상 튜브가 위에 있으면 높이 차만큼 더 올리고, 아래에 있으면 그만큼 덜 올린다
            float heightDiff = destTubePos.y - _basePosition.y;
            float neededLift = Mathf.Max(LiftHeight + heightDiff, 0.8f); // 최소 0.8 확보
            float liftY      = _basePosition.y + neededLift;
            Vector3 pourPos  = new Vector3(
                destTubePos.x - direction * mouthReach,
                liftY,
                0f
            );

            DOTween.Sequence()
                // 1. 제자리에서 위로 올리기 (대상 병보다 충분히 높이)
                .Append(transform.DOMoveY(liftY, 0.22f).SetEase(Ease.OutSine))
                // 2. 대상 병 위로 수평 이동
                .Append(transform.DOMove(pourPos, 0.22f).SetEase(Ease.OutSine))
                // 3. 기울여서 붓기 (~85도)
                .Append(transform.DORotate(new Vector3(0f, 0f, zAngle), 0.28f).SetEase(Ease.OutSine))
                // 4. 붓기 콜백 (양쪽 튜브 화면 갱신)
                .AppendCallback(() => onPoured?.Invoke())
                // 5. 기울인 채로 잠깐 유지
                .AppendInterval(0.15f)
                // 6. 기울기 복귀
                .Append(transform.DORotate(Vector3.zero, 0.22f).SetEase(Ease.InSine))
                // 7. 원위치로 복귀
                .Append(transform.DOMove(_basePosition, 0.3f).SetEase(Ease.OutBack))
                .OnComplete(() => onDone?.Invoke());
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
