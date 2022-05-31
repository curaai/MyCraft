using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockTable : MonoBehaviour
{
    public Texture2D AtlasTexture;
    public List<Rect> TextureUvList;
    public List<BlockTextureModel> TextureModelList;
    public Material material;


    public void Start()
    {
        var bundle = BundleLoader.LoadBundle();
        var _textures = bundle.Item1.ToArray();
        AtlasTexture = new Texture2D(512, 512);

        TextureUvList = AtlasTexture.PackTextures(_textures, 0, 512, true).ToList();
        TextureModelList = bundle.Item2;
        material.mainTexture = AtlasTexture;
    }
}