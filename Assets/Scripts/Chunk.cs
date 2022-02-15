using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int Width = 30;
    public static readonly int Height = 128;

    public Block[,,] BlockMap = new Block[Width, Height, Width];

    private World world;
    public ChunkCoord coord;
    public GameObject gameObj;
    protected Transform transform => gameObj.transform;
    public Vector3Int pos => Vector3Int.CeilToInt(transform.position);

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

        gameObj = new GameObject();
        meshRenderer = gameObj.AddComponent<MeshRenderer>();
        meshFilter = gameObj.AddComponent<MeshFilter>();
        meshCollider = gameObj.AddComponent<MeshCollider>();
        meshRenderer.material = world.material;

        gameObj.name = $"Chunk [{coord.x}, {coord.z}]";
        transform.SetParent(world.transform);
        transform.position = new Vector3(coord.x * Width, 0f, coord.z * Width);

        ActivateBlocks();
        CreateCube();
        CreateMesh();
    }

    // ? Currently Make simple rectangle chunk
    protected void ActivateBlocks()
    {
        foreach (var pos in BlockFullIterator())
            BlockMap[pos.x, pos.y, pos.z] = world.GenerateBlock(this.pos + pos);
    }

    protected void CreateCube()
    {
        foreach (var pos in BlockFullIterator())
        {
            var block = GetBlock(pos);
            if (block.IsSolid)
                MakeCube(block, pos);
        }
    }

    protected void MakeCube(Block block, in Vector3Int chunkCoord)
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

    public Block GetBlock(in Vector3Int v) => BlockMap[v.x, v.y, v.z];
}