using GameplayKit.Health;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayKit.UI
{
    /// <summary>Barra de vida (Slider o Image de tipo Fill) enlazada a un CharacterHealth via sus eventos.</summary>
    public class UIHealthBar : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Si se deja vacío, usa el CharacterHealth del objeto con tag Player.")]
        [SerializeField] private CharacterHealth health;
        [SerializeField] private Image fillImage;
        [SerializeField] private Slider slider;

        private void OnEnable()
        {
            if (health == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) health = player.GetComponentInParent<CharacterHealth>();
            }
            if (health == null) return;

            health.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        // El CharacterHealth puede inicializarse después de este OnEnable (orden de Awake entre objetos):
        // en Start ya tiene su vida real, así que se vuelve a pintar.
        private void Start()
        {
            if (health != null) HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
        }

        private void OnDisable()
        {
            if (health != null) health.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;

            if (fillImage != null) fillImage.fillAmount = ratio;
            if (slider != null) slider.value = ratio;
        }
    }
}
