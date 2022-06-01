using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public record Block
{
    // TODO: User interaction methods can be added
    public Block() { }

    public Block(int _id)
    {
        id = _id;
        isSolid = true;
    }
    public Block(int _id, bool _isSolid)
    {
        id = _id;
        isSolid = _isSolid;
    }

    public int id;
    public bool isSolid;
}
