using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes.PlanetTypes
{
    public class MoltenPlanet : DynamicPlanet
    {
        public Color surfColor1;
        public Color surfColor2;

        public MoltenPlanet(SystemController controller, GameObject gameObject, int seed, CelestialBody host = null) : base(controller, gameObject, seed, new CelestialType.MoltenPlanet(), host)
        {
            waterLevel = 1f;
            System.Random random = new System.Random(seed);
            surfColor1 = Utility.GetRandomColorInHSVRange(random, 0, 0, 0, 0, 0.2f, 0.3f);
            surfColor2 = Utility.GetRandomColorInHSVRange(random, 0, 0, 0.6f, 1, 0.3f, 0.8f);
            GenerateCraters(50, radius / 25f, radius / 5f);
        }

        public override Material GetMaterial(int LOD = 0)
        {
            Material material = new Material(controller.materialController.moltenPlanetMaterial);

            material.SetColor("_Color1", surfColor1);
            material.SetColor("_Color2", surfColor2);

            return material;
        }
    }
}
