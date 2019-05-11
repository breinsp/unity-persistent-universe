using Assets.StarSystem.Generation.Planet;
using System.Collections.Generic;

namespace Assets.Utils
{
    public class RegionStack
    {
        private List<Region> regions;

        public RegionStack()
        {
            regions = new List<Region>();
        }

        public void Add(Region region)
        {
            if (!region.processing)
            {
                RemoveChildrenAndParentsFromStack(region);
                regions.Add(region);
            }
        }

        private void RemoveChildrenAndParentsFromStack(Region region)
        {
            Region parent = region.parent;
            while (parent != null)
            {
                if (regions.Contains(parent)) regions.Remove(parent);
                parent = parent.parent;
            }
            RemoveChildren(region);
        }

        private void RemoveChildren(Region region)
        {
            if (regions.Contains(region))
            {
                regions.Remove(region);
            }
            if (region.IsLeaf) return;
            for (int i = 0; i < region.children.Length; i++)
            {
                RemoveChildren(region.children[i]);
            }
        }

        public int Count { get { return regions.Count; } }

        public Region Remove()
        {
            if (regions.Count == 0) return null;
            Region last = null;
            lock (regions)
            {
                int index = regions.Count - 1;
                last = regions[index];
                regions.RemoveAt(index);
            }
            return last;
        }
    }
}
