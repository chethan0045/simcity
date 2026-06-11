using UnityEngine;
using Simcity.Stats;
using Simcity.Economy;

namespace Simcity.Interaction
{
    /// <summary>Eat: restore hunger. Optionally costs Coins (buying food); set
    /// cost = 0 for a free fridge.</summary>
    public class FoodInteractable : Interactable
    {
        public float hungerRestored = 35f;
        public int cost = 6;

        public override void Interact(GameObject interactor)
        {
            var needs = interactor.GetComponent<CharacterNeeds>();
            if (needs == null) return;

            if (needs.Get(NeedType.Hunger) >= 99f)
            {
                Debug.Log("[Eat] Already full.");
                return;
            }

            if (cost > 0)
            {
                var wallet = interactor.GetComponent<Wallet>();
                if (wallet == null || !wallet.TrySpend(cost))
                {
                    Debug.Log("[Eat] Can't afford food — go earn some Coins.");
                    return;
                }
            }

            needs.Replenish(NeedType.Hunger, hungerRestored);
            Debug.Log($"[Eat] +{hungerRestored} hunger (-{cost} Coins).");
        }
    }
}
