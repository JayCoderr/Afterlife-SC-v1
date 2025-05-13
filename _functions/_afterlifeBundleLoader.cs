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
using static _afterlifeScModMenu._globalVariables;
using static MelonLoader.Utils.MelonEnvironment;
using static System.IO.Path;
using static System.IO.Directory;
using static System.IO.File;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;
using static UnityEngine.Quaternion;
using static UnityEngine.Vector3;
using static UnityEngine.Il2CppAssetBundleManager;
using static _afterlifeMod._functions;
using static UnityEngine.Camera;
using static UnityEngine.Random;
using static UnityEngine.Input;
using static UnityEngine.Physics;
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
    public class _afterlifeBundleLoader : MonoBehaviour
    {
        public static IEnumerator LoadAllAssetBundles()
        {
            string bundleDir = Combine(ModsDirectory, "AssetBundles");
            string configPath = Combine(bundleDir, "AfterlifeAssetBundles.txt");

            if (!Directory.Exists(bundleDir))
            {
                CreateDirectory(bundleDir);
                _afterlifeConsole($"📁 Created AssetBundles directory at: {bundleDir}");
            }

            if (!Directory.Exists(configPath))
            {
                WriteAllText(configPath, "# Add bundle filenames here, one per line (without .assetbundle if not needed)\n");
                _afterlifeConsole($"📝 Created missing config file: {configPath}");
                yield break;
            }

            string[] bundleLines = ReadAllLines(configPath);

            foreach (string rawLine in bundleLines)
            {
                string line = rawLine.Trim();

                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(',');

                if (parts.Length < 1)
                    continue;

                string bundleName = parts[0].Trim().ToLowerInvariant();
                string bundlePath = Combine(bundleDir, bundleName);

                Quaternion rotation = identity;
                Vector3 scale = one;

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

                _afterlifeConsole($"🔍 Looking for bundle at: {bundlePath}");

                if (!File.Exists(bundlePath))
                {
                    _afterlifeConsole($"⚠️ Bundle not found: {bundlePath}");
                    continue;
                }

                Il2CppAssetBundle bundlePtr = LoadFromFile(bundlePath);
                if (bundlePtr == null)
                {
                    _afterlifeConsole($"❌ Failed to load bundle: {bundleName}");
                    continue;
                }

                LoadedBundles[bundleName] = bundlePtr;

                Il2CppStringArray assetNames = bundlePtr.AllAssetNames();
                if (assetNames == null)
                {
                    _afterlifeConsole($"⚠️ No assets found in: {bundleName}");
                    continue;
                }

                _afterlifeConsole($"📦 Bundle '{bundleName}' contains {assetNames.Length} asset(s):");

                for (int i = 0; i < assetNames.Length; i++)
                {
                    string assetName = assetNames[i];

                    try
                    {
                        _afterlifeConsole($"🎯 Loading prefab: {assetName}");

                        GameObject prefab = bundlePtr.LoadAsset<GameObject>(assetName);
                        if (prefab == null)
                            continue;

                        // Place far away by default
                        GameObject instantiatedPrefab = Instantiate(prefab, new Vector3(999, 999, 999), rotation);
                        if (PrefabScales.TryGetValue(bundleName, out Vector3 scaleValue))
                        {
                            instantiatedPrefab.transform.localScale = scaleValue;
                        }
                        else
                        {
                            instantiatedPrefab.transform.localScale = one;
                        }

                        DontDestroyOnLoad(instantiatedPrefab);

                        string simpleName = GetFileNameWithoutExtension(assetName).ToLowerInvariant();
                        LoadedPrefabs[simpleName] = prefab;

                        _afterlifeConsole($"✅ Loaded prefab '{assetName}' at (999, 999, 999)");
                    }
                    catch (Exception ex)
                    {
                        _afterlifeConsole($"🔥 Exception loading asset '{assetName}': {ex}");
                    }
                }

                yield return null;
            }

            _afterlifeConsole("🎉 All bundles processed!");
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

                        Quaternion finalRotation = (rotateObj && rotation.HasValue) ? rotation.Value : identity;
                        Vector3 finalScale = scale ?? one;  // Use passed scale or default to Vector3.one

                        GameObject instance = Instantiate(prefab, spawnPosition, finalRotation);
                        instance.transform.localScale = finalScale;  // Apply scale

                        DontDestroyOnLoad(instance);

                        _afterlifeConsole($"✅ Instantiated prefab '{fullAssetName}' at position {spawnPosition} with rotation: {(rotateObj ? finalRotation.eulerAngles.ToString() : "none")} and scale: {finalScale}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _afterlifeConsole($"🔥 Error loading asset '{fullAssetName}' from a bundle: {ex.Message}");
                }
            }

            _afterlifeConsole($"❌ Asset '{fullAssetName}' not found in any loaded bundle.");
        }
        public static void DropCashAtLookPosition(Il2CppScheduleOne.ItemFramework.CashPickup cashPrefab)
        {
            if (cashPrefab == null)
            {
                _afterlifeConsole("❌ CashPrefab is null.");
                return;
            }

            Vector3 spawnPosition = GetLookOrMousePosition(); // Your own look/mouse point logic
            Quaternion spawnRotation = identity;

            GameObject gameObject = Instantiate(cashPrefab.gameObject, spawnPosition, spawnRotation);

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(main.transform.forward * Range(1.5f, 2.5f), ForceMode.VelocityChange);
                rb.AddTorque(insideUnitSphere * 2f, ForceMode.VelocityChange);
            }

            var netObj = gameObject.GetComponent<Il2CppFishNet.Object.NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(cashPrefab.gameObject); // ✅ This is the correct way to spawn over network
                _afterlifeConsole("✅ Spawned cash pickup over network.");
            }
            else
            {
                _afterlifeConsole("⚠️ Spawned cash object has no NetworkObject component.");
            }
        }


        private static Vector3 GetLookOrMousePosition()
        {
            Camera cam = main;
            if (cam == null)
            {
                _afterlifeConsole("❌ No main camera found.");
                return new Vector3(999, 999, 999);
            }

            Ray ray;

            if (mousePresent && GetMouseButton(0))
            {
                // If mouse is being clicked, raycast from mouse
                ray = cam.ScreenPointToRay(mousePosition);
            }
            else
            {
                // Otherwise, raycast from center of camera view
                ray = new Ray(cam.transform.position, cam.transform.forward);
            }

            if (Raycast(ray, out RaycastHit hit, 100f))
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
                SpawnAssetAtMouse(prefab);
            }
            else
            {
                _afterlifeConsole($"❌ Prefab '{simpleName}' not found in loaded prefabs.");
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