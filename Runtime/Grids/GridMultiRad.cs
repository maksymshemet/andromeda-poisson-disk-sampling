using System;
using System.Collections;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridMultiRad : PointsHolder<PointPropertiesMultiRadius, GridMultiRad>
    {
        public GridMultiRad(PointPropertiesMultiRadius userProperties, GridProperties gridProperties, 
            ICandidateValidator candidateValidator) 
            : base(new CellHolderArray(gridProperties), candidateValidator, gridProperties, userProperties)
        {
            
        }
        
        public GridMultiRad(PointPropertiesMultiRadius userProperties, GridProperties gridProperties) 
            : base(new CellHolderArray(gridProperties), new DefaultCandidateValidator(), gridProperties, userProperties)
        {
            
        }
        
        public void Fill()
        {
            Vector3 fakeWorldPosition = new Vector3(
                x: GridProperties.Size.x / 2f, 
                y: GridProperties.Size.y / 2f) + GridProperties.PositionOffset;

            float fakePointRadius = CreateCandidateRadius(0, PointProperties.Tries);
            var fakePoint = new PointGrid(worldPosition: fakeWorldPosition, radius: fakePointRadius, margin: 0);
            
            int pointIndex = TrySpawnPointFrom(fakePoint, out PointGrid _);
            if (pointIndex == -1)
            {
                throw new Exception("Couldn't spawn the point");
            }

            var spawnPoints = new List<int> { 1 };
            
            do
            {
                int spawnIndex = Random.Range(0, spawnPoints.Count);
                int spawnPointIndex = spawnPoints[spawnIndex];
                PointGrid spawnPoint = GetPointByIndex(spawnPointIndex);
                
                pointIndex = TrySpawnPointFrom(spawnPoint, out PointGrid _);
                if (pointIndex > -1)
                {
                    spawnPoints.Add(pointIndex + 1);
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

        protected override float CreateCandidateRadius(int currentTry, int maxTries)
        {
            return PointProperties.RadiusProvider.GetRandomRadius(currentTry, maxTries);
        }

#if UNITY_EDITOR
        protected bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
        {
            if (GridProperties.CellLenghtX * GridProperties.CellLenghtY >= PointCount) return false;
            
            Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
            return true;
        }
#endif
    }
}