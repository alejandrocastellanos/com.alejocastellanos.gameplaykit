using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Teletransporte corto en la dirección que mira el personaje, con chequeo de obstrucción y cooldown.</summary>
    public class PlayerTeleportBlink : AbilityBase
    {
        [SerializeField] private float blinkDistance = 4f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private KeyCode blinkKey = KeyCode.LeftAlt;

        private float _cooldownTimer;

        public override void ProcessAbility()
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f || !Input.GetKeyDown(blinkKey)) return;

            float direction = Mathf.Sign(transform.localScale.x);
            Vector2 origin = transform.position;
            Vector2 desired = origin + Vector2.right * direction * blinkDistance;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, blinkDistance, obstacleLayers);
            Vector2 destination = hit.collider != null ? hit.point - Vector2.right * direction * 0.1f : desired;

            transform.position = destination;
            _cooldownTimer = cooldown;
        }
    }
}
