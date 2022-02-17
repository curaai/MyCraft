using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int Width = 30;
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
        get => gameObj != null ? gameObj.activeSelf : false;
        set => gameObj.SetActive(value);
    }

    protected List<Vector3> verts = new List<Vector3>();
    protected List<int> tris = new List<int>();
    protected List<Vector2> uvs = new List<Vector2>();
    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected MeshCollider meshCollider;

    public Chunk(ChunkCoord coord, World world)
    {

        this.world = world;
        this.coord = coord;

        Init();
    }

    public void Init()
    {
        void GenerateBlocks()
        {
            foreach (var pos in BlockFullIterator())
                BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(chunkPos + pos);
        }

        if (gameObj != null)
            return;

        gameObj = new GameObject();
        meshRenderer = gameObj.AddComponent<MeshRenderer>();
        meshFilter = gameObj.AddComponent<MeshFilter>();
        meshCollider = gameObj.AddComponent<MeshCollider>();
        meshRenderer.material = world.material;

        gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
        transform.SetParent(world.transform);
        transform.position = new Vector3(coord.x * Width, 0f, coord.z * Width);

        GenerateBlocks();
        UpdateChunk();
    }

    protected void UpdateChunk()
    {
        ClearMesh();

        foreach (var pos in BlockFullIterator())
        {
            if (GetBlock(pos).IsSolid)
                UpdateMeshBlock(pos);
        }

        CreateMesh();
    }

    protected void UpdateMeshBlock(Vector3Int inChunkCoord)
    {
        var block = GetBlock(inChunkCoord);

        for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
        {
            if (!IsSolidBlock(inChunkCoord + VoxelData.SurfaceNormal[faceIdx]))
            {
                int vertIdx = verts.Count;
                for (int i = 0; i < 4; i++)
                    verts.Add(inChunkCoord + VoxelData.Verts[VoxelData.Tris[faceIdx, i]]);

                foreach (var i in VoxelData.TriIdxOrder)
                    tris.Add(vertIdx + i);
                uvs.AddRange(block.GetTexture((VoxelFace)faceIdx));
            }
        }
    }

    public void EditBlock(Block newBlock, Vector3Int coord)
    {
        void UpdateSurroundedChunks()
        {
            for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
            {
                var faceCoord = coord + VoxelData.SurfaceNormal[faceIdx];
                if (!IsVoxelInChunk(faceCoord))
                    world.GetChunk(chunkPos + faceCoord).UpdateChunk();
            }
        }

        BlockMap[coord.x, coord.y, coord.z] = newBlock;
        UpdateSurroundedChunks();
        UpdateChunk();
    }

    protected void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    protected void ClearMesh()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
    }

    public static IEnumerable<Vector3Int> BlockFullIterator()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Width; z++)
                    yield return new Vector3Int(x, y, z);
    }

    public bool IsSolidBlock(in Vector3Int vp)
    {
        if (IsVoxelInChunk(vp)) return GetBlock(vp).IsSolid;
        return world.IsSolidBlock(chunkPos + vp);
    }

    public Block GetBlock(in Vector3Int v) => BlockMap[v.x, v.y, v.z];
    protected bool IsVoxelInChunk(in Vector3Int v) => v == Vector3Int.Max(Vector3Int.zero, Vector3Int.Min(ChunkShape, v));
}