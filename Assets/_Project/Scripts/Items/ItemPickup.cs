using UnityEngine;
using Project.Items;

namespace Project.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private GameItemData item;

        [Header("Wiring")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform tooltip;

        private bool _playerInRange;
        private bool _pickedUp;

        private const KeyCode InteractKey = KeyCode.P;

        private void Awake()
        {
            if (tooltip != null)
                tooltip.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_pickedUp || !_playerInRange) return;

            if (Input.GetKeyDown(InteractKey))
            {
                if (item == null || inventory == null) return;

                inventory.Add(item);
                _pickedUp = true;

                Debug.Log($"Picked up {item.displayName}");

                if (tooltip != null)
                    tooltip.gameObject.SetActive(false);

                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_pickedUp) return;
            if (!other.CompareTag("Player")) return;

            if (inventory == null)
                inventory = other.GetComponent<PlayerInventory>();

            _playerInRange = true;
            if (tooltip != null) tooltip.gameObject.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_pickedUp) return;
            if (!other.CompareTag("Player")) return;

            _playerInRange = false;
            if (tooltip != null) tooltip.gameObject.SetActive(false);
        }
    }
}
