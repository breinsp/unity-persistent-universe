using Assets.StarSystem.Generation.Planet;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.StarSystem.Generation.CelestialTypes
{
    public abstract class DynamicPlanet : CelestialBody
    {
        public float waterLevel = 1;
        public List<Region> regions; //20 regions/faces
        public TerrainNoise terrain;
        public List<Crater> craters;

        public float Circumference { get { return radius * 2 * Mathf.PI; } }

        public DynamicPlanet(SystemController controller, GameObject gameObject, int seed, CelestialType type, CelestialBody host = null) : base(controller, gameObject, seed, type, host)
        {
            this.seed = seed;
            this.gameObject = gameObject;
            position = gameObject.transform.position;
            regions = new List<Region>();
            terrain = new TerrainNoise(this, seed);
            craters = new List<Crater>();
            Vector3[] vertices = null;
            int[] triangles = null;
            Octahedron.GetVerticesAndTriangles(ref vertices, ref triangles);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int t1 = triangles[i + 0];
                int t2 = triangles[i + 1];
                int t3 = triangles[i + 2];

                Vector3 A = vertices[t1].normalized * radius;
                Vector3 B = vertices[t2].normalized * radius;
                Vector3 C = vertices[t3].normalized * radius;

                regions.Add(new Region(this, null, A, B, C, 0));
            }
        }

        public abstract Material GetMaterial(int LOD = 0);

        /// <summary>
        /// Generate a list of craters
        /// </summary>
        public void GenerateCraters(int count, float minRad, float maxRad)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Utility.RandomVector(random);
                float rad = (float)Utility.GetRandomBetween(random, minRad, maxRad);
                craters.Add(new Crater { direction = dir, radius = rad });
            }
        }
    }
}