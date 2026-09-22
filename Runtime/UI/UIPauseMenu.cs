using GameplayKit.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayKit.UI
{
    /// <summary>Menú de pausa básico (reanudar, reiniciar, salir), mostrado/ocultado escuchando a PauseManager.</summary>
    public class UIPauseMenu : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void OnEnable()
        {
            if (PauseManager.Instance != null) PauseManager.Instance.OnPauseChanged += HandlePauseChanged;
        }

        private void OnDisable()
        {
            if (PauseManager.Instance != null) PauseManager.Instance.OnPauseChanged -= HandlePauseChanged;
        }

        private void HandlePauseChanged(bool isPaused)
        {
            if (menuRoot != null) menuRoot.SetActive(isPaused);
        }

        public void OnResumeButtonPressed()
        {
            PauseManager.Instance?.SetPaused(false);
        }

        public void OnRestartButtonPressed()
        {
            PauseManager.Instance?.SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnQuitButtonPressed()
        {
            PauseManager.Instance?.SetPaused(false);
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
