using UnityEngine;
using UnityEngine.Rendering;

namespace Simcity.Common
{
    /// <summary>
    /// Render-pipeline-agnostic material helpers. Phase 0 builds greybox geometry
    /// at runtime, and the correct lit shader / color property differs between the
    /// Built-in pipeline ("Standard" / "_Color") and URP ("URP/Lit" / "_BaseColor").
    /// These helpers pick the right one so the same code works in either project.
    /// </summary>
    public static class MaterialUtils
    {
        private static Shader _litShader;

        public static Shader LitShader
        {
            get
            {
                if (_litShader != null) return _litShader;

                // A scriptable render pipeline (e.g. URP) is active.
                if (GraphicsSettings.defaultRenderPipeline != null)
                    _litShader = Shader.Find("Universal Render Pipeline/Lit");

                // Fall back to Built-in, then a last-ditch always-present shader.
                if (_litShader == null) _litShader = Shader.Find("Standard");
                if (_litShader == null) _litShader = Shader.Find("Sprites/Default");

                return _litShader;
            }
        }

        public static Material NewMaterial(Color color)
        {
            var m = new Material(LitShader);
            SetColor(m, color);
            return m;
        }

        public static void SetColor(Material m, Color color)
        {
            if (m == null) return;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color); // URP
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);         // Built-in
            m.color = color;                                                   // harmless fallback
        }

        public static void SetColor(Renderer r, Color color)
        {
            if (r != null) SetColor(r.material, color);
        }
    }
}
