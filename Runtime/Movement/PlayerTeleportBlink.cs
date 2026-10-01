using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Teletransporte corto en la dirección que mira el personaje, con chequeo de obstrucción y cooldown.</summary>
    public class PlayerTeleportBlink : AbilityBase
    {
        [SerializeField] private float blinkDistance = 4f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private LayerMask obstacleLayers = ~0;
        [SerializeField] private KeyCode blinkKey = KeyCode.LeftAlt;

        private float _cooldownTimer;

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f || !InputCompat.GetKeyDown(blinkKey)) return;

            float direction = Mathf.Sign(transform.localScale.x);
            Vector2 origin = transform.position;
            Vector2 desired = origin + Vector2.right * direction * blinkDistance;

            RaycastHit2D hit = PhysicsQuery2D.Raycast(origin, Vector2.right * direction, blinkDistance, obstacleLayers, transform);
            var body = GetComponent<Collider2D>();
            float margin = 0.1f + (body != null ? body.bounds.extents.x : 0f);
            Vector2 destination = hit.collider != null ? hit.point - Vector2.right * direction * margin : desired;

            transform.position = destination;
            _cooldownTimer = cooldown;
        }
    }
}
