using UnityEngine;

public class Point
{
    public Vector3 WorldPosition;
    public float Radius;
    public Vector2Int Cell;
    public bool IsIntersectWithPoint(Point point)
    {
        var sqrDst = (WorldPosition - point.WorldPosition).sqrMagnitude;
        var radius = Radius + point.Radius;
        return sqrDst < (radius * radius);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Point point)
        {
            return WorldPosition.Equals(point.WorldPosition)
                   && Radius.Equals(point.Radius)
                   && Cell.Equals(point.Cell);
        }
            
        return false;
    }

    public override int GetHashCode()
    {
        return (WorldPosition, Radius, Cell).GetHashCode();
    }

    public override string ToString()
    {
        return $"Point: wp{WorldPosition}, c{Cell}, r{Radius}]";
    }
}

public class PointWorld : Point
{
    public Vector2Int ChunkPosition;

    public override bool Equals(object obj)
    {
        if (obj is PointWorld point)
        {
            return ChunkPosition.Equals(point.ChunkPosition)
                   && WorldPosition.Equals(point.WorldPosition)
                   && Radius.Equals(point.Radius)
                   && Cell.Equals(point.Cell);
        }
            
        return false;
    }

    public override int GetHashCode()
    {
        return (WorldPosition, Radius, Cell, ChunkPosition).GetHashCode();
    }

    public override string ToString()
    {
        return $"Point{ChunkPosition}: wp{WorldPosition}, c{Cell}, r{Radius}]";
    }
}

