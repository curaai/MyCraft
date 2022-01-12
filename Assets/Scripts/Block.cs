using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    void Start()
    {
        int totalVertIdx = 0;
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int faceIdx = 0; faceIdx < VoxelData.FACE_COUNT; faceIdx++)
        {
            for (int i = 0; i < 4; i++)
                verts.Add(VoxelData.verts[VoxelData.tris[faceIdx, i]]);

            var idxList = new List<int> { 0, 1, 2, 2, 1, 3 };
            foreach (var i in idxList)
                tris.Add(totalVertIdx + i);
            totalVertIdx += 4;
        }

        uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
        uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
        uvs.AddRange(TilePos.GetUVs(Tile.Grass));
        uvs.AddRange(TilePos.GetUVs(Tile.Dirt));
        uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));
        uvs.AddRange(TilePos.GetUVs(Tile.GrassSide));

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}