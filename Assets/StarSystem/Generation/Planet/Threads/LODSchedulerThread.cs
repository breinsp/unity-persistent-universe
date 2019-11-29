using Assets.StarSystem.Generation.CelestialTypes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.StarSystem.Generation.Planet.Threads
{
    public class LODSchedulerThread : BaseThread
    {
        //The LOD scheduler thread is responsible for dividing up the regions in their required lods

        public LODSchedulerThread(SystemController system) : base(system)
        {
        }

        protected override void Action()
        {
            controller.celestials.ForEach(celestial =>
            {
                if (celestial is DynamicPlanet)
                {
                    var planet = celestial as DynamicPlanet;
                    planet.regions.ForEach(region =>
                    {
                        SubdivideRegion(region);
                    });
                }
            });
            System.Threading.Thread.Sleep(20);
        }

        private void SubdivideRegion(Region region)
        {
            float zoomFactor = region.planet.zoomed ? controller.planetScaleMultiplier : 1;
            bool farAway = region.planet.FarAway;

            int lod = controller.LODdistances.Length - 1;
            for (; lod >= 0; lod--)
            {
                bool render = false;
                //if the planet is far away, it can be rendered on lowest detail
                if (farAway)
                {
                    render = true;
                }
                else
                {
                    float minDist = GetMinDistanceToRegion(controller.playerPosition, region);
                    float maxDist = GetMaxDistanceToRegion(controller.playerPosition, region);

                    float previousLODrange = (lod == 0 ? 0 : controller.LODdistances[lod - 1]) * region.planet.radius * zoomFactor;
                    float nextLODrange = controller.LODdistances[lod] * region.planet.radius * zoomFactor;

                    //check if region lies within lod range
                    if (maxDist > previousLODrange && minDist < nextLODrange)
                    {
                        render = true;
                    }
                }
                if (render)
                {
                    if (region.LOD > lod)
                    {
                        if (region.gameObject != null)
                        {
                            //destroy region if lod is too low
                            lock (controller.regionPlannedDestroyQueue)
                            {
                                controller.regionPlannedDestroyQueue.Add(region.Key, region, region.LOD);
                            }
                        }
                        if (region.IsLeaf)
                        {
                            //create subregions unitl desired depth is reached
                            region.CreateChildren();
                        }
                        //iterate children and repeat process
                        for (int i = 0; i < region.children.Length; i++)
                        {
                            SubdivideRegion(region.children[i]);
                        }
                    }
                    else
                    {
                        //depth is correct, region can be calculated
                        CalculateRegion(region);
                        RemoveParentsAndChildren(region);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Destroys parents and children of a region
        /// </summary>
        private void RemoveParentsAndChildren(Region region)
        {
            Region parent = region.parent;
            while (parent != null)
            {
                if (parent.gameObject != null)
                {
                    lock (controller.regionPlannedDestroyQueue)
                    {
                        controller.regionPlannedDestroyQueue.Add(region.parent.Key, region.parent, region.parent.LOD);
                    }
                }
                parent = parent.parent;
            }
            if (!region.IsLeaf)
            {
                region.DestroyChildrenWithGameObjects();
            }
        }

        /// <summary>
        /// Adds a region to the calculation queue
        /// </summary>
        private void CalculateRegion(Region region)
        {
            if (region.gameObject == null && !region.processing)
            {
                //float minDist = GetMinDistanceToRegion(controller.playerPosition, region);
                //check if region is within visible area of screen
                //if (minDist < region.planet.MaximumVisibleDistance)
                //{
                    lock (controller.regionCalculationStack)
                    {
                        controller.regionCalculationStack.Add(region);
                    }
                //}
            }
        }

        private float GetMinDistanceToRegion(Vector3 pos, Region region)
        {
            Vector3 planet_pos = region.planet.zoomed ? Vector3.zero : region.planet.position;
            float zoomFactor = region.planet.zoomed ? controller.planetScaleMultiplier : 1;
            return (new List<Vector3>(new Vector3[] { region.A_mod, region.B_mod, region.C_mod })).Min(V => ((planet_pos + V * zoomFactor) - pos).magnitude);
        }

        private float GetMaxDistanceToRegion(Vector3 pos, Region region)
        {
            Vector3 planet_pos = region.planet.zoomed ? Vector3.zero : region.planet.position;
            float zoomFactor = region.planet.zoomed ? controller.planetScaleMultiplier : 1;
            return (new List<Vector3>(new Vector3[] { region.A_mod, region.B_mod, region.C_mod })).Max(V => ((planet_pos + V * zoomFactor) - pos).magnitude);
        }
    }
}