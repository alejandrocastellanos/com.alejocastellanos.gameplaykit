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

        // Solo cuenta si el input empuja hacia la pared (no hacia afuera).
        private bool PushingTowardWall() =>
            Mathf.Abs(CharacterInput.MoveInput.x) > 0.1f && Mathf.Sign(CharacterInput.MoveInput.x) == PhysicsQuery2D.Facing(transform);

        public override void ProcessAbility()
        {
            bool clinging = _wallCheck.IsTouchingWall &&
                            !Character.Controller.IsGrounded &&
                            PushingTowardWall();

            if (!clinging) return;
            // El frame del salto y el bloqueo de control posterior pertenecen al wall jump: aferrarse aquí
            // anularía su impulso vertical.
            if (CharacterInput.JumpPressedThisFrame || Character.Controller.IsHorizontalControlLocked) return;

            Character.Controller.SetVerticalVelocity(0f);
            Character.Movement.ChangeState(MovementState.WallSliding);
        }
    }
}
