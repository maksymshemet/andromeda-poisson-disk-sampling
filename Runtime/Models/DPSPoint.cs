using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class DPSPoint : IDPSPoint
    {
        public int Index { get; set; }
        public Vector2Int CellMin { get; set; }
        public Vector2Int CellMax { get; set; }
        public Vector3 WorldPosition { get; set; }
        public PointSize Size { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj is DPSPoint point)
            {
                return Index.Equals(point.Index)
                       && Size.Radius.Equals(point.Size.Radius) 
                       && Size.Margin.Equals(point.Size.Margin)
                       && WorldPosition.Equals(point.WorldPosition);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (WorldPosition, Size).GetHashCode();
        }
        
        public override string ToString()
        {
            return $"PointGrid: wp{WorldPosition}, r{Size.Radius} m{Size.Margin} i{Index}";
        }
    }
}

