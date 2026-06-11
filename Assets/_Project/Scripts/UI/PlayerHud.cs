using UnityEngine;
using Simcity.Core;
using Simcity.Stats;
using Simcity.Economy;
using Simcity.World;
using Simcity.Social;
using System.Text;

namespace Simcity.UI
{
    /// <summary>
    /// Minimal OnGUI HUD: need bars, Coins, day/time, rent debt. Zero scene setup,
    /// consistent with Phase 0. Replaced by a real UGUI/Canvas HUD in Phase 8.
    /// </summary>
    public class PlayerHud : MonoBehaviour
    {
        public CharacterNeeds needs;
        public Wallet wallet;
        public Housing housing;
        public Relationships relationships;
        public Inventory inventory;
        public SellerProfile seller;

        private GUIStyle _style;
        private static Texture2D _white;

        private void OnGUI()
        {
            EnsureResources();

            // Clock + money + debt (top-right)
            string clock = GameClock.Instance != null ? GameClock.Instance.TimeString() : "--";
            string money = wallet != null ? $"{wallet.Coins} Coins" : "";
            string debt = (housing != null && housing.Debt > 0) ? $"   ⚠ rent debt {housing.Debt}" : "";
            GUI.Label(new Rect(Screen.width - 340, 10, 330, 24), $"{clock}     {money}{debt}", _style);

            // Goods + reputation (second top-right line)
            if (inventory != null || seller != null)
            {
                var sb = new StringBuilder();
                if (inventory != null)
                {
                    sb.Append($"Goods: {inventory.Total}");
                    foreach (var kv in inventory.Items) sb.Append($"  {kv.Key}×{kv.Value}");
                }
                if (seller != null) sb.Append($"   Rep: {seller.reputation:0}");
                GUI.Label(new Rect(Screen.width - 460, 32, 450, 22), sb.ToString(), _style);
            }

            // Need bars (top-left, below PlayerInteractor's controls hint)
            float y = 40f;
            if (needs != null)
            {
                DrawBar("Hunger", needs.Get(NeedType.Hunger), new Color(0.9f, 0.6f, 0.2f), ref y);
                DrawBar("Energy", needs.Get(NeedType.Energy), new Color(0.3f, 0.6f, 0.95f), ref y);
            }

            DrawRelationships();
        }

        private void DrawRelationships()
        {
            if (relationships == null) return;

            float y = 96f;
            GUI.Label(new Rect(12, y, 320, 20), "Relationships:", _style);
            y += 20f;

            foreach (var kv in relationships.All)
            {
                if (!Character.Registry.TryGetValue(kv.Key, out var other) || other == null) continue;
                string tier = RelationshipTier.Name(kv.Value.value, kv.Value.partner);
                GUI.Label(new Rect(18, y, 320, 18), $"{other.displayName} — {tier} ({kv.Value.value:0})", _style);
                y += 18f;
            }
        }

        private void DrawBar(string label, float value, Color color, ref float y)
        {
            const float w = 180f, h = 16f, x = 12f;
            GUI.Label(new Rect(x, y - 1, 70, h + 4), label, _style);

            var back = new Rect(x + 64, y, w, h);
            DrawRect(back, new Color(0f, 0f, 0f, 0.55f));

            var fill = new Rect(back.x, back.y, w * Mathf.Clamp01(value / 100f), h);
            DrawRect(fill, value < 25f ? new Color(0.85f, 0.25f, 0.25f) : color); // warn when low

            y += h + 8f;
        }

        private static void DrawRect(Rect r, Color c)
        {
            var prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = prev;
        }

        private void EnsureResources()
        {
            if (_style == null)
                _style = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };

            if (_white == null)
            {
                _white = new Texture2D(1, 1);
                _white.SetPixel(0, 0, Color.white);
                _white.Apply();
            }
        }
    }
}
