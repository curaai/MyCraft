using System;

public record BlockData
{
    public int id;
    public string name;
    public BlockTextureModel textureModel;

    public override string ToString() => $"BlockData [{id}, {name}]";
}