using System;
using UnityEngine;

namespace Simcity.Economy
{
    /// <summary>
    /// The player's money. Soft currency "Coins" — but split into buckets so the Phase 7
    /// real-money layer can enforce the golden rule (GAME_DESIGN §6.3):
    ///
    ///   • cashable  — Coins EARNED BY SELLING on the marketplace. The only Coins eligible
    ///                 for real-money cash-out.
    ///   • purchased — Coins bought with real money (IAP). Spendable, NEVER cashable.
    ///   • granted   — starting Origin grant, job wages, gifts. Spendable, not cashable
    ///                 (only sale revenue is, by the anti-money-laundering rule).
    ///
    /// The public total (<see cref="Coins"/>) and the Add/TrySpend/OnChanged API are
    /// unchanged, so Phases 1-6 keep working. Spending consumes non-cashable Coins first,
    /// preserving the player's cashable balance (and closing a laundering path).
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private int cashableCoins = 0;   // earned via marketplace sales
        [SerializeField] private int purchasedCoins = 0;  // bought with real money (IAP)
        [SerializeField] private int grantedCoins = 50;   // Origin grant / wages / gifts

        public int Coins => cashableCoins + purchasedCoins + grantedCoins;

        /// <summary>Real-money-cashable balance (sale earnings only).</summary>
        public int CashableCoins => cashableCoins;
        /// <summary>Coins bought with real money — spendable but not cashable.</summary>
        public int PurchasedCoins => purchasedCoins;

        public event Action<int> OnChanged;
        /// <summary>Fires when cashable (sale) earnings are added — used to start the
        /// payout holding-period clock.</summary>
        public event Action<int> OnSaleEarnings;

        /// <summary>Legacy add — non-cashable Coins (wages, gifts, Origin grant).</summary>
        public void Add(int amount)
        {
            grantedCoins += amount;
            OnChanged?.Invoke(Coins);
        }

        /// <summary>Marketplace sale revenue — the only Coins that become cashable.</summary>
        public void AddSaleEarnings(int amount)
        {
            if (amount <= 0) return;
            cashableCoins += amount;
            OnChanged?.Invoke(Coins);
            OnSaleEarnings?.Invoke(amount);
        }

        /// <summary>Coins bought with real money (Phase 7 store). Never cashable.</summary>
        public void AddPurchased(int amount)
        {
            if (amount <= 0) return;
            purchasedCoins += amount;
            OnChanged?.Invoke(Coins);
        }

        /// <summary>Spend Coins. Consumes non-cashable buckets first (purchased, then
        /// granted) so the player's cashable balance is preserved as long as possible.</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > Coins) return false;

            int remaining = amount;
            int fromPurchased = Mathf.Min(purchasedCoins, remaining);
            purchasedCoins -= fromPurchased; remaining -= fromPurchased;

            int fromGranted = Mathf.Min(grantedCoins, remaining);
            grantedCoins -= fromGranted; remaining -= fromGranted;

            cashableCoins -= remaining; // whatever's left comes from cashable

            OnChanged?.Invoke(Coins);
            return true;
        }

        /// <summary>Reserve (remove) cashable Coins for a cash-out. Enforces the golden
        /// rule: only sale-earned Coins can be withdrawn. Returns false if insufficient.</summary>
        public bool TryReserveCashable(int amount)
        {
            if (amount <= 0 || amount > cashableCoins) return false;
            cashableCoins -= amount;
            OnChanged?.Invoke(Coins);
            return true;
        }

        /// <summary>Return reserved cashable Coins if a payout fails downstream.</summary>
        public void RefundCashable(int amount)
        {
            if (amount <= 0) return;
            cashableCoins += amount;
            OnChanged?.Invoke(Coins);
        }
    }
}
