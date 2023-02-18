using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.MultiRadiusProvider
{
    public class MultiRadiusProviderRandom : IMultiRadiusProvider
    {
        public RadiusProperties Properties => _radiusProperties;
        
        public float MinRadius => _radiusProperties.MinRadius;
        public float MaxRadius => _radiusProperties.MaxRadius;
        
        private readonly RadiusPropertiesMinMax _radiusProperties;

        public MultiRadiusProviderRandom(RadiusPropertiesMinMax radiusProperties)
        {
            _radiusProperties = radiusProperties;
        }

        public float GetRandomRadius(int currentTry, int maxTries)
        {
            float minRad = _radiusProperties.MinRadius;
            float maxRad = _radiusProperties.MaxRadius;
            float newRadius = Random.Range(minRad, maxRad);
            
            if(_radiusProperties.RadiusPerTryCurve == null)
                return newRadius;
            
            float adjustPercent = _radiusProperties.RadiusPerTryCurve
                .Evaluate((float) currentTry / maxTries);
            return Mathf.Min(maxRad, 
                Mathf.Max(minRad, newRadius * adjustPercent));
        }
    }
}