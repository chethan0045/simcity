using System.Collections.Generic;
using UnityEngine;

namespace Simcity.World
{
    public enum AmenityType { Food, Bed, Work, Social }

    /// <summary>
    /// Marks a place in the world NPCs can target to satisfy a need (eat, sleep,
    /// work, socialize). Maintains a registry for nearest-of-type queries. The same
    /// object usually also carries a player Interactable, so players and NPCs share
    /// the world's facilities.
    /// </summary>
    public class Amenity : MonoBehaviour
    {
        public AmenityType type;

        public static readonly List<Amenity> All = new List<Amenity>();

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        public static Amenity Nearest(AmenityType type, Vector3 from)
        {
            Amenity best = null;
            float bestSqr = float.MaxValue;
            foreach (var a in All)
            {
                if (a.type != type) continue;
                float d = (a.transform.position - from).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = a; }
            }
            return best;
        }
    }
}
