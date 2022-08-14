using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace dd_andromeda_poisson_disk_sampling.Demo
{
    [ExecuteInEditMode]
    public class StaticMultiRadGridDemo : MonoBehaviour
    {
        public float MinRadius = 1;
        public float MaxRadius = 2;
        public Vector2 Offset;
        public AnimationCurve RadiusCurve;
        
        public Vector2Int RegionSize = new Vector2Int(10, 10);
        public int Tries = 20;
        
        [Header("Colors")]
        public Color GridColor = new Color(.5f, .5f, .5f, .5f);
        public Color PointColor = new Color(.5f, .5f, .5f, .5f);

        
        [Header("Trigger")] public bool Trigger;

        private List<Point> _points;
        private IGrid _staticAbstractGrid;

        private static bool _cachedTrigger;


        private void Update()
        {
            Debug.Log("Edit cause this");
        }

        private void OnValidate()
        {
            if (_cachedTrigger != Trigger)
            {
                _staticAbstractGrid = Factory.CreateGrid(RegionSize, MinRadius, MaxRadius, Tries, RadiusCurve);//new MultiRadGrid(RegionSize, MinRadius, MaxRadius, Tries, RadiusCurve)
                _staticAbstractGrid.WorldPositionOffset = Offset;
                // {
                //     WorldPositionOffset = Offset
                // };
                // Random.InitState(24325);
                var sw = new Stopwatch();
                sw.Start();
                _points = _staticAbstractGrid.Fill();
                sw.Stop();
                
                Debug.LogWarning($"Benchmark: {sw.Elapsed.TotalMilliseconds} ms ({_points.Count} points)");
                
                _cachedTrigger = Trigger;
            }
        }

        private void OnDrawGizmos()
        {
            if (_staticAbstractGrid == null) return;
            
            ShowGrid();

            if (_points == null) return;
            
            foreach (var point in _points)
            {
                Gizmos.color = PointColor;
                Gizmos.DrawSphere(point.WorldPosition, point.Radius);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point.WorldPosition, 0.03f);
            }
            
            // for (int x = 0; x < _staticGrid.Width; x++)
            // {
            //     for (int y = 0; y < _staticGrid.Height; y++)
            //     {
            //         // if (_staticGrid.CellValue(x, y) != 0)
            //         if (!_staticGrid.IsCellEmpty(x, y))
            //         {
            //             Gizmos.color = Color.yellow;
            //             Gizmos.DrawSphere(
            //                 new Vector2(
            //                     _staticGrid.WorldPositionOffset.x + (x * _staticGrid.CellSize), 
            //                     _staticGrid.WorldPositionOffset.y+ (y * _staticGrid.CellSize)),0.04f);
            //             
            //             // Gizmos.color = Color.green;
            //             // Gizmos.DrawSphere(new Vector2((x + 1) * _cellSize, (y + 1) * _cellSize),0.04f);
            //             // Gizmos.DrawSphere(new Vector2((x + 0) * _cellSize, (y + 1) * _cellSize),0.04f);
            //             // Gizmos.DrawSphere(new Vector2((x + 1) * _cellSize, (y + 0) * _cellSize),0.04f);
            //         }
            //     }
            // }
        }
        
        private void ShowGrid()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(
                new Vector3((RegionSize.x / 2f) + _staticAbstractGrid.WorldPositionOffset.x, 
                    (RegionSize.y / 2f) + _staticAbstractGrid.WorldPositionOffset.y), 
                new Vector3(RegionSize.x, RegionSize.y));
            Gizmos.color = GridColor;

            var offsetX = _staticAbstractGrid.GridProperties.CellSize;
            var offsetY = _staticAbstractGrid.GridProperties.CellSize;
            
            for (int x = 0; x <= _staticAbstractGrid.GridProperties.CellWidth; x++)
            {
                var from = new Vector2(_staticAbstractGrid.WorldPositionOffset.x + offsetX * x, _staticAbstractGrid.WorldPositionOffset.y);
                var to = new Vector2(_staticAbstractGrid.WorldPositionOffset.x + offsetX * x, _staticAbstractGrid.WorldPositionOffset.y + RegionSize.y);

                Gizmos.DrawLine(@from, to);
            }

            for (int y = 0; y <= _staticAbstractGrid.GridProperties.CellHeight; y++)
            {
                var from = new Vector2(_staticAbstractGrid.WorldPositionOffset.x, _staticAbstractGrid.WorldPositionOffset.y + offsetY * y);
                var to = new Vector2(_staticAbstractGrid.WorldPositionOffset.x + RegionSize.x, _staticAbstractGrid.WorldPositionOffset.y + offsetY * y);

                Gizmos.DrawLine(@from, to);
            }
        }
    }
}