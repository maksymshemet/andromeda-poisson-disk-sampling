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
            
            point.CellMinWorld = new WorldCoordinates(World, cellMin);
            point.CellMaxWorld = new WorldCoordinates(World, cellMax);
            
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

            var ab = new WorldCoordinates(World, new Vector2Int(cellMin.x, cellMax.y));
            var ba = new WorldCoordinates(World, new Vector2Int(cellMax.x, cellMin.y));

            SetCellValueLocal(ab);
            SetCellValueLocal(ba);
        }
    }
}