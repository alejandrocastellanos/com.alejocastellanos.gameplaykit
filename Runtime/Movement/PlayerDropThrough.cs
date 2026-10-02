using System.Collections;
using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Abajo + saltar sobre una plataforma de una vía (OneWayPlatform o cualquier PlatformEffector2D) la atraviesa
    /// hacia abajo. Ese salto no dispara PlayerJump.</summary>
    public class PlayerDropThrough : AbilityBase
    {
        [SerializeField] private float ignoreDuration = 0.35f;
        [SerializeField] private float probeDistance = 0.2f;

        public bool IsDropping { get; private set; }

        private Collider2D _body;

        public override void Initialize(CharacterCore character)
        {
            base.Initialize(character);
            _body = GetComponent<Collider2D>();
        }

        public override void EarlyProcessAbility()
        {
            if (IsDropping || _body == null || !Character.Controller.IsGrounded) return;
            if (CharacterInput.MoveInput.y > -0.5f || !CharacterInput.JumpPressedThisFrame) return;

            Collider2D platform = FindOneWayPlatformBelow();
            if (platform == null) return;

            Character.Controller.BlockJumpThisFrame();
            StartCoroutine(DropRoutine(platform));
        }

        private Collider2D FindOneWayPlatformBelow()
        {
            Bounds b = _body.bounds;
            var hits = Physics2D.OverlapBoxAll(new Vector2(b.center.x, b.min.y - probeDistance * 0.5f), new Vector2(b.size.x * 0.9f, probeDistance), 0f);
            foreach (var hit in hits)
            {
                if (PhysicsQuery2D.IsPartOf(hit, transform) || hit.isTrigger) continue;
                if (hit.usedByEffector && hit.GetComponent<PlatformEffector2D>() != null) return hit;
            }
            return null;
        }

        private IEnumerator DropRoutine(Collider2D platform)
        {
            IsDropping = true;
            Physics2D.IgnoreCollision(_body, platform, true);
            Character.Controller.SetVerticalVelocity(-2f);
            yield return new WaitForSeconds(ignoreDuration);
            if (platform != null && _body != null) Physics2D.IgnoreCollision(_body, platform, false);
            IsDropping = false;
        }
    }
}
