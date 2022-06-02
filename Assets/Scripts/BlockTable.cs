using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BlockTable
{
    public static readonly List<String> CurSupportModels = new List<string>() { "block", "cube", "cube_all", "grass" };

    public List<Texture2D> Textures;
    public Texture2D AtlasTexture;
    public List<Vector2[]> TextureUvList;
    public Dictionary<int, BlockData> dataTable;
    public Material material;

    public BlockTable()
    {
        List<Texture2D> textureLoad(AssetBundle bundle)
        {
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }

            Dictionary<String, Texture2D> res = new();
            return bundle.LoadAllAssets<Texture2D>().ToList();
        }

        List<BlockData> tableLoad(AssetBundle tableBundle, AssetBundle modelBundle, in List<Texture2D> textures)
        {
            var rawTable = tableBundle.LoadAsset<TextAsset>("table");
            var models = modelBundle.LoadAllAssets<TextAsset>();

            Dictionary<String, int> texDict = textures.Select((e, i) => new { e, i }).ToDictionary(x => x.e.name, x => x.i);
            Func<String, String> getName = x => x.Substring(x.IndexOf('/') + 1);

            BlockData parse(JObject json)
            {
                BlockTextureModel? parseModel(in TextAsset asset)
                {
                    var json = JObject.Parse(asset.text);
                    if (json["textures"] == null ||
                        json["parent"] == null ||
                        CurSupportModels.IndexOf(getName(json["parent"].ToString())) == -1)
                        return null;

                    var res = new BlockTextureModel();
                    JObject textureJson = json["textures"].ToObject<JObject>();
                    try
                    {
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
                    }
                    catch (Exception e) // catch when don't modelize 
                    {
                        return null;
                    }
                    return res;
                }

                BlockData res = new();
                res.id = json["id"].ToObject<int>();
                res.name = json["name"].ToString();
                res.materialType = Enum.Parse<MaterialType>(json["material_type"].ToString(), true);
                res.hardness = json["hardness"].ToObject<float>();

                if (json["model"] != null)
                {
                    var modelName = json["model"].ToString();
                    var model = parseModel(models.Where(x => x.name == modelName).ToList()[0]);
                    if (model.HasValue)
                        res.textureModel = model.Value;
                }
                return res;
            }

            if (tableBundle == null || modelBundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }

            var air = new BlockData() { id = 0, name = "Air", hardness = -1 };
            var res = JArray.Parse(rawTable.text).Select(x => parse(x.ToObject<JObject>())).ToList();
            res.Insert(0, air);
            return res;
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


        Textures = textureLoad(AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "textures")));
        var _datas = tableLoad(
            AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "table")),
            AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "models")),
            Textures);
        dataTable = _datas.ToDictionary(x => x.id, x => x);

        AtlasTexture = new Texture2D(512, 512);
        material = new Material(Shader.Find("Unlit/Texture"));
        TextureUvList = AtlasTexture.PackTextures(Textures.ToArray(), 0, 512, true).Select(x => rect2vec(x)).ToList();
        material.mainTexture = AtlasTexture;
    }

    public Vector2[] GetTexture(int id, VoxelFace face) => TextureUvList[dataTable[id].textureModel.GetFace(face)];
}