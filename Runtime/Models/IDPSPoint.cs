using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public interface IDPSPoint
    {
        int Index { get; set; }
        Vector2Int CellMin { get; set; }
        Vector2Int CellMax { get; set; }
        
        Vector3 WorldPosition { get; set; }
        public PointSize Size { get; set; }
    }
}