// 게임 효과음을 관리하는 MonoBehaviour.
// 인스펙터에서 클립을 연결하고, GameManager에서 호출해 재생한다.

using UnityEngine;

namespace WaterSortPuzzle.Game
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip _selectClip; // 튜브 선택 효과음
        [SerializeField] private AudioClip _pourClip;   // 붓기 성공 효과음
        [SerializeField] private AudioClip _failClip;   // 붓기 실패 효과음 (미할당 시 무음)
        [SerializeField] private AudioClip _clearClip;  // 레벨 클리어 효과음

        private AudioSource _audioSource; // 효과음 재생용 AudioSource

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        // 튜브 선택 시 호출한다.
        public void PlaySelect()
        {
            PlayClip(_selectClip);
        }

        // 붓기 성공 시 호출한다.
        public void PlayPour()
        {
            PlayClip(_pourClip);
        }

        // 붓기 실패 시 호출한다. (규칙 위반으로 부을 수 없는 대상 튜브를 눌렀을 때)
        public void PlayFail()
        {
            PlayClip(_failClip);
        }

        // 레벨 클리어 시 호출한다.
        public void PlayClear()
        {
            PlayClip(_clearClip);
        }

        // 클립이 연결돼 있고 효과음이 켜져 있을 때만 재생한다.
        private void PlayClip(AudioClip clip)
        {
            if (clip == null) return;
            if (!WaterSortPuzzle.UI.SettingsManager.SFXEnabled) return;
            _audioSource.PlayOneShot(clip);
        }
    }
}
