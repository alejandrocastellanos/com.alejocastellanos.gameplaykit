using System;
using UnityEngine;

namespace GameplayKit.Combat
{
    /// <summary>Guarda varias armas (como GameObjects hijos) y activa solo la equipada, desactivando el resto.</summary>
    public class WeaponInventorySlot : MonoBehaviour
    {
        [Header("Inventario de armas")]
        [SerializeField] private GameObject[] weapons;
        [SerializeField] private int startingWeaponIndex = 0;

        public int EquippedIndex { get; private set; } = -1;
        public event Action<int> OnWeaponEquipped;

        private void Start()
        {
            EquipWeapon(startingWeaponIndex);
        }

        public void EquipWeapon(int index)
        {
            if (weapons == null || index < 0 || index >= weapons.Length) return;

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null) weapons[i].SetActive(i == index);
            }

            EquippedIndex = index;
            OnWeaponEquipped?.Invoke(index);
        }

        public void EquipNext()
        {
            if (weapons == null || weapons.Length == 0) return;
            EquipWeapon((EquippedIndex + 1) % weapons.Length);
        }

        public void EquipPrevious()
        {
            if (weapons == null || weapons.Length == 0) return;
            EquipWeapon((EquippedIndex - 1 + weapons.Length) % weapons.Length);
        }
    }
}
