using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Detecta pared adelante mientras está en el aire y permite saltar alejándose de ella.</summary>
    public class PlayerWallJump : AbilityBase
    {
        [Tooltip("Origen del chequeo de pared. Si se deja vacío se usa el centro del personaje.")]
        [SerializeField] private Transform wallCheck;
        [SerializeField] private float wallCheckDistance = 0.3f;
        [SerializeField] private LayerMask wallLayers = ~0;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(8f, 12f);
        [Tooltip("Segundos durante los que el input horizontal no anula el impulso del wall jump.")]
        [SerializeField] private float controlLockTime = 0.2f;

        public bool IsTouchingWall { get; private set; }
        private int _wallDirection;

        private Collider2D _bodyCollider;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _bodyCollider = GetComponent<Collider2D>();
        }

        public override void EarlyProcessAbility()
        {
            // La dirección sale de la escala X (así voltea PlayerWalkRun); transform.right no cambia al voltear.
            _wallDirection = (int)PhysicsQuery2D.Facing(transform);

            Vector2 origin = wallCheck != null ? (Vector2)wallCheck.position : (Vector2)transform.position;
            float distance = wallCheckDistance;
            if (wallCheck == null && _bodyCollider != null)
            {
                origin = _bodyCollider.bounds.center;
                distance += _bodyCollider.bounds.extents.x;
            }

            RaycastHit2D hit = PhysicsQuery2D.Raycast(origin, Vector2.right * _wallDirection, distance, wallLayers, transform);
            IsTouchingWall = hit.collider != null;
        }

        public override void ProcessAbility()
        {
            if (!IsTouchingWall || Character.Controller.IsGrounded) return;
            if (!CharacterInput.JumpPressedThisFrame) return;

            Character.Controller.Move(new Vector2(-_wallDirection * wallJumpForce.x, wallJumpForce.y));
            Character.Controller.LockHorizontalControl(controlLockTime);
            Character.Movement.ChangeState(MovementState.Jumping);
        }
    }
}
