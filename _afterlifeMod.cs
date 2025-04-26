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
using static _clientids._unityfunctions;
using static _clientids._airesponses;
using static _clientids._afterlifeBundleLoader;
using static _afterlifeMod._clientids;
using static _clientids._hudelements;
using static _clientids._advanceUnityEngineForgeMode;

namespace _afterlifeMod
{
    public static class BuildInfo
    {
        public const string Name = "AfterlifeModMenu"; 
        public const string Description = "Modded Game for Testing"; 
        public const string Author = "Jaycoder"; 
        public const string Company = null; 
        public const string Version = "1.0.0"; 
        public const string DownloadLink = "https://www.yourMomsBasement.com"; 
    }
    public class _afterlifeMod : MelonMod
    {
        public static string sceneStaticName = "";
        public static string versionResponse = "";
        public static string PostOffice = "";
        public static string PostOfficeXFloat = "";
        public static string PostOfficeYFloat = "";
        public static string PostOfficeZFloat = "";//GarageFrame

        public override void OnLateInitializeMelon()
        {
            PreloadPickup("$10 Pickup");
            PreloadPickup("Bungalow");
        }

        public static void PreloadPickup(string searchName)
        {
            if (_advanceUnityEngineForgeMode.cachedPickupSource != null) return; // Already cached, no need to search again

            MelonCoroutines.Start(WaitAndPreloadPickup(searchName));
        }

        private static IEnumerator WaitAndPreloadPickup(string searchName)
        {
            GameObject found = null;
            int attempts = 0;

            while (found == null && attempts++ < 3)
            {
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allObjects)
                {
                    if (go == null || string.IsNullOrEmpty(go.name)) continue;
                    if (go.name.Contains(searchName) && !go.name.Contains("Clone"))
                    {
                        found = go;
                        break;
                    }
                }

                if (found == null)
                {
                    yield return new WaitForSeconds(0.5f); // Wait before retrying
                }
            }

            if (found != null)
            {
                _advanceUnityEngineForgeMode.cachedPickupSource = found;
                MelonLogger.Msg($"✅ [Preload] Cached '{found.name}'");
            }
            else
            {
                MelonLogger.Warning($"❌ Could not preload '{searchName}'");
            }
        }
        public override async void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //_functions._networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
            MelonLogger.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
            if (sceneName == "Menu")
            {
                sceneStaticName = "Menu";
                versionResponse = await GetScheduleIAIResponse("what is the game version?", "system", "version request");
                MelonCoroutines.Start(ScheduleIObjectActive("Title", false));
                MelonCoroutines.Start(ScheduleIObjectActive("RV", false));
                MelonCoroutines.Start(ScheduleIObjectActive("Background", false));
                System.Random sysRand = new System.Random();
                string randomName = _allnpcs.allNpcCharacters[sysRand.Next(_allnpcs.allNpcCharacters.Length)];
                System.Random sysRandX = new System.Random();
                string[] namesX = { "PostOffice", "Barn" };
                string randomNameX = namesX[sysRandX.Next(namesX.Length)];
                MelonCoroutines.Start(_unityfunctions.SpawnScheduleIObjectCoroutine(randomNameX, new Vector3(-2.8537f, 0f, 0.3383f), Quaternion.Euler(358.5705f, 72.6876f, 0.0013f), true));
                GameObject existingNpc = GameObject.Find(randomName + "_Clone");
                if (existingNpc == null)
                {
                    DestroyAllPreviousNPCs();
                    MelonCoroutines.Start(_unityfunctions.SpawnScheduleIObjectCoroutine(randomName, new Vector3(-1.9f, 0f, -0.5818f), Quaternion.Euler(358.5705f, -50.6876f, 0.0013f), true));
                    MelonCoroutines.Start(MoveCloneOverTimeCoroutine(randomName + "_Clone", new Vector3(-1.9f, 0f, 0.9f), 5f));
                }
            }
            else
            {
                MelonLogger.Msg("🎯 Loading AssetBundle now...");
                MelonCoroutines.Start(LoadAllAssetBundles());
                sceneStaticName = "Main";                  
            }
        }
        void DestroyAllPreviousNPCs()
        {
            foreach (var name in _allnpcs.allNpcCharacters)
            {
                GameObject clone = GameObject.Find(name + "_Clone");
                if (clone != null)
                {
                    UnityEngine.Object.Destroy(clone);
                }
            }
        }
        public static bool forgeModeBool = false;  // Set from your UI
        public static bool forgeModeActive = false;
        public static bool lastForgeModeLogged = false;

        public override void OnUpdate()
        {
            MenuControls();
            MenuForgeMode(true);//just a soft lock for dumping all of the gameobjects
        }

        public override void OnGUI()
        {
            Init();
            if (styleNeedsUpdate && windowBackground != null)
            {
                dynamicStyle = new GUIStyle(GUI.skin.window);
                dynamicStyle.normal.background = windowBackground;
                styleNeedsUpdate = false;
            }
        }

    }
}