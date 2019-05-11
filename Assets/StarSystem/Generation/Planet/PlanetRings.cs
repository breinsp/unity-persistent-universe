using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet
{
    public class PlanetRings : MonoBehaviour
    {

        public SystemController controller;
        public CelestialBody planet;
        public System.Random random;

        public void Init(SystemController controller, CelestialBody planet)
        {
            this.controller = controller;
            this.planet = planet;
            random = new System.Random(planet.seed);
            Material mat = new Material(controller.materialController.ringMaterial);

            //set shader variables
            mat.SetColor("_Color1", Utility.GetRandomColorInHSVRange(random, 0.1f, 0.1f, 0, 1, 0, 1));
            mat.SetColor("_Color2", Utility.GetRandomGrayColor(random));
            mat.SetTexture("_MainTex", GetRingTexture());
            mat.SetVector("_PlanetPosition", planet.position);
            mat.SetFloat("_PlanetRadius", planet.radius);
            mat.SetFloat("_Frequency", (float)Utility.GetRandomBetween(random, 15, 50));

            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.AddComponent<MeshRenderer>().material = mat;
            gameObject.AddComponent<MeshFilter>().mesh = GetRingMesh();

            //copy and rotate 180 to see the backside
            GameObject copy = Instantiate(gameObject);
            copy.transform.parent = gameObject.transform;
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.Euler(180, 0, 0);
            copy.transform.localScale = Vector3.one;
        }

        private Texture2D GetRingTexture()
        {
            int width = 256;
            float frq1 = (float)Utility.GetRandomBetween(random, 1, 100);
            float frq2 = (float)Utility.GetRandomBetween(random, 1, 10);

            Texture2D tex = new Texture2D(1, width);

            for (int i = 0; i < width; i++)
            {
                float f = i / (width - 1f);
                float s = Mathf.Sqrt(Mathf.Sqrt(Mathf.Sin(f * Mathf.PI)));
                float p1 = (Mathf.PerlinNoise(f * frq1, planet.seed) + 1) / 2;
                float p2 = (Mathf.PerlinNoise(f * frq2, planet.seed + 200) + 1) / 2;
                float p = (p1 + p2) / 2;
                float r = Mathf.Clamp(s * p, 0, 1);
                if (r < 0.3) r = 0;
                tex.SetPixel(0, i, new Color(r, r, r, 0));
            }
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Generates the mesh for a ring
        /// </summary>
        private Mesh GetRingMesh()
        {
            float inner = (float)Utility.GetRandomBetween(random, 1, 2);
            float outer = (float)Utility.GetRandomBetween(random, inner + .5, inner + 2);

            inner *= planet.radius;
            outer *= planet.radius;

            int segments = 256;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();

            Vector3 center = transform.position;
            float angle, x, y, z;

            //add vertices, normals and uvs for first row
            angle = 0;
            for (int i = 0; i < segments; i++)
            {
                x = Mathf.Sin(Mathf.Deg2Rad * angle) * inner;
                y = 0f;
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * inner;

                vertices.Add(new Vector3(x, y, z));
                normals.Add(Vector3.up);
                uv.Add(new Vector2(angle / 360f, 0));

                angle += (360f / segments);
            }

            //add vertices, normals and uvs for second row
            angle = 0;
            for (int i = 0; i < segments; i++)
            {
                x = Mathf.Sin(Mathf.Deg2Rad * angle) * outer;
                y = 0f;
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * outer;

                vertices.Add(new Vector3(x, y, z));
                normals.Add(Vector3.up);
                uv.Add(new Vector2(angle / 360f, 1));

                angle += (360f / segments);
            }

            //add triangles for first row
            for (int i = 0; i < segments - 1; i++)
            {
                triangles.AddRange(new int[] { i, i + segments, i + segments + 1 });
            }
            triangles.AddRange(new int[] { segments - 1, (2 * segments) - 1, segments });

            //add triangles for first row
            for (int i = 0; i < segments - 1; i++)
            {
                triangles.AddRange(new int[] { i + segments + 1, i + 1, i });
            }
            triangles.AddRange(new int[] { segments, 0, segments - 1 });

            //apply mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uv.ToArray();

            return mesh;
        }

        /// <summary>
        /// Update the planet radius in the shader
        /// </summary>
        public static void UpdatePlanetRadius(GameObject gameObject, float radius)
        {
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_PlanetRadius", radius);
            foreach (Transform t in gameObject.transform)
            {
                UpdatePlanetRadius(t.gameObject, radius);
            }
        }
    }
}