using System;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Pausa/reanuda el tiempo del juego (Time.timeScale) y notifica a la UI correspondiente.</summary>
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        public bool IsPaused { get; private set; }
        public event Action<bool> OnPauseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            OnPauseChanged?.Invoke(paused);
        }
    }
}
