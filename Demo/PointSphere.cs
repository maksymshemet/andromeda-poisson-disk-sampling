using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class PointSphere : MonoBehaviour
    {
        public Color PointColor { get; set; }
        
        private DPSPoint _point;
        private IDPSGrid _grid;
        
        public Color RadiusColor = Color.green;
        public Color RadiusColorSelected = new Color(0.01851193f, 0.3018868f,0.05279091f, 1);
        public Color MarginColor = Color.magenta;
        
        [Header("Center")]
        public bool CenterGizmosOnSelected = true;
        public bool CenterShow = true;
        public Color CenterColor = Color.red;
        public float CenterRadius = 0.05f;
        
        [Header("Used Grid Cells")] 
        public bool GridCellsGizmosOnSelected = true;
        public bool GridCellsShow = true;
        public Color GridCellsColor = Color.red;
        public float GridCellsRadius = 0.1f;
        
        [Header("Spawn new point radius")] 
        public bool SpawnNewPointGizmosOnSelected = false;
        public bool SpawnNewPointShow = false;
        public Color SpawnNewPointColor = Color.cyan;
        
        [Header("Search Range")] 
        public bool SearchRangeGizmosOnSelected = true;
        public bool SearchRangeShow = false;
        public Color SearchRangeColor = Color.yellow;
        public float SearchRangeRadius = 0.16f;


        private GridProperties _gridProperties;
        
        // public void Init(Point point, GridStatic grid)
        // {
        //     name = $"[{point.WorldPosition}]";
        //     transform.position = point.WorldPosition;
        //     transform.localScale = new Vector3(point.Radius, point.Radius, point.Radius) * 2;
        //
        //     _point = point;
        //     _grid = grid;
        //     _gridProperties = grid.GridProperties;
        // }
        
        public void Init(DPSPoint point, IDPSGrid grid)
        {
            name = $"[{point.WorldPosition}]";
            transform.position = point.WorldPosition;
            transform.localScale = new Vector3(point.Size.Radius, point.Size.Radius, point.Size.Radius) * 2;

            _point = point;
            _gridProperties = grid.GridProperties;
            
            // int a= Random.Range(0,2);
            var sphere = GetComponent<Renderer>();
            sphere.material.SetColor("_Color", PointColor);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_point == null) return;
            
            if(GridCellsGizmosOnSelected) GridCellsShowFunc();
            if(SpawnNewPointGizmosOnSelected) SpawnNewPointFunc();
            if(SearchRangeGizmosOnSelected) SearchRangeFunc();
            if(CenterGizmosOnSelected) CenterFunc();
            
            Gizmos.color = RadiusColorSelected;
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius);
        }

        private void OnDrawGizmos()
        {
            if (_point == null) return;
            
            Gizmos.color = MarginColor;
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius + _point.Size.Margin);
            
            Gizmos.color = RadiusColor;
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius);
            
            if(!GridCellsGizmosOnSelected) GridCellsShowFunc();
            if(!SpawnNewPointGizmosOnSelected) SpawnNewPointFunc();
            if(!SearchRangeGizmosOnSelected) SearchRangeFunc();
            if(!CenterGizmosOnSelected) CenterFunc();
        }

        private void CenterFunc()
        {
            if (!CenterShow) return;

            Gizmos.color = CenterColor;
            Gizmos.DrawWireSphere(transform.position, CenterRadius);
        }
        
        private void SearchRangeFunc()
        {
            if(!SearchRangeShow) return;

            Gizmos.color = SearchRangeColor;
            
            const int searchSize = 3;
            int startX = Mathf.Max(0, _point.CellMin.x - searchSize);
            int endX = Mathf.Min(_point.CellMax.x + searchSize, _gridProperties.CellLenghtX - 1);
            int startY = Mathf.Max(0, _point.CellMin.y - searchSize);
            int endY = Mathf.Min(_point.CellMax.y + searchSize, _gridProperties.CellLenghtY - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var pos = new Vector3(
                        x: _gridProperties.CellSize * x + _gridProperties.PositionOffset.x,
                        y: _gridProperties.CellSize * y + _gridProperties.PositionOffset.y);
                    Gizmos.DrawWireSphere(pos, SearchRangeRadius);
                }
            }
        }
        
        private void GridCellsShowFunc()
        {
            if(!GridCellsShow) return;

            Gizmos.color = GridCellsColor;
            
            Gizmos.DrawSphere(new Vector3(
                _gridProperties.CellSize * _point.CellMin.x,
                _gridProperties.CellSize * _point.CellMin.y
            ), GridCellsRadius);
            
            Gizmos.DrawSphere(new Vector3(
                _gridProperties.CellSize * _point.CellMax.x,
                _gridProperties.CellSize * _point.CellMin.y
            ), GridCellsRadius);
            
            Gizmos.DrawSphere(new Vector3(
                _gridProperties.CellSize * _point.CellMin.x,
                _gridProperties.CellSize * _point.CellMax.y
            ), GridCellsRadius);
            
            Gizmos.DrawSphere(new Vector3(
                _gridProperties.CellSize * _point.CellMax.x,
                _gridProperties.CellSize * _point.CellMax.y
            ), GridCellsRadius);
        }

        private void SpawnNewPointFunc()
        {
            if(!SpawnNewPointShow) return;

            Gizmos.color = SpawnNewPointColor;
            
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius * 2);
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius * 3);
        }
    }
}