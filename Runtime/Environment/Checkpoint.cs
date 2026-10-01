using System;
using GameplayKit.Health;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Environment
{
    /// <summary>Guarda el punto de respawn activo del jugador (via CharacterRespawn) al ser tocado.</summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private GameObject activeVisual;

        public bool IsActivated { get; private set; }
        public event Action OnActivated;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActivated || !TagFilter.Matches(other, playerTag)) return;

            var respawn = other.GetComponentInParent<CharacterRespawn>();
            if (respawn != null) respawn.SetCheckpoint(transform);

            IsActivated = true;
            if (activeVisual != null) activeVisual.SetActive(true);
            OnActivated?.Invoke();
        }
    }
}
