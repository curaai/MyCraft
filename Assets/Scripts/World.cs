using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WorldEnvironment;


public class World : MonoBehaviour
{
    public static readonly int WidthByChunk = 100;
    public static readonly int SizeByVoxels = WidthByChunk * Chunk.Width;

    [SerializeField]
    public Transform player;
    public BlockTable BlockTable;

    public Chunk[,] chunks = new Chunk[WidthByChunk, WidthByChunk];
    public BiomeAttribute[] biomes;
    public Queue<BlockMod> modificationQueue = new Queue<BlockMod>();

    private void Awake()
    {
        BlockTable = new BlockTable();
        biomes = Resources.LoadAll<BiomeAttribute>("Table/Biomes");

        Cursor.lockState = CursorLockMode.Locked;

        int ctr = WidthByChunk / 2;
        player.position = new Vector3(ctr * Chunk.Width, Chunk.Height - 20, ctr * Chunk.Width);
    }

    private void Update()
    {
        IEnumerator applyModification()
        {
            var buffer = new Queue<BlockMod>();
            while (0 < modificationQueue.Count)
            {
                var mod = modificationQueue.Dequeue();
                buffer.Enqueue(mod);

                var chunkCoord = ToChunkCoord(mod.pos).Item1;
                if (GetChunk(chunkCoord) == null)
                    chunks[chunkCoord.x, chunkCoord.z] = new Chunk(chunkCoord, this);

                if (buffer.Count > 200)
                {
                    EditBlock(buffer.ToList());
                    buffer.Clear();
                    yield return null;
                }
            }
        }

        StartCoroutine(applyModification());
    }

    public Block GenerateBlock(Vector3Int pos)
    {
        if (IsSolidBlock(pos)) return GetBlock(pos);

        float sumOfHeights = 0f;
        float strongestWeight = 0f;
        BiomeAttribute strongestBiome = biomes[0];

        foreach (var biome in biomes)
        {
            float weight = NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), biome.offset, biome.scale);
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiome = biome;
            }
            float height = biome.terrainHeight * NoiseHelper.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale) * weight;
            sumOfHeights += height;
        }
        return strongestBiome.GenerateBlock(pos, Mathf.FloorToInt(sumOfHeights / biomes.Length) + BiomeAttribute.BASE_GROUND_HEIGHT);
    }

    public void EditBlock(BlockMod mod)
    {
        GetChunk(mod.pos).EditBlock(new List<BlockMod>() { mod.ConvertInChunkCoord() });
    }

    public void EditBlock(List<BlockMod> mods)
    {
        var modGroups = mods.GroupBy(x => ToChunkCoord(x.pos).Item1).ToList();
        foreach (var modGroup in modGroups)
            GetChunk(modGroup.Key).EditBlock(modGroup.Select(x => x.ConvertInChunkCoord()).ToList());
    }

    public Block GetBlock(Vector3 worldPos)
    {
        var pair = ToChunkCoord(worldPos);
        return GetChunk(pair.Item1).GetBlock(pair.Item2);
    }

    public static (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3Int worldPos)
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
    public static (ChunkCoord, Vector3Int) ToChunkCoord(in Vector3 worldPos) => ToChunkCoord(Vector3Int.FloorToInt(worldPos));
    public Chunk GetChunk(Vector3Int worldPos) => GetChunk(ToChunkCoord(worldPos).Item1);
    public Chunk GetChunk(ChunkCoord coord) => chunks[coord.x, coord.z];
    public bool IsSolidBlock(in Vector3 worldPos)
    {
        var pos = ToChunkCoord(worldPos);
        if (pos.Item2.y < 0 || Chunk.Height <= pos.Item2.y)
            return false;

        if (GetChunk(pos.Item1) != null && GetBlock(worldPos) != null)
            return GetBlock(worldPos).isSolid;
        return false;
    }
}

public class BlockMod
{
    public Vector3Int pos;
    public Block block;

    public bool worldCoord;

    public BlockMod(Vector3Int _pos, Block _block, bool _worldCoord = true)
    {
        pos = _pos;
        block = _block;
        worldCoord = _worldCoord;
    }

    public BlockMod ConvertInChunkCoord()
    {
        if (worldCoord)
            return new BlockMod(World.ToChunkCoord(pos).Item2, block, false);
        throw new InvalidOperationException("Cannot convert to InChunkCoord");
    }
}