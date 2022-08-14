using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace dd_andromeda_poisson_disk_sampling.Demo
{
    public class StaticGridDemo : MonoBehaviour
    {
        public float Radius = 1;
        public Vector2Int RegionSize = new Vector2Int(5, 5);
        public int Tries = 20;
        
        [Header("Colors")]
        public Color GridColor = new Color(.5f, .5f, .5f, .5f);
        public Color PointColor = new Color(.5f, .5f, .5f, .5f);
        
        [Header("Trigger")] public bool Trigger;

        private List<Point> _points;
        private IGrid _staticAbstractGrid;
        private static bool _cachedTrigger;
        
        private void OnValidate()
        {
            if (_cachedTrigger != Trigger)
            {
                _staticAbstractGrid = Factory.CreateGrid(RegionSize, Radius, Tries);
                Random.InitState(24325);
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
            
            foreach (var point in _points)
            {
                Gizmos.color = PointColor;
                Gizmos.DrawSphere(point.WorldPosition, point.Radius);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point.WorldPosition, 0.03f);
            }
        }
        
        private void ShowGrid()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(new Vector3(RegionSize.x / 2, RegionSize.y / 2), 
                new Vector3(RegionSize.x, RegionSize.y));
            Gizmos.color = GridColor;

            // var offsetX = _staticGrid.CellSize;
            // var offsetY = _staticGrid.CellSize;
            //
            // for (int x = 0; x <= _staticGrid.Width; x++)
            // {
            //     var from = new Vector2(0 + offsetX * x, 0);
            //     var to = new Vector2(0 + offsetX * x, RegionSize.y);
            //
            //     Gizmos.DrawLine(@from, to);
            // }
            //
            // for (int y = 0; y <= _staticGrid.Height; y++)
            // {
            //     var from = new Vector2(0, 0 + offsetY * y);
            //     var to = new Vector2(RegionSize.x, 0 + offsetY * y);
            //
            //     Gizmos.DrawLine(@from, to);
            // }
        }
    }
}