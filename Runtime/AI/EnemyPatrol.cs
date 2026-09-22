using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Patrulla en línea recta y cambia de dirección al chocar con un obstáculo o al llegar al borde de una plataforma.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Patrulla")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private Transform wallCheck;
        [SerializeField] private Transform edgeCheck;
        [SerializeField] private float wallCheckDistance = 0.2f;
        [SerializeField] private float edgeCheckDistance = 0.5f;
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private LayerMask groundLayers;

        private Rigidbody2D _rb;
        private int _direction = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (ShouldTurnAround()) Flip();

            _rb.linearVelocity = new Vector2(speed * _direction, _rb.linearVelocity.y);
        }

        private bool ShouldTurnAround()
        {
            bool hitsWall = wallCheck != null &&
                Physics2D.Raycast(wallCheck.position, Vector2.right * _direction, wallCheckDistance, obstacleLayers);

            bool aboutToFallOffEdge = edgeCheck != null &&
                !Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDistance, groundLayers);

            return hitsWall || aboutToFallOffEdge;
        }

        private void Flip()
        {
            _direction *= -1;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
