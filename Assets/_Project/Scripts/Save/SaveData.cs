using System;
using System.Collections.Generic;

namespace Project.Save
{
    [Serializable]
    public class InventorySlotSave
    {
        public string itemId;
        public int amount;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public List<InventorySlotSave> slots = new List<InventorySlotSave>();
        public string equippedWeaponId;
    }
}
