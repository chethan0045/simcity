using UnityEngine;
using Simcity.UI;

namespace Simcity.Interaction
{
    /// <summary>Workbench: opens the crafting menu, where you turn gathered materials into
    /// sellable goods via recipes (see <see cref="CraftingScreen"/>). Sell the goods at the
    /// Market for Coins.</summary>
    public class CraftInteractable : Interactable
    {
        public override void Interact(GameObject interactor) => CraftingScreen.Open(interactor);
    }
}
