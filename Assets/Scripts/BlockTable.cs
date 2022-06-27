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
            var textureModels = textureModelsLoad(textModelList, textureList);

            material = new Material(Shader.Find("Unlit/Texture"));
            AtlasTexture = new Texture2D(512, 512) { filterMode = FilterMode.Point };
            var textureUvs = AtlasTexture.PackTextures(textureList.ToArray(), 0, 512, true).Select(rect2vec).ToList();
            material.mainTexture = AtlasTexture;

            // DataDict = new Dictionary<int, BlockData>();
            // foreach (var scriptData in Resources.LoadAll<BlockScriptData>("Table/Blocks"))
            // {
            //     var textures = new List<Texture2D>();
            //     for (int i = 0; i < VoxelData.FACE_COUNT; i++)
            //         textures.Add(textureList[textureModels[scriptData.id].GetFace((VoxelFace)i)]);
            //     var uvs = new List<Vector2[]>();
            //     for (int i = 0; i < VoxelData.FACE_COUNT; i++)
            //         uvs.Add(textureUvs[textureModels[scriptData.id].GetFace((VoxelFace)i)]);

            //     var res = new BlockData()
            //     {
            //         id = scriptData.id,
            //         name = scriptData.name,
            //         materialType = scriptData.materialType,
            //         hardness = scriptData.hardness,
            //         textures = textures,
            //         uvs = uvs,
            //         iconSprite = scriptData.iconSprite,
            //     };
            //     DataDict[scriptData.id] = res;
            // }
        }

        private static Dictionary<string, Rendering.BlockTextureModelNew> textureModelsLoad(List<TextAsset> modelAssets, List<Texture2D> textures)
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

            Rendering.BlockTextureModelNew? convert2newmodel(BlockTextureModelNew model)
            {
                Rendering.BlockTextureModelNew res = new Rendering.BlockTextureModelNew();
                res.elements = new List<Rendering.BlockTextureModelNew.Element>();

                for (int i = 0; i < model.elements.Count; i++)
                {
                    var elem = model.elements[i];
                    var resElem = new Rendering.BlockTextureModelNew.Element();
                    resElem.faces = new Dictionary<string, (Texture2D, Rect)>();
                    resElem.from = elem.from;
                    resElem.to = elem.to;

                    foreach (var prop in elem.faces)
                    {
                        var textureTag = removeHashtag(prop.Value.Item1);

                        while (model.textureTagMap.ContainsKey(textureTag) && model.textureTagMap[textureTag].Contains('#'))
                            textureTag = removeHashtag(model.textureTagMap[textureTag]);

                        if (!model.textureTagMap.ContainsKey(textureTag))
                            return null;

                        var texture = textures[texDict[model.textureTagMap[textureTag]]];
                        resElem.faces[prop.Key] = (texture, prop.Value.Item2);
                    }
                    res.elements.Add(resElem);
                }
                return res;
            }

            loadTempModels();
            var models = new Dictionary<string, Rendering.BlockTextureModelNew>();
            foreach (var pair in tempModels)
            {
                if (pair.Value.textureTagMap.Count == 0)
                    continue;

                var model = convert2newmodel(pair.Value);
                if (model != null)
                    models[pair.Key] = model.Value;
            }
            return models;
        }
        private static BlockTextureModelNew parseModel(BlockTextureModelNew? parent, in TextAsset asset)
        {
            Func<JToken, string, float[]> toFloatArr = (parent, key) => JArray.Parse(parent[key].ToString()).ToObject<float[]>();
            Func<float[], float[]> divide16 = (arr) => arr.Select(x => x / 16f).ToArray();
            Func<float[], Vector3> toVec = (a) => new Vector3(a[0], a[1], a[2]);

            (Vector3, Vector3) rotate(JToken obj, Vector3 from, Vector3 to)
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
                return (Quaternion.AngleAxis(angle, axis) * (from - pivot) + pivot,
                        Quaternion.AngleAxis(angle, axis) * (to - pivot) + pivot);
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
                        (elem.from, elem.to) = rotate(elemJson["rotation"], elem.from, elem.to);

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
                public Vector3 from;
                public Vector3 to;
                public Dictionary<string, (string, Rect)> faces = new Dictionary<string, (string, Rect)>();
            }

            public List<Element> elements = new List<Element>();
            public Dictionary<string, string> textureTagMap = new Dictionary<string, string>();
        }
    }
}
