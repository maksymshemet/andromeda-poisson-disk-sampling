using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.MultiRadiusProvider;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public abstract class PointPropertiesBuilder<TProperties, TSelf> : IBuilder<TProperties>
        where TProperties : PointProperties
        where TSelf : PointPropertiesBuilder<TProperties, TSelf>
    {
        protected TProperties Properties { get;}

        protected PointPropertiesBuilder(TProperties properties)
        {
            Properties = properties;
        }

        public TSelf WithTries(int tries)
        {
            Properties.Tries = tries;
            return (TSelf) this;
        }
        
        public TSelf WithMargin(float pointMargin)
        {
            Properties.PointMargin = pointMargin;
            return (TSelf) this;
        }
        
        public virtual TProperties Build()
        {
            return Properties;
        }
    }

    public class PointPropertiesBuilderConsRadius : 
        PointPropertiesBuilder<PointPropertiesConstRadius, PointPropertiesBuilderConsRadius>
    {
        public PointPropertiesBuilderConsRadius() : base(new PointPropertiesConstRadius())
        {
        }

        public PointPropertiesBuilderConsRadius WithRadius(float radius)
        {
            Properties.Radius = radius;
            return this;
        }
    }
    
    public class PointPropertiesBuilderMultiRadius :
        PointPropertiesBuilder<PointPropertiesMultiRadius, PointPropertiesBuilderMultiRadius>
    {
        private AnimationCurve _radiusPerTryCurve;

        public PointPropertiesBuilderMultiRadius() : base(new PointPropertiesMultiRadius())
        {
        }
        
        public PointPropertiesBuilderMultiRadius WithRadii(float[] radii)
        {
            Array.Sort(radii);
            
            Properties.RadiusProvider = new MultiRadiusProviderPredefined(new RadiusPropertiesPredefined()
            {
                PredefinedRadii = radii
            });

            return this;
        }
        
        public PointPropertiesBuilderMultiRadius WithSortedRadii(float[] radii)
        {
            Properties.RadiusProvider = new MultiRadiusProviderPredefined(new RadiusPropertiesPredefined()
            {
                PredefinedRadii = radii
            });

            return this;
        }

        public PointPropertiesBuilderMultiRadius WithRadii(float min, float max)
        {
            Properties.RadiusProvider = new MultiRadiusProviderRandom(new RadiusPropertiesMinMax
            {
                MinRadius = min,
                MaxRadius = max
            });

            return this;
        }

        public PointPropertiesBuilderMultiRadius WithRadiusPerTryCurve(AnimationCurve radiusPerTryCurve)
        {
            _radiusPerTryCurve = radiusPerTryCurve;
            return this;
        }

        public override PointPropertiesMultiRadius Build()
        {
            PointPropertiesMultiRadius p = base.Build();
            p.RadiusProvider.Properties.RadiusPerTryCurve = _radiusPerTryCurve;

            return p;
        }
    }
}