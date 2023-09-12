using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface IGrid<TPoint> where TPoint : PointGrid, new()
    {
        ICellHolder Cells { get; }

        TPoint GetPointByIndex(int pointIndex);
    }
}