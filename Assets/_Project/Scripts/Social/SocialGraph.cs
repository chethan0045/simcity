using UnityEngine;

namespace Simcity.Social
{
    /// <summary>
    /// Helpers that mutate relationships symmetrically and handle romance promotion.
    /// Keeping both sides in sync here means callers never have to.
    /// </summary>
    public static class SocialGraph
    {
        /// <summary>Strengthen (or, with a negative amount, weaken) the bond both ways.</summary>
        public static void Bond(Character a, Character b, float amount)
        {
            if (a == null || b == null || a == b) return;
            a.relationships.Add(b.id, amount);
            b.relationships.Add(a.id, amount);
            TryRomance(a, b);
        }

        private static void TryRomance(Character a, Character b)
        {
            if (!a.isAdult || !b.isAdult) return;
            if (a.relationships.IsPartner(b.id)) return;
            if (a.relationships.Value(b.id) < 80f || b.relationships.Value(a.id) < 80f) return;
            if (a.relationships.HasAnyPartner() || b.relationships.HasAnyPartner()) return;

            a.relationships.SetPartner(b.id, true);
            b.relationships.SetPartner(a.id, true);
            Debug.Log($"[Romance] {a.displayName} and {b.displayName} are now partners. ♥");
        }
    }
}
