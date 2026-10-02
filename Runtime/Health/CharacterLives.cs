using System;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Vidas del personaje. Con CharacterRespawn, cada muerte consume una vida y sin vidas ya no reaparece:
    /// dispara OnGameOver y pone GameManager en GameOver si existe.</summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class CharacterLives : MonoBehaviour
    {
        [SerializeField] private int startingLives = 3;
        [SerializeField] private int maxLives = 9;

        public int Lives { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int> OnLivesChanged;
        public event Action OnGameOver;

        private CharacterHealth _health;

        private void Awake()
        {
            Lives = Mathf.Clamp(startingLives, 0, maxLives);
            _health = GetComponent<CharacterHealth>();
        }

        private void OnEnable() => _health.OnDeath += HandleDeath;
        private void OnDisable() => _health.OnDeath -= HandleDeath;

        // Con CharacterRespawn (reaparición automática) es él quien descuenta la vida al decidir si reaparece;
        // sin él, se descuenta aquí. Se decide al morir, así no importa el orden en que se agregaron.
        private void HandleDeath()
        {
            var respawn = GetComponent<CharacterRespawn>();
            if (respawn != null && respawn.isActiveAndEnabled && respawn.AutoRespawn) return;
            ConsumeLife();
        }

        /// <summary>Descuenta una vida. Devuelve true si todavía puede reaparecer.</summary>
        public bool ConsumeLife()
        {
            if (IsGameOver) return false;
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);
            if (Lives > 0) return true;

            IsGameOver = true;
            OnGameOver?.Invoke();
            if (GameplayKit.Managers.GameManager.Instance != null)
                GameplayKit.Managers.GameManager.Instance.SetState(GameplayKit.Managers.GameManager.GameState.GameOver);
            return false;
        }

        public void AddLife(int amount = 1)
        {
            if (amount <= 0 || IsGameOver) return;
            Lives = Mathf.Min(maxLives, Lives + amount);
            OnLivesChanged?.Invoke(Lives);
        }

        public void ResetLives()
        {
            IsGameOver = false;
            Lives = Mathf.Clamp(startingLives, 0, maxLives);
            OnLivesChanged?.Invoke(Lives);
        }
    }
}
