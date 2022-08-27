using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling
{
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
    }
}