using System;
using System.Collections.Generic;
using UnityEngine;
using Simcity.Core;

namespace Simcity.Stats
{
    public enum NeedType { Hunger, Energy }

    /// <summary>
    /// Tracks 0..100 needs that decay over GAME time (100 = satisfied, 0 = critical).
    /// Decay is driven by GameClock.OnMinutesAdvanced so it's consistent whether time
    /// passes live or is skipped by sleeping/working. Phase 1 ships Hunger + Energy;
    /// hygiene/social/fun/health slot in later (see GAME_DESIGN.md §4.2).
    /// </summary>
    public class CharacterNeeds : MonoBehaviour
    {
        [Serializable]
        public struct NeedConfig
        {
            public NeedType type;
            [Range(0, 100)] public float start;
            public float decayPerHour; // points lost per in-game hour
        }

        public List<NeedConfig> config = new List<NeedConfig>
        {
            new NeedConfig { type = NeedType.Hunger, start = 80f, decayPerHour = 4f },
            new NeedConfig { type = NeedType.Energy, start = 90f, decayPerHour = 3f },
        };

        public event Action<NeedType, float> OnNeedChanged;

        private readonly Dictionary<NeedType, float> _values = new Dictionary<NeedType, float>();
        private readonly Dictionary<NeedType, float> _decay = new Dictionary<NeedType, float>();

        private void Awake()
        {
            foreach (var c in config)
            {
                _values[c.type] = c.start;
                _decay[c.type] = c.decayPerHour;
            }
        }

        private void Start()
        {
            if (GameClock.Instance != null)
                GameClock.Instance.OnMinutesAdvanced += ApplyDecay;
        }

        private void OnDestroy()
        {
            if (GameClock.Instance != null)
                GameClock.Instance.OnMinutesAdvanced -= ApplyDecay;
        }

        public float Get(NeedType t) => _values.TryGetValue(t, out var v) ? v : 0f;

        public void Replenish(NeedType t, float amount) => SetValue(t, Get(t) + amount);

        private void ApplyDecay(float minutes)
        {
            float hours = minutes / 60f;
            // Copy keys: we mutate values while iterating.
            var keys = new List<NeedType>(_decay.Keys);
            foreach (var t in keys)
                if (_decay[t] != 0f)
                    SetValue(t, Get(t) - _decay[t] * hours);
        }

        private void SetValue(NeedType t, float v)
        {
            v = Mathf.Clamp(v, 0f, 100f);
            _values[t] = v;
            OnNeedChanged?.Invoke(t, v);
        }
    }
}
