using System.Collections.Generic;
using UnityEngine;
using Simcity.Stats;

namespace Simcity.AI
{
    /// <summary>
    /// Utility-AI brain for an NPC villager. Every decisionInterval it scores all
    /// actions and runs the best one; the chosen action drives movement (via
    /// SimpleMover) and its effect. Reuses CharacterNeeds, so NPCs get hungry/tired
    /// over game-time exactly like the player and act to fix it.
    /// </summary>
    [RequireComponent(typeof(CharacterNeeds))]
    [RequireComponent(typeof(SimpleMover))]
    public class NpcBrain : MonoBehaviour
    {
        public float decisionInterval = 1.25f;
        public bool verboseLogging;
        public string displayName = "Villager";

        [HideInInspector] public CharacterNeeds needs;
        [HideInInspector] public SimpleMover mover;

        private List<UtilityAction> _actions;
        private UtilityAction _current;
        private float _nextDecision;

        private void Awake()
        {
            needs = GetComponent<CharacterNeeds>();
            mover = GetComponent<SimpleMover>();
            _actions = new List<UtilityAction>
            {
                new SleepAction(), new EatAction(), new WorkAction(),
                new SocializeAction(), new WanderAction(),
            };
        }

        private void Update()
        {
            if (_current == null || Time.time >= _nextDecision)
            {
                Choose();
                _nextDecision = Time.time + decisionInterval;
            }
            if (_current == null) return;

            Vector3? target = _current.GetTarget(this);
            if (target.HasValue && !mover.AtPosition(target.Value))
            {
                mover.MoveTowards(target.Value);
                return;
            }

            if (_current.Perform(this, Time.deltaTime))
                _current = null; // finished — re-choose next frame
        }

        private void Choose()
        {
            UtilityAction best = null;
            float bestScore = float.NegativeInfinity;
            foreach (var a in _actions)
            {
                float s = a.Score(this);
                if (s > bestScore) { bestScore = s; best = a; }
            }

            if (best != _current)
            {
                _current = best;
                _current?.OnEnter(this);
                if (verboseLogging) Debug.Log($"[NPC {displayName}] -> {best?.Name}");
            }
        }
    }
}
