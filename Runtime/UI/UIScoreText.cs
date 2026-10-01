using GameplayKit.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayKit.UI
{
    /// <summary>Muestra el puntaje de ScoreManager en un Text de uGUI.</summary>
    [RequireComponent(typeof(Text))]
    public class UIScoreText : MonoBehaviour
    {
        [SerializeField] private string format = "Puntos: {0}";

        private Text _label;
        private ScoreManager _subscribed;

        private void Awake() => _label = GetComponent<Text>();
        private void OnEnable() => TrySubscribe();
        private void Start() => TrySubscribe();

        private void OnDisable()
        {
            if (_subscribed != null) _subscribed.OnScoreChanged -= Refresh;
            _subscribed = null;
        }

        private void TrySubscribe()
        {
            var manager = ScoreManager.Instance;
            if (manager == null || manager == _subscribed) return;
            if (_subscribed != null) _subscribed.OnScoreChanged -= Refresh;
            manager.OnScoreChanged += Refresh;
            _subscribed = manager;
            Refresh(manager.CurrentScore);
        }

        private void Refresh(int score) => _label.text = string.Format(format, score);
    }
}
