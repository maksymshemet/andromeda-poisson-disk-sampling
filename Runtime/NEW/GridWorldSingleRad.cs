using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridWorldSingleRad : GridWorld
    {
        public GridWorldSingleRad(GridCore gridCore, World world, Vector2Int chunkPosition) : base(gridCore, world, chunkPosition)
        {
        }


        protected override int GetSearchRange(float pointRadius)
        {
            return 3;
        }
    }
}