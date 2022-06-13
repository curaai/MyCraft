using System.IO;
using UnityEditor;
using UnityEngine;

namespace MyCraft
{
    [CreateAssetMenu(fileName = "BlockScriptData", menuName = "MyCraft/Block Attribute")]
    public class BlockScriptData : ScriptableObject
    {
        public int id;
        public string textureModelName;
        public MaterialType materialType;
        public float hardness;

        public override string ToString() => $"BlockData [{id}, {name}]";
    }
}
