using UnityEngine;

namespace GameplayKit.AI
{
    /// <summary>Igual que EnemyPatrol, pero además respeta un límite izquierdo y derecho fijos en el mundo.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyPatrolWithinBounds : MonoBehaviour
    {
        [Header("Patrulla acotada")]
        [SerializeField] private float speed = 2f;
        [SerializeField] private Transform leftBound;
        [SerializeField] private Transform rightBound;

        private Rigidbody2D _rb;
        private int _direction = 1;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_direction > 0 && rightBound != null && transform.position.x >= rightBound.position.x)
            {
                _direction = -1;
                Flip();
            }
            else if (_direction < 0 && leftBound != null && transform.position.x <= leftBound.position.x)
            {
                _direction = 1;
                Flip();
            }

            _rb.linearVelocity = new Vector2(speed * _direction, _rb.linearVelocity.y);
        }

        private void Flip()
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
}
