using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WaterSortPuzzle.Game
{
  // 레벨 클리어 시 표시되는 팝업 UI.
  // Canvas를 코드로 생성하고 DOTween으로 등장 애니메이션을 재생한다.
  public class ClearPopup : MonoBehaviour
  {
    // 화면 전체를 덮는 반투명 어두운 오버레이
    private Image _overlay;

    // 중앙에 표시되는 팝업 패널
    private RectTransform _panel;

    // 클리어 타이틀 텍스트 (등장 후 추가 펀치 애니메이션용)
    private RectTransform _titleRect;

    // 다시하기 / 다음 레벨 콜백
    private Action _onRetry;
    private Action _onNext;

    // 모든 TMP 텍스트에 적용할 폰트 에셋 (한글 지원)
    private TMP_FontAsset _font;

    // 팝업을 초기화한다. GameManager의 Start()에서 호출한다.
    // font: GameManager 인스펙터에서 연결한 한글 TMP 폰트 에셋
    public void Init(Action onRetry, Action onNext, TMP_FontAsset font)
    {
      _onRetry = onRetry;
      _onNext = onNext;
      _font = font;
      BuildUI();
      gameObject.SetActive(false);
    }

    // 클리어 팝업을 표시하고 연출을 재생한다.
    // 오버레이 페이드 인 → 패널 스케일 인 → 타이틀 펀치 순서로 진행된다.
    public void Show()
    {
      gameObject.SetActive(true);

      // 초기 상태 (애니메이션 시작 전)
      _overlay.color = new Color(0f, 0f, 0f, 0f);
      _panel.localScale = Vector3.zero;

      DOTween.Sequence()
          // DOFade는 UI 모듈이 필요하므로 DOVirtual.Float으로 대체
          .Append(DOVirtual.Float(0f, 0.75f, 0.25f,
              v => _overlay.color = new Color(0f, 0f, 0f, v)))
          .Append(_panel.DOScale(1f, 0.45f).SetEase(Ease.OutBack))
          .AppendCallback(() =>
              // 패널 등장 후 타이틀이 살짝 튀어오르는 느낌
              _titleRect.DOPunchScale(Vector3.one * 0.12f, 0.35f, 5, 0.5f));
    }

    // UI 계층구조를 코드로 생성한다.
    private void BuildUI()
    {
      // ── Canvas 설정 ─────────────────────────────────────
      var canvas = gameObject.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 100; // 모든 게임 오브젝트 위

      // 기준 해상도(1080×1920)에 맞게 스케일 (GameManager의 Camera와 동일 기준)
      var scaler = gameObject.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1080f, 1920f);
      scaler.matchWidthOrHeight = 0f; // 가로 폭 기준 스케일

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
      var panelGo = new GameObject("Panel");
      panelGo.transform.SetParent(transform, false);
      _panel = panelGo.AddComponent<RectTransform>();
      _panel.sizeDelta = new Vector2(700f, 480f);
      _panel.anchoredPosition = Vector2.zero;
      panelGo.AddComponent<Image>().color = Color.white;

      // ── 클리어 타이틀 ────────────────────────────────────
      // #FF8A8A : 흰 배경에서 잘 보이도록 딥 핑크 사용
      var titleGo = CreateText(panelGo.transform, "클리어!", 96f,
          new Vector2(0f, 130f), new Color(1f, 0.541f, 0.541f, 1f));
      _titleRect = titleGo.GetComponent<RectTransform>();

      // ── 버튼 두 개 ──────────────────────────────────────
      // 다시하기: 연한 핑크 계열 (보조 버튼)
      CreateButton(panelGo.transform, "다시하기",
          new Vector2(-165f, -120f),
          new Color(1f, 0.761f, 0.761f, 1f),
          () => _onRetry?.Invoke());

      // 다음 레벨: #FF8A8A 브랜드 딥 핑크 (주 버튼)
      CreateButton(panelGo.transform, "다음 레벨",
          new Vector2(165f, -120f),
          new Color(1f, 0.541f, 0.541f, 1f),
          () => _onNext?.Invoke());
    }

    // 텍스트 오브젝트를 생성해서 parent에 붙인다.
    private GameObject CreateText(Transform parent, string content, float size, Vector2 pos, Color color)
    {
      var go = new GameObject("Text");
      go.transform.SetParent(parent, false);
      var rect = go.AddComponent<RectTransform>();
      rect.sizeDelta = new Vector2(660f, 160f);
      rect.anchoredPosition = pos;
      var tmp = go.AddComponent<TextMeshProUGUI>();
      if (_font != null) tmp.font = _font; // 한글 폰트 적용
      tmp.text = content;
      tmp.fontSize = size;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = color;
      tmp.fontStyle = FontStyles.Bold;
      return go;
    }

    // 버튼 오브젝트를 생성해서 parent에 붙인다.
    private void CreateButton(Transform parent, string label, Vector2 pos, Color bgColor, Action onClick)
    {
      var btnGo = new GameObject("Btn_" + label);
      btnGo.transform.SetParent(parent, false);
      var rect = btnGo.AddComponent<RectTransform>();
      rect.sizeDelta = new Vector2(290f, 110f);
      rect.anchoredPosition = pos;
      btnGo.AddComponent<Image>().color = bgColor;
      var btn = btnGo.AddComponent<Button>();
      btn.onClick.AddListener(() => onClick());

      // 버튼 레이블
      var txtGo = new GameObject("Label");
      txtGo.transform.SetParent(btnGo.transform, false);
      var txtRect = txtGo.AddComponent<RectTransform>();
      txtRect.anchorMin = Vector2.zero;
      txtRect.anchorMax = Vector2.one;
      txtRect.offsetMin = txtRect.offsetMax = Vector2.zero;
      var tmp = txtGo.AddComponent<TextMeshProUGUI>();
      if (_font != null) tmp.font = _font; // 한글 폰트 적용
      tmp.text = label;
      tmp.fontSize = 44f;
      tmp.alignment = TextAlignmentOptions.Center;
      tmp.color = Color.white;
      tmp.fontStyle = FontStyles.Bold;
    }
  }
}
