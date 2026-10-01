using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Transporta a quien lo toque desde este punto hasta un destino, opcionalmente conservando la velocidad.</summary>
    public class Teleporter : MonoBehaviour
    {
        [Header("Teletransporte")]
        [SerializeField] private Transform destination;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private float cooldown = 0.2f;
        [SerializeField] private bool preserveVelocity = true;

        // Enfriamiento por objeto transportado y compartido entre teletransportadores: sin esto, al aparecer
        // dentro de otro teletransportador (un par de portales) se le devolvía de inmediato.
        private static readonly System.Collections.Generic.Dictionary<Transform, float> LastTeleportTime =
            new System.Collections.Generic.Dictionary<Transform, float>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (destination == null) return;
            if (!TagFilter.PassesOptional(other, targetTag)) return;

            var rb = other.attachedRigidbody;
            Transform traveler = rb != null ? rb.transform : other.transform;
            if (LastTeleportTime.TryGetValue(traveler, out float last) && Time.time - last < cooldown) return;

            Vector2 previousVelocity = rb != null ? rb.linearVelocity : Vector2.zero;
            if (rb != null) rb.position = destination.position;
            traveler.position = destination.position;
            if (rb != null) rb.linearVelocity = preserveVelocity ? previousVelocity : Vector2.zero;
            LastTeleportTime[traveler] = Time.time;
        }
    }
}
