using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public struct Candidate
    {
        public float Radius;
        public Vector3 WorldPosition;
        public Vector2Int Cell;
    }
}