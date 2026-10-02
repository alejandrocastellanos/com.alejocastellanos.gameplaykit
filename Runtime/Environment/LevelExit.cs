using System;
using GameplayKit.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameplayKit.Environment
{
    /// <summary>Meta del nivel: al tocarla el jugador se carga otra escena (con fundido si hay SceneTransitionManager).
    /// Puede exigir un ítem del inventario. Sin escena asignada carga la siguiente de la lista del build.</summary>
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("Escena a cargar. Vacío = la siguiente en la lista de escenas del build.")]
        [SerializeField] private string sceneName = "";
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Ítem necesario para usar la salida (IKeyHolder, ej. InventoryManager). Vacío = sin requisito.")]
        [SerializeField] private string requiredItemId = "";

        public bool Triggered { get; private set; }
        public event Action OnExitReached;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Triggered || !TagFilter.Matches(other, playerTag)) return;
            if (!string.IsNullOrEmpty(requiredItemId))
            {
                var holder = other.GetComponentInParent<IKeyHolder>();
                if (holder == null || !holder.HasKey(requiredItemId)) return;
            }

            Triggered = true;
            OnExitReached?.Invoke();
            Load();
        }

        private void Load()
        {
            var transition = GameplayKit.Managers.SceneTransitionManager.Instance;
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (transition != null) transition.LoadScene(sceneName); else SceneManager.LoadScene(sceneName);
                return;
            }

            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next <= 0 || next >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogWarning("[GameplayKit] LevelExit: no hay escena siguiente en la lista del build; asigna Scene Name.");
                return;
            }
            if (transition != null) transition.LoadScene(next); else SceneManager.LoadScene(next);
        }
    }
}
