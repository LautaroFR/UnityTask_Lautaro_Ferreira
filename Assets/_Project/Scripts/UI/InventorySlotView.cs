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
            if (_inventory == null || _inventory.Slots == null) return;
            if (_index < 0 || _index >= _inventory.Slots.Length) return;

            var slot = _inventory.Slots[_index];
            bool occupied = !slot.IsEmpty;

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
                icon.sprite = slot.item != null ? slot.item.icon : null;
            }

            if (amountText != null)
                amountText.text = slot.amount > 1 ? slot.amount.ToString() : string.Empty;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inventory == null) return;

            // If empty, ignore (even if someone forced interactable)
            if (_inventory.Slots[_index].IsEmpty) return;

            _inventory.Select(_index);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inventory == null) return;
            if (_inventory.Slots[_index].IsEmpty) return;
            if (_rootCanvas == null) return;

            // Ghost icon
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

            var dragged = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponentInParent<InventorySlotView>()
                : null;

            if (dragged == null) return;
            if (dragged._inventory != _inventory) return;

            int from = dragged._index;
            int to = _index;

            if (from == to) return;
            if (_inventory.Slots[from].IsEmpty) return;

            // Empty target -> MOVE (no shifting)
            if (_inventory.Slots[to].IsEmpty)
            {
                _inventory.TryMove(from, to);
                return;
            }

            // Occupied target -> SWAP
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
