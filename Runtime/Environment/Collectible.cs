using System;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Ítem recogible (moneda, gema, power-up): dispara eventos y se destruye al ser tocado por el jugador.</summary>
    public class Collectible : MonoBehaviour
    {
        [Header("Coleccionable")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private int value = 1;
        [SerializeField] private GameObject collectEffectPrefab;

        /// <summary>Evento estático: permite que un ScoreManager escuche todos los coleccionables sin referenciarlos uno a uno.</summary>
        public static event Action<Collectible, int> OnAnyCollected;
        public event Action OnCollected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            if (collectEffectPrefab != null) Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

            OnCollected?.Invoke();
            OnAnyCollected?.Invoke(this, value);
            Destroy(gameObject);
        }
    }
}
