using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
    public static readonly int FACE_COUNT = 6;
    public static readonly int VERTEX_COUNT = 6;

    public static readonly Vector3[] verts = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };

    public static readonly int[,] tris = new int[6, 4]
    {
        {0, 3, 1, 2}, // Back Face   (-Z)
		{5, 6, 4, 7}, // Front Face  (+Z)
		{3, 7, 2, 6}, // Top Face    (+Y)
		{1, 5, 0, 4}, // Bottom Face (-Y)
		{4, 7, 0, 3}, // Left Face   (-X)
		{1, 2, 5, 6} // Right Face  (+X)
    };

    // LUT - uv data
    public static readonly Vector2[] uvs = new Vector2[4]
    {
        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)
    };
}
