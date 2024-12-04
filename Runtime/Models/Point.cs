using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class Point
    {
        public int Index;
        public Vector2Int CellMin;
        public Vector2Int CellMax;
        
        public readonly Vector3 WorldPosition;
        public readonly float Radius;
        public readonly float Margin;
        
        public Point(Vector3 worldPosition, float radius, float margin)
        {
            WorldPosition = worldPosition;
            Radius = radius;
            Margin = margin;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Point point)
            {
                return Index.Equals(point.Index)
                       && Radius.Equals(point.Radius) 
                       && Margin.Equals(point.Margin)
                       && WorldPosition.Equals(point.WorldPosition);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (WorldPosition, Radius, Margin).GetHashCode();
        }
        
        public override string ToString()
        {
            return $"PointGrid: wp{WorldPosition}, r{Radius} i{Index}";
        }
    }
}

