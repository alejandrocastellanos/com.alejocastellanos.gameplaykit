using GameplayKit.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayKit.UI
{
    /// <summary>Menú de pausa básico (reanudar, reiniciar, salir), mostrado/ocultado escuchando a PauseManager.
    /// Ponlo en un objeto que siempre esté activo (el Canvas) y asigna como Menu Root el panel a mostrar.</summary>
    public class UIPauseMenu : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private PauseManager _subscribed;

        // Start además de OnEnable: el PauseManager puede despertar después que este menú.
        private void OnEnable() => TrySubscribe();
        private void Start()
        {
            TrySubscribe();
            if (menuRoot != null) menuRoot.SetActive(PauseManager.Instance != null && PauseManager.Instance.IsPaused);
        }

        private void OnDisable()
        {
            if (_subscribed != null) _subscribed.OnPauseChanged -= HandlePauseChanged;
            _subscribed = null;
        }

        private void TrySubscribe()
        {
            var manager = PauseManager.Instance;
            if (manager == null || manager == _subscribed) return;
            if (_subscribed != null) _subscribed.OnPauseChanged -= HandlePauseChanged;
            manager.OnPauseChanged += HandlePauseChanged;
            _subscribed = manager;
        }

        private void HandlePauseChanged(bool isPaused)
        {
            if (menuRoot != null) menuRoot.SetActive(isPaused);
        }

        public void OnResumeButtonPressed()
        {
            if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);
        }

        public void OnRestartButtonPressed()
        {
            if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) { SceneManager.LoadScene(scene.buildIndex); return; }
#if UNITY_EDITOR
            // En el editor se puede recargar aunque la escena no esté en Build Profiles (ej. la escena demo).
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Debug.LogError($"[GameplayKit] No se puede reiniciar '{scene.name}': agrégala a la lista de escenas del build.");
#endif
        }

        public void OnQuitButtonPressed()
        {
            if (PauseManager.Instance != null) PauseManager.Instance.SetPaused(false);
            if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
            else Debug.LogWarning($"[GameplayKit] La escena '{mainMenuSceneName}' no está en la lista de escenas del build.");
        }
    }
}
