using System.Collections.Generic;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>
    /// Mantiene vivo al GameObject entre cargas de escena (DontDestroyOnLoad), evitando duplicados
    /// mediante un identificador de persistencia (útil para Player, GameManager, etc. por separado).
    /// </summary>
    public class CharacterPersistence : MonoBehaviour
    {
        [Header("Persistencia")]
        [SerializeField] private string persistenceId = "Player";

        private static readonly Dictionary<string, CharacterPersistence> _instances = new Dictionary<string, CharacterPersistence>();

        private void Awake()
        {
            if (_instances.TryGetValue(persistenceId, out var existing) && existing != null && existing != this)
            {
                Destroy(gameObject);
                return;
            }

            _instances[persistenceId] = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instances.TryGetValue(persistenceId, out var existing) && existing == this)
            {
                _instances.Remove(persistenceId);
            }
        }
    }
}
