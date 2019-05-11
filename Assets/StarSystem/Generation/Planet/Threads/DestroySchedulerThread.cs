namespace Assets.StarSystem.Generation.Planet.Threads
{
    public class DestroySchedulerThread : BaseThread
    {
        //the destroy scheduler thread checks if a planned-destroy-region is allowed to be destroyed
        //this avoids holes in the ground, because this thread ensures that mesh is rendered before an overlapping region is destroyed

        public DestroySchedulerThread(SystemController system) : base(system)
        {
        }

        protected override void Action()
        {
            //check if planned destroy queue has entries
            if (controller.regionPlannedDestroyQueue.Count > 0)
            {
                Region region;
                lock (controller.regionPlannedDestroyQueue)
                {
                    region = controller.regionPlannedDestroyQueue.Remove();
                }

                //check if other mesh is rendered so it can be destroyed
                if (HasOtherMeshRendered(region))
                {
                    lock (controller.regionDestroyQueue)
                    {
                        controller.regionDestroyQueue.Add(region.Key, region, region.LOD);
                    }
                }
            }
            else
            {
                System.Threading.Thread.Sleep(30);
            }
        }

        /// <summary>
        /// checks if parents or children have a rendered mesh
        /// </summary>
        private bool HasOtherMeshRendered(Region region)
        {
            return HasParentsWithMesh(region) || HasChildrenWithMesh(region);
        }

        /// <summary>
        /// checks if all children have a rendered mesh
        /// </summary>
        private bool HasChildrenWithMesh(Region region)
        {
            if (!region.IsLeaf)
            {
                int c = 0;
                foreach (var child in region.children)
                {
                    if (child == null) return false;
                    if (child.gameObject != null) c++;
                    //recursively calls this method to see if children have mesh
                    else if (HasChildrenWithMesh(child)) c++;
                }
                return c == 4;
            }
            return false;
        }

        /// <summary>
        /// checks if any parent has a rendered mesh
        /// </summary>
        private bool HasParentsWithMesh(Region region)
        {
            Region parent = region.parent;
            while (parent != null)
            {
                if (parent.gameObject != null)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }
    }
}
