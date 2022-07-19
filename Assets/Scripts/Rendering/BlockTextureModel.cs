using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Rendering
{
    public struct BlockTextureModel
    {
        public BlockTextureModel(List<Element> elements)
        {
            this.elements = elements;
            this.verts = new List<Vector3>();
            this.tris = new List<int>();
            this.uvs = new List<Vector2>();

            foreach (var elem in elements)
            {
                var vertList = RenderHelper.GenerateVerts(elem.from, elem.to);
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
                        uvs.AddRange(RenderHelper.GetPatchedAtlasUv(elem.atlasUvDict[face], elem.patchUvDict[face]));
                        tris.AddRange(VoxelData.TriIdxOrder.Select(i => i + vertIdx));
                    }
                }
            }
        }
        public (Vector3[], int[], Vector2[]) renderElements => (verts.ToArray(), tris.ToArray(), uvs.ToArray());

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
