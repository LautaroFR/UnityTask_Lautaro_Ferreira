using System.Collections.Generic;
using UnityEngine;
using Project.Items;

namespace Project.Items
{
    [CreateAssetMenu(menuName = "Project/Items/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<GameItemData> items = new();

        private Dictionary<string, GameItemData> _map;

        public bool TryGet(string id, out GameItemData item)
        {
            if (_map == null)
            {
                _map = new Dictionary<string, GameItemData>();
                foreach (var it in items)
                {
                    if (it == null || string.IsNullOrWhiteSpace(it.id)) continue;
                    _map[it.id] = it;
                }
            }

            return _map.TryGetValue(id, out item);
        }
    }
}
