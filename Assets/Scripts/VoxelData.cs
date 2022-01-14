using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    public static readonly int FACE_COUNT = 6;
    public static readonly int VERTEX_COUNT = 6;

    public static readonly Vector3[] Verts = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
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

    public static readonly Vector2[] Uvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
