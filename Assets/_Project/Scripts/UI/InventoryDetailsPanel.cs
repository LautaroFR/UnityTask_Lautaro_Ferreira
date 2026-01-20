using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Project.Inventory;
using Project.Items;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public class InventoryDetailsPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button useEquipButton;
        [SerializeField] private TMP_Text useEquipButtonText;

        private PlayerInventory _inventory;

        public void Bind(PlayerInventory inventory)
        {
            _inventory = inventory;

            if (_inventory != null)
                _inventory.SelectionChanged += OnSelectionChanged;

            if (removeButton != null) removeButton.onClick.AddListener(OnRemoveClicked);
            if (useEquipButton != null) useEquipButton.onClick.AddListener(OnUseEquipClicked);

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(int index)
        {
            if (_inventory == null) return;

            if (index < 0 || index >= _inventory.Slots.Length || _inventory.Slots[index].IsEmpty)
            {
                SetVisible(false);
                return;
            }

            var slot = _inventory.Slots[index];
            var item = slot.item;

            if (item == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (titleText != null) titleText.text = item.displayName;
            if (descriptionText != null) descriptionText.text = item.description;

            if (useEquipButtonText != null)
                useEquipButtonText.text = item.type == ItemType.Weapon ? "Equip" : "Use";
        }

        private void OnRemoveClicked()
        {
            if (_inventory == null) return;
            if (_inventory.SelectedIndex < 0) return;

            _inventory.TryRemoveAt(_inventory.SelectedIndex, int.MaxValue);
        }

        private void OnUseEquipClicked()
        {
            if (_inventory == null) return;
            _inventory.TryUseOrEquipSelected();
        }

        private void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }
    }
}
