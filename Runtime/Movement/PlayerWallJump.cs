using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Detecta pared adelante mientras está en el aire y permite saltar alejándose de ella.</summary>
    public class PlayerWallJump : AbilityBase
    {
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float wallCheckDistance = 0.3f;
        [SerializeField] private LayerMask wallLayers;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 12f);

        public bool IsTouchingWall { get; private set; }
        private int _wallDirection;

        public override void EarlyProcessAbility()
        {
            if (wallCheck == null) return;
            RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, wallLayers);
            IsTouchingWall = hit.collider != null;
            _wallDirection = (int)Mathf.Sign(transform.localScale.x);
        }

        public override void ProcessAbility()
        {
            if (!IsTouchingWall || Character.Controller.IsGrounded) return;
            if (!CharacterInput.JumpPressedThisFrame) return;

            Character.Controller.Move(new Vector2(-_wallDirection * wallJumpForce.x, wallJumpForce.y));
            Character.Movement.ChangeState(MovementState.Jumping);
        }
    }
}
