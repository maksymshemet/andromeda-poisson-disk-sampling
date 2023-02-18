using System;
using System.Reflection;
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
    }
}