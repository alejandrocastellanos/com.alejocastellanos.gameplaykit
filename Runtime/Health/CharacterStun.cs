using System.Collections;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Aturde al personaje: desactiva temporalmente todas sus abilities y marca ConditionState.Stunned.
    /// No requiere CharacterCore (funciona con solo un Rigidbody2D), pero si existe lo usa para el estado.
    /// </summary>
    public class CharacterStun : MonoBehaviour
    {
        [SerializeField] private float defaultStunDuration = 1f;

        public bool IsStunned { get; private set; }

        private CharacterCore _character;
        private AbilityBase[] _abilities;
        private Coroutine _stunRoutine;

        private void Awake()
        {
            _character = GetComponent<CharacterCore>();
            _abilities = GetComponentsInChildren<AbilityBase>();
        }

        public void Stun() => Stun(defaultStunDuration);

        public void Stun(float duration)
        {
            if (_stunRoutine != null) StopCoroutine(_stunRoutine);
            _stunRoutine = StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            IsStunned = true;
            if (_character != null) _character.Condition.ChangeState(ConditionState.Stunned);
            SetAbilitiesEnabled(false);

            yield return new WaitForSeconds(duration);

            SetAbilitiesEnabled(true);
            if (_character != null && _character.Condition.CurrentState == ConditionState.Stunned)
            {
                _character.Condition.ChangeState(ConditionState.Normal);
            }

            IsStunned = false;
            _stunRoutine = null;
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
