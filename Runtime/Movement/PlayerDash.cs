using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Impulso rápido horizontal en la dirección que mira el personaje, con cooldown.</summary>
    public class PlayerDash : AbilityBase
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
                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _cooldownTimer = cooldown;
            Character.Movement.ChangeState(MovementState.Dashing);

            float direction = Mathf.Sign(transform.localScale.x);
            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                Character.Controller.Move(new Vector2(direction * dashSpeed, 0f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
        }
    }
}
