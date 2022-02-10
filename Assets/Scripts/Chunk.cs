using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    #region World
    private World world;
    public ChunkCoord coord;
    #endregion

    #region BlockMap
    public static readonly int Width = 30;
    public static readonly int Height = 128;

    // ? Map type can be change to another types or seperated to `Sometype blockMap` and  `bool blockActivateMap`
    public Block[,,] BlockMap = new Block[Width, Height, Width];
    #endregion

    #region Render

    protected List<Vector3> verts = new List<Vector3>();
    protected List<int> tris = new List<int>();
    protected List<Vector2> uvs = new List<Vector2>();

    protected MeshRenderer meshRenderer;
    protected MeshFilter meshFilter;
    protected MeshCollider meshCollider;
    #endregion

    public GameObject chunkObject;
    protected Transform chunkTransform;
    public Vector3 chunkPos { get => chunkTransform.position; }

    public Chunk(ChunkCoord coord, World world)
    {
        this.world = world;
        this.coord = coord;

        chunkObject = new GameObject();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer.material = world.material;

        chunkObject.name = $"Chunk [{coord.x}, {coord.z}]";
        chunkTransform = chunkObject.transform;
        chunkTransform.SetParent(world.transform);
        chunkTransform.position = new Vector3(coord.x * Width, 0f, coord.z * Width);

        ActivateBlocks();
        CreateCube();
        CreateMesh();
    }

    // ? Currently Make simple rectangle chunk
    protected void ActivateBlocks()
    {
        foreach (var pos in BlockFullIterator())
        {
            BlockMap[pos.x, pos.y, pos.z] = world.GenerateVoxel(chunkTransform.position + pos);
        }
    }

    protected void CreateCube()
    {
        foreach (var pos in BlockFullIterator())
        {
            var block = BlockMap[pos.x, pos.y, pos.z];
            if (block.IsSolid)
                MakeCube(block, pos);
        }
    }

    protected void MakeCube(Block block, Vector3Int chunkCoord)
    {
        bool IsEmptyBlock(Vector3Int coord)
        {
            if (coord.x < 0 || Width <= coord.x ||
                coord.z < 0 || Width <= coord.z ||
                coord.y < 0 || Height <= coord.y)
                return true;
            return !GetBlock(coord).IsSolid;
        }

        for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
        {
            if (block.IsSolid && IsEmptyBlock(chunkCoord + VoxelData.SurfaceNormal[faceIdx]))
            {
                int vertIdx = verts.Count;

                for (int i = 0; i < 4; i++)
                    verts.Add(chunkCoord + VoxelData.Verts[VoxelData.Tris[faceIdx, i]]);

                foreach (var i in VoxelData.TriIdxOrder)
                    tris.Add(vertIdx + i);
                uvs.AddRange(block.GetTexture((VoxelFace)faceIdx));
            }
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    public static IEnumerable<Vector3Int> BlockFullIterator()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Width; z++)
                    yield return new Vector3Int(x, y, z);
    }

    public Block GetBlock(Vector3Int v)
    {
        return BlockMap[v.x, v.y, v.z];
    }
}

public readonly struct ChunkCoord
{
    public readonly int x;
    public readonly int z;

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(in ChunkCoord temp)
    {
        if (temp == null)
            return false;
        return (x == temp.x) && (z == temp.z);
    }
    public static bool operator ==(ChunkCoord a, ChunkCoord b) => a.Equals(b);
    public static bool operator !=(ChunkCoord a, ChunkCoord b) => !(a == b);

}