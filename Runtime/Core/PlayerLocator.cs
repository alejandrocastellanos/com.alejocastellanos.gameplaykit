using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Busca al jugador por el tag "Player" cuando un componente no tiene objetivo asignado (por ejemplo,
    /// enemigos instanciados en runtime por EnemySpawner). Limita la búsqueda a una vez cada medio segundo.
    /// </summary>
    public static class PlayerLocator
    {
        public const string PlayerTag = "Player";
        private const float SearchInterval = 0.5f;

        /// <summary>Devuelve true si <paramref name="target"/> tiene valor; si está vacío y toca buscar,
        /// lo llena con el objeto de tag Player.</summary>
        public static bool Resolve(ref Transform target, ref float nextSearchTime)
        {
            if (target != null) return true;
            if (Time.time < nextSearchTime) return false;
            nextSearchTime = Time.time + SearchInterval;
            GameObject player = GameObject.FindWithTag(PlayerTag);
            if (player != null) target = player.transform;
            return target != null;
        }
    }
}
