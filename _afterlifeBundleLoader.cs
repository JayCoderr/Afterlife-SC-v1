using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using MelonLoader;
using _clientids;
using UnityEngine;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.NPCs;
using System.Security.Cryptography;
using System.Linq;
using Ray = UnityEngine.Ray;
using System.Collections;
using Il2CppFishNet.Managing;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.DevUtilities;
using static _afterlifeMod._functions;
using MelonLoader.Utils;
using UnityEngine.Networking;
using System.IO;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace _clientids
{
    public class _afterlifeBundleLoader : MonoBehaviour
    {
        public static Dictionary<string, Il2CppAssetBundle> LoadedBundles = new Dictionary<string, Il2CppAssetBundle>();
        public static Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();

        public static IEnumerator LoadAllAssetBundles()
        {
            string bundleDir = Path.Combine(MelonEnvironment.ModsDirectory, "AssetBundles");
            string configPath = Path.Combine(bundleDir, "AfterlifeAssetBundles.txt");

            // Ensure directory exists
            if (!Directory.Exists(bundleDir))
            {
                Directory.CreateDirectory(bundleDir);
                MelonLogger.Msg($"📁 Created AssetBundles directory at: {bundleDir}");
            }

            // Create config file if it doesn't exist
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "# Add bundle filenames here, one per line (without .assetbundle if not needed)\n");
                MelonLogger.Warning($"📝 Created missing config file: {configPath}");
                yield break;
            }

            string[] bundleNames = File.ReadAllLines(configPath);

            foreach (string rawName in bundleNames)
            {
                string bundleName = rawName.Trim();

                // Skip empty or comment lines
                if (string.IsNullOrEmpty(bundleName) || bundleName.StartsWith("#"))
                    continue;

                string bundlePath = Path.Combine(bundleDir, bundleName);

                MelonLogger.Msg($"🔍 Looking for bundle at: {bundlePath}");

                if (!File.Exists(bundlePath))
                {
                    MelonLogger.Warning($"⚠️ Bundle not found: {bundlePath}");
                    continue;
                }

                Il2CppAssetBundle bundlePtr = Il2CppAssetBundleManager.LoadFromFile(bundlePath);
                if (bundlePtr == null)
                {
                    MelonLogger.Error($"❌ Failed to load bundle: {bundleName}");
                    continue;
                }

                LoadedBundles[bundleName] = bundlePtr;

                Il2CppStringArray assetNames = bundlePtr.AllAssetNames();
                if (assetNames == null)
                {
                    MelonLogger.Warning($"⚠️ No assets found in: {bundleName}");
                    continue;
                }

                MelonLogger.Msg($"📦 Bundle '{bundleName}' contains {assetNames.Length} asset(s):");

                for (int i = 0; i < assetNames.Length; i++)
                {
                    string assetName = assetNames[i];

                    try
                    {
                        MelonLogger.Msg($"🎯 Loading prefab: {assetName}");

                        GameObject prefab = bundlePtr.LoadAsset<GameObject>(assetName);
                        if (prefab == null)
                        {
                            MelonLogger.Error($"❌ Failed to load prefab: {assetName}");
                            continue;
                        }

                        LoadedPrefabs[assetName] = prefab;
                        MelonLogger.Msg($"✅ Loaded prefab '{assetName}' (not instantiated)");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"🔥 Exception loading asset '{assetName}': {ex}");
                    }
                }

                yield return null; // Allow frame to breathe between bundles
            }

            MelonLogger.Msg("🎉 All bundles processed!");
        }
    }
}
