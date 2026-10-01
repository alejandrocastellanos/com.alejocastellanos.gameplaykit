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
        [Tooltip("Collider que se encoge al agacharse. Si se deja vacío se usa el Collider2D del personaje (Box o Capsule).")]
        [SerializeField] private Collider2D bodyCollider;
        [Tooltip("Altura agachado como fracción de la altura de pie (0.5 = la mitad).")]
        [Range(0.2f, 1f)]
        [SerializeField] private float crouchHeightRatio = 0.5f;
        [SerializeField] private float speedMultiplier = 0.5f;

        public bool IsCrouching { get; private set; }

        // Tamaño y offset de pie se toman del collider real al iniciar, para no deformarlo al levantarse.
        private Vector2 _standingSize;
        private Vector2 _standingOffset;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();
            if (bodyCollider is CapsuleCollider2D capsule) { _standingSize = capsule.size; _standingOffset = capsule.offset; }
            else if (bodyCollider is BoxCollider2D box) { _standingSize = box.size; _standingOffset = box.offset; }
        }

        public override void ProcessAbility()
        {
            IsCrouching = CharacterInput.CrouchHeld && Character.Controller.IsGrounded;
            ApplyColliderShape();

            if (IsCrouching)
            {
                Character.Movement.ChangeState(MovementState.Crouching);
                Vector2 v = Character.Controller.Velocity;
                v.x *= speedMultiplier;
                Character.Controller.Move(v);
            }
        }

        private void ApplyColliderShape()
        {
            Vector2 size = _standingSize;
            Vector2 offset = _standingOffset;
            if (IsCrouching)
            {
                // Se encoge desde arriba: los pies se quedan donde estaban.
                size.y = _standingSize.y * crouchHeightRatio;
                offset.y = _standingOffset.y - (_standingSize.y - size.y) * 0.5f;
            }

            if (bodyCollider is CapsuleCollider2D capsule) { capsule.size = size; capsule.offset = offset; }
            else if (bodyCollider is BoxCollider2D box) { box.size = size; box.offset = offset; }
        }

        public override void ResetAbility()
        {
            IsCrouching = false;
            ApplyColliderShape();
        }
    }
}
