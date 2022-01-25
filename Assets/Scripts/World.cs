using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{
    #region World Init
    public Material material;
    public static readonly int WorldChunkWidth = 100;
    public static readonly int WorldSizeInVoxels = WorldChunkWidth * Chunk.Width;
    #endregion

    #region User
    public Transform player;
    #endregion

    static readonly int ChunkViewDistance = 1;
    public Chunk[,] chunks = new Chunk[WorldChunkWidth, WorldChunkWidth];
    private void Start()
    {
        int ctr = WorldChunkWidth / 2;
        (int viewMin, int viewMax) = (ctr - ChunkViewDistance, ctr + ChunkViewDistance);
        for (int x = viewMin; x < viewMax; x++)
        {
            for (int z = viewMin; z < viewMax; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
            }
        }

        player.position = new Vector3(ctr * Chunk.Width, 120f, ctr * Chunk.Width);
    }


    public bool IsBlockInWorld(in Vector3 worldPos)
    {
        return (
            0 <= worldPos.x && worldPos.x <= WorldSizeInVoxels &&
            0 <= worldPos.z && worldPos.z <= WorldSizeInVoxels &&
            0 <= worldPos.y && worldPos.y <= Chunk.Height
        );
    }

    public Block GetVoxel(Vector3 worldPos)
    {
        int x = (int)worldPos.x;
        int y = (int)worldPos.y;
        int z = (int)worldPos.z;

        int chunkX = x / Chunk.Width;
        int chunkZ = z / Chunk.Width;

        x -= chunkX;
        z -= chunkZ;
        return chunks[chunkX, chunkZ].BlockMap[x, y, z];
    }

    public Block GenerateVoxel(Vector3 pos)
    {
        var block = new Block();

        void SetTerrarian()
        {
            int yPos = Mathf.FloorToInt(pos.y);
            float noise = Noise.Perlin(new Vector2(pos.x, pos.z), 0f, 0.1f);
            var terrianHeight = Mathf.FloorToInt(noise * Chunk.Height);
            if (yPos <= terrianHeight)
                block.IsSolid = true;
            else
                block.IsSolid = false;
            block.type = BlockType.Grass;
        }

        SetTerrarian();

        return block;
    }
}
