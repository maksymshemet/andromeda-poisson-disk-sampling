using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public class MultiRadWorldGrid : AbstractMultiRadGrid, IWorldGrid
    {
        public IWorld World { get; }
        public Vector2Int ChunkPosition { get; }

        public MultiRadWorldGrid(IWorld world, Vector2Int chunkPosition, float minRadius, float maxRadius)
            : base(world.GridProperties, minRadius, maxRadius)
        {
            World = world;
            ChunkPosition = chunkPosition;
        }

        protected override bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, float candidateRadius, Vector2Int candidateCell)
        {
            return CandidateValidator.IsCandidateValid(this, searchSize, 
                candidateWorldPosition, candidateRadius, candidateCell, World, ChunkPosition);
        }

        protected override void OnPointCreated(ref Point point)
        {
            PointFiller.FillPoints(this, point, World, ChunkPosition);
        }
    }
}