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
    private void Start()
    {
        void modelLoad()
        {
            var modelBundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "models"));
            if (modelBundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                return;
            }
            // var models = modelBundle.LoadAllAssets<TextAsset>();
            var test_model = modelBundle.LoadAsset<TextAsset>("grass");
            JObject temp = JObject.Parse(test_model.text);
            Debug.Log(temp.ToString());

        }
        void textureLoad() {
            var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath + "/AssetBundles", "textures"));
            if (bundle == null)
            {
                Debug.Log("Failed to load Asset Bundle");
                return;
            }
            // var models = modelBundle.LoadAllAssets<TextAsset>();
            // var test_model = bundle.LoadAsset<>("grass");
            // JObject temp = JObject.Parse(test_model.text);
            // Debug.Log(temp.ToString());
        }
    }
}