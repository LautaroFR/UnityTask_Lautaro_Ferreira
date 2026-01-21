using UnityEngine;
using Project.Items;
using Project.Save;

namespace Project.Inventory
{
    [DisallowMultipleComponent]
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Min(1)] private int slotCount = 16;
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("UI (optional)")]
        [SerializeField] private GameObject inventoryUI;

        public int SlotCount => slotCount;
        public InventorySlot[] Slots { get; private set; }

        public const int EquippedSlotIndex = -2;

        public int SelectedIndex { get; private set; } = -1;
        public bool IsOpen => inventoryUI != null && inventoryUI.activeSelf;

        public string EquippedWeaponId { get; private set; }
        private GameItemData _equippedWeaponItem;

        public bool HasEquippedWeapon => !string.IsNullOrWhiteSpace(EquippedWeaponId);

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

            if (SaveService.TryLoad(out var data))
                LoadFrom(data);

            Select(-1);
        }

        public void ToggleInventoryUI()
        {
            if (inventoryUI == null) return;

            bool newState = !inventoryUI.activeSelf;
            inventoryUI.SetActive(newState);
            InventoryVisibilityChanged?.Invoke(newState);

            if (!newState)
                Select(-1);
        }

        /// <summary>
        /// Returns the equipped weapon item for UI/anim usage.
        /// Uses cached reference when possible; falls back to database after load.
        /// </summary>
        public GameItemData GetEquippedWeaponItem()
        {
            if (!HasEquippedWeapon) return null;

            // Fast path: already cached (best for UI responsiveness)
            if (_equippedWeaponItem != null && _equippedWeaponItem.id == EquippedWeaponId)
                return _equippedWeaponItem;

            // Fallback: resolve from DB (needed after Load)
            if (itemDatabase == null) return null;

            if (itemDatabase.TryGet(EquippedWeaponId, out var item))
            {
                _equippedWeaponItem = item;
                return item;
            }

            return null;
        }

        public void Select(int index)
        {
            // Allow: -1 (none), -2 (equipped), or valid inventory indices
            if (index != -1 && index != EquippedSlotIndex && (index < 0 || index >= Slots.Length))
                return;

            if (SelectedIndex == index) return;

            SelectedIndex = index;
            SelectionChanged?.Invoke(SelectedIndex);
        }

        public bool TryAdd(GameItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;

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

            Compact();

            if (SelectedIndex == index && Slots[index].IsEmpty)
                Select(-1);

            InventoryChanged?.Invoke();
            return true;
        }

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
            if (SelectedIndex == EquippedSlotIndex)
                return TryUnequipWeapon();

            if (!IsValidIndex(SelectedIndex)) return false;
            if (Slots[SelectedIndex].IsEmpty) return false;

            var item = Slots[SelectedIndex].item;
            if (item == null) return false;

            switch (item.type)
            {
                case ItemType.Consumable:
                    return TryRemoveAt(SelectedIndex, 1);

                case ItemType.Weapon:
                    return TryEquipFromSlot(SelectedIndex);

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

        private bool TryAddToFirstEmpty(GameItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;

            int empty = FindFirstEmptySlot();
            if (empty < 0) return false;

            Slots[empty].item = item;
            Slots[empty].amount = amount;
            return true;
        }

        public bool TryEquipFromSlot(int index)
        {
            if (!IsValidIndex(index)) return false;
            if (Slots[index].IsEmpty) return false;

            var item = Slots[index].item;
            if (item == null) return false;
            if (item.type != ItemType.Weapon) return false;

            // If there's a weapon already equipped, return it to inventory
            if (_equippedWeaponItem != null)
            {
                bool returned = TryAddToFirstEmpty(_equippedWeaponItem, 1);
                if (!returned) return false;
            }

            Slots[index].amount -= 1;
            if (Slots[index].amount <= 0)
                Slots[index].Clear();

            // Equip new weapon
            _equippedWeaponItem = item;
            EquippedWeaponId = item.id;

            if (SelectedIndex == index)
                Select(-1);

            InventoryChanged?.Invoke();
            EquippedWeaponChanged?.Invoke(EquippedWeaponId);
            return true;
        }

        public bool TryUnequipWeapon()
        {
            if (!HasEquippedWeapon) return false;

            var weapon = GetEquippedWeaponItem();
            if (weapon == null) return false;

            int empty = FindFirstEmptySlot();
            if (empty < 0) return false;

            Slots[empty].item = weapon;
            Slots[empty].amount = 1;

            _equippedWeaponItem = null;
            EquippedWeaponId = null;

            // If the details panel is pointing to equipped slot, clear selection
            if (SelectedIndex == EquippedSlotIndex)
                Select(-1);

            InventoryChanged?.Invoke();
            EquippedWeaponChanged?.Invoke(null);
            return true;
        }

        #region Save Data
        public PlayerSaveData ToSaveData()
        {
            var data = new PlayerSaveData
            {
                equippedWeaponId = EquippedWeaponId
            };

            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].IsEmpty) continue;

                var it = Slots[i].item;
                if (it == null || string.IsNullOrWhiteSpace(it.id)) continue;

                data.slots.Add(new InventorySlotSave
                {
                    itemId = it.id,
                    amount = Slots[i].amount
                });
            }

            return data;
        }

        public void LoadFrom(PlayerSaveData data)
        {
            if (data == null) return;

            for (int i = 0; i < Slots.Length; i++)
                Slots[i].Clear();

            EquippedWeaponId = data.equippedWeaponId;
            _equippedWeaponItem = null;

            if (itemDatabase == null)
            {
                Debug.LogError("[PlayerInventory] ItemDatabase is not assigned.", this);
                InventoryChanged?.Invoke();
                EquippedWeaponChanged?.Invoke(EquippedWeaponId);
                return;
            }

            int index = 0;
            foreach (var saved in data.slots)
            {
                if (index >= Slots.Length) break;
                if (saved == null || string.IsNullOrWhiteSpace(saved.itemId)) continue;
                if (saved.amount <= 0) continue;

                if (!itemDatabase.TryGet(saved.itemId, out var item) || item == null)
                    continue;

                Slots[index].item = item;
                Slots[index].amount = saved.amount;
                index++;
            }

            // Resolve equipped item reference (for UI/anim)
            if (HasEquippedWeapon)
                _equippedWeaponItem = GetEquippedWeaponItem();

            Select(-1);
            InventoryChanged?.Invoke();
            EquippedWeaponChanged?.Invoke(EquippedWeaponId);
        }

        public void ClearAll()
        {
            for (int i = 0; i < Slots.Length; i++)
                Slots[i].Clear();

            _equippedWeaponItem = null;
            EquippedWeaponId = null;

            Select(-1);
            InventoryChanged?.Invoke();
            EquippedWeaponChanged?.Invoke(null);
        }
        #endregion
    }
}
