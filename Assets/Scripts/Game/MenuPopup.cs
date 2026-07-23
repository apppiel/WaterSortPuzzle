using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WaterSortPuzzle.Game
{
    // 게임 중 우측 상단(등) 메뉴 버튼을 눌렀을 때 표시되는 팝업.
    // 두 개의 버튼(레벨선택 / 계속하기)만 제공한다.
    // 확인 문구 없이 유저의 명확한 선택으로 처리된다.
    public class MenuPopup : MonoBehaviour
    {
        // 화면 전체를 덮는 반투명 어두운 오버레이
        private Image _overlay;

        // 중앙에 표시되는 팝업 패널
        private RectTransform _panel;

        // 레벨선택 / 계속하기 콜백
        private Action _onLevelSelect;
        private Action _onContinue;

        // 한글 지원 TMP 폰트 (GameManager 인스펙터에서 전달)
        private TMP_FontAsset _font;

        // 팝업을 초기화한다. GameManager의 Start()에서 호출한다.
        public void Init(Action onLevelSelect, Action onContinue, TMP_FontAsset font)
        {
            _onLevelSelect = onLevelSelect;
            _onContinue    = onContinue;
            _font          = font;
            BuildUI();
            gameObject.SetActive(false);
        }

        // 메뉴 팝업을 표시하고 등장 연출을 재생한다.
        // 오버레이 페이드 인 → 패널 스케일 인 순서로 진행된다.
        public void Show()
        {
            gameObject.SetActive(true);

            _overlay.color    = new Color(0f, 0f, 0f, 0f);
            _panel.localScale = Vector3.zero;

            DOTween.Sequence()
                .Append(DOVirtual.Float(0f, 0.7f, 0.18f,
                    v => _overlay.color = new Color(0f, 0f, 0f, v)))
                .Append(_panel.DOScale(1f, 0.28f).SetEase(Ease.OutBack));
        }

        // 팝업을 숨기고 게임으로 돌아간다. 계속하기 버튼에서 호출.
        public void Hide()
        {
            DOTween.Sequence()
                .Append(_panel.DOScale(0f, 0.18f).SetEase(Ease.InBack))
                .Join(DOVirtual.Float(0.7f, 0f, 0.18f,
                    v => _overlay.color = new Color(0f, 0f, 0f, v)))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _onContinue?.Invoke();
                });
        }

        // UI 계층구조를 코드로 생성한다. (ClearPopup과 동일 스타일)
        private void BuildUI()
        {
            // ── Canvas ─────────────────────────────────────────
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110; // ClearPopup(100)보다 위

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0f;

            gameObject.AddComponent<GraphicRaycaster>();

            // ── 오버레이 (뒤 UI/게임 입력 차단) ─────────────────
            var overlayGo = new GameObject("Overlay");
            overlayGo.transform.SetParent(transform, false);
            var overlayRect = overlayGo.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
            _overlay = overlayGo.AddComponent<Image>();
            _overlay.color = Color.clear;
            // raycastTarget 기본 true — 오버레이가 뒤 UI 클릭을 흡수

            // ── 중앙 팝업 패널 ─────────────────────────────────
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(transform, false);
            _panel = panelGo.AddComponent<RectTransform>();
            _panel.sizeDelta       = new Vector2(700f, 360f);
            _panel.anchoredPosition = Vector2.zero;
            PopupHelpers.AddRoundedImage(panelGo, Color.white);

            // ── 두 개의 버튼 (좌: 레벨선택 / 우: 계속하기) ─────
            // 좌: 라이트 핑크 (보조)
            CreateButton(panelGo.transform, "레벨선택",
                new Vector2(-165f, 0f),
                new Color(1f, 0.761f, 0.761f, 1f),
                () => _onLevelSelect?.Invoke());

            // 우: 딥 핑크 (주 — 대부분의 유저가 실수 방지로 계속하기 선택)
            CreateButton(panelGo.transform, "계속하기",
                new Vector2(165f, 0f),
                new Color(1f, 0.541f, 0.541f, 1f),
                () => Hide());
        }

        // 버튼 오브젝트를 생성해서 parent에 붙인다. (ClearPopup과 동일 스타일)
        private void CreateButton(Transform parent, string label, Vector2 pos, Color bgColor, Action onClick)
        {
            var btnGo = new GameObject("Btn_" + label);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.sizeDelta       = new Vector2(290f, 110f);
            rect.anchoredPosition = pos;
            PopupHelpers.AddRoundedImage(btnGo, bgColor);
            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;
            tmp.text      = label;
            tmp.fontSize  = 44f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}
