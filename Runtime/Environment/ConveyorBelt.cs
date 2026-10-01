using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Arrastra en una dirección constante a quien esté parado sobre ella, sumándose a su propio movimiento.</summary>
    public class ConveyorBelt : MonoBehaviour
    {
        [Header("Cinta transportadora")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private Vector2 direction = Vector2.right;

        private PlatformRiders _riders;

        private void Awake()
        {
            _riders = new PlatformRiders(GetComponent<Collider2D>());
        }

        private void OnCollisionEnter2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionStay2D(Collision2D collision) => _riders.OnContact(collision);
        private void OnCollisionExit2D(Collision2D collision) => _riders.OnContactEnded(collision);

        private void FixedUpdate()
        {
            _riders.Carry(direction.normalized * speed * Time.fixedDeltaTime);
        }
    }
}
