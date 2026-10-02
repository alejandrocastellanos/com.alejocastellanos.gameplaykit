using System.Collections;
using GameplayKit.Core;
using UnityEngine;

namespace GameplayKit.Health
{
    /// <summary>Feedback visual de daño: tiñe los sprites un instante al recibir un golpe y, mientras el personaje es
    /// invulnerable (CharacterHealth), los hace parpadear. Funciona con cualquier IHealthSource.</summary>
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private bool blinkWhileInvulnerable = true;
        [SerializeField] private float blinkInterval = 0.08f;

        public bool IsFlashing { get; private set; }

        private SpriteRenderer[] _renderers;
        private Color[] _originalColors;
        private IHealthSource _health;
        private CharacterHealth _characterHealth;
        private Coroutine _routine;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++) _originalColors[i] = _renderers[i].color;
        }

        private void OnEnable()
        {
            _health = GetComponentInParent<IHealthSource>();
            _characterHealth = GetComponentInParent<CharacterHealth>();
            if (_health != null) _health.Damaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Damaged -= HandleDamaged;
            Restore();
        }

        private void HandleDamaged(float amount)
        {
            if (!isActiveAndEnabled) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            IsFlashing = true;
            SetColor(flashColor, 1f);
            yield return new WaitForSeconds(flashDuration);
            Restore();

            bool visible = true;
            while (blinkWhileInvulnerable && _characterHealth != null && _characterHealth.IsInvulnerable && !_characterHealth.IsDead)
            {
                visible = !visible;
                SetAlpha(visible ? 1f : 0.25f);
                yield return new WaitForSeconds(blinkInterval);
            }
            Restore();
            IsFlashing = false;
            _routine = null;
        }

        private void SetColor(Color color, float alphaFactor)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                Color c = color;
                c.a = _originalColors[i].a * alphaFactor;
                _renderers[i].color = c;
            }
        }

        private void SetAlpha(float alphaFactor)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                Color c = _originalColors[i];
                c.a *= alphaFactor;
                _renderers[i].color = c;
            }
        }

        private void Restore()
        {
            if (_renderers == null) return;
            for (int i = 0; i < _renderers.Length; i++) if (_renderers[i] != null) _renderers[i].color = _originalColors[i];
            IsFlashing = false;
        }
    }
}
