using System.Collections.Generic;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet
{
    /// <summary>
    /// Stores information about the mesh
    /// </summary>
    public class MeshData
    {
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] normals;
        public Vector2[] uv;
        public Color[] colors;

        public Vector3[] unitVertices;
        public Vector3[] unitNormals;
    }

    /// <summary>
    /// Stores information of faces neighbouring a vertex
    /// </summary>
    public class VertexData
    {
        public List<int> indexes;
        public List<Face> faces;
    }

    /// <summary>
    /// Stores triangle indexes of a single face
    /// </summary>
    public class Face
    {
        public int iA;
        public int iB;
        public int iC;
    }
}
