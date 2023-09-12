using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridStaticUnlimited<TPoint> : PointsHolder<TPoint, PointPropertiesConstRadius, GridStaticUnlimited<TPoint>>
        where TPoint : PointGrid, new()
    {
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties, 
            ICandidateValidator<TPoint> candidateValidator) 
            : base(new CellHolderDictionary(gridProperties), candidateValidator, gridProperties, userProperties)
        {
            
        }
        
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties) 
            : base(new CellHolderDictionary(gridProperties), new DefaultCandidateValidator<TPoint>(), gridProperties, userProperties)
        {
            
        }


        protected override float CreateCandidateRadius(int currentTry, int maxTries)
        {
            return PointProperties.Radius;
        }
    }
    
    public class GridStaticUnlimited : GridStaticUnlimited<PointGrid>
    {
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties, 
            ICandidateValidator<PointGrid> candidateValidator) 
            : base(userProperties, gridProperties, candidateValidator)
        {
            
        }
        
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties) 
            : base(userProperties, gridProperties)
        {
            
        }
        
        protected override float CreateCandidateRadius(int currentTry, int maxTries)
        {
            return PointProperties.Radius;
        }
    }
}