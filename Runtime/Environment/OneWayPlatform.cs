using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Configura automáticamente un Collider2D + PlatformEffector2D para que la plataforma sea atravesable desde abajo pero sólida desde arriba.</summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformEffector2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private float surfaceArc = 170f;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.usedByEffector = true;

            var effector = GetComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = surfaceArc;
        }
    }
}
