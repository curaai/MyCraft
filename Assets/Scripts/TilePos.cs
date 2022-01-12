using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TilePos
{
    static readonly int TextureAtlasSize = 4;
    static readonly float NormTextureAtlasSize = 1f / 4f;
    static List<Vector2[]> uvsList;

    static TilePos()
    {
        uvsList = new List<Vector2[]>();

        for (float tile = 0; tile < 16; tile++)
        {
            float x = tile % TextureAtlasSize;
            float y = (int)tile / TextureAtlasSize;

            x *= NormTextureAtlasSize;
            y *= NormTextureAtlasSize;
            y = 1 - y - NormTextureAtlasSize;

            Debug.Log("x: " + x + " y:" + y);

            var uvs = new Vector2[]
            {
            new Vector2(x, y),
            new Vector2(x, y + NormTextureAtlasSize),
            new Vector2(x + NormTextureAtlasSize, y),
            new Vector2(x + NormTextureAtlasSize, y + NormTextureAtlasSize)
            };
            uvsList.Add(uvs);
        }
    }

    public static Vector2[] GetUVs(Tile t)
    {
        return uvsList[(int)t];
    }
};

public enum Tile { Stone, Dirt, GrassSide, Glass, Plank, WoodSide, Wood, Grass, CobbleStone, Bedrock, Send, Brick, FurnaceFront, FurnaceSide, FurnaceActive, FurnaceUp };