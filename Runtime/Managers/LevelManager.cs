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
        [SerializeField] private float voidY = -20f;

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
    }
}
