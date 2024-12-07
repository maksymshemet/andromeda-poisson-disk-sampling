using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Radii;
using Random = UnityEngine.Random;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.RadiiSelectors.MultiRadiiSelectors
{
    public class RadiusSelectorMultiPredefined : IRadiusSelectorMulti
    {
        public float MinRadius => _radiusProperties.PredefinedRadii[0].Radius;
        public float MinPointMargin => _radiusProperties.PredefinedRadii[0].Margin;

        private readonly RadiusPropertiesPredefined _radiusProperties;

        public RadiusSelectorMultiPredefined(RadiusPropertiesPredefined radiusProperties)
        {
            Array.Sort(radiusProperties.PredefinedRadii, (c1, c2) 
                => c1.Radius.CompareTo(c2.Radius));
            _radiusProperties = radiusProperties;
        }

        public PointSize GetRadius(int currentTry, int maxTries)
        {
            PointSize newRadius = _radiusProperties.RadiusSelectType switch
            {
                PredefinedRadiusSelectType.Random => _radiusProperties.PredefinedRadii[
                    Random.Range(0, _radiusProperties.PredefinedRadii.Length)],
                PredefinedRadiusSelectType.Descending => _radiusProperties.PredefinedRadii[^1],
                _ => throw new NotImplementedException()
            };

            if(_radiusProperties.RadiusPerTryCurve == null)
                return newRadius;
            
            float adjustPercent = _radiusProperties.RadiusPerTryCurve
                .Evaluate((float) currentTry / maxTries);
      
            newRadius = _radiusProperties.PredefinedRadii.Closest((newRadius.Radius + newRadius.Margin) * adjustPercent);
            
            return newRadius;
        }
    }
}