using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoader
{
    public static class BundleLoader
    {
        private static Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

        public static AssetBundle LoadAssetBundle(string filename)
        {
            string gameDirectory = Path.GetDirectoryName(Application.dataPath);
            string relativePath = Path.Combine("BepInEx", "plugins", "tarkin", "bundles", filename);
            string fullPath = Path.Combine(gameDirectory, relativePath);

            string key = System.IO.Path.GetFileName(fullPath);

            if (loadedAssetBundles.ContainsKey(key))
            {
                return loadedAssetBundles[key];
            }

            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (assetBundle == null)
            {
                Plugin.Log.LogError("Failed to load AssetBundle at path: " + fullPath);
                return null;
            }

            loadedAssetBundles.Add(key, assetBundle);
            return assetBundle;
        }
    }
}
