using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Block
{
    // TODO: User interaction methods can be added
    public bool IsSolid;
    public BlockType type;
    protected static TextureUvPosHelper uvPosHelper = new TextureUvPosHelper(4);

    private static List<Tile[]> blockTextureList = new List<Tile[]>(){
        new Tile[] {Tile.Stone},
        new Tile[] {Tile.GrassSide, Tile.GrassSide, Tile.Grass, Tile.Dirt, Tile.GrassSide, Tile.GrassSide},
        new Tile[] {Tile.Glass},
        new Tile[] {Tile.Plank},
        new Tile[] {Tile.WoodSide, Tile.WoodSide, Tile.Wood, Tile.Wood, Tile.WoodSide, Tile.WoodSide},
        new Tile[] {Tile.CobbleStone},
        new Tile[] {Tile.Bedrock},
        new Tile[] {Tile.Sand},
        new Tile[] {Tile.Brick},
        new Tile[] {Tile.FurnaceSide, Tile.FurnaceFront, Tile.FurnaceUp, Tile.FurnaceUp, Tile.FurnaceSide, Tile.FurnaceSide},
        new Tile[] {Tile.Dirt},
    };

    public virtual Vector2[] GetTexture(VoxelFace face)
    {
        var world = GameObject.Find("World").GetComponent<World>();
        Rect uv;
        switch (face)
        {
            case VoxelFace.TOP:
                uv = world.TextureUvList[0];
                break;
            case VoxelFace.BOTTOM:
                uv = world.TextureUvList[2];
                break;
            default:
                uv = world.TextureUvList[1];
                break;
        }
        Vector2[] rect2vec(Rect uv)
        {
            var uvs = new Vector2[]
            {
            new Vector2(uv.xMin, uv.yMin),
            new Vector2(uv.xMin, uv.yMax),
            new Vector2(uv.xMax, uv.yMin),
            new Vector2(uv.xMax, uv.yMax)
            };
            return uvs;
        }
        return rect2vec(uv);

        var textures = blockTextureList[(int)type];
        var faceIdx = textures.Length == 1 ? 0 : (int)face;
        return uvPosHelper.GetUVs(textures[faceIdx]);
    }
}

public enum BlockType
{
    Stone,
    Grass,
    Glass,
    Plank,
    Wood,
    CobbleStone,
    Bedrock,
    Sand,
    Brick,
    Furnace,
    Dirt,
    Air,
};