using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface ICustomPointBuilder<TPoint> where TPoint : PointGrid, new()
    {
        bool Build(IGrid<TPoint> sender, in TPoint point, int currentTry, int maxTries);
    }
}