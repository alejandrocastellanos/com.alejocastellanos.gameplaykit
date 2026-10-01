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
        [Tooltip("Punto desde el que se busca suelo. Si se deja vacío se usa la base del Collider2D del personaje.")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask groundLayers = ~0;

        public Rigidbody2D Rigidbody { get; private set; }
        public bool IsGrounded { get; private set; }
        /// <summary>gravityScale original del Rigidbody2D; las habilidades que anulan la gravedad la restauran a este valor.</summary>
        public float DefaultGravityScale { get; private set; } = 1f;
        public Vector2 Velocity => Rigidbody.linearVelocity;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            DefaultGravityScale = Rigidbody.gravityScale;
            // Valores por defecto de un controlador de personaje: no rodar como una caja y no quedarse
            // pegado a las paredes por fricción al empujar contra ellas.
            Rigidbody.freezeRotation = true;
            var body = GetComponent<Collider2D>();
            if (body != null && body.sharedMaterial == null && Rigidbody.sharedMaterial == null)
            {
                body.sharedMaterial = NoFriction;
            }
            _bodyCollider = GetComponent<Collider2D>();
            if (groundCheck == null && _bodyCollider == null)
            {
                Debug.LogWarning($"[GameplayKit] {name}: CharacterController2D necesita un Collider2D o un Ground Check asignado para detectar el suelo.", this);
            }
        }

        private Collider2D _bodyCollider;

        private static PhysicsMaterial2D _noFriction;
        private static PhysicsMaterial2D NoFriction =>
            _noFriction != null ? _noFriction : (_noFriction = new PhysicsMaterial2D("GameplayKit_NoFriction") { friction = 0f, bounciness = 0f });

        private bool TryGetGroundProbe(out Vector2 point)
        {
            if (groundCheck != null) { point = groundCheck.position; return true; }
            if (_bodyCollider != null)
            {
                Bounds b = _bodyCollider.bounds;
                point = new Vector2(b.center.x, b.min.y);
                return true;
            }
            point = default;
            return false;
        }

        private void FixedUpdate()
        {
            IsGrounded = TryGetGroundProbe(out Vector2 probe) &&
                PhysicsQuery2D.OverlapCircle(probe, groundCheckRadius, groundLayers, transform);
        }

        private float _horizontalLockUntil;

        /// <summary>
        /// True mientras otra mecánica (wall jump, knockback...) es dueña de la velocidad horizontal;
        /// PlayerWalkRun no la sobrescribe con el input durante ese tiempo.
        /// </summary>
        public bool IsHorizontalControlLocked => Time.time < _horizontalLockUntil;

        public void LockHorizontalControl(float seconds)
        {
            _horizontalLockUntil = Mathf.Max(_horizontalLockUntil, Time.time + seconds);
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
            if (_bodyCollider == null) _bodyCollider = GetComponent<Collider2D>();
            if (!TryGetGroundProbe(out Vector2 probe)) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(probe, groundCheckRadius);
        }
    }
}
