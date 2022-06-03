using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "MyCraft/Block Attribute")]
public class BlockData : ScriptableObject
{
    public int id;
    public string textureModelName;
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