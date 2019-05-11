using Assets.External;
using Assets.StarSystem.Generation.CelestialTypes;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet
{
    public class TerrainNoise
    {
        DynamicPlanet planet;
        public int seed;
        public float frequency;
        public float amplitude;
        public float craterStrength;

        //for terrain
        private FastNoise tn1;
        private FastNoise tn2;
        private FastNoise tn3;
        //for color
        private FastNoise cn1;

        public TerrainNoise(DynamicPlanet planet, int seed)
        {
            this.planet = planet;
            this.seed = seed;
            amplitude = planet.radius / 30f;
            frequency = planet.radius / 5f;
            craterStrength = .01f;

            ConfigureNoises();
        }

        /// <summary>
        /// configures noise settings
        /// </summary>
        private void ConfigureNoises()
        {
            //continents
            tn1 = new FastNoise(seed);
            tn1.SetNoiseType(FastNoise.NoiseType.ValueFractal);
            tn1.SetFrequency(1f);
            tn1.SetFractalOctaves(4);
            tn1.SetFractalType(FastNoise.FractalType.FBM);

            //ridges
            tn2 = new FastNoise(seed + 1);
            tn2.SetNoiseType(FastNoise.NoiseType.ValueFractal);
            tn2.SetFractalOctaves(4);
            tn2.SetFrequency(1f);
            tn1.SetFractalType(FastNoise.FractalType.FBM);

            tn3 = new FastNoise(seed + 2);
            tn3.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            tn3.SetFrequency(1f);
            tn3.SetFractalOctaves(2);

            cn1 = new FastNoise(seed + 3);
            cn1.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            cn1.SetFractalOctaves(4);
            cn1.SetFrequency(1f);
        }

        /// <summary>
        /// Get the noise value for a point on the sphere
        /// </summary>
        public float GetNoise(Vector3 P, int LOD = 0)
        {
            P = P.normalized * frequency;
            float v = 0;
            float mask = 1;

            int octaves = Mathf.Clamp(10 - LOD, 4, 10);

            //generate different noise depending on the planet type
            CelestialType type = (planet.type is CelestialType.Moon) ? (planet.type as CelestialType.Moon).originalType : planet.type;

            if (type is CelestialType.WaterPlanet || type is CelestialType.IcePlanet)
            {
                float v1 = tn1.GetValueFractal(P);
                mask = Mathf.Pow(Mathf.Clamp(v1 + 1, 0, 1), 10); //-1 is 0, 0 is 1
                float blend = (tn3.GetValueFractal(P * 3) + 1) / 2f;
                float amp = blend * 0.8f + 0.2f;

                float w1 = 1;
                float w2 = 4;

                float v2 = tn2.GetValueRidged(P * 3, octaves) * amp;

                v2 = Mathf.Pow(v2, 3) * mask;

                v = Mathf.Clamp((v1 * w1 + v2 * w2) / (w1 + w2), -1, 1);
            }
            else if (type is CelestialType.RockPlanet)
            {

                float v1 = tn1.GetValueFractal(P);

                float w1 = 1;
                float w2 = 4;

                float v2 = Mathf.Clamp(tn2.GetValueRidged(P * 3, octaves) / 1.2f, -1, 1);
                v2 = Mathf.Pow(v2, 3);

                v = (v1 * w1 + v2 * w2) / (w1 + w2);
            }
            else if (type is CelestialType.MoltenPlanet)
            {
                float v2 = tn2.GetValueFractal(P * 5);
                v2 = Mathf.Clamp(1 - Mathf.Abs(v2), 0, 1);
                v2 = (v2 - 0.5f) * 0.7f;
                v = v2;
            }


            return 1 + (v * amplitude / planet.radius) + GetCraterValue(P) * craterStrength * mask;
        }

        /// <summary>
        /// Calculates the crater depth of a given point on a sphere
        /// </summary>
        private float GetCraterValue(Vector3 P)
        {
            P = P.normalized;
            float v = 0;
            //add all craters up
            foreach (Crater c in planet.craters)
            {
                float distance = ((c.direction - P) * planet.radius).magnitude;
                float x = Mathf.Clamp(distance / c.radius, 0, 1);
                if (x < Mathf.Sqrt(2) / 2)
                {
                    v += 2 * Mathf.Pow(x, 2) - 1;
                }
                else
                {
                    v += 55.8083f * Mathf.Pow(x, 3) - 150.6825f * Mathf.Pow(x, 2) + 134.0562f * x - 39.182f;
                }
            }
            return v;
        }

        /// <summary>
        /// Returns the color for a point on the sphere; this color is a noise used by the gpu in the shader
        /// </summary>
        public Color GetColor(Vector3 P)
        {
            //different frequencies per type
            CelestialType type = (planet.type is CelestialType.Moon) ? (planet.type as CelestialType.Moon).originalType : planet.type;

            if (type is CelestialType.RockPlanet)
            {
                P = P.normalized * frequency * 1f;
            }
            else
            {
                P = P.normalized * frequency * 6f;
            }

            float r = (cn1.GetValueFractal(P) + 1f) / 2f;
            float g = 0;
            float b = 0;

            float rThreshold = 0.5f;
            float rPadding = 0.02f;
            if (r < rThreshold - rPadding)
            {
                r = 0;
            }
            else if (r > rThreshold + rPadding)
            {
                r = 1;
            }
            else
            {
                float ease = r - (rThreshold - rPadding);
                r = ease * (1f / (2 * rPadding));
            }

            return new Color(r, g, b, 1);
        }
    }
}
