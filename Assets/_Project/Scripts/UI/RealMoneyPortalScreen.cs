using UnityEngine;
using Simcity.Player;
using Simcity.Economy;
using Simcity.Economy.RealMoney;

namespace Simcity.UI
{
    /// <summary>
    /// The sandbox "web portal": buy Coins, verify identity (KYC), and cash out sale-earned
    /// Coins. A modal OnGUI overlay (no Canvas setup, matching the rest of the greybox UI)
    /// that freezes the player while open. Loudly labeled SANDBOX — it charges nothing and
    /// pays out nothing; it demonstrates the Phase 7 flows and the compliance gates.
    /// </summary>
    public class RealMoneyPortalScreen : MonoBehaviour
    {
        private RealMoneyService _svc;
        private Wallet _wallet;
        private string _cashOutText = "1000";
        private CursorLockMode _prevLock;
        private bool _prevVisible;
        private GUIStyle _label, _title, _warn;

        public static void Open(RealMoneyService svc, Wallet wallet)
        {
            if (FindObjectOfType<RealMoneyPortalScreen>() != null) return; // already open
            var go = new GameObject("RealMoneyPortal");
            var s = go.AddComponent<RealMoneyPortalScreen>();
            s._svc = svc;
            s._wallet = wallet;
        }

        private void OnEnable()
        {
            _prevLock = Cursor.lockState;
            _prevVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = false; // freeze player
        }

        private void Close()
        {
            Cursor.lockState = _prevLock;
            Cursor.visible = _prevVisible;
            if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = true;
            Destroy(gameObject);
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = 460f, h = 470f;
            var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 18, panel.y + 14, w - 36, h - 28));

            GUILayout.Label("Bank / Web Portal", _title);
            GUILayout.Label("SANDBOX — no real money is charged or paid out.", _warn);
            GUILayout.Space(6);

            // Balances
            GUILayout.Label($"Total: {_wallet.Coins} Coins", _label);
            GUILayout.Label($"  • Cashable (earned by selling): {_wallet.CashableCoins}", _label);
            GUILayout.Label($"  • Purchased (not cashable): {_wallet.PurchasedCoins}", _label);
            GUILayout.Space(8);

            // Buy Coins
            GUILayout.Label("Buy Coins (purchased — spendable, never cashable):", _label);
            GUILayout.BeginHorizontal();
            foreach (var pack in CoinPackCatalog.Packs)
                if (GUILayout.Button($"{pack.label}\n{pack.coins} / {pack.PriceLabel}", GUILayout.Height(46)))
                    _svc.BuyCoins(pack);
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            // KYC
            GUILayout.Label($"Identity (KYC): {_svc.Kyc}", _label);
            if (_svc.Kyc != KycStatus.Verified && GUILayout.Button("Verify identity (sandbox)", GUILayout.Height(28)))
                _svc.Verify();
            GUILayout.Space(8);

            // Cash out
            GUILayout.Label("Cash out earned Coins:", _label);
            GUILayout.BeginHorizontal();
            _cashOutText = GUILayout.TextField(_cashOutText, 7, GUILayout.Width(90));
            int amount = ParseAmount();
            if (GUILayout.Button("Request payout", GUILayout.Height(26))) _svc.CashOut(amount);
            GUILayout.EndHorizontal();

            string preview = _svc.ValidateCashOut(amount);
            if (preview != null) GUILayout.Label(preview, _warn);
            else GUILayout.Label($"Eligible → ≈ ${PreviewNetUsd(amount):0.00} after fee.", _label);

            GUILayout.Space(6);
            if (!string.IsNullOrEmpty(_svc.LastMessage)) GUILayout.Label(_svc.LastMessage, _label);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", GUILayout.Height(30))) Close();

            GUILayout.EndArea();
        }

        private int ParseAmount() => int.TryParse(_cashOutText, out int v) ? v : 0;

        private static float PreviewNetUsd(int coins)
        {
            int gross = PayoutPolicy.GrossUsdCents(coins);
            return (gross - PayoutPolicy.FeeUsdCents(gross)) / 100f;
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white }, wordWrap = true };
            _title = new GUIStyle(GUI.skin.label)
            { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _warn = new GUIStyle(GUI.skin.label)
            { fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.75f, 0.3f) }, wordWrap = true };
        }
    }
}
