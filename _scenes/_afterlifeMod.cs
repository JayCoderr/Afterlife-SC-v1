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
using static _afterlifeAssetLoader._afterlifeBundleLoader;
using static _afterlifeMod._clientids;
using static _clientids._hudelements;
using static _clientids._advanceUnityEngineForgeMode;
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.AvatarFramework;
using static _afterlifeScModMenu._mainMenu;
using static MelonLoader.MelonCoroutines;
using static UnityEngine.GUI;
using static Il2CppScheduleOne.PlayerScripts.Player;
using static _afterlifeScModMenu._globalVariables;
using System.Diagnostics;
using System.Threading;

namespace _afterlifeMod
{
    public class _afterlifeMod : MelonMod
    {
        public override void OnLateInitializeMelon()
        {
            // Immediately fail if debugger is attached
            if (Debugger.IsAttached)
                Environment.FailFast("Debugger detected!");

            StartAntiDebugThread();

            // Start preloading assets without blocking the main thread
            _ = PreloadMainMenuAssets(); // Fire-and-forget async call
        }
        private void StartAntiDebugThread()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (Debugger.IsAttached)
                        Environment.FailFast("Debugger attached during runtime!");
                    Thread.Sleep(2000); // Check every 2 seconds
                }
            }).Start();
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Menu")
            {
                sceneStaticName = "Menu";
                _mainMenuRandomScene();
            }
            else
            {
                sceneStaticName = "Main";
                _stopMainMenuRandomScene();
            }
        }

        public override void OnUpdate()
        {
            StartAntiDebugThread();//can i put it on the OnUpdate so it checks every frame?
            MenuControls();
            MenuForgeMode(true);
        }

        public override void OnGUI()
        {
            Init();
            TestGUICode();
        }

        public static void TestGUICode()
        {
            if (styleNeedsUpdate && windowBackground != null)
            {
                dynamicStyle = new GUIStyle(skin.window);
                dynamicStyle.normal.background = windowBackground;
                styleNeedsUpdate = false;
            }
            if (!showGUIPlayer || TargetPlayer == null)
                return;

            Box(new Rect(10, 10, 200, 80), $"Teleport to: {TargetPlayer.name}");

            if (Button(new Rect(20, 40, 180, 30), "Teleport Now"))
            {
                Local.transform.position = TargetPlayer.transform.position;
            }
        }

    }
}