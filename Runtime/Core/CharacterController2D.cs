using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Wrapper sobre Rigidbody2D + detección de suelo. Las habilidades de movimiento hablan
    /// con este componente en vez de tocar Rigidbody2D directamente, para no repetir la
    /// lógica de suelo/velocidad en cada una.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask groundLayers;

        public Rigidbody2D Rigidbody { get; private set; }
        public bool IsGrounded { get; private set; }
        public Vector2 Velocity => Rigidbody.linearVelocity;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            IsGrounded = groundCheck != null &&
                Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        }

        public void Move(Vector2 velocity)
        {
            Rigidbody.linearVelocity = velocity;
        }

        public void SetVerticalVelocity(float velocityY)
        {
            Vector2 v = Rigidbody.linearVelocity;
            v.y = velocityY;
            Rigidbody.linearVelocity = v;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
