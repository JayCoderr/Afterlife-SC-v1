using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppTMPro;
using UnityEngine;
using System.Collections.Concurrent;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppFishNet.Object;
using UnityEngine.SceneManagement;

namespace _clientids
{
    internal class _unityfunctions
    {
        public static async Task ChangeScheduleITextByNameAsync(string searchText, string newText)
        {
            await Task.Yield(); // This is like waiting a frame asynchronously

            TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true); // include inactive

            bool found = false;
            foreach (TMP_Text tmp in allTexts)
            {
                if (tmp.text.Contains(searchText))
                {
                    MelonLogger.Msg($"[Match] GameObject: {tmp.gameObject.name} | Before: \"{tmp.text}\"");

                    tmp.text = tmp.text.Replace(searchText, newText);

                    MelonLogger.Msg($"[Updated] GameObject: {tmp.gameObject.name} | After: \"{tmp.text}\"");
                    found = true;
                }
            }

            if (!found)
            {
                MelonLogger.Warning($"No TMP_Text found containing: \"{searchText}\"");
            }
        }
        public static async Task SetParentGameObjectActiveByNameAsync(string searchName, bool setActive = false, Action<GameObject> callback = null)
        {
            // Wait for the next frame to ensure any async initialization is complete
            await Task.Yield();

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true); // include inactive objects
            GameObject parent = null;

            foreach (GameObject go in allObjects)
            {
                if (go.name.Contains(searchName))
                {
                    if (go.transform.parent != null)
                    {
                        parent = go.transform.parent.gameObject;
                        parent.SetActive(setActive);
                        MelonLogger.Msg($"[SetActive] Parent GameObject '{parent.name}' set to {(setActive ? "enabled" : "disabled")}");
                    }
                    else
                    {
                        MelonLogger.Warning($"GameObject '{go.name}' has no parent.");
                    }
                    break; // Exit after the first match (if you expect only one match)
                }
            }

            if (parent == null)
            {
                MelonLogger.Warning($"No GameObject found with name containing: \"{searchName}\"");
            }

            // Call the callback with the found parent object (or null if not found)
            callback?.Invoke(parent);
        }
        public static IEnumerator ScheduleIObjectActive(string searchName, bool setActive)
        {
            yield return null; // This gives Unity one frame to settle (like Task.Yield)

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true); // Include inactive objects
            bool found = false;

            foreach (GameObject go in allObjects)
            {
                if (go.name != null && go.name.Contains(searchName))
                {
                    go.SetActive(setActive);
                    MelonLogger.Msg($"[SetActive] GameObject '{go.name}' set to {(setActive ? "enabled" : "disabled")}");
                    found = true;
                }
            }

            if (!found)
            {
                MelonLogger.Warning($"No GameObject found with name containing: \"{searchName}\"");
            }

            yield break;
        }


        public static IEnumerator SpawnScheduleIObjectCoroutine(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive)
        {
            const float retryDelay = 0.5f;
            const int maxAttempts = 2;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                yield return null; // Wait one frame

                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (var go in allObjects)
                {
                    if (go == null || string.IsNullOrEmpty(go.name))
                        continue;

                    if (go.name.Contains(searchName))
                    {
                        // Log detailed info about each match attempt
                        MelonLogger.Msg($"[Debug] Found potential match: '{go.name}' | Scene: {go.scene.name} | Active: {go.activeSelf} | HideFlags: {go.hideFlags}");

                        // Temporarily comment out this filter for debugging
                        // if (!go.name.Contains("Clone") && go.scene.IsValid() && go.hideFlags == HideFlags.None)
                        if (!go.name.Contains("Clone"))
                        {
                            GameObject clone = UnityEngine.Object.Instantiate(go);
                            clone.name = go.name + "_Clone";
                            clone.transform.position = newPosition;
                            clone.transform.rotation = newRotation;
                            clone.SetActive(setActive);

                            MelonLogger.Msg($"✅ [Clone] Cloned '{go.name}' from scene '{go.scene.name}'");
                            MelonCoroutines.Start(ScheduleIObjectActive(clone.name, true));
                            MelonCoroutines.Start(ScheduleIObjectActive("RV", false));
                            yield break;
                        }
                    }
                }

                attempt++;
                MelonLogger.Msg($"[Clone] Waiting for '{searchName}'... (attempt {attempt})");
                yield return new WaitForSeconds(retryDelay);
            }

            MelonLogger.Warning($"❌ Could not find GameObject with name containing \"{searchName}\" after {maxAttempts} attempts.");
        }
        public static IEnumerator SpawnNetworkedSceneObjectCoroutine(
    string searchName,
    Vector3 newPosition,
    Quaternion newRotation,
    bool setActive,
    string targetSceneName = null,
    bool allowHiddenScenes = false,
    string requiredChildName = null)
        {
            const float retryDelay = 0.5f;
            const int maxAttempts = 30;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                yield return null;

                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (var go in allObjects)
                {
                    if (go == null || string.IsNullOrEmpty(go.name))
                        continue;

                    if (!go.name.Contains(searchName) || go.name.Contains("Clone"))
                        continue;

                    // Scene filter
                    if (!string.IsNullOrEmpty(targetSceneName))
                    {
                        if (!go.scene.IsValid() || !go.scene.name.Equals(targetSceneName, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    // Hidden scene filter
                    if (!allowHiddenScenes && (go.hideFlags != HideFlags.None || !go.scene.IsValid()))
                        continue;

                    // 🔍 Check for required child name
                    if (!string.IsNullOrEmpty(requiredChildName))
                    {
                        bool childFound = false;
                        foreach (Transform child in go.transform)
                        {
                            if (child.name.Contains(requiredChildName))
                            {
                                childFound = true;
                                break;
                            }
                        }

                        if (!childFound)
                        {
                            MelonLogger.Msg($"[Skip] '{go.name}' does not contain required child '{requiredChildName}'.");
                            continue;
                        }
                    }

                    // ✅ Clone and spawn
                    GameObject clone = UnityEngine.Object.Instantiate(go);
                    clone.name = go.name + "_Clone";
                    clone.transform.position = newPosition;
                    clone.transform.rotation = newRotation;
                    clone.SetActive(setActive);

                    MelonLogger.Msg($"✅ [Clone] Cloned '{go.name}' from scene '{go.scene.name}' (HideFlags: {go.hideFlags})");

                    SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());

                    // Networking
                    var networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
                    if (networkManager == null)
                    {
                        MelonLogger.Error("❌ Could not find FishNet NetworkManager.");
                        yield break;
                    }

                    var netObj = clone.GetComponent<NetworkObject>();
                    if (netObj == null)
                    {
                        MelonLogger.Error("❌ Clone is missing NetworkObject component.");
                        yield break;
                    }

                    if (networkManager.IsServer)
                    {
                        networkManager.ServerManager.Spawn(clone);
                        MelonLogger.Msg("📡 NetworkObject spawned on server.");
                    }
                    else
                    {
                        MelonLogger.Warning("❌ You must be the server to spawn network objects.");
                    }

                    MelonCoroutines.Start(ScheduleIObjectActive(clone.name, true));
                    yield break;
                }

                attempt++;
                MelonLogger.Msg($"[Clone] Waiting for '{searchName}'... (Attempt {attempt})");
                yield return new WaitForSeconds(retryDelay);
            }

            MelonLogger.Warning($"❌ Could not find GameObject named '{searchName}' with child '{requiredChildName}' in {maxAttempts} attempts.");
        }

        public static IEnumerator MoveCloneOverTimeCoroutine(string searchName, Vector3 targetPosition, float duration)
        {
            const float retryDelay = 0.5f;
            const int maxAttempts = 3;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                yield return null;

                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (var go in allObjects)
                {
                    if (go == null || string.IsNullOrEmpty(go.name))
                        continue;

                    // Look specifically for the "_Clone"
                    if (go.name.Contains(searchName) && go.name.Contains("_Clone"))
                    {
                        Vector3 startPosition = go.transform.position;
                        float elapsed = 0f;

                        MelonLogger.Msg($"🚚 [MoveClone] Moving '{go.name}' from {startPosition} to {targetPosition} over {duration} seconds.");

                        while (elapsed < duration)
                        {
                            if (go == null) yield break; // Handle destroyed object

                            elapsed += Time.deltaTime;
                            float t = Mathf.Clamp01(elapsed / duration);
                            go.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                            yield return null;
                        }

                        go.transform.position = targetPosition;
                        MelonLogger.Msg($"✅ [MoveClone] Finished moving '{go.name}' to {targetPosition}");
                        yield break;
                    }
                }

                attempt++;
                MelonLogger.Msg($"[MoveClone] Waiting for clone '{searchName}_Clone'... (attempt {attempt})");
                yield return new WaitForSeconds(retryDelay);
            }

            MelonLogger.Warning($"❌ Could not find cloned GameObject with name containing \"{searchName}_Clone\" after {maxAttempts} attempts.");
        }
    }
}
