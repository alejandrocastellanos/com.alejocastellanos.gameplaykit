using System.Collections;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Reproduce música y efectos con mezcla y fundido entre pistas: dos AudioSource en crossfade para música y un AudioSource dedicado para SFX.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Música")]
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        [SerializeField] private float musicFadeDuration = 1f;

        [Header("Efectos")]
        [SerializeField] private AudioSource sfxSource;

        private AudioSource _activeMusicSource;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _activeMusicSource = musicSourceA;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(CrossfadeMusic(clip, loop));
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip, volume);
        }

        private IEnumerator CrossfadeMusic(AudioClip clip, bool loop)
        {
            AudioSource incoming = _activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
            AudioSource outgoing = _activeMusicSource;

            if (incoming == null)
            {
                _fadeRoutine = null;
                yield break;
            }

            incoming.clip = clip;
            incoming.loop = loop;
            incoming.volume = 0f;
            incoming.Play();

            float elapsed = 0f;
            float outgoingStartVolume = outgoing != null ? outgoing.volume : 0f;

            while (elapsed < musicFadeDuration)
            {
                float t = elapsed / musicFadeDuration;
                incoming.volume = Mathf.Lerp(0f, 1f, t);
                if (outgoing != null) outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            incoming.volume = 1f;
            if (outgoing != null)
            {
                outgoing.volume = 0f;
                outgoing.Stop();
            }

            _activeMusicSource = incoming;
            _fadeRoutine = null;
        }
    }
}
