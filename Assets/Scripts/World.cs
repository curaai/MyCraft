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

        player.position = new Vector3(ctr * Chunk.Width, Chunk.Height - 20, ctr * Chunk.Width);
    }

    public bool IsSolidBlockInWorld(in Vector3 worldPos)
    {
        return GetBlock(worldPos).IsSolid;
    }

    public Block GetBlock(Vector3 worldPos)
    {
        int x = (int)worldPos.x;
        int y = (int)worldPos.y;
        int z = (int)worldPos.z;

        int chunkX = x / Chunk.Width;
        int chunkZ = z / Chunk.Width;

        x -= chunkX * Chunk.Width;
        z -= chunkZ * Chunk.Width;
        return chunks[chunkX, chunkZ].BlockMap[x, y, z];
    }

    public Block GenerateVoxel(Vector3 pos)
    {
        Block TerrarianBlock()
        {
            int yPos = Mathf.FloorToInt(pos.y);
            float noise = NoiseHelper.Perlin(new Vector2(pos.x, pos.z), 0f, 0.1f);
            var terrianHeight = Mathf.FloorToInt(noise * Chunk.Height);

            Block block;
            if (yPos <= terrianHeight)
            {
                block = new Block();
                block.type = BlockType.Grass;
                block.IsSolid = true;
            }
            else
            {
                block = new Blocks.Air();
            }
            return block;
        }

        return TerrarianBlock();
    }

    public ChunkCoord GetChunkCoord(Vector3 worldPos) =>
        new ChunkCoord((int)worldPos.x / Chunk.Width, (int)worldPos.z / Chunk.Width);
}
