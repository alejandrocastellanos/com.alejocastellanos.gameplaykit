using UnityEngine;

namespace GameplayKit.Movement
{
    /// <summary>Marca los dos extremos de una tirolesa. Se coloca en el mismo objeto que el trigger de entrada,
    /// y ese objeto debe tener el tag configurado en PlayerZipline (por defecto "Zipline").</summary>
    public class ZiplinePath : MonoBehaviour
    {
        public Transform Start;
        public Transform End;
    }
}
