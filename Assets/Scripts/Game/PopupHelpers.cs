using UnityEngine;
using UnityEngine.UI;

namespace WaterSortPuzzle.Game
{
  // 팝업 UI 공통 유틸.
  // Menu/Clear/Reward 팝업은 모든 배경 Image 를 라운드 스프라이트로 9-slicing 해서
  // 살짝 둥근 모서리를 갖도록 통일한다. 색상은 호출자가 별도로 지정.
  // (Overlay 는 전체 화면 커버라 대상 아님.)
  public static class PopupHelpers
  {
    // 라운드 반경 (픽셀). 9-slice 로 어느 사이즈 패널이든 이 반경이 고정된다.
    private const int Radius = 15;
    // 텍스처 총 크기 = 반경 * 2 + 여유 2px (센터 stretch 영역 최소).
    private const int Size = Radius * 2 + 2;

    // 프로그래매틱으로 생성한 흰색 라운드 사각형 스프라이트 (9-sliced).
    // Unity 6 에선 UI/Skin/UISprite.psd 내장 리소스 경로가 사라져 GetBuiltinResource 로 못 가져옴.
    // 코드로 만들면 버전 독립 + 반경 자유 조절.
    private static Sprite _roundedSprite;
    private static Sprite RoundedSprite
    {
      get
      {
        if (_roundedSprite == null)
          _roundedSprite = BuildRoundedSprite();
        return _roundedSprite;
      }
    }

    // GameObject 에 라운드 배경 Image 를 붙여 반환한다. color 는 tint 로 적용.
    public static Image AddRoundedImage(GameObject go, Color color)
    {
      var img = go.AddComponent<Image>();
      img.sprite = RoundedSprite;
      img.type = Image.Type.Sliced;
      img.color = color;
      return img;
    }

    // 흰색 라운드 사각형 텍스처를 만들고 border 정보와 함께 Sprite 로 감싼다.
    // 코너는 1px 페더링으로 계단 현상 완화.
    private static Sprite BuildRoundedSprite()
    {
      var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
      tex.filterMode = FilterMode.Bilinear;
      tex.wrapMode = TextureWrapMode.Clamp;

      var pixels = new Color32[Size * Size];
      for (int y = 0; y < Size; y++)
      {
        for (int x = 0; x < Size; x++)
        {
          // 코너 중심으로부터의 거리 계산.
          // 픽셀 좌표를 [Radius, Size-Radius-1] 범위로 clamp 하면 자연스럽게
          // 중앙 flat 영역은 dx=dy=0, 코너 영역은 해당 코너 중심점까지의 거리.
          int cx = Mathf.Clamp(x, Radius, Size - Radius - 1);
          int cy = Mathf.Clamp(y, Radius, Size - Radius - 1);
          float dx = x - cx;
          float dy = y - cy;
          float dist = Mathf.Sqrt(dx * dx + dy * dy);
          // 반경 경계에서 1px 페더링 (안티에일리어싱)
          float alpha = Mathf.Clamp01(Radius + 0.5f - dist);
          byte a = (byte)(alpha * 255f);
          pixels[y * Size + x] = new Color32(255, 255, 255, a);
        }
      }
      tex.SetPixels32(pixels);
      tex.Apply();

      // 9-slice border = 코너 크기. 중앙은 스트레치, 코너는 원본 유지.
      var border = new Vector4(Radius, Radius, Radius, Radius);
      return Sprite.Create(
        tex,
        new Rect(0, 0, Size, Size),
        new Vector2(0.5f, 0.5f),
        100f,
        0,
        SpriteMeshType.FullRect,
        border);
    }
  }
}
