using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>
    /// Detecta que el suelo está por terminar (el siguiente paso sería una caída) y expone
    /// IsAtEdge para que el Animator reaccione (ej. bracear los brazos). No cambia el movimiento.
    /// </summary>
    public class PlayerEdgeDangle : AbilityBase
    {
        [Tooltip("Punto frente a los pies desde el que se busca suelo. Si se deja vacío se usa el borde delantero del Collider2D.")]
        [SerializeField] private Transform edgeCheck;
        [SerializeField] private float rayLength = 1f;
        [SerializeField] private LayerMask groundLayers = ~0;

        public bool IsAtEdge { get; private set; }

        public override void ProcessAbility()
        {
            if (!Character.Controller.IsGrounded) { IsAtEdge = false; return; }

            Vector2 origin;
            if (edgeCheck != null) origin = edgeCheck.position;
            else
            {
                var body = GetComponent<Collider2D>();
                if (body == null) { IsAtEdge = false; return; }
                Bounds b = body.bounds;
                origin = new Vector2(b.center.x + PhysicsQuery2D.Facing(transform) * b.extents.x, b.min.y + 0.05f);
            }

            IsAtEdge = PhysicsQuery2D.Raycast(origin, Vector2.down, rayLength, groundLayers, transform).collider == null;
        }
    }
}
