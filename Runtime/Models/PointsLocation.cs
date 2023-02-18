using System;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    [Serializable]
    public enum PointsLocation
    {
        CenterInsideGrid,
        PointInsideGrid,
        PointsWithMarginInsideGrid
    }
}