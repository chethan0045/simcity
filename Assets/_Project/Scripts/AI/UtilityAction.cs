using UnityEngine;
using Simcity.Core;

namespace Simcity.AI
{
    /// <summary>
    /// One thing an NPC can choose to do. The brain scores every action each tick
    /// and runs the highest. This is classic utility AI — no scripted routines, so
    /// behavior emerges from needs + context (GAME_DESIGN §4.3).
    ///
    /// Lifecycle: Score() ranks it → OnEnter() when it becomes active → GetTarget()
    /// tells the mover where to go → once arrived, Perform() runs each frame until
    /// it returns true (done), then the brain re-chooses.
    /// </summary>
    public abstract class UtilityAction
    {
        public abstract string Name { get; }

        /// <summary>Desirability right now (higher wins). 0 = don't.</summary>
        public abstract float Score(NpcBrain npc);

        /// <summary>Called once when this action becomes the active choice.</summary>
        public virtual void OnEnter(NpcBrain npc) { }

        /// <summary>World position to walk to first, or null to act in place.</summary>
        public virtual Vector3? GetTarget(NpcBrain npc) => null;

        /// <summary>Run while active and arrived; return true when finished.</summary>
        public abstract bool Perform(NpcBrain npc, float dt);

        protected static bool IsNight()
        {
            var c = GameClock.Instance;
            return c != null && (c.Hour < 6f || c.Hour >= 21f);
        }
    }
}
