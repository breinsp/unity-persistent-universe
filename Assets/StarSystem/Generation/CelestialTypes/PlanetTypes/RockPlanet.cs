using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes.PlanetTypes
{
    public class RockPlanet : DynamicPlanet
    {
        public Color rockColor1;
        public Color rockColor2;

        public RockPlanet(SystemController controller, GameObject gameObject, int seed, CelestialBody host = null) : base(controller, gameObject, seed, new CelestialType.RockPlanet(), host)
        {
            System.Random random = new System.Random(seed);
            rockColor1 = Utility.GetRandomColorInHSVRange(random, 10f / 360f, 30f / 360f, 0f, 0.8f, 0.4f, 0.9f);
            rockColor2 = Utility.GetRandomColorInHSVRange(random, 0f / 360f, 30f / 360f, 0f, 0.8f, 0.4f, 0.9f);
            GenerateCraters(80, radius / 25f, radius / 5f);
        }

        public override Material GetMaterial(int LOD = 0)
        {
            Material material = new Material(controller.materialController.rockPlanetMaterial);

            material.SetColor("_Color1", rockColor1);
            material.SetColor("_Color2", rockColor2);

            return material;
        }
    }
}
