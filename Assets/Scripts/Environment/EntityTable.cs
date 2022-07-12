using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyCraft.Rendering;

namespace MyCraft.Environment
{
    public class EntityTable
    {
        private static readonly string TextureBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "textures", "entity");
        private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "models", "entity");

        public Material material;

        public EntityTable()
        {
            var modelAssets = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>();
            foreach (var model in modelAssets)
            {
                Debug.Log($"model name: {model.name}");
            }

            var textureAssets = loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>();
            foreach (var texture in textureAssets)
            {
                Debug.Log($"texture name: {texture.name}");
            }

        }

        private static AssetBundle loadBundle(string path)
        {
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }
            return bundle;
        }
    }
}