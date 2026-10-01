using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>
    /// Aplica una fuerza constante a los Rigidbody2D dentro del área — para viento, corrientes de aire, etc.
    /// Se llama WindZone2D (y no WindZone) porque Unity no permite que un script comparta nombre con un
    /// componente nativo: con "WindZone" el menú Add Component y GetComponent no funcionan.
    /// </summary>
    public class WindZone2D : MonoBehaviour
    {
        [Header("Zona de viento")]
        [SerializeField] private Vector2 force = Vector2.right * 5f;

        private readonly TriggerOccupants<Rigidbody2D> _bodies = new TriggerOccupants<Rigidbody2D>();

        private void OnTriggerEnter2D(Collider2D other) => _bodies.Enter(other.attachedRigidbody);
        private void OnTriggerExit2D(Collider2D other) => _bodies.Exit(other.attachedRigidbody);

        private void FixedUpdate()
        {
            foreach (var rb in _bodies.Snapshot()) rb.AddForce(force, ForceMode2D.Force);
        }
    }
}
