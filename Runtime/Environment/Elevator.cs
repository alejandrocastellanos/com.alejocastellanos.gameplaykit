using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Plataforma vertical (o entre dos puntos cualquiera) que se activa externamente, típicamente desde un Lever o PressurePlate.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Elevator : MonoBehaviour
    {
        [Header("Elevador")]
        [SerializeField] private Transform bottomPoint;
        [SerializeField] private Transform topPoint;
        [SerializeField] private float speed = 2f;

        public bool IsMoving { get; private set; }

        private Rigidbody2D _rb;
        private Transform _destination;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        /// <summary>Llamado desde un Lever/PressurePlate (por ejemplo suscrito a su evento) para mandar el elevador al otro extremo.</summary>
        public void Activate()
        {
            if (IsMoving) return;

            bool atBottom = _destination == null || _destination == topPoint;
            _destination = atBottom ? topPoint : bottomPoint;
            IsMoving = true;
        }

        private void FixedUpdate()
        {
            if (!IsMoving || _destination == null) return;

            Vector2 newPosition = Vector2.MoveTowards(_rb.position, _destination.position, speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPosition);

            if (Vector2.Distance(newPosition, _destination.position) < 0.01f)
            {
                IsMoving = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player")) collision.transform.SetParent(transform);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.transform.CompareTag("Player")) collision.transform.SetParent(null);
        }
    }
}
