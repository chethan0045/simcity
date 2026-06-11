using UnityEngine;
using Simcity.Interaction;
using Simcity.Stats;

namespace Simcity.World
{
    /// <summary>
    /// Builds the single-apartment gameplay world (Phase 1/2): a small room with a
    /// bed, fridge, and work desk, plus the first-person player. Uses the shared
    /// WorldCommon + PlayerFactory helpers. (Phase 3 uses TownWorld instead.)
    /// </summary>
    public static class ApartmentWorld
    {
        public static GameObject Build(AppearanceConfig appearance)
        {
            WorldCommon.EnsureLighting();
            WorldCommon.EnsureClock();
            BuildApartment();

            var player = PlayerFactory.BuildPlayer(appearance, new Vector3(0f, 1.2f, -2f));
            PlayerFactory.BuildHud(player);

            Debug.Log("[ApartmentWorld] Ready. Work the desk → eat at the fridge → sleep in the bed; " +
                      "rent is due each new day.");
            return player;
        }

        private static void BuildApartment()
        {
            var floorColor = new Color(0.5f, 0.42f, 0.34f);
            var wallColor = new Color(0.8f, 0.78f, 0.72f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            Common.MaterialUtils.SetColor(floor.GetComponent<Renderer>(), floorColor);

            const float s = 10f, h = 3f, t = 0.3f;
            WorldCommon.CreateBox("Wall_N", new Vector3(0, h / 2, s / 2), new Vector3(s, h, t), wallColor);
            WorldCommon.CreateBox("Wall_S", new Vector3(0, h / 2, -s / 2), new Vector3(s, h, t), wallColor);
            WorldCommon.CreateBox("Wall_E", new Vector3(s / 2, h / 2, 0), new Vector3(t, h, s), wallColor);
            WorldCommon.CreateBox("Wall_W", new Vector3(-s / 2, h / 2, 0), new Vector3(t, h, s), wallColor);

            WorldCommon.CreateBox("Bed", new Vector3(-3f, 0.4f, 3f), new Vector3(2f, 0.8f, 3f),
                new Color(0.30f, 0.45f, 0.80f)).AddComponent<BedInteractable>().prompt = "Sleep";

            WorldCommon.CreateBox("Fridge", new Vector3(3.5f, 1f, 3.5f), new Vector3(1.2f, 2f, 1f),
                new Color(0.90f, 0.90f, 0.92f)).AddComponent<FoodInteractable>().prompt = "Eat";

            WorldCommon.CreateBox("Desk", new Vector3(3.5f, 0.5f, -3f), new Vector3(2f, 1f, 1f),
                new Color(0.45f, 0.32f, 0.20f)).AddComponent<WorkInteractable>().prompt = "Work";
        }
    }
}
