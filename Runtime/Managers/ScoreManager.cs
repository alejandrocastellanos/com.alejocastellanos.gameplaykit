using System;
using GameplayKit.Environment;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Lleva el puntaje de la partida y dispara un evento al cambiar. Se suscribe automáticamente a Collectible.OnAnyCollected.</summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            Collectible.OnAnyCollected += HandleCollected;
        }

        private void OnDisable()
        {
            Collectible.OnAnyCollected -= HandleCollected;
        }

        private void HandleCollected(Collectible collectible, int value)
        {
            AddScore(value);
        }

        public void AddScore(int amount)
        {
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
