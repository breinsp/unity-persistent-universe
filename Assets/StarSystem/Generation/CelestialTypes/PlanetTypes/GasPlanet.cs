using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes.PlanetTypes
{
    public class GasPlanet : CelestialBody
    {
        public Color color1;
        public Color color2;
        public Color color3;
        public Color atmos;

        public GasPlanet(SystemController controller, GameObject gameObject, int seed, CelestialBody host = null) : base(controller, gameObject, seed, new CelestialType.GasPlanet(), host)
        {
            gameObject.AddComponent<MeshFilter>().mesh = Octahedron.Create(5, radius);
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = GetMaterial();
        }

        public Material GetMaterial(int LOD = 0)
        {
            Material material = new Material(controller.materialController.gasPlanetMaterial);

            System.Random random = new System.Random(seed);
            color1 = Utility.GetRandomStrongColor(random);
            color2 = Color.white - color1;
            color3 = Utility.GetRandomColor(random);
            atmos = color1;

            int width = 2048;
            float frq1 = (float)Utility.GetRandomBetween(random, 10, 100);
            float frq2 = (float)Utility.GetRandomBetween(random, frq1, 100);

            Texture2D tex = new Texture2D(width, 1);

            for (int i = 0; i < width; i++)
            {
                float f = i / (width - 1f);
                float s = Mathf.Sin(f * Mathf.PI);
                float p1 = (Mathf.PerlinNoise(f * frq1, seed * 100) + 1) / 2;
                float p2 = (Mathf.PerlinNoise(f * frq2, seed * 200) + 1) / 2;
                float r1 = Mathf.Clamp(s * p1, 0, 1);
                float r2 = Mathf.Clamp(s * p2, 0, 1);
                tex.SetPixel(i, 0, new Color(r1, r2, 1, 1));
            }
            tex.Apply();

            material.SetTexture("_MainTex", tex);

            material.SetColor("_Color1", color1);
            material.SetColor("_Color2", color2);
            material.SetColor("_Color3", color3);

            material.SetFloat("_Frequency1", (float)Utility.GetRandomBetween(random, 0, 5));
            material.SetFloat("_Frequency2", (float)Utility.GetRandomBetween(random, 0, 20));

            material.SetFloat("_Strength1", (float)Utility.GetRandomBetween(random, 0, 2));
            material.SetFloat("_Strength2", (float)Utility.GetRandomBetween(random, 0, 1));

            material.SetFloat("_FresnelWidth", (float)Utility.GetRandomBetween(random, 0.7, 0.8));
            material.SetFloat("_FresnelStrength", (float)Utility.GetRandomBetween(random, 1, 3));

            material.SetFloat("_Speed", (float)Utility.GetRandomBetween(random, 1, 3));
            return material;
        }
    }
}
