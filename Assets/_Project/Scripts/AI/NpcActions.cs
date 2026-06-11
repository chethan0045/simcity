using UnityEngine;
using Simcity.Stats;
using Simcity.World;

namespace Simcity.AI
{
    /// <summary>Go to food and eat until satisfied. Urgency rises as hunger falls.</summary>
    public class EatAction : UtilityAction
    {
        private Amenity _a;
        public override string Name => "Eat";
        public override void OnEnter(NpcBrain n) => _a = Amenity.Nearest(AmenityType.Food, n.transform.position);
        public override Vector3? GetTarget(NpcBrain n) => _a ? _a.transform.position : (Vector3?)null;
        public override bool Perform(NpcBrain n, float dt)
        {
            if (_a == null) return true;                 // no diner — give up, re-choose
            n.needs.Replenish(NeedType.Hunger, 25f * dt);
            return n.needs.Get(NeedType.Hunger) >= 95f;
        }
        public override float Score(NpcBrain n)
        {
            float h = n.needs.Get(NeedType.Hunger);
            return h >= 65f ? 0f : Mathf.Clamp01((65f - h) / 65f) * 1.3f;
        }
    }

    /// <summary>Go to a bed and sleep. Urgency rises as energy falls; strong at night.</summary>
    public class SleepAction : UtilityAction
    {
        private Amenity _a;
        public override string Name => "Sleep";
        public override void OnEnter(NpcBrain n) => _a = Amenity.Nearest(AmenityType.Bed, n.transform.position);
        public override Vector3? GetTarget(NpcBrain n) => _a ? _a.transform.position : (Vector3?)null;
        public override bool Perform(NpcBrain n, float dt)
        {
            if (_a == null) return true;
            n.needs.Replenish(NeedType.Energy, 22f * dt);
            return n.needs.Get(NeedType.Energy) >= 95f;
        }
        public override float Score(NpcBrain n)
        {
            float e = n.needs.Get(NeedType.Energy);
            float urgency = e >= 70f ? 0f : Mathf.Clamp01((70f - e) / 70f);
            return urgency * 1.2f + (IsNight() ? 0.5f : 0f);
        }
    }

    /// <summary>Daytime job: go to the workshop and put in a shift (drains energy).</summary>
    public class WorkAction : UtilityAction
    {
        private Amenity _a;
        private float _t;
        public override string Name => "Work";
        public override void OnEnter(NpcBrain n) { _a = Amenity.Nearest(AmenityType.Work, n.transform.position); _t = 0f; }
        public override Vector3? GetTarget(NpcBrain n) => _a ? _a.transform.position : (Vector3?)null;
        public override bool Perform(NpcBrain n, float dt)
        {
            if (_a == null) return true;
            _t += dt;
            n.needs.Replenish(NeedType.Energy, -6f * dt);
            return _t >= 6f;
        }
        public override float Score(NpcBrain n)
        {
            if (IsNight()) return 0f;
            if (n.needs.Get(NeedType.Energy) < 30f || n.needs.Get(NeedType.Hunger) < 30f) return 0f;
            return 0.45f;
        }
    }

    /// <summary>Hang out at a social spot for a bit — makes villagers gather.</summary>
    public class SocializeAction : UtilityAction
    {
        private Amenity _a;
        private float _t;
        public override string Name => "Socialize";
        public override void OnEnter(NpcBrain n) { _a = Amenity.Nearest(AmenityType.Social, n.transform.position); _t = 0f; }
        public override Vector3? GetTarget(NpcBrain n) => _a ? _a.transform.position : (Vector3?)null;
        public override bool Perform(NpcBrain n, float dt)
        {
            if (_a == null) return true;
            _t += dt;
            return _t >= 5f;
        }
        public override float Score(NpcBrain n)
        {
            if (n.needs.Get(NeedType.Energy) < 20f) return 0f;
            return IsNight() ? 0.05f : 0.2f;
        }
    }

    /// <summary>Default behavior: stroll to a random nearby spot. Lowest priority.</summary>
    public class WanderAction : UtilityAction
    {
        private Vector3 _target;
        private float _pause;
        public override string Name => "Wander";
        public override void OnEnter(NpcBrain n)
        {
            var r = Random.insideUnitCircle * TownWorld.WanderRadius;
            _target = TownWorld.Center + new Vector3(r.x, 0f, r.y);
            _pause = 0f;
        }
        public override Vector3? GetTarget(NpcBrain n) => _target;
        public override bool Perform(NpcBrain n, float dt)
        {
            _pause += dt;          // arrived — pause a beat, then re-choose
            return _pause >= 1f;
        }
        public override float Score(NpcBrain n) => 0.1f;
    }
}
