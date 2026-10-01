using UnityEngine;
using GameplayKit.Core;

namespace GameplayKit.Movement
{
    /// <summary>Se desliza a velocidad constante entre dos puntos al entrar en contacto con una tirolesa.</summary>
    public class PlayerZipline : AbilityBase
    {
        [Tooltip("Filtro opcional: si no está vacío, solo se usan tirolesas (ZiplinePath) con este tag.")]
        [SerializeField] private string ziplineTag = "";
        [SerializeField] private float rideSpeed = 8f;

        public bool IsRiding { get; private set; }
        private Transform _start;
        private Transform _end;
        private float _progress;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var zipline = other.GetComponent<ZiplinePath>();
            if (zipline == null || !TagFilter.PassesOptional(other, ziplineTag)) return;

            _start = zipline.Start;
            _end = zipline.End;
            _progress = 0f;
            IsRiding = true;
            Character.Controller.Rigidbody.gravityScale = 0f;
        }

        public override void ProcessAbility()
        {
            if (!IsRiding) return;

            float distance = Vector2.Distance(_start.position, _end.position);
            _progress += rideSpeed * Time.deltaTime / Mathf.Max(distance, 0.001f);
            transform.position = Vector3.Lerp(_start.position, _end.position, _progress);

            if (_progress >= 1f || CharacterInput.JumpPressedThisFrame)
            {
                IsRiding = false;
                Character.Controller.Rigidbody.gravityScale = Character.Controller.DefaultGravityScale;
            }
        }
    }

    /// <summary>Marca los dos extremos de una tirolesa. Se coloca en el mismo objeto que el trigger de entrada,
    /// y ese objeto debe tener el tag configurado en PlayerZipline (por defecto "Zipline").</summary>
    public class ZiplinePath : MonoBehaviour
    {
        public Transform Start;
        public Transform End;
    }
}
