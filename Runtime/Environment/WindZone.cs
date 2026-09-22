using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>
    /// Aplica una fuerza constante a los Rigidbody2D dentro del área — para viento, corrientes de aire, etc.
    /// Nota de nombres: coincide con UnityEngine.WindZone (viento de partículas/terreno, otro sistema);
    /// si algún script importa "using UnityEngine;" junto a este namespace, referenciar esta clase
    /// como GameplayKit.Environment.WindZone para evitar ambigüedad.
    /// </summary>
    public class WindZone : MonoBehaviour
    {
        [Header("Zona de viento")]
        [SerializeField] private Vector2 force = Vector2.right * 5f;

        private void OnTriggerStay2D(Collider2D other)
        {
            var rb = other.attachedRigidbody;
            rb?.AddForce(force, ForceMode2D.Force);
        }
    }
}
