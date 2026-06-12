using UnityEngine;
using Simcity.Core;
using Simcity.Stats;
using Simcity.Economy;

namespace Simcity.Interaction
{
    /// <summary>
    /// A gatherable resource node (tree, rock, bush, clay pit). Press E to harvest a raw
    /// material into your inventory for a little time + energy. Nodes deplete and regrow the
    /// next in-game day, so the world has a light renewable-resource rhythm rather than infinite
    /// free materials.
    /// </summary>
    public class ResourceInteractable : Interactable
    {
        public string materialId = "Wood";
        public int amountPerGather = 1;
        public int capacity = 5;
        public float gatherHours = 0.5f;
        public float energyCost = 3f;

        private int _remaining;

        private void Awake() => _remaining = capacity;

        private void OnEnable()
        {
            if (GameClock.Instance != null) GameClock.Instance.OnNewDay += Regrow;
        }

        private void OnDisable()
        {
            if (GameClock.Instance != null) GameClock.Instance.OnNewDay -= Regrow;
        }

        private void Regrow(int day) => _remaining = capacity;

        public override string Prompt =>
            _remaining > 0 ? $"Gather {materialId}  ({_remaining} left)" : $"{materialId} depleted — regrows tomorrow";

        public override void Interact(GameObject interactor)
        {
            if (_remaining <= 0)
            {
                Debug.Log($"[Gather] {name} is depleted — it regrows tomorrow.");
                return;
            }
            var inv = interactor.GetComponent<Inventory>();
            if (inv == null) return;

            GameClock.Instance?.AdvanceMinutes(gatherHours * 60f);
            interactor.GetComponent<CharacterNeeds>()?.Replenish(NeedType.Energy, -energyCost);

            int got = Mathf.Min(amountPerGather, _remaining);
            inv.Add(materialId, got);
            _remaining -= got;
            Debug.Log($"[Gather] +{got} {materialId}. ({_remaining} left at {name}.)");
        }
    }
}
