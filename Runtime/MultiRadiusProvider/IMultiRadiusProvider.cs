using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.MultiRadiusProvider
{
    public interface IMultiRadiusProvider
    {
        RadiusProperties Properties { get; }
        
        float MinRadius { get; }
        
        float MaxRadius { get; }
        
        float GetRandomRadius(int currentTry, int maxTries);
    }
}