
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors
{
    public interface IRadiusSelector
    {
        PointSize GetRadius(int currentTry, int maxTries);
    }
}