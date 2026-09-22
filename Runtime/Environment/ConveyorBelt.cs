using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Arrastra en una dirección constante a quien esté parado sobre ella.</summary>
    public class ConveyorBelt : MonoBehaviour
    {
        [Header("Cinta transportadora")]
        [SerializeField] private float speed = 3f;
        [SerializeField] private Vector2 direction = Vector2.right;

        private void OnCollisionStay2D(Collision2D collision)
        {
            var rb = collision.rigidbody;
            if (rb == null) return;

            rb.MovePosition(rb.position + direction.normalized * speed * Time.fixedDeltaTime);
        }
    }
}
