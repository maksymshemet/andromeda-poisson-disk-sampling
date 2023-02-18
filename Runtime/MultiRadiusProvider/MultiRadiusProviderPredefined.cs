using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.MultiRadiusProvider
{
    public class MultiRadiusProviderPredefined : IMultiRadiusProvider
    {
        public RadiusProperties Properties => _radiusProperties;

        public float MinRadius => _radiusProperties.PredefinedRadii[0];
        public float MaxRadius => _radiusProperties.PredefinedRadii[^1];

        private readonly RadiusPropertiesPredefined _radiusProperties;

        public MultiRadiusProviderPredefined(RadiusPropertiesPredefined radiusProperties)
        {
            _radiusProperties = radiusProperties;
        }

        public float GetRandomRadius(int currentTry, int maxTries)
        {
            float newRadius =
                _radiusProperties.PredefinedRadii[Random.Range(0, _radiusProperties.PredefinedRadii.Length)];
            
            if(_radiusProperties.RadiusPerTryCurve == null)
                return newRadius;
            
            float adjustPercent = _radiusProperties.RadiusPerTryCurve
                .Evaluate((float) currentTry / maxTries);
            return _radiusProperties.PredefinedRadii.Closest(newRadius * adjustPercent);
        }
    }
}