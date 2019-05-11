using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes.PlanetTypes
{
    public class IcePlanet : DynamicPlanet
    {
        public Color snowColor;
        public Color iceColor;
        public Color dirtColor;
        public Color rockColor;

        public IcePlanet(SystemController controller, GameObject gameObject, int seed, CelestialBody host = null) : base(controller, gameObject, seed, new CelestialType.IcePlanet(), host)
        {
            GenerateCraters(30, radius / 25f, radius / 5f);
            snowColor = Color.white;
            System.Random random = new System.Random(seed);
            dirtColor = Utility.GetRandomColorInHSVRange(random, 30f / 360f, 40f / 360f, 0.4f, 0.7f, 0.6f, 0.9f);
            rockColor = new Color32(159, 165, 168, 255);
        }

        public override Material GetMaterial(int LOD = 0)
        {
            Material material = new Material(controller.materialController.icePlanetMaterial);
            material.SetColor("_SnowColor", snowColor);
            material.SetColor("_DirtColor", dirtColor);
            material.SetColor("_RockColor", rockColor);
            return material;
        }
    }
}
