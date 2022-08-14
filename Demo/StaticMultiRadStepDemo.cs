using System;
using System.Collections;
using System.Collections.Generic;
using dd_andromeda_poisson_disk_sampling.Propereties;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace dd_andromeda_poisson_disk_sampling.Demo
{
    [ExecuteInEditMode]
    public class StaticMultiRadStepDemo : MonoBehaviour
    {
        public float MinRadius = 1;
        public float MaxRadius = 2;
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
        private static bool _gridChanged;

        private void Update()
        {
            if(!_gridChanged) return;

            foreach (var gameObject in GetComponentsInChildren<Transform>())
            {
                if(transform == gameObject.transform) continue;
                DestroyImmediate(gameObject.gameObject);
            }

            
            _points = new List<Point>();
            if (_staticAbstractGrid.TrySpawnPointAt(
                    new Vector3(_staticAbstractGrid.GridProperties.Size.x / 2, _staticAbstractGrid.GridProperties.Size.y / 2), out var point))
            {
                _points.Add(point);
                var go = new GameObject("New Point");
                go.transform.parent = transform;
                go.transform.position = point.WorldPosition;

                var c = go.AddComponent<DemoStepRadPoint>();
                c.Point = point;
                c.AbstractGrid = _staticAbstractGrid;
                c.Demo = this;
            }
            
            _gridChanged = false;
        }

        public void Spawn(Point p)
        {
            if (_staticAbstractGrid.TrySpawnPointFrom(p, out var point))
            {
                _points.Add(point);
                var go = new GameObject("New Point");
                go.transform.parent = transform;
                go.transform.position = point.WorldPosition;

                var c = go.AddComponent<DemoStepRadPoint>();
                c.Point = point;
                c.AbstractGrid = _staticAbstractGrid;
                c.Demo = this;
            }
        }
        
        private IEnumerator CreateCoroutine()
        {
            var pos = new Vector3(_staticAbstractGrid.GridProperties.Size.x / 2, _staticAbstractGrid.GridProperties.Size.y / 2);
            while (_staticAbstractGrid.TrySpawnPointAt(pos, out var point))
            {
                _points.Add(point);
                pos = point.WorldPosition;
                // yield return new WaitForSeconds(0.2f);
                yield return new EditorWaitForSeconds(0.02f);
            }
        }
        
        private void OnValidate()
        {
            if (_cachedTrigger != Trigger)
            {
                _staticAbstractGrid = Factory
                    .CreateGrid(RegionSize, MinRadius, MaxRadius, Tries, RadiusCurve);
                _cachedTrigger = Trigger;
                _gridChanged = true;
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
            
            // for (int x = 0; x < _staticGrid.Width; x++)
            // {
            //     for (int y = 0; y < _staticGrid.Height; y++)
            //     {
            //         if (_staticGrid.Grid2[x, y] != 0)
            //         {
            //             Gizmos.color = Color.yellow;
            //             Gizmos.DrawSphere(
            //                 new Vector2(x * _staticGrid.CellSize, y * _staticGrid.CellSize),0.04f);
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
            Gizmos.DrawWireCube(new Vector3(RegionSize.x / 2, RegionSize.y / 2), 
                new Vector3(RegionSize.x, RegionSize.y));
            Gizmos.color = GridColor;

            var offsetX = _staticAbstractGrid.GridProperties.CellSize;
            var offsetY = _staticAbstractGrid.GridProperties.CellSize;
            
            for (int x = 0; x <= _staticAbstractGrid.GridProperties.CellWidth; x++)
            {
                var from = new Vector2(0 + offsetX * x, 0);
                var to = new Vector2(0 + offsetX * x, RegionSize.y);

                Gizmos.DrawLine(@from, to);
            }

            for (int y = 0; y <= _staticAbstractGrid.GridProperties.CellHeight; y++)
            {
                var from = new Vector2(0, 0 + offsetY * y);
                var to = new Vector2(RegionSize.x, 0 + offsetY * y);

                Gizmos.DrawLine(@from, to);
            }
        }
    }
}