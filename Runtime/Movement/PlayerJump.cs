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
        [Tooltip("Si se presiona saltar un poco antes de tocar el suelo, el salto sale al aterrizar.")]
        [SerializeField] private float jumpBufferTime = 0.12f;

        private float _lastGroundedTime;
        private float _lastJumpPressedTime = -999f;

        public override void EarlyProcessAbility()
        {
            if (Character.Controller.IsGrounded) _lastGroundedTime = Time.time;
        }

        public override void ProcessAbility()
        {
            if (CharacterInput.JumpPressedThisFrame) _lastJumpPressedTime = Time.time;
            if (Character.Controller.IsJumpBlocked) _lastJumpPressedTime = -999f; // otra habilidad usó el botón

            bool canJump = Time.time - _lastGroundedTime <= coyoteTime;
            bool wantsJump = Time.time - _lastJumpPressedTime <= jumpBufferTime;

            if (wantsJump && canJump)
            {
                Character.Controller.SetVerticalVelocity(jumpForce);
                Character.Movement.ChangeState(MovementState.Jumping);
                _lastGroundedTime = -999f; // evita doble salto por coyote time
                _lastJumpPressedTime = -999f;
                // Salto con buffer y botón ya soltado: se aplica el recorte de inmediato (salto corto).
                if (!CharacterInput.JumpHeld) Character.Controller.SetVerticalVelocity(jumpForce * jumpCutMultiplier);
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
