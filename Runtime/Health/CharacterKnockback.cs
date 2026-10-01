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
                if (_lockoutRoutine != null) { StopCoroutine(_lockoutRoutine); SetAbilitiesEnabled(true); }
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

        private bool[] _enabledBeforeLockout;

        // Al terminar se restaura el estado previo de cada habilidad, no "todas activas": así no se
        // reactivan habilidades que el juego (o CharacterCore, por un error) había desactivado.
        private void SetAbilitiesEnabled(bool enabled)
        {
            if (!enabled)
            {
                _enabledBeforeLockout = new bool[_abilities.Length];
                for (int i = 0; i < _abilities.Length; i++)
                {
                    if (_abilities[i] == null) continue;
                    _enabledBeforeLockout[i] = _abilities[i].AbilityEnabled;
                    _abilities[i].AbilityEnabled = false;
                }
            }
            else if (_enabledBeforeLockout != null)
            {
                for (int i = 0; i < _abilities.Length; i++)
                {
                    if (_abilities[i] != null) _abilities[i].AbilityEnabled = _enabledBeforeLockout[i];
                }
                _enabledBeforeLockout = null;
            }
        }
    }
}
