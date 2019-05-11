using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes.PlanetTypes
{
    public class WaterPlanet : DynamicPlanet
    {
        public Color sandColor;
        public Color grassColor;
        public Color dirtColor;
        public Color rockColor;

        public WaterPlanet(SystemController controller, GameObject gameObject, int seed, CelestialBody host = null) : base(controller, gameObject, seed, new CelestialType.WaterPlanet(), host)
        {
            System.Random random = new System.Random(seed);
            sandColor = Utility.GetRandomColorInHSVRange(random, 30f / 360f, 45f / 360f, 0.4f, 0.7f, 0.9f, 1);
            grassColor = Utility.GetRandomColorInRange(random, 0, 255, 120, 200, 0, 50);
            dirtColor = Utility.GetRandomColorInHSVRange(random, 20f / 360f, 40f / 360f, 0.3f, 0.9f, 0.5f, 0.9f);
            rockColor = Utility.GetRandomGrayColor(random);
            GenerateCraters((int)Utility.GetRandomBetween(random, 0, 5), radius / 30f, radius / 10f);
        }

        public override Material GetMaterial(int LOD = 0)
        {
            Material material = new Material(controller.materialController.waterPlanetMaterial);
            material.SetVector("_Center", position);
            material.SetColor("_SandColor", sandColor);
            material.SetColor("_GrassColor", grassColor);
            material.SetColor("_DirtColor", dirtColor);
            material.SetColor("_RockColor", rockColor);
            material.SetInt("_Lod", LOD);
            material.SetFloat("_SandLevel", waterLevel * radius * (zoomed ? controller.planetScaleMultiplier : 1));

            return material;
        }
    }
}
