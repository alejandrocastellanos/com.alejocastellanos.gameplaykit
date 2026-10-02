using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Conecta el personaje con un Animator sin código: cada frame escribe velocidad, suelo, estado de movimiento y
    /// muerte en los parámetros que existan en el Animator Controller (los que no existan se ignoran sin errores),
    /// y dispara un trigger al recibir daño. Nombres por defecto: Speed, VelocityY, Grounded, MovementState, Dead, Hit.
    /// </summary>
    public class CharacterAnimatorBridge : MonoBehaviour
    {
        [Tooltip("Vacío = el primer Animator del objeto o sus hijos.")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameter = "Speed";
        [SerializeField] private string verticalVelocityParameter = "VelocityY";
        [SerializeField] private string groundedParameter = "Grounded";
        [SerializeField] private string movementStateParameter = "MovementState";
        [SerializeField] private string deadParameter = "Dead";
        [SerializeField] private string hitTrigger = "Hit";

        private CharacterCore _core;
        private CharacterController2D _controller;
        private IHealthSource _health;
        private readonly HashSet<string> _available = new HashSet<string>();
        private RuntimeAnimatorController _cachedController;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            _core = GetComponent<CharacterCore>();
            _controller = GetComponent<CharacterController2D>();
        }

        private void OnEnable()
        {
            _health = GetComponentInParent<IHealthSource>();
            if (_health != null) _health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Damaged -= HandleDamaged;
        }

        private void LateUpdate()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            RefreshParameters();

            if (_controller != null && _controller.Rigidbody != null)
            {
                Set(speedParameter, Mathf.Abs(_controller.Velocity.x));
                Set(verticalVelocityParameter, _controller.Velocity.y);
                Set(groundedParameter, _controller.IsGrounded);
            }
            if (_core != null && _core.Movement != null)
            {
                Set(movementStateParameter, (int)_core.Movement.CurrentState);
                Set(deadParameter, _core.Condition.CurrentState == ConditionState.Dead);
            }
        }

        private void HandleDamaged(float amount)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            RefreshParameters();
            if (_available.Contains(hitTrigger)) animator.SetTrigger(hitTrigger);
        }

        // Se leen los parámetros que realmente tiene el controller (y se vuelve a leer si se cambia el controller).
        private void RefreshParameters()
        {
            if (_cachedController == animator.runtimeAnimatorController) return;
            _cachedController = animator.runtimeAnimatorController;
            _available.Clear();
            foreach (var p in animator.parameters) _available.Add(p.name);
        }

        private void Set(string parameter, float value) { if (_available.Contains(parameter)) animator.SetFloat(parameter, value); }
        private void Set(string parameter, int value) { if (_available.Contains(parameter)) animator.SetInteger(parameter, value); }
        private void Set(string parameter, bool value) { if (_available.Contains(parameter)) animator.SetBool(parameter, value); }
    }
}
