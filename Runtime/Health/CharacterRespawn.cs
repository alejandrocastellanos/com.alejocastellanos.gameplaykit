using System;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Reposiciona al personaje en el último checkpoint, manualmente o al morir.</summary>
    public class CharacterRespawn : MonoBehaviour
    {
        [Header("Respawn")]
        [Tooltip("Punto de reaparición inicial. Si se deja vacío se reaparece donde empezó el personaje.")]
        [SerializeField] private Transform initialCheckpoint;
        [SerializeField] private float respawnDelay = 1f;
        [SerializeField] private bool autoRespawnOnDeath = true;

        public event Action OnRespawn;

        private Transform _currentCheckpoint;
        private Vector3 _startPosition;
        private CharacterHealth _health;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _currentCheckpoint = initialCheckpoint;
            _startPosition = transform.position;
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
            transform.position = _currentCheckpoint != null ? _currentCheckpoint.position : _startPosition;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            if (_health != null) _health.ResetHealth();

            // Sin esto el personaje reaparecía con la vida llena pero congelado: CharacterDeath había
            // desactivado sus habilidades y dejado la condición en Dead.
            var death = GetComponent<CharacterDeath>();
            if (death != null) death.Revive();
            var core = GetComponent<GameplayKit.Core.CharacterCore>();
            if (core != null) core.ResetCharacter();

            OnRespawn?.Invoke();
        }
    }
}
