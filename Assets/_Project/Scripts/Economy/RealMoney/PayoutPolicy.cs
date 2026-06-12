using UnityEngine;

namespace Simcity.Economy.RealMoney
{
    /// <summary>
    /// The cash-out rulebook (GAME_DESIGN §6.3). These are the compliance dials the real
    /// product would tune with its payments provider and legal counsel. Real values, real
    /// fees, real holding periods, KYC/AML and tax reporting are all the PROVIDER's job
    /// (Stripe Connect / Tilia / PayPal) — these constants just model the gates so the
    /// client can preview and explain them.
    /// </summary>
    public static class PayoutPolicy
    {
        /// <summary>Minimum sale-earned Coins before a payout can be requested.</summary>
        public const int MinPayoutCoins = 1000;

        /// <summary>Conversion: how many real US cents 100 Coins is worth (gross).</summary>
        public const int UsdCentsPer100Coins = 100; // 100 Coins = $1.00

        /// <summary>Platform fee taken from the gross payout = core revenue.</summary>
        public const float PlatformFeePercent = 0.10f;

        /// <summary>In-game days sale earnings must settle before they're withdrawable
        /// (anti-fraud / chargeback window). Tune low for testing.</summary>
        public const int HoldingPeriodDays = 2;

        /// <summary>Cash-out is age-gated; no minors (provider verifies age at KYC).</summary>
        public const int MinAge = 18;

        public static int GrossUsdCents(int coins) =>
            Mathf.RoundToInt(coins / 100f * UsdCentsPer100Coins);

        public static int FeeUsdCents(int grossUsdCents) =>
            Mathf.RoundToInt(grossUsdCents * PlatformFeePercent);
    }
}
