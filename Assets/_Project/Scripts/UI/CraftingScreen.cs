using System;
using System.Text;
using UnityEngine;
using Simcity.Core;
using Simcity.Stats;
using Simcity.Economy;
using Simcity.Player;

namespace Simcity.UI
{
    /// <summary>
    /// The Workbench crafting menu: pick a recipe whose materials you have, spend the time +
    /// energy, get a finished good. A modal OnGUI overlay (freezes the player, frees the cursor)
    /// consistent with the rest of the greybox UI. Recipes whose inputs you're short on are
    /// shown but disabled, with the have/need counts spelled out.
    /// </summary>
    public class CraftingScreen : MonoBehaviour
    {
        private GameObject _interactor;
        private Inventory _inv;
        private CursorLockMode _prevLock;
        private bool _prevVisible;
        private GUIStyle _label, _title, _ok, _warn;

        public static void Open(GameObject interactor)
        {
            if (FindObjectOfType<CraftingScreen>() != null) return;
            var inv = interactor.GetComponent<Inventory>();
            if (inv == null) { Debug.Log("[Craft] No inventory to craft into."); return; }

            var go = new GameObject("CraftingScreen");
            var s = go.AddComponent<CraftingScreen>();
            s._interactor = interactor;
            s._inv = inv;
        }

        private void OnEnable()
        {
            _prevLock = Cursor.lockState;
            _prevVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = false;
        }

        private void Close()
        {
            Cursor.lockState = _prevLock;
            Cursor.visible = _prevVisible;
            if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = true;
            Destroy(gameObject);
        }

        private int Have(string id) { _inv.Items.TryGetValue(id, out var c); return c; }

        private bool CanCraft(ItemCatalog.Recipe r)
        {
            foreach (var input in r.inputs) if (Have(input.id) < input.count) return false;
            return true;
        }

        private void DoCraft(ItemCatalog.Recipe r)
        {
            if (!CanCraft(r)) return;
            foreach (var input in r.inputs) _inv.TryRemove(input.id, input.count);

            GameClock.Instance?.AdvanceMinutes(r.craftHours * 60f);
            _interactor.GetComponent<CharacterNeeds>()?.Replenish(NeedType.Energy, -r.energyCost);
            _inv.Add(r.outputId, 1);
            Debug.Log($"[Craft] Made a {r.outputId} (worth ~{r.OutputValue}). Sell it at the Market.");
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = 540f, h = 410f;
            var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 18, panel.y + 14, w - 36, h - 28));

            GUILayout.Label("Workbench — Crafting", _title);

            var sb = new StringBuilder("On hand: ");
            foreach (var m in ItemCatalog.Materials) sb.Append($"{m.id} {Have(m.id)}    ");
            GUILayout.Label(sb.ToString(), _label);
            GUILayout.Space(8);

            foreach (var r in ItemCatalog.Recipes)
            {
                bool can = CanCraft(r);
                string inputs = string.Join(", ", Array.ConvertAll(r.inputs, i => $"{i.id}×{i.count} (have {Have(i.id)})"));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{r.outputId}  (~{r.OutputValue}c)  ←  {inputs}", can ? _ok : _warn, GUILayout.Width(w - 150));
                GUI.enabled = can;
                if (GUILayout.Button("Craft", GUILayout.Width(92), GUILayout.Height(24))) DoCraft(r);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Carrying {_inv.Total} items. Gather materials from trees/rocks/bushes; sell goods at the Market.", _label);
            if (GUILayout.Button("Close", GUILayout.Height(28))) Close();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            _title = new GUIStyle(GUI.skin.label)
            { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _ok = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(0.6f, 1f, 0.6f) } };
            _warn = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(1f, 0.7f, 0.4f) } };
        }
    }
}
