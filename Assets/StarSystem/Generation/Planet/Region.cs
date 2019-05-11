using Assets.StarSystem.Generation.CelestialTypes;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet
{
    public class Region
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        /// <summary>
        /// depth in the region tree
        /// </summary>
        public int depth;

        /// <summary>
        /// the amount of subdivisions made per region
        /// </summary>
        public const int detailLevel = 4;

        public DynamicPlanet planet;
        public Region parent;
        public Region[] children;

        public MeshData meshData;
        public GameObject gameObject;

        public Vector3 A_mod { get { return A * planet.terrain.GetNoise(A, LOD); } }
        public Vector3 B_mod { get { return B * planet.terrain.GetNoise(B, LOD); } }
        public Vector3 C_mod { get { return C * planet.terrain.GetNoise(C, LOD); } }

        public bool processing;

        public Region(DynamicPlanet planet, Region parent, Vector3 A, Vector3 B, Vector3 C, int depth)
        {
            this.planet = planet;
            this.parent = parent;
            this.A = A;
            this.B = B;
            this.C = C;
            this.depth = depth;
        }

        /// <summary>
        /// calculates a unique Key for the region
        /// </summary>
        public int Key
        {
            get
            {
                Vector3 P = A * 1000 + B * 100 + C * 10;
                return depth + (int)(P.x * 1000) + (int)(P.y * 100) + (int)(P.z * 10);
            }
        }
        
        public bool IsLeaf { get { return (children == null); } }
        public int LOD { get { return planet.controller.MaxLod - depth; } }
        public Vector3 Center { get { return (A + B + C) / 3; } }

        public void CreateChildren()
        {
            if (!IsLeaf)
            {
                throw new Exception("Region already has children");
            }
            if (depth + 1 > planet.controller.MaxLod)
            {
                throw new Exception("Maximum LOD reached!");
            }
            children = new Region[4];

            Vector3 AB = ((A + B) / 2).normalized * planet.radius;
            Vector3 BC = ((B + C) / 2).normalized * planet.radius;
            Vector3 CA = ((C + A) / 2).normalized * planet.radius;

            //subdivide region and create 4 new subregions
            children[0] = new Region(planet, this, A, AB, CA, depth + 1);
            children[1] = new Region(planet, this, AB, B, BC, depth + 1);
            children[2] = new Region(planet, this, BC, C, CA, depth + 1);
            children[3] = new Region(planet, this, AB, BC, CA, depth + 1);
        }

        /// <summary>
        /// Removes all children and destroys all gameobjects
        /// </summary>
        public void DestroyChildrenWithGameObjects()
        {
            if (IsLeaf)
            {
                throw new Exception("Region has no children");
            }
            for (int i = 0; i < children.Length; i++)
            {
                Region child = children[i];
                if (child.gameObject != null)
                {
                    //add to planned destroy queue
                    lock (planet.controller.regionPlannedDestroyQueue)
                    {
                        planet.controller.regionPlannedDestroyQueue.Add(child.Key, child, child.LOD);
                    }
                }
                if (!child.IsLeaf)
                {
                    //destroy children of children
                    child.DestroyChildrenWithGameObjects();
                }
            }

        }

        public void Destroy()
        {
            if (gameObject != null)
            {
                gameObject = null;
            }
            if (!IsLeaf)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].Destroy();
                }
            }
        }

        /// <summary>
        /// Creates the mesh for a region
        /// </summary>
        public void CreateMesh()
        {
            Vector3[] vertices = new Vector3[] { A, B, C };
            int[] triangles = new int[] { 0, 1, 2 };
            Vector3[] normals = new Vector3[] { A, B, C };

            //subdivide
            for (int i = 0; i < detailLevel; i++)
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

                    CreateTriangle(newVertices, newTriangles, index, a, ab, ca);
                    index += 3;
                    CreateTriangle(newVertices, newTriangles, index, ab, b, bc);
                    index += 3;
                    CreateTriangle(newVertices, newTriangles, index, bc, c, ca);
                    index += 3;
                    CreateTriangle(newVertices, newTriangles, index, ab, bc, ca);
                    index += 3;
                }
                triangles = newTriangles;
                vertices = newVertices;
            }
            normals = new Vector3[vertices.Length];

            Dictionary<Vector3, VertexData> vertexFaces = new Dictionary<Vector3, VertexData>(new Vector3EqualityComparer());
            List<int> borderVertices = new List<int>();
            //create bounds to the region to avoid seams between chunks
            AddBoundsAndNormals(ref vertices, ref triangles, ref normals, borderVertices);

            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            Vector3[] unitVertices = new Vector3[vertices.Length];
            Vector3[] unitNormals = new Vector3[vertices.Length];
            
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int t1 = triangles[i];
                int t2 = triangles[i + 1];
                int t3 = triangles[i + 2];
                Face face = new Face
                {
                    iA = t1,
                    iB = t2,
                    iC = t3
                };
                Vector3 a = vertices[t1].normalized;
                Vector3 b = vertices[t2].normalized;
                Vector3 c = vertices[t3].normalized;
                //unitVertices/unitNormals are the mesh data for the water surface mesh
                unitVertices[t1] = a * planet.waterLevel * planet.radius;
                unitVertices[t2] = b * planet.waterLevel * planet.radius;
                unitVertices[t3] = c * planet.waterLevel * planet.radius;
                unitNormals[t1] = a;
                unitNormals[t2] = b;
                unitNormals[t3] = c;
                //set neighbouring faces for all vertices
                AddFace(vertexFaces, a, t1, face);
                AddFace(vertexFaces, b, t2, face);
                AddFace(vertexFaces, c, t3, face);
                //modified vertices which contain height manipulation via terrain noise
                Vector3 a_mod = a * planet.radius * planet.terrain.GetNoise(a, LOD);
                Vector3 b_mod = b * planet.radius * planet.terrain.GetNoise(b, LOD);
                Vector3 c_mod = c * planet.radius * planet.terrain.GetNoise(c, LOD);
                vertices[t1] = a_mod;
                vertices[t2] = b_mod;
                vertices[t3] = c_mod;
                //get the uv coordinates for the vertices
                var uv_a = Octahedron.RadialCoords(a);
                var uv_b = Octahedron.RadialCoords(b);
                var uv_c = Octahedron.RadialCoords(c);
                //get the colors for the vertices; these just contain noise values, which are used in the shader
                colors[t1] = planet.terrain.GetColor(a);
                colors[t2] = planet.terrain.GetColor(b);
                colors[t3] = planet.terrain.GetColor(c);

                //fix texture seams around the beginning of the uv texture
                FixUvs(ref uv_a, ref uv_b, ref uv_c);
                uv[t1] = uv_a;
                uv[t2] = uv_b;
                uv[t3] = uv_c;
            }

            //calculate normals for all vertices, by calculating the direction of all neighbouring faces
            foreach (var P in vertexFaces.Keys)
            {
                VertexData vd = vertexFaces[P];
                Vector3 sum = Vector3.zero;
                //add all neighbours together
                foreach (var f in vd.faces)
                {
                    Vector3 a = vertices[f.iA];
                    Vector3 b = vertices[f.iB];
                    Vector3 c = vertices[f.iC];
                    sum += Vector3.Cross(b - a, c - a);
                }

                foreach (var i in vd.indexes)
                {
                    normals[i] = sum.normalized;
                }
            }

            //lower the overlapping bounds of a region by a bit to make terrain smoother
            foreach (var index in borderVertices)
            {
                vertices[index] *= 0.995f;
                unitVertices[index] *= 0.995f;
            }

            //save meshdata
            meshData = new MeshData()
            {
                vertices = vertices,
                normals = normals,
                triangles = triangles,
                uv = uv,
                colors = colors,
                unitVertices = unitVertices,
                unitNormals = unitNormals
            };
        }

        /// <summary>
        /// adds the neighbouring face to a vertex
        /// </summary
        private void AddFace(Dictionary<Vector3, VertexData> vertexFaces, Vector3 P, int index, Face face)
        {
            if (!vertexFaces.ContainsKey(P))
            {
                vertexFaces.Add(P, new VertexData
                {
                    indexes = new List<int>(),
                    faces = new List<Face>()
                });
            }
            VertexData vertexData = vertexFaces[P];
            vertexData.faces.Add(face);
            vertexData.indexes.Add(index);
        }

        /// <summary>
        /// generates overlapping bounds of a region
        /// </summary>
        private void AddBoundsAndNormals(ref Vector3[] vertices, ref int[] triangles, ref Vector3[] normals, List<int> borderVertices)
        {
            int verticesPerSide = 2;
            for (int i = 0; i < detailLevel; i++)
            {
                verticesPerSide += (int)Mathf.Pow(2, i);
            }
            float factor = (1f + (1f / (verticesPerSide - 1f)));

            //the offset corners of the region triangle
            Vector3 AB_mod = A + (B - A) * factor;
            Vector3 BA_mod = B + (A - B) * factor;
            Vector3 BC_mod = B + (C - B) * factor;
            Vector3 CB_mod = C + (B - C) * factor;
            Vector3 CA_mod = C + (A - C) * factor;
            Vector3 AC_mod = A + (C - A) * factor;

            int index = 0;
            int len = verticesPerSide * 3 * 6;
            Vector3[] new_vertices = new Vector3[len];
            int[] new_triangles = new int[len];
            Vector3[] new_normals = new Vector3[len];

            int indexOffset = triangles.Length;
            GenerateBound(A, B, CA_mod, CB_mod, verticesPerSide, new_vertices, new_triangles, ref index, borderVertices, indexOffset);
            GenerateBound(B, C, AB_mod, AC_mod, verticesPerSide, new_vertices, new_triangles, ref index, borderVertices, indexOffset);
            GenerateBound(C, A, BC_mod, BA_mod, verticesPerSide, new_vertices, new_triangles, ref index, borderVertices, indexOffset);

            Vector2 o = Vector2.zero;

            //generate corners
            CreateTriangle(new_vertices, new_triangles, index, A, BA_mod, CA_mod, indexOffset);
            borderVertices.AddRange(new int[] { index + indexOffset + 1, index + indexOffset + 2 });
            index += 3;
            CreateTriangle(new_vertices, new_triangles, index, B, CB_mod, AB_mod, indexOffset);
            borderVertices.AddRange(new int[] { index + indexOffset + 1, index + indexOffset + 2 });
            index += 3;
            CreateTriangle(new_vertices, new_triangles, index, C, AC_mod, BC_mod, indexOffset);
            borderVertices.AddRange(new int[] { index + indexOffset + 1, index + indexOffset + 2 });
            index += 3;

            vertices = vertices.Concat(new_vertices).ToArray();
            triangles = triangles.Concat(new_triangles).ToArray();
            normals = normals.Concat(new_normals).ToArray();
        }

        /// <summary>
        /// Generates a single side of the region bounds
        /// </summary>
        private void GenerateBound(Vector3 A, Vector3 B, Vector3 A_mod, Vector3 B_mod, int verticesPerSide, Vector3[] vertices, int[] triangles, ref int index, List<int> borderVertices, int indexOffset = 0)
        {
            for (int inner1 = 0; inner1 < verticesPerSide; inner1++)
            {
                int outer1 = inner1;
                int outer2 = inner1 + 1;
                int inner2 = inner1 - 1;

                float outer_factor1 = outer1 / (float)verticesPerSide;
                float outer_factor2 = outer2 / (float)verticesPerSide;

                float inner_factor1 = inner1 / (verticesPerSide - 1f);
                float inner_factor2 = inner2 / (verticesPerSide - 1f);

                Vector3 a = A + (B - A) * inner_factor2;
                Vector3 b = A + (B - A) * inner_factor1;

                Vector3 c = A_mod + (B_mod - A_mod) * outer_factor2;
                Vector3 d = A_mod + (B_mod - A_mod) * outer_factor1;

                CreateTriangle(vertices, triangles, index, d, c, b, indexOffset);
                borderVertices.AddRange(new int[] { index + indexOffset, index + indexOffset + 1 });
                index += 3;

                if (inner2 >= 0)
                {
                    CreateTriangle(vertices, triangles, index, a, d, b, indexOffset);
                    borderVertices.AddRange(new int[] { index + indexOffset + 1 });
                    index += 3;
                }
            }
        }

        /// <summary>
        /// Generates a triangle by setting vertices and triangles
        /// </summary>
        private void CreateTriangle(Vector3[] vertices, int[] triangles, int index, Vector3 a, Vector3 b, Vector3 c, int indexOffset = 0)
        {
            vertices[index] = a;
            vertices[index + 1] = b;
            vertices[index + 2] = c;
            triangles[index] = index + indexOffset;
            triangles[index + 1] = index + 1 + indexOffset;
            triangles[index + 2] = index + 2 + indexOffset;
        }

        /// <summary>
        /// Fixes the uv seam where the texture meets itself
        /// </summary>
        private void FixUvs(ref Vector2 uv_a, ref Vector2 uv_b, ref Vector2 uv_c)
        {
            float delta_ab = Mathf.Abs(uv_a.x - uv_b.x);
            float delta_bc = Mathf.Abs(uv_b.x - uv_c.x);
            float delta_ca = Mathf.Abs(uv_c.x - uv_a.x);

            bool a_fixed = false;
            bool b_fixed = false;
            bool c_fixed = false;

            //if a uv span is greater than 0.5, the inverse is the correct uv

            if (delta_ab > 0.5f)
            {
                if (uv_a.x < uv_b.x && !a_fixed)
                {
                    uv_a.x = 1 - uv_a.x;
                    a_fixed = true;
                }
                if (uv_b.x < uv_a.x && !b_fixed)
                {
                    uv_b.x = 1 - uv_b.x;
                    b_fixed = true;
                }
            }
            if (delta_bc > 0.5f)
            {
                if (uv_b.x < uv_c.x && !b_fixed)
                {
                    uv_b.x = 1 - uv_b.x;
                    b_fixed = true;
                }
                if (uv_c.x < uv_b.x && !c_fixed)
                {
                    uv_c.x = 1 - uv_c.x;
                    c_fixed = true;
                }
            }
            if (delta_ca > 0.5f)
            {
                if (uv_c.x < uv_a.x && !c_fixed)
                {
                    uv_c.x = 1 - uv_c.x;
                    c_fixed = true;
                }
                if (uv_a.x < uv_c.x && !a_fixed)
                {
                    uv_a.x = 1 - uv_a.x;
                    a_fixed = true;
                }
            }
        }

        public override string ToString()
        {
            return "region:lod_" + LOD + ", A:" + A.normalized + ", B:" + B.normalized + ", C:" + C.normalized;
        }
    }
}
