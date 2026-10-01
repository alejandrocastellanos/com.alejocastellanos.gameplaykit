using UnityEngine;

namespace GameplayKit.Core
{
    /// <summary>Algo que el jugador puede accionar con el botón de interactuar (palancas, cofres, NPCs...).
    /// PlayerInteract busca el más cercano y llama Interact.</summary>
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }
}
