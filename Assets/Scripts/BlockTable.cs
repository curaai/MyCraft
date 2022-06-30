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
            var textureAssets = loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>();
            var textModelList = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>().ToList();

            material = new Material(Shader.Find("Unlit/Texture"));
            AtlasTexture = new Texture2D(512, 512) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var atlas = AtlasTexture.PackTextures(textureAssets, 0, 512, true);
            material.mainTexture = AtlasTexture;

            var scriptDatas = Resources.LoadAll<BlockScriptData>("Table/Blocks");
            var textureModels = textureModelsLoad(textModelList, scriptDatas, textureAssets.Zip(atlas, (x, y) => (x, y)).ToList());

            DataDict = new Dictionary<int, BlockData>();
            foreach (var scriptData in scriptDatas)
            {
                DataDict[scriptData.id] = new BlockData()
                {
                    id = scriptData.id,
                    name = scriptData.name,
                    materialType = scriptData.materialType,
                    hardness = scriptData.hardness,
                    textureModel = textureModels[scriptData.id],
                    iconSprite = scriptData.iconSprite,
                };

            }
        }

        private static Dictionary<int, Rendering.BlockTextureModel> textureModelsLoad(List<TextAsset> modelAssets, BlockScriptData[] targets, List<(Texture2D, Rect)> atlasTextures)
        {
            var tempModels = new Dictionary<string, BlockTextureModelAbstract>();

            string parseParentName(TextAsset asset)
            {
                var json = JObject.Parse(asset.text);
                var parent = json["parent"]?.ToString();
                if (parent != null)
                    parent = getName(parent);
                return parent;
            }

            BlockTextureModelAbstract load(TextAsset asset)
            {
                if (tempModels.ContainsKey(asset.name))
                    return tempModels[asset.name];

                var parentName = parseParentName(asset);
                BlockTextureModelAbstract? parent;
                if (parentName == null || parentName == "block" || parentName == "thin_block")
                    parent = null;
                else if (tempModels.ContainsKey(parentName))
                    parent = tempModels[parentName];
                else
                    parent = tempModels[parentName] = load(modelAssets.Find(x => x.name == parentName));

                return parseModel(parent, asset);
            }

            Rendering.BlockTextureModel? convert2newmodel(BlockTextureModelAbstract model)
            {
                string findNameFromVariable(string directionKey)
                {
                    var textureName = removeHashtag(directionKey);

                    while (model.texVariableDict.ContainsKey(textureName) && model.texVariableDict[textureName].Contains('#'))
                        textureName = removeHashtag(model.texVariableDict[textureName]);
                    return textureName;
                }
                var elements = new List<Rendering.BlockTextureModel.Element>();

                for (int i = 0; i < model.elements.Count; i++)
                {
                    var elem = model.elements[i];
                    var resElem = new Rendering.BlockTextureModel.Element()
                    {
                        from = elem.from,
                        to = elem.to,
                        textures = new Dictionary<string, Texture2D>(),
                        patchUvDict = new Dictionary<string, Rect>(),
                        atlasUvDict = new Dictionary<string, Rect>(),
                    };

                    if (elem.faces.Values.Select(x => x.textureVariable).Contains("#overlay"))
                        continue;

                    foreach (var prop in elem.faces)
                    {
                        var textureName = findNameFromVariable(prop.Value.textureVariable);
                        if (!model.texVariableDict.ContainsKey(textureName))
                            return null;

                        var texName = model.texVariableDict[textureName];

                        (resElem.textures[prop.Key], resElem.atlasUvDict[prop.Key]) = atlasTextures.Find(x => x.Item1.name == texName);
                        resElem.patchUvDict[prop.Key] = prop.Value.patchUv;
                    }
                    elements.Add(resElem);
                }
                return new Rendering.BlockTextureModel(elements);
            }

            var models = new Dictionary<int, Rendering.BlockTextureModel>();
            foreach (var target in targets)
            {
                var modelName = target.textureModelName;
                var tempModel = load(modelAssets.Find(x => x.name == modelName));
                var newModel = convert2newmodel(tempModel);
                if (newModel.HasValue)
                    models[target.id] = newModel.Value;
                else
                    throw new InvalidDataException("Cannot parse model");
            }
            return models;
        }

        private static BlockTextureModelAbstract parseModel(BlockTextureModelAbstract? parent, in TextAsset asset)
        {
            Func<JToken, string, float[]> toFloatArr = (parent, key) => JArray.Parse(parent[key].ToString()).ToObject<float[]>();
            Func<float[], float[]> divide16 = (arr) => arr.Select(x => x / 16f).ToArray();
            Func<float[], Vector3> toVec = (a) => new Vector3(a[0], a[1], a[2]);
            Rendering.BlockTextureModel.Element.Rotation parseRotation(JToken obj)
            {
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
                return new Rendering.BlockTextureModel.Element.Rotation()
                {
                    pivot = toVec(divide16(toFloatArr(obj, "origin"))),
                    angle = obj["angle"].ToObject<float>(),
                    axis = axis,
                };
            }

            BlockTextureModelAbstract res = new BlockTextureModelAbstract();

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
                    var elem = new BlockTextureModelAbstract.Element()
                    {
                        from = toVec(divide16(toFloatArr(elemJson, "from"))),
                        to = toVec(divide16(toFloatArr(elemJson, "to"))),
                        faces = new Dictionary<string, BlockTextureModelAbstract.FaceElement>(),
                    };

                    if (elemJson["rotation"] != null)
                        elem.rotation = parseRotation(elemJson["rotation"]);

                    foreach (var face in elemJson["faces"].ToObject<JObject>())
                    {
                        var uv = new float[] { 0, 0, 1, 1 };
                        if (face.Value["uv"] != null)
                            uv = divide16(toFloatArr(face.Value, "uv"));

                        elem.faces[face.Key] = new BlockTextureModelAbstract.FaceElement()
                        {
                            textureVariable = face.Value["texture"].ToString(),
                            patchUv = Rect.MinMaxRect(uv[0], uv[1], uv[2], uv[3])
                        };
                    }
                    res.elements.Add(elem);
                }
            }
            else
            {
                // becareful copy
                res.elements = new List<BlockTextureModelAbstract.Element>(parent.elements);
                res.texVariableDict = new Dictionary<string, string>(parent?.texVariableDict);
            }

            if (json["textures"] != null)
                foreach (var prop in json["textures"].ToObject<JObject>())
                {
                    var temp = prop.Value.ToString();
                    if (!temp.Contains('#'))
                        temp = getName(temp);
                    res.texVariableDict[prop.Key] = temp;
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

        private class BlockTextureModelAbstract
        {
            public class FaceElement
            {
                public string textureVariable;
                public Rect patchUv;
            }

            public class Element
            {
                public Rendering.BlockTextureModel.Element.Rotation? rotation;
                public Vector3 from;
                public Vector3 to;
                public Dictionary<string, FaceElement> faces = new Dictionary<string, FaceElement>();
            }

            public List<Element> elements = new List<Element>();
            public Dictionary<string, string> texVariableDict = new Dictionary<string, string>();
        }
    }
}
