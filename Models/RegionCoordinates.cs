using UnityEngine;

namespace dd_andromeda_poisson_disk_sampling
{
    public readonly struct RegionCoordinates
    {
        public readonly int StartX;
        public readonly int EndX;
        public readonly int StartY;
        public readonly int EndY;

        private RegionCoordinates(int startX, int endX, int startY, int endY)
        {
            StartX = startX;
            EndX = endX;
            StartY = startY;
            EndY = endY;
        }
        
        public static RegionCoordinates Create(int searchSize, Vector2Int sourceCoordinate) => 
            Create(searchSize, sourceCoordinate.x, sourceCoordinate.y);

        public static RegionCoordinates Create(int searchSize, int x, int y)
        {
            return new RegionCoordinates(
                startX: x - searchSize,
                endX: x + searchSize,
                startY: y - searchSize,
                endY: y + searchSize);
        }
        
        public static RegionCoordinates Create(int searchSize, int x, int y,
            int startXLimit = int.MinValue, 
            int endXLimit = int.MaxValue, 
            int startYLimit = int.MinValue,
            int endYLimit= int.MaxValue)
        {
            return Create(
                searchSizeStart: searchSize,
                searchSizeEnd: searchSize,
                x: x, y: y,
                startXLimit: startXLimit, 
                endXLimit: endXLimit, 
                startYLimit: startYLimit,
                endYLimit: endYLimit);
        }
        
        public static RegionCoordinates Create(int searchSize, Vector2Int sourceCoordinate, 
            int startXLimit = int.MinValue, 
            int endXLimit = int.MaxValue, 
            int startYLimit = int.MinValue,
            int endYLimit= int.MaxValue)
        {
            return Create(
                searchSizeStart: searchSize,
                searchSizeEnd: searchSize,
                x: sourceCoordinate.x, y: sourceCoordinate.y,
                startXLimit: startXLimit, 
                endXLimit: endXLimit, 
                startYLimit: startYLimit,
                endYLimit: endYLimit);
        }
        
        public static RegionCoordinates Create(int searchSizeStart, int searchSizeEnd, int x, int y, 
            int startXLimit = int.MinValue, 
            int endXLimit = int.MaxValue, 
            int startYLimit = int.MinValue,
            int endYLimit= int.MaxValue)
        {
            return new RegionCoordinates(
                startX: Mathf.Max(startXLimit, x - searchSizeStart),
                startY: Mathf.Max(startYLimit, y - searchSizeStart),
                endX: Mathf.Min(x + searchSizeEnd, endXLimit),
                endY: Mathf.Min(y + searchSizeEnd, endYLimit)
            );
        }
    }
}