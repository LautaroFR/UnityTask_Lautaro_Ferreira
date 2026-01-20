using Item.Inventory;
using Project.Inventory;
using UnityEngine;

namespace Project.Interaction
{
    [DisallowMultipleComponent]
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Keys")]
        [SerializeField] private KeyCode interactKey;
        [SerializeField] private KeyCode pickupKey;
        [SerializeField] private KeyCode inventoryKey;

        private PlayerInventory _inventory;

        private IInteractable _currentInteractable;
        private IPickupable _currentPickupable;

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(interactKey))
                _currentInteractable?.Interact();

            if (Input.GetKeyDown(pickupKey))
                _currentPickupable?.Pickup();

            if (Input.GetKeyDown(inventoryKey))
            {
                _inventory?.ToggleInventoryUI();

                if (_inventory != null && !_inventory.IsOpen)
                    Project.Save.SaveService.Save(_inventory.ToSaveData());
            }
        }

        public void SetInteractable(IInteractable interactable) => _currentInteractable = interactable;

        public void ClearInteractable(IInteractable interactable)
        {
            if (_currentInteractable == interactable)
                _currentInteractable = null;
        }

        public void SetPickupable(IPickupable pickupable) => _currentPickupable = pickupable;

        public void ClearPickupable(IPickupable pickupable)
        {
            if (_currentPickupable == pickupable)
                _currentPickupable = null;
        }
    }
}
