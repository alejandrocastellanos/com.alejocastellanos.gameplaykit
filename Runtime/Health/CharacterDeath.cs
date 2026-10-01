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

            _enabledBeforeDeath = new bool[_abilities.Length];
            for (int i = 0; i < _abilities.Length; i++)
            {
                if (_abilities[i] == null) continue;
                _enabledBeforeDeath[i] = _abilities[i].AbilityEnabled;
                _abilities[i].AbilityEnabled = false;
            }

            if (_animator != null && !string.IsNullOrEmpty(deathAnimatorTrigger))
            {
                _animator.SetTrigger(deathAnimatorTrigger);
            }

            OnCharacterDeath?.Invoke();

            if (disableDelay >= 0f) Invoke(nameof(DisableGameObject), disableDelay);
        }

        private bool[] _enabledBeforeDeath;

        /// <summary>
        /// Deshace lo que hizo la muerte: reactiva las habilidades que estaban activas, vuelve a la condición
        /// Normal y reactiva el GameObject si se había desactivado. CharacterRespawn lo llama al respawnear.
        /// </summary>
        public void Revive()
        {
            CancelInvoke(nameof(DisableGameObject));
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (_enabledBeforeDeath != null)
            {
                for (int i = 0; i < _abilities.Length; i++)
                {
                    if (_abilities[i] != null) _abilities[i].AbilityEnabled = _enabledBeforeDeath[i];
                }
                _enabledBeforeDeath = null;
            }
            if (_character != null) _character.Condition.ChangeState(ConditionState.Normal);
        }

        private void DisableGameObject()
        {
            gameObject.SetActive(false);
        }
    }
}
