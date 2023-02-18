using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public abstract class WorldAbstract<TWorldUserProperties, TWorldGrid, TSelf>
        where TWorldUserProperties : PointProperties
        where TSelf : WorldAbstract<TWorldUserProperties, TWorldGrid, TSelf>
        where TWorldGrid : WorldGridAbstract<TSelf, TWorldUserProperties, TWorldGrid>
    {
        public event Action<TWorldGrid, PointWorld> OnPointCreated;
        
        public GridProperties GridProperties { get; }
        public TWorldUserProperties UserProperties { get; }
        public IReadOnlyDictionary<Vector2Int, TWorldGrid> Grids => _grids;
        
        private readonly Dictionary<Vector2Int, TWorldGrid> _grids;
        private readonly Dictionary<Vector2Int, Dictionary<PointWorld, List<Vector2Int>>> _pointsForFutureGrids;
        
        public WorldAbstract(GridProperties gridProperties, TWorldUserProperties userProperties)
        {
            _grids = new Dictionary<Vector2Int, TWorldGrid>();
            _pointsForFutureGrids = new Dictionary<Vector2Int, Dictionary<PointWorld, List<Vector2Int>>>();
            
            GridProperties = gridProperties;
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
            var grid = CreateGrid(chunkPosition, chunkProperties);
            
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
            
            grid.OnPointCreated += (a, b) =>
            {
                OnPointCreated?.Invoke(a, b);
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
            return RelativeToWorldCoordinates(cellPosition);
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


        public WorldCoordinates NewWorldCoordinatesWithOffset(WorldCoordinates original,
            int cellOffsetX, int cellOffsetY)
        {
            
            Tuple<int, int> Calc(int originalCell, int originalChunk, int length, int offset)
            {
                int cellX = originalCell + offset;
                int chunkX = originalChunk;
                if (cellX > 0)
                {
                    if (cellX >= length)
                    {
                        int chunkOffset = cellX / length;
                        int cellOffset = cellX % length;

                        chunkX += chunkOffset;
                        cellX = cellOffset;
                    }
                }
                else if(cellX < 0)
                {
                    int chunkOffset = cellX / length - 1;
                    int cellOffset = cellX % length;

                    chunkX += chunkOffset;
                    cellX = length - Mathf.Abs(cellOffset) - 1;
                }

                return Tuple.Create(chunkX, cellX);
            }

            Tuple<int, int> tupleX = Calc(
                originalCell: original.CellPosition.x,
                originalChunk: original.ChunkPosition.x,
                length: GridProperties.CellLenghtX,
                offset: cellOffsetX);
            
            Tuple<int, int> tupleY = Calc(
                originalCell: original.CellPosition.y,
                originalChunk: original.ChunkPosition.y,
                length: GridProperties.CellLenghtY,
                offset: cellOffsetY);

            return new WorldCoordinates
            {
                ChunkPosition = new Vector2Int(x: tupleX.Item1, y: tupleY.Item1),
                CellPosition = new Vector2Int(x: tupleX.Item2, y: tupleY.Item2),
            };
        }
        
        public WorldCoordinates RelativeToWorldCoordinates(Vector2Int cellPosition)
            => RelativeToWorldCoordinates(cellPosition, Vector2Int.zero);
        
        public WorldCoordinates RelativeToWorldCoordinates(Vector2Int cellPosition, Vector2Int chunkPosition)
        {
            Tuple<int, int> Calc(int originalCell, int originalChunk, int length, int offset)
            {
                int cellX = originalCell + offset;
                int chunkX = originalChunk;
                if (cellX > 0)
                {
                    if (cellX >= length)
                    {
                        int chunkOffset = cellX / length;
                        int cellOffset = cellX % length;

                        chunkX += chunkOffset;
                        cellX = cellOffset;
                    }
                }
                else if(cellX < 0)
                {
                    int chunkOffset = cellX / length - 1;
                    int cellOffset = cellX % length;

                    chunkX += chunkOffset;
                    cellX = length - Mathf.Abs(cellOffset) - 1;
                }

                return Tuple.Create(chunkX, cellX);
            }
            
            Tuple<int, int> tupleX = Calc(
                originalCell: 0,
                originalChunk: chunkPosition.x,
                length: GridProperties.CellLenghtX,
                offset: cellPosition.x);
            
            Tuple<int, int> tupleY = Calc(
                originalCell: 0,
                originalChunk: chunkPosition.y,
                length: GridProperties.CellLenghtY,
                offset: cellPosition.y);
            
            return new WorldCoordinates
            {
                ChunkPosition = new Vector2Int(x: tupleX.Item1, y: tupleY.Item1),
                CellPosition = new Vector2Int(x: tupleX.Item2, y: tupleY.Item2),
            };
        }

        public int GetCellValue(WorldCoordinates currentCoord)
        {
            if (_grids.TryGetValue(currentCoord.ChunkPosition, out TWorldGrid grid))
            {
                return grid.GetCellValue(
                    currentCoord.CellPosition.x,
                    currentCoord.CellPosition.y);
            }

            return 0;
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