using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Salto estándar de altura variable: soltar el botón antes de tiempo corta el salto.</summary>
    public class PlayerJump : AbilityBase
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float coyoteTime = 0.1f;

        private float _lastGroundedTime;

        public override void EarlyProcessAbility()
        {
            if (Character.Controller.IsGrounded) _lastGroundedTime = Time.time;
        }

        public override void ProcessAbility()
        {
            bool canJump = Time.time - _lastGroundedTime <= coyoteTime;

            if (CharacterInput.JumpPressedThisFrame && canJump)
            {
                Character.Controller.SetVerticalVelocity(jumpForce);
                Character.Movement.ChangeState(MovementState.Jumping);
                _lastGroundedTime = -999f; // evita doble salto por coyote time
            }
            else if (CharacterInput.JumpReleasedThisFrame && Character.Controller.Velocity.y > 0f)
            {
                Character.Controller.SetVerticalVelocity(Character.Controller.Velocity.y * jumpCutMultiplier);
            }

            if (!Character.Controller.IsGrounded && Character.Controller.Velocity.y < -0.01f)
            {
                Character.Movement.ChangeState(MovementState.Falling);
            }
        }
    }
}
