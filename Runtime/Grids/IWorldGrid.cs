using System;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public interface IWorldGrid : IGridAbstract<PointWorld>
    {
        event Action<PointWorld> OnPointCreated;
        
        public IWorld World { get; }
        
        Vector2Int ChunkPosition { get; }
        int TrySpawnPoint(Vector3 candidateWorldPosition, float candidateRadius);
    }
}