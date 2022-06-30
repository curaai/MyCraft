using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft
{
    [Serializable]
    public class Block
    {
        // TODO: User interaction methods can be added
        public Block() { }

        public Block(int _id, bool _isSolid = true)
        {
            id = _id;
            isSolid = _isSolid;
        }

        public int id;
        public bool isSolid;
    }
}
