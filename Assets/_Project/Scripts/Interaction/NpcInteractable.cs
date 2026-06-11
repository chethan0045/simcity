using UnityEngine;
using Simcity.Social;

namespace Simcity.Interaction
{
    /// <summary>
    /// Lets the player walk up to an NPC and "Talk" (press E) to actively build the
    /// relationship. The prompt shows the NPC's name and current relationship tier.
    /// </summary>
    public class NpcInteractable : Interactable
    {
        public float pointsPerChat = 8f;

        public override string Prompt
        {
            get
            {
                var me = GetComponent<Character>();
                var player = Character.Player();
                if (me != null && player != null)
                {
                    float v = player.relationships.Value(me.id);
                    bool partner = player.relationships.IsPartner(me.id);
                    return $"Talk — {me.displayName} ({RelationshipTier.Name(v, partner)})";
                }
                return "Talk";
            }
        }

        public override void Interact(GameObject interactor)
        {
            var me = GetComponent<Character>();
            var other = interactor.GetComponent<Character>();
            if (me == null || other == null) return;

            SocialGraph.Bond(me, other, pointsPerChat);
            Debug.Log($"[Talk] You chatted with {me.displayName} " +
                      $"(now {RelationshipTier.Name(other.relationships.Value(me.id), other.relationships.IsPartner(me.id))}).");
        }
    }
}
