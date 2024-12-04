using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Radii;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors.MultiRadiiSelectors
{
    public class RadiusSelectorMultiRandom : IRadiusSelectorMulti
    {
        public float MinRadius => _radiusProperties.MinRadius;
        public float MinPointMargin => 0;
        
        private readonly RadiusPropertiesMinMax _radiusProperties;

        public RadiusSelectorMultiRandom(RadiusPropertiesMinMax radiusProperties)
        {
            _radiusProperties = radiusProperties;
        }

        public PointSize GetRadius(int currentTry, int maxTries)
        {
            float minRad = _radiusProperties.MinRadius;
            float maxRad = _radiusProperties.MaxRadius;
            float newRadius = Random.Range(minRad, maxRad);
            
            if(_radiusProperties.RadiusPerTryCurve == null)
                return new PointSize
                {
                    Radius = newRadius,
                };
            
            float adjustPercent = _radiusProperties.RadiusPerTryCurve
                .Evaluate((float) currentTry / maxTries);
            
            return new PointSize
            {
                Radius = Mathf.Min(maxRad, 
                    Mathf.Max(minRad, newRadius * adjustPercent)),
            };
        }
    }
}