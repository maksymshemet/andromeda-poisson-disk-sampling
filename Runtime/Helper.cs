using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime
{
    public static class Helper
    {
        public const float DoublePI = 6.28318548f;
        public const float Sqrt2 = 1.41421354f;
        
        public static void ClearConsole()
        {
            var logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries
                .GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        public static Vector3 GetCandidateRandomWorldPosition(Vector3 spawnWorldPosition,
            float spawnerRadius, float candidateRadius)
        {
            float angel = Random.value * DoublePI;
            var direction = new Vector3(Mathf.Sin(angel), Mathf.Cos(angel));
            return spawnWorldPosition + direction * Random.Range(2 * spawnerRadius, candidateRadius * 3f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SearchBoundaries GetSearchBoundaries(in IDPSGridConfig gridConfig, Vector2Int cellMin, Vector2Int cellMax, int region)
        {
            return new SearchBoundaries
            {
                StartX = Mathf.Max(gridConfig.Cells.MinBound.x, cellMin.x - region),
                EndX = Mathf.Min(cellMax.x + region, gridConfig.Cells.MaxBound.x - 1),
                StartY = Mathf.Max(gridConfig.Cells.MinBound.y, cellMin.y - region),
                EndY = Mathf.Min(cellMax.y + region, gridConfig.Cells.MaxBound.y - 1),
            };
        }
        
    }
}