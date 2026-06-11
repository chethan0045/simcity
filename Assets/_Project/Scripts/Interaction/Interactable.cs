using UnityEngine;

namespace Simcity.Interaction
{
    /// <summary>
    /// Base class for anything the player can look at and use. The whole "look at ->
    /// prompt -> use" flow keys off this type, so every interactive object in the
    /// game (doors, crops, market stalls, NPCs...) will eventually derive from it.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Tooltip("Verb shown in the interaction prompt, e.g. 'Open', 'Use', 'Talk'.")]
        public string prompt = "Use";

        /// <summary>Text the interactor shows while focused on this object.</summary>
        public virtual string Prompt => prompt;

        /// <summary>Called when the player presses the interact key while focused.</summary>
        public abstract void Interact(GameObject interactor);
    }
}
