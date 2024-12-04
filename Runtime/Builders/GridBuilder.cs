using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CandidateValidator;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{

    public static class GridBuilder
    {
        public static GridBuilderConstRadius ConstRadius()
        {
            return new GridBuilderConstRadius();
        }
        
        public static GridBuilderMultiRadius MultiRadius()
        {
            return new GridBuilderMultiRadius();
        }
    }
    
    public abstract class GridBuilder<TPointProperties, TSelf>
        where TSelf : GridBuilder<TPointProperties, TSelf>
    {
        protected Action<GridProperties> GridConsumer;
        protected TPointProperties PointProperties;
        protected ICandidateValidator CandidateValidator;
        protected bool IsGridUnlimited = false;
        
        public TSelf WithPointProperties(TPointProperties pointProperties)
        {
            PointProperties = pointProperties;
            return (TSelf) this;
        }
        
        public TSelf WithGridProperties(Action<GridProperties> gridConsumer)
        {
            GridConsumer = gridConsumer;
            return (TSelf) this;
        }
        
        public TSelf WithCandidateValidator(ICandidateValidator candidateValidator)
        {
            CandidateValidator = candidateValidator;
            return (TSelf) this;
        }
        
        public TSelf IsUnlimited(bool isUnlimited)
        {
            IsGridUnlimited = isUnlimited;
            return (TSelf) this;
        }
                
        public abstract IGrid Build();
    }
}