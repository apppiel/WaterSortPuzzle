using System;
using UnityEngine;

namespace WaterSortPuzzle.Game
{
    public class TubeClickTarget : MonoBehaviour
    {
        public int TubeIndex;
        public Action<int> OnClicked;
    }
}
