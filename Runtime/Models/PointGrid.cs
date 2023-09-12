using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class PointGrid : PointAbstract
    {
        public Vector2Int CellMin;
        public Vector2Int CellMax;

        public PointGrid(Vector3 worldPosition, float radius, float margin) : base(worldPosition, radius, margin)
        {
        }

        public override string ToString()
        {
            return $"PointGrid: wp{WorldPosition}, r{Radius}]";
        }
    }
}

