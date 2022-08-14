using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public interface IWorldGrid : IGrid
    {
        public IWorld World { get; }
        
        Vector2Int ChunkPosition { get; }
    }
}