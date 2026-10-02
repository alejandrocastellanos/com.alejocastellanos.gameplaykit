using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayKit.Managers
{
    /// <summary>Carga escenas con un fundido a negro. Si no se asigna un CanvasGroup, crea su propia capa de fundido
    /// (que sobrevive al cambio de escena junto con el manager). Ignora pedidos mientras ya hay una transición.</summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transición")]
        [Tooltip("Capa a fundir. Vacío = se crea una negra a pantalla completa. Si asignas una, debe ser hija de este objeto.")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (transform.parent != null) transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject);
            if (fadeCanvasGroup == null) fadeCanvasGroup = CreateFadeLayer();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void LoadScene(string sceneName)
        {
            if (IsTransitioning) return;
            StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(sceneName)));
        }

        public void LoadScene(int buildIndex)
        {
            if (IsTransitioning) return;
            StartCoroutine(LoadSceneRoutine(() => SceneManager.LoadSceneAsync(buildIndex)));
        }

        private IEnumerator LoadSceneRoutine(System.Func<AsyncOperation> load)
        {
            IsTransitioning = true;
            fadeCanvasGroup.blocksRaycasts = true;
            yield return Fade(1f);
            Time.timeScale = 1f; // por si se cambió de escena desde un menú de pausa
            AsyncOperation asyncLoad = load();
            while (asyncLoad != null && !asyncLoad.isDone) yield return null;
            yield return Fade(0f);
            fadeCanvasGroup.blocksRaycasts = false;
            IsTransitioning = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                elapsed += Time.unscaledDeltaTime; // funciona aunque el juego esté en pausa
                yield return null;
            }
            fadeCanvasGroup.alpha = targetAlpha;
        }

        private CanvasGroup CreateFadeLayer()
        {
            var go = new GameObject("FadeLayer", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = fadeColor;
            image.raycastTarget = true;
            return go.AddComponent<CanvasGroup>();
        }
    }
}
