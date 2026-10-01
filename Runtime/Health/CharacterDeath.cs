using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Reacciona a CharacterHealth.OnDeath: dispara animación, congela las habilidades y marca ConditionState.Dead.</summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class CharacterDeath : MonoBehaviour
    {
        [Header("Muerte")]
        [SerializeField] private string deathAnimatorTrigger = "Die";
        [Tooltip("Segundos hasta desactivar el GameObject tras morir. -1 = no se desactiva.")]
        [SerializeField] private float disableDelay = -1f;

        public event Action OnCharacterDeath;

        private CharacterHealth _health;
        private CharacterCore _character;
        private Animator _animator;

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _character = GetComponent<CharacterCore>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable() => _health.OnDeath += HandleDeath;
        private void OnDisable() => _health.OnDeath -= HandleDeath;

        private void HandleDeath()
        {
            if (_character == null) _character = GetComponent<CharacterCore>();
            if (_character != null)
            {
                _character.Condition.ChangeState(ConditionState.Dead);
                _character.Suspend(this);
            }

            if (_animator != null && !string.IsNullOrEmpty(deathAnimatorTrigger))
            {
                _animator.SetTrigger(deathAnimatorTrigger);
            }

            OnCharacterDeath?.Invoke();

            if (disableDelay >= 0f) Invoke(nameof(DisableGameObject), disableDelay);
        }

        /// <summary>
        /// Deshace lo que hizo la muerte: reanuda las habilidades, vuelve a la condición Normal y reactiva
        /// el GameObject si se había desactivado. CharacterRespawn lo llama al respawnear.
        /// </summary>
        public void Revive()
        {
            CancelInvoke(nameof(DisableGameObject));
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (_character != null)
            {
                _character.Resume(this);
                _character.Condition.ChangeState(ConditionState.Normal);
            }
        }

        private void DisableGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}
