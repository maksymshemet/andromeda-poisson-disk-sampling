using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public class GridProperties
    {
        public Vector2 Size { get; set; }
        public float CellSize { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
        public int Tries { get; set; }
    }
}