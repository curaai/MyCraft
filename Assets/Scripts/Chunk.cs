using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int Width = 16;
    public static readonly int Height = 128;

    public Block[,,] BlockMap = new Block[Width, Height, Width];
    protected static readonly Vector3Int ChunkShape = new Vector3Int(Width - 1, Height - 1, Width - 1);

    protected World world;
    public ChunkCoord coord;
    public GameObject gameObj;
    protected Transform transform => gameObj.transform;
    public Vector3Int chunkPos => Vector3Int.CeilToInt(transform.position);
    public bool Activated
    {
        get => gameObj.activeSelf;
        set => gameObj.SetActive(value);
    }

    public ChunkRenderer renderer;

    public bool Initialized { get; private set; }
    public Chunk(ChunkCoord coord, World world)
    {
        this.world = world;
        this.coord = coord;

        gameObj = new GameObject();
        gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
        transform.SetParent(world.transform);
        transform.position = new Vector3(coord.x * Width, 0f, coord.z * Width);

        Initialized = false;
        renderer = new ChunkRenderer(this, world.BlockTable);
    }

    public void Init()
    {
        void GenerateBlocks()
        {
            foreach (var pos in BlockFullIterator())
                BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(chunkPos + pos);
        }

        if (Initialized)
            return;
        GenerateBlocks();
        renderer.RefreshMesh();
        Initialized = true;
    }

    public void EditBlock(List<BlockMod> mods)
    {
        void UpdateSurroundedChunks(Vector3Int coord)
        {
            for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
            {
                var faceCoord = coord + VoxelData.SurfaceNormal[faceIdx];
                if (!IsVoxelInChunk(faceCoord))
                    world.GetChunk(chunkPos + faceCoord)?.renderer.RefreshMesh();
            }
        }

        foreach (var mod in mods)
        {
            var coord = mod.pos;
            BlockMap[coord.x, coord.y, coord.z] = mod.block;
            UpdateSurroundedChunks(coord);
        }
        renderer.RefreshMesh();
    }


    public bool IsSolidBlock(in Vector3Int vp)
    {
        if (IsVoxelInChunk(vp)) return GetBlock(vp).isSolid;
        return world.IsSolidBlock(chunkPos + vp);
    }

    public Block GetBlock(in Vector3Int v) => BlockMap[v.x, v.y, v.z];
    protected bool IsVoxelInChunk(in Vector3Int v) => v == Vector3Int.Max(Vector3Int.zero, Vector3Int.Min(ChunkShape, v));

    public static IEnumerable<Vector3Int> BlockFullIterator()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Width; z++)
                    yield return new Vector3Int(x, y, z);
    }
}