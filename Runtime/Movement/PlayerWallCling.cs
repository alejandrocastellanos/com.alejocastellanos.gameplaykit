using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Variante de WallSlide que se aferra completamente (velocidad vertical cero) mientras
    /// se mantiene presionado hacia la pared. Usa esta o PlayerWallSlide, no ambas.
    /// </summary>
    [RequireComponent(typeof(PlayerWallJump))]
    public class PlayerWallCling : AbilityBase
    {
        private PlayerWallJump _wallCheck;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _wallCheck = GetComponent<PlayerWallJump>();
        }

        public override void ProcessAbility()
        {
            bool clinging = _wallCheck.IsTouchingWall &&
                            !Character.Controller.IsGrounded &&
                            !Mathf.Approximately(CharacterInput.MoveInput.x, 0f);

            if (!clinging) return;

            Character.Controller.SetVerticalVelocity(0f);
            Character.Movement.ChangeState(MovementState.WallSliding);
        }
    }
}
