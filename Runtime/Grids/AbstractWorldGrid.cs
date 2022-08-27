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


        protected sealed override void PrePointCreated(in PointWorld point, int pointIndex)
        {
            point.ChunkPosition = ChunkPosition;
        }

        protected override void PostPointCreated(in PointWorld point, int pointIndex)
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
                    Cell = new Vector2Int(candidateCell.x, candidateCell.y),
                    ChunkPosition = ChunkPosition
                };
                // PrePointCreated(point, _points.Count);
                _points.Add(point);
                _cells[FlatCoordinates(point.Cell)] = _points.Count;
                PostPointCreated(point, _points.Count);
                return _points.Count - 1;
            }

            return -1;
        }

        public bool RemovePoint(PointWorld point)
        {
            var index = FlatCoordinates(point.Cell);
            var cellValue = GetCellValue(index);
            if(cellValue == 0)
                return false;
            
            var storedPoint = GetPoint(cellValue - 1);
            
            if(storedPoint != point)
                return false;
            
            var searchSize = PointSettings.GetSearchSize(point.Radius, GridProperties);
            var regionCoordinates = RegionCoordinates.Create(
                searchSizeStart: searchSize, 
                searchSizeEnd: searchSize + 1, 
                x: point.Cell.x, y: point.Cell.y,
                startXLimit: 0, endXLimit: GridProperties.CellWidth,
                startYLimit: 0, endYLimit: GridProperties.CellHeight);

            for (int y1 = regionCoordinates.StartY; y1 < regionCoordinates.EndY; y1++)
            {
                var coordStart = y1 * GridProperties.CellWidth + regionCoordinates.StartX;
                var coordEnd = y1 * GridProperties.CellWidth + regionCoordinates.EndX;

                for (var x1 = coordStart; x1 < coordEnd; x1++)
                {
                    if(GetCellValue(x1) == cellValue)
                        SetCellValue(x1, 0);
                }
            }

            _points[cellValue - 1] = null;
            OnPointRemoved(point, cellValue);
            return true;
        }

        protected virtual void OnPointRemoved(PointWorld point, int cellValue)
        {
        }

        public void TestRemoveCellValue(int cell)
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                if(_cells[i] == cell)
                {
                    _cells[i] = 0;
                    _points[cell - 1] = null;
                }
            }
            
        }
    }
}