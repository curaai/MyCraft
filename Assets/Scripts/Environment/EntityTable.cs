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
    public class EntityTable
    {
        private static readonly string TextureBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "textures", "entity");
        private static readonly string TextureModelBundlePath = Path.Combine(Application.dataPath + "/AssetBundles", "models", "entity");

        public Material material;
        public MyCraft.Rendering.EntityTextureModel steve;

        public Texture2D steveTex;

        public EntityTable()
        {
            var modelAssets = loadBundle(TextureModelBundlePath).LoadAllAssets<TextAsset>().ToList();

            var steveModel = modelAssets.Find(x => x.name == "mobs");

            var textureAssets = loadBundle(TextureBundlePath).LoadAllAssets<Texture2D>().ToList();
            var steveTex = textureAssets.Find(x => x.name == "steve");

            var steve = loadSteve(steveModel, steveTex);
            this.steveTex = steveTex;

            this.steve = new MyCraft.Rendering.EntityTextureModel(steve, steveTex);
            material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = steveTex;
        }

        private EntityTextureModel loadSteve(TextAsset text, Texture2D tex)
        {
            var a = JsonConvert.DeserializeObject<EntityTextureModel>(JObject.Parse(text.text)["geometry.humanoid"].ToString());
            return a;
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


        public class EntityTextureModel
        {
            public class Cube
            {
                public Cube(float[] origin, float[] size, float[] uv)
                {
                    this.origin = divide16(arr2vec3(origin));
                    this.size = divide16(arr2vec3(size));
                    this.uv = Vector2Int.RoundToInt(arr2vec2(uv));
                }

                public Vector3 origin;
                public Vector3 size;
                public Vector2Int uv;

                public override string ToString() => $"{uv}";
            }

            public class Bone
            {
                public Bone(string name, float[] pivot, bool neverRender = false, bool mirror = false)
                {
                    this.name = name;
                    this.pivot = divide16(arr2vec3(pivot));
                    this.neverRender = neverRender;
                    this.mirror = mirror;
                }

                public string name;
                public Vector3 pivot;
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