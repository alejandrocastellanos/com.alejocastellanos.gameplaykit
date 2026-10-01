using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Consultas de física 2D que ignoran al propio personaje. Los chequeos de suelo, pared, borde o
    /// línea de visión suelen empezar dentro del collider de quien pregunta; con Physics2D directo ese
    /// collider se detecta a sí mismo (o hay que configurar capas a mano para evitarlo). Con estas
    /// funciones las capas por defecto pueden ser "Everything" y todo funciona recién agregado.
    /// </summary>
    public static class PhysicsQuery2D
    {
        private static readonly List<RaycastHit2D> RayResults = new List<RaycastHit2D>(16);
        private static readonly List<Collider2D> OverlapResults = new List<Collider2D>(32);

        /// <summary>Raíz física de un componente: el Rigidbody2D que lo contiene, o su propio Transform.</summary>
        public static Transform OwnerOf(Component component)
        {
            if (component == null) return null;
            var rb = component.GetComponentInParent<Rigidbody2D>();
            return rb != null ? rb.transform : component.transform;
        }

        /// <summary>True si el collider pertenece a <paramref name="owner"/> (él mismo, un hijo o su Rigidbody2D).</summary>
        public static bool IsPartOf(Collider2D collider, Transform owner)
        {
            if (collider == null || owner == null) return false;
            if (collider.transform == owner || collider.transform.IsChildOf(owner)) return true;
            return collider.attachedRigidbody != null && collider.attachedRigidbody.transform == owner;
        }

        /// <summary>Raycast que devuelve el impacto más cercano que no pertenezca a <paramref name="ignore"/> ni a <paramref name="ignoreAlso"/>.</summary>
        public static RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, LayerMask mask,
            Transform ignore, Transform ignoreAlso = null, bool includeTriggers = false)
        {
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(mask);
            Physics2D.Raycast(origin, direction, filter, RayResults, distance);

            RaycastHit2D closest = default;
            float closestDistance = float.MaxValue;
            foreach (var hit in RayResults)
            {
                if (!includeTriggers && hit.collider.isTrigger) continue;
                if (IsPartOf(hit.collider, ignore) || IsPartOf(hit.collider, ignoreAlso)) continue;
                if (hit.distance < closestDistance)
                {
                    closest = hit;
                    closestDistance = hit.distance;
                }
            }
            return closest;
        }

        /// <summary>True si hay algún collider (sólido, por defecto) en el círculo que no pertenezca a <paramref name="ignore"/>.</summary>
        public static bool OverlapCircle(Vector2 point, float radius, LayerMask mask, Transform ignore, bool includeTriggers = false)
        {
            var filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(mask);
            Physics2D.OverlapCircle(point, radius, filter, OverlapResults);

            foreach (var collider in OverlapResults)
            {
                if (!includeTriggers && collider.isTrigger) continue;
                if (!IsPartOf(collider, ignore)) return true;
            }
            return false;
        }

        /// <summary>Dirección a la que mira un personaje que se voltea con la escala X (como hace PlayerWalkRun): 1 o -1.</summary>
        public static float Facing(Transform transform) => transform.lossyScale.x < 0f ? -1f : 1f;
    }
}
