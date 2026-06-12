#if SIMCITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using Simcity.Core;

namespace Simcity.Net
{
    /// <summary>
    /// Keeps the day/time identical for everyone in a co-op session. The host's GameClock
    /// is the source of truth; this replicates Day + Hour to clients, which stop advancing
    /// their own clock and mirror the host. So sleeping/working time-skips by the host roll
    /// the world forward for the whole group, and the sun is in the same place for all.
    /// </summary>
    public class NetworkClock : NetworkBehaviour
    {
        private readonly NetworkVariable<int> _day = new(1);
        private readonly NetworkVariable<float> _hour = new(8f);

        private GameClock _clock;
        private float _nextPush;
        private const float PushInterval = 0.4f; // ~2.5 Hz — plenty for a clock, light on the wire

        public override void OnNetworkSpawn()
        {
            _clock = GameClock.Instance;

            if (!IsServer && _clock != null)
            {
                _clock.autoAdvance = false; // the host drives time now
                ApplyToClock();
                _day.OnValueChanged += (_, __) => ApplyToClock();
                _hour.OnValueChanged += (_, __) => ApplyToClock();
            }
        }

        private void Update()
        {
            if (!IsServer || _clock == null) return;
            if (Time.unscaledTime < _nextPush) return;
            _nextPush = Time.unscaledTime + PushInterval;

            if (_day.Value != _clock.Day) _day.Value = _clock.Day;
            _hour.Value = _clock.Hour;
        }

        private void ApplyToClock()
        {
            if (_clock != null) _clock.SetTime(_day.Value, _hour.Value);
        }
    }
}
#endif
