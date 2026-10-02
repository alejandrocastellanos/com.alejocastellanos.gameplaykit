using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Movimiento cenital (top-down) en 8 direcciones o analógico, sin gravedad. Usa la misma entrada que el resto
    /// del kit (WASD/flechas o stick) y Shift para correr. Reemplaza a PlayerWalkRun/PlayerJump en juegos vistos desde arriba.
    /// </summary>
    public class PlayerTopDownMovement : AbilityBase
    {
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 6.5f;
        [Tooltip("Tiempo para alcanzar la velocidad deseada (0 = instantáneo).")]
        [SerializeField] private float acceleration = 0.05f;
        [SerializeField] private bool flipWithDirection = true;

        public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

        private Vector2 _velocityRef;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            Character.Controller.OverrideGravity(this, 0f);
        }

        public override void ProcessAbility()
        {
            var condition = Character.Condition.CurrentState;
            Vector2 input = (condition == ConditionState.Stunned || condition == ConditionState.Dead)
                ? Vector2.zero
                : Vector2.ClampMagnitude(CharacterInput.MoveInput, 1f);

            Vector2 desired = input * (CharacterInput.RunHeld ? runSpeed : walkSpeed);
            Vector2 velocity = acceleration > 0f
                ? Vector2.SmoothDamp(Character.Controller.Velocity, desired, ref _velocityRef, acceleration)
                : desired;
            Character.Controller.Move(velocity);

            if (input.sqrMagnitude > 0.01f) LastMoveDirection = input.normalized;
            Character.Movement.ChangeState(input.sqrMagnitude < 0.01f ? MovementState.Idle
                : CharacterInput.RunHeld ? MovementState.Running : MovementState.Walking);

            if (flipWithDirection && Mathf.Abs(input.x) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(input.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        public override void ResetAbility() => Character.Controller.OverrideGravity(this, 0f);
    }
}
