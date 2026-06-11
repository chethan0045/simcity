using System.Collections.Generic;
using UnityEngine;

namespace Simcity.Social
{
    /// <summary>
    /// Identity for any character that can have relationships (player or NPC). Holds
    /// a stable id, display name, and a sociability trait that modulates how fast it
    /// bonds. Maintains a global registry so systems/UI can resolve ids to names.
    /// </summary>
    [RequireComponent(typeof(Relationships))]
    public class Character : MonoBehaviour
    {
        public int id;
        public string displayName = "Character";
        public bool isPlayer;
        public bool isAdult = true;
        [Range(0f, 1f)] public float sociability = 0.5f;

        [HideInInspector] public Relationships relationships;

        public static readonly Dictionary<int, Character> Registry = new Dictionary<int, Character>();
        private static int _nextId = 1;

        private void Awake()
        {
            if (id == 0) id = _nextId++;
            relationships = GetComponent<Relationships>();
        }

        private void OnEnable() => Registry[id] = this;
        private void OnDisable() { if (Registry.TryGetValue(id, out var c) && c == this) Registry.Remove(id); }

        public static Character Player()
        {
            foreach (var c in Registry.Values) if (c.isPlayer) return c;
            return null;
        }
    }
}
