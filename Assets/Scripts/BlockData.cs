using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MyCraft.Rendering;

namespace MyCraft
{
    public struct BlockData
    {
        public byte id;
        public string name;
        public MaterialType materialType;
        public float hardness;
        public bool isTransparent;
        public bool isSolid;
        public BlockTextureModel textureModel;
        public Sprite iconSprite;

        public override string ToString() => $"BlockData [{id}, {name}]";
    }

    public enum MaterialType
    {
        ORE,
        DIRT,
        WOOD,
    }
}
