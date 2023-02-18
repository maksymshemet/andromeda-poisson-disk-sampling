using System;
using System.Collections;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public abstract class GridAbstract<TPoint, TGridUserProperty, TSelf> 
        where TPoint : PointGrid
        where TGridUserProperty : PointProperties
        where TSelf : GridAbstract <TPoint, TGridUserProperty, TSelf>
    {
        public event Action<TSelf, TPoint> OnPointCreated;
        
        public TGridUserProperty UserProperties { get; }
        
        public GridProperties GridProperties { get; }
        
        public IReadOnlyList<TPoint> Points => PointsInternal;

        protected readonly int[] Cells;
        protected readonly List<TPoint> PointsInternal;
        
        protected GridAbstract(TGridUserProperty userProperties, GridProperties gridProperties)
        {
            UserProperties = userProperties;
            GridProperties = gridProperties;
            
            Cells = new int[GridProperties.CellLenghtY * GridProperties.CellLenghtX];
            PointsInternal = new List<TPoint>();
        }

        public Vector2Int WorldPositionToCell(Vector3 worldPosition,
            WorldToCellPositionMethod method = WorldToCellPositionMethod.Round)
        {
            float x = (worldPosition.x - GridProperties.PositionOffset.x) / GridProperties.CellSize;
            float y = (worldPosition.y - GridProperties.PositionOffset.y) / GridProperties.CellSize;

            return method switch
            {
                WorldToCellPositionMethod.Round => new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y)),
                WorldToCellPositionMethod.Ceil => new Vector2Int(Mathf.CeilToInt(x), Mathf.CeilToInt(y)),
                WorldToCellPositionMethod.Floor => new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y)),
                WorldToCellPositionMethod.Clamped => new Vector2Int(
                    x: Mathf.Max(0, Mathf.Min(Mathf.RoundToInt(x), GridProperties.CellLenghtX - 1)),
                    y: Mathf.Max(0, Mathf.Min(Mathf.RoundToInt(y), GridProperties.CellLenghtY - 1))),
                
                _ => throw new NotImplementedException($"method={method.ToString()} not implemented")
            };
        }
        

        public bool IsValidCellPosition(Vector2Int cellCoordinate)
        {
            return cellCoordinate.x < GridProperties.CellLenghtX ||
                   cellCoordinate.y < GridProperties.CellLenghtY ||
                   cellCoordinate.x >= 0 ||
                   cellCoordinate.y >= 0;
        }

        public int GetCellValue(int x, int y) => GetCellValue(FlatCoordinates(x, y));

        public TPoint GetPoint(int x, int y)
        {
            int index = GetCellValue(x, y) - 1;
            return index > -1 ? PointsInternal[index] : default;
        }
        
        protected int GetCellValue(int flatCoordinate) => Cells[flatCoordinate];
        
        public void SetCellValue(int x, int y, int value) => SetCellValue(FlatCoordinates(x, y), value);

        protected void SetCellValue(int flatCoordinate, int value) => Cells[flatCoordinate] = value;
        
        public int ClearCellValue(int x, int y) => ClearCellValue(FlatCoordinates(x, y));
        
        protected int ClearCellValue(int flatCoordinate) => Cells[flatCoordinate];

        public bool IsCellEmpty(int x, int y) => IsCellEmpty(FlatCoordinates(x, y));

        protected bool IsCellEmpty(int flatCoordinate) => Cells[flatCoordinate] == 0;
        
        public int FlatCoordinates(Vector2Int cell) => FlatCoordinates(cell.x, cell.y);
        
        protected int FlatCoordinates(int x, int y) => y * GridProperties.CellLenghtX + x;

        protected PointGrid GetPointByIndex(int pointIndex) => PointsInternal[pointIndex - 1];

        protected int AddPoint(TPoint point)
        {
            PointsInternal.Add(point);
            
            OnPointCreatedInternal(point);
            
            OnPointCreated?.Invoke((TSelf) this, point);
            
            return PointsInternal.Count - 1;
        }

        protected abstract void OnPointCreatedInternal(in TPoint point);
        
#if UNITY_EDITOR
        protected bool EditorCheckForEndlessSpawn(ICollection spawnPoints)
        {
            if (GridProperties.CellLenghtX * GridProperties.CellLenghtY >= PointsInternal.Count) return false;
            
            Debug.LogError($"Endless spawn points: {spawnPoints.Count}");
            return true;
        }
#endif
    }
}