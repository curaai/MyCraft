using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class BundleLoader : MonoBehaviour
{
    public static readonly List<String> CurSupportModels = new List<string>() { "block", "cube", "cube_all", "grass" };

    public static (List<Texture2D>, List<BlockTextureModel>) LoadBundle()
    {
        List<Texture2D> textureLoad()
        {
            Dictionary<String, Texture2D> res = new();
            var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "textures"));
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }
            return bundle.LoadAllAssets<Texture2D>().ToList();
        }

        List<BlockTextureModel> modelLoad(Dictionary<String, int> texDict)
        {
            Func<String, String> getName = x => x.Substring(x.IndexOf('/') + 1);

            BlockTextureModel? parse(in TextAsset asset)
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
                res.name = asset.name;
                return res;
            }

            var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "models"));
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }
            var asstes = bundle.LoadAllAssets<TextAsset>();
            return asstes.Select(x => parse(x)).Where(x => x.HasValue).Cast<BlockTextureModel>().ToList();
        }
        var textures = textureLoad();
        var models = modelLoad(textures.Select((e, i) => new { e, i }).ToDictionary(x => x.e.name, x => x.i));
        return (textures, models);
    }
}