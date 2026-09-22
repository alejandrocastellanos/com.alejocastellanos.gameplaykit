using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Como PlayerDash, pero en la dirección del input (8 direcciones) en vez de solo horizontal.</summary>
    public class PlayerDash8Directions : AbilityBase
    {
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float cooldown = 0.5f;

        private bool _isDashing;
        private float _cooldownTimer;

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;

            if (!_isDashing && CharacterInput.DashPressedThisFrame && _cooldownTimer <= 0f)
            {
                Vector2 direction = CharacterInput.MoveInput.sqrMagnitude > 0.01f
                    ? CharacterInput.MoveInput.normalized
                    : new Vector2(Mathf.Sign(transform.localScale.x), 0f);
                StartCoroutine(DashRoutine(direction));
            }
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            _isDashing = true;
            _cooldownTimer = cooldown;
            Character.Movement.ChangeState(MovementState.Dashing);

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                Character.Controller.Move(direction * dashSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
        }
    }
}
