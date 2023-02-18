using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.MultiRadiusProvider;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties
{
    public abstract class PointProperties
    {
        public int Tries = 20;
        public float PointMargin = 0;
    }
    
    public class PointPropertiesConstRadius : PointProperties
    {
        public float Radius;
    }
    
    public class PointPropertiesMultiRadius : PointProperties
    {
        public IMultiRadiusProvider RadiusProvider;
    }
}