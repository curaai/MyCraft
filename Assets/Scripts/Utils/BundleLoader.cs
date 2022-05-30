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
    List<String> CurSupportModels = new List<string>() { "block", "cube", "cube_all", "grass" };

    public struct TextureModel
    {
        public string name;
        public int up;
        public int down;
        public int east;
        public int west;
        public int south;
        public int north;
    };

    private void Start()
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

        List<TextureModel> modelLoad(Dictionary<String, int> texDict)
        {
            Func<String, String> getName = x => x.Substring(x.IndexOf('/'));

            TextureModel? parse(in TextAsset asset)
            {
                var json = JObject.Parse(asset.text);
                if (CurSupportModels.IndexOf(json["parent"].ToString()) == -1 ||
                    json["textures"] == null)
                    return null;

                var res = new TextureModel();
                JObject textureJson = json["textures"].ToObject<JObject>();
                switch (getName(json["parent"].ToString()))
                {
                    case "cube_all":
                        res.up = res.down = res.east = res.west = res.south = res.north = texDict[textureJson["all"].ToString()];
                        break;
                    case "grass":
                    case "block":
                        res.up = texDict[textureJson["up"].ToString()];
                        res.down = texDict[textureJson["down"].ToString()];
                        res.east = res.west = res.south = res.north = texDict[textureJson["side"].ToString()];
                        break;
                    case "cube":
                        res.up = texDict[textureJson["up"].ToString()];
                        res.down = texDict[textureJson["down"].ToString()];
                        res.east = texDict[textureJson["east"].ToString()];
                        res.west = texDict[textureJson["west"].ToString()];
                        res.south = texDict[textureJson["south"].ToString()];
                        res.north = texDict[textureJson["north"].ToString()];
                        break;
                }
                return res;
            }

            var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "models"));
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                throw new ArgumentNullException("Can't find asset bundle");
            }
            // var models = modelBundle.LoadAllAssets<TextAsset>();
            var asstes = bundle.LoadAllAssets<TextAsset>();
            return asstes.Select(x => parse(x)).Where(x => x.HasValue).Cast<TextureModel>().ToList();
        }
        var textures = textureLoad();
        var models = modelLoad(textures.Select((e, i) => new { e, i }).ToDictionary(x => x.e.name, x => x.i));
    }
}