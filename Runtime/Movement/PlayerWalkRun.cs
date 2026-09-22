using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Movimiento horizontal básico con velocidad de caminar y de correr (Shift).</summary>
    public class PlayerWalkRun : AbilityBase
    {
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private bool flipWithDirection = true;

        public override void ProcessAbility()
        {
            if (Character.Condition.CurrentState == ConditionState.Stunned ||
                Character.Condition.CurrentState == ConditionState.Dead) return;

            float horizontal = CharacterInput.MoveInput.x;
            float speed = CharacterInput.RunHeld ? runSpeed : walkSpeed;

            Vector2 velocity = Character.Controller.Velocity;
            velocity.x = horizontal * speed;
            Character.Controller.Move(velocity);

            Character.Movement.ChangeState(
                Mathf.Approximately(horizontal, 0f) ? MovementState.Idle :
                CharacterInput.RunHeld ? MovementState.Running : MovementState.Walking);

            if (flipWithDirection && !Mathf.Approximately(horizontal, 0f))
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(horizontal) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }
}
