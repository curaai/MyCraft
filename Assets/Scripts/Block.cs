using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Block
{
    // TODO: User interaction methods can be added
    public bool IsSolid;
    public BlockType type;

    public virtual Vector2[] GetTexture(VoxelFace face)
    {
        var world = GameObject.Find("World").GetComponent<World>();
        Rect uv;
        var blockTable = world.GetComponent<BlockTable>();
        var texModel = blockTable.TextureModelList.Where(x => x.name == "grass_normal").ToArray()[0];
        uv = blockTable.TextureUvList[texModel.GetFace(face)];
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