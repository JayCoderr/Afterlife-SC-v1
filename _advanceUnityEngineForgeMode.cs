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
using _afterlifeMod;
using static _afterlifeMod._afterlifeMod;

namespace _clientids
{
    class _advanceUnityEngineForgeMode
    {

        public static GameObject selectedObject = null;
        public static Vector3 grabOffset = Vector3.zero;
        public static float moveDistance = 5f;
        public static bool isMovingObject = false;
        public static List<GameObject> selectedObjects = new List<GameObject>();
        public static Stack<(GameObject obj, Vector3 position)> deletedObjects = new Stack<(GameObject, Vector3)>();
        public static bool ConfigLock = true;
        public static GameObject cachedPickupSource;
        public static string[] allowedPickUpObjectNames = new string[]
        {
            "SingleBed(Clone)",
            "SingleBed",
            "Blinds (1)",
            "Blinds",
            "Standalone Sink",
            "Intercom Save Point",
            "Classical Wooden door",
            "Wall Clock",
            "TrashCan_Built(Clone)",
            "TrashCan_Built",
            "Shirley",
            "Salvador",
            "Jessi",
            "Donna",
            "Geraldine",
            "Keith",
            "OfficerGreen",
            "Kathy",
            "Beth",
            "Kim",
            "Trent",
            "Dean",
            "Mick",
            "Chole",
            "Igor",
            "Molly",
            "Jerry",
            "Jeff",
            "George",
            "Benji",
            "Javier",
            "Albert",
            "UncleNelson",
            "Peter",
            "Austin",
            "OfficerJackson",
            "OfficerLee",
            "Ming",
            "Charles",
            "Frank",
            "Kyle",
            "Peggy",
            "Anna",
            "Joyce",
            "Dan",
            "Ludwig",
            "Fiona",
            "Doris",
            "Billy",
            "Sam",
            "OfficerMurphy",
            "OfficerLopez",
            "OfficerDavis",
            "SUV_Police(Clone)",
            "SUV_Police",
            "Vending Machine (10)",
            "Vending Machine (9)",
            "Vending Machine (8)",
            "Vending Machine (7)",
            "Vending Machine (6)",
            "Vending Machine (5)",
            "Vending Machine (4)",
            "Vending Machine (3)",
            "Vending Machine (2)",
            "Vending Machine (1)",
            "Vending Machine",
            "Coupe",
            "ATM (13)",
            "ATM (12)",
            "ATM (11)",
            "ATM (10)",
            "ATM (9)",
            "ATM (8)",
            "ATM (7)",
            "ATM (6)",
            "ATM (5)",
            "ATM (4)",
            "ATM (3)",
            "ATM (2)",
            "ATM (1)",
            "atm (13)",
            "atm (12)",
            "atm (11)",
            "atm (10)",
            "atm (9)",
            "atm (8)",
            "atm (7)",
            "atm (6)",
            "atm (5)",
            "atm (4)",
            "atm (3)",
            "atm (2)",
            "atm (1)",
        };

        public static void MenuForgeMode(bool ConfigLock)
        {
            if (forgeModeBool != lastForgeModeLogged)
            {
                MelonLogger.Msg($"Forgemode: {(forgeModeBool ? "On" : "Off")}");
                lastForgeModeLogged = forgeModeBool;
            }
            if (forgeModeBool)
            {
                if (Camera.main == null)
                    return;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Toggle movement on/off with F8
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    if (!isMovingObject)
                    {
                        if (Physics.Raycast(ray, out hit, 100f))
                        {
                            GameObject hitObject = hit.collider.gameObject;
                            GameObject objectToMove = GetTopMostAllowedParent(hitObject);

                            if (objectToMove != null && !selectedObjects.Contains(objectToMove))
                            {
                                selectedObjects.Add(objectToMove);
                                selectedObject = objectToMove;

                                moveDistance = Vector3.Distance(Camera.main.transform.position, hit.point);
                                grabOffset = objectToMove.transform.position - hit.point;

                                isMovingObject = true;

                                MelonLogger.Msg($"[F8] Grabbed: {objectToMove.name}");
                                PrintChildren(objectToMove);
                            }
                            else
                            {
                                MelonLogger.Msg($"[F8] Object {hitObject.name} is not allowed for movement.");
                            }
                        }
                    }
                    else
                    {
                        foreach (var obj in selectedObjects)
                            MelonLogger.Msg($"[F8] Released: {obj.name}");

                        selectedObjects.Clear();
                        selectedObject = null;
                        isMovingObject = false;
                    }
                }

                // 🖱️ Scroll wheel: Adjust move distance
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f && isMovingObject)
                {
                    moveDistance += scroll * 5f;
                    moveDistance = Mathf.Clamp(moveDistance, 0.1f, 100f);
                }

                // 🔁 E: Rotate in 60-degree increments
                if (Input.GetKeyDown(KeyCode.E) && isMovingObject)
                {
                    foreach (GameObject obj in selectedObjects)
                    {
                        obj.transform.Rotate(Vector3.up, 60f, Space.World);
                    }
                }

                // ❌ Delete: Remove selected objects and release
                // When deleting an object, make sure to store the original GameObject reference
                if (Input.GetKeyDown(KeyCode.Delete) && isMovingObject)
                {
                    foreach (GameObject obj in selectedObjects)
                    {
                        // Store deleted object and its position
                        deletedObjects.Push((obj, obj.transform.position));

                        // Destroy the object in Unity
                        UnityEngine.Object.Destroy(obj);
                        MelonLogger.Msg($"[Delete] Destroyed: {obj.name}");
                    }

                    selectedObjects.Clear();
                    selectedObject = null;
                    isMovingObject = false;
                }

                // Undo: Respawn the last deleted object at its last position (Ctrl+Z)
                if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
                {
                    if (deletedObjects.Count > 0)
                    {
                        // Pop the most recent deleted object and its position
                        var (obj, position) = deletedObjects.Pop();

                        if (obj != null)
                        {
                            // Instantiate the object at the stored position
                            GameObject restoredObject = UnityEngine.Object.Instantiate(obj);
                            restoredObject.transform.position = position;
                            restoredObject.SetActive(true);  // Ensure it's active in the scene
                            MelonCoroutines.Start(_unityfunctions.ScheduleIObjectActive(restoredObject.name, true));

                            // Optionally, handle resetting any other properties (like rotation)
                            MelonLogger.Msg($"[Ctrl+Z] Restored: {obj.name} at {position}");
                        }
                        else
                        {
                            MelonLogger.Msg("[Ctrl+Z] Failed to restore: object is null.");
                        }
                    }
                }
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P))
                {
                    GameObject objectToPrint = null;

                    if (selectedObject != null)
                    {
                        objectToPrint = selectedObject;
                    }
                    else if (Physics.Raycast(ray, out hit, 100f))
                    {
                        objectToPrint = hit.collider?.gameObject;
                        if (objectToPrint != null)
                        {
                            Transform parentTransform = objectToPrint.transform.parent;
                            if (parentTransform != null)
                            {
                                var parent = parentTransform.gameObject;
                                MelonLogger.Msg($"[Ctrl+P] Looking at object: {parent.name} at {parent.transform.position}");
                                LogObjectDataToFile(new LoggedGameObject(parent.name, parent.transform.position));
                            }
                            else
                            {
                                MelonLogger.Msg($"[Ctrl+P] Looking at object: {objectToPrint.name} at {objectToPrint.transform.position}");
                                LogObjectDataToFile(new LoggedGameObject(objectToPrint.name, objectToPrint.transform.position));
                            }
                        }
                    }

                    if (objectToPrint == null)
                    {
                        MelonLogger.Msg("[Ctrl+P] No object in view.");
                    }
                }
                // 🧬 Ctrl + D: Dump all GameObject names in the scene
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D) && !ConfigLock)
                {
                    MelonLogger.Msg($"{ConfigLock.ToString()}");
                    DumpAllGameObjectsAsync();
                }

                // 🧬 C: Clone selected object(s), release original, grab cloned
                if (Input.GetKeyDown(KeyCode.C) && isMovingObject && selectedObjects.Count > 0)
                {
                    List<GameObject> clonedObjects = new List<GameObject>();

                    foreach (GameObject obj in selectedObjects)
                    {
                        GameObject clone = UnityEngine.Object.Instantiate(obj);
                        clone.transform.position = obj.transform.position;
                        clone.transform.rotation = obj.transform.rotation;
                        clone.name = obj.name + "_Clone";
                        clone.SetActive(true);
                        MelonCoroutines.Start(_unityfunctions.ScheduleIObjectActive(clone.name, true));

                        clonedObjects.Add(clone);
                        MelonLogger.Msg($"[C] Cloned: {obj.name} → {clone.name}");
                    }

                    // Release original
                    selectedObjects.Clear();
                    selectedObject = null;
                    isMovingObject = false;

                    // Start moving the clone
                    if (clonedObjects.Count > 0)
                    {
                        selectedObjects = clonedObjects;
                        selectedObject = selectedObjects[0];

                        // ✅ Recalculate ray AFTER clone exists
                        Ray newRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit newHit;

                        if (Physics.Raycast(newRay, out newHit, 100f))
                        {
                            moveDistance = Vector3.Distance(Camera.main.transform.position, newHit.point);
                            grabOffset = selectedObject.transform.position - newHit.point;
                        }
                        else
                        {
                            // Fallback if raycast fails
                            moveDistance = Vector3.Distance(Camera.main.transform.position, selectedObject.transform.position);
                            grabOffset = Vector3.zero;
                        }

                        isMovingObject = true;
                    }
                }

                // ✋ Apply movement
                if (isMovingObject && selectedObjects.Count > 0)
                {
                    Vector3 targetPosition = ray.GetPoint(moveDistance) + grabOffset;
                    Vector3 delta = targetPosition - selectedObjects[0].transform.position;

                    foreach (GameObject obj in selectedObjects)
                    {
                        obj.transform.position += delta;
                    }
                }
            }
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        // Function to log the object to a text file if it's not already logged
        private static string filePath = Path.Combine(MelonEnvironment.ModsDirectory, "AfterlifeLogs", "AfterlifeGameObjectNames.txt");

        public static void LogObjectDataToFile(LoggedGameObject loggedObj)
        {
            // Ensure directory exists
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // Ensure file exists
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, "GameObject Log:\n");

            string logEntry = $"gameObject: \"{loggedObj.Name}\", position: ({loggedObj.Position.x}, {loggedObj.Position.y}, {loggedObj.Position.z})\n";

            // Check for duplicates
            string[] existingEntries = File.ReadAllLines(filePath);
            if (!existingEntries.Contains(logEntry))
            {
                File.AppendAllText(filePath, logEntry);
            }
        }
        public struct LoggedGameObject
        {
            public string Name;
            public Vector3 Position;

            public LoggedGameObject(string name, Vector3 position)
            {
                Name = name;
                Position = position;
            }
        }
        // Check if the parent (or ancestor) of the object has a specific name
        public static async void DumpAllGameObjectsAsync()
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            List<LoggedGameObject> objectsToLog = new List<LoggedGameObject>();

            // Collect names and positions on the main thread
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    objectsToLog.Add(new LoggedGameObject(obj.name, obj.transform.position));
                }
            }

            int count = objectsToLog.Count;

            // Run the file writing on a background thread
            await Task.Run(() =>
            {
                foreach (var loggedObj in objectsToLog)
                {
                    LogObjectDataToFile(loggedObj);
                }
            });

            MelonLogger.Msg($"[Ctrl+D] Dumped {count} gameObject names to log.");
        }
        private GameObject GetParentWithName(Il2CppSystem.Object obj, string name)
        {
            GameObject gameObject = obj.TryCast<GameObject>();
            if (gameObject == null)
                return null;

            Transform current = gameObject.transform;

            while (current.parent != null)
            {
                if (current.parent.name == name)
                {
                    return current.parent.gameObject;
                }
                current = current.parent;
            }

            return null;
        }
        public static GameObject GetTopMostAllowedParent(Il2CppSystem.Object obj)
        {
            GameObject gameObject = obj.TryCast<GameObject>();
            if (gameObject == null)
                return null;

            GameObject result = null;
            Transform current = gameObject.transform;

            while (current != null)
            {
                if (allowedPickUpObjectNames.Contains(current.name))
                {
                    result = current.gameObject;
                }
                current = current.parent;
            }

            return result;
        }

        public static void PrintChildren(GameObject gameObject, string prefix = "")
        {
            if (gameObject == null)
            {
                MelonLogger.Msg($"{prefix}GameObject is null.");
                return;
            }

            // Ensure the gameObject has a transform
            Transform objTransform = gameObject.GetComponent<Transform>();

            if (objTransform == null)
            {
                MelonLogger.Msg($"{prefix}Transform is null for: {gameObject.name}");
                return;
            }

            MelonLogger.Msg($"{prefix}Parent: {gameObject.name}");

            foreach (Transform child in objTransform)
            {
                if (child == null)
                {
                    MelonLogger.Msg($"{prefix}  Child is null.");
                    continue;
                }

                GameObject childObj = child.gameObject;

                if (childObj != null)
                {
                    MelonLogger.Msg($"{prefix}  Child: {childObj.name}");
                    PrintChildren(childObj, prefix + "    ");
                }
                else
                {
                    MelonLogger.Msg($"{prefix}  Child GameObject is null.");
                }
            }

            MelonLogger.Msg($"Type of gameObject.transform: {gameObject.transform.GetIl2CppType().FullName}");
        }
    }
}
