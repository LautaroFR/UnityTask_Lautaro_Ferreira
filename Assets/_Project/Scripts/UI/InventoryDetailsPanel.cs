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

            if (removeButton != null)
                removeButton.onClick.AddListener(OnRemoveClicked);

            if (useEquipButton != null)
                useEquipButton.onClick.AddListener(OnUseEquipClicked);

            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(int index)
        {
            if (_inventory == null)
            {
                SetVisible(false);
                return;
            }

            // ─────────────────────────────────────────────
            // NOTHING SELECTED
            // ─────────────────────────────────────────────
            if (index == -1)
            {
                SetVisible(false);
                return;
            }

            // ─────────────────────────────────────────────
            // 🔥 EQUIPPED WEAPON SLOT
            // ─────────────────────────────────────────────
            if (index == PlayerInventory.EquippedSlotIndex)
            {
                var equippedItem = _inventory.GetEquippedWeaponItem();
                if (equippedItem == null)
                {
                    SetVisible(false);
                    return;
                }

                SetVisible(true);

                if (titleText != null) titleText.text = equippedItem.displayName;
                if (descriptionText != null) descriptionText.text = equippedItem.description;

                // Equipped item cannot be removed directly
                if (removeButton != null)
                    removeButton.gameObject.SetActive(false);

                if (useEquipButton != null)
                {
                    useEquipButton.gameObject.SetActive(true);
                    if (useEquipButtonText != null)
                        useEquipButtonText.text = "Unequip";
                }

                return;
            }

            // ─────────────────────────────────────────────
            // NORMAL INVENTORY SLOT
            // ─────────────────────────────────────────────
            if (index < 0 || index >= _inventory.Slots.Length)
            {
                SetVisible(false);
                return;
            }

            var slot = _inventory.Slots[index];
            if (slot.IsEmpty || slot.item == null)
            {
                SetVisible(false);
                return;
            }

            var item = slot.item;

            SetVisible(true);

            if (titleText != null) titleText.text = item.displayName;
            if (descriptionText != null) descriptionText.text = item.description;

            if (removeButton != null)
                removeButton.gameObject.SetActive(true);

            if (useEquipButton != null)
            {
                useEquipButton.gameObject.SetActive(true);

                if (useEquipButtonText != null)
                {
                    useEquipButtonText.text =
                        item.type == ItemType.Weapon ? "Equip" : "Use";
                }
            }
        }

        private void OnRemoveClicked()
        {
            if (_inventory == null) return;

            int index = _inventory.SelectedIndex;

            // Do not allow removing equipped weapon from here
            if (index == PlayerInventory.EquippedSlotIndex)
                return;

            if (index < 0)
                return;

            _inventory.TryRemoveAt(index, int.MaxValue);
        }

        private void OnUseEquipClicked()
        {
            if (_inventory == null) return;

            int index = _inventory.SelectedIndex;

            if (index == PlayerInventory.EquippedSlotIndex)
            {
                // Unequip
                _inventory.TryUnequipWeapon();
                return;
            }

            _inventory.TryUseOrEquipSelected();
        }

        private void SetVisible(bool visible)
        {
            if (root != null)
                root.SetActive(visible);
        }
    }
}
