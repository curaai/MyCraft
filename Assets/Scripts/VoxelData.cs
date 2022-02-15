using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    public static readonly int FACE_COUNT = 6;

    public static readonly Vector3Int[] Verts = new Vector3Int[8] {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1),
    };

    public static readonly int[,] Tris = new int[6, 4]
    {
        {0, 3, 1, 2}, // Back Face   (-Z)
		{5, 6, 4, 7}, // Front Face  (+Z)
		{3, 7, 2, 6}, // Top Face    (+Y)
		{1, 5, 0, 4}, // Bottom Face (-Y)
		{4, 7, 0, 3}, // Left Face   (-X)
		{1, 2, 5, 6} // Right Face  (+X)
    };

    public static readonly Vector3Int[] SurfaceNormal = new Vector3Int[6]
    {
        Vector3Int.back,
        Vector3Int.forward,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
    };

    // ! Left Triangle {0, 1, 2}, Right Triangle {2, 1, 3}
    public static readonly int[] TriIdxOrder = new int[6] { 0, 1, 2, 2, 1, 3 };

    public static readonly Vector2Int[] Uvs = new Vector2Int[4]
    {
        new Vector2Int (0, 0),
        new Vector2Int (0, 1),
        new Vector2Int (1, 0),
        new Vector2Int (1, 1)
    };
}

public enum VoxelFace { BACK, FRONT, TOP, BOTTOM, LEFT, RIGHT };
