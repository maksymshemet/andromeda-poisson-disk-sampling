using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public class GridStaticUnlimited : PointsHolder<PointPropertiesConstRadius, GridStaticUnlimited>
    {
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties, 
            ICandidateValidator candidateValidator) 
            : base(new CellHolderDictionary(gridProperties), candidateValidator, gridProperties, userProperties)
        {
            
        }
        
        public GridStaticUnlimited(PointPropertiesConstRadius userProperties, GridProperties gridProperties) 
            : base(new CellHolderDictionary(gridProperties), new DefaultCandidateValidator(), gridProperties, userProperties)
        {
            
        }


        protected override float CreateCandidateRadius(int currentTry, int maxTries)
        {
            return PointProperties.Radius;
        }
    }
}