using System;

public struct BlockData
{
    public uint id;
    public string name;
    public string model;
    public uint rewardId;

    public override string ToString() => $"BlockData [{id}, {name}]";
}