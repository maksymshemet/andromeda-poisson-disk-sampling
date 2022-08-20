using System;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public abstract class AbstractWorldGrid : AbstractGrid<IWorldGrid, PointWorld>, IWorldGrid
    {
        public event Action<PointWorld> OnPointCreated;
        public IWorld World { get; }
        public Vector2Int ChunkPosition { get; }

        protected AbstractWorldGrid(IWorld world, Vector2Int chunkPosition, GridProperties gridProperties, ICandidateValidator<IWorldGrid, PointWorld> candidateValidator, IPointSettings pointSettings) : base(gridProperties, candidateValidator, pointSettings)
        {
            World = world;
            ChunkPosition = chunkPosition;
        }


        protected override void PrePointCreated(in PointWorld point)
        {
            point.ChunkPosition = ChunkPosition;
        }

        protected override void PostPointCreated(in PointWorld point)
        {
            OnPointCreated?.Invoke(point);
        }

        protected override bool IsCandidateValid(int searchSize, Vector3 candidateWorldPosition, float candidateRadius, Vector2Int candidateCell)
        {
            return CandidateValidator.IsCandidateValid(this, searchSize, candidateWorldPosition, candidateRadius,
                candidateCell);
        }

        public int TrySpawnPoint(Vector3 candidateWorldPosition, float candidateRadius)
        {
            var candidateCell = CellFromPoint(candidateWorldPosition);
            var searchSize = PointSettings.GetSearchSize(candidateRadius, GridProperties);

            if (IsCandidateValid(searchSize, candidateWorldPosition, candidateRadius, 
                    new Vector2Int(candidateCell.x, candidateCell.y)))
            {
                var point = new PointWorld()
                {
                    WorldPosition = candidateWorldPosition,
                    Radius = candidateRadius - PointSettings.Margin,
                    Cell = new Vector2Int(candidateCell.x, candidateCell.y)
                };
                PrePointCreated(point);
                _points.Add(point);
                _cells[FlatCoordinates(point.Cell)] = _points.Count;
                PostPointCreated(point);
                return _points.Count - 1;
            }

            return -1;
        }
    }
}