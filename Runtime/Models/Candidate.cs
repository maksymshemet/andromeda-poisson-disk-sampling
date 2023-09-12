using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public struct Candidate
    {
        public float FullRadius => Radius + Margin;
        
        public Vector3 WorldPosition;
        public float Radius;
        public float Margin;
        
        public Vector2Int CellMin;
        public Vector2Int CellMax;
        
        public bool IsIntersectWithPoint(PointGrid point)
        {
            float sqrDst = (point.WorldPosition - WorldPosition).sqrMagnitude;
            float radius = point.FullRadius + FullRadius;
            return sqrDst < (radius * radius);
        }
    }
}