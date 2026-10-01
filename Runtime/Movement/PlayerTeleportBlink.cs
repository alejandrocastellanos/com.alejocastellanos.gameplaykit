using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Teletransporte corto en la dirección que mira el personaje, con chequeo de obstrucción y cooldown.
    /// Si hay una pared en el camino se detiene justo antes de ella.</summary>
    public class PlayerTeleportBlink : AbilityBase
    {
        [SerializeField] private float blinkDistance = 4f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private LayerMask obstacleLayers = ~0;
        [SerializeField] private CharacterAction blinkAction = CharacterAction.Blink;

        private float _cooldownTimer;

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f || !CharacterInput.GetActionDown(blinkAction)) return;

            float direction = PhysicsQuery2D.Facing(transform);
            var body = GetComponent<Collider2D>();
            Vector2 origin = body != null ? (Vector2)body.bounds.center : (Vector2)transform.position;
            Vector2 offsetToPivot = (Vector2)transform.position - origin;
            float halfWidth = body != null ? body.bounds.extents.x : 0f;

            RaycastHit2D hit = PhysicsQuery2D.Raycast(origin, Vector2.right * direction, blinkDistance + halfWidth, obstacleLayers, transform);
            float travel = hit.collider != null ? Mathf.Max(0f, hit.distance - halfWidth - 0.05f) : blinkDistance;

            Vector2 destination = origin + Vector2.right * direction * travel + offsetToPivot;
            Character.Controller.Rigidbody.position = destination;
            transform.position = destination;
            _cooldownTimer = cooldown;
        }
    }
}
