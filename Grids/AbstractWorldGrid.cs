using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class AbstractWorldGrid : AbstractGrid<IWorldGrid, PointWorld>, IWorldGrid
    {
        public IWorld World { get; }
        public Vector2Int ChunkPosition { get; }

        protected AbstractWorldGrid(IWorld world, Vector2Int chunkPosition, GridProperties gridProperties, ICandidateValidator<IWorldGrid, PointWorld> candidateValidator, IPointSettings pointSettings) : base(gridProperties, candidateValidator, pointSettings)
        {
            World = world;
            ChunkPosition = chunkPosition;
        }

        protected override void OnPointCreated(in PointWorld point)
        {
            point.ChunkPosition = ChunkPosition;
        }

        protected override bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, float candidateRadius, Vector2Int candidateCell)
        {
            return CandidateValidator.IsCandidateValid(this, searchSize, candidateWorldPosition, candidateRadius,
                candidateCell);
        }
    }
}