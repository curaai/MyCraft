using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Air : Block
{
    // TODO: User interaction methods can be added
    public Air()
    {
        IsSolid = false;
    }

    public override Vector2[] GetTexture(VoxelFace face)
    {
        throw new InvalidOperationException("Air Cannot generate texture");
    }
}