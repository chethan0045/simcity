using UnityEngine;
using Simcity.Core;
using Simcity.Economy;

namespace Simcity.World
{
    /// <summary>
    /// Charges daily rent at the start of each new day. If the player can't pay,
    /// debt accrues — the Nobody-style cost-of-living pressure that gives earning
    /// stakes. Soft consequence (debt), not a hard fail (see GAME_DESIGN.md §6.1b).
    /// </summary>
    [RequireComponent(typeof(Wallet))]
    public class Housing : MonoBehaviour
    {
        public int dailyRent = 20;
        public int Debt { get; private set; }

        private Wallet _wallet;

        private void Awake() => _wallet = GetComponent<Wallet>();

        private void Start()
        {
            if (GameClock.Instance != null)
                GameClock.Instance.OnNewDay += ChargeRent;
        }

        private void OnDestroy()
        {
            if (GameClock.Instance != null)
                GameClock.Instance.OnNewDay -= ChargeRent;
        }

        private void ChargeRent(int day)
        {
            // First settle any back-rent we can, then today's rent.
            int owed = Debt + dailyRent;
            int paid = Mathf.Min(owed, _wallet.Coins);
            _wallet.TrySpend(paid);
            Debt = owed - paid;

            if (Debt > 0)
                Debug.Log($"[Rent] Day {day}: short on rent — debt now {Debt} Coins.");
            else
                Debug.Log($"[Rent] Day {day}: rent paid. Balance {_wallet.Coins} Coins.");
        }
    }
}
