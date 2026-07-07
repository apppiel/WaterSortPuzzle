// 설정 팝업 UI를 코드로 생성하고 제어하는 MonoBehaviour.
// 효과음 ON/OFF 토글 버튼 하나를 제공한다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WaterSortPuzzle.UI
{
    public class SettingsPopup : MonoBehaviour
    {
        // 브랜드 컬러
        private static readonly Color DeepPink  = new(1f, 0.541f, 0.541f); // #FF8C8C
        private static readonly Color LightPink = new(1f, 0.761f, 0.761f); // #FFC2C2
        private static readonly Color GrayOff   = new(0.75f, 0.75f, 0.75f);

        private GameObject _root;       // 팝업 루트 (오버레이 포함)
        private GameObject _panel;      // 흰색 패널
        private TextMeshProUGUI _sfxLabel; // SFX 버튼 텍스트
        private Image _sfxBtnImage;        // SFX 버튼 배경

        private TMP_FontAsset _font;

        // 팝업을 초기화한다. MainMenuManager가 호출한다.
        public void Init(TMP_FontAsset font)
        {
            _font = font;
            BuildUI();
            _root.SetActive(false); // 처음엔 숨김
        }

        // 팝업을 열고 애니메이션을 재생한다.
        public void Show()
        {
            _root.SetActive(true);
            RefreshSFXButton();

            _panel.transform.localScale = Vector3.zero;
            _panel.transform
                  .DOScale(Vector3.one, 0.3f)
                  .SetEase(Ease.OutBack);
        }

        // 팝업을 닫는다.
        private void Hide()
        {
            _panel.transform
                  .DOScale(Vector3.zero, 0.2f)
                  .SetEase(Ease.InBack)
                  .OnComplete(() => _root.SetActive(false));
        }

        // SFX 버튼을 눌렀을 때 상태를 토글하고 버튼 색을 갱신한다.
        private void OnSFXToggle()
        {
            SettingsManager.SFXEnabled = !SettingsManager.SFXEnabled;
            RefreshSFXButton();
        }

        // 현재 SFX 설정에 맞게 버튼 텍스트와 색을 업데이트한다.
        private void RefreshSFXButton()
        {
            bool on = SettingsManager.SFXEnabled;
            _sfxLabel.text     = on ? "효과음  ON" : "효과음  OFF";
            _sfxBtnImage.color = on ? DeepPink : GrayOff;
        }

        // UI 오브젝트를 코드로 생성한다.
        private void BuildUI()
        {
            // ── 루트 Canvas ───────────────────────────────────────
            _root = new GameObject("SettingsPopupRoot");
            _root.transform.SetParent(transform, false);

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // 다른 UI 위에 표시

            _root.AddComponent<GraphicRaycaster>();

            // ── 어두운 오버레이 ───────────────────────────────────
            var overlay = MakeRect(_root.transform, "Overlay");
            Stretch(overlay);
            var overlayImg = overlay.gameObject.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.45f);

            // 오버레이 클릭 시 닫기
            var overlayBtn = overlay.gameObject.AddComponent<Button>();
            overlayBtn.onClick.AddListener(Hide);

            // ── 흰색 패널 ─────────────────────────────────────────
            var panelRT = MakeRect(_root.transform, "Panel");
            panelRT.sizeDelta        = new Vector2(520f, 360f);
            panelRT.anchoredPosition = Vector2.zero;
            _panel = panelRT.gameObject;

            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = Color.white;

            // ── 타이틀 "설정" ─────────────────────────────────────
            var title = MakeRect(panelRT, "Title");
            title.anchorMin        = new Vector2(0f, 1f);
            title.anchorMax        = new Vector2(1f, 1f);
            title.pivot            = new Vector2(0.5f, 1f);
            title.anchoredPosition = new Vector2(0f, -30f);
            title.sizeDelta        = new Vector2(0f, 60f);

            var titleTmp = title.gameObject.AddComponent<TextMeshProUGUI>();
            titleTmp.text      = "설정";
            titleTmp.font      = _font;
            titleTmp.fontSize  = 48f;
            titleTmp.color     = new Color(0.2f, 0.2f, 0.2f);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // ── SFX 토글 버튼 ─────────────────────────────────────
            var sfxRT = MakeRect(panelRT, "SFXButton");
            sfxRT.sizeDelta        = new Vector2(360f, 80f);
            sfxRT.anchoredPosition = new Vector2(0f, 20f);

            _sfxBtnImage = sfxRT.gameObject.AddComponent<Image>();
            var sfxBtn = sfxRT.gameObject.AddComponent<Button>();
            sfxBtn.onClick.AddListener(OnSFXToggle);

            _sfxLabel = MakeLabel(sfxRT, "SFXLabel", "", 36f, Color.white);

            // ── 닫기 버튼 ─────────────────────────────────────────
            var closeRT = MakeRect(panelRT, "CloseButton");
            closeRT.sizeDelta        = new Vector2(280f, 70f);
            closeRT.anchoredPosition = new Vector2(0f, -80f);

            var closeImg = closeRT.gameObject.AddComponent<Image>();
            closeImg.color = LightPink;

            var closeBtn = closeRT.gameObject.AddComponent<Button>();
            closeBtn.onClick.AddListener(Hide);

            MakeLabel(closeRT, "CloseLabel", "닫기", 34f, Color.white);
        }

        // ── UI 생성 헬퍼 ──────────────────────────────────────────

        // 빈 RectTransform을 부모 아래에 만든다.
        private static RectTransform MakeRect(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        // RectTransform을 부모에 꽉 차게 펼친다.
        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        // TextMeshProUGUI 레이블을 생성한다.
        private TextMeshProUGUI MakeLabel(RectTransform parent, string name,
            string text, float size, Color color)
        {
            var rt = MakeRect(parent, name);
            Stretch(rt);

            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.font      = _font;
            tmp.fontSize  = size;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }
    }
}
