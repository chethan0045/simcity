namespace Simcity.Social
{
    /// <summary>Maps a relationship value (+ partner flag) to a human-readable tier.</summary>
    public static class RelationshipTier
    {
        public static string Name(float value, bool partner)
        {
            if (partner) return "Partner";
            if (value >= 80f) return "Best Friend";
            if (value >= 50f) return "Close Friend";
            if (value >= 20f) return "Friend";
            if (value >= 5f) return "Acquaintance";
            if (value <= -25f) return "Rival";
            if (value < 0f) return "Disliked";
            return "Stranger";
        }
    }
}
