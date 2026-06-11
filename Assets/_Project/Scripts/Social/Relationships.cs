using System.Collections.Generic;
using UnityEngine;

namespace Simcity.Social
{
    /// <summary>
    /// One character's view of its relationships: a value in [-100, 100] toward each
    /// other character it has met, plus a "partner" flag for romance. Lives on every
    /// character (player + NPC); the pair is kept symmetric by SocialGraph.
    /// </summary>
    public class Relationships : MonoBehaviour
    {
        public class State
        {
            public float value;
            public bool partner;
        }

        private readonly Dictionary<int, State> _map = new Dictionary<int, State>();

        public IEnumerable<KeyValuePair<int, State>> All => _map;

        public State GetOrCreate(int id)
        {
            if (!_map.TryGetValue(id, out var s)) { s = new State(); _map[id] = s; }
            return s;
        }

        public float Value(int id) => _map.TryGetValue(id, out var s) ? s.value : 0f;
        public bool IsPartner(int id) => _map.TryGetValue(id, out var s) && s.partner;

        public void Add(int id, float amount)
        {
            var s = GetOrCreate(id);
            s.value = Mathf.Clamp(s.value + amount, -100f, 100f);
        }

        public void SetPartner(int id, bool value) => GetOrCreate(id).partner = value;

        public bool HasAnyPartner()
        {
            foreach (var kv in _map) if (kv.Value.partner) return true;
            return false;
        }
    }
}
