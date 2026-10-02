using System.Collections;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Aplica un impulso de retroceso y pausa brevemente las habilidades al recibir un golpe,
    /// para que el input no anule el empujón.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterKnockback : MonoBehaviour
    {
        [Header("Knockback")]
        [Tooltip("Velocidad (u/s) del empujón, igual para cualquier golpe sin importar el daño ni la masa.")]
        [SerializeField] private float force = 8f;
        [Tooltip("Cuánto se inclina el empujón hacia arriba (0 = solo en la dirección del golpe). Ayuda a que se lea en el suelo.")]
        [SerializeField] private float upwardLift = 0.35f;
        [SerializeField] private float lockoutDuration = 0.2f;

        private Rigidbody2D _rb;
        private CharacterCore _character;
        private Coroutine _lockoutRoutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _character = GetComponent<CharacterCore>();
        }

        /// <summary>Empuja con la fuerza configurada en el Inspector.</summary>
        public void ApplyKnockback(Vector2 direction) => ApplyKnockback(direction, force);

        /// <summary>Empuja con una velocidad explícita (u/s) en lugar de la configurada.</summary>
        public void ApplyKnockback(Vector2 direction, float speed)
        {
            if (_rb == null || direction == Vector2.zero) return;

            Vector2 dir = direction.normalized;
            if (upwardLift > 0f) dir = (dir + Vector2.up * upwardLift).normalized;
            _rb.linearVelocity = dir * speed;

            if (lockoutDuration > 0f)
            {
                if (_lockoutRoutine != null) StopCoroutine(_lockoutRoutine);
                _lockoutRoutine = StartCoroutine(LockoutRoutine());
            }
        }

        private IEnumerator LockoutRoutine()
        {
            if (_character == null) _character = GetComponent<CharacterCore>();
            if (_character != null) _character.Suspend(this);
            yield return new WaitForSeconds(lockoutDuration);
            if (_character != null) _character.Resume(this);
            _lockoutRoutine = null;
        }

        private void OnDisable()
        {
            if (_character != null) _character.Resume(this);
            _lockoutRoutine = null;
        }
    }
}
