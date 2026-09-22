using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Agacharse: reduce el collider y aplica un multiplicador de velocidad. Colócalo
    /// DESPUÉS de PlayerWalkRun en la lista de componentes para que el multiplicador
    /// se aplique sobre la velocidad ya calculada.
    /// </summary>
    public class PlayerCrouch : AbilityBase
    {
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private Vector2 standingSize = new Vector2(1f, 2f);
        [SerializeField] private Vector2 crouchingSize = new Vector2(1f, 1f);
        [SerializeField] private float speedMultiplier = 0.5f;

        public bool IsCrouching { get; private set; }

        public override void ProcessAbility()
        {
            IsCrouching = CharacterInput.CrouchHeld && Character.Controller.IsGrounded;

            if (bodyCollider is CapsuleCollider2D capsule)
            {
                capsule.size = IsCrouching ? crouchingSize : standingSize;
            }
            else if (bodyCollider is BoxCollider2D box)
            {
                box.size = IsCrouching ? crouchingSize : standingSize;
            }

            if (IsCrouching)
            {
                Character.Movement.ChangeState(MovementState.Crouching);
                Vector2 v = Character.Controller.Velocity;
                v.x *= speedMultiplier;
                Character.Controller.Move(v);
            }
        }
    }
}
