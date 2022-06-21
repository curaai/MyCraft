using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft
{
    public struct BlockData
    {
        public int id;
        public string name;
        public MaterialType materialType;
        public float hardness;

        public List<Texture2D> textures;
        public List<Vector2[]> uvs;
        public Sprite iconSprite;

        public override string ToString() => $"BlockData [{id}, {name}]";
        public Texture2D GetTexture(VoxelFace face) => textures[(int)face];
        public Vector2[] GetUv(VoxelFace face) => uvs[(int)face];
    }
    public enum MaterialType
    {
        ORE,
        DIRT,
        WOOD,
    }
}
