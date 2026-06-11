using System.Collections.Generic;
using UnityEngine;
using Simcity.Economy;

namespace Simcity.Interaction
{
    /// <summary>
    /// Market stall: sell all carried goods for Coins, minus a market fee, scaled by
    /// seller reputation. This is the soft-currency marketplace; in co-op (Phase 6)
    /// it becomes player-to-player, and in Phase 7 it gains real-money cash-out.
    /// </summary>
    public class MarketInteractable : Interactable
    {
        [Range(0f, 0.3f)] public float marketFee = 0.1f;

        public override void Interact(GameObject interactor)
        {
            var inv = interactor.GetComponent<Inventory>();
            var wallet = interactor.GetComponent<Wallet>();
            if (inv == null || wallet == null) return;

            // Only finished GOODS sell here — raw materials stay as crafting inputs.
            var goods = new List<KeyValuePair<string, int>>();
            foreach (var kv in inv.Items)
                if (ItemCatalog.IsGood(kv.Key)) goods.Add(kv);

            if (goods.Count == 0)
            {
                Debug.Log("[Market] No finished goods to sell — craft some at the Workbench first.");
                return;
            }

            var seller = interactor.GetComponent<SellerProfile>();
            float multiplier = seller != null ? seller.PriceMultiplier : 1f;

            int gross = 0;
            foreach (var kv in goods)
                gross += Mathf.RoundToInt(ItemCatalog.Value(kv.Key) * kv.Value * multiplier);

            int fee = Mathf.RoundToInt(gross * marketFee);
            int net = gross - fee;

            wallet.AddSaleEarnings(net); // sale revenue is the only real-money-cashable Coins (Phase 7)
            seller?.RecordSale(net);
            foreach (var kv in goods) inv.TryRemove(kv.Key, kv.Value);

            Debug.Log($"[Market] Sold goods for {net} Coins (gross {gross}, fee {fee}). " +
                      $"Reputation now {(seller != null ? seller.reputation : 0f):0}.");
        }
    }
}
