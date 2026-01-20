using UnityEngine;
using Project.Inventory;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public class InventoryView : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private InventorySlotView slotPrefab;
        [SerializeField] private Transform slotsRoot;
        [SerializeField] private InventoryDetailsPanel detailsPanel;

        private InventorySlotView[] _slotViews;

        private void Awake()
        {
            if (inventory == null || slotPrefab == null || slotsRoot == null)
            {
                Debug.LogError("[InventoryView] Missing wiring (inventory/slotPrefab/slotsRoot).", this);
                enabled = false;
                return;
            }

            Build();
            RefreshAll();

            if (detailsPanel != null)
                detailsPanel.Bind(inventory);
        }

        private void OnEnable()
        {
            if (inventory == null) return;

            inventory.InventoryChanged += RefreshAll;
            inventory.InventoryVisibilityChanged += OnInventoryVisibilityChanged;

            RefreshAll();
        }

        private void OnInventoryVisibilityChanged(bool visible)
        {
            if (visible)
                RefreshAll();
        }

        private void OnDisable()
        {
            if (inventory == null) return;
            inventory.InventoryChanged -= RefreshAll;
            inventory.InventoryVisibilityChanged -= OnInventoryVisibilityChanged;
        }

        private void Build()
        {
            for (int i = slotsRoot.childCount - 1; i >= 0; i--)
                Destroy(slotsRoot.GetChild(i).gameObject);

            _slotViews = new InventorySlotView[inventory.SlotCount];

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                var view = Instantiate(slotPrefab, slotsRoot);
                view.Bind(inventory, i);
                _slotViews[i] = view;
            }
        }

        private void RefreshAll()
        {
            if (_slotViews == null) return;

            for (int i = 0; i < _slotViews.Length; i++)
                _slotViews[i]?.Refresh();
        }
    }
}
