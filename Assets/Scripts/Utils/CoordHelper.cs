using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Utils
{
    public class CoordHelper
    {
        public static (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3Int worldPos)
        {
            int x = worldPos.x;
            int y = worldPos.y;
            int z = worldPos.z;

            int chunkX = x / Chunk.ChunkShape.x;
            int chunkZ = z / Chunk.ChunkShape.x;

            x -= chunkX * Chunk.ChunkShape.x;
            z -= chunkZ * Chunk.ChunkShape.x;

            var a = new ChunkCoord(chunkX, chunkZ);
            var b = new Vector3Int(x, y, z);
            return (a, b);
        }
        public static (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3 worldPos) => CoordHelper.ToChunkCoord(Vector3Int.FloorToInt(worldPos));

        public static IEnumerable<Vector3Int> ChunkIndexIterator()
        {
            for (int x = 0; x < Chunk.ChunkShape.x; x++)
                for (int y = 0; y < Chunk.ChunkShape.y; y++)
                    for (int z = 0; z < Chunk.ChunkShape.x; z++)
                        yield return new Vector3Int(x, y, z);
        }
    }
}