using System.Collections.Generic;

namespace Simcity.Economy
{
    /// <summary>
    /// The goods that can be crafted and sold, with base values. A tiny static
    /// catalog for Phase 5; later this becomes data-driven (ScriptableObjects) and
    /// expands into resources, recipes, and tiers.
    /// </summary>
    public static class ItemCatalog
    {
        public struct Item
        {
            public string id;
            public int baseValue;
            public Item(string id, int baseValue) { this.id = id; this.baseValue = baseValue; }
        }

        public static readonly Item[] Craftables =
        {
            new Item("Trinket", 12),
            new Item("Pottery", 18),
            new Item("Furniture", 30),
        };

        private static readonly Dictionary<string, int> Values = new Dictionary<string, int>();

        static ItemCatalog()
        {
            foreach (var it in Craftables) Values[it.id] = it.baseValue;
        }

        public static int Value(string id) => Values.TryGetValue(id, out var v) ? v : 0;
    }
}
