// 로딩 씬을 관리하는 스크립트
// 지정한 시간만큼 대기한 뒤 메인메뉴로 이동한다.
// 나중에 에셋 로드나 초기화 작업이 생기면 여기에 추가한다.

using System.Collections;
using UnityEngine;

namespace WaterSortPuzzle.UI
{
    public class LoadingSceneManager : MonoBehaviour
    {
        // 메인메뉴로 넘어가기 전 대기 시간 (초)
        [SerializeField] private float _loadingDuration = 1.5f;

        private void Start()
        {
            StartCoroutine(LoadMainMenu());
        }

        // 대기 후 메인메뉴로 이동하는 코루틴
        private IEnumerator LoadMainMenu()
        {
            yield return new WaitForSeconds(_loadingDuration);
            SceneLoader.LoadMainMenu();
        }
    }
}
