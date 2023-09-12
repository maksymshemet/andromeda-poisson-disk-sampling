using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface IGrid// where TPoint : PointGrid, new()
    {
        ICellHolder Cells { get; }

        PointGrid GetPointByIndex(int pointIndex);
    }
}