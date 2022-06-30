using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyCraft.Rendering;
using System.Reflection;
using System.Globalization;

namespace MyCraft
{
    public class BlockTable
    {
        private static readonly string TextureBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "textures");
        private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "models");
        public static readonly List<String> CurSupportModels = new List<string>() { "block", "cube", "cube_all", "cube_column", "grass", "leaves" };

        public Texture2D AtlasTexture;
        public Material material;

        public Dictionary<int, BlockData> DataDict;

        public BlockData this[int id] { get => DataDict[id]; }

        public BlockTable()
        {
            Dictionary<int, BlockTextureModel> _textureModelsLoad(List<TextAsset> assets, Dictionary<string, int> dataIdDict, List<Texture2D> textures, List<Vector2[]> uvs)
            {
                Dictionary<String, Texture2D> texDict = textures.ToDictionary(x => x.name, x => x);
                Func<String, String> getName = x => x.Substring(x.IndexOf('/') + 1);

                BlockTextureModel parseModel(in TextAsset asset)
                {
                    var json = JObject.Parse(asset.text);
                    JObject textureJson = json["textures"].ToObject<JObject>();

                    var res = new BlockTextureModel();
                    switch (getName(json["parent"].ToString()))
                    {
                        case "cube_all":
                        case "leaves":
                            var tex1 = texDict[getName(textureJson["all"].ToString())];
                            res.up = res.down = res.east = res.west = res.south = res.north = (tex1, uvs[textures.IndexOf(tex1)]);
                            break;
                        case "cube_column":
                            var endTex = texDict[getName(textureJson["end"].ToString())];
                            var sideTex1 = texDict[getName(textureJson["side"].ToString())];
                            res.up = res.down = (endTex, uvs[textures.IndexOf(endTex)]);
                            res.east = res.west = res.south = res.north = (sideTex1, uvs[textures.IndexOf(sideTex1)]);
                            break;
                        case "grass":
                        case "block":
                            var topTex = texDict[getName(textureJson["top"].ToString())];
                            var botTex = texDict[getName(textureJson["bottom"].ToString())];
                            var sideTex = texDict[getName(textureJson["side"].ToString())];
                            res.up = (topTex, uvs[textures.IndexOf(topTex)]);
                            res.down = (botTex, uvs[textures.IndexOf(botTex)]);
                            res.east = res.west = res.south = res.north = (sideTex, uvs[textures.IndexOf(sideTex)]);
                            break;
                        case "cube":
                            var tex = texDict[getName(textureJson["up"].ToString())];
                            res.up = (tex, uvs[textures.IndexOf(tex)]);
                            tex = texDict[getName(textureJson["down"].ToString())];
                            res.down = (tex, uvs[textures.IndexOf(tex)]);
                            tex = texDict[getName(textureJson["east"].ToString())];
                            res.east = (tex, uvs[textures.IndexOf(tex)]);
                            tex = texDict[getName(textureJson["west"].ToString())];
                            res.west = (tex, uvs[textures.IndexOf(tex)]);
                            tex = texDict[getName(textureJson["south"].ToString())];
                            res.south = (tex, uvs[textures.IndexOf(tex)]);
                            tex = texDict[getName(textureJson["north"].ToString())];
                            res.north = (tex, uvs[textures.IndexOf(tex)]);
                            break;
                    }
                    return res;
                }

                return (from x in assets
                        where dataIdDict.Keys.Contains(x.name)
                        select (dataIdDict[x.name], parseModel(x)))
                        .ToDictionary(x => x.Item1, x => x.Item2);
            }


            Vector2[] rect2vec(Rect uv)
            {
                return new Vector2[] {
                    new Vector2(uv.xMin, uv.yMin),
                    new Vector2(uv.xMin, uv.yMax),
                    new Vector2(uv.xMax, uv.yMin),
                    new Vector2(uv.xMax, uv.yMax) };
            }

            var textureList = loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>().ToList(); ;
            var textModelList = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>().ToList();

            material = new Material(Shader.Find("Unlit/Transparent"));
            AtlasTexture = new Texture2D(512, 512) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var atlas = AtlasTexture.PackTextures(textureList.ToArray(), 0, 512, true);
            var textureUvs = atlas.Select(rect2vec).ToList();
            material.mainTexture = AtlasTexture;

            // TODO: Temp Code for compatiabilty
            var scriptDict = Resources.LoadAll<BlockScriptData>("Table/Blocks").ToDictionary(x => x.id, x => x);
            var dataIdDict = scriptDict.Values.ToDictionary(x => x.textureModelName, x => x.id);
            var textureModels = _textureModelsLoad(textModelList, dataIdDict, textureList, textureUvs);

            // TODO: Original Code for WIP
            var newTextureModels = textureModelsLoad(textModelList, dataIdDict, textureList, atlas);

            DataDict = new Dictionary<int, BlockData>();
            foreach (var scriptData in Resources.LoadAll<BlockScriptData>("Table/Blocks"))
            {
                var res = new BlockData()
                {
                    id = scriptData.id,
                    name = scriptData.name,
                    materialType = scriptData.materialType,
                    hardness = scriptData.hardness,
                    textureModel = textureModels[scriptData.id],
                    newTextureModel = newTextureModels[scriptData.id],
                    iconSprite = scriptData.iconSprite,
                };
                DataDict[scriptData.id] = res;
            }
        }

        private static Dictionary<int, Rendering.BlockTextureModelNew> textureModelsLoad(List<TextAsset> modelAssets, Dictionary<string, int> dataIdDict, List<Texture2D> textures, Rect[] atlasUvs)
        {
            Dictionary<String, int> texDict = textures.Select((e, i) => new { e, i }).ToDictionary(x => x.e.name, x => x.i);
            var tempModels = new Dictionary<string, BlockTextureModelNew>();

            void loadTempModels()
            {
                string parseParentName(TextAsset asset)
                {
                    var json = JObject.Parse(asset.text);
                    var parent = json["parent"]?.ToString();
                    if (parent != null)
                        parent = getName(parent);
                    return parent;
                }
                BlockTextureModelNew load(TextAsset asset)
                {
                    if (tempModels.ContainsKey(asset.name))
                        return tempModels[asset.name];

                    var parentName = parseParentName(asset);
                    BlockTextureModelNew? parent;
                    if (parentName == null || parentName == "block" || parentName == "thin_block")
                        parent = null;
                    else if (tempModels.ContainsKey(parentName))
                        parent = tempModels[parentName];
                    else
                        parent = tempModels[parentName] = load(modelAssets.Find(x => x.name == parentName));

                    return parseModel(parent, asset);
                }

                while (0 < modelAssets.Count)
                {
                    var asset = modelAssets[0];
                    if (!(asset.name == "block" || asset.name == "thin_block" || tempModels.ContainsKey(asset.name)))
                    {
                        tempModels[asset.name] = load(asset);
                    }
                    modelAssets.Remove(asset);
                }
            }

            Rendering.BlockTextureModelNew? convert2newmodel(BlockTextureModelNew model, Rect[] atlasUvs)
            {
                var elements = new List<Rendering.BlockTextureModelNew.Element>();

                for (int i = 0; i < model.elements.Count; i++)
                {
                    var elem = model.elements[i];
                    var resElem = new Rendering.BlockTextureModelNew.Element();
                    resElem.from = elem.from;
                    resElem.to = elem.to;
                    resElem.textures = new Dictionary<string, Texture2D>();
                    resElem.patchUvDict = new Dictionary<string, Rect>();
                    resElem.atlasUvDict = new Dictionary<string, Rect>();

                    if (elem.faces.Values.Select(x => x.Item1).Contains("#overlay"))
                        continue;

                    foreach (var prop in elem.faces)
                    {
                        var textureTag = removeHashtag(prop.Value.Item1);

                        while (model.textureTagMap.ContainsKey(textureTag) && model.textureTagMap[textureTag].Contains('#'))
                            textureTag = removeHashtag(model.textureTagMap[textureTag]);

                        if (!model.textureTagMap.ContainsKey(textureTag))
                            return null;

                        var idx = texDict[model.textureTagMap[textureTag]];
                        var texture = textures[idx];
                        resElem.textures[prop.Key] = texture;
                        resElem.patchUvDict[prop.Key] = prop.Value.Item2;
                        resElem.atlasUvDict[prop.Key] = atlasUvs[idx];
                    }
                    elements.Add(resElem);
                }
                return new Rendering.BlockTextureModelNew(elements);
            }

            loadTempModels();
            var models = new Dictionary<int, Rendering.BlockTextureModelNew>();
            foreach (var pair in tempModels)
            {
                if (pair.Value.textureTagMap.Count == 0)
                    continue;

                var model = convert2newmodel(pair.Value, atlasUvs);
                if (model != null && dataIdDict.ContainsKey(pair.Key))
                    models[dataIdDict[pair.Key]] = model.Value;
            }
            return models;
        }
        private static BlockTextureModelNew parseModel(BlockTextureModelNew? parent, in TextAsset asset)
        {
            Func<JToken, string, float[]> toFloatArr = (parent, key) => JArray.Parse(parent[key].ToString()).ToObject<float[]>();
            Func<float[], float[]> divide16 = (arr) => arr.Select(x => x / 16f).ToArray();
            Func<float[], Vector3> toVec = (a) => new Vector3(a[0], a[1], a[2]);
            Rendering.BlockTextureModelNew.Element.Rotation parseRotation(JToken obj)
            {
                var pivot = toVec(divide16(toFloatArr(obj, "origin")));
                Vector3 axis;
                switch (obj["axis"].ToString())
                {
                    case "x":
                        axis = Vector3.right;
                        break;
                    case "y":
                        axis = Vector3.up;
                        break;
                    case "z":
                        axis = Vector3.forward;
                        break;
                    default:
                        throw new InvalidCastException("Cannot find axis");
                }
                var angle = obj["angle"].ToObject<float>();
                return new Rendering.BlockTextureModelNew.Element.Rotation()
                {
                    pivot = pivot,
                    axis = axis,
                    angle = angle
                };
            }

            BlockTextureModelNew res = new BlockTextureModelNew();

            var json = JObject.Parse(asset.text);
            // Parent Model
            if (parent == null)
            {
                /*
                example ./cross.json
                {
                    "ambientocclusion": false,
                    "textures": {
                        "particle": "#cross"
                    },
                    "elements": [
                        {   "from": [ 0.8, 0, 8 ],
                            "to": [ 15.2, 16, 8 ],
                            "rotation": { "origin": [ 8, 8, 8 ], "axis": "y", "angle": 45, "rescale": true },
                            "shade": false,
                            "faces": {
                                "north": { "uv": [ 0, 0, 16, 16 ], "texture": "#cross" },
                                "south": { "uv": [ 0, 0, 16, 16 ], "texture": "#cross" }
                            }
                        },
                        {   "from": [ 8, 0, 0.8 ],
                            "to": [ 8, 16, 15.2 ],
                            "rotation": { "origin": [ 8, 8, 8 ], "axis": "y", "angle": 45, "rescale": true },
                            "shade": false,
                            "faces": {
                                "west": { "uv": [ 0, 0, 16, 16 ], "texture": "#cross" },
                                "east": { "uv": [ 0, 0, 16, 16 ], "texture": "#cross" }
                            }
                        }
                    ]
                }
                */

                foreach (var elemJson in JArray.Parse(json["elements"].ToString()))
                {
                    BlockTextureModelNew.Element elem = new BlockTextureModelNew.Element();
                    elem.from = toVec(divide16(toFloatArr(elemJson, "from")));
                    elem.to = toVec(divide16(toFloatArr(elemJson, "to")));

                    if (elemJson["rotation"] != null)
                        elem.rotation = parseRotation(elemJson["rotation"]);
                    // (elem.from, elem.to) = rotate(elemJson["rotation"], elem.from, elem.to);

                    elem.faces = new Dictionary<string, (string, Rect)>();
                    foreach (var face in elemJson["faces"].ToObject<JObject>())
                    {
                        Rect uvRect;
                        if (face.Value["uv"] == null)
                        {
                            uvRect = Rect.MinMaxRect(0, 0, 1, 1);
                        }
                        else
                        {
                            var uv = divide16(toFloatArr(face.Value, "uv"));
                            uvRect = Rect.MinMaxRect(uv[0], uv[1], uv[2], uv[3]);
                        }
                        elem.faces[face.Key] = (face.Value["texture"].ToString(), uvRect);
                    }
                    res.elements.Add(elem);
                }
            }
            else
            {
                // becareful copy
                res.elements = new List<BlockTextureModelNew.Element>(parent.elements);
                res.textureTagMap = new Dictionary<string, string>(parent?.textureTagMap);
            }

            if (json["textures"] != null)
                foreach (var prop in json["textures"].ToObject<JObject>())
                {
                    var temp = prop.Value.ToString();
                    if (!temp.Contains('#'))
                        temp = getName(temp);
                    res.textureTagMap[prop.Key] = temp;
                }
            return res;
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

        private static String getName(string x) => x.Substring(x.IndexOf('/') + 1);
        private static string removeHashtag(string x) => x.Substring(1);

        private class BlockTextureModelNew
        {
            public class Element
            {
                public Rendering.BlockTextureModelNew.Element.Rotation? rotation;
                public Vector3 from;
                public Vector3 to;
                public Dictionary<string, (string, Rect)> faces = new Dictionary<string, (string, Rect)>();
            }

            public List<Element> elements = new List<Element>();
            public Dictionary<string, string> textureTagMap = new Dictionary<string, string>();
        }
    }
}
