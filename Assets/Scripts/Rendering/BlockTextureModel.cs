using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Rendering
{
    public struct BlockTextureModel
    {
        public int up;
        public int down;
        public int east;
        public int west;
        public int south;
        public int north;

        public int GetFace(VoxelFace face)
        {
            switch (face)
            {
                case VoxelFace.UP:
                    return up;
                case VoxelFace.DOWN:
                    return down;
                case VoxelFace.EAST:
                    return east;
                case VoxelFace.WEST:
                    return west;
                case VoxelFace.SOUTH:
                    return south;
                case VoxelFace.NORTH:
                    return north;
            }
            return 0;
        }
    }
    public struct BlockTextureModelNew
    {
        public struct Element
        {
            public Vector3 from;
            public Vector3 to;
            public Dictionary<string, (Texture2D, Rect)> faces;
        }

        public List<Element> elements;
    }
}
