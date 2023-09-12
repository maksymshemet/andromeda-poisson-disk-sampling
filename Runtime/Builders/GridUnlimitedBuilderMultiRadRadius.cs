using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;


namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public abstract class GridUnlimitedBuilderMultiRadRadius<TGrid> : 
        GridBuilder<PointPropertiesMultiRadius, 
            PointPropertiesBuilderMultiRadius, 
            TGrid,
            GridUnlimitedBuilderMultiRadRadius<TGrid>>
        where TGrid : GridMultiRadUnlimited
    {

        private ICandidateValidator _candidateValidator;
        
        public GridUnlimitedBuilderMultiRadRadius<TGrid> WithCandidateValidator(ICandidateValidator candidateValidator)
        {
            _candidateValidator = candidateValidator;
            return this;
        }

        public override TGrid Build()
        {
            PointPropertiesMultiRadius pointProperties = PointPropertiesBuilder.Build();
            GridProperties gridProperties = BuildGridProperties();
            
            float cellSize = (pointProperties.RadiusProvider.MinRadius + pointProperties.PointMargin) / 2f;

            gridProperties.CellSize = cellSize;
            gridProperties.CellLenghtY = Mathf.CeilToInt(gridProperties.Size.y / cellSize);
            gridProperties.CellLenghtX = Mathf.CeilToInt(gridProperties.Size.x / cellSize);
            gridProperties.Center = new Vector3(gridProperties.Size.x / 2f, gridProperties.Size.y / 2f) 
                                    + gridProperties.PositionOffset;

            gridProperties.FillCellsInsidePoint = true;

            return BuildInternal(pointProperties, gridProperties, _candidateValidator);
            // if (_candidateValidator != null)
            // {
            //     return new GridMultiRadUnlimited(pointProperties, gridProperties, _candidateValidator)
            //     {
            //         CustomBuilder = CustomBuilder
            //     };
            // }
            //
            // return new GridMultiRadUnlimited(pointProperties, gridProperties)
            // {
            //     CustomBuilder = CustomBuilder
            // };
        }

        protected abstract TGrid BuildInternal(PointPropertiesMultiRadius pointProperties, GridProperties gridProperties, ICandidateValidator candidateValidator);
       
    }

    public class GridUnlimitedBuilderMultiRadRadius : GridUnlimitedBuilderMultiRadRadius<GridMultiRadUnlimited>
    {
        protected override GridMultiRadUnlimited BuildInternal(PointPropertiesMultiRadius pointProperties,
            GridProperties gridProperties, ICandidateValidator candidateValidator)
        {
            if (candidateValidator != null)
            {
                return new GridMultiRadUnlimited(pointProperties, gridProperties, candidateValidator)
                {
                    CustomBuilder = CustomBuilder
                };
            }
            
            return new GridMultiRadUnlimited(pointProperties, gridProperties)
            {
                CustomBuilder = CustomBuilder
            };
        }
    }
}