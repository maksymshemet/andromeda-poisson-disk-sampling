using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties
{
    public class GridProperties
    {
        public int CellLenghtX;
        public int CellLenghtY;
        public float CellSize;
        
        public Vector2 Size;
        public Vector3 PositionOffset;
        public Vector3 Center;
        
        public PointsLocation PointsLocation = PointsLocation.CenterInsideGrid;
        public bool FillCellsInsidePoint;
    }
}