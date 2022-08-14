using System;
using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Services;
using dd_andromeda_poisson_disk_sampling.Worlds;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public class World : IWorld
    {
        public GridProperties GridProperties { get; }
        public IPointSettings PointSettings { get; set; }
        public Vector2 ChunkSize { get; }
        public IEnumerable<IWorldGrid> Grids => _chunks.Values;
        
        private readonly Dictionary<Vector2Int, IWorldGrid> _chunks;
        private readonly Dictionary<Vector2Int, Dictionary<PointWorld, HashSet<Vector2Int>>> _pointsPerChunk;
        
        public World(GridProperties gridProperties)
        {
            _chunks = new Dictionary<Vector2Int, IWorldGrid>();
            _pointsPerChunk = new Dictionary<Vector2Int, Dictionary<PointWorld, HashSet<Vector2Int>>>();

            GridProperties = gridProperties;
            
            ChunkSize = new Vector2(
                x:gridProperties.CellSize * gridProperties.CellWidth,
                y:gridProperties.CellSize * gridProperties.CellHeight);
        }

        public List<PointWorld> CreateGrid(Vector2Int chunkPosition, bool fill = true)
        {
            var grid = Factory.CreateWorldGrid(world: this, chunkPosition: chunkPosition);
           
            _chunks[chunkPosition] = grid;

            if (_pointsPerChunk.TryGetValue(chunkPosition, out var points))
            {
                foreach (var cellPerPoints in points)
                {
                    var point = cellPerPoints.Key;
                    var cells = cellPerPoints.Value;
                    
                    var cellValue = 0;
                    foreach (var cell in cells)
                    {
                        if (cellValue == 0)
                        {
                            grid.AddPoint(cell.x, cell.y, point);
                            cellValue = grid.GetCellValue(cell.x, cell.y);
                        }
                        else
                        {
                            grid.SetCellValue(cell.x, cell.y, cellValue);
                        }
                    }
                }
                _pointsPerChunk.Remove(chunkPosition);
            }
            if (!fill)
                return new List<PointWorld>();
            return grid.Fill();
        }

        public IWorldGrid GetGrid(Vector2Int vector2Int)
        {
            return _chunks.TryGetValue(vector2Int, out var i) ? i : null;
        }

        public PointWorld GetPoint(Vector2Int chunkCoord, int cellX, int cellY) =>
            GetPoint(chunkCoord.x, chunkCoord.y, cellX, cellY);
        
        public PointWorld GetPoint(int chunkX, int chunkY, int cellX, int cellY)
        {
            var worldCoord = GetRealWorldCoordinate(chunkX, chunkY, cellX, cellY);
            return _chunks.TryGetValue(worldCoord.ChunkPosition, out var chunk) 
                ? chunk.GetPoint(worldCoord.CellX, worldCoord.CellY) : default;
        }

        public WorldCoordinate GetRealWorldCoordinate(Vector2Int chunkPosition, int x, int y)
        {
            return GetRealWorldCoordinate(chunkPosition.x, chunkPosition.y, x, y);
        }
        
        public WorldCoordinate GetRealWorldCoordinate(int chunkX, int chunkY, int cellX, int cellY)
        {
            GetChunkCoordinate(cellX, GridProperties.CellWidth - 1, chunkX, 
                out var targetChunkX, out var targetCellX);
            GetChunkCoordinate(cellY, GridProperties.CellHeight - 1, chunkY,
                out var targetChunkY, out var targetCellY);
            
            return new WorldCoordinate(new Vector2Int(targetChunkX, targetChunkY), targetCellX, targetCellY);
        }

        public int AddPoint(WorldCoordinate coordinate, PointWorld point)
        {
            if (_chunks.TryGetValue(coordinate.ChunkPosition, out var chunk))
            {
                return chunk.AddPoint(coordinate.CellX, coordinate.CellY, point);
            }

            if (!_pointsPerChunk.TryGetValue(coordinate.ChunkPosition, out var points))
            {
                points = new Dictionary<PointWorld, HashSet<Vector2Int>>
                {
                    [point] = new HashSet<Vector2Int> { new Vector2Int(coordinate.CellX, coordinate.CellY) }
                };
                _pointsPerChunk[coordinate.ChunkPosition] = points;
            }

            if (points.TryGetValue(point, out var cells))
            {
                cells.Add(new Vector2Int(coordinate.CellX, coordinate.CellY));
            }
            else
            {
                points[point] = new HashSet<Vector2Int> { new Vector2Int(coordinate.CellX, coordinate.CellY) };
            }
            
            return 0;
        }

        public PointWorld TrySpawnPointFrom(PointWorld spawnPoint)
        {
            if (_chunks.TryGetValue(spawnPoint.ChunkPosition, out var chunk))
            {
                return chunk.TrySpawnPointFrom(spawnPoint);
            }

            return null;
        }

        private void GetChunkCoordinate(int rawCoord, int gridLenght, int chunkPosition, out int chunkCoord, out int cellCoord)
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