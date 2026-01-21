using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Project.Inventory;

namespace Project.UI
{
    [DisallowMultipleComponent]
    public class InventorySlotView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Button button;

        private PlayerInventory _inventory;
        private int _index;

        private Canvas _rootCanvas;
        private Image _dragGhost;

        public void Bind(PlayerInventory inventory, int index)
        {
            _inventory = inventory;
            _index = index;

            if (button == null)
                button = GetComponent<Button>();

            _rootCanvas = GetComponentInParent<Canvas>();
            Refresh();
        }

        public void Refresh()
        {
            if (_inventory == null) return;

            bool occupied;
            Sprite sprite = null;
            int amount = 0;

            if (_index == PlayerInventory.EquippedSlotIndex)
            {
                var equipped = _inventory.GetEquippedWeaponItem();
                occupied = equipped != null;
                sprite = occupied ? equipped.icon : null;
                amount = 1;
            }
            else
            {
                if (_inventory.Slots == null) return;
                if (_index < 0 || _index >= _inventory.Slots.Length) return;

                var slot = _inventory.Slots[_index];
                occupied = !slot.IsEmpty;
                sprite = occupied && slot.item != null ? slot.item.icon : null;
                amount = occupied ? slot.amount : 0;
            }

            if (button != null)
                button.interactable = occupied;

            if (!occupied)
            {
                if (icon != null)
                {
                    icon.enabled = false;
                    icon.sprite = null;
                }

                if (amountText != null)
                    amountText.text = string.Empty;

                return;
            }

            if (icon != null)
            {
                icon.enabled = true;
                icon.sprite = sprite;
            }

            // Equipped always shows no stack count; inventory shows count only if > 1
            if (amountText != null)
            {
                if (_index == PlayerInventory.EquippedSlotIndex)
                    amountText.text = string.Empty;
                else
                    amountText.text = amount > 1 ? amount.ToString() : string.Empty;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inventory == null) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;

            // If equipped slot has nothing equipped, ignore
            if (_index == PlayerInventory.EquippedSlotIndex && !_inventory.HasEquippedWeapon)
                return;

            // If normal slot is empty, ignore
            if (_index >= 0 && _index < _inventory.Slots.Length && _inventory.Slots[_index].IsEmpty)
                return;

            _inventory.Select(_index);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inventory == null) return;
            if (_index == PlayerInventory.EquippedSlotIndex) return;

            if (_inventory.Slots[_index].IsEmpty) return;
            if (_rootCanvas == null) return;

            _dragGhost = new GameObject("DragGhost").AddComponent<Image>();
            _dragGhost.transform.SetParent(_rootCanvas.transform, false);
            _dragGhost.raycastTarget = false;

            var slot = _inventory.Slots[_index];
            _dragGhost.sprite = slot.item != null ? slot.item.icon : null;
            _dragGhost.enabled = _dragGhost.sprite != null;
            _dragGhost.SetNativeSize();
            _dragGhost.transform.localScale = Vector3.one * 0.9f;

            UpdateDragGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragGhost == null) return;
            UpdateDragGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_dragGhost != null)
                Destroy(_dragGhost.gameObject);

            _dragGhost = null;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_inventory == null) return;
            if (_index == PlayerInventory.EquippedSlotIndex) return;

            var dragged = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponentInParent<InventorySlotView>()
                : null;

            if (dragged == null) return;
            if (dragged._inventory != _inventory) return;

            int from = dragged._index;
            int to = _index;

            if (from < 0) return; // ignore equipped drag for now
            if (from == to) return;
            if (_inventory.Slots[from].IsEmpty) return;

            if (_inventory.Slots[to].IsEmpty)
                _inventory.TryMove(from, to);
            else
                _inventory.TrySwap(from, to);
        }

        private void UpdateDragGhostPosition(PointerEventData eventData)
        {
            if (_dragGhost == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPos
            );

            _dragGhost.rectTransform.anchoredPosition = localPos;
        }
    }
}
