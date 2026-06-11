using UnityEngine;
using Simcity.Core;
using Simcity.Stats;
using Simcity.Economy;

namespace Simcity.Interaction
{
    /// <summary>
    /// Work a shift: advance time, spend energy, earn Coins. The heart of the
    /// Nobody-style earn -> spend loop. Too tired? Sleep first.
    /// </summary>
    public class WorkInteractable : Interactable
    {
        public float shiftHours = 4f;
        public float energyCostPerHour = 8f;
        public int payPerHour = 10;
        public float minEnergyToWork = 15f;

        public override void Interact(GameObject interactor)
        {
            var clock = GameClock.Instance;
            if (clock == null) return;

            var needs = interactor.GetComponent<CharacterNeeds>();
            if (needs != null && needs.Get(NeedType.Energy) < minEnergyToWork)
            {
                Debug.Log("[Work] Too tired to work — get some sleep first.");
                return;
            }

            clock.AdvanceMinutes(shiftHours * 60f); // decays needs across the shift
            if (needs != null)
                needs.Replenish(NeedType.Energy, -energyCostPerHour * shiftHours);

            int pay = Mathf.RoundToInt(payPerHour * shiftHours);
            interactor.GetComponent<Wallet>()?.Add(pay);

            Debug.Log($"[Work] Worked {shiftHours}h, earned {pay} Coins -> {clock.TimeString()}.");
        }
    }
}
