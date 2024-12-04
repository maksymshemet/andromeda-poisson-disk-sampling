using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties
{
    public class GridProperties
    {
        public int CellLenghtX => Mathf.CeilToInt(Size.x / CellSize);
        public int CellLenghtY => Mathf.CeilToInt(Size.y / CellSize);
        public Vector3 Center => new Vector3(Size.x / 2f, Size.y / 2f) + PositionOffset;
        public float CellSize { get; }
        
        public Vector2 Size;
        public Vector3 PositionOffset;
        
        public PointsLocation PointsLocation = PointsLocation.CenterInsideGrid;
        public bool FillCellsInsidePoint;
        
        public float PointMargin = 0;
        public int Tries = 20;

        public GridProperties(float minRadius, float minPointMargin)
        {
            CellSize = (minRadius + minPointMargin) / 2f;;
        }
    }
}