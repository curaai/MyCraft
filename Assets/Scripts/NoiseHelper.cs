using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseHelper
{
    public static float Perlin(Vector2 pos, float offset, float scale)
    {
        return Mathf.PerlinNoise(
            (pos.x + 0.1f) / Chunk.Width * scale + offset,
            (pos.y + 0.1f) / Chunk.Width * scale + offset
        );
    }
}