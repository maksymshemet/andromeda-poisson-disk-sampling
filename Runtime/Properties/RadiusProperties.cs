using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties
{
    public abstract class RadiusProperties
    {
        public AnimationCurve RadiusPerTryCurve;
    }

    public class RadiusPropertiesPredefined : RadiusProperties
    {
        public float[] PredefinedRadii;
    }

    public class RadiusPropertiesMinMax : RadiusProperties
    {
        public float MinRadius;
        public float MaxRadius;
    }
}