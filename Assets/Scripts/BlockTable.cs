using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BlockTable
{
    private static readonly string TextureBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "textures");
    private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "models");
    public static readonly List<String> CurSupportModels = new List<string>() { "block", "cube", "cube_all", "grass" };

    public List<Texture2D> Textures;
    public Texture2D AtlasTexture;
    public List<Vector2[]> TextureUvList;
    public Material material;

    public Dictionary<int, BlockData> DataDict;
    public Dictionary<int, BlockTextureModel> TextureModelDict;

    public BlockTable()
    {
        AssetBundle loadBundle(string path)
        {
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }
            return bundle;
        }

        List<Texture2D> textureLoad()
        {
            Dictionary<String, Texture2D> res = new();
            return loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>().ToList();
        }

        Dictionary<int, BlockTextureModel> textureModelsLoad(in List<Texture2D> textures)
        {
            Dictionary<String, int> texDict = textures.Select((e, i) => new { e, i }).ToDictionary(x => x.e.name, x => x.i);
            Func<String, String> getName = x => x.Substring(x.IndexOf('/') + 1);

            BlockTextureModel parseModel(in TextAsset asset)
            {
                var json = JObject.Parse(asset.text);
                JObject textureJson = json["textures"].ToObject<JObject>();

                var res = new BlockTextureModel();
                switch (getName(json["parent"].ToString()))
                {
                    case "cube_all":
                        res.up = res.down = res.east = res.west = res.south = res.north = texDict[getName(textureJson["all"].ToString())];
                        break;
                    case "grass":
                    case "block":
                        res.up = texDict[getName(textureJson["top"].ToString())];
                        res.down = texDict[getName(textureJson["bottom"].ToString())];
                        res.east = res.west = res.south = res.north = texDict[getName(textureJson["side"].ToString())];
                        break;
                    case "cube":
                        res.up = texDict[getName(textureJson["up"].ToString())];
                        res.down = texDict[getName(textureJson["down"].ToString())];
                        res.east = texDict[getName(textureJson["east"].ToString())];
                        res.west = texDict[getName(textureJson["west"].ToString())];
                        res.south = texDict[getName(textureJson["south"].ToString())];
                        res.north = texDict[getName(textureJson["north"].ToString())];
                        break;
                }
                return res;
            }

            var assets = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>();
            var supportNames = DataDict.Values.ToDictionary(x => x.textureModelName, x => x.id);
            return (from x in assets
                    where supportNames.Keys.Contains(x.name)
                    select (supportNames[x.name], parseModel(x)))
                    .ToDictionary(x => x.Item1, x => x.Item2);
        }

        Vector2[] rect2vec(Rect uv)
        {
            var uvs = new Vector2[]
            {
            new Vector2(uv.xMin, uv.yMin),
            new Vector2(uv.xMin, uv.yMax),
            new Vector2(uv.xMax, uv.yMin),
            new Vector2(uv.xMax, uv.yMax)
            };
            return uvs;
        }

        DataDict = Resources.LoadAll<BlockData>("Table/Blocks").ToDictionary(x => x.id, x => x);
        Textures = textureLoad();
        TextureModelDict = textureModelsLoad(Textures);

        material = new Material(Shader.Find("Unlit/Texture"));
        AtlasTexture = new Texture2D(512, 512) { filterMode = FilterMode.Point };
        TextureUvList = AtlasTexture.PackTextures(Textures.ToArray(), 0, 512, true).Select(rect2vec).ToList();
        material.mainTexture = AtlasTexture;
    }

    public int GetTextureFaceIdx(int id, VoxelFace face) => TextureModelDict[id].GetFace(face);
    public Texture2D GetTexture(int id, VoxelFace face) => Textures[GetTextureFaceIdx(id, face)];
    public Vector2[] GetTextureUv(int id, VoxelFace face) => TextureUvList[GetTextureFaceIdx(id, face)];
}