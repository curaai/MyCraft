using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static readonly int Width = 10;
    public static readonly int Height = 30;

    // ? Map type can be change to another types or seperated to `Sometype blockMap` and  `bool blockActivateMap`
    protected bool[,,] blockMap = new bool[Width, Height, Width]; // x, y, z

    protected List<Vector3> verts = new List<Vector3>();
    protected List<int> tris = new List<int>();
    protected List<Vector2> uvs = new List<Vector2>();

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    void Start()
    {
        ActivateBlocks();
        CreateCube();
        CreateMesh();
    }

    // ? Currently Make simple rectangle chunk
    protected void ActivateBlocks()
    {
        foreach (var pos in ChunkFullIterator())
        {
            blockMap[pos.x, pos.y, pos.z] = true;
        }
    }

    protected void CreateCube()
    {
        foreach (var pos in ChunkFullIterator())
        {
            if (blockMap[pos.x, pos.y, pos.z])
                MakeCube(transform.position + pos);
        }

    }

    protected void MakeCube(Vector3 pos)
    {
        void SetGrassBlock()
        {
            uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
            uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
            uvs.AddRange(TilePos.GetUVs(Tile.Grass));
            uvs.AddRange(TilePos.GetUVs(Tile.Dirt));
            uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
            uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
        }

        for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
        {
            int vertIdx = verts.Count;

            for (int i = 0; i < 4; i++)
                verts.Add(pos + VoxelData.Verts[VoxelData.Tris[faceIdx, i]]);

            foreach (var i in VoxelData.TriIdxOrder)
                tris.Add(vertIdx + i);
        }

        SetGrassBlock();
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
    public static IEnumerable<Vector3Int> ChunkFullIterator()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < Width; z++)
                    yield return new Vector3Int(x, y, z);
    }
}