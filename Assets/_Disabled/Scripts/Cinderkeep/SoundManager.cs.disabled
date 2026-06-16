using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource AudioSource_EffectSource;

        public static SoundManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            if (AudioSource_EffectSource == null)
            {
                AudioSource_EffectSource = GetComponent<AudioSource>();
            }
        }

        public void PlayOneShot(AudioClip audioClip, float volume = 1f)
        {
            if (AudioSource_EffectSource == null || audioClip == null)
            {
                return;
            }

            AudioSource_EffectSource.PlayOneShot(audioClip, Mathf.Clamp01(volume));
        }
    }
}
