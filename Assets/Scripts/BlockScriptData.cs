using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MyCraft
{
    [CreateAssetMenu(fileName = "BlockScriptData", menuName = "MyCraft/Block Attribute")]
    public class BlockScriptData : ScriptableObject
    {
        public byte id;
        public string textureModelName;
        public MaterialType materialType;
        public float hardness;
        public bool isTransparent;
        public bool isSolid;
        public Sprite iconSprite;
    }
}
