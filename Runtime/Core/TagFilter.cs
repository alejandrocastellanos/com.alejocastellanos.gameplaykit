using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>
    /// Comparación de tags tolerante: CompareTag lanza error si el tag no existe en el Tag Manager,
    /// lo que rompe un componente recién agregado en un proyecto que aún no creó ese tag.
    /// </summary>
    public static class TagFilter
    {
        /// <summary>True solo si <paramref name="tag"/> no está vacío y el objeto lo tiene.</summary>
        public static bool Matches(Component component, string tag) =>
            component != null && !string.IsNullOrEmpty(tag) && HasTag(component, tag);

        /// <summary>Filtro opcional: un tag vacío acepta cualquier objeto.</summary>
        public static bool PassesOptional(Component component, string tag) =>
            component != null && (string.IsNullOrEmpty(tag) || HasTag(component, tag));

        // Si es un collider hijo (ej. el collider vive en un hijo del jugador), también vale el tag de su Rigidbody2D.
        private static bool HasTag(Component component, string tag)
        {
            if (component.gameObject.tag == tag) return true;
            return component is Collider2D col && col.attachedRigidbody != null && col.attachedRigidbody.gameObject.tag == tag;
        }
    }
}
