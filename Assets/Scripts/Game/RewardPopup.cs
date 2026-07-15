using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Game
{
  // 마지막 레벨(100) 클리어 시 표시되는 리워드 팝업 UI.
  // ClearPopup과 구조는 같지만, 발급된 인증코드를 표시하고 복사 기능을 제공한다.
  // RewardManager.OnCodeIssued 이벤트를 구독해서 코드 텍스트를 갱신한다.
  public class RewardPopup : MonoBehaviour
  {
    // 화면 전체를 덮는 반투명 어두운 오버레이
    private Image _overlay;

    // 중앙에 표시되는 팝업 패널
    private RectTransform _panel;

    // 타이틀 등장 후 펀치 애니메이션용 RectTransform
    private RectTransform _titleRect;

    // 발급된 코드를 표시하는 텍스트
    private TextMeshProUGUI _codeLabel;

    // 상태 메시지 텍스트 ("코드 생성 중..." → "발급 완료!" → "복사됨!")
    private TextMeshProUGUI _statusLabel;

    // 복사할 코드값 (CopyCode에서 클립보드에 넣음)
    private string _currentCode = "";

    // 닫기 버튼 콜백 (레벨선택으로 이동 등)
    private Action _onClose;

    // 모든 TMP 텍스트에 적용할 한글 폰트 에셋
    private TMP_FontAsset _font;

    // 팝업을 초기화한다. GameManager의 BuildRewardPopup()에서 호출한다.
    public void Init(Action onClose, TMP_FontAsset font)
    {
      _onClose = onClose;
      _font = font;
      BuildUI();
      gameObject.SetActive(false);
    }

    // 리워드 팝업을 표시하고 등장 연출을 재생한다.
    // ClearPopup과 동일한 오버레이 페이드 + 패널 스케일 인 + 타이틀 펀치 순서.
    public void Show()
    {
      gameObject.SetActive(true);

      _overlay.color = new Color(0f, 0f, 0f, 0f);
      _panel.localScale = Vector3.zero;

      DOTween.Sequence()
          .Append(DOVirtual.Float(0f, 0.75f, 0.25f,
              v => _overlay.color = new Color(0f, 0f, 0f, v)))
          .Append(_panel.DOScale(1f, 0.45f).SetEase(Ease.OutBack))
          .AppendCallback(() =>
              _titleRect.DOPunchScale(Vector3.one * 0.12f, 0.35f, 5, 0.5f));
    }

    // RewardManager.OnCodeIssued 이벤트 핸들러.
    // 코드가 비어있으면 로딩 중 상태, 값이 있으면 코드 표시 + 상태 메시지 갱신.
    public void SetResult(string code, string status, bool isReissue)
    {
      if (!string.IsNullOrEmpty(code))
      {
        _currentCode = code;
        if (_codeLabel != null) _codeLabel.text = code;
      }
      if (_statusLabel != null)
      {
        _statusLabel.text = status;
        // 발급 성공 시 상태 텍스트를 초록 계열로 강조 (복사됨과는 별개)
        _statusLabel.color = new Color(0.4f, 0.4f, 0.4f, 1f);
      }
    }

    // "복사하기" 버튼 콜백.
    // 시스템 클립보드에 코드를 넣고 상태 메시지를 잠깐 초록으로 바꾼다.
    private void CopyCode()
    {
      if (string.IsNullOrEmpty(_currentCode)) return;
      GUIUtility.systemCopyBuffer = _currentCode;
      if (_statusLabel != null)
      {
        _statusLabel.text = "클립보드에 복사되었습니다!";
        // 성공 피드백용 딥 그린
        _statusLabel.color = new Color(0.13f, 0.65f, 0.35f, 1f);
      }
    }

    // UI 계층구조를 코드로 생성한다. (ClearPopup과 동일한 패턴)
    private void BuildUI()
    {
      // ── Canvas 설정 ─────────────────────────────────────
      var canvas = gameObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 100; // ClearPopup과 동일 수준 (동시에 뜨지 않음)

      var scaler = gameObject.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1080f, 1920f);
      scaler.matchWidthOrHeight = 0f;

      gameObject.AddComponent<GraphicRaycaster>();

      // ── 전체 화면 오버레이 ───────────────────────────────
      var overlayGo = new GameObject("Overlay");
      overlayGo.transform.SetParent(transform, false);
      var overlayRect = overlayGo.AddComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
      _overlay = overlayGo.AddComponent<Image>();
      _overlay.color = Color.clear;

      // ── 중앙 팝업 패널 ──────────────────────────────────
      // ClearPopup(700x480)보다 세로로 김: 타이틀 + 안내 + 코드 + 상태 + 버튼 다 필요
      var panelGo = new GameObject("Panel");
      panelGo.transform.SetParent(transform, false);
      _panel = panelGo.AddComponent<RectTransform>();
      _panel.sizeDelta = new Vector2(820f, 780f);
      _panel.anchoredPosition = Vector2.zero;
      panelGo.AddComponent<Image>().color = Color.white;

      // ── 타이틀 ──────────────────────────────────────────
      // #FF8C8C : 브랜드 딥 핑크
      var titleGo = CreateText(panelGo.transform, "100 레벨 클리어!", 72f,
          new Vector2(0f, 290f), new Vector2(760f, 100f),
          new Color(1f, 0.541f, 0.541f, 1f), FontStyles.Bold);
      _titleRect = titleGo.GetComponent<RectTransform>();

      // ── 안내 문구 ────────────────────────────────────────
      CreateText(panelGo.transform, "이 코드로 상품을 받으세요", 34f,
          new Vector2(0f, 190f), new Vector2(760f, 60f),
          new Color(0.3f, 0.3f, 0.3f, 1f), FontStyles.Normal);

      // ── 코드 표시 박스 (라이트 핑크 배경에 큰 코드 텍스트) ──
      var codeBoxGo = new GameObject("CodeBox");
      codeBoxGo.transform.SetParent(panelGo.transform, false);
      var codeBoxRect = codeBoxGo.AddComponent<RectTransform>();
      codeBoxRect.sizeDelta = new Vector2(700f, 160f);
      codeBoxRect.anchoredPosition = new Vector2(0f, 40f);
      // #FFEBEB : 라이트 핑크보다 더 연한 배경 (강조된 코드를 위한 안정된 배경)
      codeBoxGo.AddComponent<Image>().color = new Color(1f, 0.922f, 0.922f, 1f);

      // #3D1E5E 딥 플럼: 라이트 핑크 배경과 보색 대비로 리워드 프리미엄 느낌.
      var codeGo = CreateText(codeBoxGo.transform, "----" + "-" + "----", 82f,
          Vector2.zero, new Vector2(680f, 140f),
          new Color(0.239f, 0.118f, 0.369f, 1f), FontStyles.Bold);
      _codeLabel = codeGo.GetComponent<TextMeshProUGUI>();
      // 코드 자릿수 강조를 위해 자간 살짝 넓힘
      _codeLabel.characterSpacing = 15f;

      // ── 상태 메시지 ───────────────────────────────────────
      var statusGo = CreateText(panelGo.transform, "코드 생성 중...", 28f,
          new Vector2(0f, -80f), new Vector2(760f, 50f),
          new Color(0.4f, 0.4f, 0.4f, 1f), FontStyles.Normal);
      _statusLabel = statusGo.GetComponent<TextMeshProUGUI>();

      // ── 버튼 두 개 ──────────────────────────────────────
      // 복사하기: 라이트 핑크 (보조 액션이지만 주 기능이라 큰 편)
      CreateButton(panelGo.transform, "복사하기",
          new Vector2(-175f, -260f),
          new Color(1f, 0.761f, 0.761f, 1f),
          CopyCode);

      // 닫기: 딥 핑크 (주 액션 = 다음 화면으로)
      CreateButton(panelGo.transform, "닫기",
          new Vector2(175f, -260f),
          new Color(1f, 0.541f, 0.541f, 1f),
          () => _onClose?.Invoke());
    }

    // 텍스트 오브젝트를 생성해서 parent에 붙인다.
    private GameObject CreateText(Transform parent, string content, float size,
                                  Vector2 pos, Vector2 sizeDelta,
                                  Color color, FontStyles style)
    {
      var go = new GameObject("Text");
      go.transform.SetParent(parent, false);
      var rect = go.AddComponent<RectTransform>();
      rect.sizeDelta = sizeDelta;
      rect.anchoredPosition = pos;
      var tmp = go.AddComponent<TextMeshProUGUI>();
      if (_font != null) tmp.font = _font;
      tmp.text = content;
      tmp.fontSize = size;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = color;
      tmp.fontStyle = style;
      return go;
    }

    // 버튼 오브젝트를 생성해서 parent에 붙인다.
    private void CreateButton(Transform parent, string label, Vector2 pos, Color bgColor, Action onClick)
    {
      var btnGo = new GameObject("Btn_" + label);
      btnGo.transform.SetParent(parent, false);
      var rect = btnGo.AddComponent<RectTransform>();
      rect.sizeDelta = new Vector2(310f, 120f);
      rect.anchoredPosition = pos;
      btnGo.AddComponent<Image>().color = bgColor;
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
      tmp.text = label;
      tmp.fontSize = 44f;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = Color.white;
      tmp.fontStyle = FontStyles.Bold;
    }
  }
}
