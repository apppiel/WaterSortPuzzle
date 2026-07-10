// 메인 메뉴 씬을 관리하는 스크립트
// 버튼 이벤트 처리 + 진입 애니메이션 + 배경 거품 효과

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WaterSortPuzzle.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset _koreanFont;     // 설정 팝업용 한글 폰트
        [SerializeField] private RectTransform _playButton;     // 플레이 버튼 (선택)
        [SerializeField] private RectTransform _settingsButton; // 설정 버튼 (선택)

        private SettingsPopup _settingsPopup;

        // 씬 시작 시 버튼 참조를 확보하고 연출을 시작한다
        private void Start()
        {
            ResolveButtonRefs();
            AddButtonEffects();
            SpawnFloatingBubbles();
            PlayEntranceAnimation();
        }

        // Inspector에 연결이 없으면 이름으로 찾는다
        private void ResolveButtonRefs()
        {
            if (_playButton == null)
            {
                var go = GameObject.Find("PlayButton");
                if (go != null) _playButton = go.GetComponent<RectTransform>();
            }
            if (_settingsButton == null)
            {
                var go = GameObject.Find("SettingsButton");
                if (go != null) _settingsButton = go.GetComponent<RectTransform>();
            }
        }

        // 버튼마다 UIButtonEffect 컴포넌트를 붙인다 (없으면 추가)
        private void AddButtonEffects()
        {
            AddEffectIfMissing(_playButton);
            AddEffectIfMissing(_settingsButton);
        }

        private void AddEffectIfMissing(RectTransform target)
        {
            if (target == null) return;
            if (target.GetComponent<UIButtonEffect>() == null)
                target.gameObject.AddComponent<UIButtonEffect>();
        }

        // 화면 진입 시 버튼들이 페이드 인 + 스케일 인으로 등장한다
        private void PlayEntranceAnimation()
        {
            AnimateButton(_settingsButton, delay: 0.05f, fromScale: 0.5f);
            AnimateButton(_playButton,     delay: 0.2f,  fromScale: 0.5f, onDone: StartPlayButtonFloat);
        }

        // 버튼 하나를 fadeIn + scaleIn으로 애니메이션한다
        private void AnimateButton(RectTransform btn, float delay, float fromScale, TweenCallback onDone = null)
        {
            if (btn == null) return;

            var cg = GetOrAddCanvasGroup(btn.gameObject);
            cg.alpha = 0f;
            btn.localScale = Vector3.one * fromScale;

            DOTween.Sequence()
                .AppendInterval(delay)
                .Append(DOTween.To(() => cg.alpha, v => cg.alpha = v, 1f, 0.35f))
                .Join(btn.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack))
                .OnComplete(onDone);
        }

        // 플레이 버튼 등장 후 살짝 위아래로 둥실거리는 루프 애니메이션
        private void StartPlayButtonFloat()
        {
            if (_playButton == null) return;

            Vector2 basePos = _playButton.anchoredPosition;
            DOTween.To(
                () => _playButton.anchoredPosition,
                v => _playButton.anchoredPosition = v,
                new Vector2(basePos.x, basePos.y + 12f),
                1.4f
            ).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        // 배경 위에 반투명 거품들이 천천히 떠오르는 효과
        private void SpawnFloatingBubbles()
        {
            // Canvas 하위에 Background(0) 다음, SafeArea(2) 앞에 삽입
            var root = new GameObject("Bubbles");
            root.transform.SetParent(transform, false);
            root.transform.SetSiblingIndex(1);

            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

            // 거품 색상 팔레트 (브랜드 핑크 계열 파스텔)
            Color[] palette =
            {
                new Color(1f,   0.76f, 0.76f, 0.45f), // 라이트 핑크
                new Color(1f,   0.54f, 0.54f, 0.30f), // 딥 핑크
                new Color(1f,   0.90f, 0.90f, 0.35f), // 연한 핑크
                new Color(0.85f,0.76f, 1f,    0.30f), // 연보라
                new Color(1f,   1f,    1f,    0.20f), // 흰색
            };

            float[] sizes = { 28f, 48f, 68f, 38f, 58f, 32f, 52f, 42f };

            var circleSprite = CreateCircleSprite();
            for (int i = 0; i < 8; i++)
                CreateBubble(rootRect, palette[i % palette.Length], sizes[i], i * 0.9f, circleSprite);
        }

        // 거품 하나를 생성하고 떠오르는 루프를 시작한다
        private void CreateBubble(RectTransform parent, Color color, float size, float initialDelay, Sprite sprite)
        {
            var go = new GameObject("Bubble");
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);

            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false; // 거품이 터치를 막지 않게 한다
            img.color = color;

            float startY = Random.Range(-1050f, -750f); // 화면 하단 아래에서 시작
            float startX = Random.Range(-430f, 430f);
            rect.anchoredPosition = new Vector2(startX, startY);

            // 초기 딜레이 후 떠오르기 시작
            DOVirtual.DelayedCall(initialDelay, () => LoopBubble(rect, img, color, startY));
        }

        // 거품 하나의 떠오르기 → 페이드 아웃 → 리셋 루프
        private void LoopBubble(RectTransform rect, Image img, Color baseColor, float startY)
        {
            float duration = Random.Range(5.5f, 10f);
            float distance = Random.Range(2300f, 2700f);
            float newX = Random.Range(-430f, 430f);

            rect.anchoredPosition = new Vector2(newX, startY);
            img.color = baseColor;

            DOTween.Sequence()
                .Append(DOTween.To(
                    () => rect.anchoredPosition,
                    v => rect.anchoredPosition = v,
                    new Vector2(newX, startY + distance),
                    duration
                ).SetEase(Ease.Linear))
                // 올라가는 후반부에 서서히 사라진다
                .Join(DOVirtual.Float(baseColor.a, 0f, duration * 0.55f, v =>
                {
                    var c = img.color;
                    c.a = v;
                    img.color = c;
                }).SetDelay(duration * 0.45f))
                .OnComplete(() => LoopBubble(rect, img, baseColor, startY));
        }

        // 원형 스프라이트를 코드로 생성한다 (거품 모양용)
        private Sprite CreateCircleSprite()
        {
            const int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = res / 2f;
            float radius = center - 1f;
            var pixels = new Color[res * res];

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    // 가장자리를 부드럽게 처리
                    float a = Mathf.Clamp01(1f - Mathf.InverseLerp(radius - 2f, radius, dist));
                    pixels[y * res + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f);
        }

        // CanvasGroup이 없으면 추가한다
        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            return cg;
        }

        // 플레이 버튼을 눌렀을 때 호출된다
        public void OnPlayButtonClicked()
        {
            SceneLoader.LoadLevelSelect();
        }

        // 설정 버튼을 눌렀을 때 호출된다
        public void OnSettingsButtonClicked()
        {
            if (_settingsPopup == null)
            {
                var go = new GameObject("SettingsPopup");
                _settingsPopup = go.AddComponent<SettingsPopup>();
                _settingsPopup.Init(_koreanFont);
            }
            _settingsPopup.Show();
        }
    }
}
