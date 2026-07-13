// 버튼에 추가하면 눌렀다 뗄 때 탄력 있는 스케일 애니메이션이 재생된다
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace WaterSortPuzzle.UI
{
    public class UIButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private RectTransform _rect;
        private Vector3 _baseScale; // 원래 스케일 저장

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _baseScale = _rect.localScale;
        }

        // 누를 때 살짝 줄어든다
        public void OnPointerDown(PointerEventData eventData)
        {
            _rect.DOKill();
            _rect.DOScale(_baseScale * 0.88f, 0.08f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        // 뗄 때 탄력 있게 원래 크기로 돌아온다
        public void OnPointerUp(PointerEventData eventData)
        {
            _rect.DOKill();
            _rect.DOScale(_baseScale, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
        }
    }
}
