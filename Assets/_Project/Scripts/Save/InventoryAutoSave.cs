using UnityEngine;
using Project.Inventory;

namespace Project.Save
{
    [DisallowMultipleComponent]
    public class InventoryAutoSave : MonoBehaviour
    {
        private PlayerInventory _inventory;

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GetComponent<PlayerInventory>();
        }

        private void OnEnable()
        {
            if (_inventory != null)
                _inventory.InventoryChanged += Save;
        }

        private void OnDisable()
        {
            if (_inventory != null)
                _inventory.InventoryChanged -= Save;
        }

        private void Save()
        {
            SaveService.Save(_inventory.ToSaveData());
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}
