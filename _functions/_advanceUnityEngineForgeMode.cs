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
using static _afterlifeScModMenu._globalVariables;
using static _afterlifeScModMenu._mainMenu;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;
using static UnityEngine.Vector3;
using static UnityEngine.Physics;
using static UnityEngine.GameObject;
using static UnityEngine.Input;
using static UnityEngine.Camera;
using static UnityEngine.Mathf;
using static UnityEngine.Object;
using static System.IO.Path;
using static System.IO.File;
using static System.IO.Directory;
using static MelonLoader.Utils.MelonEnvironment;
using static System.Threading.Tasks.Task;

namespace _clientids
{
    class _advanceUnityEngineForgeMode
    {
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
                _afterlifeConsole($"Forgemode: {(forgeModeBool ? "On" : "Off")}");
                lastForgeModeLogged = forgeModeBool;
            }
            if (forgeModeBool)
            {
                if (main == null)
                    return;

                Ray ray = main.ScreenPointToRay(mousePosition);
                RaycastHit hit;

                // Toggle movement on/off with F8
                if (GetKeyDown(KeyCode.F8))
                {
                    if (!isMovingObject)
                    {
                        if (Raycast(ray, out hit, 100f))
                        {
                            GameObject hitObject = hit.collider.gameObject;
                            GameObject objectToMove = GetTopMostAllowedParent(hitObject);

                            if (objectToMove != null && !selectedObjects.Contains(objectToMove))
                            {
                                selectedObjects.Add(objectToMove);
                                selectedObject = objectToMove;

                                moveDistance = Distance(main.transform.position, hit.point);
                                grabOffset = objectToMove.transform.position - hit.point;

                                isMovingObject = true;

                                _afterlifeConsole($"[F8] Grabbed: {objectToMove.name}");
                                PrintChildren(objectToMove);
                            }
                            else
                            {
                                _afterlifeConsole($"[F8] Object {hitObject.name} is not allowed for movement.");
                            }
                        }
                    }
                    else
                    {
                        foreach (var obj in selectedObjects)
                            _afterlifeConsole($"[F8] Released: {obj.name}");

                        selectedObjects.Clear();
                        selectedObject = null;
                        isMovingObject = false;
                    }
                }

                // 🖱️ Scroll wheel: Adjust move distance
                float scroll = GetAxis("Mouse ScrollWheel");
                if (scroll != 0f && isMovingObject)
                {
                    moveDistance += scroll * 5f;
                    moveDistance = Clamp(moveDistance, 0.1f, 100f);
                }

                // 🔁 E: Rotate in 60-degree increments
                if (GetKeyDown(KeyCode.E) && isMovingObject)
                {
                    foreach (GameObject obj in selectedObjects)
                    {
                        obj.transform.Rotate(up, 60f, Space.World);
                    }
                }

                // ❌ Delete: Remove selected objects and release
                // When deleting an object, make sure to store the original GameObject reference
                if (GetKeyDown(KeyCode.Delete) && isMovingObject)
                {
                    foreach (GameObject obj in selectedObjects)
                    {
                        // Store deleted object and its position
                        deletedObjects.Push((obj, obj.transform.position));

                        // Destroy the object in Unity
                        Destroy(obj);
                        _afterlifeConsole($"[Delete] Destroyed: {obj.name}");
                    }

                    selectedObjects.Clear();
                    selectedObject = null;
                    isMovingObject = false;
                }

                // Undo: Respawn the last deleted object at its last position (Ctrl+Z)
                if (GetKeyDown(KeyCode.Z) && GetKey(KeyCode.LeftControl))
                {
                    if (deletedObjects.Count > 0)
                    {
                        // Pop the most recent deleted object and its position
                        var (obj, position) = deletedObjects.Pop();

                        if (obj != null)
                        {
                            // Instantiate the object at the stored position
                            GameObject restoredObject = Instantiate(obj);
                            restoredObject.transform.position = position;
                            restoredObject.SetActive(true);  // Ensure it's active in the scene
                            _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(restoredObject.name, true));

                            // Optionally, handle resetting any other properties (like rotation)
                            _afterlifeConsole($"[Ctrl+Z] Restored: {obj.name} at {position}");
                        }
                        else
                        {
                            _afterlifeConsole("[Ctrl+Z] Failed to restore: object is null.");
                        }
                    }
                }
                if (GetKey(KeyCode.LeftControl) && GetKeyDown(KeyCode.P))
                {
                    GameObject objectToPrint = null;

                    if (selectedObject != null)
                    {
                        objectToPrint = selectedObject;
                    }
                    else if (Raycast(ray, out hit, 100f))
                    {
                        objectToPrint = hit.collider?.gameObject;
                        if (objectToPrint != null)
                        {
                            Transform parentTransform = objectToPrint.transform.parent;
                            if (parentTransform != null)
                            {
                                var parent = parentTransform.gameObject;
                                _afterlifeConsole($"[Ctrl+P] Looking at object: {parent.name} at {parent.transform.position}");
                                LogObjectDataToFile(new LoggedGameObject(parent.name, parent.transform.position));
                            }
                            else
                            {
                                _afterlifeConsole($"[Ctrl+P] Looking at object: {objectToPrint.name} at {objectToPrint.transform.position}");
                                LogObjectDataToFile(new LoggedGameObject(objectToPrint.name, objectToPrint.transform.position));
                            }
                        }
                    }

                    if (objectToPrint == null)
                    {
                        _afterlifeConsole("[Ctrl+P] No object in view.");
                    }
                }
                // 🧬 Ctrl + D: Dump all GameObject names in the scene
                if (GetKey(KeyCode.LeftControl) && GetKeyDown(KeyCode.D) && !ConfigLock)
                {
                    _afterlifeConsole($"{ConfigLock.ToString()}");
                    DumpAllGameObjectsAsync();
                }

                // 🧬 C: Clone selected object(s), release original, grab cloned
                if (GetKeyDown(KeyCode.C) && isMovingObject && selectedObjects.Count > 0)
                {
                    List<GameObject> clonedObjects = new List<GameObject>();

                    foreach (GameObject obj in selectedObjects)
                    {
                        GameObject clone = Instantiate(obj);
                        clone.transform.position = obj.transform.position;
                        clone.transform.rotation = obj.transform.rotation;
                        clone.name = obj.name + "_Clone";
                        clone.SetActive(true);
                        _afterlifeCoroutinesStart(_unityfunctions.ScheduleIObjectActive(clone.name, true));

                        clonedObjects.Add(clone);
                        _afterlifeConsole($"[C] Cloned: {obj.name} → {clone.name}");
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
                        Ray newRay = main.ScreenPointToRay(mousePosition);
                        RaycastHit newHit;

                        if (Raycast(newRay, out newHit, 100f))
                        {
                            moveDistance = Distance(main.transform.position, newHit.point);
                            grabOffset = selectedObject.transform.position - newHit.point;
                        }
                        else
                        {
                            // Fallback if raycast fails
                            moveDistance = Distance(main.transform.position, selectedObject.transform.position);
                            grabOffset = zero;
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
            foreach (char c in GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        // Function to log the object to a text file if it's not already logged
        private static string filePath = Combine(ModsDirectory, "AfterlifeLogs", "AfterlifeGameObjectNames.txt");

        public static void LogObjectDataToFile(LoggedGameObject loggedObj)
        {
            // Ensure directory exists
            string dir = GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                CreateDirectory(dir);

            // Ensure file exists
            if (!File.Exists(filePath))
                WriteAllText(filePath, "GameObject Log:\n");

            string logEntry = $"gameObject: \"{loggedObj.Name}\", position: ({loggedObj.Position.x}, {loggedObj.Position.y}, {loggedObj.Position.z})\n";

            // Check for duplicates
            string[] existingEntries = ReadAllLines(filePath);
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
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
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
            await Run(() =>
            {
                foreach (var loggedObj in objectsToLog)
                {
                    LogObjectDataToFile(loggedObj);
                }
            });

            _afterlifeConsole($"[Ctrl+D] Dumped {count} gameObject names to log.");
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
                _afterlifeConsole($"{prefix}GameObject is null.");
                return;
            }

            // Ensure the gameObject has a transform
            Transform objTransform = gameObject.GetComponent<Transform>();

            if (objTransform == null)
            {
                _afterlifeConsole($"{prefix}Transform is null for: {gameObject.name}");
                return;
            }

            _afterlifeConsole($"{prefix}Parent: {gameObject.name}");

            foreach (Transform child in objTransform)
            {
                if (child == null)
                {
                    _afterlifeConsole($"{prefix}  Child is null.");
                    continue;
                }

                GameObject childObj = child.gameObject;

                if (childObj != null)
                {
                    _afterlifeConsole($"{prefix}  Child: {childObj.name}");
                    PrintChildren(childObj, prefix + "    ");
                }
                else
                {
                    _afterlifeConsole($"{prefix}  Child GameObject is null.");
                }
            }

            _afterlifeConsole($"Type of gameObject.transform: {gameObject.transform.GetIl2CppType().FullName}");
        }
    }
}
