// 씬 전환을 담당하는 유틸리티 클래스
// 게임 어디서든 SceneLoader.Load(씬이름) 으로 씬을 이동할 수 있다.
// static 클래스라 MonoBehaviour 없이도 사용 가능하다.

using UnityEngine.SceneManagement;

namespace WaterSortPuzzle.UI
{
    public static class SceneLoader
    {
        // 씬 이름을 받아서 이동한다
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        // 메인 메뉴로 이동
        public static void LoadMainMenu()
        {
            Load(SceneNames.MainMenu);
        }

        // 레벨 선택 화면으로 이동
        public static void LoadLevelSelect()
        {
            Load(SceneNames.LevelSelect);
        }

        // 특정 레벨(게임 씬)으로 이동
        // levelIndex: 플레이할 레벨 번호 (0부터 시작)
        public static void LoadGame(int levelIndex)
        {
            // 레벨 번호를 PlayerPrefs에 저장해두고 게임 씬에서 읽어간다
            UnityEngine.PlayerPrefs.SetInt("SelectedLevel", levelIndex);
            Load(SceneNames.Game);
        }
    }
}
