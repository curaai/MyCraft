using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyCraft.Utils
{
    public static class NoiseHelper
    {
        public static float Get2DPerlin(Vector2 pos, float offset, float scale)
        {
            return Mathf.PerlinNoise(
                (pos.x + 0.1f) / Chunk.Width * scale + offset,
                (pos.y + 0.1f) / Chunk.Width * scale + offset
            );
        }

        public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
        {
            // https://www.youtube.com/watch?v=Aga0TBJkchM Carpilot on YouTube

            float x = (position.x + offset + 0.1f) * scale;
            float y = (position.y + offset + 0.1f) * scale;
            float z = (position.z + offset + 0.1f) * scale;

            var _list = new List<(float, float)> { (x, y), (y, x), (x, z), (z, x), (y, z), (z, y) };
            var res = (from a in _list select Mathf.PerlinNoise(a.Item1, a.Item2)).Average();
            return res > threshold;
        }
    }
}
