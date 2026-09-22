namespace GameplayKit.Core
{
    /// <summary>
    /// Contrato para cualquier cosa que pueda llevar llaves/ítems de un inventario (normalmente el jugador).
    /// Permite que piezas de entorno como DoorWithKey pregunten por un ítem sin conocer InventoryManager.
    /// </summary>
    public interface IKeyHolder
    {
        bool HasKey(string keyId);
        void RemoveKey(string keyId);
    }
}
