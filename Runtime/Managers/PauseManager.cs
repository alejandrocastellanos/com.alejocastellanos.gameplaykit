using System;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Pausa/reanuda el tiempo del juego (Time.timeScale) y notifica a la UI correspondiente.</summary>
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        [Tooltip("Tecla para pausar/reanudar. None = solo por código o botones de UI.")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private GameplayKit.Core.InputCompat.PadButton pausePadButton = GameplayKit.Core.InputCompat.PadButton.Start;

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
            // En el editor un timeScale = 0 puede quedar guardado de una sesión anterior en pausa.
            Time.timeScale = 1f;
        }

        // Se "arma" solo después de ver la tecla suelta: una tecla que ya venía presionada al cargar la escena
        // (o que el sistema reporta presionada al entrar a Play) no pausa el juego por sorpresa.
        private bool _armed;
        private float _enabledAt;
        private const float StartupGrace = 1f;

        private void OnEnable()
        {
            _armed = false;
            _enabledAt = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            bool held = GameplayKit.Core.InputCompat.GetKey(pauseKey) || GameplayKit.Core.InputCompat.GetPadButton(pausePadButton);
            // Al entrar a Play el sistema de input puede reportar pulsaciones fantasma en los primeros frames.
            if (Time.realtimeSinceStartup - _enabledAt < StartupGrace) return;
            if (!_armed)
            {
                if (!held) _armed = true;
                return;
            }

            if (GameplayKit.Core.InputCompat.GetKeyDown(pauseKey) || GameplayKit.Core.InputCompat.GetPadButtonDown(pausePadButton))
                TogglePause();
        }

        private void OnDestroy()
        {
            // Si se destruye en pausa (cambio de escena), no dejar el juego congelado.
            if (Instance == this) { if (IsPaused) Time.timeScale = 1f; Instance = null; }
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            // Refleja la pausa en el estado global (sin pisar un GameOver).
            var game = GameManager.Instance;
            if (game != null)
            {
                if (paused && game.CurrentState == GameManager.GameState.Playing) game.SetState(GameManager.GameState.Paused);
                else if (!paused && game.CurrentState == GameManager.GameState.Paused) game.SetState(GameManager.GameState.Playing);
            }

            OnPauseChanged?.Invoke(paused);
        }
    }
}
