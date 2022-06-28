using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCraft.Rendering;

namespace MyCraft
{
    public struct BlockData
    {
        public int id;
        public string name;
        public MaterialType materialType;
        public float hardness;

        public BlockTextureModel textureModel;

        public Sprite iconSprite;

        public override string ToString() => $"BlockData [{id}, {name}]";
        public Texture2D GetTexture(VoxelFace face) => textureModel.GetFace(face)?.Item1;
        public Vector2[] GetUv(VoxelFace face) => textureModel.GetFace(face)?.Item2;
    }
    public enum MaterialType
    {
        ORE,
        DIRT,
        WOOD,
    }
}
