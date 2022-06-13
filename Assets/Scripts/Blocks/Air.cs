using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCraft.Blocks
{
    [Serializable]
    public class Air : Block
    {
        // TODO: User interaction methods can be added
        public Air()
        {
            isSolid = false;
            id = 0;
        }
    }
}
