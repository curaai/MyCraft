using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{
    public static readonly int WidthByChunk = 100;
    public static readonly int SizeByVoxels = WidthByChunk * Chunk.Width;

    [SerializeField]
    public Material material;
    [SerializeField]
    public Transform player;

    public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];

    private void Start()
    {
        int ctr = WidthByChunk / 2;
        player.position = new Vector3(ctr * Chunk.Width, Chunk.Height - 20, ctr * Chunk.Width);
    }

    public Block GetBlock(Vector3 worldPos)
    {
        var pair = ToChunkCoord(worldPos);
        var chunkCoord = pair.Item1;
        var pos = pair.Item2;
        return chunks[chunkCoord.x, chunkCoord.z].BlockMap[pos.x, pos.y, pos.z];
    }

    public Block GenerateBlock(Vector3Int pos)
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

    public (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3Int worldPos)
    {
        int x = worldPos.x;
        int y = worldPos.y;
        int z = worldPos.z;

        int chunkX = x / Chunk.Width;
        int chunkZ = z / Chunk.Width;

        x -= chunkX * Chunk.Width;
        z -= chunkZ * Chunk.Width;

        var a = new ChunkCoord(chunkX, chunkZ);
        var b = new Vector3Int(x, y, z);
        return (a, b);
    }
    public (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3 worldPos) => ToChunkCoord(Vector3Int.FloorToInt(worldPos));
    public Chunk GetChunk(Vector3Int worldPos) => GetChunk(ToChunkCoord(worldPos).Item1);
    public Chunk GetChunk(ChunkCoord coord) => chunks[coord.x, coord.z];
    public bool IsSolidBlockInWorld(in Vector3 worldPos) => GetBlock(worldPos).IsSolid;
}
