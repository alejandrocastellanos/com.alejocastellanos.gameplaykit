using GameplayKit.Core;
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
        [SerializeField] private LayerMask obstacleLayers = ~0;
        [SerializeField] private LayerMask groundLayers = ~0;

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
            // Sin wallCheck/edgeCheck asignados se usan los bordes del Collider2D del enemigo.
            var body = GetComponent<Collider2D>();
            Vector2 wallOrigin; float wallDistance = wallCheckDistance;
            if (wallCheck != null) wallOrigin = wallCheck.position;
            else if (body != null) { wallOrigin = body.bounds.center; wallDistance += body.bounds.extents.x; }
            else wallOrigin = transform.position;

            Vector2 edgeOrigin;
            bool canCheckEdge = true;
            if (edgeCheck != null) edgeOrigin = edgeCheck.position;
            else if (body != null) { Bounds b = body.bounds; edgeOrigin = new Vector2(b.center.x + _direction * (b.extents.x + 0.05f), b.min.y + 0.05f); }
            else { edgeOrigin = default; canCheckEdge = false; }

            bool hitsWall = PhysicsQuery2D.Raycast(wallOrigin, Vector2.right * _direction, wallDistance, obstacleLayers, transform).collider != null;

            bool aboutToFallOffEdge = canCheckEdge &&
                PhysicsQuery2D.Raycast(edgeOrigin, Vector2.down, edgeCheckDistance, groundLayers, transform).collider == null;

            return hitsWall || aboutToFallOffEdge;
        }

        private void Flip()
        {
            _direction *= -1;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
