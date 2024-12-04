using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors
{
    public class RadiusSelectorConst : IRadiusSelector
    {
        public float Radius;
        public float PointMargin = 0;
        
        public PointSize GetRadius(int currentTry, int maxTries)
        {
            return new PointSize
            {
                Radius = Radius, 
                Margin = PointMargin
            };
        }
    }
}