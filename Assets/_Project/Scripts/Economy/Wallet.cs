using System;
using UnityEngine;

namespace Simcity.Economy
{
    /// <summary>
    /// The player's money. Phase 1 uses a single soft currency, "Coins".
    /// In Phase 5+ this splits into earned vs. purchased Coins for the real-money
    /// economy (only earned Coins are cashable — see GAME_DESIGN.md §6.3).
    /// </summary>
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private int coins = 50;

        public int Coins => coins;
        public event Action<int> OnChanged;

        public void Add(int amount)
        {
            coins += amount;
            OnChanged?.Invoke(coins);
        }

        public bool TrySpend(int amount)
        {
            if (amount > coins) return false;
            coins -= amount;
            OnChanged?.Invoke(coins);
            return true;
        }
    }
}
