using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 게임 씬 전용 사운드 매니저입니다.
    // 현재는 MVP 루프의 BGM과 전투/채집 효과음 재생 기반만 준비합니다.
    // BGM과 효과음 AudioSource는 Inspector에서 연결하고, 다른 스크립트는 이 매니저 함수만 호출합니다.
    public sealed class SoundManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private AudioSource AudioSource_Bgm;
        [SerializeField] private AudioSource AudioSource_Effect;
        [SerializeField] private float _defaultVolume = 0.3f;

        private bool _isInitialized;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            SetVolume(_defaultVolume);
            _isInitialized = true;
        }

        public void PlayBgm(AudioClip bgmClip)
        {
            if (AudioSource_Bgm == null || bgmClip == null)
            {
                return;
            }

            AudioSource_Bgm.clip = bgmClip;
            AudioSource_Bgm.loop = true;
            AudioSource_Bgm.Play();
        }

        public void StopBgm()
        {
            if (AudioSource_Bgm == null)
            {
                return;
            }

            AudioSource_Bgm.Stop();
        }

        public void PlayEffect(AudioClip effectClip)
        {
            if (AudioSource_Effect == null || effectClip == null)
            {
                return;
            }

            AudioSource_Effect.PlayOneShot(effectClip);
        }

        public void SetVolume(float volume)
        {
            float safeVolume = Mathf.Clamp01(volume);

            if (AudioSource_Bgm != null)
            {
                AudioSource_Bgm.volume = safeVolume;
            }

            if (AudioSource_Effect != null)
            {
                AudioSource_Effect.volume = safeVolume;
            }
        }
    }
}
