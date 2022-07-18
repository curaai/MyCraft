using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using MyCraft.Rendering;

namespace MyCraft.Environment
{
    // ! Support only Steve & zombie now
    public class EntityTable
    {
        private static readonly string TextureBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "textures", "entity");
        private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "models", "entity");

        // TODO: Make Entity int id & EntityData
        public EntityTextureModel this[string name] { get => table[name]; }
        public Material material;

        private static string[] ignoreCases = {
            // not mob
            "arrow", "experience_orb", "fishing_hook", 
            // not cube texture template
            "bat", 
            // overlapped texture template
            // ! not tested all entities in table (except Steve & zombie)
            "pig", "piglin", 
        };

        private Dictionary<string, MyCraft.Rendering.EntityTextureModel> table = new Dictionary<string, Rendering.EntityTextureModel>();

        public EntityTable()
        {
            var modelAssets = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>().ToList();
            var textureAssets = loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>().ToList();
            var atlasTexture = new Texture2D(4096, 4096) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var atlasRects = atlasTexture.PackTextures(textureAssets.ToArray(), 0, 4096);

            foreach (var model in modelAssets)
            {
                var name = model.name;
                if (model.name.Contains(".geo"))
                    name = model.name.Remove(model.name.IndexOf(".geo"));
                else if (name == "mobs")
                    name = "steve";

                var tex = textureAssets.Find(x => x.name == name);
                if (tex == null
                || model.name.Contains("v1.0") // duplicate model
                || ignoreCases.Contains(name))
                    continue;

                var idx = textureAssets.IndexOf(tex);
                table[name] = new EntityTextureModel(load(model, tex), tex, atlasRects[idx]);
            }

            material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = atlasTexture;
        }

        private JsonTextureModel? load(TextAsset text, Texture2D tex)
        {
            var root = JObject.Parse(text.text);
            var mainKey = root.Properties().Select(p => p.Name).Where(s => !s.Contains("format_version")).First();

            var temp = root[mainKey];
            if (temp.Type == JTokenType.Array)
                temp = temp[0];
            return JsonConvert.DeserializeObject<JsonTextureModel>(temp.ToString());
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


        public class JsonTextureModel
        {
            public class Cube
            {
                public Cube(float[] origin, float[] size, float[] uv, float inflate = 0)
                {
                    this.origin = divide16(arr2vec3(origin));
                    this.size = divide16(arr2vec3(size));
                    this.uv = Vector2Int.RoundToInt(arr2vec2(uv));
                    this.inflate = inflate;
                }

                public Vector3 origin;
                public Vector3 size;
                public Vector2Int uv;
                public float inflate; // * ignore now

                public override string ToString() => $"{uv}";
            }

            public class Bone
            {
                public Bone(string name, float[] pivot, string parent = null, float[] rotation = null, bool neverRender = false, bool mirror = false)
                {
                    this.name = name;
                    this.parent = parent;
                    this.neverRender = neverRender;
                    this.mirror = mirror;

                    if (pivot != null && pivot.Length > 0)
                        this.pivot = divide16(arr2vec3(pivot));
                    else
                        this.pivot = Vector3.zero;

                    if (rotation != null && rotation.Length > 0)
                        this.rotation = arr2vec3(pivot);
                    else
                        this.rotation = Vector3.zero;
                }

                public string name;
                public string parent;
                public Vector3 pivot;
                public Vector3 rotation;
                public bool neverRender;
                public bool mirror;

                public List<Cube> cubes = new List<Cube>();

                public override string ToString() => $"{name}, {pivot}";
            }

            public List<Bone> bones = new List<Bone>();
        }

        public static Vector3 divide16(Vector3 v) => v / 16;
        public static Vector3 arr2vec3(float[] a) => new Vector3(a[0], a[1], a[2]);
        public static Vector2 arr2vec2(float[] a) => new Vector2(a[0], a[1]);
    }
}