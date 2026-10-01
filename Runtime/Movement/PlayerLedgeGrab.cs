using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Detecta un borde de plataforma (pared bloqueada a la altura del pecho, libre a la altura
    /// de la cabeza) y se cuelga: congela al personaje en el sitio hasta que se suelte (abajo) o trepe.
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
        /// <summary>Esquina superior del borde agarrado (donde quedarían los pies al trepar).</summary>
        public Vector2 LedgePoint { get; private set; }
        /// <summary>Dirección hacia el borde agarrado: 1 derecha, -1 izquierda.</summary>
        public float LedgeDirection { get; private set; } = 1f;

        private Collider2D _body;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _body = GetComponent<Collider2D>();
        }

        public override void ProcessAbility()
        {
            if (!IsGrabbingLedge)
            {
                if (Character.Controller.IsGrounded || Character.Controller.Velocity.y >= 0f) return;
                if (TryFindLedge(out Vector2 ledgePoint, out float direction))
                {
                    IsGrabbingLedge = true;
                    LedgePoint = ledgePoint;
                    LedgeDirection = direction;
                }
            }

            if (!IsGrabbingLedge) return;

            Character.Controller.Move(Vector2.zero);
            Character.Controller.OverrideGravity(this, 0f);

            if (CharacterInput.MoveInput.y < -0.5f) Release();
        }

        /// <summary>Suelta el borde y devuelve la gravedad normal.</summary>
        public void Release()
        {
            if (!IsGrabbingLedge) return;
            IsGrabbingLedge = false;
            Character.Controller.ReleaseGravity(this);
        }

        public override void ResetAbility() => Release();

        private bool TryFindLedge(out Vector2 ledgePoint, out float direction)
        {
            ledgePoint = default;
            direction = PhysicsQuery2D.Facing(transform);
            Vector2 facing = Vector2.right * direction;

            Vector2 chest = chestCheck != null ? (Vector2)chestCheck.position : (Vector2)transform.position + chestOffset;
            Vector2 head = headCheck != null ? (Vector2)headCheck.position : (Vector2)transform.position + headOffset;
            float reach = checkDistance + (_body != null ? _body.bounds.extents.x : 0f);

            RaycastHit2D chestHit = PhysicsQuery2D.Raycast(chest, facing, reach, ledgeLayers, transform);
            if (chestHit.collider == null) return false;
            if (PhysicsQuery2D.Raycast(head, facing, reach, ledgeLayers, transform).collider != null) return false;

            // Para saber dónde está la cara superior se lanza un rayo hacia abajo justo dentro de la pared.
            Vector2 probe = new Vector2(chestHit.point.x + direction * 0.05f, head.y);
            RaycastHit2D top = PhysicsQuery2D.Raycast(probe, Vector2.down, head.y - chest.y + 0.05f, ledgeLayers, transform);
            if (top.collider == null) return false;

            ledgePoint = new Vector2(chestHit.point.x, top.point.y);
            return true;
        }
    }
}
