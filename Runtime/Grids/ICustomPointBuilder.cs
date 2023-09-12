using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface ICustomPointBuilder
    {
        bool Build(IGrid sender, in PointGrid point, int currentTry, int maxTries);
    }
}