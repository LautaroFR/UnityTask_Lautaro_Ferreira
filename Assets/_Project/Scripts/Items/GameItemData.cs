using UnityEngine;

namespace Project.Items
{
    public enum ItemType
    {
        Consumable,
        Weapon
    }

    [CreateAssetMenu(menuName = "Project/Items/Game Item Data", fileName = "GameItemData_")]
    public class GameItemData : ScriptableObject
    {
        public string id;             
        public string displayName;     
        [TextArea] public string description;

        public ItemType type;
        public Sprite icon;           
    }
}
