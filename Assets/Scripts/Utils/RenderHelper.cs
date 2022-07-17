using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MyCraft.Utils
{
    public static class RenderHelper
    {
        public static Vector3[] GenerateVerts(Vector3 min, Vector3 max)
        {
            return new Vector3[] {
                new Vector3(min.x, min.y, min.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, max.z),
                new Vector3(min.x, max.y, max.z)
            };
        }
        public static Vector2[] GetPatchedAtlasUv(Rect atlas, Rect patch)
        {
            var minAtlas = atlas.min + atlas.size * patch.min;
            var maxAtlas = atlas.min + atlas.size * patch.max;
            return new Vector2[] {
                minAtlas,
                new Vector2 (minAtlas.x, maxAtlas.y),
                new Vector2 (maxAtlas.x, minAtlas.y),
                maxAtlas,
            };
        }
        public static Vector2[] rect2vec(Rect uv)
        {
            return new Vector2[] {
                    new Vector2(uv.xMin, uv.yMin),
                    new Vector2(uv.xMin, uv.yMax),
                    new Vector2(uv.xMax, uv.yMin),
                    new Vector2(uv.xMax, uv.yMax) };
        }
    }
}