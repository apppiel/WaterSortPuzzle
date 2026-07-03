// 씬 이름을 상수로 모아두는 파일
// 코드 여러 곳에서 씬 이름을 문자열로 직접 쓰면 오타가 나기 쉽다.
// 여기서 한 번만 정의해두면 오타를 컴파일 단계에서 잡을 수 있다.

namespace WaterSortPuzzle.UI
{
    public static class SceneNames
    {
        public const string Loading    = "LoadingScene";    // 로딩 씬
        public const string MainMenu   = "MainMenuScene";   // 메인 메뉴 씬
        public const string LevelSelect = "LevelSelectScene"; // 레벨 선택 씬
        public const string Game       = "GameScene";       // 게임 씬
    }
}
