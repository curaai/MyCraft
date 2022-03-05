using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class TextureAssetLoader : MonoBehaviour
{
    private readonly string AssetBundlePath = "Assets/AssetBundles/resourcepack.resource";
    private readonly string ItemTableName = "ItemTable";

    // AssetBundle & Deserialize test
    private void Start()
    {
        var itemTableBundle = AssetBundle.LoadFromFile(AssetBundlePath);
        var textAsset = itemTableBundle.LoadAsset<TextAsset>(ItemTableName);

        var setting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        var obj = JsonConvert.DeserializeObject<List<BlockData>>(textAsset.text, setting);
        Debug.Log(obj.Count);
        Debug.Log(obj[0].ToString());
    }
}