using _afterlifeMod;
using _clientids;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppSystem.IO;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static _afterlifeMod._clientids;
using static _afterlifeMod._functions;
using static _clientids._menuStructure;


namespace _afterlifeScModMenu
{
    internal class _generatedSubMenus
    {
        public static List<MenuOption> dynamicOptions = new List<MenuOption>();
        // This function will be called when a player is selected
        public static void loadPlayersByNameMenu(string submenuName)
        {
            const string dynamicPrefix = "[Player] ";
            const string backLabel = "Back";

            string returnToMenu = "Afterlife SC v1"; // Main menu
            string allowedMenu = "Players Menu"; // <-- Only run for this menu

            // Skip all other menus
            if (submenuName != allowedMenu)
            {
                return;
            }

            List<MenuOption> dynamicOptions = new List<MenuOption>();

            foreach (var player in Player.PlayerList)
            {
                string playerName = player.PlayerName;
                dynamicOptions.Add(new MenuOption(dynamicPrefix + playerName, () => LoadPlayerSubMenu(playerName)));
            }

            dynamicOptions.Add(new MenuOption(backLabel, () => CreateSubMenu(returnToMenu, 22)));
            
            if (menuStructure.ContainsKey(submenuName))
            {
                menuStructure[submenuName].RemoveAll(o =>
                    o is MenuOption option &&
                    (option.Label.StartsWith(dynamicPrefix) || option.Label == backLabel));

                menuStructure[submenuName].AddRange(dynamicOptions.Cast<MenuElement>());
            }
            else
            {
                menuStructure[submenuName] = dynamicOptions.Cast<MenuElement>().ToList();
            }
            
            unifiedMenuOptions = new List<MenuElement>(menuStructure[submenuName]);
            activeSubMenuCount = unifiedMenuOptions.Count;
            MenuTotalIndex = activeSubMenuCount;
            submenuCounts[submenuName] = activeSubMenuCount;
        }


        // This function will be called when a player is selected
        private static void LoadPlayerSubMenu(string playerName)
        {
            // Check if the player exists by using a simple loop and Count
            Player player = null;
            int playerCount = Player.PlayerList.Count;

            for (int i = 0; i < playerCount; i++)
            {
                if (Player.PlayerList[i].PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    player = Player.PlayerList[i];
                    break;
                }
            }

            if (player == null)
            {
                TestMsg($"Player not found: {playerName}");
                return;
            }

            // Recreate the submenu for this player
            List<MenuElement> playerSubMenu = new List<MenuElement>
            {
                new MenuOption("Send Message", () => TestMsg($"Message sent to {playerName}")),
                new MenuOption("Teleport To", () => TeleportToPlayer(player)),
                new MenuOption("Kick Player", () => TestMsg($"Kicked {playerName}")),
                new MenuOption("Parent to player", () => ToggleFollowPlayer(Player.Local, player)),
                new MenuOption("Player to parent", () => ToggleFollowPlayer(player, Player.Local)),//ToggleGUI
                new MenuOption("Draw gui", () => ToggleGUI(player)),
                new MenuOption("Back", () => CreateSubMenu("Players Menu", Player.PlayerList.Count)) // Back to the player list
            };

            // Always recreate or update the submenu for the player
            menuStructure[playerName] = playerSubMenu;

            // Now load it into the active menu
            unifiedMenuOptions = new List<MenuElement>(playerSubMenu);
            activeSubMenuCount = unifiedMenuOptions.Count;
            MenuTotalIndex = activeSubMenuCount;
            submenuCounts[playerName] = activeSubMenuCount;
        }


        public static void ModifyAudioMenu(string submenuName)
        {
            string audioDirectory = Path.Combine(MelonLoader.Utils.MelonEnvironment.ModsDirectory, "AfterlifeMusic");

            if (!Directory.Exists(audioDirectory))
            {
                Directory.CreateDirectory(audioDirectory);
                MelonLogger.Msg("Created AfterlifeMusic directory.");
                return;
            }

            // Clear previous submenu items for this submenu
            if (submenuCounts.TryGetValue(submenuName, out int previousCount))
            {
                unifiedMenuOptions.RemoveRange(unifiedMenuOptions.Count - previousCount, previousCount);
            }

            int newCount = 0;
            string[] mp3Files = Directory.GetFiles(audioDirectory, "*.mp3");

            foreach (string file in mp3Files)
            {
                string fileName = Path.GetFileName(file);
                string displayName = fileName.Length > 20 ? fileName.Substring(0, 19) + "..." : fileName;

                unifiedMenuOptions.Add(new MenuOption($"Play {displayName}", () =>
                {
                    MelonCoroutines.Start(PlayAudio(fileName));
                }));

                newCount++;
            }

            // Update submenu count
            submenuCounts[submenuName] = newCount;
            activeSubMenuCount = unifiedMenuOptions.Count;
            MenuTotalIndex = activeSubMenuCount;
        }

        public static void ModifyMoneyMenu(string submenuName)
        {
            for (int i = -9999; i <= 9999; i++)
            {
                if (i == 0) continue; // Skip zero

                int index = i;
                string label = index > 0 ? $"Give {index}$" : $"Take {Math.Abs(index)}$";
                unifiedMenuOptions.Add(new MenuOption(label, () => PlayerInventory.Instance.cashInstance.ChangeBalance(index)));
            }

            // Update submenu count
            activeSubMenuCount = unifiedMenuOptions.Count;
            MenuTotalIndex = activeSubMenuCount;
            submenuCounts[submenuName] = activeSubMenuCount;
        }
        public static void loadPrefabsMenu(string submenuName)
        {
            // Remove previously added entries for this submenu, if tracked
            if (submenuCounts.TryGetValue(submenuName, out int previousCount))
            {
                int startIndex = unifiedMenuOptions.Count - previousCount;
                if (startIndex >= 0)
                    unifiedMenuOptions.RemoveRange(startIndex, previousCount);
            }

            int addedCount = 0;
            var loadedModels = _afterlifeAssetLoader._afterlifeBundleLoader.LoadedBundles;

            foreach (var kvp in loadedModels)
            {
                string modelName = kvp.Key;

                // Prevent duplicates
                if (unifiedMenuOptions.Any(option => option.Label == modelName))
                    continue;

                string prefabPath = $"assets/assetbundlepacks/{modelName}/source/{modelName}.prefab";

                // Load rotation and scale
                _afterlifeAssetLoader._afterlifeBundleLoader.PrefabRotations.TryGetValue(modelName.ToLowerInvariant(), out Quaternion rotation);
                _afterlifeAssetLoader._afterlifeBundleLoader.PrefabScales.TryGetValue(modelName.ToLowerInvariant(), out Vector3 scale);

                bool hasRotation = _afterlifeAssetLoader._afterlifeBundleLoader.PrefabRotations.ContainsKey(modelName.ToLowerInvariant());
                bool hasScale = _afterlifeAssetLoader._afterlifeBundleLoader.PrefabScales.ContainsKey(modelName.ToLowerInvariant());

                if (!hasRotation)
                    MelonLogger.Warning($"⚠️ Rotation not found for model '{modelName}', using default rotation.");
                if (!hasScale)
                    MelonLogger.Warning($"⚠️ Scale not found for model '{modelName}', using default scale.");

                unifiedMenuOptions.Add(new MenuOption(modelName, () =>
                    _afterlifeAssetLoader._afterlifeBundleLoader.InstantiateByFullAssetNameAtLookOrMouse(
                        prefabPath,
                        rotateObj: hasRotation,
                        rotation: hasRotation ? (Quaternion?)rotation : null,
                        scale: hasScale ? (Vector3?)scale : null
                    )));

                addedCount++;
            }

            // Update submenu count
            activeSubMenuCount = unifiedMenuOptions.Count;
            MenuTotalIndex = activeSubMenuCount;
            submenuCounts[submenuName] = addedCount;
        }

    }
}
