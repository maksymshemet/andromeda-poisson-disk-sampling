using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridMultiRadUnlimited<TPoint> : PointsHolder<TPoint, PointPropertiesMultiRadius, GridMultiRadUnlimited<TPoint>>
        where TPoint : PointGrid, new()
    {
        public GridMultiRadUnlimited(PointPropertiesMultiRadius userProperties, GridProperties gridProperties, 
            ICandidateValidator<TPoint> candidateValidator) 
            : base(new CellHolderDictionary(gridProperties), candidateValidator, gridProperties, userProperties)
        {
            
        }
        
        public GridMultiRadUnlimited(PointPropertiesMultiRadius userProperties, GridProperties gridProperties) 
            : base(new CellHolderDictionary(gridProperties), new DefaultCandidateValidator<TPoint>(), gridProperties, userProperties)
        {
            
        }

        
        protected override float CreateCandidateRadius(int currentTry, int maxTries)
        {
            return PointProperties.RadiusProvider.GetRandomRadius(currentTry, maxTries);
        }
    }
    
    public class GridMultiRadUnlimited : GridMultiRadUnlimited<PointGrid>
    {
        public GridMultiRadUnlimited(PointPropertiesMultiRadius userProperties, GridProperties gridProperties,
            ICandidateValidator<PointGrid> candidateValidator) 
            : base(userProperties, gridProperties, candidateValidator)
        {
            
        }
        
        public GridMultiRadUnlimited(PointPropertiesMultiRadius userProperties, GridProperties gridProperties) 
            : base(userProperties, gridProperties)
        {
            
        }
    }
}