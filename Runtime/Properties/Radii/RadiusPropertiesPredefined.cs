using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties.Radii
{
    [Serializable]
    public enum PredefinedRadiusSelectType
    {
        Random = 0,
        Descending = 2
    }
    
    public class RadiusPropertiesPredefined : RadiusProperties
    {
        public PointSize[] PredefinedRadii;
        public PredefinedRadiusSelectType RadiusSelectType = PredefinedRadiusSelectType.Random;
    }
}