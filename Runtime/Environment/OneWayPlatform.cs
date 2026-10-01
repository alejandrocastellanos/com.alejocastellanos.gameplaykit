using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Configura automáticamente un Collider2D + PlatformEffector2D para que la plataforma sea atravesable desde abajo pero sólida desde arriba.
    /// Si el objeto no tiene ningún Collider2D se agrega un BoxCollider2D; si ya tiene otro (Edge, Polygon...) se respeta.</summary>
    [RequireComponent(typeof(PlatformEffector2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [SerializeField] private float surfaceArc = 170f;

        // Collider2D es abstracto, así que no puede ir en [RequireComponent] (Unity no sabría cuál
        // instanciar y AddComponent fallaría). Lo garantizamos a mano aquí.
        private Collider2D EnsureCollider()
        {
            var col = GetComponent<Collider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.usedByEffector = true;
            return col;
        }

        private void Reset()
        {
            EnsureCollider();

            var effector = GetComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = surfaceArc;
        }

        private void Awake()
        {
            // También cubre el caso de agregar el componente por código en runtime, donde Reset() no se ejecuta.
            EnsureCollider();
        }
    }
}
