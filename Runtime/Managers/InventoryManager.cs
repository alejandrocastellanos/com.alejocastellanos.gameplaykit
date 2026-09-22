using System;
using System.Collections.Generic;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>Guarda ítems, llaves y equipo del jugador. Implementa IKeyHolder para que DoorWithKey (Entorno) lo consulte sin conocerlo directamente.</summary>
    public class InventoryManager : MonoBehaviour, IKeyHolder
    {
        private readonly HashSet<string> _items = new HashSet<string>();

        public event Action<string> OnItemAdded;
        public event Action<string> OnItemRemoved;

        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || !_items.Add(itemId)) return;
            OnItemAdded?.Invoke(itemId);
        }

        public bool HasItem(string itemId) => !string.IsNullOrEmpty(itemId) && _items.Contains(itemId);

        public void RemoveItem(string itemId)
        {
            if (!_items.Remove(itemId)) return;
            OnItemRemoved?.Invoke(itemId);
        }

        // --- IKeyHolder ---
        public bool HasKey(string keyId) => HasItem(keyId);
        public void RemoveKey(string keyId) => RemoveItem(keyId);
    }
}
