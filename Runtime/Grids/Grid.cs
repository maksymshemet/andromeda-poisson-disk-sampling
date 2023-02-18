using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public abstract class Grid<TGridUserProperty, TSelf> : GridAbstract<PointGrid, TGridUserProperty, TSelf>
        where TGridUserProperty : PointProperties
        where TSelf : Grid<TGridUserProperty, TSelf>
    {
        protected Grid(TGridUserProperty userProperties, GridProperties gridProperties) 
            : base(userProperties, gridProperties)
        {
            
        }
        
        protected override void OnPointCreatedInternal(in PointGrid point)
        {
            point.CellMin = WorldPositionToCell(point.WorldPosition, WorldToCellPositionMethod.Floor);
            Vector2Int maxPosition = WorldPositionToCell(point.WorldPosition,
                WorldToCellPositionMethod.Ceil);
            point.CellMax = new Vector2Int(
                Mathf.Min(maxPosition.x, GridProperties.CellLenghtX - 1),
                Mathf.Min(maxPosition.y, GridProperties.CellLenghtY - 1));
            
            Cells[FlatCoordinates(point.CellMin)] = PointsInternal.Count;
            Cells[FlatCoordinates(point.CellMax)] = PointsInternal.Count;
            Cells[FlatCoordinates(point.CellMin.x, point.CellMax.y)] = PointsInternal.Count;
            Cells[FlatCoordinates(point.CellMax.x, point.CellMin.y)] = PointsInternal.Count;
        }
        
        public bool IsPositionInAABB(Vector3 worldPosition)
        {
            return worldPosition.x >= GridProperties.PositionOffset.x &&
                   worldPosition.y >= GridProperties.PositionOffset.y &&
                   worldPosition.x <= GridProperties.PositionOffset.x + GridProperties.Size.x &&
                   worldPosition.y <= GridProperties.PositionOffset.y + GridProperties.Size.y;
        }


        protected bool IsCandidateInAABB(in Candidate candidate)
        {
            if (GridProperties.PointsLocation == PointsLocation.CenterInsideGrid)
            {
                return IsPositionInAABB(candidate.WorldPosition);
            }

            if (GridProperties.PointsLocation == PointsLocation.PointInsideGrid)
            {
                return IsPositionInAABB(new Vector3(
                           candidate.WorldPosition.x - candidate.Radius,
                           candidate.WorldPosition.y - candidate.Radius)) &&
                       IsPositionInAABB(new Vector3(
                           candidate.WorldPosition.x + candidate.Radius,
                           candidate.WorldPosition.y + candidate.Radius));
            }
            
            return IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x - candidate.FullRadius,
                       candidate.WorldPosition.y - candidate.FullRadius)) &&
                   IsPositionInAABB(new Vector3(
                       candidate.WorldPosition.x + candidate.FullRadius,
                       candidate.WorldPosition.y + candidate.FullRadius));
        }
    }
}