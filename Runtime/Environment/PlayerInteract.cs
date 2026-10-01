using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Habilidad de interacción: al presionar interactuar, acciona el IInteractable más cercano dentro del radio
    /// (palancas y cualquier script propio que implemente IInteractable).</summary>
    public class PlayerInteract : AbilityBase
    {
        [SerializeField] private float interactRadius = 1.2f;

        private readonly Collider2D[] _hits = new Collider2D[16];

        public IInteractable FindClosest()
        {
            var filter = ContactFilter2D.noFilter; // incluye triggers: las palancas suelen serlo
            int count = Physics2D.OverlapCircle(transform.position, interactRadius, filter, _hits);
            IInteractable closest = null;
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (PhysicsQuery2D.IsPartOf(_hits[i], transform)) continue;
                var interactable = _hits[i].GetComponentInParent<IInteractable>();
                if (interactable == null) continue;
                float distance = Vector2.Distance(transform.position, _hits[i].bounds.ClosestPoint(transform.position));
                if (distance < best) { best = distance; closest = interactable; }
            }
            return closest;
        }

        public override void ProcessAbility()
        {
            if (!CharacterInput.InteractPressedThisFrame) return;
            FindClosest()?.Interact(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
