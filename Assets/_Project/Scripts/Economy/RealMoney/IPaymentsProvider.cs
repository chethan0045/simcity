using System.Threading.Tasks;

namespace Simcity.Economy.RealMoney
{
    public enum KycStatus { None, Pending, Verified, Rejected }

    public struct PurchaseResult
    {
        public bool ok;
        public int coins;
        public string message;
    }

    public struct PayoutResult
    {
        public bool ok;
        public int netUsdCents;
        public string message;
    }

    /// <summary>
    /// The seam to a real payments/marketplace provider — Stripe Connect, PayPal, or Tilia
    /// (GAME_DESIGN §6.5). The provider, NOT this game, owns KYC/AML, age verification, tax
    /// reporting (e.g. 1099-K), chargebacks, and the payout rails. We never collect or store
    /// card data or PII on-device. In production, buying Coins and cashing out happen on the
    /// WEB (app-store rules), with the client only kicking off these flows.
    ///
    /// The only implementation in the repo is <see cref="MockPaymentsProvider"/> — a sandbox
    /// stub that moves no real money. Swapping in a real provider is a backend integration.
    /// </summary>
    public interface IPaymentsProvider
    {
        KycStatus Kyc { get; }

        Task<bool> InitializeAsync();

        /// <summary>Kick off provider-hosted identity/age verification (KYC/AML). PII is
        /// collected by the provider, off-device.</summary>
        Task<KycStatus> BeginVerificationAsync();

        /// <summary>Buy a Coin pack with real money (web/IAP via the provider).</summary>
        Task<PurchaseResult> PurchaseAsync(CoinPack pack);

        /// <summary>Pay out sale-earned Coins to a verified seller's payout account.</summary>
        Task<PayoutResult> PayoutAsync(int coins, int grossUsdCents, int feeUsdCents);
    }
}
