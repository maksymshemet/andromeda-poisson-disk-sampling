using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    // public interface IGridAbstract<T> where T : Point
    // {
    //     public Vector3 WorldPositionOffset { get; set; }
    //  
    //     public GridProperties GridProperties { get; }
    //     
    //     public IReadOnlyList<T> Points { get; }
    //     
    //     List<T> Fill();
    //     
    //     List<T> Fill(Vector3 startSpawnPoint);                                             
    //     
    //     bool TrySpawnPointAt(Vector3 startPosition);
    //     
    //     bool TrySpawnPointAt(Vector3 startPosition, out T point);
    //     
    //     bool TrySpawnPointFrom(T spawnPoint);
    //     
    //     bool TrySpawnPointFrom(T spawnPoint, out T point);
    //     
    //     int GetCellValue(Vector2Int coordinates);
    //     
    //     int GetCellValue(int x, int y);
    //     
    //     void SetCellValue(int x, int y, int value);
    //     
    //     T GetPointValue(int x, int y);
    //     
    //     int AddPoint(int x, int y, T point);
    //     
    //     int GetCellValue(int i);
    //     
    //     void SetCellValue(int i, int value);
    //
    //     int FlatCoordinates(int x, int y);
    //
    //     T GetPointValue(int index);
    // }
    
    public interface IGrid
    {
        public Vector3 WorldPositionOffset { get; set; }
     
        public GridProperties GridProperties { get; }
        
        public IReadOnlyList<Point> Points { get; }
        
        List<Point> Fill();
        
        List<Point> Fill(Vector3 startSpawnPoint);                                             
        
        bool TrySpawnPointAt(Vector3 startPosition);
        
        bool TrySpawnPointAt(Vector3 startPosition, out Point point);
        
        bool TrySpawnPointFrom(Point spawnPoint);
        
        bool TrySpawnPointFrom(Point spawnPoint, out Point point);
        
        int GetCellValue(Vector2Int coordinates);
        
        int GetCellValue(int x, int y);
        
        void SetCellValue(int x, int y, int value);
        
        Point GetPointValue(int x, int y);
        
        int AddPoint(int x, int y, Point point);
        
        int GetCellValue(int i);
        void SetCellValue(int i, int value);

        public int FlatCoordinates(int x, int y);

        Point GetPointValue(int index);
    }
}