using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using _clientids;
using Il2CppFishNet.Managing;
using Il2CppFishNet.Object;
using Il2CppFishNet;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppInterop.Runtime;
using Il2CppFishNet.Serializing;
using Il2CppFishNet.Transporting;
using System.Runtime.CompilerServices;
using Il2CppEasyButtons;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Object.Delegating;
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Interaction;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Persistence.Datas;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.ATM;
using Il2CppScheduleOne;
using UnityEngine.Events;
using System.Net;
using UnityEngine.UI;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.Rendering.PostProcessing;
using Il2CppScheduleOne.PlayerScripts.Health;
using System.Threading;
using Il2CppScheduleOne.AvatarFramework;
using Il2Cpp;
using Il2CppScheduleOne.AvatarFramework.Equipping;
using HarmonyLib;
using Il2CppSystem.IO;
using MelonLoader.Utils;
using Il2CppInterop.Runtime.Startup;
using Il2CppInterop.Runtime.Runtime;
using UnityEngine.Jobs;
using Il2CppScheduleOne.Police;
using Il2CppScheduleOne.Skating;
using UnityEngine.Events;
using Il2CppFunly.SkyStudio;
using static Il2CppScheduleOne.DevUtilities.ValueTracker;
using UnityEditor.Experimental;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using RenderSettings = UnityEngine.RenderSettings;
using UnityEngine.PostProcessing;
using AtmosphericHeightFog;
using Il2CppScheduleOne.FX;
using Il2CppScheduleOne.Audio;
using static _afterlifeScModMenu._globalVariables;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;
using static _afterlifeScModMenu._mainMenu;
using static _afterlifeScModMenu.AfterlifeUnityMapFunctions;
using System.ComponentModel;
using UnityEngine.Playables;

namespace _afterlifeMod
{
    public class DraggablePanel : MonoBehaviour
    {
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void OnMouseDown()
        {
            // Start dragging
            isDragging = true;
            offset = rectTransform.position - (Vector3)Input.mousePosition;
        }

        void OnMouseUp()
        {
            // Stop dragging
            isDragging = false;
        }

        void Update()
        {
            if (isDragging)
            {
                // Update the position of the panel based on mouse movement
                rectTransform.position = (Vector2)Input.mousePosition + offset;
            }
        }
    }
    public static class _functions
    {
        public static void Test()
        {
            _afterlifeConsole("this is a test");
        }
        public static void TestMsg(string msg)
        {
            _afterlifeConsole(msg);
        }
        public static void SetJumpForce(float forceValue)
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.jumpForce = forceValue;
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }
        public static void SetMoveSpeed(float speedForceValue)
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.CurrentSprintMultiplier = speedForceValue;
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }
        public static bool AutoTeaBagEnabled = false;

        public static void AutoTeaBagMode()
        {
            AutoTeaBagEnabled = !AutoTeaBagEnabled;

            if (AutoTeaBagEnabled)
            {
                _afterlifeConsole("✅ Teabagging the ground...");
                _afterlifeCoroutinesStart(AutoTeaBagLoop());
            }
            else
            {
                _afterlifeConsole("❌ Stopped teabagging.");
            }
        }

        public static void AutoDeleteNearbyCollidersToggle()
        {
            autoDeleteEnabled = !autoDeleteEnabled;
            if (autoDeleteEnabled)
            {
                _afterlifeConsole("✅ Auto-deleting nearby colliders enabled.");
                _afterlifeCoroutinesStart(AutoDeleteNearbyColliders());
            }
            else
            {
                _afterlifeConsole("❌ Auto-deleting nearby colliders disabled. Restoring objects.");
                foreach (var obj in hiddenObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
                hiddenObjects.Clear();
            }
        }

        private static IEnumerator AutoDeleteNearbyColliders()
        {
            while (autoDeleteEnabled)
            {
                if (Player.Local != null && Player.Local.LocalGameObject != null)
                {
                    Vector3 playerPos = Player.Local.LocalGameObject.transform.position;

                    // Check all colliders within 10 units
                    Collider[] nearby = Physics.OverlapSphere(playerPos, 10f);
                    foreach (var col in nearby)
                    {
                        GameObject obj = col.gameObject;

                        if (obj != null && obj != Player.Local.LocalGameObject && obj.activeSelf)
                        {
                            float distance = Vector3.Distance(playerPos, obj.transform.position);

                            if (distance < 10f && !hiddenObjects.Contains(obj))
                            {
                                obj.SetActive(false);
                                hiddenObjects.Add(obj);
                            }
                        }
                    }

                    // Reactivate any objects that are now 25+ units away
                    for (int i = hiddenObjects.Count - 1; i >= 0; i--)
                    {
                        GameObject obj = hiddenObjects[i];
                        if (obj == null) continue;

                        float distance = Vector3.Distance(playerPos, obj.transform.position);
                        if (distance >= 25f)
                        {
                            obj.SetActive(true);
                            hiddenObjects.RemoveAt(i);
                        }
                    }
                }

                yield return new WaitForSeconds(1f); // check every 1 second
            }
        }

        public static void ToggleFOV()
        {
            isWideFOV = !isWideFOV;
            Camera.main.fieldOfView = isWideFOV ? 100f : 60f;
        }
        public static IEnumerator SmoothFOV(float targetFOV, float duration = 1f)
        {
            float startFOV = Camera.main.fieldOfView;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Camera.main.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Camera.main.fieldOfView = targetFOV;
        }
        private static IEnumerator AutoTeaBagLoop()
        {
            yield return WaitForPlayerMovement((playerMovement) =>
            {
                _afterlifeCoroutinesStart(TeaBagRoutine(playerMovement));
            });
        }

        private static IEnumerator TeaBagRoutine(PlayerMovement playerMovement)
        {
            while (AutoTeaBagEnabled)
            {
                playerMovement.isCrouched = true;
                yield return new WaitForSeconds(0.3f);
                playerMovement.isCrouched = false;
                yield return new WaitForSeconds(0.3f);
            }
        }

        public static void ToggleRotateToPoliceNPC()
        {
            isRotatingToPoliceNPC = !isRotatingToPoliceNPC;

            if (isRotatingToPoliceNPC)
                _afterlifeCoroutinesStart(RotateToNearestPoliceNPCTask());
        }

        private static IEnumerator RotateToNearestPoliceNPCTask()
        {
            while (isRotatingToPoliceNPC)
            {
                if (Input.GetMouseButton(1)) // RMB held
                {
                    GameObject nearestPoliceNPC = null;
                    float shortestDistance = float.MaxValue;
                    Vector3 playerPos = Player.Local.transform.position;

                    foreach (string npcName in _allnpcs.allNpcCharacters)
                    {
                        if (!npcName.ToLower().Contains("police"))
                            continue;

                        GameObject npcObj = GameObject.Find(npcName);
                        if (npcObj == null) continue;

                        float dist = Vector3.Distance(playerPos, npcObj.transform.position);
                        if (dist < shortestDistance)
                        {
                            shortestDistance = dist;
                            nearestPoliceNPC = npcObj;
                        }
                    }

                    if (nearestPoliceNPC != null)
                    {
                        Vector3 dir = nearestPoliceNPC.transform.position - playerPos;
                        Quaternion rot = Quaternion.LookRotation(dir.normalized);
                        Player.Local.transform.rotation = Quaternion.Slerp(Player.Local.transform.rotation, rot, Time.deltaTime * 5f);
                    }
                }

                yield return null;
            }
        }

        public static void InvisibleToggle(bool Active)
        {
            Player.Local.Avatar.SetVisible(Active);
           // _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(Avatar.Object, false));
        }

        public static void SetWorldTime(int timeToSet)
        {
            NetworkSingleton<TimeManager>.Instance.SetTime(timeToSet, false);
        }

        public static void SetWorldTimeScale(int timeToSet)
        {
            NetworkSingleton<TimeManager>.Instance.TimeProgressionMultiplier = timeToSet;
        }

        public static void SetTimeScale(string[] args)
        {
            if (args.Length == 0 || !float.TryParse(args[0], out float time))
            {
                MelonLogger.Warning("❌ Invalid or missing time argument.");
                return;
            }

            try
            {
                var cmd = new Il2CppScheduleOne.Console.SetTimeScale();
                var commandList = new Il2CppSystem.Collections.Generic.List<string>();
                commandList.Add("settimescale");
                commandList.Add(time.ToString());
                cmd.Execute(commandList);

                _afterlifeConsole($"✅ World time set to {time}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"❌ Failed to set world time: {ex.Message}");
            }
        }

        public static void SetGravity(float gravityForceValue)
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.gravityMultiplier = gravityForceValue;
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }
        public static void SetSlipperyMovement(float slipperyAmount)
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.SlipperyMovementMultiplier = slipperyAmount;
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }
        public static void SetJumpForceD(params object[] args)
        {
            if (args.Length > 0 && args[0] is float force)
            {
                SetJumpForce(force);
            }
        }
        public static void SetMovementSpeed(float SpeedValue)
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.MoveSpeedMultiplier = SpeedValue;
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }

        public static void TeleportToGround()
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                playerMovement.WarpToNavMesh();
                _afterlifeConsole($"✅ PlayerMovement component found and modified.\nSet Jump Force: {playerMovement.jumpForce.ToString()}");
            }));
        }

        private static void UpdateJsonValue(JObject appearance, string key, JToken newValue)
        {
            if (appearance[key] != null)
            {
                appearance[key] = newValue;
            }
            else
            {
                appearance.Add(key, newValue);
            }
        }
        public static void SetAppearanceProperty(string property, string value)
        {
            _afterlifeCoroutinesStart(WaitForPlayerScripts(playerScripts =>
            {
                try
                {
                    // Get the current appearance
                    string currentAppearanceJson = playerScripts.GetAppearanceString();
                    if (string.IsNullOrEmpty(currentAppearanceJson))
                    {
                        MelonLogger.Error("❌ Failed to retrieve current appearance.");
                        return;
                    }

                    // Parse the current appearance JSON
                    JObject appearance = JObject.Parse(currentAppearanceJson);

                    // Handle each property
                    switch (property.ToLower())
                    {
                        case "gender":
                            UpdateJsonValue(appearance, "Gender", int.Parse(value));
                            break;

                        case "weight":
                            UpdateJsonValue(appearance, "Weight", float.Parse(value));
                            break;

                        case "skincolor":
                            string[] rgba = value.Split(',');
                            if (rgba.Length == 4 &&
                                float.TryParse(rgba[0], out float r) &&
                                float.TryParse(rgba[1], out float g) &&
                                float.TryParse(rgba[2], out float b) &&
                                float.TryParse(rgba[3], out float a))
                            {
                                JObject skinColor = new JObject
                                {
                                    ["r"] = r,
                                    ["g"] = g,
                                    ["b"] = b,
                                    ["a"] = a
                                };
                                UpdateJsonValue(appearance, "SkinColor", skinColor);
                            }
                            break;

                        case "hairstyle":
                            UpdateJsonValue(appearance, "HairStyle", value);
                            break;

                        case "haircolor":
                            string[] hairRgba = value.Split(',');
                            if (hairRgba.Length == 4 &&
                                float.TryParse(hairRgba[0], out float hr) &&
                                float.TryParse(hairRgba[1], out float hg) &&
                                float.TryParse(hairRgba[2], out float hb) &&
                                float.TryParse(hairRgba[3], out float ha))
                            {
                                JObject hairColor = new JObject
                                {
                                    ["r"] = hr,
                                    ["g"] = hg,
                                    ["b"] = hb,
                                    ["a"] = ha
                                };
                                UpdateJsonValue(appearance, "HairColor", hairColor);
                            }
                            break;

                        case "mouth":
                            UpdateJsonValue(appearance, "Mouth", value);
                            break;

                        case "eyeballcolor":
                            string[] eyeballRgba = value.Split(',');
                            if (eyeballRgba.Length == 4 &&
                                float.TryParse(eyeballRgba[0], out float er) &&
                                float.TryParse(eyeballRgba[1], out float eg) &&
                                float.TryParse(eyeballRgba[2], out float eb) &&
                                float.TryParse(eyeballRgba[3], out float ea))
                            {
                                JObject eyeballColor = new JObject
                                {
                                    ["r"] = er,
                                    ["g"] = eg,
                                    ["b"] = eb,
                                    ["a"] = ea
                                };
                                UpdateJsonValue(appearance, "EyeballColor", eyeballColor);
                            }
                            break;

                        case "eyewearcolor":
                            string[] eyewearRgba = value.Split(',');
                            if (eyewearRgba.Length == 4 &&
                                float.TryParse(eyewearRgba[0], out float er1) &&
                                float.TryParse(eyewearRgba[1], out float eg1) &&
                                float.TryParse(eyewearRgba[2], out float eb1) &&
                                float.TryParse(eyewearRgba[3], out float ea1))
                            {
                                JObject eyewearColor = new JObject
                                {
                                    ["r"] = er1,
                                    ["g"] = eg1,
                                    ["b"] = eb1,
                                    ["a"] = ea1
                                };
                                UpdateJsonValue(appearance, "EyewearColor", eyewearColor);
                            }
                            break;

                        case "topcolor":
                            string[] topRgba = value.Split(',');
                            if (topRgba.Length == 4 &&
                                float.TryParse(topRgba[0], out float tr) &&
                                float.TryParse(topRgba[1], out float tg) &&
                                float.TryParse(topRgba[2], out float tb) &&
                                float.TryParse(topRgba[3], out float ta))
                            {
                                JObject topColor = new JObject
                                {
                                    ["r"] = tr,
                                    ["g"] = tg,
                                    ["b"] = tb,
                                    ["a"] = ta
                                };
                                UpdateJsonValue(appearance, "TopColor", topColor);
                            }
                            break;

                        case "bottomcolor":
                            string[] bottomRgba = value.Split(',');
                            if (bottomRgba.Length == 4 &&
                                float.TryParse(bottomRgba[0], out float br) &&
                                float.TryParse(bottomRgba[1], out float bg) &&
                                float.TryParse(bottomRgba[2], out float bb) &&
                                float.TryParse(bottomRgba[3], out float ba))
                            {
                                JObject bottomColor = new JObject
                                {
                                    ["r"] = br,
                                    ["g"] = bg,
                                    ["b"] = bb,
                                    ["a"] = ba
                                };
                                UpdateJsonValue(appearance, "BottomColor", bottomColor);
                            }
                            break;

                        case "shoescolor":
                            string[] shoesRgba = value.Split(',');
                            if (shoesRgba.Length == 4 &&
                                float.TryParse(shoesRgba[0], out float sr) &&
                                float.TryParse(shoesRgba[1], out float sg) &&
                                float.TryParse(shoesRgba[2], out float sb) &&
                                float.TryParse(shoesRgba[3], out float sa))
                            {
                                JObject shoesColor = new JObject
                                {
                                    ["r"] = sr,
                                    ["g"] = sg,
                                    ["b"] = sb,
                                    ["a"] = sa
                                };
                                UpdateJsonValue(appearance, "ShoesColor", shoesColor);
                            }
                            break;

                        case "headwearcolor":
                            string[] headwearRgba = value.Split(',');
                            if (headwearRgba.Length == 4 &&
                                float.TryParse(headwearRgba[0], out float hr1) &&
                                float.TryParse(headwearRgba[1], out float hg1) &&
                                float.TryParse(headwearRgba[2], out float hb1) &&
                                float.TryParse(headwearRgba[3], out float ha1))
                            {
                                JObject headwearColor = new JObject
                                {
                                    ["r"] = hr1,
                                    ["g"] = hg1,
                                    ["b"] = hb1,
                                    ["a"] = ha1
                                };
                                UpdateJsonValue(appearance, "HeadwearColor", headwearColor);
                            }
                            break;

                        case "tattoos":
                            JArray tattoos = JArray.Parse(value);
                            UpdateJsonValue(appearance, "Tattoos", tattoos);
                            break;

                        default:
                            MelonLogger.Error($"❌ Unsupported property: {property}");
                            return;
                    }

                    // Serialize the updated JSON and apply the new appearance
                    string updatedJson = appearance.ToString();
                    playerScripts.LoadAppearance(updatedJson);
                    _afterlifeConsole($"✅ Updated {property} successfully.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"❌ Failed to update appearance: {ex}");
                }
            }));
        }
        public static IEnumerator WaitForPlayerMovement(Action<Il2CppScheduleOne.PlayerScripts.PlayerMovement> callback)
        {
            GameObject player = null;
            Il2CppScheduleOne.PlayerScripts.PlayerMovement playerMovement = null;

            // Wait until Player.Local.LocalGameObject is not null
            while ((player = Player.Local?.LocalGameObject) == null)
            {
                yield return null; // wait one frame
            }

            // Wait until PlayerMovement component is found
            while ((playerMovement = player.GetComponent<Il2CppScheduleOne.PlayerScripts.PlayerMovement>()) == null)
            {
                yield return null; // wait one frame
            }

            callback?.Invoke(playerMovement);
        }
        public static IEnumerator WaitForRangedWeapon(Action<Il2CppScheduleOne.AvatarFramework.Equipping.AvatarRangedWeapon> callback)
        {
            Il2CppSystem.Object equippable = null;
            Il2CppScheduleOne.AvatarFramework.Equipping.AvatarRangedWeapon weapon = null;

            // Wait for Player, Avatar, and CurrentEquippable to become available
            while ((equippable = Player.Local?.Avatar?.CurrentEquippable) == null)
                yield return null;

            // Wait until it's castable as AvatarRangedWeapon
            while ((weapon = equippable.TryCast<Il2CppScheduleOne.AvatarFramework.Equipping.AvatarRangedWeapon>()) == null)
                yield return null;

            callback?.Invoke(weapon);
        }

        public static IEnumerator WaitForPlayerScripts(Action<Il2CppScheduleOne.PlayerScripts.Player> callback)
        {
            GameObject localObj = null;
            GameObject parent = null;
            Il2CppScheduleOne.PlayerScripts.Player playerScripts = null;

            while ((localObj = Player.Local?.LocalGameObject) == null)
            {
                _afterlifeConsole("⏳ Waiting for LocalGameObject...");
                yield return null;
            }

            _afterlifeConsole("✅ Found LocalGameObject");

            // Get parent GameObject
            parent = localObj.transform.parent?.gameObject;

            while (parent == null)
            {
                _afterlifeConsole("⏳ Waiting for LocalGameObject's parent...");
                yield return null;
                parent = localObj.transform.parent?.gameObject;
            }

            _afterlifeConsole("✅ Found Parent GameObject");

            while ((playerScripts = parent.GetComponent<Il2CppScheduleOne.PlayerScripts.Player>()) == null)
            {
                _afterlifeConsole("⏳ Waiting for PlayerScripts on parent...");
                yield return null;
            }

            _afterlifeConsole("✅ Found PlayerScripts on parent");

            callback?.Invoke(playerScripts);
        }

        public async static void UnlockDemoFeatures()
        {
            /*
            await _unityfunctions.ScheduleIObjectActive("DemoEndScreen", false);
            await _unityfunctions.ScheduleIObjectActive("Demo blocker", false);
            await _unityfunctions.ScheduleIObjectActive("Demo Barrier", false);
            await _unityfunctions.ScheduleIObjectActive("RoadNodes Demo", false);
            await _unityfunctions.ScheduleIObjectActive("DemoCheckpoint1", false);
            await _unityfunctions.ScheduleIObjectActive("DemoCheckpoint2", false);
            await _unityfunctions.ScheduleIObjectActive("Shirley (Demo)", false);
            await _unityfunctions.ScheduleIObjectActive("DemoBoundaries", false);
            await _unityfunctions.ScheduleIObjectActive("DemoBoundaries", false);
            await _unityfunctions.ScheduleIObjectActive("DemoBoundaries", false);
            */
        }

        public static void FlyMode()
        {
            _afterlifeCoroutinesStart(WaitForGameObjectCreation("Player_Local", onObjectFound: (foundObject) =>
            {
                Transform parentTransform = foundObject.transform.parent;
                Vector3 newParentPosition = parentTransform.position;

                if (flyModeOption)
                {
                    //MiddleScreenNotify("Flymode Activated: Crouch & RB to fly.");

                    if (parentTransform != null)
                    {
                        // Start and store the coroutine
                        flyModeCoroutine = _afterlifeCoroutinesStart(FlyMode(parentTransform, newParentPosition, true));
                        flyModeActive = true;
                    }
                    else
                    {
                        MelonLogger.Warning("❌ Parent not found.");
                    }

                    flyModeOption = false;
                }
                else
                {
                    //MiddleScreenNotify("Flymode Deactivated: You are no longer superman.");

                    // Stop the coroutine if it's running
                    if (flyModeCoroutine != null)
                    {
                        MelonCoroutines.Stop(flyModeCoroutine);
                        flyModeCoroutine = null;
                    }

                    flyModeOption = true;
                    flyModeActive = false;
                    isFollowingMouse = false;
                }
            }));
        }
        public static IEnumerator FlyMode(Transform parentTransform, Vector3 currentPosition, bool boolFly)
        {
            Camera mainCamera = Camera.main;
            float moveToPositionSpeed = 5.0f;
            float fixedDistance = 10.0f; // distance in front of camera
            Vector3 lockedPosition = parentTransform.position;

            while (boolFly)
            {
                // Toggle FlyMode on Ctrl press
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl) || UnityEngine.Input.GetKeyDown(KeyCode.RightControl))
                {
                    flyModeActive = !flyModeActive;
                    isFollowingMouse = false;

                    if (flyModeActive)
                    {
                        lockedPosition = parentTransform.position;
                        //MiddleScreenNotify("Flymode Activated: Your now superman.");
                    }
                    else
                    {
                        //MiddleScreenNotify("Flymode Deactivated: You fell on your face!");
                    }

                    yield return null;
                }

                if (flyModeActive)
                {
                    // Start following on LMB press
                    if (UnityEngine.Input.GetMouseButtonDown(0))
                    {
                        isFollowingMouse = true;
                        _afterlifeConsole("🟢 Started following mouse.");
                    }

                    // Stop following on LMB release
                    if (UnityEngine.Input.GetMouseButtonUp(0))
                    {
                        isFollowingMouse = false;
                        _afterlifeConsole("🔴 Stopped following mouse. Position frozen.");
                    }

                    if (isFollowingMouse)
                    {
                        // Get a world position in front of the camera
                        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * fixedDistance;
                        lockedPosition = Vector3.Lerp(lockedPosition, targetPosition, Time.deltaTime * moveToPositionSpeed);
                    }

                    parentTransform.position = lockedPosition;
                }

                yield return null;
            }
        }
        public static IEnumerator WaitForGameObjectCreation(
            string gameObjectName,
            string componentTypeName = null,
            string propertyToUse = null,
            string methodToInvoke = null,
            object methodArgument = null,
            Action<GameObject> onObjectFound = null)  // New parameter
        {
            foundObject = null;

            // Keep checking every frame for the GameObject to be created
            while (foundObject == null)
            {
                // Find the GameObject by its name
                foundObject = GameObject.Find(gameObjectName);

                // Wait for the next frame before checking again
                yield return null;
            }

            // Once the GameObject is found, log the result
            _afterlifeConsole($"✅ Found GameObject: {foundObject.name}");

            // Print the hierarchy of the GameObject
            PrintHierarchy(foundObject);

            // Print the parent of the found GameObject
            if (foundObject.transform.parent != null)
            {
                _afterlifeConsole($"Parent of {foundObject.name} is: {foundObject.transform.parent.name}");
            }
            else
            {
                _afterlifeConsole($"The GameObject {foundObject.name} has no parent.");
            }

            // Invoke the callback (if provided) after finding the object
            onObjectFound?.Invoke(foundObject);  // Callback invocation

            // Return the found object for further use
            yield return foundObject;

            // Get all components on the GameObject
            UnityEngine.Component[] components = foundObject.GetComponents<UnityEngine.Component>();

            bool foundComponent = false; // Track if the component is found
            List<UnityEngine.Component> matchedComponents = new List<UnityEngine.Component>(); // List to store matched components

            Type componentType = null;
            if (componentTypeName != null)
            {
                // Try to get the type from the current assembly
                componentType = Type.GetType(componentTypeName);

                // If we still couldn't find it, we need to load it explicitly
                if (componentType == null)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        componentType = assembly.GetType(componentTypeName);
                        if (componentType != null)
                            break;
                    }

                    if (componentType == null)
                    {
                        MelonLogger.Warning($"❌ Could not find type: {componentTypeName}. Make sure the type name is correct and the assembly is loaded.");
                        yield break;  // Exit early if we can't find the type
                    }
                }
            }

            foreach (var component in components)
            {
                _afterlifeConsole($"- Found Component: {component.GetType().FullName}");

                if (componentType != null && component.GetType() == componentType)
                {
                    _afterlifeConsole($"✅ Found and using the specified component: {component.GetType().FullName}");
                    matchedComponents.Add(component);
                    foundComponent = true;
                }
                else if (componentType == null)
                {
                    matchedComponents.Add(component);
                }
            }

            if (componentType != null && !foundComponent)
            {
                MelonLogger.Warning($"❌ No component of type {componentType.FullName} found on the GameObject.");
            }

            foreach (var matchedComponent in matchedComponents)
            {
                _afterlifeConsole($"✅ Printing properties for component: {matchedComponent.GetType().FullName}");

                var properties = matchedComponent.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                bool propertyFound = false;

                foreach (var property in properties)
                {
                    try
                    {
                        if (property.CanRead)
                        {
                            var value = property.GetValue(matchedComponent);
                            _afterlifeConsole($"- {property.Name}: {value}");

                            if (propertyToUse != null && property.Name.Equals(propertyToUse, StringComparison.OrdinalIgnoreCase))
                            {
                                _afterlifeConsole($"✅ Using the property: {property.Name} with value: {value}");

                                if (value != null)
                                {
                                    _afterlifeConsole("✅ Target object found. Dumping fields, properties, and methods...");

                                    Type valueType = value.GetType();

                                    // Print Fields
                                    var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    _afterlifeConsole("🔹 Fields:");
                                    foreach (var field in fields)
                                    {
                                        try
                                        {
                                            object fieldValue = field.GetValue(value);
                                            _afterlifeConsole($"  - {field.Name}: {fieldValue}");
                                        }
                                        catch (Exception ex)
                                        {
                                            MelonLogger.Warning($"  ❌ Failed to get field {field.Name}: {ex.Message}");
                                        }
                                    }

                                    // Print Properties
                                    var nestedProperties = valueType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    _afterlifeConsole("🔹 Properties:");
                                    foreach (var prop in nestedProperties)
                                    {
                                        try
                                        {
                                            if (prop.CanRead)
                                            {
                                                object propValue = prop.GetValue(value);
                                                _afterlifeConsole($"  - {prop.Name}: {propValue}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MelonLogger.Warning($"  ❌ Failed to get property {prop.Name}: {ex.Message}");
                                        }
                                    }

                                    // Print Methods with arguments
                                    var methods = valueType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                        .Where(m => !m.IsSpecialName); // Skip property accessors

                                    _afterlifeConsole("🔹 Methods:");
                                    foreach (var method in methods)
                                    {
                                        var parameters = method.GetParameters();
                                        string paramString = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                                        _afterlifeConsole($"  - {method.Name}({paramString})");

                                        // Check if this method is the one we want to invoke
                                        if (method.Name == methodToInvoke)
                                        {
                                            _afterlifeConsole($"✅ Found method {method.Name}, invoking it...");

                                            // Prepare the arguments for the method (dynamic handling of method argument)
                                            if (parameters.Length == 1 && parameters[0].ParameterType == methodArgument?.GetType())
                                            {
                                                object[] methodArgs = new object[] { methodArgument }; // Argument passed dynamically
                                                method.Invoke(matchedComponent, methodArgs);
                                                _afterlifeConsole($"✅ Invoked method {method.Name} with arguments: {string.Join(", ", methodArgs)}");
                                            }
                                        }
                                    }
                                }
                            }

                            propertyFound = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"❌ Failed to get value for property {property.Name}: {ex.Message}");
                    }
                }

                if (!propertyFound && propertyToUse != null)
                {
                    MelonLogger.Warning($"❌ Could not find property {propertyToUse} on the component {matchedComponent.GetType().FullName}.");
                }
            }
        }//$10 Pickup, SpawnNetWorkScheduleIObjectCoroutineFromTut
        public static string GameObjectName = "";//Bungalow

        public static void TutObjectSpawner(string searchSceneName, string searchName)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                _afterlifeCoroutinesStart(
                    CloneGameObjectFromScene(
                        sceneName: searchSceneName,
                        searchName: searchName,
                        newPosition: spawnPosition + new Vector3(0, 50, 0),
                        newRotation: new Quaternion(0, 0, 0, 0),
                        setActive: true
                    )
                );
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }
        public static void ClearWantedLevel()
        {
            PlayerCrimeData componentInParent = GameObject.Find("Player_Local").GetComponentInParent<PlayerCrimeData>();
            componentInParent.CurrentPursuitLevel = 0;
            componentInParent.TimeSinceSighted = 100f;
        }

        public static void MoneyGunCheck()
        {
            if (MoneyGun)
            {
                if (Player.Local.GetInventoryString().Contains("M1911") && Input.GetMouseButton(1))
                {
                    PlayerInventory.Instance.cashInstance.ChangeBalance(999);
                }
            }
        }

        public static void SpawnAssetAtMouse(GameObject prefabToSpawn)
        {
            if (prefabToSpawn == null)
            {
                MelonLogger.Error("❌ Prefab to spawn is null.");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                // ✅ Instantiate it at mouse location
                GameObject spawned = UnityEngine.Object.Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

                // Optional: prevent destruction on scene change
                UnityEngine.Object.DontDestroyOnLoad(spawned);

                _afterlifeConsole($"✨ Spawned prefab '{prefabToSpawn.name}' at {spawnPosition}");
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }
        public static void KamikizeRocks(string searchName, int count = 10)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 targetPosition = hitInfo.point;

                for (int i = 0; i < count; i++)
                {
                    Vector3 spawnPos = new Vector3(
                        targetPosition.x + UnityEngine.Random.Range(-5f, 5f),
                        999f,
                        targetPosition.z + UnityEngine.Random.Range(-5f, 5f)
                    );

                    _afterlifeCoroutinesStart(SpawnAndDropRock(searchName, spawnPos, targetPosition));
                }
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }

        private static IEnumerator SpawnAndDropRock(string searchName, Vector3 start, Vector3 target)
        {
            yield return SpawnNetWorkScheduleIObjectCoroutineX(
                searchName: searchName,
                newPosition: start,
                newRotation: Quaternion.identity,
                setActive: true
            );

            GameObject rock = GameObject.Find(searchName); // You may want to use a better lookup for multiple objects

            if (rock != null)
            {
                Rigidbody rb = rock.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = rock.AddComponent<Rigidbody>();
                }

                // Apply force toward the target
                Vector3 direction = (target - start).normalized;
                float speed = 200f; // Adjust as needed
                rb.velocity = direction * speed;
            }
        }

        public static void SkybaseHouseSpawner(string searchName)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                _afterlifeCoroutinesStart(
                    SpawnNetWorkScheduleIObjectCoroutineX(
                        searchName: searchName,
                        newPosition: spawnPosition + new Vector3(0, 50, 0),
                        newRotation: new Quaternion(0, 0, 0, 0),
                        setActive: true
                    )
                );
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }

        public static void BouncePadSpawner(string searchName)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                _afterlifeCoroutinesStart(SpawnAndAttachTrigger(searchName, spawnPosition, Quaternion.Euler(359.674f, 343.7509f, 90.3319f)));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }
        public static IEnumerator SpawnAndAttachTrigger(string searchName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var spawnCoroutine = SpawnNetWorkScheduleIObjectCoroutineX(
                searchName: searchName,
                newPosition: spawnPosition,
                newRotation: spawnRotation,
                setActive: true
            );

            yield return spawnCoroutine;

            GameObject spawned = FindInAllScenes(searchName);

            if (spawned == null)
            {
                MelonLogger.Warning($"❌ Could not find GameObject with name '{searchName}' — creating invisible fallback.");

                spawned = new GameObject(searchName);
                spawned.transform.position = spawnPosition;
                spawned.transform.rotation = spawnRotation;
            }

            // Ensure it has a trigger collider
            if (!spawned.TryGetComponent<Collider>(out var col))
            {
                col = spawned.AddComponent<BoxCollider>();
            }
            col.isTrigger = true;

            // Add Rigidbody for trigger detection
            if (!spawned.TryGetComponent<Rigidbody>(out var rb))
            {
                rb = spawned.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }

            // Add the BouncePadTriggerProxy component in IL2CPP
            try
            {
                var il2cppType = Il2CppType.Of<BouncePadTriggerProxy>();
                var proxy = (BouncePadTriggerProxy)spawned.AddComponent(il2cppType);
                proxy.SetLaunchForce(launchForce); // Optional: Set launch force dynamically
                _afterlifeConsole("✅ BouncePadTriggerProxy added successfully.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"❌ Failed to add BouncePadTriggerProxy via Il2CppType: {ex}");
            }
        }

        public static GameObject FindInAllScenes(string name)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (var obj in rootObjects)
                {
                    var found = FindInChildrenRecursive(obj.transform, name);
                    if (found != null)
                        return found.gameObject;
                }
            }

            return null;
        }

        private static GameObject FindInChildrenRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent.gameObject;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                var result = FindInChildrenRecursive(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
        public static void MakeObjectDrivable(string searchName, GameObject searchObject)
        {
            // Check if the searchObject is null
            if (searchObject == null)
            {
                MelonLogger.Error("❌ The provided searchObject is null. Cannot proceed.");
                return;
            }

            // Get the main camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find the main camera.");
                return;
            }

            // Raycast to get the spawn position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                _afterlifeConsole($"✅ Raycast hit at position: {spawnPosition}. Spawning '{searchName}' at {spawnPosition}.");

                // Start the coroutine to spawn the object
                _afterlifeCoroutinesStart(
                    SpawnNetWorkScheduleIObjectCoroutineX(
                        searchName: searchName,
                        newPosition: spawnPosition,  // Offset to spawn slightly above the ground
                        newRotation: Quaternion.identity,  // No rotation, reset to default
                        setActive: true,
                        copyComponentsFrom: searchObject // Pass the GameObject to copy components from
                    )
                );
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }

        public static void CreateRickPortal(string ObjectName, string hintString, string ImageUrl)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;
                _afterlifeCoroutinesStart(
                    CreateNewObjectWithGUIRotateZ(
                        objectName: ObjectName,
                        objectMessage: hintString,
                        position: spawnPosition,
                        rotation: new Quaternion(0, 0, 0, 0),
                        setActive: true,
                        imageUrl: ImageUrl
                    )
                );
                //_afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(searchName + "_Clone", true));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(ObjectName, true));
        }
        public static void Create3dGui(string ObjectName, string hintString, string ImageUrl)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;
                _afterlifeCoroutinesStart(
                    CreateNewObjectWithGUI(
                        objectName: ObjectName,
                        objectMessage: hintString,
                        position: spawnPosition,
                        rotation: new Quaternion(0, 0, 0, 0),
                        setActive: true,
                        imageUrl: ImageUrl
                    )
                );
                //_afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(searchName + "_Clone", true));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(ObjectName, true));
        }
        public static void SpawnScheduleINpcWithGui(string NPCName, string hintString, string ImageUrl)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;
                _afterlifeCoroutinesStart(
                    SpawnObjectWithGUI(
                        searchName: NPCName,
                        ObjectMessage: hintString,
                        newPosition: spawnPosition,
                        newRotation: new Quaternion(0, 0, 0, 0),
                        setActive: true,
                        imageUrl: ImageUrl
                    )
                );
                //_afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(searchName + "_Clone", true));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(NPCName + "_Clone", true));
        }
        public static void SpawnScheduleINpc(string NPCName)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;

                _afterlifeCoroutinesStart(
                    SpawnNetWorkScheduleIObjectCoroutine(
                        searchName: NPCName,
                        newPosition: spawnPosition,
                        newRotation: new Quaternion(0, 0, 0, 0),
                        setActive: true
                    )
                );
                //_afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(searchName + "_Clone", true));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(NPCName + "_Clone", true));
        }
        public static void CashDrop()
        {
            if (cachedPickupSource == null)
            {
                MelonLogger.Warning("❌ Cached Pickup is not ready.");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                MelonLogger.Warning("❌ Could not find main camera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f))
            {
                Vector3 spawnPosition = hitInfo.point;
                Quaternion spawnRotation = Quaternion.LookRotation(hitInfo.normal * -1f); // Face outward from surface

                _afterlifeCoroutinesStart(SpawnNetWorkScheduleIObjectCoroutineDelay("$10 Pickup", spawnPosition, spawnRotation, true));
            }
            else
            {
                MelonLogger.Warning("❌ Mouse click didn't hit anything.");
            }
        }

        public static IEnumerator SpawnScheduleIObjectCoroutine(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive)
        {
            // Find the original GameObject by name
            GameObject original = null;

            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name))
                    continue;

                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("Clone"))
                {
                    if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name)) // Hidden or prefab-like object
                    {
                        original = go;
                        break;
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in hidden scenes.");
                yield break;
            }

            // Clone it
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = searchName + "_Clone";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' at {newPosition}, active: {clone.activeSelf}");

            // Activate it later if needed
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));

            yield break;
        }

        public static GameObject GetRoot(GameObject go)
        {
            while (go.transform.parent != null)
                go = go.transform.parent.gameObject;
            return go;
        }
        public static IEnumerator CloneGameObjectFromScene(
            string sceneName,
            string searchName,
            Vector3 newPosition,
            Quaternion newRotation,
            bool setActive,
            bool isNetworkObject = false)
        {
            // Load the scene additively if it hasn't been loaded yet
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                UnityEngine.AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!loadOp.isDone)
                    yield return null;

                _afterlifeConsole($"📥 Scene '{sceneName}' loaded additively.");
            }

            Scene targetScene = SceneManager.GetSceneByName(sceneName);

            // Ensure it's valid
            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                MelonLogger.Error($"❌ Scene '{sceneName}' failed to load properly.");
                yield break;
            }

            // Find GameObject in the loaded scene
            GameObject original = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name))
                    continue;

                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && go.scene == targetScene)
                {
                    original = GetRoot(go);
                    _afterlifeConsole($"🎯 Found '{searchName}' in scene '{sceneName}' with root '{original.name}'.");
                    break;
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject '{searchName}' in scene '{sceneName}'.");
                yield break;
            }

            // Clone the GameObject and move it to the active scene
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + "_Clone";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' at {newPosition} in active scene.");

            // Optional: Spawn it over the network
            if (isNetworkObject)
            {
                var networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
                if (networkManager == null)
                {
                    MelonLogger.Error("❌ Could not find FishNet NetworkManager.");
                    yield break;
                }

                var netObj = clone.GetComponent<Il2CppFishNet.Object.NetworkObject>();
                if (netObj == null)
                {
                    MelonLogger.Error("❌ Clone does not have a NetworkObject component.");
                    yield break;
                }

                if (networkManager.IsServer)
                {
                    networkManager.ServerManager.Spawn(clone);
                    _afterlifeConsole("📡 NetworkObject successfully spawned on the server.");
                }
                else
                {
                    MelonLogger.Warning("❌ You must be the server to spawn NetworkObjects.");
                }
            }

            // Optional delayed activation
            if (setActive)
                _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));
        }

        public static IEnumerator SpawnNetWorkScheduleIObjectCoroutineX(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive)
        {
            GameObject original = null;

            // Search for the original GameObject
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name))
                    continue;

                // Match the exact name (case-insensitive), ignore clones
                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("Clone"))
                {
                    if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name)) // Hidden scene
                    {
                        _afterlifeConsole($"Found in scene: {go.scene.name}, hideFlags: {go.hideFlags}");
                        original = GetRoot(go); // Get root for full hierarchy
                        break;
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in hidden scenes.");
                yield break;
            }

            // Clone the object
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + "_Clone";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' with full hierarchy at {newPosition}");

            SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());

            // Find the NetworkManager
            var networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
            if (networkManager == null)
            {
                MelonLogger.Error("❌ Could not find FishNet NetworkManager.");
                yield break;
            }

            // Ensure NetworkObject is attached to the cloned object
            var netObj = clone.GetComponent<Il2CppFishNet.Object.NetworkObject>();
            if (netObj == null)
            {
                MelonLogger.Error("❌ Clone does not have a NetworkObject component (check if root was cloned).");
                yield break;
            }

            // ✅ No need to manually create a NetworkBehaviour or check IsClientInitialized

            if (networkManager.IsServer)
            {
                networkManager.ServerManager.Spawn(clone);

                _afterlifeConsole("📡 NetworkObject successfully spawned on the server.");
            }
            else
            {
                MelonLogger.Warning("❌ You must be the server to spawn NetworkObjects.");
            }

            // Ensure object is activated if requested
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));
        }
        public static IEnumerator SpawnNetWorkScheduleIObjectCoroutineX(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive, GameObject copyComponentsFrom = null)
        {
            GameObject original = null;

            // --- SEARCH ALL SCENES ---
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;

                foreach (GameObject rootObject in scene.GetRootGameObjects())
                {
                    Transform foundTransform = FindChildRecursive(rootObject.transform, searchName);
                    if (foundTransform != null)
                    {
                        _afterlifeConsole($"✅ Found '{searchName}' in scene '{scene.name}'.");
                        original = GetRootX(foundTransform.gameObject);
                        break;
                    }
                }

                if (original != null)
                    break;
            }

            // --- ALSO SEARCH HIDDEN OBJECTS ---
            if (original == null)
            {
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go == null || string.IsNullOrEmpty(go.name))
                        continue;

                    if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("Clone"))
                    {
                        if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name))
                        {
                            _afterlifeConsole($"✅ Found hidden object: '{go.name}' in scene: '{go.scene.name}'");
                            original = GetRoot(go);
                            break;
                        }
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in any loaded scenes or hidden scenes.");
                yield break;
            }

            // Instantiate the original GameObject
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + "_Clone";

            // Temporarily deactivate the clone before updating its position
            clone.SetActive(false);

            // Update the clone's position and rotation
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;

            // --- MOVE THE COPY COMPONENTS OBJECT AS WELL ---
            if (copyComponentsFrom != null)
            {
                _afterlifeConsole("✅ Copy source provided. Moving and parenting...");

                if (copyComponentsFrom is GameObject validCopySource)
                {
                    validCopySource.transform.position = newPosition;
                    validCopySource.transform.rotation = newRotation;

                    validCopySource.transform.SetParent(clone.transform, true);
                    _afterlifeConsole($"✅ GameObject '{validCopySource.name}' moved and parented under '{clone.name}'!");
                }
                else if (copyComponentsFrom is Il2CppSystem.Object il2cppSource)
                {
                    Transform il2cppTransform = il2cppSource.TryCast<Transform>();
                    if (il2cppTransform != null)
                    {
                        il2cppTransform.position = newPosition;
                        il2cppTransform.rotation = newRotation;

                        il2cppTransform.SetParent(clone.transform, true);
                        _afterlifeConsole($"✅ IL2CPP Transform '{il2cppTransform.name}' moved and parented under '{clone.name}'!");
                    }
                    else
                    {
                        MelonLogger.Error("❌ Il2CppSystem.Object is not a Transform or cannot be cast.");
                    }
                }
                else
                {
                    MelonLogger.Error("❌ Provided object is not a valid GameObject or Il2CppSystem.Object.");
                }
            }
            else
            {
                MelonLogger.Warning("❌ No copy source provided. Skipping parenting.");
            }

            // Reactivate the clone after all setup
            clone.SetActive(setActive);
        }

        // Helper to recursively search children
        private static Transform FindChildRecursive(Transform parent, string searchName)
        {
            if (parent.name.Equals(searchName, StringComparison.OrdinalIgnoreCase))
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), searchName);
                if (found != null)
                    return found;
            }
            return null;
        }

        // Helper to find the root GameObject
        private static GameObject GetRootX(GameObject obj)
        {
            Transform current = obj.transform;
            while (current.parent != null)
            {
                current = current.parent;
            }
            return current.gameObject;
        }

        public static IEnumerator SpawnNetWorkScheduleIObjectCoroutine(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive)
        {
            GameObject original = null;

            // Search for the original GameObject
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name))
                    continue;

                // Match the exact name (case-insensitive), ignore clones
                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("Clone"))
                {
                    if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name))
                    {
                        _afterlifeConsole($"Found in scene: {go.scene.name}, hideFlags: {go.hideFlags}");
                        original = GetRoot(go);
                        break;
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in hidden scenes.");
                yield break;
            }

            // Clone the object
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + "_Clone";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' with full hierarchy at {newPosition}");

            SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());

            // Find the NetworkManager
            var networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
            if (networkManager == null)
            {
                MelonLogger.Error("❌ Could not find FishNet NetworkManager.");
                yield break;
            }

            // Ensure NetworkObject is attached to the cloned object
            var netObj = clone.GetComponent<Il2CppFishNet.Object.NetworkObject>();
            if (netObj == null)
            {
                MelonLogger.Error("❌ Clone does not have a NetworkObject component (check if root was cloned).");
                yield break;
            }

            // Try to get an existing NetworkBehaviour on the clone
            var netBehaviour = clone.GetComponent<Il2CppFishNet.Object.NetworkBehaviour>();
            if (netBehaviour == null || !netBehaviour.IsClientInitialized)
            {
                MelonLogger.Warning("❌ NetworkBehaviour is missing or client is not initialized. The object may not be initialized or may be deinitialized.");
                yield break;
            }

            // If we are the server, spawn the object
            if (networkManager.IsServer)
            {
                networkManager.ServerManager.Spawn(clone);
                _afterlifeConsole("📡 NetworkObject successfully spawned on the server.");
            }
            else
            {
                MelonLogger.Warning("❌ You must be the server to spawn NetworkObjects.");
            }

            // Make sure the object is active
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));
        }

        public static void PrintHierarchy(GameObject gameObject)
        {
            Transform currentTransform = gameObject.transform;

            // Create a list to store the hierarchy chain
            List<string> hierarchy = new List<string>();

            // Traverse up the hierarchy chain and store each name
            while (currentTransform != null)
            {
                hierarchy.Insert(0, currentTransform.gameObject.name); // Insert at the beginning to maintain the top-down order
                currentTransform = currentTransform.parent;
            }

            // Log the hierarchy
            _afterlifeConsole($"[Hierarchy] Object Hierarchy for {gameObject.name}: {string.Join(" -> ", hierarchy)}");
        }

        public static void SpawnVehicle(string vehicleCode)
        {
            _afterlifeCoroutinesStart(WaitForVehicleManager(vehicleCode));
        }

        public static IEnumerator WaitForVehicleManager(string vehicleCode)//example: hounddog
        {
            GameObject player = null;

            // Wait for the local player GameObject
            while ((player = Player.Local?.LocalGameObject) == null)
                yield return null;

            // Wait for VehicleManager (you might need to adapt this depending on where it's stored)
            Il2CppScheduleOne.Vehicles.VehicleManager vehicleManager = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Vehicles.VehicleManager>();
            while (vehicleManager == null)
            {
                yield return null;
                vehicleManager = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Vehicles.VehicleManager>();
            }

            // Define spawn position and rotation
            Vector3 spawnPosition = player.transform.position + player.transform.forward * 5f; // 5 meters in front
            Quaternion spawnRotation = Quaternion.identity;
            bool playerOwned = true;

            // Spawn the vehicle
            vehicleManager.SpawnVehicle(vehicleCode, spawnPosition, spawnRotation, playerOwned);

            _afterlifeConsole($"✅ Spawned vehicle: {vehicleCode} at {spawnPosition}");
        }
        //watering_can
        public static void GiveItemToPlayer(string itemCode, int quantity)
        {
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();
            args.Add(itemCode);

            if (quantity > 1)
                args.Add(quantity.ToString());

            _afterlifeCoroutinesStart(WaitAndGiveItem(args));
        }
        public static bool ValidatePlayer(Player player, string actionName)
        {
            if (player == null)
            {
                MelonLogger.Error($"Cannot perform {actionName} on a null player");
                return false;
            }
            return true;
        }
        public static void TeleportToPlayer(Player player)
        {
            if (!ValidatePlayer(player, "teleport"))
                return;

            // Directly assign since Vector3 is a value type.
            Player.Local.transform.position = player.transform.position;
        }

        public static void ToggleFollowPlayer(Player childPlayer, Player parentPlayer)
        {
            if (_isFollowing)
            {
                // Stop following
                _isFollowing = false;
                _followTokenSource?.Cancel();
                _followTokenSource = null;

                if (childPlayer != null)
                    LockPlayerMovement(childPlayer, false);

                _afterlifeConsole($"{childPlayer?.name ?? "Unknown"} stopped following.");
                return;
            }

            // Start following
            if (!ValidatePlayer(childPlayer, "follow (child)") || !ValidatePlayer(parentPlayer, "follow (target)"))
                return;

            _isFollowing = true;
            _followTokenSource = new CancellationTokenSource();

            LockPlayerMovement(childPlayer, true);
            FollowPlayerLoop(childPlayer, parentPlayer, _followTokenSource.Token);
            _afterlifeConsole($"{childPlayer.name} started following {parentPlayer.name}.");
        }

        private static async void FollowPlayerLoop(Player child, Player parent, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!ValidatePlayer(child, "follow (child)") || !ValidatePlayer(parent, "follow (target)"))
                        break;

                    Vector3 targetPosition = parent.transform.position + parent.transform.forward * 5f;
                    child.transform.position = Vector3.Lerp(child.transform.position, targetPosition, 0.15f);

                    await Task.Delay(20, token);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Follow loop crashed: {ex.Message}");
            }
            finally
            {
                if (child != null)
                    LockPlayerMovement(child, false);
                _isFollowing = false;
            }
        }

        private static void LockPlayerMovement(Player player, bool lockMovement)
        {
            if (player == null)
                return;

            var rigidbody = player.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.isKinematic = lockMovement;
            }

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = !lockMovement;
            }

            var movementScript = player.GetComponent<PlayerMovement>(); // Replace if different
            if (movementScript != null)
            {
                movementScript.enabled = !lockMovement;
            }
        }

        public static IEnumerator WaitAndGiveItem(Il2CppSystem.Collections.Generic.List<string> args)
        {
            // Wait for player inventory to be ready
            while (PlayerSingleton<PlayerInventory>.Instance == null)
                yield return null;

            if (args.Count <= 0)
            {
                MelonLogger.Warning("Unrecognized command format. Example: 'give watering_can', 'give watering_can 5'");
                yield break;
            }

            var item = Il2CppScheduleOne.Registry.GetItem(args[0]);
            if (item == null || args[0] == "cash")
            {
                MelonLogger.Warning($"Unrecognized item code '{args[0]}'");
                yield break;
            }

            var defaultInstance = item.GetDefaultInstance(1);

            if (!PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance, 1))
            {
                MelonLogger.Warning("Insufficient inventory space");
                yield break;
            }

            int amountToAdd = 1;
            if (args.Count > 1)
            {
                if (!int.TryParse(args[1], out amountToAdd) || amountToAdd <= 0)
                {
                    MelonLogger.Warning($"Unrecognized quantity '{args[1]}'. Please provide a positive integer.");
                    yield break;
                }
            }

            int addedCount = 0;
            while (amountToAdd > 0 && PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance, 1))
            {
                PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(defaultInstance);
                amountToAdd--;
                addedCount++;
            }

            _afterlifeConsole($"🧺 Added {addedCount} × {item.Name} to inventory.");
        }

        [HarmonyPatch(typeof(Il2CppScheduleOne.Equipping.Equippable_RangedWeapon))]
        public static class UnlimitedAmmoPatch
        {
            public static bool PatchEnabled { get; private set; } = false;
            public static float DefaultRange = 200f;
            public static float DefaultAccuracy = 1f;
            public static float DefaultMaxSpread = 0f;
            public static float DefaultMinSpread = 0f;
            public static void EnablePatch() => PatchEnabled = true;

            public static void DisablePatch() => PatchEnabled = false;
            public static void ToggleUnlimitedAmmo()
            {
                PatchEnabled = !PatchEnabled;
                _afterlifeConsole($"[UnlimitedAmmoPatch] Toggled: {PatchEnabled}");
            }
            // Allow the user to set the Range dynamically
            public static void SetWeaponRange(float range)
            {
                DefaultRange = range;
                _afterlifeConsole($"[UnlimitedAmmoPatch] Weapon range set to: {DefaultRange}");
            }

            // Allow the user to set the Accuracy dynamically
            public static void SetWeaponAccuracy(float accuracy)
            {
                DefaultAccuracy = accuracy;
                _afterlifeConsole($"[UnlimitedAmmoPatch] Weapon accuracy set to: {DefaultAccuracy}");
            }

            // Allow the user to set MaxSpread dynamically
            public static void SetWeaponMaxSpread(float maxSpread)
            {
                DefaultMaxSpread = maxSpread;
                _afterlifeConsole($"[UnlimitedAmmoPatch] Weapon max spread set to: {DefaultMaxSpread}");
            }

            // Allow the user to set MinSpread dynamically
            public static void SetWeaponMinSpread(float minSpread)
            {
                DefaultMinSpread = minSpread;
                _afterlifeConsole($"[UnlimitedAmmoPatch] Weapon min spread set to: {DefaultMinSpread}");
            }
            [HarmonyPrefix]
            [HarmonyPatch("Fire")]
            public static void Fire_Prefix(Il2CppScheduleOne.Equipping.Equippable_RangedWeapon __instance)
            {
                if (!PatchEnabled || __instance == null || __instance.weaponItem == null)
                    return;

                __instance.Range = DefaultRange;
                __instance.Accuracy = DefaultAccuracy;
                __instance.MaxSpread = DefaultMaxSpread;
                __instance.MinSpread = DefaultMinSpread;
                __instance.weaponItem.Value = __instance.MagazineSize;

                _afterlifeConsole("[UnlimitedAmmoPatch] Applied unlimited ammo values");
            }
        }

        public static void PrintCoords()
        {
            _afterlifeCoroutinesStart(WaitForPlayerMovement((playerMovement) =>
            {
                Vector3 position = playerMovement.transform.position;
                _afterlifeConsole($"📍 Player Position: {position}");
            }));
        }
        public static void TeleportLocalPlayer(Vector3 targetPosition)
        {
            _afterlifeCoroutinesStart(SetLocalPlayerPosition(targetPosition));
        }

        private static IEnumerator SetLocalPlayerPosition(Vector3 targetPosition)
        {
            GameObject localPlayer;

            while ((localPlayer = Player.Local?.LocalGameObject) == null)
            {
                yield return null;
            }

            Transform parent = localPlayer.transform.parent;
            if (parent != null)
            {
                parent.position = targetPosition;
                _afterlifeConsole($"🚀 Teleported Parent GameObject to: {targetPosition}");
            }
            else
            {
                // fallback: move local player if no parent exists
                localPlayer.transform.position = targetPosition;
                _afterlifeConsole($"🚀 Teleported LocalPlayer directly to: {targetPosition}");
            }
        }
        public static IEnumerator SpawnNetWorkScheduleIObjectCoroutineDelay(string searchName, Vector3 newPosition, Quaternion newRotation, bool setActive)
        {
            GameObject original = null;

            // Search for the original GameObject
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name))
                    continue;

                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("Clone"))
                {
                    if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name))
                    {
                        _afterlifeConsole($"Found in scene: {go.scene.name}, hideFlags: {go.hideFlags}");
                        original = GetRoot(go);
                        break;
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in hidden scenes.");
                yield break;
            }

            // Clone the object
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + "_Clone";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' with full hierarchy at {newPosition}");

            // Move clone to active scene
            SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());

            // Get the NetworkManager
            var networkManager = UnityEngine.Object.FindObjectOfType<Il2CppFishNet.Managing.NetworkManager>();
            if (networkManager == null)
            {
                MelonLogger.Error("❌ Could not find FishNet NetworkManager.");
                yield break;
            }

            // Ensure it has a NetworkObject
            var netObj = clone.GetComponent<Il2CppFishNet.Object.NetworkObject>();
            if (netObj == null)
            {
                MelonLogger.Error("❌ Clone does not have a NetworkObject component.");
                yield break;
            }

            // Try to get any NetworkBehaviour (you can replace with a specific one)
            var netBehaviour = clone.GetComponent<Il2CppFishNet.Object.NetworkBehaviour>();
            if (netBehaviour == null)
            {
                MelonLogger.Warning("⚠️ No NetworkBehaviour found on the clone. You may need to attach a custom one.");
            }

            // Only spawn if server
            if (networkManager.IsServer)
            {
                networkManager.ServerManager.Spawn(clone);
                _afterlifeConsole("📡 NetworkObject successfully spawned on the server.");

                // Wait several frames for initialization
                bool initialized = false;
                for (int i = 0; i < 10; i++)
                {
                    yield return null;

                    netBehaviour = clone.GetComponent<Il2CppFishNet.Object.NetworkBehaviour>();
                    if (netBehaviour != null && netBehaviour.IsClientInitialized)
                    {
                        _afterlifeConsole("✅ NetworkBehaviour is now initialized.");
                        initialized = true;
                        break;
                    }
                }

                if (!initialized)
                {
                    MelonLogger.Warning("⚠️ NetworkBehaviour is still not initialized after spawn.");
                }
            }
            else
            {
                MelonLogger.Warning("❌ You must be the server to spawn NetworkObjects.");
            }

            // Final activation
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));
        }
        public static IEnumerator CreateNewObjectWithGUIRotateZ(string objectName, string objectMessage, Vector3 position, Quaternion rotation, bool setActive, string imageUrl)
        {
            int indexAmount = 0;

            // Create a new empty GameObject
            GameObject newObject = new GameObject($"{objectName}_New_{indexAmount}");
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.SetActive(setActive);

            _afterlifeConsole($"✅ [New] Created new GameObject '{newObject.name}' at {position}");

            // Create the GUI Canvas
            GameObject canvasGO = new GameObject($"AttachedCanvas_{indexAmount}");
            canvasGO.transform.SetParent(newObject.transform);
            canvasGO.transform.localPosition = new Vector3(0, 2.09f, 0);
            canvasGO.transform.localRotation = Quaternion.identity;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 150);
            canvasRect.localScale = new Vector3(0.01f, 0.02f, 0.01f);

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 10;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create the Panel
            GameObject panelGO = new GameObject($"Panel_{indexAmount}");
            panelGO.transform.SetParent(canvasGO.transform, false);

            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300, 150);
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0f); // Transparent initially
            panelImage.preserveAspect = true;

            // Load background image
            _afterlifeCoroutinesStart(LoadBackgroundImage(imageUrl, panelImage));

            if (!string.IsNullOrWhiteSpace(objectMessage))
            {
                // Create the Text object
                GameObject textGO = new GameObject($"Text_{indexAmount}");
                textGO.transform.SetParent(panelGO.transform, false);

                RectTransform textRect = textGO.AddComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(280, 130);

                Text text = textGO.AddComponent<Text>();
                text.text = objectMessage;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(1, 1, 1, 0f); // Start invisible
                text.fontSize = 12;

                // Optional: resize panel based on text height
                float textHeight = text.preferredHeight;
                panelRect.sizeDelta = new Vector2(300, textHeight + 2);

                // Fade and proximity behavior
                _afterlifeCoroutinesStart(FadeGUIOnProximity(panelImage, text, canvasGO.transform, 10f));
            }
            else
            {
                // Fade background only (no text)
                _afterlifeCoroutinesStart(FadeGUIOnProximity(panelImage, null, canvasGO.transform, 10f));
            }

            // Optional activation scheduler
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(newObject.name, true));

            // Start spinning the panel image on Z-axis
            _afterlifeCoroutinesStart(RotateImageZ(panelGO.transform));

            yield break;
        }

        private static IEnumerator RotateImageZ(Transform target, float speed = 80f)
        {
            while (true)
            {
                target.Rotate(0, 0, speed * Time.deltaTime);
                yield return null;
            }
        }

        public static IEnumerator CreateNewObjectWithGUI(string objectName, string objectMessage, Vector3 position, Quaternion rotation, bool setActive, string imageUrl)
        {
            int indexAmount = 0;

            // Create a new empty GameObject
            GameObject newObject = new GameObject($"{objectName}_New_{indexAmount}");
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.SetActive(setActive);

            _afterlifeConsole($"✅ [New] Created new GameObject '{newObject.name}' at {position}");

            // Create the GUI Canvas
            GameObject canvasGO = new GameObject($"AttachedCanvas_{indexAmount}");
            canvasGO.transform.SetParent(newObject.transform);
            canvasGO.transform.localPosition = new Vector3(0, 2.09f, 0);
            canvasGO.transform.localRotation = Quaternion.identity;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 150);
            canvasRect.localScale = new Vector3(0.01f, 0.02f, 0.01f);

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 10;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create the Panel
            GameObject panelGO = new GameObject($"Panel_{indexAmount}");
            panelGO.transform.SetParent(canvasGO.transform, false);

            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300, 150);
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0f); // Transparent initially
            panelImage.preserveAspect = true;

            // Load background image
            _afterlifeCoroutinesStart(LoadBackgroundImage(imageUrl, panelImage));

            if (!string.IsNullOrWhiteSpace(objectMessage))
            {
                // Create the Text object
                GameObject textGO = new GameObject($"Text_{indexAmount}");
                textGO.transform.SetParent(panelGO.transform, false);

                RectTransform textRect = textGO.AddComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(280, 130);

                Text text = textGO.AddComponent<Text>();
                text.text = objectMessage;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(1, 1, 1, 0f); // Start invisible
                text.fontSize = 12;

                // Optional: resize panel based on text height
                float textHeight = text.preferredHeight;
                panelRect.sizeDelta = new Vector2(300, textHeight + 2);

                // Fade and proximity behavior
                _afterlifeCoroutinesStart(FadeGUIOnProximity(panelImage, text, canvasGO.transform, 10f));
            }
            else
            {
                // Fade background only (no text)
                _afterlifeCoroutinesStart(FadeGUIOnProximity(panelImage, null, canvasGO.transform, 10f));
            }

            // Optional activation scheduler
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(newObject.name, true));

            yield break;
        }

        public static IEnumerator SpawnObjectWithGUI(string searchName, string ObjectMessage, Vector3 newPosition, Quaternion newRotation, bool setActive, string imageUrl)
        {
            int IndexAmount = 0;

            // Find the original GameObject
            GameObject original = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go == null || string.IsNullOrEmpty(go.name)) continue;
                if (go.name.Equals(searchName, StringComparison.OrdinalIgnoreCase) && !go.name.Contains("_Clone_" + IndexAmount))
                {
                    if (go.hideFlags == HideFlags.HideAndDontSave || string.IsNullOrEmpty(go.scene.name))
                    {
                        original = go;
                        break;
                    }
                }
            }

            if (original == null)
            {
                MelonLogger.Error($"❌ Could not find GameObject with name '{searchName}' in hidden scenes.");
                yield break;
            }

            // Clone the object
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = $"{searchName}_Clone_{IndexAmount}";
            clone.transform.position = newPosition;
            clone.transform.rotation = newRotation;
            clone.SetActive(setActive);

            _afterlifeConsole($"✅ [Clone] Spawned '{clone.name}' at {newPosition}");

            // Create the GUI Canvas
            GameObject canvasGO = new GameObject($"AttachedCanvas_{IndexAmount}");
            canvasGO.transform.SetParent(clone.transform);
            canvasGO.transform.localPosition = new Vector3(0, 2.09f, 0);
            canvasGO.transform.localRotation = Quaternion.identity;

            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(300, 150);
            canvasRect.localScale = new Vector3(0.01f, 0.02f, 0.01f);

            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 10;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create the Panel
            GameObject panelGO = new GameObject($"Panel_{IndexAmount}");
            panelGO.transform.SetParent(canvasGO.transform, false);

            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300, 150);
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0f); // Transparent initially

            // Load background image
            _afterlifeCoroutinesStart(LoadBackgroundImage(imageUrl, panelImage));

            // Create the Text object
            GameObject textGO = new GameObject($"Text_{IndexAmount}");
            textGO.transform.SetParent(panelGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(280, 130);

            Text text = textGO.AddComponent<Text>();
            text.text = ObjectMessage;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1, 1, 1, 0f); // Start invisible
            text.fontSize = 12;

            float textHeight = text.preferredHeight;
            panelRect.sizeDelta = new Vector2(300, textHeight + 2);

            // Animate GUI behavior
            _afterlifeCoroutinesStart(FadeGUIOnProximity(panelImage, text, canvasGO.transform, 10f));
            _afterlifeCoroutinesStart(RotateCanvasToFacePlayer(canvasGO.transform));
            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));

            yield break;
        }

        private static IEnumerator LoadBackgroundImage(string imageUrl, Image panelImage)
        {
            UnityWebRequest uwr = UnityWebRequest.Get(imageUrl);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                byte[] imageData = uwr.downloadHandler.data;

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(imageData))
                {
                    panelImage.sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    panelImage.color = new Color(1f, 1f, 1f, 1f); // fully visible
                }
                else
                {
                    MelonLogger.Warning("❌ Failed to load image into Texture2D.");
                }
            }
            else
            {
                MelonLogger.Warning($"❌ Failed to download image from {imageUrl} — Error: {uwr.error}");
            }

            uwr.Dispose();
        }
        private static IEnumerator RotateCanvasToFacePlayer(Transform canvasTransform)
        {
            GameObject player = Player.Local.LocalGameObject;
            if (player == null)
            {
                MelonLogger.Warning("⚠️ Player not found for canvas rotation.");
                yield break;
            }

            while (true)
            {
                Vector3 direction = canvasTransform.position - player.transform.position;
                direction.y = 0; // Optional: only rotate horizontally
                if (direction != Vector3.zero)
                    canvasTransform.rotation = Quaternion.LookRotation(direction);
                yield return null;
            }
        }

        private static IEnumerator FadeGUIOnProximity(Image panel, Text text, Transform targetTransform, float threshold)
        {
            GameObject player = Player.Local.LocalGameObject;

            if (player == null)
            {
                MelonLogger.Warning("⚠️ Player not found for proximity check.");
                yield break;
            }

            float fadeSpeed = 2f;

            while (true)
            {
                float dist = Vector3.Distance(player.transform.position, targetTransform.position);
                float targetAlpha = dist <= threshold ? 1f : 0f;

                Color panelColor = panel.color;
                Color textColor = text.color;

                float alphaDiff = Mathf.Abs(panelColor.a - targetAlpha);
                while (alphaDiff > 0.01f)
                {
                    float newAlpha = Mathf.MoveTowards(panelColor.a, targetAlpha, Time.deltaTime * fadeSpeed);
                    panel.color = new Color(panelColor.r, panelColor.g, panelColor.b, newAlpha);
                    text.color = new Color(textColor.r, textColor.g, textColor.b, newAlpha);

                    yield return null;

                    panelColor = panel.color;
                    textColor = text.color;
                    alphaDiff = Mathf.Abs(panelColor.a - targetAlpha);
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
        public static void ToggleSchedulIUIByName(string ui)
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var go = allObjects.FirstOrDefault(g => g.name == ui && g.hideFlags == HideFlags.None);

            if (go == null)
            {
                MelonLogger.Warning($"GameObject '{ui}' not found.");
                return;
            }

            var container = go.transform.Find("Container");
            var canvas = go.GetComponent<Canvas>();

            bool goActive = go.activeSelf;
            bool containerActive = container != null && container.gameObject.activeSelf;
            bool canvasEnabled = canvas != null && canvas.enabled;

            // If there's any mismatch, normalize all to true
            if (!(goActive && containerActive && canvasEnabled))
            {
                if (!goActive) go.SetActive(true);
                if (container != null && !container.gameObject.activeSelf) container.gameObject.SetActive(true);
                if (canvas != null && !canvas.enabled) canvas.enabled = true;

                _afterlifeConsole($"Normalized '{ui}' - GameObject, Container, and Canvas are now all enabled.");
                return;
            }

            // Otherwise, toggle all together
            bool newState = false;

            go.SetActive(newState);
            if (container != null) container.gameObject.SetActive(newState);
            if (canvas != null) canvas.enabled = newState;

            _afterlifeConsole($"Toggled '{ui}' - GameObject, Container, and Canvas are now {(newState ? "enabled" : "disabled")}.");
        }
        public static void BlackHoleMode()
        {
            if (Camera.main == null) return;

            BlackHoleEnabled = !BlackHoleEnabled;
            Camera.main.farClipPlane = BlackHoleEnabled ? 10f : 1000f;
        }

        public static void SetCameraMod(string colorString)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                MelonLogger.Error("No main camera found.");
                return;
            }

            if (colorString.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                // Reset all mods to default settings
                camera.clearFlags = CameraClearFlags.Skybox;
                camera.orthographic = false;
                camera.orthographicSize = 5f;
                camera.nearClipPlane = 0.02f;

                // Reset post-processing effects
                var volume = camera.gameObject.GetComponent<PostProcessVolume>();
                if (volume != null && volume.sharedProfile != null)
                {
                    var profile = volume.sharedProfile;
                    if (profile.TryGetSettings(out ColorGrading colorGrading))
                    {
                        profile.RemoveSettings<ColorGrading>();
                    }
                }
                // Reset meth head mode
                var playerCamera = GameObject.FindObjectOfType<PlayerCamera>();
                if (playerCamera != null)
                {
                    playerCamera._CocaineVisuals_k__BackingField = false;
                    _afterlifeConsole("Meth Head Mode reset: Cocaine visuals disabled.");
                }

                _afterlifeConsole("Camera and effects reset to default.");
                return;
            }

            if (colorString.Equals("wallhack", StringComparison.OrdinalIgnoreCase))
            {
                if (Mathf.Approximately(camera.nearClipPlane, 0.02f))
                {
                    camera.nearClipPlane = 0.5f;
                    _afterlifeConsole("Wallhack ON: nearClipPlane set to 0.5");
                }
                else
                {
                    camera.nearClipPlane = 0.02f;
                    _afterlifeConsole("Wallhack OFF: nearClipPlane reset to 0.02");
                }
                return;
            }

            if (colorString.Equals("rts", StringComparison.OrdinalIgnoreCase))
            {
                isRtsModeEnabled = !isRtsModeEnabled;

                if (isRtsModeEnabled)
                {
                    camera.orthographic = true;
                    camera.orthographicSize = 5f;

                    // Optional: adjust position/rotation to top-down view
                    camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                    // Raise camera height to prevent clipping
                    Vector3 pos = camera.transform.position;
                    pos.y = Mathf.Max(pos.y, 50f); // Adjust as needed for your terrain height
                    camera.transform.position = pos;

                    _afterlifeConsole("RTS mode enabled: orthographic view set.");
                }
                else
                {
                    camera.orthographic = false;
                    camera.orthographicSize = 0f;

                    // Reset position/rotation if needed
                    camera.transform.rotation = Quaternion.identity;

                    _afterlifeConsole("RTS mode disabled: perspective view restored.");
                }

                return;
            }

            if (colorString.Equals("old times", StringComparison.OrdinalIgnoreCase))
            {
                var layer = camera.gameObject.GetComponent<PostProcessLayer>();
                if (layer == null)
                {
                    layer = camera.gameObject.AddComponent<PostProcessLayer>();
                    layer.volumeLayer = LayerMask.GetMask("PostProcessing");
                    layer.Init(Resources.Load<PostProcessResources>("PostProcessResources"));
                }

                var volume = camera.gameObject.GetComponent<PostProcessVolume>();
                if (volume == null)
                {
                    volume = camera.gameObject.AddComponent<PostProcessVolume>();
                    volume.isGlobal = true;
                    volume.priority = 10;
                    volume.weight = 1;
                    volume.sharedProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
                }

                var profile = volume.sharedProfile;

                // Get or add color grading settings
                ColorGrading colorGrading;
                if (!profile.TryGetSettings(out colorGrading))
                {
                    colorGrading = profile.AddSettings<ColorGrading>();
                }

                // Apply full grayscale (black and white effect)
                colorGrading.saturation.value = -100f; // Fully desaturate colors to grayscale
                colorGrading.contrast.value = 50f; // Increase contrast for an old film look
                colorGrading.gamma.value = new Vector4(1.2f, 1.2f, 1.2f, 0f); // Adjust gamma for a vintage feel

                // Optional: Add some grain to simulate old film texture
                var grain = profile.GetSetting<Grain>();
                if (grain == null)
                {
                    grain = profile.AddSettings<Grain>();
                }
                grain.intensity.value = 0.3f; // Add some film grain

                _afterlifeConsole("Old Times Mode enabled: black & white effect with vintage film look.");
                return;
            }
            if (colorString.Equals("meth head", StringComparison.OrdinalIgnoreCase))
            {
                var playerCamera = GameObject.FindObjectOfType<PlayerCamera>();
                if (playerCamera == null)
                {
                    MelonLogger.Error("PlayerCamera not found.");
                    return;
                }

                // Toggle the cocaine visuals
                playerCamera._CocaineVisuals_k__BackingField = !playerCamera._CocaineVisuals_k__BackingField;

                string state = playerCamera._CocaineVisuals_k__BackingField ? "ON" : "OFF";
                _afterlifeConsole($"Meth Head Mode {state}: Cocaine visuals toggled.");
                return;
            }

            Color color;

            // Try parsing from hex or named color (e.g., "#FF0000", "red")
            if (ColorUtility.TryParseHtmlString(colorString, out color))
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = color;
                _afterlifeConsole($"Camera background color set to {colorString}.");
            }
            else
            {
                MelonLogger.Error($"Invalid color: '{colorString}'. Use a color name, hex (e.g., '#FF0000'), 'wallmod', 'rts mode', or 'reset'.");
            }
        }

        public static void ToggleDiscoSkyMode()
        {
            if (!discoSkyEnabled)
            {
                discoSkyEnabled = true;
                discoSkyCoroutine = _afterlifeCoroutinesStart(DiscoSkyLoop());
                _afterlifeConsole("🌈 DiscoSkyMode ENABLED.");
            }
            else
            {
                discoSkyEnabled = false;
                if (discoSkyCoroutine != null)
                    MelonCoroutines.Stop(discoSkyCoroutine);

                Camera.main.clearFlags = CameraClearFlags.Skybox; // Or SolidColor with default
                Camera.main.backgroundColor = Color.black; // Reset
                _afterlifeConsole("❌ DiscoSkyMode DISABLED.");
            }
        }

        public static IEnumerator DiscoSkyLoop()
        {
            var camera = Camera.main;
            camera.clearFlags = CameraClearFlags.SolidColor;

            while (discoSkyEnabled)
            {
                camera.backgroundColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                yield return new WaitForSeconds(0.2f);
            }
        }

        public static void SetTextureFromUrl(Renderer renderer, string imageUrl)
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null.");
                return;
            }

            _afterlifeCoroutinesStart(DownloadAndApplyTexture(renderer, imageUrl));
        }

        private static IEnumerator DownloadAndApplyTexture(Renderer renderer, string url)
        {
            _afterlifeConsole($"Downloading texture from: {url}");  // Log the URL

            // Send a request to download the texture as bytes
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            uwr.downloadHandler = new DownloadHandlerBuffer();  // We use a buffer to download the texture as bytes

            yield return uwr.SendWebRequest();  // Wait for the request to finish

            // Check if the request was successful
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                MelonLogger.Error("Texture download failed: " + uwr.error);
            }
            else
            {
                _afterlifeConsole("Texture downloaded successfully!");

                // Create a new Texture2D object
                byte[] textureData = uwr.downloadHandler.data;  // Get the downloaded data
                Texture2D texture = new Texture2D(2, 2);  // Create a new texture (size doesn't matter here)

                // Load the texture from the downloaded bytes
                if (texture.LoadImage(textureData))
                {
                    // Apply the texture to the renderer
                    _afterlifeConsole("Texture loaded successfully!");
                    ApplyTexture(renderer, texture);
                }
                else
                {
                    MelonLogger.Error("Failed to load texture from downloaded data.");
                }
            }

            // Dispose of the web request to free up resources
            uwr.Dispose();
        }

        private static void ApplyTexture(Renderer renderer, Texture2D texture)
        {
            if (renderer != null && texture != null)
            {
                // Create a new material instance (if you want to replace the material)
                Material newMaterial = new Material(Shader.Find("Standard")); // or use another shader if needed
                newMaterial.SetTexture("_MainTex", texture); // Apply the texture to the material

                renderer.material = newMaterial; // Set the new material to the renderer
                Debug.Log("Texture applied to renderer: " + renderer.name);
            }
            else
            {
                MelonLogger.Error("Renderer or texture is null. Cannot apply texture.");
            }
        }

        public static void testTexture()
        {
            GameObject myObject = GameObject.Find("Body_LOD0"); // Or reference directly
            if (myObject != null)
            {
                Renderer renderer = myObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    SetTextureFromUrl(renderer, "https://iili.io/3a5yV9e.png");
                }
                else
                {
                    MelonLogger.Error("No Renderer found on GameObject.");
                }
            }
            else
            {
                MelonLogger.Error("GameObject not found.");
            }
        }

        public static void ToggleGodMode()
        {
            GodModeEnabled = !GodModeEnabled;

            if (GodModeEnabled)
            {
                _afterlifeConsole("✅ GodMode Enabled");
                StartGodModeLoop();
            }
            else
            {
                _afterlifeConsole("❌ GodMode Disabled");
                godModeTokenSource?.Cancel();
            }
        }

        private static void StartGodModeLoop()
        {
            godModeTokenSource = new CancellationTokenSource();
            var token = godModeTokenSource.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var health = Player.Local?.Health;
                        if (health != null && health.CurrentHealth < 9999f)
                        {
                            health.CurrentHealth = 9999f;
                            _afterlifeConsole("❤️ GodMode: Health restored to 9999");
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Warning($"GodMode loop error: {ex.Message}");
                    }

                    await Task.Delay(100, token); // Lower delay for tighter loop, still async
                }
            }, token);
        }

        public static void ToggleDemiGodMode()
        {
            DemiGodModeEnabled = !DemiGodModeEnabled;

            if (DemiGodModeEnabled)
            {
                _afterlifeConsole("✨ Demi-GodMode Enabled");
                ApplyDemiGodMode();
            }
            else
            {
                _afterlifeConsole("🛑 Demi-GodMode Disabled (one-time effect remains)");
            }
        }
        public static void ApplyDemiGodMode()
        {
            var health = Player.Local?.Health;
            if (health == null)
            {
                MelonLogger.Warning("⚠️ Cannot apply Demi-GodMode: Player health is null");
                return;
            }

            if (originalHealth == null)
            {
                originalHealth = health.CurrentHealth;
            }

            float boostedHealth = (float)(originalHealth * 3f);
            health.CurrentHealth = boostedHealth;

            _afterlifeConsole($"💪 Demi-GodMode applied: Health set to {boostedHealth}");
        }

        public static void ToggleThirdPersonCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                MelonLogger.Warning("⚠️ Cannot toggle camera: Main camera not found");
                return;
            }

            if (!thirdPersonEnabled)
            {
                // Save original camera position and rotation
                originalCamPosition = mainCamera.transform.position;
                originalCamRotation = mainCamera.transform.rotation;

                // Position camera 3 units behind the player
                var player = Player.Local;
                if (player == null)
                {
                    MelonLogger.Warning("⚠️ Player is null");
                    return;
                }

                // Get player position and forward direction
                Vector3 playerPos = player.transform.position;
                Vector3 backward = -player.transform.forward;

                // Offset camera
                mainCamera.transform.position = playerPos + backward * 3f + new Vector3(0, 1.5f, 0); // lift the camera a bit
                mainCamera.transform.LookAt(playerPos + new Vector3(0, 1.5f, 0));

                thirdPersonEnabled = true;
                _afterlifeConsole("📷 Third-person camera enabled");
            }
            else
            {
                // Restore camera to original position/rotation
                mainCamera.transform.position = originalCamPosition;
                mainCamera.transform.rotation = originalCamRotation;

                thirdPersonEnabled = false;
                _afterlifeConsole("📷 Camera reset to original position");
            }
        }

        public static void ToggleCameraRotation()
        {
            if (rotationIndex == 0)
                originalRotation = Player.Local.transform.rotation;

            rotationIndex = (rotationIndex + 1) % customRotations.Length;

            Quaternion targetRotation = rotationIndex == 0
                ? originalRotation
                : customRotations[rotationIndex];

            Player.Local.transform.rotation = targetRotation;
        }
        public static void ToggleFog()
        {
            RenderSettings.fog = !RenderSettings.fog;
        }

        //PostProcessingManager.Instance.SetBlur(0f);
        //PostProcessingManager.Instance.SetGodRayIntensity(0f);
        //PostProcessingManager.Instance.SetBloomThreshold(0f);


        public static void ChangeFogDensity(float amount)
        {
            float newDensity = RenderSettings.fogDensity + amount;
            RenderSettings.fogDensity = Mathf.Clamp(newDensity, 0f, 1f);
        }

        public static class CustomFogController
        {
            public static void SetFogColor(Color? color = null)
            {
                IsFogEnabled = !IsFogEnabled;

                if (color.HasValue)
                {
                    CustomFogColor = color.Value;
                    FogJustUpdated = true; // mark fog as just updated
                }
            }
        }
        //PostProcessingManager, UpdateEffects
        [HarmonyPatch(typeof(EnvironmentFX), "UpdateVisuals")]
        public static class EnvironmentFXPatch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                if (IsFogEnabled)
                {
                    RenderSettings.fogColor = CustomFogColor;
                    RenderSettings.fogMode = UnityEngine.FogMode.ExponentialSquared;

                    if (FogJustUpdated)
                    {
                        RenderSettings.fog = true;
                        RenderSettings.fogDensity = 0.05f;
                        FogJustUpdated = false; // reset after applying
                    }

                    return false; // skip original method
                }

                return true; // let original run if not enabled

                
            }
        }
        public static class FogColorController
        {
            public static Color? CustomFogColor = null;
        }

        [HarmonyPatch(typeof(EnvironmentFX), "UpdateVisuals")]
        public static class PostProcessingContrastManagerPatch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                if (IsGreyScale)
                {
                    PostProcessingManager.Instance.SetContrast(CustomContrastFloat);
                    return false; // skip original method
                }

                return true; // let original run if not enabled
            }
        }

        public static class CustomContrastController
        {
            public static void ToggleGreyscale(float? contrast = null)
            {
                IsGreyScale = !IsGreyScale;

                if (contrast.HasValue)
                {
                    CustomContrastFloat = contrast.Value;
                }
            }
        }

        [HarmonyPatch(typeof(EnvironmentFX), "UpdateVisuals")]
        public static class PostProcessingManagerPatch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                // If blur is disabled, return true and allow the original method to run (no changes to blur)
                if (!IsBlur)
                {
                    return true; // Run original method
                }

                // If blur is enabled, set the blur scale
                PostProcessingManager.Instance.SetBlur(CustomBlurScale);
                return false; // Skip the original method
            }
        }

        public static class CustomBlurController
        {
            // Toggle blur state and adjust blur scale
            public static void ToggleBlurscale(float? blurScale = null)
            {
                // If blur is off, toggle to on and set the blur scale
                if (!IsBlur)
                {
                    IsBlur = true;
                    if (blurScale.HasValue)
                    {
                        CustomBlurScale = blurScale.Value; // Set the custom blur scale
                    }
                }
                else
                {
                    // If blur is on, toggle to off and set blur scale to 0
                    IsBlur = false;
                    PostProcessingManager.Instance.SetBlur(0); // Reset blur scale to 0 when toggling off
                }

                // Debugging output to ensure toggle behavior works
                _afterlifeConsole($"Blur toggled: {IsBlur}, Blur scale: {CustomBlurScale}");
            }
        }
        public static Il2CppScheduleOne.AvatarFramework.AvatarEffects GetAvatarEffectsComponent(string npcName)
        {
            GameObject npcObj = _findAfterlifeGameObj(npcName + "_Clone");

            // If the GameObject is not found, return null.
            if (npcObj == null) return null;

            // Find the "Avatar/Effects" transform within the GameObject.
            var effectsTransform = npcObj.transform.Find("Avatar/Effects");

            // If the "Avatar/Effects" transform is not found, return null.
            if (effectsTransform == null) return null;

            // Return the AvatarEffects component from the found transform.
            return effectsTransform.GetComponent<Il2CppScheduleOne.AvatarFramework.AvatarEffects>();
        }

        [HarmonyPatch(typeof(Il2CppScheduleOne.AvatarFramework.AvatarEffects), "Update")]
        public static class AvatarEffects_Update_Patch
        {
            // Flag to allow patching
            public static bool patchAvatarEffects = true;

            [HarmonyPrefix]
            public static bool Prefix(Il2CppScheduleOne.AvatarFramework.AvatarEffects __instance)
            {
                if (!patchAvatarEffects)
                    return true; // Allow original method to run if patching is disabled

                // Fire-related logic: you can check if the fire settings are being updated, and change it here.
                if (__instance.FireLight != null)
                {
                    // Modify fire settings (if required) — only update fire-related things.
                    bool isFireActive = __instance.FireLight.Enabled;
                    // Example: Set fire active only based on custom logic.
                    if (isFireActive)
                    {
                        __instance.FireParticles.Play();
                        __instance.FireSound?.Play();
                        __instance.SetFireActive(true, true);
                    }
                    else
                    {
                        __instance.FireParticles.Stop();
                        __instance.FireSound?.Stop();
                        __instance.SetFireActive(false, false);
                    }

                    // Skip the original Update for fire logic, but let the rest of the Update run
                    return false;
                }

                // If no fire-related logic is active, allow original Update to run
                return true;
            }
        }


        // Method to set skybox from a URL
        public static void SetSkyBoxUrl(string url)
        {
            // Start the coroutine to download and apply the texture to the skybox
            _afterlifeCoroutinesStart(DownloadAndSetSkybox(url));
        }

        private static IEnumerator DownloadAndSetSkybox(string url)
        {
            // Send a web request to download the texture from the URL
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
            yield return uwr.SendWebRequest();

            // Handle errors during the download
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                _afterlifeConsole($"Error downloading texture: {uwr.error}");
            }
            else
            {
                // Get the downloaded texture
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // Create a new material and assign the downloaded texture
                Material skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));  // You can use a different skybox shader if needed
                skyboxMaterial.mainTexture = texture;

                // Set the skybox material in Unity's RenderSettings
                UnityEngine.RenderSettings.skybox = skyboxMaterial;

                // Optionally log the change
                _afterlifeConsole($"Skybox set to the texture from URL: {url}");
            }

            // Dispose the UnityWebRequest explicitly after it's done
            uwr.Dispose();
        }
        [HarmonyPatch(typeof(RenderSettings))]
        public static class FogColorPatch
        {
            // Flag to enable/disable the patch
            public static bool PatchEnabled { get; private set; } = false;

            // Methods to enable/disable the patch
            public static void EnablePatch() => PatchEnabled = true;
            public static void DisablePatch() => PatchEnabled = false;

            // Toggle the patch
            public static void ToggleFogColor()
            {
                PatchEnabled = !PatchEnabled;
                _afterlifeConsole($"[FogColorPatch] Toggled: {PatchEnabled}");
            }

            // Prefix to intercept changes to fog color
            [HarmonyPrefix]
            [HarmonyPatch("set_fogColor")]  // Assuming this is the method you're trying to patch
            public static bool SetFogColor_Prefix(ref Color __0)
            {
                if (!PatchEnabled)
                    return true;  // Let the original method run if patch is disabled

                // Change fog color to red
                __0 = new Color(255f, 0f, 0f);  // Red color in normalized range (0 to 1)

                _afterlifeConsole("[FogColorPatch] Applied red color to fogColor");
                return false;  // Skip the original method (optional depending on your needs)
            }
        }

        public static void ToggleAFKKill()
        {
            if (afkKillRoutine != null)
            {
                MelonCoroutines.Stop(afkKillRoutine);
                afkKillRoutine = null;
                return;
            }

            afkKillRoutine = (Coroutine)_afterlifeCoroutinesStart(CheckAndKillAFKPlayers());
        }

        private static IEnumerator CheckAndKillAFKPlayers()
        {
            Dictionary<Player, Vector3> lastPositions = new Dictionary<Player, Vector3>();
            Dictionary<Player, float> stillTime = new Dictionary<Player, float>();

            while (true)
            {
                foreach (var player in Player.PlayerList)
                {
                    if (player == Player.Local || player == null) continue;

                    Vector3 currentPos = player.transform.position;

                    if (!lastPositions.ContainsKey(player))
                    {
                        lastPositions[player] = currentPos;
                        stillTime[player] = 0f;
                    }

                    if (Vector3.Distance(currentPos, lastPositions[player]) < 0.01f)
                    {
                        stillTime[player] += Time.deltaTime;

                        if (stillTime[player] >= 5f)
                        {
                            player.OnDied(); // Kill the AFK player
                            stillTime[player] = 0f; // Reset timer to avoid multiple deaths
                        }
                    }
                    else
                    {
                        lastPositions[player] = currentPos;
                        stillTime[player] = 0f;
                    }
                }

                yield return null;
            }
        }
        public static class AudioControl
        {
            public static bool MusicMutedForever = true;
        }

        [HarmonyPatch(typeof(AudioManager), "Awake")]
        public static class AudioManagerAwakePatch
        {
            [HarmonyPostfix]
            public static void Postfix(AudioManager __instance)
            {
                if (AudioControl.MusicMutedForever)
                {
                    __instance.musicVolume = 0f;
                    __instance.SetVolume(EAudioType.Music, 0f);
                }
            }
        }
        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.SetVolume))]
        public static class AudioManagerPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(AudioManager __instance, EAudioType type, ref float volume)
            {
                if (type == EAudioType.Music && AudioControl.MusicMutedForever)
                {
                    volume = 0f;
                    __instance.musicVolume = 0f;
                    return false; // Block original method
                }

                return true;
            }
        }
        public static IEnumerator EnforceMusicMuteForever()
        {
            while (true)
            {
                if (AudioControl.MusicMutedForever)
                {
                    if (Il2CppScheduleOne.Audio.AudioManager.Instance != null)
                        Il2CppScheduleOne.Audio.AudioManager.Instance.musicVolume = 0f;

                    if (AudioManager.instance != null)
                        AudioManager.instance.SetVolume(EAudioType.Music, 0f);

                    if (Singleton<AudioManager>.Instance != null)
                        Singleton<AudioManager>.Instance.SetVolume(EAudioType.Music, 0f);
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        public static void ToggleDrunkCamera(float durationSeconds, float drunkStrength)
        {
            if (isDrunkCameraActive)
            {
                // Stop the effect early and restore original rotation
                isDrunkCameraActive = false;
                if (drunkRoutine != null)
                    MelonCoroutines.Stop(drunkRoutine);

                if (Camera.main != null)
                    Camera.main.transform.localRotation = originalCameraRotation;

                return;
            }

            if (Camera.main == null) return;

            originalCameraRotation = Camera.main.transform.localRotation;
            isDrunkCameraActive = true;
            drunkRoutine = (Coroutine)_afterlifeCoroutinesStart(DrunkCameraRoutine(durationSeconds, drunkStrength));
        }

        private static IEnumerator DrunkCameraRoutine(float duration, float drunkStrength)
        {
            Transform cameraTransform = Camera.main.transform;
            float startTime = Time.time;
            float speed = 5.0f;
            float maxRoll = 35f;
            float maxYawPitch = 25f;

            while (isDrunkCameraActive && (Time.time - startTime < duration))
            {
                float elapsed = Time.time - startTime;
                float t = Time.time * speed;

                float normalizedTime = elapsed / duration;
                float intensity = Mathf.Sin(normalizedTime * Mathf.PI); // in/out curve

                float yaw = (Mathf.Sin(t * 1.3f) + (Mathf.PerlinNoise(t * 0.2f, 0f) - 0.5f) * 2f) * maxYawPitch * drunkStrength * intensity;
                float pitch = (Mathf.Cos(t * 1.7f) + (Mathf.PerlinNoise(0f, t * 0.3f) - 0.5f) * 2f) * maxYawPitch * drunkStrength * intensity;
                float roll = (Mathf.Sin(t * 2.5f) + (Mathf.PerlinNoise(t * 0.4f, t * 0.4f) - 0.5f) * 2f) * maxRoll * drunkStrength * intensity;

                Quaternion drunkRotation = Quaternion.Euler(pitch, yaw, roll);
                cameraTransform.localRotation = originalCameraRotation * drunkRotation;

                yield return null;
            }

            // Clean up and restore
            if (Camera.main != null)
                Camera.main.transform.localRotation = originalCameraRotation;

            isDrunkCameraActive = false;
        }


        public static void ToggleGUI(Player target)
        {
            if (target == null || !ValidatePlayer(target, "gui teleport target"))
                return;

            TargetPlayer = target;
            showGUIPlayer = !showGUIPlayer;
        }
        public static void SetSkateboardJumpForce(object val)
        {
            if (float.TryParse(val as string, out var f))
                Player.Local.ActiveSkateboard.JumpForce = f;
            Player.Local._ActiveSkateboard_k__BackingField.JumpForce = f;
        }

        public static void SkateboardSuperJump()
        {
            Player.Local.ActiveSkateboard.JumpForce = 300f;
            Player.Local._ActiveSkateboard_k__BackingField.JumpForce = 300f;
        }

        public static void SetSkateboardPushForce(object val)
        {
            if (float.TryParse(val as string, out var f))
                Player.Local.ActiveSkateboard.PushForceMultiplier = f;
            Player.Local._ActiveSkateboard_k__BackingField.PushForceMultiplier = f;
        }

        public static IEnumerator GiveSuperJumpTimeSpecified(float timeBetween)
        {
            _afterlifeConsole($"You have super jump for {timeBetween} seconds.");
            SetJumpForce(150f);
            yield return new WaitForSeconds(timeBetween); 
            SetJumpForce(10f);
        }
        public static IEnumerator GiveGodModeTimeSpecified(float timeBetween)
        {
            _afterlifeConsole($"You have godmode for {timeBetween} seconds.");
            ToggleGodMode(); 
            yield return new WaitForSeconds(timeBetween); 
            ToggleGodMode();
        }
        public static IEnumerator GiveSetMovementSpeedTimeSpecified(float timeBetween)
        {
            _afterlifeConsole($"You have godmode for {timeBetween} seconds.");
            SetMovementSpeed(500f);
            yield return new WaitForSeconds(timeBetween);
            SetMovementSpeed(10f);
        }

        public static IEnumerator PlayAudio(string MP3Name)
        {
            var audioSettings = new Il2CppScheduleOne.DevUtilities.AudioSettings();
            audioSettings.MusicVolume = 0f;

            // If audio is already playing, stop it and reuse the objects
            if (isAudioPlaying)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    _afterlifeConsole($"Stopped playing: {currentTrack}");
                }

                // Reuse audioObj and source for the new track
                source.clip = null;
                currentTrack = null;

                isAudioPlaying = false;
                yield break;
            }

            string modsDir = MelonLoader.Utils.MelonEnvironment.ModsDirectory;
            string musicDir = Path.Combine(modsDir, "AfterlifeMusic");
            Directory.CreateDirectory(musicDir);

            string fileName = MP3Name.StartsWith("http") ? ExtractFileNameFromUrl(MP3Name) : MP3Name;
            string filePath = Path.Combine(musicDir, fileName);

            // Download if it's a URL
            if (MP3Name.StartsWith("http://") || MP3Name.StartsWith("https://"))
            {
                UnityWebRequest uwr = UnityWebRequest.Get(MP3Name);
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    MelonLogger.Error("Download failed: " + uwr.error);
                    yield break;
                }

                byte[] data = uwr.downloadHandler.data;
                _afterlifeConsole($"Downloaded {data.Length} bytes");
                _afterlifeConsole($"First 10 bytes of the downloaded file: {BitConverter.ToString(data.Take(10).ToArray())}");

                File.WriteAllBytes(filePath, data);
                _afterlifeConsole($"Saved audio to: {filePath}");

                uwr.Dispose();
            }

            if (!File.Exists(filePath))
            {
                MelonLogger.Error("Audio file not found: " + filePath);
                yield break;
            }

            if (!filePath.EndsWith(".mp3"))
            {
                MelonLogger.Error("The downloaded file is not an MP3 file: " + filePath);
                yield break;
            }

            if (new FileInfo(filePath).Length == 0)
            {
                MelonLogger.Error("The MP3 file is empty: " + filePath);
                yield break;
            }

            string uri = "file:///" + Path.GetFullPath(filePath).Replace("\\", "/");
            _afterlifeConsole("Attempting to load audio from: " + uri);

            WWW www = new WWW(uri);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                MelonLogger.Error("Failed to load audio: " + www.error);
            }
            else
            {
                AudioClip clip = www.GetAudioClip(false, false, AudioType.MPEG);

                // Reuse or create audioObj
                if (audioObj == null)
                {
                    audioObj = new GameObject("MP3Player");
                    UnityEngine.Object.DontDestroyOnLoad(audioObj);
                }

                // Reuse or create source
                if (source == null)
                {
                    source = audioObj.AddComponent<AudioSource>();
                }

                source.clip = clip;
                source.Play();
                currentTrack = Path.GetFileName(filePath);

                _afterlifeConsole($"Now playing: {currentTrack}");
                isAudioPlaying = true;
            }
        }

        private static string ExtractFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath); // Extract the file name
            return fileName.Split('?')[0]; // Remove any query parameters if they exist
        }
    }

    public static class OfficerCache
    {
        public static void CacheOfficers()
        {
            officers.Clear();
            officers.AddRange(UnityEngine.Object.FindObjectsOfType<PoliceOfficer>());
            _afterlifeConsole($"Found {officers.Count} PoliceOfficer objects.");
        }
    }
    public class BouncePadTriggerProxy : MonoBehaviour
    {
        // Called when another object enters the trigger collider
        private void OnTriggerEnter(Collider other)
        {
            // Replace with your actual local player reference or name check
            if (other.gameObject.name == "local_player")
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Launch the player upward
                    rb.velocity = Vector3.zero; // reset velocity for consistency
                    rb.AddForce(Vector3.up * launchForce, ForceMode.VelocityChange);

                    _afterlifeConsole($"🚀 Launched {other.gameObject.name} with force {launchForce}!");
                }
                else
                {
                    MelonLogger.Warning($"⚠️ {other.gameObject.name} has no Rigidbody.");
                }
            }
            else
            {
                _afterlifeConsole($"Ignored trigger from {other.gameObject.name}.");
            }
        }

        // Optionally, you can expose methods like this to modify the launch force dynamically
        public void SetLaunchForce(float force)
        {
            launchForce = force;
        }
    }
    public static class PoliceESPController
    {
        public static void ToggleESP()
        {
            espEnabled = !espEnabled;

            if (espEnabled && _espObject == null)
            {
                _espObject = new GameObject("PoliceESP");
                _espObject.AddComponent<PoliceESP>();
                UnityEngine.Object.DontDestroyOnLoad(_espObject);
            }
            else if (!espEnabled && _espObject != null)
            {
                UnityEngine.Object.Destroy(_espObject);
                _espObject = null;
            }
        }
    }
    public class PoliceESP : MonoBehaviour
    {
        private void OnGUI()
        {
            if (!espEnabled) return;

            foreach (string npcName in _allnpcs.allNpcCharacters)
            {
                if (!npcName.ToLower().Contains("police"))
                    continue;

                GameObject npc = GameObject.Find(npcName);
                if (npc == null) continue;

                Vector3 worldPos = npc.transform.position;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                if (screenPos.z <= 0) continue;

                float boxWidth = 100f;
                float boxHeight = 120f;
                float x = screenPos.x - (boxWidth / 2);
                float y = Screen.height - screenPos.y - boxHeight;

                GUI.color = Color.red;
                GUI.Box(new Rect(x, y, boxWidth, boxHeight), GUIContent.none);

                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.UpperCenter
                };
                GUI.Label(new Rect(screenPos.x - 50, y - 20, 100, 20), npcName, style);
            }
        }
    }
}
