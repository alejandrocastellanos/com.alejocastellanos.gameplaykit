using System;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Controla el checkpoint activo y los límites del nivel actual (útil para CameraBounds o para detectar caída al vacío).</summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Nivel")]
        [SerializeField] private Vector2 levelMinBounds;
        [SerializeField] private Vector2 levelMaxBounds;
        [SerializeField] private float voidY = -30f;
        [Tooltip("Mata a cualquier personaje con CharacterHealth que caiga por debajo de Void Y (normalmente reaparece en el último checkpoint).")]
        [SerializeField] private bool killBelowVoid = true;

        public Transform ActiveCheckpoint { get; private set; }
        public event Action<Transform> OnCheckpointChanged;

        public Vector2 LevelMinBounds => levelMinBounds;
        public Vector2 LevelMaxBounds => levelMaxBounds;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetActiveCheckpoint(Transform checkpoint)
        {
            ActiveCheckpoint = checkpoint;
            OnCheckpointChanged?.Invoke(checkpoint);
        }

        public bool IsBelowVoid(float worldY) => worldY < voidY;
        public bool HasBounds => levelMaxBounds.x > levelMinBounds.x && levelMaxBounds.y > levelMinBounds.y;

        private void Update()
        {
            if (!killBelowVoid) return;
            foreach (var health in GameplayKit.Health.CharacterHealth.Active)
            {
                if (health != null && !health.IsDead && IsBelowVoid(health.transform.position.y))
                    health.TakeDamage(float.MaxValue);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
