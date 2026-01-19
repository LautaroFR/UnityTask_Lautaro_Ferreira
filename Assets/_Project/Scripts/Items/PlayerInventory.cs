using System.Collections.Generic;
using UnityEngine;

namespace Project.Items
{
    [DisallowMultipleComponent]
    public class PlayerInventory : MonoBehaviour
    {
        private readonly HashSet<string> _itemIds = new HashSet<string>();

        public bool Has(string itemId) => _itemIds.Contains(itemId);

        public void Add(GameItemData item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id)) return;
            _itemIds.Add(item.id);
        }
    }
}
