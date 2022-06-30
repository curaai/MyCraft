using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyCraft.Rendering
{

    public struct BlockTextureModel
    {
        public BlockTextureModel(List<Element> elements)
        {
            Vector3[] GenerateVerts(Vector3 min, Vector3 max)
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
            Vector2[] GetPatchedAtlasUv(Rect atlas, Rect patch)
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

            this.elements = elements;
            this.verts = new List<Vector3>();
            this.tris = new List<int>();
            this.uvs = new List<Vector2>();

            foreach (var elem in elements)
            {
                var vertList = GenerateVerts(elem.from, elem.to);
                if (elem.rotation.HasValue)
                {
                    var rotation = elem.rotation.Value;
                    vertList = vertList.Select(v => Quaternion.AngleAxis(rotation.angle, rotation.axis) * (v - rotation.pivot) + rotation.pivot).ToArray();
                }
                foreach (var enumFace in Enum.GetValues(typeof(VoxelFace)))
                {
                    var face = enumFace.ToString().ToLower();
                    var vertIdx = verts.Count;
                    if (elem.patchUvDict.ContainsKey(face))
                    {
                        for (int i = 0; i < 4; i++)
                            verts.Add(vertList[VoxelData.Tris[(int)enumFace, i]]);
                        uvs.AddRange(GetPatchedAtlasUv(elem.atlasUvDict[face], elem.patchUvDict[face]));
                        tris.AddRange(VoxelData.TriIdxOrder.Select(i => i + vertIdx));
                    }
                }
            }
        }
        public (List<Vector3>, List<int>, List<Vector2>) renderElements => (verts, tris, uvs);

        public struct Element
        {
            public struct Rotation
            {
                public Vector3 pivot;
                public Vector3 axis;
                public float angle;
            }

            public Rotation? rotation;

            public Vector3 from;
            public Vector3 to;
            public Dictionary<string, Texture2D> textures;
            public Dictionary<string, Rect> patchUvDict;
            public Dictionary<string, Rect> atlasUvDict;
        }

        public List<Element> elements;
        private List<Vector3> verts;
        private List<int> tris;
        private List<Vector2> uvs;
    }
}
