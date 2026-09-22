using System;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Reposiciona al personaje en el último checkpoint, manualmente o al morir.</summary>
    public class CharacterRespawn : MonoBehaviour
    {
        [Header("Respawn")]
        [SerializeField] private Transform initialCheckpoint;
        [SerializeField] private float respawnDelay = 1f;
        [SerializeField] private bool autoRespawnOnDeath = true;

        public event Action OnRespawn;

        private Transform _currentCheckpoint;
        private CharacterHealth _health;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _currentCheckpoint = initialCheckpoint;
            _health = GetComponent<CharacterHealth>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDeath -= HandleDeath;
        }

        public void SetCheckpoint(Transform checkpoint)
        {
            _currentCheckpoint = checkpoint;
        }

        private void HandleDeath()
        {
            if (autoRespawnOnDeath) Invoke(nameof(Respawn), respawnDelay);
        }

        public void Respawn()
        {
            if (_currentCheckpoint == null) return;

            transform.position = _currentCheckpoint.position;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            _health?.ResetHealth();
            OnRespawn?.Invoke();
        }
    }
}
