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
        [SerializeField] private Transform edgeCheck;
        [SerializeField] private float rayLength = 1f;
        [SerializeField] private LayerMask groundLayers;

        public bool IsAtEdge { get; private set; }

        public override void ProcessAbility()
        {
            if (!Character.Controller.IsGrounded || edgeCheck == null) { IsAtEdge = false; return; }

            IsAtEdge = !Physics2D.Raycast(edgeCheck.position, Vector2.down, rayLength, groundLayers);
        }
    }
}
