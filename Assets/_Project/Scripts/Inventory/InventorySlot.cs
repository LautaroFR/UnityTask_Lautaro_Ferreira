using System;
using Project.Items;

namespace Project.Inventory
{
    [Serializable]
    public struct InventorySlot
    {
        public GameItemData item;
        public int amount;

        public bool IsEmpty => item == null || amount <= 0;

        public void Clear()
        {
            item = null;
            amount = 0;
        }
    }
}
