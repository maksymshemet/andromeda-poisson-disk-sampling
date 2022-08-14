using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public interface IGridAbstract<T> where T : Point
    {
        public Vector3 WorldPositionOffset { get; set; }
     
        public GridProperties GridProperties { get; }

        public IReadOnlyList<T> Points { get; }
        
        List<T> Fill();
        
        List<T> Fill(Vector3 spawnPosition);                                             
        
        T TrySpawnPointFrom(Vector3 spawnPosition);
        
        T TrySpawnPointFrom(T spawnPoint);
        
        int GetCellValue(Vector2Int coordinates);
        
        int GetCellValue(int x, int y);
        
        int GetCellValue(int i);
        
        void SetCellValue(int x, int y, int value);
        
        void SetCellValue(int i, int value);
        
        T GetPoint(int index);
        
        T GetPoint(int x, int y);
        
        int AddPoint(int x, int y, T point);
    
        int FlatCoordinates(int x, int y);
    }
}