using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class PointMultiRadSphere : MonoBehaviour
    {
        private DPSPoint _point;
        private IDPSGrid _grid;
        
        public Color RadiusColor = Color.green;
        public Color RadiusColorSelected = new Color(0.01851193f, 0.3018868f,0.05279091f, 1);
        public Color MarginColor = Color.magenta;
        public Color MarginPlusGridColor = new Color(1, 0.3018868f,0.05279091f, 1);
        
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
        
        public void Init(DPSPoint point, IDPSGrid grid)
        {
            name = $"[{point.WorldPosition}] r{point.Size.Radius}";
            transform.position = point.WorldPosition;
            transform.localScale = new Vector3(point.Size.Radius, point.Size.Radius, point.Size.Radius) * 2;

            _point = point;
            _grid = grid;
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
            
            Gizmos.color = MarginPlusGridColor;
            Gizmos.DrawWireSphere(transform.position, _point.Size.Radius  + _point.Size.Margin + _grid.GridProperties.PointMargin);
            
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
            
            int searchSize = Mathf.RoundToInt((_point.Size.Radius + _point.Size.Margin + _grid.GridProperties.PointMargin) / _grid.GridProperties.CellSize);
            SearchBoundaries searchBoundaries = Helper.GetSearchBoundaries(_grid, _point.CellMin, _point.CellMax, searchSize);
            
            float fullRadius = _point.Size.Radius + _point.Size.Margin + _grid.GridProperties.PointMargin;
            float sqrtRad = fullRadius * fullRadius;
            for (int y = searchBoundaries.StartY; y <= searchBoundaries.EndY; y++)
            {
                for (int x = searchBoundaries.StartX; x <= searchBoundaries.EndX; x++)
                {
                    var pos = new Vector3(
                        x: _grid.GridProperties.CellSize * x + _grid.GridProperties.PositionOffset.x,
                        y: _grid.GridProperties.CellSize * y + _grid.GridProperties.PositionOffset.y);
                    
                    if(IsInsideCircle(_point, sqrtRad, pos.x, pos.y))
                        Gizmos.DrawWireSphere(pos, SearchRangeRadius);
                    else
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawWireSphere(pos, SearchRangeRadius);
                        Gizmos.color = SearchRangeColor;
                    }
                }
            }
            
            bool IsInsideCircle(DPSPoint point, float sqrtRad, float x, float y)
            {
                double dx = x - point.WorldPosition.x;
                double dy = y - point.WorldPosition.y;
                double distanceSquared = dx * dx + dy * dy;
                return distanceSquared < sqrtRad;
            }
        }
        
        private void GridCellsShowFunc()
        {
            if(!GridCellsShow) return;

            Gizmos.color = GridCellsColor;

            for (var y = 0; y < _grid.GridProperties.CellLenghtY; y++)
            {
                for (var x = 0; x < _grid.GridProperties.CellLenghtX; x++)
                {
                    DPSPoint point = _grid.GetPoint(x, y);
                    if (point != null && point.Equals(_point))
                    {
                        Gizmos.DrawSphere(new Vector3(
                            (_grid.GridProperties.CellSize * x) + _grid.GridProperties.PositionOffset.x,
                            (_grid.GridProperties.CellSize * y) + _grid.GridProperties.PositionOffset.y
                        ), GridCellsRadius);
                    }
                }
            }
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