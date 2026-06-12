using System.Collections.Generic;

namespace Simcity.Economy
{
    public enum ItemKind { Material, Good }

    /// <summary>
    /// The item economy: raw <b>materials</b> you gather, finished <b>goods</b> you craft and
    /// sell, and the <b>recipes</b> that turn the former into the latter. Static for now; the
    /// natural next step is data-driven ScriptableObjects + tiers/skill gating. Goods carry the
    /// margin (output value &gt; input value), so the gather → craft → sell loop pays off.
    /// </summary>
    public static class ItemCatalog
    {
        public struct Item
        {
            public string id;
            public int baseValue;
            public ItemKind kind;
            public Item(string id, int baseValue, ItemKind kind) { this.id = id; this.baseValue = baseValue; this.kind = kind; }
        }

        public struct Recipe
        {
            public string outputId;
            public (string id, int count)[] inputs;
            public float craftHours;
            public float energyCost;

            public Recipe(string outputId, float craftHours, float energyCost, params (string id, int count)[] inputs)
            {
                this.outputId = outputId;
                this.craftHours = craftHours;
                this.energyCost = energyCost;
                this.inputs = inputs;
            }

            public int OutputValue => Value(outputId);
        }

        public static readonly Item[] Materials =
        {
            new Item("Wood",  3, ItemKind.Material),
            new Item("Stone", 4, ItemKind.Material),
            new Item("Fiber", 2, ItemKind.Material),
            new Item("Clay",  3, ItemKind.Material),
        };

        public static readonly Item[] Craftables =
        {
            new Item("Basket",    16, ItemKind.Good),
            new Item("Trinket",   12, ItemKind.Good),
            new Item("Pottery",   18, ItemKind.Good),
            new Item("Furniture", 30, ItemKind.Good),
            new Item("Statue",    45, ItemKind.Good),
        };

        public static readonly Recipe[] Recipes =
        {
            new Recipe("Basket",    1.0f, 4f, ("Fiber", 4)),
            new Recipe("Trinket",   1.5f, 5f, ("Fiber", 2), ("Wood", 1)),
            new Recipe("Pottery",   2.0f, 6f, ("Clay", 3)),
            new Recipe("Furniture", 3.0f, 9f, ("Wood", 4), ("Stone", 2)),
            new Recipe("Statue",    3.0f, 10f, ("Stone", 3), ("Clay", 2)),
        };

        private static readonly Dictionary<string, Item> All = new Dictionary<string, Item>();

        static ItemCatalog()
        {
            foreach (var it in Materials) All[it.id] = it;
            foreach (var it in Craftables) All[it.id] = it;
        }

        public static int Value(string id) => All.TryGetValue(id, out var it) ? it.baseValue : 0;
        public static ItemKind Kind(string id) => All.TryGetValue(id, out var it) ? it.kind : ItemKind.Good;
        public static bool IsGood(string id) => All.TryGetValue(id, out var it) && it.kind == ItemKind.Good;
        public static bool IsMaterial(string id) => All.TryGetValue(id, out var it) && it.kind == ItemKind.Material;
    }
}
