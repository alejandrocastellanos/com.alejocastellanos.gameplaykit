using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayKit.Managers
{
    /// <summary>Maneja fundidos (fade in/out) y la carga de escenas, usando un CanvasGroup como cortina.</summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transición")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            yield return Fade(1f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (asyncLoad != null && !asyncLoad.isDone) yield return null;

            yield return Fade(0f);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeCanvasGroup == null) yield break;

            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}
