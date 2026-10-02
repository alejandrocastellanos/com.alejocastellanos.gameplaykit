using UnityEngine;
using UnityEngine.UI;

namespace GameplayKit.UI
{
    /// <summary>Número flotante de daño o curación que sube y se desvanece. Normalmente lo crea DamagePopupSpawner;
    /// también se puede usar con un prefab propio (un Text de uGUI en un Canvas World Space con este componente).</summary>
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
        private Color _baseColor;
        private GameObject _destroyTarget; // lo que se destruye al terminar (el objeto creado por Spawn)

        private void Awake()
        {
            _label = GetComponent<Text>();
        }

        public void Setup(float amount, bool isHeal)
        {
            if (_label == null) _label = GetComponent<Text>();
            _label.text = (isHeal ? "+" : "-") + Mathf.RoundToInt(Mathf.Abs(amount));
            _baseColor = isHeal ? healColor : damageColor;
            _label.color = _baseColor;
        }

        private void Update()
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            _elapsed += Time.deltaTime;
            if (_label != null)
            {
                Color c = _baseColor;
                c.a = Mathf.Clamp01(1f - _elapsed / lifetime);
                _label.color = c;
            }
            if (_elapsed >= lifetime) Destroy(_destroyTarget != null ? _destroyTarget : gameObject);
        }

        /// <summary>Crea un número flotante en una posición del mundo. Con <paramref name="prefab"/> nulo arma uno sencillo.</summary>
        public static UIDamagePopup Spawn(Vector3 worldPosition, float amount, bool isHeal, UIDamagePopup prefab = null)
        {
            UIDamagePopup popup;
            if (prefab != null)
            {
                popup = Instantiate(prefab, worldPosition, Quaternion.identity);
                popup._destroyTarget = popup.gameObject;
            }
            else
            {
                var root = new GameObject("DamagePopup", typeof(RectTransform));
                root.transform.position = worldPosition;
                var canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 100;
                var rt = (RectTransform)root.transform;
                rt.sizeDelta = new Vector2(200f, 60f);
                rt.localScale = Vector3.one * 0.01f;

                var labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(root.transform, false);
                var text = labelGo.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 48;
                text.alignment = TextAnchor.MiddleCenter;
                var labelRt = (RectTransform)labelGo.transform;
                labelRt.sizeDelta = rt.sizeDelta;
                popup = labelGo.AddComponent<UIDamagePopup>();
                popup._destroyTarget = root;
            }
            popup.Setup(amount, isHeal);
            return popup;
        }
    }
}
