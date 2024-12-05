using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface IDPSGridConfig
    {
        ICellHolder Cells { get; }
        GridProperties GridProperties { get; }
        
        bool IsPositionFree(Candidate candidate, int x, int y);
    }
    
    public interface IDPSGrid : IDPSGrid<DPSPoint>
    {
    }

    public interface IDPSGrid<TPoint> : IDPSGridConfig where TPoint : DPSPoint, new()
    {
        event Action<TPoint> OnPointCreated;
        event Action<TPoint> OnPointRemoved;
        
        IEnumerable<TPoint> Points { get; }
        int PointsCount { get; }
        
        TPoint GetPoint(int x, int y);
        TPoint GetPointByIndex(int pointIndex);
        
        HashSet<TPoint> GetPointsAround(in TPoint point, int region);
        HashSet<TPoint> GetPointsAround(in Vector2Int cellMin, in Vector2Int cellMax, int region);

        void StorePoint(TPoint point);
        TPoint TrySpawnPointFrom(TPoint point);
        TPoint TryAddPoint(Candidate candidate);
        
        void RemovePoint(TPoint point);

        Vector2 CellToWorldCoordinate(in Vector2Int cell);
        Vector2 CellToWorldCoordinate(int x, int y);
        
        void Clear();
    }
}