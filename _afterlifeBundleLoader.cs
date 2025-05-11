/*
        _____  __               .__  .__  _____                       .__                    .___         .___
_____ _/ ____\/  |_  ___________|  | |__|/ ____\____   _______   ____ |  |   _________     __| _/____   __| _/
\__  \\   __\\   __\/ __ \_  __ \  | |  \   __\/ __ \  \_  __ \_/ __ \|  |  /  _ \__  \   / __ |/ __ \ / __ | 
 / __ \|  |   |  | \  ___/|  | \/  |_|  ||  | \  ___/   |  | \/\  ___/|  |_(  <_> ) __ \_/ /_/ \  ___// /_/ | 
(____  /__|   |__|  \___  >__|  |____/__||__|  \___  >  |__|    \___  >____/\____(____  /\____ |\___  >____ | 
     \/                 \/                         \/               \/                \/      \/    \/     \/  
*/
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using MelonLoader;
using UnityEngine;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.NPCs;
using System.Security.Cryptography;
using System.Linq;
using Ray = UnityEngine.Ray;
using System.Collections;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.DevUtilities;
using MelonLoader.Utils;
using UnityEngine.Networking;
using System.IO;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Object;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.ItemFramework;
/*
_____  __               .__  .__  _____                       .__                    .___         .___
_____ _/ ____\/  |_  ___________|  | |__|/ ____\____   _______   ____ |  |   _________     __| _/____   __| _/
\__  \\   __\\   __\/ __ \_  __ \  | |  \   __\/ __ \  \_  __ \_/ __ \|  |  /  _ \__  \   / __ |/ __ \ / __ | 
/ __ \|  |   |  | \  ___/|  | \/  |_|  ||  | \  ___/   |  | \/\  ___/|  |_(  <_> ) __ \_/ /_/ \  ___// /_/ | 
(____  /__|   |__|  \___  >__|  |____/__||__|  \___  >  |__|    \___  >____/\____(____  /\____ |\___  >____ | 
\/                 \/                         \/               \/                \/      \/    \/     \/  
*/
namespace _afterlifeAssetLoader
{
    public static class BuildInfo
    {
        public const string Name = "Afterlife Reloaded AssetBundle Loader";
        public const string Description = "Loads unity assets.";
        public const string Author = "Jaycoder";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }
    public class _afterlifeBundleLoader : MonoBehaviour
    {
        public static Dictionary<string, Il2CppAssetBundle> LoadedBundles = new Dictionary<string, Il2CppAssetBundle>();
        public static Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();
        public static GameObject instantiatedPrefab;
        List<GameObject> LoadedPrefabsIndexed = new List<GameObject>();
        public static string assetName;
        public static Dictionary<string, Quaternion> PrefabRotations = new Dictionary<string, Quaternion>();
        public static Dictionary<string, Vector3> PrefabPositions = new Dictionary<string, Vector3>();
        public static Dictionary<string, Vector3> PrefabScales = new Dictionary<string, Vector3>();


        public static IEnumerator LoadAllAssetBundles()
        {
            string bundleDir = Path.Combine(MelonEnvironment.ModsDirectory, "AssetBundles");
            string configPath = Path.Combine(bundleDir, "AfterlifeAssetBundles.txt");

            if (!Directory.Exists(bundleDir))
            {
                Directory.CreateDirectory(bundleDir);
                MelonLogger.Msg($"📁 Created AssetBundles directory at: {bundleDir}");
            }

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, "# Add bundle filenames here, one per line (without .assetbundle if not needed)\n");
                MelonLogger.Warning($"📝 Created missing config file: {configPath}");
                yield break;
            }

            string[] bundleLines = File.ReadAllLines(configPath);

            foreach (string rawLine in bundleLines)
            {
                string line = rawLine.Trim();

                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(',');

                if (parts.Length < 1)
                    continue;

                string bundleName = parts[0].Trim().ToLowerInvariant();
                string bundlePath = Path.Combine(bundleDir, bundleName);

                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one; // default scale

                if (parts.Length >= 4)
                {
                    // Parse rotation (x, y, z)
                    if (float.TryParse(parts[1], out float rx) &&
                        float.TryParse(parts[2], out float ry) &&
                        float.TryParse(parts[3], out float rz))
                    {
                        rotation = Quaternion.Euler(rx, ry, rz);
                    }
                }

                if (parts.Length == 7)
                {
                    // Parse scale (x, y, z)
                    if (float.TryParse(parts[4], out float sx) &&
                        float.TryParse(parts[5], out float sy) &&
                        float.TryParse(parts[6], out float sz))
                    {
                        scale = new Vector3(sx, sy, sz);
                    }
                }

                // Save default or parsed rotation
                PrefabRotations[bundleName] = rotation;
                PrefabScales[bundleName] = scale;

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
                            continue;

                        // Place far away by default
                        GameObject instantiatedPrefab = UnityEngine.Object.Instantiate(prefab, new Vector3(999, 999, 999), rotation);
                        if (PrefabScales.TryGetValue(bundleName, out Vector3 scaleValue))
                        {
                            instantiatedPrefab.transform.localScale = scaleValue;
                        }
                        else
                        {
                            instantiatedPrefab.transform.localScale = Vector3.one;
                        }

                        UnityEngine.Object.DontDestroyOnLoad(instantiatedPrefab);

                        string simpleName = Path.GetFileNameWithoutExtension(assetName).ToLowerInvariant();
                        LoadedPrefabs[simpleName] = prefab;

                        MelonLogger.Msg($"✅ Loaded prefab '{assetName}' at (999, 999, 999)");
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"🔥 Exception loading asset '{assetName}': {ex}");
                    }
                }

                yield return null;
            }

            MelonLogger.Msg("🎉 All bundles processed!");
        }

        public static void InstantiateByFullAssetNameAtLookOrMouse(string fullAssetName, bool rotateObj = false, Quaternion? rotation = null, Vector3? scale = null)
        {
            foreach (var bundle in LoadedBundles.Values)
            {
                try
                {
                    GameObject prefab = bundle.LoadAsset<GameObject>(fullAssetName);
                    if (prefab != null)
                    {
                        Vector3 spawnPosition = GetLookOrMousePosition();

                        Quaternion finalRotation = (rotateObj && rotation.HasValue) ? rotation.Value : Quaternion.identity;
                        Vector3 finalScale = scale ?? Vector3.one;  // Use passed scale or default to Vector3.one

                        GameObject instance = UnityEngine.Object.Instantiate(prefab, spawnPosition, finalRotation);
                        instance.transform.localScale = finalScale;  // Apply scale

                        UnityEngine.Object.DontDestroyOnLoad(instance);

                        MelonLogger.Msg($"✅ Instantiated prefab '{fullAssetName}' at position {spawnPosition} with rotation: {(rotateObj ? finalRotation.eulerAngles.ToString() : "none")} and scale: {finalScale}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"🔥 Error loading asset '{fullAssetName}' from a bundle: {ex.Message}");
                }
            }

            MelonLogger.Error($"❌ Asset '{fullAssetName}' not found in any loaded bundle.");
        }
        public static void DropCashAtLookPosition(Il2CppScheduleOne.ItemFramework.CashPickup cashPrefab)
        {
            if (cashPrefab == null)
            {
                MelonLogger.Error("❌ CashPrefab is null.");
                return;
            }

            Vector3 spawnPosition = GetLookOrMousePosition(); // Your own look/mouse point logic
            Quaternion spawnRotation = Quaternion.identity;

            GameObject gameObject = UnityEngine.Object.Instantiate(cashPrefab.gameObject, spawnPosition, spawnRotation);

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Camera.main.transform.forward * UnityEngine.Random.Range(1.5f, 2.5f), ForceMode.VelocityChange);
                rb.AddTorque(UnityEngine.Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
            }

            var netObj = gameObject.GetComponent<Il2CppFishNet.Object.NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(cashPrefab.gameObject); // ✅ This is the correct way to spawn over network
                MelonLogger.Msg("✅ Spawned cash pickup over network.");
            }
            else
            {
                MelonLogger.Warning("⚠️ Spawned cash object has no NetworkObject component.");
            }
        }


        private static Vector3 GetLookOrMousePosition()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Error("❌ No main camera found.");
                return new Vector3(999, 999, 999);
            }

            Ray ray;

            if (Input.mousePresent && Input.GetMouseButton(0))
            {
                // If mouse is being clicked, raycast from mouse
                ray = cam.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                // Otherwise, raycast from center of camera view
                ray = new Ray(cam.transform.position, cam.transform.forward);
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return hit.point;
            }
            else
            {
                // Fallback: place 5 units in front of camera
                return cam.transform.position + cam.transform.forward * 5f;
            }
        }

        public static void SpawnLoadedPrefabByName(string simpleName)
        {
            if (LoadedPrefabs.TryGetValue(simpleName.ToLowerInvariant(), out GameObject prefab))
            {
                _afterlifeMod._functions.SpawnAssetAtMouse(prefab);
            }
            else
            {
                MelonLogger.Error($"❌ Prefab '{simpleName}' not found in loaded prefabs.");
            }
        }
    }
}
/*
        _____  __               .__  .__  _____                       .__                    .___         .___
_____ _/ ____\/  |_  ___________|  | |__|/ ____\____   _______   ____ |  |   _________     __| _/____   __| _/
\__  \\   __\\   __\/ __ \_  __ \  | |  \   __\/ __ \  \_  __ \_/ __ \|  |  /  _ \__  \   / __ |/ __ \ / __ | 
 / __ \|  |   |  | \  ___/|  | \/  |_|  ||  | \  ___/   |  | \/\  ___/|  |_(  <_> ) __ \_/ /_/ \  ___// /_/ | 
(____  /__|   |__|  \___  >__|  |____/__||__|  \___  >  |__|    \___  >____/\____(____  /\____ |\___  >____ | 
     \/                 \/                         \/               \/                \/      \/    \/     \/  
*/