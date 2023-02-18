using System;
using System.Collections.Generic;
using System.Linq;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds
{
    public class WorldGridMultiRad : WorldGridAbstract<WorldMultiRad, PointPropertiesMultiRadius, WorldGridMultiRad>
    {
        public WorldGridMultiRad(WorldMultiRad world2, Vector2Int chunkCoordinate, WorldChunkProperties chunkProperties) 
            : base(world2, chunkCoordinate, chunkProperties, world2.UserProperties)
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
                    Radius = World.UserProperties.RadiusProvider.GetRandomRadius(
                        Random.Range(0, UserProperties.Tries), UserProperties.Tries),
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
        
        protected override Candidate CreateCandidateFrom(PointWorld point, int currentTry)
        {
            float candidateRadius = UserProperties.RadiusProvider.GetRandomRadius(currentTry, UserProperties.Tries);
            Vector3 candidatePosition = Helper
                .GetCandidateRandomWorldPosition(
                    spawnWorldPosition: point.WorldPosition,
                    spawnerRadius: point.Radius + World.UserProperties.PointMargin,
                    candidateRadius: candidateRadius + World.UserProperties.PointMargin);
            
            return new Candidate
            {
                WorldPosition = candidatePosition,
                Radius = candidateRadius,
                Margin = World.UserProperties.PointMargin
            };
        }
        
        protected override void OnPointCreatedInternal(in PointWorld point)
        {
            
            int deltaRadius = Mathf.RoundToInt((point.Radius + UserProperties.PointMargin) / GridProperties.CellSize);
            
            Vector2Int cellMin = WorldPositionToCell(point.WorldPosition, WorldToCellPositionMethod.Floor);
            Vector2Int cellMax = WorldPositionToCell(point.WorldPosition, WorldToCellPositionMethod.Ceil);
        
            point.CellMinWorld = World.RelativeToWorldCoordinates(cellMin);
            point.CellMaxWorld = World.RelativeToWorldCoordinates(cellMax);
        
            var searchFrom = new Vector2Int(x: cellMin.x - deltaRadius, y: cellMin.y - deltaRadius);
            var searchTo = new Vector2Int(x: cellMax.x + deltaRadius, y: cellMax.y + deltaRadius);
            
            float sqrtRad = Mathf.Pow(point.Radius + UserProperties.PointMargin, 2);
        
            for (int y = searchFrom.y; y <= searchTo.y; y++)
            {
                float cellY = Mathf.Abs(GridProperties.CellSize * y) 
                              + GridProperties.PositionOffset.y;
                float powY = Mathf.Pow(cellY - point.WorldPosition.y, 2);
                
                for (int x = searchFrom.x; x <= searchTo.x; x++)
                {
                    float cellX = Mathf.Abs(GridProperties.CellSize * x)
                                  + GridProperties.PositionOffset.x;
                    float powX = Mathf.Pow(cellX - point.WorldPosition.x, 2);
                    float dstToCenter = powX + powY;
                    if (dstToCenter < sqrtRad)
                    {
                        WorldCoordinates wc = World
                            .RelativeToWorldCoordinates(new Vector2Int(x, y));
                        
                        if (ChunkCoordinate == wc.ChunkPosition)
                        {
                            Cells[FlatCoordinates(wc.CellPosition)] = PointsInternal.Count;
                        }
                        else
                        {
                            World.AddPointToGrid(wc, point);
                        }
                        
                    }
                }
            }
        }
    }
}