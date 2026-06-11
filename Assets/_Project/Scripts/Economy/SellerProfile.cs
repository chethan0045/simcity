using UnityEngine;

namespace Simcity.Economy
{
    /// <summary>
    /// A seller's standing. Reputation (0..100) grows with sales and raises prices —
    /// the soft-currency precursor to the real marketplace reputation in Phase 5/6
    /// (and the verified-seller system in Phase 7).
    /// </summary>
    public class SellerProfile : MonoBehaviour
    {
        [Range(0f, 100f)] public float reputation;
        public int totalEarned;

        /// <summary>Up to +50% price at max reputation.</summary>
        public float PriceMultiplier => 1f + Mathf.Clamp01(reputation / 100f) * 0.5f;

        public void RecordSale(int coins)
        {
            totalEarned += coins;
            reputation = Mathf.Min(100f, reputation + 2f);
        }
    }
}
