using System;
using System.IO;
using UnityEngine;
using Simcity.Stats;

namespace Simcity.Core
{
    /// <summary>The persisted player character. Phase 2 stores appearance; wallet,
    /// needs, and progress join this later so a character carries across sessions
    /// (and eventually into co-op — GAME_DESIGN §4.1).</summary>
    [Serializable]
    public class CharacterProfile
    {
        public int version = 1;
        public AppearanceConfig appearance = new AppearanceConfig();
    }

    /// <summary>
    /// JSON persistence to Application.persistentDataPath/profile.json.
    /// Unity's JsonUtility serializes the nested AppearanceConfig (incl. Color) fine.
    /// </summary>
    public static class SaveSystem
    {
        private static string FilePath =>
            Path.Combine(Application.persistentDataPath, "profile.json");

        public static bool HasProfile() => File.Exists(FilePath);

        public static void Save(CharacterProfile profile)
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(profile, true));
                Debug.Log($"[Save] Profile written to {FilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Failed: {e.Message}");
            }
        }

        public static CharacterProfile Load()
        {
            try
            {
                if (HasProfile())
                    return JsonUtility.FromJson<CharacterProfile>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Load failed: {e.Message}");
            }
            return new CharacterProfile();
        }

        public static void Delete()
        {
            if (HasProfile()) File.Delete(FilePath);
            Debug.Log("[Save] Profile deleted — the character creator will show on next Play.");
        }
    }
}
