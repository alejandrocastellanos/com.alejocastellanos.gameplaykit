using System.Collections;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Aplica un impulso de retroceso y bloquea brevemente las abilities al recibir un golpe.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterKnockback : MonoBehaviour
    {
        [Header("Knockback")]
        [SerializeField] private float lockoutDuration = 0.2f;

        private Rigidbody2D _rb;
        private AbilityBase[] _abilities;
        private Coroutine _lockoutRoutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _abilities = GetComponentsInChildren<AbilityBase>();
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
            SetAbilitiesEnabled(false);
            yield return new WaitForSeconds(lockoutDuration);
            SetAbilitiesEnabled(true);
            _lockoutRoutine = null;
        }

        private void SetAbilitiesEnabled(bool enabled)
        {
            foreach (var ability in _abilities)
            {
                if (ability != null) ability.AbilityEnabled = enabled;
            }
        }
    }
}
