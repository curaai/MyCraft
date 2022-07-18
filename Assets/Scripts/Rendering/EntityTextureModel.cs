using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Rendering
{
    public struct EntityTextureModel
    {
        public EntityTextureModel(MyCraft.Environment.EntityTable.JsonTextureModel tempModel, Texture2D tex)
        {
            this.verts = new List<Vector3>();
            this.tris = new List<int>();
            this.uvs = new List<Vector2>();
            var texSize = new Vector2(tex.width, tex.height);

            foreach (var bone in tempModel.bones)
            {
                if (bone.neverRender)
                    continue;

                foreach (var cube in bone.cubes)
                {
                    var cubeSize = RenderHelper.FindCubeSizeFromTex(tex, cube.uv);
                    var _verts = RenderHelper.GenerateVerts(cube.origin, cube.origin + cube.size);
                    var _uvs = RenderHelper.GenerateCubeUvs(texSize, cube.uv, cubeSize, bone.mirror);

                    foreach (var enumFace in Enum.GetValues(typeof(VoxelFace)))
                    {
                        var face = enumFace.ToString().ToLower();
                        var vertIdx = verts.Count;
                        for (int i = 0; i < 4; i++)
                            verts.Add(_verts[VoxelData.Tris[(int)enumFace, i]]);
                        uvs.AddRange(_uvs[(int)enumFace]);
                        tris.AddRange(VoxelData.TriIdxOrder.Select(i => i + vertIdx));
                    }
                }
            }
        }

        private List<Vector3> verts;
        private List<int> tris;
        private List<Vector2> uvs;

        public (List<Vector3>, List<int>, List<Vector2>) renderElements => (verts, tris, uvs);
    }
}