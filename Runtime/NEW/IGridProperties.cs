using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling.Propereties
{
    public interface IGridProperties
    {
        Vector2 Size {get; }
        float CellSize {get; }
        int CellWidth {get; }
        int CellHeight {get; }
    }
}