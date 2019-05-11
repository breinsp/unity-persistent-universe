using Assets.External;
using Assets.Utils;
using UnityEngine;

namespace Assets.StarSystem.Generation
{
    public class ProceduralSkybox
    {
        Color color1;
        Color color2;
        Color color3;
        FastNoise fastNoise;

        int size;

        public ProceduralSkybox(int seed, int size)
        {
            seed = seed + 1;
            this.size = size;
            System.Random random = new System.Random(seed);
            color1 = Utility.GetRandomColor(random);
            color2 = Utility.GetRandomColor(random);
            color3 = Utility.GetRandomColor(random);

            fastNoise = new FastNoise(seed);
            fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            fastNoise.SetFractalType(FastNoise.FractalType.FBM);
            fastNoise.SetFrequency(1f);
            fastNoise.SetFractalOctaves(8);
        }

        public Texture2D GenerateTexture()
        {
            int width = 2 * size;
            int height = size;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float u = x / (width - 1f);
                    float v = y / (height - 1f);

                    Vector3 P = Utility.UVToSphere(new Vector2(u, v));
                    Vector3 Q = Utility.UVToSphere(new Vector2(u + 0.5f, v));

                    Color c = GetColorForPoint(P, Q);
                    texture.SetPixel(x, y, c);
                }
            }
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        private Color GetColorForPoint(Vector3 p, Vector3 q)
        {
            p = p.normalized * 2;
            float mask = Mathf.Pow(fastNoise.GetValueFractal(q.x, q.y, q.z), 2);
            p *= 2;
            float f1 = fastNoise.GetValueFractal(p.x, p.y, p.z);
            float f2 = (1f + f1) / 2f;
            f1 = (1 - Mathf.Abs(f1));

            float f3 = (1 - f2) / 2f;

            return mask * (color1 * f1 + color2 * f2 + f3 * color3) * 0.3f;
        }
    }
}
