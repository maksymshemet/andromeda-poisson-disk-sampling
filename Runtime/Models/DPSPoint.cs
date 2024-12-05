using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class DPSPoint : IDPSPoint
    {
        public int Index { get; set; }
        public Vector2Int CellMin { get; set; }
        public Vector2Int CellMax { get; set; }
        
        public Vector3 WorldPosition { get; set; }
        public float Radius { get; set; }
        public float Margin { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj is DPSPoint point)
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

