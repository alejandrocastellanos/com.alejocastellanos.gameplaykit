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
        private float _minX = float.NegativeInfinity, _maxX = float.PositiveInfinity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            // Posiciones fijas: si los límites son hijos del enemigo se moverían con él y nunca los alcanzaría.
            if (leftBound != null) _minX = leftBound.position.x;
            if (rightBound != null) _maxX = rightBound.position.x;
        }

        private void Update()
        {
            if (_direction > 0 && transform.position.x >= _maxX)
            {
                _direction = -1;
                Flip();
            }
            else if (_direction < 0 && transform.position.x <= _minX)
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
