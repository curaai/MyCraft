using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using MyCraft.Utils;

namespace MyCraft.Editor
{
    public class PackageRebuilder
    {
        private static readonly string TextureBundlePath = Path.Combine(Application.dataPath, "AssetBundles", "textures", "blocks");
        private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath, "AssetBundles", "models", "blocks");
        private static readonly string saveRoot = "MinecraftPackage/Editor";


        [MenuItem("Assets/Build Material")]
        static void BuildTextures()
        {
            void BuildBlockAtlas()
            {
                var root = Path.Combine(Application.dataPath, "MinecraftPackage", "textures", "blocks");
                var textures = loadTextures(root);

                var atlasTexture = new Texture2D(512, 512) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var atlasRects = atlasTexture.PackTextures(textures, 0, 512).Select(r => new float[4] { r.x, r.y, r.width, r.height }).ToList();

                var texPath = Path.Combine(Application.dataPath, saveRoot, "BlockAtlasTexture.png");
                var bytes = atlasTexture.EncodeToPNG();
                File.WriteAllBytes(texPath, bytes);

                var material = new Material(Shader.Find("Unlit/Texture"));
                material.mainTexture = atlasTexture;
                AssetDatabase.CreateAsset(material, "Assets/" + saveRoot + "/Block.mat");

                var _material = new Material(Shader.Find("Unlit/Transparent"));
                _material.mainTexture = atlasTexture;
                AssetDatabase.CreateAsset(_material, "Assets/" + saveRoot + "/BlockTransparent.mat");

                var bf = new BinaryFormatter();
                var file = File.Create(Path.Combine(Application.dataPath, saveRoot, "BlockAtlasRects.dat"));
                bf.Serialize(file, atlasRects);
                file.Close();
            }

            void BuildEntityAtlas()
            {
                var blockRoot = Path.Combine(Application.dataPath, "MinecraftPackage", "textures", "entity");
                var blockTextures = loadTextures(blockRoot);

                var atlasTexture = new Texture2D(4096, 4096) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var atlasRects = atlasTexture.PackTextures(blockTextures, 0, 512).Select(r => new float[4] { r.x, r.y, r.width, r.height }).ToList();

                var blockTexPath = Path.Combine(Application.dataPath, saveRoot, "EntityAtlasTexture.png");
                var bytes = atlasTexture.EncodeToPNG();
                File.WriteAllBytes(blockTexPath, bytes);

                var material = new Material(Shader.Find("Unlit/Texture"));
                material.mainTexture = atlasTexture;
                AssetDatabase.CreateAsset(material, "Assets/" + saveRoot + "/Entity.mat");

                var _material = new Material(Shader.Find("Unlit/Transparent"));
                _material.mainTexture = atlasTexture;
                AssetDatabase.CreateAsset(_material, "Assets/" + saveRoot + "/EntityTransparent.mat");

                var bf = new BinaryFormatter();
                var file = File.Create(Path.Combine(Application.dataPath, saveRoot, "EntityAtlasRects.dat"));
                bf.Serialize(file, atlasRects);
                file.Close();

            }

            BuildBlockAtlas();
            BuildEntityAtlas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Texture2D[] loadTextures(string rootDirPath)
        {
            return Directory.GetFiles(rootDirPath, "*.png", SearchOption.AllDirectories)
                .Select(x => "Assets" + x.Replace(Application.dataPath, "").Replace('\\', '/'))
                .Select(x => AssetDatabase.LoadAssetAtPath<Texture2D>(x)).ToArray();
        }
    }
}
