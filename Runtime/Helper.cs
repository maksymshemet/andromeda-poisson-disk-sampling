using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using dd_andromeda_poisson_disk_sampling.Propereties;
using dd_andromeda_poisson_disk_sampling.Propereties.Radius;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling
{

    public interface IFiller
    {
        // IRadius Radius { get; }
        int Tries { get; }
        GridCore GridCore { get; }
        
        bool TryAddPoint(PointWorld candidate);

        bool TryCreateCandidate(Vector3 spawnerPosition, float spawnerRadius, int currentTry, int maxTries, out PointWorld candidate);
        
        bool TryCreateCandidate(Vector3 spawnerPosition, int currentTry, int maxTries, out PointWorld candidate);

        IReadOnlyList<PointWorld> GetPoints();
    }
    
    public static class Helper
    {
        public const float DoublePI = 6.28318548f;
        
        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
 
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
 
            clearMethod.Invoke(null, null);
        }

        public static Vector3 GetCandidateRandomWorldPosition(Vector3 spawnWorldPosition,
            float spawnerRadius, float candidateRadius)
        {
            var angel = Random.value * DoublePI;
            var direction = new Vector3(Mathf.Sin(angel), Mathf.Cos(angel));
            return spawnWorldPosition + direction * Random.Range(2 * spawnerRadius, candidateRadius * 3f);
        }
        
        public static float PowLengthBetweenCellPoints(int a, int b, float cellSize)
        {
            var deltaY = Mathf.Abs(a - b);
            var yy = (deltaY * cellSize);
            return yy * yy;
        }

        public static IReadOnlyList<PointWorld> Fill(this IFiller filler, Vector3 spawnPosition)
        {
            return Fill(filler, spawnPosition, filler.Tries);
        }
        
        public static IReadOnlyList<PointWorld> Fill(this IFiller filler)
        {
            return Fill(filler, filler.GridCore.Center, filler.Tries);
        }

        public static IReadOnlyList<PointWorld> Fill(this IFiller filler, int tries)
        {
            return Fill(filler, filler.GridCore.Center, tries);
        }

        public static IReadOnlyList<PointWorld> Fill(this IFiller filler, Vector3 spawnPosition, int tries)
        {
            PointWorld point = TrySpawnPoint(filler, spawnPosition, tries);
            if(point == null)
            {
                throw new Exception("Couldn't spawn the point");
            }
            
            var spawnPoints = new List<PointWorld> { point };

            do {
                try
                {
                    var spawnIndex = Random.Range(0, spawnPoints.Count);
                    var spawnPoint = spawnPoints[spawnIndex];
                    point = TrySpawnPoint(filler, spawnPoint.WorldPosition, tries, spawnPoint.Radius);
                    if (point != null)
                    {
                        spawnPoints.Add(point);
                    }
                    else
                    {
                        spawnPoints.RemoveAt(spawnIndex);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

#if UNITY_EDITOR
                if (EditorCheckForEndlessSpawn(spawnPoints, filler.GridCore, filler.GetPoints())) break;
#endif
            }
            while (spawnPoints.Count > 0);

            return filler.GetPoints();
        }
        
        private static PointWorld TrySpawnPoint(IFiller filler, Vector3 spawnPosition, int tries, float radius = -1) 
        {
            for (var i = 0; i < tries; i++)
            {
                if (radius == -1)
                {
                    if (filler.TryCreateCandidate(spawnPosition, i, tries, out var candidate))
                    {
                        if(filler.TryAddPoint(candidate))
                            return candidate;
                    }
                }
                else
                {
                    if (filler.TryCreateCandidate(spawnPosition, radius, i, tries, out var candidate))
                    {
                        if(filler.TryAddPoint(candidate))
                            return candidate;
                    }
                }
            }

            return null;
        }
        
#if UNITY_EDITOR
        private static bool EditorCheckForEndlessSpawn(ICollection spawnPoints, GridCore gridCore, IReadOnlyCollection<PointWorld> points)
        {
            if (gridCore.Properties.CellWidth * gridCore.Properties.CellHeight >= points.Count) return false;
            
            Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
            return true;
        }
#endif
    }
}