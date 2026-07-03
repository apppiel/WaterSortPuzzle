// 레벨 선택 씬을 관리하는 스크립트
// 레벨 버튼을 자동으로 생성하고 클릭 이벤트를 처리한다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WaterSortPuzzle.UI
{
    public class LevelSelectManager : MonoBehaviour
    {
        [SerializeField] private GameObject _levelButtonPrefab; // 레벨 버튼 프리팹
        [SerializeField] private Transform _content;            // 버튼들이 들어갈 부모 오브젝트 (Content)
        [SerializeField] private int _totalLevels = 100;        // 총 레벨 수

        // 씬이 시작될 때 버튼을 생성한다
        private void Start()
        {
            GenerateLevelButtons();
        }

        // 레벨 수만큼 버튼을 생성하고 번호를 붙인다
        private void GenerateLevelButtons()
        {
            for (int i = 0; i < _totalLevels; i++)
            {
                int levelIndex = i; // 클로저 문제 방지용 로컬 변수

                // 프리팹을 Content 안에 복사해서 생성
                GameObject buttonObj = Instantiate(_levelButtonPrefab, _content);

                // 버튼 텍스트에 레벨 번호 표시 (1부터 시작)
                TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = (levelIndex + 1).ToString();

                // 버튼 클릭 시 해당 레벨로 이동
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
            }
        }

        // 레벨 버튼을 눌렀을 때 호출된다
        private void OnLevelButtonClicked(int levelIndex)
        {
            SceneLoader.LoadGame(levelIndex);
        }

        // 뒤로가기 버튼을 눌렀을 때 메인메뉴로 이동
        public void OnBackButtonClicked()
        {
            SceneLoader.LoadMainMenu();
        }
    }
}
