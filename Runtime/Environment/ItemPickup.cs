using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Objeto recogible que entrega un ítem/llave a quien lo toque si puede guardarlo (IKeyHolder, como InventoryManager).
    /// Se usa con DoorWithKey: mismo Item Id en ambos.</summary>
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private string itemId = "llave_dorada";
        [SerializeField] private GameObject pickupEffectPrefab;

        public string ItemId => itemId;
        public event Action<GameObject> OnPickedUp;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var holder = other.GetComponentInParent<IKeyHolder>();
            if (holder == null) return;

            holder.AddKey(itemId);
            if (pickupEffectPrefab != null) Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            OnPickedUp?.Invoke(other.gameObject);
            Destroy(gameObject);
        }
    }
}
