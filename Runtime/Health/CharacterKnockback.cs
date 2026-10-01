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
        [SerializeField] private float lockoutDuration = 0.2f;

        private Rigidbody2D _rb;
        private CharacterCore _character;
        private Coroutine _lockoutRoutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _character = GetComponent<CharacterCore>();
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            if (_rb == null) return;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

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
