using System;
using System.Collections.Generic;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids.CellHolders;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids
{
    public interface IGrid
    {
        event Action<IGrid, Point> OnPointCreated;
        event Action<IGrid, Point> OnPointRemoved;
        
        ICellHolder Cells { get; }
        GridProperties GridProperties { get; }
        
        IEnumerable<Point> Points { get; }
        int PointsCount { get; }
        
        Point GetPoint(int x, int y);
        Point GetPointByIndex(int pointIndex);
        
        HashSet<Point> GetPointsAround(in Point point, int region);
        HashSet<Point> GetPointsAround(in Vector2Int cellMin, in Vector2Int cellMax, int region);
        
        Point TrySpawnPointFrom(Point point);
        Point TryAddPoint(Candidate candidate);
        
        void RemovePoint(Point point);

        Vector2 CellToWorldCoordinate(in Vector2Int cell);
        Vector2 CellToWorldCoordinate(int x, int y);
        
        void Clear();
    }
}