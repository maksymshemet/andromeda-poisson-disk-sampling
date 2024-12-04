using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public struct Candidate
    {
        public Vector3 WorldPosition;
        public float Radius;
        public float Margin;
        
        public bool IsIntersectWithPoint(Point point)
        {
            float sqrDst = (point.WorldPosition - WorldPosition).sqrMagnitude;
            float radius = point.Radius + point.Margin + Margin + Radius;
            return sqrDst < (radius * radius);
        }
    }
}