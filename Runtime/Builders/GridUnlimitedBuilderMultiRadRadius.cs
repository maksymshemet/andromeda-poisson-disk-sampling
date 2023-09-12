using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Builders
{
    public class GridUnlimitedBuilderMultiRadRadius<TPoint> : 
        GridBuilder<PointPropertiesMultiRadius, PointPropertiesBuilderMultiRadius, 
            GridMultiRadUnlimited<TPoint>, TPoint, GridUnlimitedBuilderMultiRadRadius<TPoint>>
        where TPoint : PointGrid, new()
    {

        private ICandidateValidator<TPoint> _candidateValidator;
        
        public GridUnlimitedBuilderMultiRadRadius<TPoint> WithCandidateValidator(ICandidateValidator<TPoint> candidateValidator)
        {
            _candidateValidator = candidateValidator;
            return this;
        }

        public override GridMultiRadUnlimited<TPoint> Build()
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
            
            if (_candidateValidator != null)
            {
                return new GridMultiRadUnlimited<TPoint>(pointProperties, gridProperties, _candidateValidator)
                {
                    CustomBuilder = CustomBuilder
                };
            }
            
            return new GridMultiRadUnlimited<TPoint>(pointProperties, gridProperties)
            {
                CustomBuilder = CustomBuilder
            };
        }
    }
}