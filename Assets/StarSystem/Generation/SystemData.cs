using Assets.StarSystem.Generation.CelestialTypes;
using Assets.StarSystem.Generation.CelestialTypes.PlanetTypes;
using Assets.StarSystem.Generation.Planet;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.StarSystem.Generation
{
    public class SystemData
    {
        int seed;
        SystemController controller;
        GameObject systemScale;
        GameObject planetScale;
        List<KeyValuePair<CelestialBody, GameObject>> localCopies;

        public SystemData(SystemController controller, int seed)
        {
            this.controller = controller;
            this.seed = seed;
        }

        /// <summary>
        /// Generates a system with all celestial bodies
        /// </summary>
        public void GenerateSystem()
        {
            System.Random random = new System.Random(seed);
            controller.celestials = new List<CelestialBody>();

            systemScale = new GameObject("System Scale");
            systemScale.transform.parent = controller.gameObject.transform;
            planetScale = new GameObject("Planet Scale");
            planetScale.SetActive(false);
            planetScale.transform.parent = controller.gameObject.transform;

            SpawnStar(seed + 1);
            int planetCount = (int)Math.Ceiling(Utility.GetRandomBetween(random, 1, 5));

            for (int i = 0; i < planetCount; i++)
            {
                SpawnPlanet(seed + 2 + i);
            }
        }

        /// <summary>
        /// Spawns a star at the center
        /// </summary>
        public void SpawnStar(int seed)
        {
            GameObject go = new GameObject();
            go.transform.parent = systemScale.transform;
            var star = new Star(controller, seed, go);
            controller.celestials.Add(star);
            Shader.SetGlobalVector("_SunPos", Vector3.zero);
        }

        /// <summary>
        /// Spawns a planet
        /// </summary>
        public void SpawnPlanet(int seed, CelestialBody host = null)
        {
            bool hasHost = host != null;

            System.Random random = new System.Random(seed);
            GameObject go = new GameObject();
            go.transform.parent = systemScale.transform;

            CelestialBody body;
            CelestialType type = hasHost ? CelestialType.GetRandomPlanetTypeExceptGas(random) : CelestialType.GetRandomPlanetType(random);

            if (type is CelestialType.GasPlanet)
            {
                body = new GasPlanet(controller, go, seed, host);
                int satellites = (int)Utility.GetRandomBetween(random, 1, 2.9);
                for (int i = 0; i < satellites; i++)
                {
                    SpawnPlanet(seed + 100 + i, body);
                }
            }
            else if (type is CelestialType.WaterPlanet)
            {
                body = new WaterPlanet(controller, go, seed, host);
                AddAtmosphere(body);
            }
            else if (type is CelestialType.RockPlanet)
            {
                body = new RockPlanet(controller, go, seed, host);
                AddAtmosphere(body);
            }
            else if (type is CelestialType.MoltenPlanet)
            {
                body = new MoltenPlanet(controller, go, seed, host);
                AddAtmosphere(body);
            }
            else
            {
                body = new IcePlanet(controller, go, seed, host);
                AddAtmosphere(body);
            }

            if (host == null && random.NextDouble() < 0.3)
            {
                GameObject rings = new GameObject("rings");
                rings.transform.parent = go.transform;
                var ringsScript = rings.AddComponent<PlanetRings>();
                ringsScript.Init(controller, body);
            }
            controller.celestials.Add(body);
        }

        /// <summary>
        /// Zoom in on a planet
        /// </summary>
        public void EnterPlanet(CelestialBody celestialBody)
        {
            controller.zoomed = celestialBody;
            celestialBody.zoomed = true;
            systemScale.SetActive(false);
            planetScale.SetActive(true);
            Utility.DestroyAllChildren(planetScale.transform);
            var localCopy = GameObject.Instantiate(celestialBody.gameObject, Vector3.zero, Quaternion.identity, planetScale.transform);
            localCopy.name = "Local Copy of " + celestialBody.type.ToString();
            localCopy.transform.localRotation = Quaternion.identity;
            localCopy.transform.localScale = Vector3.one * controller.planetScaleMultiplier;

            if (celestialBody is DynamicPlanet)
            {
                Utility.DestroyAllChildren(localCopy.transform, "region");
                (celestialBody as DynamicPlanet).regions.ForEach(r => r.Destroy());
            }
            var rings = localCopy.transform.Find("rings");
            if (rings != null)
            {
                PlanetRings.UpdatePlanetRadius(rings.gameObject, celestialBody.radius * controller.planetScaleMultiplier);
            }

            celestialBody.originalGameObject = celestialBody.gameObject;
            celestialBody.gameObject = localCopy;

            TeleportPlayerIn(celestialBody);
            CopyRelativeCelestials(celestialBody);
            UpdateVolumetricAtmosphere(celestialBody);
        }

        /// <summary>
        /// teleport player to zoomed in planet
        /// </summary>
        private void TeleportPlayerIn(CelestialBody celestialBody)
        {
            GameObject player = GameObject.Find("Player");

            Vector3 planet_pos = celestialBody.position;
            Vector3 player_pos = player.transform.position;

            Vector3 player_to_planet = planet_pos - player_pos;
            float mag = player_to_planet.magnitude;
            player_to_planet = celestialBody.rotation * player_to_planet;

            Vector3 new_pos = Vector3.zero + player_to_planet.normalized * mag * controller.planetScaleMultiplier * 0.97f;
            player.transform.position = new_pos;
            player.transform.rotation *= celestialBody.rotation;
        }

        /// <summary>
        /// zoom out of planet
        /// </summary>
        public void LeavePlanet(CelestialBody celestialBody)
        {
            controller.zoomed = null;
            celestialBody.zoomed = false;
            celestialBody.gameObject = celestialBody.originalGameObject;

            systemScale.SetActive(true);
            planetScale.SetActive(false);
            celestialBody.zoomed = false;

            TeleportPlayerOut(celestialBody);
            UpdateVolumetricAtmosphere(celestialBody);
            Shader.SetGlobalVector("_SunPos", Vector3.zero);
        }

        /// <summary>
        /// teleport player out to system scale again
        /// </summary>
        private void TeleportPlayerOut(CelestialBody celestialBody)
        {
            GameObject player = GameObject.Find("Player");
            player.GetComponent<Rigidbody>().velocity = Vector3.zero;

            Vector3 planet_pos = Vector3.zero;
            Vector3 player_pos = player.transform.position;

            Vector3 player_to_planet = player_pos - planet_pos;
            float mag = player_to_planet.magnitude;
            float mult = mag / celestialBody.radius / controller.planetScaleMultiplier;

            Quaternion inverse = Quaternion.Inverse(celestialBody.rotation);
            player_to_planet = inverse * player_to_planet;
            Vector3 new_pos = celestialBody.position + player_to_planet.normalized * mult * celestialBody.radius * 1.03f;
            player.transform.position = new_pos;
            player.transform.rotation *= inverse;
        }

        /// <summary>
        /// copy celestial bodies to their relative position when a planet gets zoomed, due to float coordinate limitations
        /// </summary>
        public void CopyRelativeCelestials(CelestialBody celestialBody)
        {
            localCopies = new List<KeyValuePair<CelestialBody, GameObject>>();
            foreach (CelestialBody c in controller.celestials)
            {
                if (c != celestialBody)
                {
                    GameObject copy = GameObject.Instantiate(c.gameObject, Vector3.zero, Quaternion.identity, planetScale.transform);
                    localCopies.Add(new KeyValuePair<CelestialBody, GameObject>(c, copy));

                    Light light = copy.GetComponent<Light>();
                    if (light != null)
                    {
                        light.type = LightType.Directional;
                    }
                }
            }
        }

        private void UpdateRelativePositions()
        {
            Vector3 player = GetWorldPlayerPosition();

            foreach (var kv in localCopies)
            {
                CelestialBody celestialBody = kv.Key;
                GameObject copy = kv.Value;
                CelestialBody zoomed = controller.zoomed;

                Quaternion inverse = Quaternion.Inverse(zoomed.rotation);
                Shader.SetGlobalVector("_SkyboxRotation", zoomed.rotation.eulerAngles);

                Vector3 planetToPlanet = celestialBody.position - zoomed.position;
                planetToPlanet = inverse * planetToPlanet;

                Vector3 rotatedCelestialBodyPosition = zoomed.position + planetToPlanet;                
                
                Vector3 realDistance = rotatedCelestialBodyPosition - player;
                float realMagnitude = realDistance.magnitude;
                float v_realMagnitude = Mathf.Clamp((realMagnitude + 2000f) / 14000f, 0, 1); //0 - 1
                v_realMagnitude = Mathf.Pow(v_realMagnitude, .1f);//move close objects farther away

                float newMagnitude = 90000 * v_realMagnitude; //max unity coords
                Vector3 newDistance = realDistance.normalized * newMagnitude;
                float factor = newMagnitude / realMagnitude;

                Vector3 relativePos = controller.playerPosition + newDistance;
                celestialBody.relativePosition = relativePos;
                copy.transform.position = relativePos;
                copy.transform.localScale = Vector3.one * factor;

                foreach (Transform child in copy.transform)
                {
                    if (child.name == "rings")
                    {
                        PlanetRings.UpdatePlanetRadius(child.gameObject, celestialBody.radius * factor);
                    }
                }

                if (celestialBody.type is CelestialType.Star)
                {
                    Shader.SetGlobalVector("_SunPos", celestialBody.relativePosition);
                    Vector3 from = Vector3.forward;
                    Vector3 to = Vector3.zero - celestialBody.relativePosition;
                    copy.transform.rotation = Quaternion.FromToRotation(from, to); //Directional Light
                }
            }
        }

        public void UpdatePositions()
        {
            foreach (var c in controller.celestials)
            {
                c.UpdatePositionInSystem();
            }

            if (controller.zoomed != null)
            {
                UpdateRelativePositions();
            }
        }

        private Vector3 GetWorldPlayerPosition()
        {
            Vector3 relativePlayer = controller.playerPosition;
            Vector3 world = controller.zoomed.position + relativePlayer / controller.planetScaleMultiplier;
            return world;
        }

        /// <summary>
        /// add a volumetric atmosphere to a celestial body
        /// </summary>
        public void AddAtmosphere(CelestialBody celestialBody)
        {
            GameObject atmo = new GameObject("atmosphere");
            atmo.AddComponent<MeshFilter>().mesh = Octahedron.Create(5, 1);
            Material material = controller.materialController.GetMaterialForVolumetricAtmosphere(celestialBody);
            material.SetColor("_Color1", controller.materialController.GetColor1ForAtmosphere(celestialBody));
            material.SetColor("_Color2", controller.materialController.GetColor2ForAtmosphere(celestialBody));
            atmo.transform.parent = celestialBody.gameObject.transform;
            atmo.transform.localScale = Vector3.one * celestialBody.radius * celestialBody.atmosphereRadiusMultiplier;
            atmo.transform.localPosition = Vector3.zero;
            atmo.AddComponent<MeshRenderer>().material = material;
        }

        /// <summary>
        /// update the volumetric atmosphere shader parameters when a planet gets zoomed in/out
        /// </summary>
        public void UpdateVolumetricAtmosphere(CelestialBody celestialBody)
        {
            Transform atmo = celestialBody.gameObject.transform.Find("atmosphere");
            if (atmo != null)
            {
                Material material = atmo.GetComponent<MeshRenderer>().material;
                controller.materialController.UpdateVolumetricAtmosphereMaterial(material, celestialBody);
            }
        }
    }
}
