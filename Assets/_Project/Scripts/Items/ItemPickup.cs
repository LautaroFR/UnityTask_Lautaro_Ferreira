using UnityEngine;
using Project.Items;
using Project.Inventory;

namespace Project.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour, IPickupable
    {
        [Header("Item")]
        [SerializeField] private GameItemData item;

        [Header("UI")]
        [SerializeField] private Transform tooltip;

        private PlayerInventory _inventory;
        private PlayerInteractor _interactor;
        private bool _pickedUp;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (tooltip != null)
                tooltip.gameObject.SetActive(false);
        }

        public void Pickup()
        {
            if (_pickedUp) return;
            if (_inventory == null || item == null) return;

            if (!_inventory.TryAdd(item, 1))
                return;

            _pickedUp = true;

            if (tooltip != null)
                tooltip.gameObject.SetActive(false);

            // Unregister from interactor so it doesn't keep a stale reference
            if (_interactor != null)
                _interactor.ClearPickupable(this);

            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_pickedUp) return;
            if (!other.CompareTag("Player")) return;

            _inventory = other.GetComponent<PlayerInventory>();
            _interactor = other.GetComponent<PlayerInteractor>();

            if (_interactor != null)
                _interactor.SetPickupable(this);

            if (tooltip != null)
                tooltip.gameObject.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_pickedUp) return;
            if (!other.CompareTag("Player")) return;

            if (_interactor != null)
                _interactor.ClearPickupable(this);

            _inventory = null;
            _interactor = null;

            if (tooltip != null)
                tooltip.gameObject.SetActive(false);
        }
    }
}
