using Assets.StarSystem.Generation;
using Assets.Utils;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Assets.Galaxy
{

    public class GalaxyController : MonoBehaviour
    {
        public int seed;
        public float innerRadius;
        public float outerRadius;
        public int starCountPerSegment;
        public int segmentCount;
        public float proportion;
        public int maxAngle;
        public Texture2D starSpectrum;
        public Material starMaterial;

        private System.Random random;
        private int starCount;

        float innerA;
        float innerB;
        float outerA;
        float outerB;
        
        // Use this for initialization
        void Start()
        {
            random = new System.Random(seed);
            GenerateGalaxy();
        }

        public void ResetGalaxy()
        {
            Utility.DestroyAllChildren(transform);
            GenerateGalaxy();
        }

        private void GenerateGalaxy()
        {
            innerA = innerRadius;
            innerB = innerA * proportion;
            outerA = outerRadius;
            outerB = outerA * proportion;
            GenerateEllipseSegments();
        }

        private void GenerateEllipseSegments()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float t = i / (segmentCount - 1f);
                float a = Mathf.Lerp(innerA, outerA, t);
                float b = Mathf.Lerp(innerB, outerB, t);
                float angle = Mathf.Lerp(0, maxAngle, t);
                GenerateEllipse(a, b, angle, i);
            }
        }

        private void GenerateEllipse(float a, float b, float angle, int segment)
        {
            GameObject orbitContainer = new GameObject("Orbit " + segment);
            orbitContainer.transform.parent = transform;
            orbitContainer.transform.Rotate(Vector3.up, angle);

            for (int i = 0; i < starCountPerSegment; i++)
            {
                CreateStar(orbitContainer.transform, a, b, (float)Utility.GetRandomBetween(random, 0, 360), segment);
            }
        }
        private void CreateStar(Transform parent, float a, float b, float angle, int segment)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * a;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * b;

            float deviationA = (outerA - innerA) / segmentCount;
            float deviationB = (outerB - innerB) / segmentCount;

            x += (float)Utility.GetRandomBetween(random, -deviationA, deviationA) * 0.9f;
            z += (float)Utility.GetRandomBetween(random, -deviationB, deviationB) * 0.9f;

            float abs = new Vector3(x, 0, z).magnitude;
            float s = Mathf.Clamp((abs - innerB) / (outerB - innerB), 0, 1);

            float maxheight = GetGalaxyHeight(s) * 10;
            float y = (float)Utility.GetRandomBetween(random, -maxheight, maxheight);

            int starSeed = starCount * 10;
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.transform.parent = parent;
            star.transform.localPosition = new Vector3(x, y, z);
            star.transform.localRotation = Quaternion.identity;
            CelestialType starType = new CelestialType.Star();
            double temperature = Utility.GetRandomBetween(new System.Random(starSeed), starType.minTemperature, starType.maxTemperature);

            float temp_f = Mathf.Clamp((float)(temperature - starType.minTemperature) / (starType.maxTemperature - starType.minTemperature), 0, 1);
            Color color = starSpectrum.GetPixel((int)(temp_f * starSpectrum.width), 0);

            star.name = starSeed.ToString();
            Material mat = new Material(starMaterial);
            mat.SetColor("_Color", Color.black);
            mat.SetColor("_EmissionColor", color + Color.white);
            star.GetComponent<Renderer>().material = mat;
            float scale = (float)Utility.GetRandomBetween(new System.Random(starSeed), 0.5, 1);
            star.transform.localScale = new Vector3(scale, scale, scale);
            starCount++;
        }

        public float GetGalaxyHeight(float x)
        {
            return Mathf.Sin(x * Mathf.PI);
        }
    }

    [CustomEditor(typeof(GalaxyController))]
    class GalaxyControllerEditor : Editor
    {
        SerializedProperty seed;
        SerializedProperty innerRadius;
        SerializedProperty outerRadius;
        SerializedProperty starCountPerSegment;
        SerializedProperty segmentCount;
        SerializedProperty proportion;
        SerializedProperty maxAngle;
        SerializedProperty starSpectrum;
        SerializedProperty starMaterial;

        void OnEnable()
        {
            seed = serializedObject.FindProperty("seed");
            innerRadius = serializedObject.FindProperty("innerRadius");
            outerRadius = serializedObject.FindProperty("outerRadius");
            starCountPerSegment = serializedObject.FindProperty("starCountPerSegment");
            segmentCount = serializedObject.FindProperty("segmentCount");
            proportion = serializedObject.FindProperty("proportion");
            maxAngle = serializedObject.FindProperty("maxAngle");
            starSpectrum = serializedObject.FindProperty("starSpectrum");
            starMaterial = serializedObject.FindProperty("starMaterial");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate"))
            {
                (target as GalaxyController).ResetGalaxy();
            }
            serializedObject.Update();
            EditorGUILayout.PropertyField(seed);
            EditorGUILayout.PropertyField(innerRadius);
            EditorGUILayout.PropertyField(outerRadius);
            EditorGUILayout.PropertyField(starCountPerSegment);
            EditorGUILayout.PropertyField(segmentCount);
            EditorGUILayout.PropertyField(proportion);
            EditorGUILayout.PropertyField(maxAngle);
            EditorGUILayout.PropertyField(starSpectrum);
            EditorGUILayout.PropertyField(starMaterial);
            serializedObject.ApplyModifiedProperties();
        }
    }
}