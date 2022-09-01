using System;
using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class WorldMultiRad
    {
        public IRadius Radius { get; }
        public Vector3 WorldPositionOffset { get; }
        public IGridProperties GridProperties { get; }
        public IEnumerable<GridWorld> Grids => _grids.Values;
        public int Tries { get; set; }
        public float Margin { get; set; }

        private readonly Dictionary<Vector2Int, GridWorld> _grids;
        private readonly Dictionary<Vector2Int, Dictionary<Vector2Int, PointWorld>> _cachedPoints;

        public WorldMultiRad(IRadius radius, Vector3 worldPositionOffset, IGridProperties gridProperties)
        {
            Radius = radius;
            WorldPositionOffset = worldPositionOffset;
            GridProperties = gridProperties;
            _grids = new Dictionary<Vector2Int, GridWorld>();
            _cachedPoints = new Dictionary<Vector2Int,Dictionary<Vector2Int, PointWorld>>();
        }

        public GridWorld CreateGrid(Vector2Int position)
        {
            if(_grids.ContainsKey(position))
                throw new Exception("Grid already exists");
            
            var grid = Factory.CreateWorldGrid(world: this, chunkPosition: position);
            _grids[position] = grid;

            if (_cachedPoints.TryGetValue(position, out var pointWorlds))
            {
                foreach (var pair in pointWorlds)
                {
                    var cellCoord = pair.Key;
                    var point = pair.Value;
                    grid.LinkPoint(cellCoord.x, cellCoord.y, point);
                }
            }

            return grid;
        }

        public GridWorld GetGrid(Vector2Int chunk)
        {
            return _grids.TryGetValue(chunk, out var grid) ? grid : null;
        }

        public PointWorld GetPoint(WorldCoordinate worldCoordinate, bool excludeCached = false, bool excludeLinked = false)
        {
            return GetPoint(worldCoordinate.ChunkPosition, worldCoordinate.CellX, worldCoordinate.CellY, excludeCached, excludeLinked);
        }

        public PointWorld GetPoint(Vector2Int chunk, int x, int y, bool excludeCached = false, bool excludeLinked = false)
        {
            if (_grids.TryGetValue(chunk, out var grid))
            {
                return grid.GetPoint(x, y, excludeLinked);
            }
            
            if (excludeCached) return null;

            if (_cachedPoints.TryGetValue(chunk, out var gridPoints))
            {
                if (gridPoints.TryGetValue(new Vector2Int(x, y), out var point))
                    return point;
            }
            
            return null;
        }

        public WorldCoordinate GetRealWorldCoordinate(Vector2Int grid, int x, int y)
        {
            return GetRealWorldCoordinate(grid.x, grid.y, x, y);
        }

        public WorldCoordinate GetRealWorldCoordinate(int gridX, int gridY, int cellX, int cellY)
        {
            GetGridCoordinate(cellX, GridProperties.CellWidth - 1, gridX, 
                out var targetGridX, out var targetCellX);
            GetGridCoordinate(cellY, GridProperties.CellHeight - 1, gridY,
                out var targetGridY, out var targetCellY);
            
            return new WorldCoordinate(new Vector2Int(targetGridX, targetGridY), targetCellX, targetCellY);
        }

        public bool TrySpawnPointFrom(PointWorld spawnPoint, out PointWorld result)
        {
            for (var i = 0; i < Tries; i++)
            {
                var radius = Radius.GetRadius(i, Tries);
                if (radius == 0) continue;

                var grid = TryCreateCandidate(spawnPoint.WorldPosition, 
                    spawnPoint.Radius, radius, out var candidate);
                if(grid.TrySpawnPointNear(candidate, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }

        public void LinkPoint(WorldCoordinate coordinate, PointWorld point)
        {
            if (_grids.TryGetValue(coordinate.ChunkPosition, out var grid))
            {
                grid.LinkPoint(coordinate.CellX, coordinate.CellY, point);
            }
            
            if (_cachedPoints.TryGetValue(coordinate.ChunkPosition, out var gridPoints))
            {
                gridPoints[new Vector2Int(coordinate.CellX, coordinate.CellY)] = point;
            }
            else
            {
                _cachedPoints[coordinate.ChunkPosition] = new Dictionary<Vector2Int, PointWorld>
                {
                    [new Vector2Int(coordinate.CellX, coordinate.CellY)] = point
                };
            }
        }

        public void RemoveLink(WorldCoordinate worldCoordinate, PointWorld point)
        {
            if (_grids.TryGetValue(worldCoordinate.ChunkPosition, out var grid))
            {
                grid.RemoveLink(worldCoordinate.CellX, worldCoordinate.CellY, point);
            }
            else
            {
                if(_cachedPoints.TryGetValue(worldCoordinate.ChunkPosition, out var cachedPoints))
                {
                    cachedPoints.Remove(new Vector2Int(worldCoordinate.CellX, worldCoordinate.CellY));
                }
            }
        } 
        
        private GridWorld TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, float radius,
            out Candidate candidate)
        {
            var position = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: spawnerPosition,
                    spawnerRadius: spawnerRadius,
                    candidateRadius: radius);


            var gridPosition = WorldPositionToGridPosition(position);
            if (!_grids.TryGetValue(gridPosition, out var grid))
            {
                grid = CreateGrid(gridPosition);
            }

            if (!grid.GridCore.IsPointInAABB(position))
            {
                candidate = default;
                return null;
            }
            
            candidate = new Candidate
            {
                Radius = radius,
                WorldPosition = position,
                Cell = grid.GridCore.PositionToCellClamped(position)
            };
            
            return grid;
        }
        
        private Vector2Int WorldPositionToGridPosition(Vector3 worldPosition)
        {
            return new Vector2Int(Mathf.FloorToInt((worldPosition.x - WorldPositionOffset.x) / GridProperties.Size.x),
                Mathf.FloorToInt((worldPosition.y - WorldPositionOffset.y) / GridProperties.Size.y));
        }
        
        private void GetGridCoordinate(int rawCoord, int gridLenght, int chunkPosition, out int chunkCoord, out int cellCoord)
        {
            if (rawCoord >= 0 && rawCoord <= gridLenght)
            {
                chunkCoord = chunkPosition;
                cellCoord = rawCoord;
                return;
            }
            
            var shiftNegative = rawCoord < 0;
            var shiftCount = shiftNegative ? Mathf.Abs(rawCoord) : Mathf.Abs(rawCoord - gridLenght);
            var shiftChunks = (shiftCount / (gridLenght)) + 1;

            chunkCoord = shiftNegative ? chunkPosition - shiftChunks : chunkPosition + shiftChunks;
            
            var xDelta = Mathf.Abs(rawCoord) - ((shiftChunks - 1) * (gridLenght));
            cellCoord = shiftNegative ? gridLenght - xDelta + 1  : xDelta - gridLenght - 1;
        }
    }
}