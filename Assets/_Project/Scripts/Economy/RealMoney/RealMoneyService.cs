using UnityEngine;
using Simcity.Core;

namespace Simcity.Economy.RealMoney
{
    /// <summary>
    /// The real-money policy engine that sits between the game and the payments provider.
    /// It enforces the golden rule and the compliance gates from GAME_DESIGN §6.3 — KYC,
    /// age, payout threshold, holding period, and "earned-by-selling only" — then delegates
    /// the actual money movement to an <see cref="IPaymentsProvider"/> (a sandbox mock here).
    ///
    /// Sandbox caveats: balances are local (a real wallet is server/backend-authoritative);
    /// the holding-period clock is per-most-recent-sale (a real one is per payout lot);
    /// no real money, PII, or provider is involved. This is the architecture, not the bank.
    /// </summary>
    [RequireComponent(typeof(Wallet))]
    public class RealMoneyService : MonoBehaviour
    {
        public bool ageVerified;          // set by verification; real flow age-gates via provider
        public string LastMessage { get; private set; } = "";

        private IPaymentsProvider _provider;
        private Wallet _wallet;
        private int _lastSaleDay = int.MinValue;
        private bool _hasSold;

        public KycStatus Kyc => _provider != null ? _provider.Kyc : KycStatus.None;

        private void Awake()
        {
            _wallet = GetComponent<Wallet>();
            _provider = new MockPaymentsProvider(); // swap for a real backend-backed provider
            _ = _provider.InitializeAsync();
            _wallet.OnSaleEarnings += OnSaleEarnings;
        }

        private void OnDestroy()
        {
            if (_wallet != null) _wallet.OnSaleEarnings -= OnSaleEarnings;
        }

        private void OnSaleEarnings(int amount)
        {
            _hasSold = true;
            _lastSaleDay = CurrentDay();
        }

        private static int CurrentDay() => GameClock.Instance != null ? GameClock.Instance.Day : 1;

        // ---- Store: buy Coins with real money (purchased = spendable, never cashable) ----
        public async void BuyCoins(CoinPack pack)
        {
            LastMessage = $"Purchasing {pack.label}…";
            var r = await _provider.PurchaseAsync(pack);
            if (r.ok)
            {
                _wallet.AddPurchased(r.coins);
                LastMessage = $"Added {r.coins} purchased Coins (spendable, not cashable).";
            }
            else LastMessage = $"Purchase failed: {r.message}";
        }

        // ---- Identity/age verification (KYC) ----
        public async void Verify()
        {
            LastMessage = "Verifying identity…";
            await _provider.BeginVerificationAsync();
            ageVerified = true; // sandbox assumes adult; real flow gets age from provider KYC
            LastMessage = Kyc == KycStatus.Verified ? "Identity verified (sandbox)." : "Verification failed.";
        }

        // ---- Cash-out: earned Coins → real money (the regulated path) ----
        public async void CashOut(int coins)
        {
            string err = ValidateCashOut(coins);
            if (err != null)
            {
                LastMessage = err;
                Debug.Log($"[CashOut blocked] {err}");
                return;
            }

            int gross = PayoutPolicy.GrossUsdCents(coins);
            int fee = PayoutPolicy.FeeUsdCents(gross);

            if (!_wallet.TryReserveCashable(coins)) { LastMessage = "Insufficient cashable balance."; return; }

            LastMessage = "Requesting payout…";
            var r = await _provider.PayoutAsync(coins, gross, fee);
            if (r.ok)
                LastMessage = $"Paid out {coins} Coins → ${r.netUsdCents / 100f:0.00} net (sandbox).";
            else
            {
                _wallet.RefundCashable(coins); // never silently lose the player's Coins
                LastMessage = $"Payout failed: {r.message}";
            }
        }

        /// <summary>The compliance check. Returns an error string, or null if the cash-out
        /// is allowed. This is the anti-fraud / AML spine — every gate from §6.3.</summary>
        public string ValidateCashOut(int coins)
        {
            if (coins <= 0) return "Enter an amount to cash out.";
            if (Kyc != KycStatus.Verified) return "Cash-out requires identity verification (KYC).";
            if (!ageVerified) return $"Cash-out is age-gated ({PayoutPolicy.MinAge}+).";
            if (coins < PayoutPolicy.MinPayoutCoins)
                return $"Below the payout threshold ({PayoutPolicy.MinPayoutCoins} Coins).";
            if (_wallet == null || coins > _wallet.CashableCoins)
                return "Golden rule: you can only cash out Coins earned by selling.";
            int held = CurrentDay() - _lastSaleDay;
            if (!_hasSold || held < PayoutPolicy.HoldingPeriodDays)
                return $"Earnings must settle {PayoutPolicy.HoldingPeriodDays} days before payout.";
            return null;
        }
    }
}
