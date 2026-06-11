using System.Collections.Generic;
using UnityEngine;

namespace Simcity.Social
{
    /// <summary>
    /// Drives organic bonding: every tick, any two characters standing close together
    /// grow their relationship a little (scaled by their sociability). So gatherings
    /// at the plaza, sharing the diner, or the player hanging around an NPC all build
    /// bonds over time — and once a pair gets close enough, romance can form.
    /// </summary>
    public class SocialSystem : MonoBehaviour
    {
        public float range = 2.5f;
        public float tickInterval = 1f;
        public float baseRate = 2.5f;     // relationship points per tick at avg sociability

        private float _timer;
        private readonly List<Character> _scratch = new List<Character>();

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < tickInterval) return;
            _timer = 0f;

            _scratch.Clear();
            _scratch.AddRange(Character.Registry.Values);

            float rangeSqr = range * range;
            for (int i = 0; i < _scratch.Count; i++)
            {
                for (int j = i + 1; j < _scratch.Count; j++)
                {
                    var a = _scratch[i];
                    var b = _scratch[j];
                    if (a == null || b == null) continue;

                    if ((a.transform.position - b.transform.position).sqrMagnitude <= rangeSqr)
                    {
                        float rate = baseRate * (0.5f + 0.5f * (a.sociability + b.sociability));
                        SocialGraph.Bond(a, b, rate);
                    }
                }
            }
        }
    }
}
