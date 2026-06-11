using UnityEngine;
using Simcity.Core;
using Simcity.Stats;

namespace Simcity.Interaction
{
    /// <summary>Sleep: skip time to morning and restore energy over the hours slept.</summary>
    public class BedInteractable : Interactable
    {
        public float wakeHour = 7f;
        public float energyRestoredPerHour = 12f;

        public override void Interact(GameObject interactor)
        {
            var clock = GameClock.Instance;
            if (clock == null) return;

            float minutes = clock.SkipToHour(wakeHour); // also decays needs over those hours
            float hours = minutes / 60f;

            var needs = interactor.GetComponent<CharacterNeeds>();
            if (needs != null)
                needs.Replenish(NeedType.Energy, energyRestoredPerHour * hours);

            Debug.Log($"[Sleep] Slept {hours:0.0}h -> {clock.TimeString()}. You wake up hungrier.");
        }
    }
}
