using UnityEngine;
using Project.Items;
using UnityEngine.UI;

namespace Project.Inventory
{
    [DisallowMultipleComponent]
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Min(1)] private int slotCount = 16;

        [Header("UI (optional)")]
        [SerializeField] private GameObject inventoryUI;

        public int SlotCount => slotCount;
        public InventorySlot[] Slots { get; private set; }

        public int SelectedIndex { get; private set; } = -1;

        public string EquippedWeaponId { get; private set; }

        public event System.Action InventoryChanged;
        public event System.Action<int> SelectionChanged;
        public event System.Action<bool> InventoryVisibilityChanged;
        public event System.Action<string> EquippedWeaponChanged;

        private void Awake()
        {
            Slots = new InventorySlot[slotCount];
        }

        private void Start()
        {
            if (inventoryUI != null)
                inventoryUI.SetActive(false);

            Select(-1);
        }

        public void ToggleInventoryUI()
        {
            if (inventoryUI == null) return;

            bool newState = !inventoryUI.activeSelf;
            inventoryUI.SetActive(newState);
            InventoryVisibilityChanged?.Invoke(newState);

            // Optional: when closing, clear selection
            if (!newState)
                Select(-1);
        }

        public void Select(int index)
        {
            if (index < -1 || index >= Slots.Length) return;

            SelectedIndex = index;
            SelectionChanged?.Invoke(SelectedIndex);
        }

        public bool TryAdd(GameItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

            // Stack rule (simple & sensible): Consumables stack, Weapons don't.
            bool canStack = item.type == ItemType.Consumable;

            if (canStack)
            {
                int existing = FindSlotByItemId(item.id);
                if (existing >= 0)
                {
                    Slots[existing].amount += amount;
                    InventoryChanged?.Invoke();
                    return true;
                }
            }

            int empty = FindFirstEmptySlot();
            if (empty < 0) return false;

            Slots[empty].item = item;
            Slots[empty].amount = amount;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryMove(int from, int to)
        {
            if (!IsValidIndex(from) || !IsValidIndex(to)) return false;
            if (from == to) return false;
            if (Slots[from].IsEmpty) return false;
            if (!Slots[to].IsEmpty) return false;

            Slots[to] = Slots[from];
            Slots[from].Clear();

            if (SelectedIndex == from) SelectedIndex = to;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryRemoveAt(int index, int amount = int.MaxValue)
        {
            if (!IsValidIndex(index)) return false;
            if (Slots[index].IsEmpty) return false;

            int removeAmount = Mathf.Clamp(amount, 1, Slots[index].amount);
            Slots[index].amount -= removeAmount;

            if (Slots[index].amount <= 0)
                Slots[index].Clear();

            // Keep things tidy
            Compact();

            // If selection got emptied, clear it
            if (SelectedIndex == index && Slots[index].IsEmpty)
                Select(-1);

            InventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Reorders the inventory like a list: removes slot from 'from' and inserts at 'to',
        /// shifting intermediate items. Works great for "move to end and reorder".
        /// </summary>
        public bool TryReorder(int from, int to)
        {
            if (!IsValidIndex(from) || !IsValidIndex(to)) return false;
            if (from == to) return false;
            if (Slots[from].IsEmpty) return false;

            InventorySlot temp = Slots[from];

            if (from < to)
            {
                for (int i = from; i < to; i++)
                    Slots[i] = Slots[i + 1];
            }
            else
            {
                for (int i = from; i > to; i--)
                    Slots[i] = Slots[i - 1];
            }

            Slots[to] = temp;

            // Keep selection following the item logically (simple rule)
            if (SelectedIndex == from) SelectedIndex = to;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool TrySwap(int a, int b)
        {
            if (!IsValidIndex(a) || !IsValidIndex(b)) return false;
            if (a == b) return false;

            (Slots[a], Slots[b]) = (Slots[b], Slots[a]);

            if (SelectedIndex == a) SelectedIndex = b;
            else if (SelectedIndex == b) SelectedIndex = a;

            InventoryChanged?.Invoke();
            return true;
        }

        public bool TryUseOrEquipSelected()
        {
            if (!IsValidIndex(SelectedIndex)) return false;
            if (Slots[SelectedIndex].IsEmpty) return false;

            var slot = Slots[SelectedIndex];
            var item = slot.item;

            if (item == null) return false;

            switch (item.type)
            {
                case ItemType.Consumable:
                    TryRemoveAt(SelectedIndex, 1);
                    return true;

                case ItemType.Weapon:
                    EquippedWeaponId = item.id;
                    EquippedWeaponChanged?.Invoke(EquippedWeaponId);
                    InventoryChanged?.Invoke();
                    return true;

                default:
                    return false;
            }
        }

        public bool HasItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            return FindSlotByItemId(itemId) >= 0;
        }

        private void Compact()
        {
            // Moves all non-empty slots to the front preserving order.
            int write = 0;
            for (int read = 0; read < Slots.Length; read++)
            {
                if (Slots[read].IsEmpty) continue;

                if (write != read)
                    Slots[write] = Slots[read];

                write++;
            }

            for (int i = write; i < Slots.Length; i++)
                Slots[i].Clear();
        }

        private int FindSlotByItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return -1;

            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].IsEmpty) continue;
                if (Slots[i].item != null && Slots[i].item.id == itemId)
                    return i;
            }
            return -1;
        }

        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i].IsEmpty)
                    return i;

            return -1;
        }

        private bool IsValidIndex(int index) => index >= 0 && index < Slots.Length;
    }
}
