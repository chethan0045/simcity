using UnityEngine;
using UnityEngine.Rendering;
using Simcity.Common;
using Simcity.Core;

namespace Simcity.World
{
    /// <summary>Shared world-building helpers used by every world builder
    /// (apartment, town, …) so lighting/clock/box creation live in one place.</summary>
    public static class WorldCommon
    {
        public static void EnsureLighting()
        {
            if (Object.FindObjectOfType<Light>() == null)
            {
                var go = new GameObject("Directional Light");
                var light = go.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                light.shadows = LightShadows.Soft;
                go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.5f);
        }

        public static void EnsureClock()
        {
            if (Object.FindObjectOfType<GameClock>() == null)
                new GameObject("GameClock").AddComponent<GameClock>();
        }

        public static GameObject CreateBox(string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = size;
            MaterialUtils.SetColor(go.GetComponent<Renderer>(), color);
            return go;
        }
    }
}
