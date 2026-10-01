using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Caja que el jugador puede empujar/jalar al chocar contra ella lateralmente.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableBox : MonoBehaviour
    {
        [Header("Caja empujable")]
        [SerializeField] private float pushSpeed = 1.5f;
        [SerializeField] private float mass = 5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.mass = mass;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!TagFilter.Matches(collision.transform, "Player")) return;

            var playerRb = collision.rigidbody;
            if (playerRb == null || Mathf.Abs(playerRb.linearVelocity.x) < 0.1f) return;

            float direction = Mathf.Sign(playerRb.linearVelocity.x);
            _rb.linearVelocity = new Vector2(direction * pushSpeed, _rb.linearVelocity.y);
        }
    }
}
