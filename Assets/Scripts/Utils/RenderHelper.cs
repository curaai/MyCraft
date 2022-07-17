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

        public static Vector3Int FindCubeSizeFromTex(Texture2D tex, Vector2Int uvLeftTop)
        {
            int getDepth()
            {
                var i = 0;
                while (true)
                {
                    var color = tex.GetPixel(uvLeftTop.x, 63 - (uvLeftTop.y + i));
                    if (color != Color.clear)
                        return i;
                    i++;
                }
                throw new InvalidDataException("Cannot find depth from texture");
            }
            int getWidth(int depth)
            {
                var i = 0;
                while (true)
                {
                    var color = tex.GetPixel(uvLeftTop.x + depth + i, 63 - uvLeftTop.y);
                    if (color == Color.clear)
                        return i / 2;
                    i++;
                }
                throw new InvalidDataException("Cannot find width from texture");
            }
            int getHeight(int depth)
            {
                var i = 0;
                while (true)
                {
                    var color = tex.GetPixel(uvLeftTop.x, 63 - (uvLeftTop.y + depth + i));
                    if (color == Color.clear)
                        return i;
                    i++;
                }
                throw new InvalidDataException("Cannot find height from texture");
            }
            var depth = getDepth();
            return new Vector3Int(getWidth(depth), getHeight(depth), depth);
        }

        public static Vector2[][] GenerateCubeUvs(Vector2 texSize, Vector2Int uvLeftTop, Vector3Int size, bool mirror)
        {
            var texScale = new Vector2(1 / texSize.x, 1 / texSize.y);
            var th = texSize.y;
            var faces = new Rect[] {
                    // Back Face
                    new Rect(uvLeftTop.x + size.z,
                             th - (uvLeftTop.y + size.z) - size.y,
                             size.x,
                             size.y),
                    // Front Face
                    new Rect(uvLeftTop.x + size.z + size.x + size.z,
                             th - (uvLeftTop.y + size.z) - size.y,
                             size.x,
                             size.y),
                    // Top Face
                    new Rect(uvLeftTop.x + size.z,
                             th - (uvLeftTop.y)- size.z,
                             size.x,
                             size.z),
                    // Bottom Face
                    new Rect(uvLeftTop.x + size.z + size.x,
                             th - (uvLeftTop.y) -size.z,
                             size.x,
                             size.z),
                    // Left Face
                    new Rect(uvLeftTop.x,
                             th - (uvLeftTop.y + size.z) - size.y,
                             size.z,
                             size.y),
                    // Right Face
                    new Rect(uvLeftTop.x + size.z + size.x,
                             th - (uvLeftTop.y + size.z) -size.y,
                             size.z,
                             size.y),
                };

            Func<Vector2, Vector2> scaling = v => Vector2.Scale(v, texScale);
            Func<Vector2[], Vector2[]> mirroring = xs =>
            {
                if (mirror)
                {
                    (var minx, var maxx) = (xs[0].x, xs[2].x);
                    xs[0].x = xs[1].x = maxx;
                    xs[2].x = xs[3].x = minx;
                }
                return xs;
            };

            Func<Rect, Vector2[]> f = r => mirroring(RenderHelper.rect2vec(r)).Select(scaling).ToArray();
            return faces.Select(f).ToArray();
        }
    }
}