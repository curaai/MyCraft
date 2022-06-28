using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Rendering
{
    public struct BlockTextureModel
    {
        public (Texture2D, Vector2[]) up;
        public (Texture2D, Vector2[]) down;
        public (Texture2D, Vector2[]) east;
        public (Texture2D, Vector2[]) west;
        public (Texture2D, Vector2[]) south;
        public (Texture2D, Vector2[]) north;

        public (Texture2D, Vector2[])? GetFace(VoxelFace face)
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
            return null;
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
