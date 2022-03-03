using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureUvPosHelper
{
    private int textureAtlasSize;
    private float normTextureAtlasSize;
    private List<Vector2[]> uvsList;

    public TextureUvPosHelper(int size)
    {
        textureAtlasSize = size;
        normTextureAtlasSize = 1f / textureAtlasSize;

        uvsList = new List<Vector2[]>();

        for (float tile = 0; tile < textureAtlasSize * textureAtlasSize; tile++)
        {
            float x = tile % textureAtlasSize;
            float y = (int)tile / textureAtlasSize;

            x *= normTextureAtlasSize;
            y *= normTextureAtlasSize;
            y = 1 - y - normTextureAtlasSize;

            var uvs = new Vector2[]
            {
            new Vector2(x, y),
            new Vector2(x, y + normTextureAtlasSize),
            new Vector2(x + normTextureAtlasSize, y),
            new Vector2(x + normTextureAtlasSize, y + normTextureAtlasSize)
            };
            uvsList.Add(uvs);
        }
    }

    public Vector2[] GetUVs(Tile t) => uvsList[(int)t];
};

public enum Tile
{
    Stone,
    Dirt,
    GrassSide,
    Glass,
    Plank,
    WoodSide,
    Wood,
    Grass,
    CobbleStone,
    Bedrock,
    Sand,
    Brick,
    FurnaceFront,
    FurnaceSide,
    FurnaceActive,
    FurnaceUp
};