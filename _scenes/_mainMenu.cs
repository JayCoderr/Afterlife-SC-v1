using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using static MelonLoader.MelonCoroutines;
using static _afterlifeScModMenu._globalVariables;
using _clientids;
using MelonLoader.Utils;
using UnityEngine;
using static System.IO.Path;
using static System.IO.Directory;
using static _afterlifeMod._functions;
using static _clientids._unityfunctions;
using static _clientids._airesponses;
using static _afterlifeAssetLoader._afterlifeBundleLoader;
using static MelonLoader.Utils.MelonEnvironment;
using static UnityEngine.GameObject;
using static MelonLoader.MelonLogger;
using static UnityEngine.Random;
using static UnityEngine.Object;
using static _clientids._allnpcs;
using static _clientids._advanceUnityEngineForgeMode;
using static _afterlifeScModMenu.AfterlifeUnityMapFunctions;
using Il2CppScheduleOne.AvatarFramework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.FX;
using System.Security;

namespace _afterlifeScModMenu
{
    public static class _mainMenu
    {
        public static object _afterlifeCoroutinesStart(IEnumerator routine)
        {
            return Start(routine);
        }
        public static void _afterlifeCoroutinesStop(object coroutineToken)
        {
            Stop(coroutineToken); // Just call the Stop method, no need to return anything
        }
        public static async Task PreloadMainMenuAssets()
        {
            await PreloadPickup("$10 Pickup");
            await PreloadPickup("Bungalow");
        }

        public static async Task PreloadPickup(string itemName)
        {
            if (cachedPickupSource != null) return; // Already cached, no need to search again

            await WaitAndPreloadPickupAsync(itemName); // This now runs the async version
        }

        public static async Task WaitAndPreloadPickupAsync(string searchName)
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
                    await Task.Delay(500);
                }
            }

            if (found != null)
            {
                cachedPickupSource = found;
                MelonLogger.Msg($"✅ [Preload] Cached '{found.name}'");
            }
            else
            {
                MelonLogger.Warning($"❌ Could not preload '{searchName}'");
            }
        }
        public async static void _mainMenuRandomScene()
        {
            _afterlifeCoroutinesStart(EnforceMusicMuteForever());
            versionResponse = await GetScheduleIAIResponse("what is the game version?", "system", "version request");
            _afterlifeCoroutinesStart(ScheduleIObjectActive("Title", false));
            _afterlifeCoroutinesStart(ScheduleIObjectActive("RV", false));
            _afterlifeCoroutinesStart(ScheduleIObjectActive("Background", false));
            _afterlifeCoroutinesStart(ScheduleIObjectActive("Rig", false));
            SpawnSingleNPC();
            Msg("🎯 Loading AssetBundle now...");
            _afterlifeCoroutinesStart(LoadAllAssetBundles());
            ToggleRandomSong();
        }

        public static void _stopMainMenuRandomScene()
        {
            ToggleRandomSong();
            _afterlifeCoroutinesStart(EnforceMusicMuteForever());
        }

        public static void ToggleRandomSong()
        {
            if (!Exists(AfterlifeModsDir))
            {
                Error("🎵 Music directory not found: " + AfterlifeModsDir);
                return;
            }

            // If already playing, stop it using the toggle behavior in PlayAudio
            if (isAudioPlaying && currentSongPath != null)
            {
                playingCoroutine = _afterlifeCoroutinesStart(PlayAudio(currentSongPath)); // This will toggle off
                return;
            }

            // Get all MP3 files
            if (mp3Files.Length == 0)
            {
                Warning("🎵 No MP3 files found in AfterlifeMusic folder.");
                return;
            }

            // Pick a random song
            currentSongPath = mp3Files[Range(0, mp3Files.Length)];
            Msg("🎶 Now playing: " + GetFileName(currentSongPath));

            // Start playback
            playingCoroutine = _afterlifeCoroutinesStart(PlayAudio(currentSongPath));
        }

        public static void SpawnSingleNPC()
        {
            if (_npcSpawningInProgress) return; // Prevent re-entrant calls
            _npcSpawningInProgress = true;
            _afterlifeCoroutinesStart(SpawnNPCSequence());
        }

        public static IEnumerator SpawnNPCSequence()
        {
            _npcSpawningInProgress = true;

            // Destroy existing NPCs
            DestroyAllPreviousNPCs();

            // Wait 1 frame to let Unity queue destruction
            yield return null;

            // Wait until all NPCs are actually destroyed
            while (!AreAllNPCsDestroyed())
                yield return null;

            // Spawn the location object
            yield return _afterlifeCoroutinesStart(_unityfunctions.SpawnScheduleIObjectCoroutine(
                randomNameX,
                new Vector3(-2.8537f, 0f, 0.3383f),
                Quaternion.Euler(358.5705f, 72.6876f, 0.0013f),
                true));

            // Spawn the new NPC
            yield return _afterlifeCoroutinesStart(_unityfunctions.SpawnScheduleIObjectCoroutine(
                randomName,
                new Vector3(-1.9f, 0.18f, -1),
                Quaternion.Euler(358.5705f, -50.6876f, 0.0013f),
                true));

            // Find the newly spawned NPC GameObject
            var avatarEffects = GetAvatarEffectsComponent(randomName);
            avatarEffects?.SetFireActive(true, true);

            // Move the NPC over time
            yield return _afterlifeCoroutinesStart(MoveCloneOverTimeCoroutine(
                randomName + "_Clone",
                new Vector3(-1.9f, 0.18f, 0.7818f),
                4.5f));

            _npcSpawningInProgress = false;
        }

        public static bool AreAllNPCsDestroyed()
        {
            foreach (var name in allNpcCharacters)
            {
                if (Find(name) != null || Find(name + "_Clone") != null)
                    return false;
            }
            return true;
        }

        public static void DestroyAllPreviousNPCs()
        {
            foreach (var name in allNpcCharacters)
            {
                string[] possibleNames = { name, name + "_Clone" };

                foreach (string targetName in possibleNames)
                {
                    GameObject obj = Find(targetName);
                    if (obj != null)
                    {
                        Destroy(obj);
                        Msg($"Destroyed: {targetName}");
                    }
                }
            }
        }
    }
}
