using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Frena la caída al estar pegado a una pared. Requiere PlayerWallJump para detectar la pared.</summary>
    [RequireComponent(typeof(PlayerWallJump))]
    public class PlayerWallSlide : AbilityBase
    {
        [SerializeField] private float slideSpeed = 2f;

        private PlayerWallJump _wallCheck;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _wallCheck = GetComponent<PlayerWallJump>();
        }

        public override void ProcessAbility()
        {
            bool sliding = _wallCheck.IsTouchingWall &&
                           !Character.Controller.IsGrounded &&
                           Character.Controller.Velocity.y < 0f &&
                           !Mathf.Approximately(CharacterInput.MoveInput.x, 0f);

            if (!sliding) return;

            Character.Controller.SetVerticalVelocity(Mathf.Max(Character.Controller.Velocity.y, -slideSpeed));
            Character.Movement.ChangeState(MovementState.WallSliding);
        }
    }
}
