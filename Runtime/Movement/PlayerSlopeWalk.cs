using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Orienta al personaje según el ángulo de la pendiente que pisa (rotación visual, no física).</summary>
    public class PlayerSlopeWalk : AbilityBase
    {
        [SerializeField] private float rayLength = 0.6f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private float rotationSpeed = 10f;

        public override void ProcessAbility()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayers);
            if (!hit.collider) return;

            float angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (Mathf.Abs(angle) > maxSlopeAngle) return;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
