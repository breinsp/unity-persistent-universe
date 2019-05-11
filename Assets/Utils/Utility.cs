using System;
using System.Linq;
using UnityEngine;

namespace Assets.Utils
{
    public class Utility
    {
        public static Vector3 RandomVector(System.Random random)
        {
            float x = (float)random.NextDouble() * 2f - 1f;
            float y = (float)random.NextDouble() * 2f - 1f;
            float z = (float)random.NextDouble() * 2f - 1f;
            return new Vector3(x, y, z).normalized;
        }

        public static Color GetRandomColorBetween(System.Random random, Color c1, Color c2)
        {
            return Color.Lerp(c1, c2, (float)random.NextDouble());
        }

        public static Color GetRandomColor(System.Random random)
        {
            float r = (float)random.NextDouble();
            float g = (float)random.NextDouble();
            float b = (float)random.NextDouble();
            return new Color(r, g, b, 1);
        }

        public static Color GetRandomGrayColor(System.Random random)
        {
            float g = (float)GetRandomBetween(random, 0.3f, 0.8f);
            return new Color(g, g, g, 1);
        }

        public static Color GetRandomColorInRange(System.Random random, int r1, int r2, int g1, int g2, int b1, int b2)
        {
            float r = (float)GetRandomBetween(random, r1, r2) / 255f;
            float g = (float)GetRandomBetween(random, g1, g2) / 255f;
            float b = (float)GetRandomBetween(random, b1, b2) / 255f;
            return new Color(r, g, b, 1);
        }

        public static int RandomIntRange(System.Random random, int v1, int v2)
        {
            return (int)Math.Round(GetRandomBetween(random, v1, v2));
        }

        public static Color GetRandomColorInHSVRange(System.Random random, float h1, float h2, float s1, float s2, float v1, float v2)
        {
            float h = (float)GetRandomBetween(random, h1, h2);
            float s = (float)GetRandomBetween(random, s1, s2);
            float v = (float)GetRandomBetween(random, v1, v2);
            return Color.HSVToRGB(h, s, v);
        }

        public static Color GetRandomStrongColor(System.Random random)
        {
            var c = GetRandomColor(random);

            var r = random.NextDouble();

            if (r < 0.33)
            {
                c.r = 1;
            }
            else if (r < 0.66)
            {
                c.g = 1;
            }
            else
            {
                c.b = 1;
            }
            return c;
        }

        public static double GetRandomBetween(System.Random random, double min, double max)
        {
            double d = random.NextDouble();
            return d * (max - min) + min;
        }

        public static Quaternion GetRandomQuaternion(System.Random random)
        {
            float x = (float)random.NextDouble();
            float y = (float)random.NextDouble();
            float z = (float)random.NextDouble();
            float w = (float)random.NextDouble();
            return new Quaternion(x, y, z, w);
        }

        public static void DestroyAllChildren(Transform transform, string name = "")
        {
            foreach (Transform t in transform.transform.Cast<Transform>().ToArray())
            {
                if (name == "" || t.name == name)
                {
                    GameObject.Destroy(t.gameObject);
                }
            }
        }

        public static Vector3 UVToSphere(Vector2 uv)
        {
            float u = uv.x;
            float v = uv.y;

            float theta = 2 * Mathf.PI * u - Mathf.PI;
            float phi = Mathf.PI * v;

            float x = Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = -Mathf.Cos(phi);
            float z = Mathf.Sin(theta) * Mathf.Sin(phi);

            return new Vector3(x, y, z);
        }
    }
}
