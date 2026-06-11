#if SIMCITY_NETCODE
using UnityEngine;
using Unity.Netcode;
using Simcity.Interaction;
using Simcity.Social;

namespace Simcity.Net
{
    /// <summary>
    /// Walk up to a friend's avatar and press E to hand them Coins — the first
    /// player-to-player economic action, and the seed of the co-op marketplace
    /// (GAME_DESIGN §6/§9). Lives on a remote NetworkPlayer; routes the transfer through
    /// the giver's NetworkPlayer so the receiver is credited on their own machine.
    /// </summary>
    public class PlayerGiftInteractable : Interactable
    {
        public int amount = 10;

        public override string Prompt
        {
            get
            {
                var me = GetComponent<Character>();
                return me != null ? $"Give {amount} Coins — {me.displayName}" : $"Give {amount} Coins";
            }
        }

        public override void Interact(GameObject interactor)
        {
            var giver = interactor.GetComponent<NetworkPlayer>();
            var meObj = GetComponent<NetworkObject>();
            if (giver == null || meObj == null) return;

            giver.TryGiveCoins(meObj.OwnerClientId, amount);

            // Generosity builds a bond on the giver's social view (relationships stay local
            // per machine for now — cross-client relationship sync is a later pass).
            var giverCh = interactor.GetComponent<Character>();
            var meCh = GetComponent<Character>();
            if (giverCh != null && meCh != null) SocialGraph.Bond(meCh, giverCh, 6f);
        }
    }
}
#endif
