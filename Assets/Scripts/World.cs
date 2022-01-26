using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{
    #region World Init
    public Material material;
    public static readonly int WidthByChunk = 100;
    public static readonly int SizeByVoxels = WidthByChunk * Chunk.Width;
    #endregion

    #region User
    public Transform player;
    #endregion

    public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];
    private void Start()
    {
        int ctr = WidthByChunk / 2;

        player.position = new Vector3(ctr * Chunk.Width, 40f, ctr * Chunk.Width);
    }

    public bool IsBlockInWorld(in Vector3 worldPos)
    {
        return (
            0 <= worldPos.x && worldPos.x <= SizeByVoxels &&
            0 <= worldPos.z && worldPos.z <= SizeByVoxels &&
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

    public ChunkCoord GetChunkCoord(Vector3 worldPos) =>
        new ChunkCoord((int)worldPos.x / Chunk.Width, (int)worldPos.z / Chunk.Width);
}
