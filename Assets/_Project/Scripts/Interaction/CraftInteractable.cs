using UnityEngine;
using Simcity.Core;
using Simcity.Stats;
using Simcity.Economy;

namespace Simcity.Interaction
{
    /// <summary>Workbench: spend time + a little energy to craft a sellable good
    /// into your inventory. Sell it at the Market for Coins.</summary>
    public class CraftInteractable : Interactable
    {
        public float craftHours = 2f;
        public float energyCostPerHour = 4f;

        public override void Interact(GameObject interactor)
        {
            var inv = interactor.GetComponent<Inventory>();
            if (inv == null) return;

            GameClock.Instance?.AdvanceMinutes(craftHours * 60f);
            interactor.GetComponent<CharacterNeeds>()?.Replenish(NeedType.Energy, -energyCostPerHour * craftHours);

            var item = ItemCatalog.Craftables[Random.Range(0, ItemCatalog.Craftables.Length)];
            inv.Add(item.id, 1);

            Debug.Log($"[Craft] Made a {item.id} (worth ~{item.baseValue}). Carry it to the Market to sell.");
        }
    }
}
