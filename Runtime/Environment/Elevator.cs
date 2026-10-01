using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Plataforma vertical (o entre dos puntos cualquiera) que se activa externamente, típicamente desde un Lever o PressurePlate.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Elevator : MonoBehaviour
    {
        [Header("Elevador")]
        [Tooltip("Extremo inferior. Puede ser hijo del elevador: su posición se fija al iniciar.")]
        [SerializeField] private Transform bottomPoint;
        [Tooltip("Extremo superior. Puede ser hijo del elevador: su posición se fija al iniciar.")]
        [SerializeField] private Transform topPoint;
        [SerializeField] private float speed = 2f;

        public bool IsMoving { get; private set; }

        private Rigidbody2D _rb;
        private Vector2 _bottom, _top;
        private bool _goingUp;
        private bool _hasMoved;
        private PlatformRiders _riders;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _riders = new PlatformRiders(GetComponent<Collider2D>());
            _bottom = bottomPoint != null ? (Vector2)bottomPoint.position : _rb.position;
            _top = topPoint != null ? (Vector2)topPoint.position : _rb.position;
        }

        /// <summary>Llamado desde un Lever/PressurePlate (por ejemplo suscrito a su evento) para mandar el elevador al otro extremo.
        /// La primera activación sube; las siguientes alternan entre extremos.</summary>
        public void Activate()
        {
            if (IsMoving) return;
            _goingUp = !_hasMoved || !_goingUp;
            _hasMoved = true;
            IsMoving = true;
        }

        private void FixedUpdate()
        {
            if (!IsMoving) return;

            Vector2 destination = _goingUp ? _top : _bottom;
            Vector2 newPosition = Vector2.MoveTowards(_rb.position, destination, speed * Time.fixedDeltaTime);
            _riders.Carry(newPosition - _rb.position);
            _rb.MovePosition(newPosition);

            if (Vector2.Distance(newPosition, destination) < 0.01f)
            {
                IsMoving = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionStay2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionExit2D(Collision2D collision) => _riders.OnContactEnded(collision);
    }
}
