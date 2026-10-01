using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Detecta un borde de plataforma (pared bloqueada a la altura del pecho, libre a la altura
    /// de la cabeza) y se cuelga: congela al personaje en el sitio hasta que se suelte o trepe.
    /// Combínalo con PlayerLedgeClimb para trepar desde el agarre.
    /// </summary>
    public class PlayerLedgeGrab : AbilityBase
    {
        [Tooltip("Origen del rayo a la altura del pecho. Si se deja vacío se usa la posición del personaje + Chest Offset.")]
        [SerializeField] private Transform chestCheck;
        [Tooltip("Origen del rayo a la altura de la cabeza. Si se deja vacío se usa la posición del personaje + Head Offset.")]
        [SerializeField] private Transform headCheck;
        [SerializeField] private Vector2 chestOffset = new Vector2(0f, 0.2f);
        [SerializeField] private Vector2 headOffset = new Vector2(0f, 0.8f);
        [SerializeField] private float checkDistance = 0.3f;
        [SerializeField] private LayerMask ledgeLayers = ~0;

        public bool IsGrabbingLedge { get; private set; }

        public override void ProcessAbility()
        {
            if (Character.Controller.IsGrounded) { IsGrabbingLedge = false; return; }

            if (!IsGrabbingLedge)
            {
                Vector2 chest = chestCheck != null ? (Vector2)chestCheck.position : (Vector2)transform.position + chestOffset;
                Vector2 head = headCheck != null ? (Vector2)headCheck.position : (Vector2)transform.position + headOffset;
                Vector2 facing = Vector2.right * PhysicsQuery2D.Facing(transform);
                var body = GetComponent<Collider2D>();
                float reach = checkDistance + (body != null ? body.bounds.extents.x : 0f);
                bool chestBlocked = PhysicsQuery2D.Raycast(chest, facing, reach, ledgeLayers, transform).collider != null;
                bool headFree = PhysicsQuery2D.Raycast(head, facing, reach, ledgeLayers, transform).collider == null;

                if (chestBlocked && headFree && Character.Controller.Velocity.y < 0f)
                {
                    IsGrabbingLedge = true;
                }
            }

            if (IsGrabbingLedge)
            {
                Character.Controller.Move(Vector2.zero);
                Character.Controller.Rigidbody.gravityScale = 0f;

                if (CharacterInput.MoveInput.y < -0.5f)
                {
                    IsGrabbingLedge = false;
                    Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
                }
            }
            else
            {
                Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
            }
        }
    }
}
