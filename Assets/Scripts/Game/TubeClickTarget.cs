using System;
using UnityEngine;

namespace WaterSortPuzzle.Game
{
    /// <summary>
    /// 튜브 배경 오브젝트에 붙이는 작은 컴포넌트.
    /// Physics2D.OverlapPoint로 클릭된 Collider2D를 찾았을 때,
    /// 어느 튜브 인덱스가 클릭됐는지 GameManager에 전달하기 위해 사용한다.
    /// </summary>
    public class TubeClickTarget : MonoBehaviour
    {
        /// <summary>이 튜브의 인덱스 (0번부터 시작)</summary>
        public int TubeIndex;

        /// <summary>클릭 시 호출할 콜백 (GameManager.HandleTubeClicked)</summary>
        public Action<int> OnClicked;
    }
}
