using System;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Puerta que solo se abre si quien la toca lleva el ítem requerido (via IKeyHolder, implementado por ejemplo por InventoryManager).</summary>
    public class DoorWithKey : MonoBehaviour
    {
        [Header("Puerta con llave")]
        [SerializeField] private string requiredItemId = "llave_dorada";
        [SerializeField] private bool consumeItemOnOpen = true;
        [SerializeField] private GameObject visualWhenClosed;
        [SerializeField] private Collider2D blockingCollider;

        public bool IsOpen { get; private set; }
        public event Action OnOpened;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsOpen) return;

            var keyHolder = other.GetComponentInParent<IKeyHolder>();
            if (keyHolder == null || !keyHolder.HasKey(requiredItemId)) return;

            if (consumeItemOnOpen) keyHolder.RemoveKey(requiredItemId);
            Open();
        }

        public void Open()
        {
            IsOpen = true;
            if (blockingCollider != null) blockingCollider.enabled = false;
            if (visualWhenClosed != null) visualWhenClosed.SetActive(false);
            OnOpened?.Invoke();
        }
    }
}
