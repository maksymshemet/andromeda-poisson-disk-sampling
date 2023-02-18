using System;
using System.Collections.Generic;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public class WorldGrid : WorldGridAbstract<World, PointPropertiesConstRadius, WorldGrid>
    {
        public WorldGrid(World world, Vector2Int chunkCoordinate, WorldChunkProperties chunkProperties) 
            : base(world, chunkCoordinate, chunkProperties, world.UserProperties)
        {
        }

        public override void Fill()
        {
            Vector3 fakeWorldPosition = ChunkProperties.WorldCenter;

            List<int> spawnPoints;
            if (Points.Count == 0)
            {
                var fakePoint = new PointWorld
                {
                    WorldPosition = fakeWorldPosition,
                    Radius = World.UserProperties.Radius,
                    // Cell = _world2.WorldPositionToWorldCoordinates(fakeWorldPosition).CellPosition
                };

                int pointIndex = TrySpawnPointFrom(fakePoint, out PointWorld _);
                if (pointIndex == -1)
                {
                    throw new Exception("Couldn't spawn the point");
                }
            
                spawnPoints = new List<int> { pointIndex };
            }
            else
            {
                spawnPoints = Enumerable.Range(0, PointsInternal.Count).ToList();
            }
            
            do
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                int spawnPointIndex = spawnPoints[spawnIndex];
                PointWorld spawnPoint = PointsInternal[spawnPointIndex];
                
                int pointIndex = TrySpawnPointFrom(spawnPoint, out PointWorld _);
                if (pointIndex > -1)
                {
                    spawnPoints.Add(pointIndex);
                }
                else
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
                
#if UNITY_EDITOR
                if (EditorCheckForEndlessSpawn(spawnPoints)) break;
#endif
            }
            while (spawnPoints.Count > 0);
        }
        
        // public bool TrySpawnPointFrom(Point point, out Point newPoint)
        // {
        //     for (var i = 0; i < _world2.UserProperties.Tries; i++)
        //     {
        //         Candidate candidate = CreateCandidateFrom(point);
        //
        //         WorldCoordinates candidateCellMax = _world2.WorldPositionToWorldCoordinates(candidate.WorldPosition,
        //             WorldToCellPositionMethod.Ceil);
        //         WorldCoordinates candidateCellMin = _world2.WorldPositionToWorldCoordinates(candidate.WorldPosition,
        //             WorldToCellPositionMethod.Floor);
        //
        //         if (candidateCellMax.ChunkPosition != ChunkCoordinate &&
        //             candidateCellMin.ChunkPosition != ChunkCoordinate)
        //         {
        //             continue;
        //         }
        //         
        //         if (IsCandidateValid(candidate, candidateCellMax, candidateCellMin))
        //         {
        //             newPoint = new Point
        //             {
        //                 WorldPosition = candidate.WorldPosition,
        //                 Radius = candidate.Radius,
        //                 Margin = candidate.Margin
        //             };
        //             AddPoint(newPoint);
        //             return true;
        //         }
        //     }
        //
        //     newPoint = default;
        //     return false;
        // }
        
        protected override Candidate CreateCandidateFrom(PointWorld point, int currentTry)
        {
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: World.UserProperties.Radius + World.UserProperties.PointMargin,
                    candidateRadius: World.UserProperties.Radius + World.UserProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = World.UserProperties.Radius,
                Margin = World.UserProperties.PointMargin
            };
        }
        
        protected override void OnPointCreatedInternal(in PointWorld point)
        {
            Vector2Int cellMin = WorldPositionToCell(point.WorldPosition, WorldToCellPositionMethod.Floor);
            Vector2Int cellMax = WorldPositionToCell(point.WorldPosition, WorldToCellPositionMethod.Ceil);
            
            point.CellMinWorld = World.RelativeToWorldCoordinates(cellMin);
            point.CellMaxWorld = World.RelativeToWorldCoordinates(cellMax);

            PointWorld point1 = point;

            void SetCellValueLocal(WorldCoordinates wc)
            {
                if (ChunkCoordinate == wc.ChunkPosition)
                {
                    Cells[FlatCoordinates(wc.CellPosition)] = PointsInternal.Count;
                }
                else
                {
                    World.AddPointToGrid(wc, point1);
                }
            }

            SetCellValueLocal(point.CellMinWorld);
            SetCellValueLocal(point.CellMaxWorld);

            WorldCoordinates ab = World.RelativeToWorldCoordinates(new Vector2Int(cellMin.x, cellMax.y));
            WorldCoordinates ba = World.RelativeToWorldCoordinates(new Vector2Int(cellMax.x, cellMin.y));

            SetCellValueLocal(ab);
            SetCellValueLocal(ba);
        }

        // internal void AddPoint(in Point point, Vector2Int cellPosition)
        // {
        //     if (PointsInternal.Count == 0 || PointsInternal[^1] != point)
        //     {
        //         PointsInternal.Add(point);
        //     }
        //     
        //     Cells[FlatCoordinates(cellPosition)] = PointsInternal.Count;
        // }
        //
        // private bool IsCandidateValid(Candidate candidate, WorldCoordinates candidateCellMax, WorldCoordinates candidateCellMin)
        // {
        //     int radius = GetSearchSize(candidate.Radius);
        //     WorldCoordinates from = _world2
        //         .NewWorldCoordinatesWithOffset(candidateCellMin, -radius, -radius);
        //     WorldCoordinates to = _world2
        //         .NewWorldCoordinatesWithOffset(candidateCellMax, radius, radius);
        //
        //     for (int chunkY = from.ChunkPosition.y; chunkY <= to.ChunkPosition.y; chunkY++)
        //     {
        //         for (int chunkX = from.ChunkPosition.x; chunkX <= to.ChunkPosition.x; chunkX++)
        //         {
        //             if (!_world2.Grids.TryGetValue(new Vector2Int(chunkX, chunkY), out WorldGrid2 grid)) continue;
        //             
        //             int cellYStart = chunkY == from.ChunkPosition.y ? from.CellPosition.y : 0;
        //             int cellXStart = chunkX == from.ChunkPosition.x ? from.CellPosition.x : 0;
        //             
        //             int cellYEnd = chunkY == to.ChunkPosition.y ? to.CellPosition.y + 1 : GridProperties.CellLenghtY;
        //             int cellXEnd = chunkX == to.ChunkPosition.x ? to.CellPosition.x + 1 : GridProperties.CellLenghtX;
        //             
        //             for (int cellY = cellYStart; cellY < cellYEnd; cellY++)
        //             {
        //                 for (int cellX = cellXStart; cellX < cellXEnd; cellX++)
        //                 {
        //                     int pointIndex = grid.GetCellValue(cellX, cellY);
        //                     if (pointIndex == 0) continue;
        //                     
        //                     Point existingPoint = grid.GetPointByIndex(pointIndex);
        //                     if (Calculations.IsCandidateIntersectWithPoint(candidate, existingPoint))
        //                     {
        //                         return false;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     
        //     return true;
        // }
        //
        // private Candidate CreateCandidateFrom(Point point)
        // {
        //     Vector3 candidatePosition = Helper
        //         .GetCandidateRandomWorldPosition(
        //             spawnWorldPosition: point.WorldPosition,
        //             spawnerRadius: _world2.UserProperties.Radius + _world2.UserProperties.PointMargin,
        //             candidateRadius: _world2.UserProperties.Radius + _world2.UserProperties.PointMargin);
        //     
        //     return new Candidate
        //     {
        //         WorldPosition = candidatePosition,
        //         Radius = _world2.UserProperties.Radius,
        //         Margin = _world2.UserProperties.PointMargin
        //     };
        // }
        //
        // private int GetSearchSize(float pointRadius) => 
        //     Mathf.Max(3, Mathf.CeilToInt(pointRadius / _world2.GridProperties.CellSize));
        //
    }
}