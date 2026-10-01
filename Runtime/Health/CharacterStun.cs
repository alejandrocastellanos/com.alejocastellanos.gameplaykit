using System.Collections;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Aturde al personaje: pausa sus habilidades y marca ConditionState.Stunned durante un tiempo.
    /// Sin CharacterCore solo expone IsStunned (útil para enemigos con scripts propios).
    /// </summary>
    public class CharacterStun : MonoBehaviour
    {
        [SerializeField] private float defaultStunDuration = 1f;

        public bool IsStunned { get; private set; }

        private CharacterCore _character;
        private Coroutine _stunRoutine;

        private void Awake()
        {
            _character = GetComponent<CharacterCore>();
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
            if (_character == null) _character = GetComponent<CharacterCore>();
            if (_character != null)
            {
                _character.Condition.ChangeState(ConditionState.Stunned);
                _character.Suspend(this);
            }

            yield return new WaitForSeconds(duration);

            if (_character != null)
            {
                _character.Resume(this);
                if (_character.Condition.CurrentState == ConditionState.Stunned)
                    _character.Condition.ChangeState(ConditionState.Normal);
            }
            IsStunned = false;
            _stunRoutine = null;
        }

        private void OnDisable()
        {
            // Si se desactiva a mitad del aturdimiento, no dejar al personaje pausado para siempre.
            if (_character != null) _character.Resume(this);
            IsStunned = false;
            _stunRoutine = null;
        }
    }
}
