using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public static readonly int Width = 10;
    public static readonly int Height = 30;

    // ? Map type can be change to another types or seperated to `Sometype blockMap` and  `bool blockActivateMap`
    public Block[,,] BlockMap = new Block[Width, Height, Width];

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
            BlockMap[pos.x, pos.y, pos.z] = new Block();
            BlockMap[pos.x, pos.y, pos.z].IsSolid = true;
        }
    }

    protected void CreateCube()
    {
        bool IsBoundaryBlock(Vector3Int pos)
        {
            foreach (var normal in VoxelData.SurfaceNormal)
            {
                var outerBlock = pos + normal;
                if (outerBlock.x < 0) return true;
                if (outerBlock.y < 0) return true;
                if (outerBlock.z < 0) return true;
                if (outerBlock.x >= Width) return true;
                if (outerBlock.y >= Height) return true;
                if (outerBlock.z >= Width) return true;
            }
            return false;
        }

        foreach (var pos in ChunkFullIterator())
            if (BlockMap[pos.x, pos.y, pos.z].IsSolid && IsBoundaryBlock(pos))
                MakeCube(transform.position + pos);
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