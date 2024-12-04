using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Grids;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models;
using UnityEngine;

namespace andromeda_poisson_disk_sampling.Demo2
{
    public class PointMultiRadSphere : MonoBehaviour
    {
        private Point _point;
        private IGrid _grid;
        
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
        
        public void Init(Point point, IGrid grid)
        {
            name = $"[{point.WorldPosition}] r{point.Radius}";
            transform.position = point.WorldPosition;
            transform.localScale = new Vector3(point.Radius, point.Radius, point.Radius) * 2;

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
            Gizmos.DrawWireSphere(transform.position, _point.Radius);
        }

        private void OnDrawGizmos()
        {
            if (_point == null) return;
            
            Gizmos.color = MarginColor;
            Gizmos.DrawWireSphere(transform.position, _point.Radius + _point.Margin);
            
            Gizmos.color = RadiusColor;
            Gizmos.DrawWireSphere(transform.position, _point.Radius);
            
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
            
            int searchSize = Mathf.RoundToInt((_point.Radius + _point.Margin) / _grid.GridProperties.CellSize);
            int startX = Mathf.Max(0, _point.CellMin.x - searchSize);
            int endX = Mathf.Min(_point.CellMax.x + searchSize, _grid.GridProperties.CellLenghtX - 1);
            int startY = Mathf.Max(0, _point.CellMin.y - searchSize);
            int endY = Mathf.Min(_point.CellMax.y + searchSize, _grid.GridProperties.CellLenghtY - 1);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var pos = new Vector3(
                        x: _grid.GridProperties.CellSize * x + _grid.GridProperties.PositionOffset.x,
                        y: _grid.GridProperties.CellSize * y + _grid.GridProperties.PositionOffset.y);
                    Gizmos.DrawWireSphere(pos, SearchRangeRadius);
                }
            }
        }
        
        private void GridCellsShowFunc()
        {
            if(!GridCellsShow) return;

            Gizmos.color = GridCellsColor;

            for (var y = 0; y < _grid.GridProperties.CellLenghtX; y++)
            {
                for (var x = 0; x < _grid.GridProperties.CellLenghtX; x++)
                {
                    Point point = _grid.GetPoint(x, y);
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
            
            Gizmos.DrawWireSphere(transform.position, _point.Radius * 2);
            Gizmos.DrawWireSphere(transform.position, _point.Radius * 3);
        }
    }
}