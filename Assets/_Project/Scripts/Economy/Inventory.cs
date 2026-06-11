using System.Collections.Generic;
using UnityEngine;

namespace Simcity.Economy
{
    /// <summary>A character's held goods (item id → quantity). Phase 5 only the
    /// player uses it (craft → carry → sell); NPCs gain inventories later.</summary>
    public class Inventory : MonoBehaviour
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int> Items => _items;

        public int Total
        {
            get { int t = 0; foreach (var kv in _items) t += kv.Value; return t; }
        }

        public void Add(string id, int count = 1)
        {
            _items.TryGetValue(id, out var c);
            _items[id] = c + count;
        }

        public bool TryRemove(string id, int count = 1)
        {
            if (!_items.TryGetValue(id, out var c) || c < count) return false;
            _items[id] = c - count;
            if (_items[id] <= 0) _items.Remove(id);
            return true;
        }

        public void Clear() => _items.Clear();
    }
}
