using System;
using UnityEngine;
using Simcity.Stats;

namespace Simcity.UI
{
    /// <summary>
    /// First-run character creator (OnGUI — zero scene setup). Edits a live preview
    /// avatar and, on "Start Life", invokes OnComplete with the chosen config.
    /// A real UGUI screen replaces this in Phase 8.
    /// </summary>
    public class CharacterCreationScreen : MonoBehaviour
    {
        public CharacterAppearance preview;
        public Transform previewPivot;   // rotated for a turntable view
        public AppearanceConfig config = AppearanceConfig.CreateDefault();
        public Action<AppearanceConfig> OnComplete;

        private readonly System.Random _rng = new System.Random(12345);
        private GUIStyle _header, _label;

        private void Update()
        {
            if (preview != null) preview.Apply(config);          // live preview
            if (previewPivot != null) previewPivot.Rotate(0f, 25f * Time.deltaTime, 0f);
        }

        private void OnGUI()
        {
            EnsureStyles();

            GUILayout.BeginArea(new Rect(20, 20, 360, Screen.height - 40), GUI.skin.box);
            GUILayout.Label("Create your character", _header);

            GUILayout.Space(6);
            GUILayout.Label("Name", _label);
            config.characterName = GUILayout.TextField(config.characterName ?? "", 24);

            GUILayout.Space(6);
            GUILayout.Label("Gender (cosmetic/social — no gameplay lockouts)", _label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(config.gender == Gender.Female, "Female", "Button")) config.gender = Gender.Female;
            if (GUILayout.Toggle(config.gender == Gender.Male, "Male", "Button")) config.gender = Gender.Male;
            if (GUILayout.Toggle(config.gender == Gender.Neutral, "Neutral", "Button")) config.gender = Gender.Neutral;
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label($"Height: {config.HeightMeters:0.00} m", _label);
            config.height = GUILayout.HorizontalSlider(config.height, 0f, 1f);
            GUILayout.Label("Build (slim ↔ broad)", _label);
            config.build = GUILayout.HorizontalSlider(config.build, 0f, 1f);

            Swatches("Skin", AppearanceConfig.SkinSwatches, ref config.skin);
            Swatches("Hair", AppearanceConfig.HairSwatches, ref config.hair);
            ColorSliders("Shirt", ref config.shirt);
            ColorSliders("Pants", ref config.pants);

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Randomize")) config.Randomize(_rng);
            if (GUILayout.Button("Start Life ▶")) OnComplete?.Invoke(config);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void Swatches(string label, Color[] options, ref Color value)
        {
            GUILayout.Space(6);
            GUILayout.Label(label, _label);
            GUILayout.BeginHorizontal();
            foreach (var c in options)
            {
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = c;
                if (GUILayout.Button(" ", GUILayout.Width(36), GUILayout.Height(24))) value = c;
                GUI.backgroundColor = prev;
            }
            GUILayout.EndHorizontal();
        }

        private void ColorSliders(string label, ref Color value)
        {
            GUILayout.Space(4);
            GUILayout.Label(label, _label);
            value.r = Channel("R", value.r);
            value.g = Channel("G", value.g);
            value.b = Channel("B", value.b);
        }

        private float Channel(string c, float v)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(c, GUILayout.Width(16));
            v = GUILayout.HorizontalSlider(v, 0f, 1f);
            GUILayout.EndHorizontal();
            return v;
        }

        private void EnsureStyles()
        {
            if (_header != null) return;
            _header = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _label = new GUIStyle(GUI.skin.label)
                { fontSize = 13, normal = { textColor = Color.white } };
        }
    }
}
