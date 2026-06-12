using System;
using UnityEngine;

namespace Simcity.Core
{
    /// <summary>
    /// The game-world clock. Maps real seconds to in-game minutes and exposes
    /// time-of-day + day count, plus events other systems hang off. Actions that
    /// skip time (sleeping, working a shift) call AdvanceMinutes / SkipToHour, so
    /// need-decay and day-rollover stay consistent whether time passes live or jumps.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock Instance { get; private set; }

        [Tooltip("Real-time seconds for one full in-game day (24h). Lower = faster days; tune for testing.")]
        public float secondsPerDay = 900f; // 15 min/day

        [Tooltip("Hour the world starts at on Day 1 (0..24).")]
        public float startHour = 8f;

        [Tooltip("Optional directional light rotated to fake a day/night cycle.")]
        public Light sun;

        [Tooltip("When false, an external system drives time (e.g. Phase 6's NetworkClock " +
                 "on co-op clients) and the clock won't advance on its own.")]
        public bool autoAdvance = true;

        public int Day { get; private set; } = 1;
        public float Hour { get; private set; }        // 0..24
        public float NormalizedTime => Hour / 24f;     // 0..1

        /// <summary>Fires once per in-game day rollover, passing the new day number.</summary>
        public event Action<int> OnNewDay;

        /// <summary>Fires whenever the clock advances, passing the in-game minutes elapsed.</summary>
        public event Action<float> OnMinutesAdvanced;

        private float _minutesPerSecond;

        private void Awake()
        {
            Instance = this;
            Hour = startHour;
            _minutesPerSecond = (24f * 60f) / Mathf.Max(1f, secondsPerDay);
        }

        private void Start()
        {
            if (sun == null)
                foreach (var l in FindObjectsOfType<Light>())
                    if (l.type == LightType.Directional) { sun = l; break; }
        }

        private void Update()
        {
            if (autoAdvance)
                AdvanceMinutes(_minutesPerSecond * Time.deltaTime);

            if (sun != null) // sunrise ~6:00, noon overhead, sunset ~18:00
                sun.transform.rotation = Quaternion.Euler((NormalizedTime * 360f) - 90f, -30f, 0f);
        }

        /// <summary>Force the clock to a specific day/hour without firing events — used by
        /// co-op clients to mirror the server's authoritative time (see NetworkClock).</summary>
        public void SetTime(int day, float hour)
        {
            Day = Mathf.Max(1, day);
            Hour = Mathf.Repeat(hour, 24f);
        }

        /// <summary>Advance the clock by in-game minutes (live or via a skip).</summary>
        public void AdvanceMinutes(float minutes)
        {
            if (minutes <= 0f) return;

            float newHour = Hour + minutes / 60f;
            while (newHour >= 24f)
            {
                newHour -= 24f;
                Day++;
                OnNewDay?.Invoke(Day);
            }
            Hour = newHour;
            OnMinutesAdvanced?.Invoke(minutes);
        }

        /// <summary>Skip forward to a target hour (rolling into the next day if needed).
        /// Returns the in-game minutes skipped. Used by sleeping.</summary>
        public float SkipToHour(float targetHour)
        {
            float minutes = ((targetHour - Hour + 24f) % 24f) * 60f;
            if (minutes <= 0f) minutes = 24f * 60f; // same hour -> a full day
            AdvanceMinutes(minutes);
            return minutes;
        }

        public string TimeString()
        {
            int h = Mathf.FloorToInt(Hour);
            int m = Mathf.FloorToInt((Hour - h) * 60f);
            return $"Day {Day}  {h:00}:{m:00}";
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
