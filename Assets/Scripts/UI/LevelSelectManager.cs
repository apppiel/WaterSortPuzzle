// 레벨 선택 씬을 관리하는 스크립트
// 레벨 버튼을 자동으로 생성하고 진행 상태(잠김/도전가능/클리어)를 반영한다.

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

        // 버튼 상태별 색상
        private static readonly Color ColorLocked    = new Color(0.75f, 0.75f, 0.75f); // 잠김 — 회색
        private static readonly Color ColorUnlocked  = Color.white;                     // 도전 가능 — 흰색
        private static readonly Color ColorClear     = new Color(1f, 0.55f, 0.55f);    // 클리어 — 딥핑크 (#FF8C8C)

        // 씬이 시작될 때 버튼을 생성한다
        private void Start()
        {
            GenerateLevelButtons();
        }

        // 레벨 수만큼 버튼을 생성하고 진행 상태를 반영한다
        private void GenerateLevelButtons()
        {
            for (int i = 0; i < _totalLevels; i++)
            {
                int levelIndex = i; // 클로저 문제 방지용 로컬 변수

                // 프리팹을 Content 안에 복사해서 생성
                GameObject buttonObj = Instantiate(_levelButtonPrefab, _content);

                // 버튼 텍스트에 레벨 번호 표시 (1부터 시작)
                TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Button button = buttonObj.GetComponent<Button>();
                Image image = buttonObj.GetComponent<Image>();

                bool isUnlocked = LevelProgressManager.IsUnlocked(levelIndex);
                bool isClear    = LevelProgressManager.IsClear(levelIndex);

                // 진행 상태에 따라 버튼 색상과 텍스트를 설정한다
                if (isClear)
                {
                    // 클리어: 딥핑크 배경 + 번호
                    if (image != null) image.color = ColorClear;
                    if (label != null) label.text = (levelIndex + 1).ToString();
                }
                else if (isUnlocked)
                {
                    // 도전 가능: 흰색 배경 + 번호
                    if (image != null) image.color = ColorUnlocked;
                    if (label != null) label.text = (levelIndex + 1).ToString();
                }
                else
                {
                    // 잠김: 회색 배경 + 번호 (흐리게 표시)
                    if (image != null) image.color = ColorLocked;
                    if (label != null)
                    {
                        label.text = (levelIndex + 1).ToString();
                        label.color = new Color(0.5f, 0.5f, 0.5f); // 글자도 회색으로
                    }
                }

                // 잠긴 레벨은 클릭 불가
                if (button != null)
                {
                    if (isUnlocked)
                        button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
                    else
                        button.interactable = false;
                }
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
