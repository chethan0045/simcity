using System;
using UnityEngine;

namespace Simcity.Stats
{
    public enum Gender { Female, Male, Neutral }

    /// <summary>
    /// Serializable description of a character's look. This is the persisted,
    /// engine-agnostic "what the character is"; CharacterAppearance turns it into
    /// visuals. When real rigged meshes/blendshapes arrive, this data model stays —
    /// only the applier changes. Gender is cosmetic/social only (GAME_DESIGN §4.1).
    /// </summary>
    [Serializable]
    public class AppearanceConfig
    {
        public string characterName = "Newcomer";
        public Gender gender = Gender.Neutral;

        [Range(0f, 1f)] public float height = 0.5f; // 0 -> 1.55m, 1 -> 1.95m
        [Range(0f, 1f)] public float build = 0.5f;  // 0 -> slim, 1 -> broad

        public Color skin = new Color(0.85f, 0.68f, 0.55f);
        public Color hair = new Color(0.25f, 0.18f, 0.12f);
        public Color shirt = new Color(0.30f, 0.45f, 0.70f);
        public Color pants = new Color(0.22f, 0.24f, 0.28f);

        public float HeightMeters => Mathf.Lerp(1.55f, 1.95f, height);
        public float BuildScale => Mathf.Lerp(0.82f, 1.18f, build);

        public static AppearanceConfig CreateDefault() => new AppearanceConfig();

        public void Randomize(System.Random rng)
        {
            gender = (Gender)rng.Next(0, 3);
            height = (float)rng.NextDouble();
            build = (float)rng.NextDouble();
            skin = SkinSwatches[rng.Next(SkinSwatches.Length)];
            hair = HairSwatches[rng.Next(HairSwatches.Length)];
            shirt = RandomColor(rng);
            pants = RandomColor(rng);
            characterName = NamePool[rng.Next(NamePool.Length)];
        }

        // Palettes shared with the creation UI.
        public static readonly Color[] SkinSwatches =
        {
            new Color(0.96f, 0.80f, 0.69f), new Color(0.85f, 0.68f, 0.55f),
            new Color(0.71f, 0.53f, 0.39f), new Color(0.51f, 0.36f, 0.25f),
            new Color(0.35f, 0.24f, 0.17f),
        };

        public static readonly Color[] HairSwatches =
        {
            new Color(0.08f, 0.07f, 0.06f), new Color(0.25f, 0.18f, 0.12f),
            new Color(0.45f, 0.30f, 0.16f), new Color(0.78f, 0.68f, 0.42f),
            new Color(0.60f, 0.60f, 0.62f), new Color(0.70f, 0.25f, 0.20f),
        };

        private static readonly string[] NamePool =
            { "Avery", "Sam", "Kai", "Riley", "Noah", "Mia", "Jordan", "Lena", "Theo", "Ivy" };

        private static Color RandomColor(System.Random rng) =>
            new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
    }
}
