using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Sube/baja escaleras. Espera un trigger con un componente que marque la zona como
    /// escalable (usa un layer/tag "Ladder" en el collider de la escalera).
    /// </summary>
    public class PlayerClimbLadder : AbilityBase
    {
        [SerializeField] private float climbSpeed = 3f;
        [Tooltip("Opcional: además de cualquier LadderZone, cuenta como escalera todo trigger con este tag.")]
        [SerializeField] private string ladderTag = "Ladder";

        public bool IsOnLadder { get; private set; }
        private bool _touchingLadder;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsLadder(other)) _touchingLadder = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsLadder(other))
            {
                _touchingLadder = false;
                if (IsOnLadder) Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
                IsOnLadder = false;
            }
        }

        private bool IsLadder(Collider2D other) =>
            other.GetComponent<LadderZone>() != null || TagFilter.Matches(other, ladderTag);

        public override void ProcessAbility()
        {
            if (!_touchingLadder) { IsOnLadder = false; return; }

            if (Mathf.Abs(CharacterInput.MoveInput.y) > 0.1f) IsOnLadder = true;
            if (!IsOnLadder) return;

            Character.Controller.Rigidbody.gravityScale = 0f;
            Character.Controller.Move(new Vector2(Character.Controller.Velocity.x, CharacterInput.MoveInput.y * climbSpeed));

            if (Character.Controller.IsGrounded && CharacterInput.MoveInput.y < 0f)
            {
                IsOnLadder = false;
                Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
            }
        }

        public override void ResetAbility()
        {
            Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
        }
    }

    /// <summary>Marca un trigger como escalera para PlayerClimbLadder (no hace falta crear tags).</summary>
    public class LadderZone : MonoBehaviour { }
}
