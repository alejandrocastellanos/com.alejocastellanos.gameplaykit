using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Impulsa hacia arriba (o en una dirección configurable) a quien la toque.</summary>
    public class SpringPad : MonoBehaviour
    {
        [Header("Resorte")]
        [SerializeField] private Vector2 launchDirection = Vector2.up;
        [SerializeField] private float launchForce = 15f;
        [SerializeField] private bool useTrigger = true;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!useTrigger) Launch(collision.rigidbody);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (useTrigger) Launch(other.attachedRigidbody);
        }

        private void Launch(Rigidbody2D rb)
        {
            if (rb == null) return;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(launchDirection.normalized * launchForce, ForceMode2D.Impulse);
        }
    }
}
