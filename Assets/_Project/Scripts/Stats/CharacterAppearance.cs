using UnityEngine;
using Simcity.Common;

namespace Simcity.Stats
{
    /// <summary>
    /// Greybox stand-in avatar built from primitives (lower body, torso, head, hair).
    /// Proves the customization DATA -> VISUAL pipeline before any real art exists.
    ///
    /// THE SEAM: when a rigged humanoid mesh + blendshapes arrive (Phase 8 / asset
    /// work), replace the primitive construction and map AppearanceConfig onto
    /// blendshape weights & material slots — Apply()'s signature stays the same, so
    /// nothing else in the game has to change.
    /// </summary>
    public class CharacterAppearance : MonoBehaviour
    {
        [Tooltip("Hide head/hair so a first-person camera inside the head doesn't clip them.")]
        public bool hideHeadForFirstPerson;

        private Transform _root;
        private Renderer _lower, _torso, _head, _hair;

        private void Awake() => EnsureBuilt();

        private void EnsureBuilt()
        {
            if (_root != null) return;

            _root = new GameObject("AvatarRoot").transform;
            _root.SetParent(transform, false);

            // Local layout assumes a ~1.8m character standing on y = 0.
            _lower = MakePart(PrimitiveType.Capsule, "Lower", new Vector3(0f, 0.50f, 0f), new Vector3(0.45f, 0.50f, 0.35f));
            _torso = MakePart(PrimitiveType.Capsule, "Torso", new Vector3(0f, 1.20f, 0f), new Vector3(0.55f, 0.45f, 0.40f));
            _head = MakePart(PrimitiveType.Sphere, "Head", new Vector3(0f, 1.62f, 0f), new Vector3(0.42f, 0.46f, 0.42f));
            _hair = MakePart(PrimitiveType.Sphere, "Hair", new Vector3(0f, 1.72f, 0f), new Vector3(0.48f, 0.34f, 0.48f));
        }

        private Renderer MakePart(PrimitiveType type, string partName, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = partName;

            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col); // visual only — never block physics or the interaction ray

            go.transform.SetParent(_root, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            return go.GetComponent<Renderer>();
        }

        public void Apply(AppearanceConfig c)
        {
            EnsureBuilt();
            if (c == null) return;

            float heightFactor = c.HeightMeters / 1.8f;
            _root.localScale = new Vector3(c.BuildScale, heightFactor, c.BuildScale);

            MaterialUtils.SetColor(_lower, c.pants);
            MaterialUtils.SetColor(_torso, c.shirt);
            MaterialUtils.SetColor(_head, c.skin);
            MaterialUtils.SetColor(_hair, c.hair);

            bool showHead = !hideHeadForFirstPerson;
            _head.enabled = showHead;
            _hair.enabled = showHead;
        }
    }
}
