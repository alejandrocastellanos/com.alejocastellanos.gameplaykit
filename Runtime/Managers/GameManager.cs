using System;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Punto central de estado global de la partida (singleton liviano) — el resto de managers puede consultarlo entre sí.</summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { Playing, Paused, GameOver }

        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad solo funciona con objetos raíz.
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
