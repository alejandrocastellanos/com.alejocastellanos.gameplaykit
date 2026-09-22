using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Reacciona a CharacterHealth.OnDeath: dispara animación, congela abilities y marca ConditionState.Dead.</summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class CharacterDeath : MonoBehaviour
    {
        [Header("Muerte")]
        [SerializeField] private string deathAnimatorTrigger = "Die";
        [SerializeField] private float disableDelay = -1f; // -1 = no desactiva el GameObject automáticamente

        public event Action OnCharacterDeath;

        private CharacterHealth _health;
        private CharacterCore _character;
        private Animator _animator;
        private AbilityBase[] _abilities;

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _character = GetComponent<CharacterCore>();
            _animator = GetComponentInChildren<Animator>();
            _abilities = GetComponentsInChildren<AbilityBase>();
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (_character != null) _character.Condition.ChangeState(ConditionState.Dead);

            foreach (var ability in _abilities)
            {
                if (ability != null) ability.AbilityEnabled = false;
            }

            if (_animator != null && !string.IsNullOrEmpty(deathAnimatorTrigger))
            {
                _animator.SetTrigger(deathAnimatorTrigger);
            }

            OnCharacterDeath?.Invoke();

            if (disableDelay >= 0f) Invoke(nameof(DisableGameObject), disableDelay);
        }

        private void DisableGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}
