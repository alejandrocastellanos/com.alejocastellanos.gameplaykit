using System.Collections.Generic;

namespace GameplayKit.Environment
{
    /// <summary>
    /// Registro de quién está dentro de un trigger, mantenido con Enter/Exit. OnTriggerStay2D deja de
    /// llamarse cuando un Rigidbody2D se duerme (por ejemplo, un jugador quieto), así que las zonas que
    /// actúan "mientras estés dentro" usan este registro y aplican su efecto desde Update/FixedUpdate.
    /// Cuenta colliders por ocupante para que un personaje con varios colliders no salga antes de tiempo.
    /// </summary>
    internal class TriggerOccupants<T> where T : class
    {
        private readonly Dictionary<T, int> _counts = new Dictionary<T, int>();
        private readonly List<T> _snapshot = new List<T>();

        public void Enter(T occupant)
        {
            if (occupant == null) return;
            _counts.TryGetValue(occupant, out int count);
            _counts[occupant] = count + 1;
        }

        /// <summary>Devuelve true si el ocupante salió del todo (ya no le queda ningún collider dentro).</summary>
        public bool Exit(T occupant)
        {
            if (occupant == null || !_counts.TryGetValue(occupant, out int count)) return false;
            if (count <= 1) { _counts.Remove(occupant); return true; }
            _counts[occupant] = count - 1;
            return false;
        }

        /// <summary>Copia estable para iterar (se puede modificar el registro dentro del bucle). Descarta objetos destruidos.</summary>
        public List<T> Snapshot()
        {
            _snapshot.Clear();
            foreach (var occupant in _counts.Keys)
            {
                if (occupant is UnityEngine.Object unityObject && unityObject == null) continue;
                _snapshot.Add(occupant);
            }
            if (_snapshot.Count != _counts.Count)
            {
                var alive = new HashSet<T>(_snapshot);
                var dead = new List<T>();
                foreach (var key in _counts.Keys) if (!alive.Contains(key)) dead.Add(key);
                foreach (var key in dead) _counts.Remove(key);
            }
            return _snapshot;
        }
    }
}
