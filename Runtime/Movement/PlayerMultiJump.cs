using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Salto con N saltos adicionales en el aire (doble, triple, o los que configures).
    /// Alternativa autocontenida a PlayerJump — usa una de las dos, no ambas en el mismo personaje.
    /// </summary>
    public class PlayerMultiJump : AbilityBase
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float airJumpForce = 10f;
        [SerializeField] private int extraJumps = 1;
        [SerializeField] private float jumpCutMultiplier = 0.5f;
        [SerializeField] private float jumpBufferTime = 0.12f;

        private int _jumpsRemaining;
        private float _lastJumpPressedTime = -999f;

        public override void EarlyProcessAbility()
        {
            if (Character.Controller.IsGrounded) _jumpsRemaining = extraJumps;
        }

        public override void ProcessAbility()
        {
            if (CharacterInput.JumpPressedThisFrame) _lastJumpPressedTime = Time.time;
            if (Character.Controller.IsJumpBlocked) { _lastJumpPressedTime = -999f; return; }
            bool buffered = Time.time - _lastJumpPressedTime <= jumpBufferTime;

            if (Character.Controller.IsGrounded && buffered)
            {
                _lastJumpPressedTime = -999f;
                Character.Controller.SetVerticalVelocity(CharacterInput.JumpHeld ? jumpForce : jumpForce * jumpCutMultiplier);
                Character.Movement.ChangeState(MovementState.Jumping);
            }
            else if (CharacterInput.JumpPressedThisFrame)
            {
                if (_jumpsRemaining > 0)
                {
                    _jumpsRemaining--;
                    _lastJumpPressedTime = -999f; // ya se usó: no debe repetirse al aterrizar
                    Character.Controller.SetVerticalVelocity(airJumpForce);
                    Character.Movement.ChangeState(MovementState.Jumping);
                }
            }
            else if (CharacterInput.JumpReleasedThisFrame && Character.Controller.Velocity.y > 0f)
            {
                Character.Controller.SetVerticalVelocity(Character.Controller.Velocity.y * jumpCutMultiplier);
            }
        }
    }
}
