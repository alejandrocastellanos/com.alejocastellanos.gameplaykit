using UnityEngine;

namespace GameplayKit.Managers
{
    /// <summary>
    /// Persiste el progreso entre sesiones usando PlayerPrefs + JSON (via JsonUtility). Punto único
    /// de guardado: cada sistema que necesite persistir expone su propio [Serializable] de datos
    /// y lo guarda/carga aquí, en vez de escribir directamente a PlayerPrefs desde todas partes.
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        public static SaveLoadSystem Instance { get; private set; }

        [SerializeField] private string saveKey = "GameplayKit.Save";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Save<T>(T data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        public bool TryLoad<T>(out T data)
        {
            if (!PlayerPrefs.HasKey(saveKey))
            {
                data = default;
                return false;
            }

            string json = PlayerPrefs.GetString(saveKey);
            try
            {
                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (System.ArgumentException)
            {
                // Guardado corrupto o de otra versión: se trata como si no existiera.
                Debug.LogWarning($"[GameplayKit] El guardado '{saveKey}' no se pudo leer y se ignora.");
                data = default;
                return false;
            }
        }

        public bool HasSave => PlayerPrefs.HasKey(saveKey);

        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(saveKey);
        }
    }
}
