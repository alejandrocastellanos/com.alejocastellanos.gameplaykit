using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Orienta al personaje según el ángulo de la pendiente que pisa (rotación visual, no física).</summary>
    public class PlayerSlopeWalk : AbilityBase
    {
        [SerializeField] private float rayLength = 0.6f;
        [SerializeField] private LayerMask groundLayers = ~0;
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private float rotationSpeed = 10f;

        public override void ProcessAbility()
        {
            var body = GetComponent<Collider2D>();
            float distance = rayLength + (body != null ? body.bounds.extents.y : 0f);
            Vector2 origin = body != null ? (Vector2)body.bounds.center : (Vector2)transform.position;
            RaycastHit2D hit = PhysicsQuery2D.Raycast(origin, Vector2.down, distance, groundLayers, transform);
            if (!hit.collider) return;

            float angle = Vector2.SignedAngle(Vector2.up, hit.normal);
            if (Mathf.Abs(angle) > maxSlopeAngle) return;

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
