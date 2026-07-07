// 메인 메뉴 씬을 관리하는 스크립트
// 버튼 이벤트를 받아서 씬 전환을 처리한다.

using UnityEngine;
using TMPro;

namespace WaterSortPuzzle.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset _koreanFont; // 설정 팝업용 한글 폰트

        private SettingsPopup _settingsPopup; // 설정 팝업 (첫 호출 시 생성)

        // 플레이 버튼을 눌렀을 때 호출된다
        public void OnPlayButtonClicked()
        {
            SceneLoader.LoadLevelSelect();
        }

        // 설정 버튼을 눌렀을 때 호출된다
        public void OnSettingsButtonClicked()
        {
            // 팝업이 없으면 처음 한 번만 생성
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
