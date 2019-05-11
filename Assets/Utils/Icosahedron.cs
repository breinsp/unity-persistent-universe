using UnityEngine;

namespace Assets.Utils
{
    public class Icosahedron
    {
        public static void GetIco(ref Vector3[] vertices, ref int[] triangles, int subdivision)
        {
            GetIco(ref vertices, ref triangles);

            Vector3[] newVertices = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                newVertices[i] = vertices[triangles[i]];
                newVertices[i + 1] = vertices[triangles[i + 1]];
                newVertices[i + 2] = vertices[triangles[i + 2]];
                triangles[i] = i;
                triangles[i + 1] = i + 1;
                triangles[i + 2] = i + 2;
            }
            vertices = newVertices;

            SubdivideSurface(ref vertices, ref triangles, subdivision);
        }

        public static void GetIco(ref Vector3[] vertices, ref int[] triangles)
        {
            float t = (float)((1.0 + Mathf.Sqrt(5f)) / 2.0);
            vertices = new Vector3[12];

            vertices[0] = new Vector3(-1, t, 0);
            vertices[1] = new Vector3(1, t, 0);
            vertices[2] = new Vector3(-1, -t, 0);
            vertices[3] = new Vector3(1, -t, 0);
            vertices[4] = new Vector3(0, -1, t);
            vertices[5] = new Vector3(0, 1, t);
            vertices[6] = new Vector3(0, -1, -t);
            vertices[7] = new Vector3(0, 1, -t);
            vertices[8] = new Vector3(t, 0, -1);
            vertices[9] = new Vector3(t, 0, 1);
            vertices[10] = new Vector3(-t, 0, -1);
            vertices[11] = new Vector3(-t, 0, 1);

            triangles = new int[] { 0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8, 3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1 };
        }

        public static Vector2 RadialCoords(Vector3 P)
        {
            var Pn = P.normalized;
            float lon = Mathf.Atan2(Pn.z, Pn.x);
            float lat = Mathf.Acos(Pn.y);
            Vector2 uv = new Vector2(lon, lat) * (1f / Mathf.PI);
            uv = new Vector2(uv.x * 0.5f + 0.5f, 1 - uv.y);
            return uv;
        }

        private static void SubdivideSurface(ref Vector3[] vertices, ref int[] triangles, int detail)
        {
            for (int i = 0; i < detail; i++)
            {
                int len = vertices.Length * 4;
                Vector3[] newVertices = new Vector3[len];
                int[] newTriangles = new int[len];

                int index = 0;

                for (int j = 0; j < triangles.Length; j += 3)
                {
                    int t1 = triangles[j];
                    int t2 = triangles[j + 1];
                    int t3 = triangles[j + 2];

                    Vector3 a = vertices[t1];
                    Vector3 b = vertices[t2];
                    Vector3 c = vertices[t3];

                    Vector3 ab = ((a + b) / 2);
                    Vector3 bc = ((b + c) / 2);
                    Vector3 ca = ((c + a) / 2);

                    CreateTriangle(ref newVertices, ref newTriangles, index, a, ab, ca);
                    index += 3;
                    CreateTriangle(ref newVertices, ref newTriangles, index, ab, b, bc);
                    index += 3;
                    CreateTriangle(ref newVertices, ref newTriangles, index, bc, c, ca);
                    index += 3;
                    CreateTriangle(ref newVertices, ref newTriangles, index, ab, bc, ca);
                    index += 3;
                }
                triangles = newTriangles;
                vertices = newVertices;
            }
        }
        private static void CreateTriangle(ref Vector3[] vertices, ref int[] triangles, int index, Vector3 a, Vector3 b, Vector3 c)
        {
            vertices[index] = a;
            vertices[index + 1] = b;
            vertices[index + 2] = c;
            triangles[index] = index;
            triangles[index + 1] = index + 1;
            triangles[index + 2] = index + 2;
        }
    }
}
