// 메인 메뉴 씬을 관리하는 스크립트
// 버튼 이벤트를 받아서 씬 전환을 처리한다.

using UnityEngine;

namespace WaterSortPuzzle.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        // 플레이 버튼을 눌렀을 때 호출된다
        public void OnPlayButtonClicked()
        {
            SceneLoader.LoadLevelSelect();
        }
    }
}
