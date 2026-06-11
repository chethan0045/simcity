using System.Threading.Tasks;
using UnityEngine;

namespace Simcity.Economy.RealMoney
{
    /// <summary>
    /// SANDBOX payments provider — moves NO real money, collects NO PII, charges NO cards.
    /// It exists so the Phase 7 client flows (buy Coins, verify, cash out) are end-to-end
    /// testable without a backend or a live provider account. Every call logs "[SANDBOX]"
    /// and returns success synchronously. Replace with a real provider integration (a
    /// backend that talks to Stripe Connect / Tilia) before anything touches real money.
    /// </summary>
    public class MockPaymentsProvider : IPaymentsProvider
    {
        public KycStatus Kyc { get; private set; } = KycStatus.None;

        public Task<bool> InitializeAsync()
        {
            Debug.Log("[SANDBOX pay] provider initialized (no real provider connected).");
            return Task.FromResult(true);
        }

        public Task<KycStatus> BeginVerificationAsync()
        {
            // Real flow: hand off to provider-hosted KYC/AML + age check; we never see PII.
            Kyc = KycStatus.Verified;
            Debug.Log("[SANDBOX pay] KYC auto-approved (no real identity data collected).");
            return Task.FromResult(Kyc);
        }

        public Task<PurchaseResult> PurchaseAsync(CoinPack pack)
        {
            Debug.Log($"[SANDBOX pay] 'purchased' {pack.coins} Coins for {pack.PriceLabel} — no real charge.");
            return Task.FromResult(new PurchaseResult
            {
                ok = true,
                coins = pack.coins,
                message = $"Bought {pack.coins} Coins ({pack.PriceLabel})"
            });
        }

        public Task<PayoutResult> PayoutAsync(int coins, int grossUsdCents, int feeUsdCents)
        {
            int net = grossUsdCents - feeUsdCents;
            Debug.Log($"[SANDBOX pay] 'paid out' {coins} Coins → net {net}¢ (gross {grossUsdCents}¢, " +
                      $"fee {feeUsdCents}¢) — no real funds moved.");
            return Task.FromResult(new PayoutResult { ok = true, netUsdCents = net, message = "Payout simulated" });
        }
    }
}
