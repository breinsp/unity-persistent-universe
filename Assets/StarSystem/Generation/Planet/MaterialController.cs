using Assets.StarSystem.Generation.CelestialTypes;
using Assets.StarSystem.Generation.CelestialTypes.PlanetTypes;
using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet
{
    public class MaterialController : MonoBehaviour
    {
        public SystemController controller;

        public Material starMaterial;

        public Material waterPlanetMaterial;
        public Material rockPlanetMaterial;
        public Material gasPlanetMaterial;
        public Material moltenPlanetMaterial;
        public Material icePlanetMaterial;

        public Material waterMaterial;
        public Material lavaMaterial;
        public Material iceMaterial;
        public Material ringMaterial;
        public Material volumetricAtmosphereMaterial;
        public Material skyboxMaterial;

        private void Start()
        {
        }

        public Material GetMaterialForDynamicPlanet(Region region)
        {
            var planet = (region.planet as DynamicPlanet);

            Material material = planet.GetMaterial(region.LOD);

            if (controller.showLodColors)
            {
                Color c = Color.white;
                switch (region.LOD)
                {
                    case 0: c = Color.red; break;
                    case 1: c = Color.yellow; break;
                    case 2: c = Color.green; break;
                    case 3: c = Color.cyan; break;
                    case 4: c = Color.blue; break;
                    case 5: c = Color.magenta; break;
                    case 6: c = Color.black; break;
                }
                material.SetColor("_Color", c);
            }

            return new Material(material);
        }


        public Material GetMaterialForVolumetricAtmosphere(CelestialBody celestialBody)
        {
            Material material = new Material(volumetricAtmosphereMaterial);
            UpdateVolumetricAtmosphereMaterial(material, celestialBody);
            return material;
        }

        public void UpdateVolumetricAtmosphereMaterial(Material material, CelestialBody celestialBody)
        {
            float radius = celestialBody.zoomed ? celestialBody.radius * controller.planetScaleMultiplier : (controller.zoomed != null ? 0 : celestialBody.radius);
            material.SetFloat("_PlanetRadius", radius);
            material.SetFloat("_AtmosphereThickness", radius * (celestialBody.atmosphereRadiusMultiplier - 1));
        }

        /// <summary>
        /// Returns the main color of the atmosphere
        /// </summary>
        public Color GetColor1ForAtmosphere(CelestialBody celestialBody)
        {
            System.Random random = new System.Random(celestialBody.seed);
            Color color = Color.white;

            CelestialType type = celestialBody.type;
            if (type is CelestialType.Moon) type = (type as CelestialType.Moon).originalType;

            if (type is CelestialType.WaterPlanet || type is CelestialType.IcePlanet)
            {
                color = Utility.GetRandomColorInRange(random, 0, 128, 70, 200, 200, 255);
            }
            else if (type is CelestialType.MoltenPlanet)
            {
                color = Utility.GetRandomColorInRange(random, 255, 255, 150, 230, 0, 90);
            }
            else if (type is CelestialType.RockPlanet)
            {
                color = (celestialBody as RockPlanet).rockColor1;
            }

            return color;
        }

        /// <summary>
        /// Returns the secondary color of the atmosphere
        /// </summary>
        public Color GetColor2ForAtmosphere(CelestialBody celestialBody)
        {
            System.Random random = new System.Random(celestialBody.seed);
            Color color = Color.white;

            CelestialType type = celestialBody.type;
            if (type is CelestialType.Moon) type = (type as CelestialType.Moon).originalType;

            if (type is CelestialType.WaterPlanet || type is CelestialType.IcePlanet)
            {
                color = Color.red;
            }
            else if (type is CelestialType.MoltenPlanet)
            {
                color = Utility.GetRandomColorInRange(random, 255, 255, 0, 255, 0, 90);
            }
            else if (type is CelestialType.RockPlanet)
            {
                color = Color.white;
            }

            return color;
        }
    }
}
