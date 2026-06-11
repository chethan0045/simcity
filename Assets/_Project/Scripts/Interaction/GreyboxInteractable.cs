using UnityEngine;
using Simcity.Common;

namespace Simcity.Interaction
{
    /// <summary>
    /// Placeholder interactable for the Phase 0 greybox: toggles its color and logs
    /// when used, purely to prove the "look at -> use" loop works end to end. Real
    /// interactables (doors, crops, etc.) replace this in later phases.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class GreyboxInteractable : Interactable
    {
        public Color idleColor = new Color(0.65f, 0.65f, 0.7f);
        public Color usedColor = new Color(0.25f, 0.8f, 0.45f);

        private Renderer _renderer;
        private bool _on;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            Apply();
        }

        public override void Interact(GameObject interactor)
        {
            _on = !_on;
            Apply();
            Debug.Log($"[Interact] {name} -> {(_on ? "ON" : "OFF")} (by {interactor.name})");
        }

        private void Apply() => MaterialUtils.SetColor(_renderer, _on ? usedColor : idleColor);
    }
}
