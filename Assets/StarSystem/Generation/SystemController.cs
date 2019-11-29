using Assets.StarSystem.Generation.Planet;
using Assets.StarSystem.Generation.Planet.Threads;
using Assets.StarSystem.UI;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.StarSystem.Generation
{
    public class SystemController : MonoBehaviour
    {
        public int seed = 0;

        public List<BaseThread> threads;
        public List<Exception> threadExceptions;
        public RegionStack regionCalculationStack;
        public PriorityDictionary<Region> regionRenderQueue;
        public PriorityDictionary<Region> regionPlannedDestroyQueue;
        public PriorityDictionary<Region> regionDestroyQueue;

        public List<CelestialBody> celestials;
        public SystemData systemData;

        [HideInInspector]
        public float[] LODdistances;
        public int MaxLod { get { return LODdistances.Length - 1; } }
        [HideInInspector]
        public Vector3 playerPosition;
        [HideInInspector]
        public Quaternion playerRotation;
        public MaterialController materialController;
        public bool showLodColors = false;

        public float planetScaleMultiplier = 1000;
        [HideInInspector]
        public CelestialBody zoomed = null;

        public PlanetHud planetHud;

        private Text text;

        // Use this for initialization
        void Start()
        {
            if (seed < 0)
            {
                seed = (int)(new System.Random().NextDouble() * 1000000.0);
            }
            LODdistances = new float[] { 0.005f, 0.03f, 0.1f, 0.3f, 0.5f, 1f, 2f, 5f, 20f, 30f };
            threads = new List<BaseThread>();
            threadExceptions = new List<Exception>();
            regionCalculationStack = new RegionStack();
            regionRenderQueue = new PriorityDictionary<Region>();
            regionPlannedDestroyQueue = new PriorityDictionary<Region>();
            regionDestroyQueue = new PriorityDictionary<Region>();
            celestials = new List<CelestialBody>();
            UpdatePlayerPosition();
            StartThreads();
            Texture2D skybox = new ProceduralSkybox(seed, 256).GenerateTexture();
            materialController.skyboxMaterial.SetTexture("_MainTex", skybox);

            systemData = new SystemData(this, seed);
            systemData.GenerateSystem();
            text = GameObject.Find("Debug").GetComponent<Text>();
            planetHud.GeneratePlanetHuds();
        }

        Stopwatch watch;
        void Update()
        {
            watch = Stopwatch.StartNew();
            UpdatePlayerPosition();
            systemData.UpdatePositions();
            UpdateQueues();
        }
        
        private void OnDestroy()
        {
            StopThreads();
        }


        /// <summary>
        /// Process all main thread queues
        /// </summary>
        void UpdateQueues()
        {
            if (watch.ElapsedMilliseconds < 16) //>60 fps, maintain smooth performance
            {
                string str = "";
                str += "\nExceptions: " + threadExceptions.Count;
                str += "\nCalculating: " + regionCalculationStack.Count;
                str += "\nRender: " + regionRenderQueue.Count;
                str += "\nPlannedDestroy: " + regionPlannedDestroyQueue.Count;
                str += "\nDestroy: " + regionDestroyQueue.Count;
                text.text = str;

                bool idle = true;
                if (threadExceptions.Count > 0)
                {
                    idle = false;
                    Exception e;
                    lock (threadExceptions)
                    {
                        e = threadExceptions[0];
                        threadExceptions.RemoveAt(0);
                    }
					UnityEngine.Debug.Log(e.ToString());
					UnityEngine.Debug.Log(e.StackTrace);

				}
                else if (regionRenderQueue.Count > 0)
                {
                    idle = false;
                    Region region;
                    lock (regionRenderQueue)
                    {
                        region = regionRenderQueue.Remove();
                    }
                    RenderRegion(region);
                }
                else if (regionDestroyQueue.Count > 0)
                {
                    idle = false;
                    Region region;
                    lock (regionDestroyQueue)
                    {
                        region = regionDestroyQueue.Remove();
                    }
                    if (region.gameObject != null)
                    {
                        Destroy(region.gameObject);
                        region.gameObject = null;
                    }
                }

                //if queues had entries, repeat process
                if (!idle)
                {
                    UpdateQueues();
                }
            }
        }

        /// <summary>
        /// Spawns a region in the world
        /// </summary>
        void RenderRegion(Region region)
        {
            if (region.gameObject == null)
            {
                GameObject parent = region.planet.gameObject;
                region.gameObject = new GameObject("region");
                region.gameObject.transform.parent = parent.transform;
                region.gameObject.transform.localPosition = Vector3.zero;
                Mesh mesh = region.gameObject.AddComponent<MeshFilter>().mesh;
                region.gameObject.AddComponent<MeshRenderer>().sharedMaterial = materialController.GetMaterialForDynamicPlanet(region);
                region.gameObject.transform.localScale = Vector3.one;

                mesh.vertices = region.meshData.vertices;
                mesh.triangles = region.meshData.triangles;
                mesh.normals = region.meshData.normals;
                mesh.uv = region.meshData.uv;
                mesh.colors = region.meshData.colors;

                if (region.LOD <= 1)
                {
                    var meshCollider = region.gameObject.AddComponent<MeshCollider>();
                    var pm = new PhysicMaterial
                    {
                        dynamicFriction = 1f,
                        staticFriction = 1f,
                        bounciness = 0f,
                        frictionCombine = PhysicMaterialCombine.Minimum,
                        bounceCombine = PhysicMaterialCombine.Minimum
                    };
                    meshCollider.sharedMesh = mesh;
                    meshCollider.sharedMaterial = pm;
                }

                if (region.planet.type.hasLava || region.planet.type.hasWater || region.planet.type.hasIce)
                {
                    string name = "";
                    Material material;

                    if (region.planet.type.hasLava)
                    {
                        name = "lava";
                        material = materialController.lavaMaterial;
                    }
                    else if (region.planet.type.hasWater)
                    {
                        name = "water";
                        material = materialController.waterMaterial;
                    }
                    else
                    {
                        name = "ice";
                        material = materialController.iceMaterial;
                    }

                    GameObject unit = new GameObject(name);
                    unit.transform.parent = region.gameObject.transform;
                    unit.AddComponent<MeshRenderer>().material = material;
                    Mesh unitMesh = new Mesh
                    {
                        vertices = region.meshData.unitVertices,
                        triangles = region.meshData.triangles,
                        normals = region.meshData.unitNormals,
                        uv = region.meshData.uv
                    };
                    unit.AddComponent<MeshFilter>().mesh = unitMesh;
                    unit.transform.localScale = Vector3.one;
                    unit.transform.localPosition = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Starts all threads
        /// </summary>
        void StartThreads()
        {
            threads.Add(new LODSchedulerThread(this));
            for (int i = 0; i < 1; i++) threads.Add(new DestroySchedulerThread(this));
            for (int i = 0; i < 4; i++) threads.Add(new LODCalculatorThread(this));
            threads.ForEach(t => t.Start());
        }


        /// <summary>
        /// Stops all threads
        /// </summary>
        private void StopThreads()
        {
            threads.ForEach(t => t.Stop());
            threads.Clear();
        }

        Stopwatch zoomCooldown;

        /// <summary>
        /// checks if player is flying close to planet and if zoom is required
        /// </summary>
        public void UpdatePlayerPosition()
        {
            playerPosition = GameObject.Find("Player").transform.position;
            playerRotation = Camera.main.transform.rotation;

            if (zoomCooldown == null || zoomCooldown.ElapsedMilliseconds > 10 * 1000)
            {
                celestials.ForEach(c =>
                {
                    if (!(c.type is CelestialType.Star) && !(c.type is CelestialType.GasPlanet))
                    {
                        if (!c.InsideTransition && !c.zoomed)
                        {
                            systemData.EnterPlanet(c);
                            zoomCooldown = Stopwatch.StartNew();
                        }
                        else if (c.InsideTransition && c.zoomed)
                        {
                            systemData.LeavePlanet(c);
                            zoomCooldown = Stopwatch.StartNew();
                        }
                    }
                });
            }
        }
    }
}