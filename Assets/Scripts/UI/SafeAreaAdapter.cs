// Canvas 안에 배치하는 Safe Area 패널 컴포넌트.
// Screen.safeArea를 읽어 자신의 RectTransform 앵커를 조정한다.
// 노치·펀치홀·홈 인디케이터 영역을 자동으로 피할 수 있다.

using UnityEngine;

namespace WaterSortPuzzle.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        private RectTransform _rectTransform; // 이 패널의 RectTransform

        // 씬 로드 시 Safe Area를 즉시 적용한다.
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        // Screen.safeArea 좌표를 0~1 앵커 비율로 변환하여 RectTransform에 적용한다.
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // 스크린 픽셀 좌표 → 0~1 정규화 앵커값
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // 오프셋을 0으로 초기화해야 Safe Area 크기와 정확히 일치한다.
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
