using System;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Properties;
using DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Worlds;
using UnityEngine;

namespace DarkDynamics.Andromeda.PoissonDiskSampling.Runtime.Models
{
    public class WorldCoordinates
    {
        public Vector2Int ChunkPosition;
        public Vector2Int CellPosition;

        public WorldCoordinates()
        {
            
        }

        public WorldCoordinates(in WorldAbstract world, Vector2Int cell)
        {
            Tuple<int, int> tupleX = CalculateAxeCoordinate(
                originalCell: 0,
                originalChunk: ChunkPosition.x,
                chunkLength: world.GridProperties.CellLenghtX,
                offset: cell.x);
            
            Tuple<int, int> tupleY = CalculateAxeCoordinate(
                originalCell: 0,
                originalChunk: ChunkPosition.y,
                chunkLength: world.GridProperties.CellLenghtY,
                offset: cell.y);

            ChunkPosition = new Vector2Int(x: tupleX.Item1, y: tupleY.Item1);
            CellPosition = new Vector2Int(x: tupleX.Item2, y: tupleY.Item2);
        }
        
        
        public WorldCoordinates(in WorldAbstract world, Vector2Int chunk, Vector2Int cell)
        {
            Tuple<int, int> tupleX = CalculateAxeCoordinate(
                originalCell: 0,
                originalChunk: chunk.x,
                chunkLength: world.GridProperties.CellLenghtX,
                offset: cell.x);
            
            Tuple<int, int> tupleY = CalculateAxeCoordinate(
                originalCell: 0,
                originalChunk: chunk.y,
                chunkLength: world.GridProperties.CellLenghtY,
                offset: cell.y);

            ChunkPosition = new Vector2Int(x: tupleX.Item1, y: tupleY.Item1);
            CellPosition = new Vector2Int(x: tupleX.Item2, y: tupleY.Item2);
        }
        
        public WorldCoordinates Offset(in WorldAbstract world, int cellOffsetX, int cellOffsetY)
        {
            Tuple<int, int> tupleX = CalculateAxeCoordinate(
                originalCell: CellPosition.x,
                originalChunk: ChunkPosition.x,
                chunkLength: world.GridProperties.CellLenghtX,
                offset: cellOffsetX);
            
            Tuple<int, int> tupleY = CalculateAxeCoordinate(
                originalCell: CellPosition.y,
                originalChunk: ChunkPosition.y,
                chunkLength: world.GridProperties.CellLenghtY,
                offset: cellOffsetY);
            
            return new WorldCoordinates
            {
                ChunkPosition = new Vector2Int(x: tupleX.Item1, y: tupleY.Item1),
                CellPosition = new Vector2Int(x: tupleX.Item2, y: tupleY.Item2),
            };
        }

        public override string ToString()
        {
            return $"[{ChunkPosition}] {CellPosition}";
        }
        
        private Tuple<int, int> CalculateAxeCoordinate(int originalCell, int originalChunk,
            int chunkLength, int offset)
        {
            int cellX = originalCell + offset;
            int chunkX = originalChunk;
            switch (cellX)
            {
                case > 0:
                {
                    if (cellX >= chunkLength)
                    {
                        int chunkOffset = cellX / chunkLength;
                        int cellOffset = cellX % chunkLength;

                        chunkX += chunkOffset;
                        cellX = cellOffset;
                    }

                    break;
                }
                case < 0:
                {
                    int chunkOffset = cellX / chunkLength - 1;
                    int cellOffset = cellX % chunkLength;

                    chunkX += chunkOffset;
                    cellX = chunkLength + cellOffset - 1;
                    break;
                }
            }

            return Tuple.Create(chunkX, cellX);
        }
    }
}