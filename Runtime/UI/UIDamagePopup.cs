using UnityEngine;
using UnityEngine.UI;

namespace GameplayKit.UI
{
    /// <summary>Texto flotante de daño/curación que aparece sobre un objetivo y se desvanece. Se instancia como prefab y se configura con Setup().</summary>
    [RequireComponent(typeof(Text))]
    public class UIDamagePopup : MonoBehaviour
    {
        [Header("Animación")]
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float lifetime = 0.8f;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;

        private Text _label;
        private float _elapsed;

        private void Awake()
        {
            _label = GetComponent<Text>();
        }

        public void Setup(float amount, bool isHeal)
        {
            _label.text = (isHeal ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(amount));
            _label.color = isHeal ? healColor : damageColor;
        }

        private void Update()
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            _elapsed += Time.deltaTime;

            if (_elapsed >= lifetime) Destroy(gameObject);
        }
    }
}
