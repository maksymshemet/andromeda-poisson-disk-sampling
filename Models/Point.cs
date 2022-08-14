using UnityEngine;

public class Point
{
    public readonly int PointIndex;
    public readonly Vector3 WorldPosition;
    public readonly float Radius;
    public readonly Vector2Int Cell;

    public Point(int pointIndex, Vector3 worldPosition, float radius, Vector2Int cell)
    {
        PointIndex = pointIndex;
        WorldPosition = worldPosition;
        Radius = radius;
        Cell = cell;
    }

    public override bool Equals(object obj)
    {
        if (obj is Point point)
        {
            return PointIndex.Equals(point.PointIndex) 
                   && WorldPosition.Equals(point.WorldPosition)
                   && Radius.Equals(point.Radius)
                   && Cell.Equals(point.Cell);
        }
            
        return false;
    }

    public override int GetHashCode()
    {
        return (PointIndex, WorldPosition, Radius, Cell).GetHashCode();
    }

    public override string ToString()
    {
        return $"Point: wp{WorldPosition}, c{Cell}, r{Radius}]";
    }
}

// public class PointWorld : Point
// {
//     public readonly Vector2Int ChunkPosition;
//
//     public PointWorld(Vector2Int chunkPosition, int pointIndex, Vector3 worldPosition, float radius, Vector2Int cell) : base(pointIndex, worldPosition, radius, cell)
//     {
//         ChunkPosition = chunkPosition;
//     }
//
//     public override bool Equals(object obj)
//     {
//         if (obj is PointWorld point)
//         {
//             return PointIndex.Equals(point.PointIndex) 
//                    && ChunkPosition.Equals(point.ChunkPosition)
//                    && WorldPosition.Equals(point.WorldPosition)
//                    && Radius.Equals(point.Radius)
//                    && Cell.Equals(point.Cell);
//         }
//             
//         return false;
//     }
//
//     public override int GetHashCode()
//     {
//         return (PointIndex, WorldPosition, Radius, Cell, ChunkPosition).GetHashCode();
//     }
//
//     public override string ToString()
//     {
//         return $"Point{ChunkPosition}: wp{WorldPosition}, c{Cell}, r{Radius}]";
//     }
// }

