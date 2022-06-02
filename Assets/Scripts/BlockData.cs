using System;

public record BlockData
{
    public int id;
    public string name;
    public BlockTextureModel textureModel;
    public MaterialType materialType;
    public float hardness;

    public override string ToString() => $"BlockData [{id}, {name}]";
}

public enum MaterialType
{
    ORE,
    DIRT,
    WOOD,
}