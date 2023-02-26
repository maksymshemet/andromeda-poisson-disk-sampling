using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public abstract class WorldAbstract
    {
        public GridProperties GridProperties { get; }

        protected WorldAbstract(GridProperties gridProperties)
        {
            GridProperties = gridProperties;
        }
    }
    
    public abstract class WorldAbstract<TWorldUserProperties, TWorldGrid, TSelf> : WorldAbstract
        where TWorldUserProperties : PointProperties
        where TSelf : WorldAbstract<TWorldUserProperties, TWorldGrid, TSelf>
        where TWorldGrid : WorldGridAbstract<TSelf, TWorldUserProperties, TWorldGrid>
    {
        public event Action<TSelf, TWorldGrid, PointWorld> OnPointCreated;
        
        public TWorldUserProperties UserProperties { get; }
        public IReadOnlyDictionary<Vector2Int, TWorldGrid> Grids => _grids;
        
        private readonly Dictionary<Vector2Int, TWorldGrid> _grids;
        private readonly Dictionary<Vector2Int, Dictionary<PointWorld, List<Vector2Int>>> _pointsForFutureGrids;
        
        public WorldAbstract(GridProperties gridProperties, TWorldUserProperties userProperties) : base(gridProperties)
        {
            _grids = new Dictionary<Vector2Int, TWorldGrid>();
            _pointsForFutureGrids = new Dictionary<Vector2Int, Dictionary<PointWorld, List<Vector2Int>>>();
            
            UserProperties = userProperties;
        }

        public TWorldGrid CreateGrid(Vector2Int chunkPosition)
        {
            if (_grids.ContainsKey(chunkPosition))
                throw new ArgumentException($"Grid with chunk position {chunkPosition} already exists");

            var chunkCenter = new Vector3(
                GridProperties.Center.x + (chunkPosition.x * GridProperties.Size.x),
                GridProperties.Center.y + (chunkPosition.y * GridProperties.Size.y),
                GridProperties.Center.z);
            
            var chunkPos = new Vector3(
                (chunkPosition.x * GridProperties.Size.x),
                (chunkPosition.y * GridProperties.Size.y),
                GridProperties.Center.z);
            
            var chunkProperties = new WorldChunkProperties
            {
                WorldCenter = chunkCenter,
                WorldPosition = chunkPos
            };
            
            // var grid = new WorldGrid2(this, chunkPosition, chunkProperties);
            TWorldGrid grid = CreateGrid(chunkPosition, chunkProperties);
            
            _grids[chunkPosition] = grid;

            if (_pointsForFutureGrids.TryGetValue(chunkPosition,
                    out Dictionary<PointWorld, List<Vector2Int>> pointAndCellCoordinates))
            {
                foreach (KeyValuePair<PointWorld,List<Vector2Int>>  pointCoordinates in pointAndCellCoordinates)
                {
                    foreach (Vector2Int cellCoordinate in pointCoordinates.Value)
                    {
                        grid.AddPoint(pointCoordinates.Key, cellCoordinate);
                    }
                }

                _pointsForFutureGrids.Remove(chunkPosition);
            }
            
            grid.OnPointCreated += (g, p) =>
            {
                OnPointCreated?.Invoke((TSelf) this, g, p);
            };
            return grid;
        }

        protected abstract TWorldGrid CreateGrid(Vector2Int chunkPosition, WorldChunkProperties chunkProperties);
        
        public Vector2Int ChunkCoordinatesFromWorldPosition(Vector3 worldPosition)
        {
            int chunkX = Mathf.FloorToInt(worldPosition.x / GridProperties.Size.x);
            int chunkY = Mathf.FloorToInt(worldPosition.y / GridProperties.Size.y);
            return new Vector2Int(chunkX, chunkY);
        }

        public WorldCoordinates WorldPositionToWorldCoordinates(Vector3 worldPosition,
            WorldToCellPositionMethod method = WorldToCellPositionMethod.Round)
        {
            Vector2Int cellPosition = GetCellPositionFrom(worldPosition, method);
            return new WorldCoordinates(this, cellPosition);
        }

        public HashSet<PointWorld> GetPointsAround(in PointWorld pointWorld, int region)
        {
            return GetPointsAround(pointWorld.CellMinWorld, pointWorld.CellMaxWorld, region);
        }
        
        public HashSet<PointWorld> GetPointsAround(in WorldCoordinates cellMin, in WorldCoordinates cellMax, int region)
        {
            WorldCoordinates from = cellMin.Offset(this, -region, -region);
            WorldCoordinates to = cellMax.Offset(this, region, region);

            var result = new HashSet<PointWorld>();

            for (int chunkY = from.ChunkPosition.y; chunkY <= to.ChunkPosition.y; chunkY++)
            {
                for (int chunkX = from.ChunkPosition.x; chunkX <= to.ChunkPosition.x; chunkX++)
                {
                    int cellStartY = chunkY == from.ChunkPosition.y ? from.CellPosition.y : 0;
                    int cellEndY = chunkY == to.ChunkPosition.y ? to.CellPosition.y : GridProperties.CellLenghtY - 1;

                    for (; cellStartY <= cellEndY; cellStartY++)
                    {
                        int cellStartX = chunkX == from.ChunkPosition.x ? from.CellPosition.x : 0;
                        int cellEndX = chunkX == to.ChunkPosition.x ? to.CellPosition.x : GridProperties.CellLenghtX - 1;

                        for (; cellStartX <= cellEndX; cellStartX++)
                        {
                            if (_grids.TryGetValue(new Vector2Int(chunkX, chunkY), out TWorldGrid grid))
                            {
                                PointWorld point = grid.GetPoint(cellStartX, cellStartY);
                                if (point != null)
                                {
                                    result.Add(point);
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        private Vector2Int GetCellPositionFrom(Vector3 worldPosition,
            WorldToCellPositionMethod method = WorldToCellPositionMethod.Round)
        {
            float x = worldPosition.x / GridProperties.CellSize;
            float y = worldPosition.y / GridProperties.CellSize;

            return method switch
            {
                WorldToCellPositionMethod.Round => new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)),
                WorldToCellPositionMethod.Ceil => new Vector2Int(Mathf.CeilToInt(x), Mathf.CeilToInt(y)),
                WorldToCellPositionMethod.Floor => new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y)),
                WorldToCellPositionMethod.Clamped => new Vector2Int(
                    x: Mathf.Max(0, Mathf.Min(Mathf.RoundToInt(x), GridProperties.CellLenghtX - 1)),
                    y: Mathf.Max(0, Mathf.Min(Mathf.RoundToInt(y), GridProperties.CellLenghtY - 1))),
                
                _ => throw new NotImplementedException($"method={method.ToString()} not implemented")
            };
        }

        internal void AddPointToGrid(WorldCoordinates wc, in PointWorld point)
        {
            if (Grids.TryGetValue(wc.ChunkPosition, out TWorldGrid grid))
            {
                grid.AddPoint(point, wc.CellPosition);
                return;
            }

            if (!_pointsForFutureGrids.TryGetValue(wc.ChunkPosition, 
                    out Dictionary<PointWorld, List<Vector2Int>> pointsPerGrid))
            {
                pointsPerGrid = new Dictionary<PointWorld, List<Vector2Int>>();
                _pointsForFutureGrids[wc.ChunkPosition] = pointsPerGrid;
            }

            if (!pointsPerGrid.TryGetValue(point, out List<Vector2Int> cellCoordinates))
            {
                cellCoordinates = new List<Vector2Int>();
                pointsPerGrid[point] = cellCoordinates;
            }
            
            cellCoordinates.Add(wc.CellPosition);
        }
    }
}