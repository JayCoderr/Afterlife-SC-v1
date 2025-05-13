using _clientids;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.ImageConversion;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerScripts.Health;
using static _afterlifeMod._functions;
using static _afterlifeMod._functions.UnlimitedAmmoPatch;
using static _afterlifeMod._clientids;
using static _clientids._menuStructure;
using static _afterlifeScModMenu._globalVariables;
using static _afterlifeScModMenu._mainMenu;
using _afterlifeScModMenu;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;
using static _afterlifeScModMenu._generatedSubMenus;
using static _clientids._hudelements;
using static UnityEngine.Time;
using static UnityEngine.Networking.WebRequestWWW;
using static System.Exception;
using static System.Convert;
using static UnityEngine.GUI.WindowFunction;
using static UnityEngine.GUI;
using static _clientids._airesponses;
using static UnityEngine.Networking.UnityWebRequest;
using static UnityEngine.Screen;
using static MelonLoader.Utils.MelonEnvironment;
using static UnityEngine.Input;
using static System.IO.Path;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.FileStream;
using static UnityEngine.GUIUtility;
using static UnityEngine.Color;

namespace _afterlifeMod
{
    public abstract class MenuElement
    {
        public string Label { get; set; }
    }
    public class MenuOption : MenuElement
    {
        public new string Label { get; set; }
        public Action<object[]> ActionWithArgs { get; set; }
        public object[] Arguments { get; set; }

        // Constructor for parameterless actions
        public MenuOption(string label, Action action)
        {
            Label = label;
            ActionWithArgs = args => action();
            Arguments = Array.Empty<object>();
        }

        // Constructor for actions with arguments
        public MenuOption(string label, Action<object[]> actionWithArgs, params object[] args)
        {
            Label = label;
            ActionWithArgs = actionWithArgs;
            Arguments = args;
        }

        public void Invoke()
        {
            ActionWithArgs?.Invoke(Arguments);
        }
    }

    public class MenuTextBox : MenuElement
    {
        public object Value { get; private set; }  // Can be string, int, float, etc.
        public System.Action<object> OnValueChanged { get; set; }

        private string previewOverride;
        private Type targetType;

        public MenuTextBox(string label, object initialValue, System.Action<object> onValueChanged)
        {
            Label = label;
            Value = initialValue;
            OnValueChanged = onValueChanged;
            targetType = initialValue?.GetType() ?? typeof(string);
            previewOverride = null;
        }

        public void SetValue(object newValue)
        {
            if (newValue == null) return;

            object parsedValue = newValue;

            if (newValue is string strValue)
            {
                parsedValue = ConvertInput(strValue, targetType);
            }

            if (!object.Equals(Value, parsedValue)) // Safe null check
            {
                Value = parsedValue;
                OnValueChanged?.Invoke(parsedValue);
            }
        }

        public static object ConvertInput(string input, Type type)
        {
            try
            {
                if (type == typeof(Vector3))
                {
                    string[] parts = input.Split(',');

                    if (parts.Length == 3)
                    {
                        if (float.TryParse(parts[0], out float x) &&
                            float.TryParse(parts[1], out float y) &&
                            float.TryParse(parts[2], out float z))
                        {
                            return new Vector3(x, y, z);
                        }
                        else
                        {
                            // Log the error if the parsing of the Vector3 fails
                            _afterlifeConsole($"Failed to parse Vector3 from input: {input}");
                            return Vector3.zero;  // Return default Vector3 if parsing fails
                        }
                    }
                    else
                    {
                        // Handle case where input doesn't have exactly 3 parts
                        _afterlifeConsole($"Invalid Vector3 input, must be in the format 'x,y,z': {input}");
                        return Vector3.zero;  // Return default Vector3 if the format is incorrect
                    }
                }

                if (type == typeof(float))
                    return float.TryParse(input, out var f) ? f : 0f;

                if (type == typeof(int))
                    return int.TryParse(input, out var i) ? i : 0;

                if (type == typeof(bool))
                    return bool.TryParse(input, out var b) ? b : false;

                // Handle string input, removing quotes
                if (type == typeof(string) && input.StartsWith("\"") && input.EndsWith("\""))
                    return input.Substring(1, input.Length - 2);

                // Fallback for other types
                return ChangeType(input, type);  // Fallback conversion for other types
            }
            catch (Exception ex)
            {
                _afterlifeConsole($"Error in ConvertInput: {ex.Message}");
                return input;  // Fallback to raw string if an error occurs
            }
        }

        public void OverrideDisplay(string previewText)
        {
            previewOverride = previewText;
        }

        public void ClearOverrideDisplay()
        {
            previewOverride = null;
        }

        public string GetDisplayText()
        {
            return previewOverride ?? Value?.ToString() ?? string.Empty;
        }
    }

    public class _clientids
    {
        public static void Init()
        {
            if (isMenuOpen)
            {
                windowRect = Window(i, windowRect, (WindowFunction)ModMenu, "", GUIStyle.none);

                void ModMenu(int windowID)
                {
                    // Set the alpha value for the background image.
                    float alphaValue = 0.95f; // Set your desired alpha value here
                    Color originalColor = color;
                    color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);

                    // Ensure the window background has been loaded
                    if (windowBackground != null)
                    {
                        // Draw the background texture on the window
                        DrawTexture(new Rect(0 - 10, 0 - 100, windowRect.width, windowRect.height), windowBackground);
                    }

                    // Reset the GUI color to its original value
                    color = originalColor;

                    // Create a new GUIStyle for the Scrollbar
                    GUIStyle Scrollbar = new GUIStyle(skin.box);

                    // Check if the custom color is set. If not, set it to your desired color.
                    if (customColor == clear) // You can use any default value check
                    {
                        customColor = SetScrollerColor(0.0f, 0.1f, 0.0f, 1f);
                    }

                    Scrollbar.normal.background = MakeTexture(2, 2, customColor); // Use the custom color

                    // Draw the scrollbar
                    Box(new Rect(ScrollbarXValue, ScrollbarYValue + ScrollbarInitPos, _globalVariables.width, 20), "", Scrollbar);

                    MenuStructure();
                    DragWindow(new Rect(0, 0, 10000, 10000));
                }
            }
            MenuSceneElements();
        }

        public static void MenuSceneElements()
        {
            // Ensure that we are in the "Menu" scene
            if (sceneStaticName == "Menu")
            {
                // Fade-in effect when the texture is loaded
                if (!textureLoaded)
                {
                    string textureUrl = "https://iili.io/3aatyAu.png"; // The texture URL
                    string textureUrl2 = "https://iili.io/3cdpxpt.png";
                    _afterlifeCoroutinesStart(DownloadTexture(textureUrl, textureUrl2));
                }
                else if (textureLoaded && loadedTexture != null)
                {
                    // Start the fade-in if not already happening
                    if (!isFadingIn)
                    {
                        isFadingIn = true;
                        _afterlifeCoroutinesStart(FadeIn());
                    }

                    // Ensure MOTD is loaded before displaying it
                    if (!isMOTDLoaded)
                    {
                        // Start the task to get the message of the day
                        _afterlifeCoroutinesStart(LoadMOTD());
                    }

                    // Create a rectangle for the background and text
                    Rect backgroundRect = new Rect(ScalePosition(1400), ScalePosition(140), 440, 280);
                    Rect textRect = new Rect(ScalePosition(1440), ScalePosition(200), 440, 280);

                    Rect backgroundRectX = new Rect(ScalePosition(-30), ScalePosition(520), 300, 150);
                    Rect textRectX = new Rect(ScalePosition(50), ScalePosition(575), 440, 280);

                    // Draw the texture with the alpha fade effect
                    color = new Color(1f, 1f, 1f, fadeAlpha);


                    depth = -1;
                    DrawTexture(backgroundRect, loadedTexture);
                    DrawTexture(backgroundRectX, loadedTextureX);

                    depth = 0;

                    GUIStyle textStyle = new GUIStyle(skin.label);
                    textStyle.wordWrap = true;
                    textStyle.richText = true;
                    textStyle.normal.textColor = white;
                    textStyle.alignment = TextAnchor.UpperLeft;

                    string richText = MOTD.Replace("{newline}", "\n").Replace("<br>", "\n");

                    Label(textRect, richText, textStyle);
                    string richTextX = $"\n\n\n\n<b>{versionResponse}</b>\r\n\r\n".Replace("{newline}", "\n").Replace("<br>", "\n");

                    Label(textRectX, richTextX, textStyle);

                    color = white;
                }
                else
                {
                    GUIStyle loadingStyle = new GUIStyle(skin.label);
                    loadingStyle.normal.textColor = white;
                    Label(new Rect(140, 140, 300, 100), "Loading Texture...", loadingStyle);
                }
            }
            else
            {
                // Start fade-out effect when leaving the "Menu" scene
                if (!isFadingOut)
                {
                    isFadingOut = true;
                    _afterlifeCoroutinesStart(FadeOut());
                }
            }
        }

        public static IEnumerator LoadMOTD()
        {
            // Start the task but don't await yet
            var task = GetScheduleIAIResponse(messageOfTheDayString, motdAISelector, actionToTake);

            // Wait until the task is completed manually
            while (!task.IsCompleted)
            {
                yield return null;  // Keep yielding until the task completes
            }

            // Handle the task completion
            if (task.Status == TaskStatus.RanToCompletion)
            {
                MOTD = task.Result;
            }
            else
            {
                MOTD = failedMOTD;
            }

            isMOTDLoaded = true;
        }
        private static IEnumerator FadeIn()
        {
            while (fadeAlpha < 1f)
            {
                fadeAlpha += deltaTime * fadeSpeed; // Increase the alpha over time
                yield return null; // Wait for the next frame
            }
            fadeAlpha = 1f; // Ensure it's fully opaque
            isFadingIn = false; // Finish the fade-in
        }
        private static IEnumerator FadeOut()
        {
            while (fadeAlpha > 0f)
            {
                fadeAlpha -= deltaTime * fadeSpeed; // Decrease the alpha over time
                yield return null; // Wait for the next frame
            }
            fadeAlpha = 0f; // Ensure it's fully transparent
            isFadingOut = false; // Finish the fade-out
        }
        private static IEnumerator DownloadTexture(string url, string url2)
        {
            // Check if first texture is cached
            if (cachedTexture1 == null)
            {
                // Download the first texture
                UnityWebRequest uwr1 = Get(url);
                uwr1.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr1.SendWebRequest();

                if (uwr1.result == Result.Success)
                {
                    byte[] imageData = uwr1.downloadHandler.data;

                    loadedTexture = new Texture2D(2, 2);
                    loadedTexture.LoadImage(imageData); // Load the first texture into loadedTexture

                    cachedTexture1 = loadedTexture; // Cache the first texture

                    _afterlifeConsole("✅ First texture loaded and cached!");
                }
                else
                {
                    _afterlifeConsole($"❌ Failed to load first texture: {uwr1.error}");
                    yield break;
                }
            }
            else
            {
                // If the first texture is cached, use the cached version
                loadedTexture = cachedTexture1;
                _afterlifeConsole("✅ Using cached first texture.");
            }

            // Check if second texture is cached
            if (cachedTexture2 == null)
            {
                // Download the second texture
                UnityWebRequest uwr2 = Get(url2);
                uwr2.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr2.SendWebRequest();

                if (uwr2.result == Result.Success)
                {
                    byte[] imageData2 = uwr2.downloadHandler.data;

                    loadedTextureX = new Texture2D(2, 2);
                    loadedTextureX.LoadImage(imageData2); // Load the second texture into loadedTextureX

                    cachedTexture2 = loadedTextureX; // Cache the second texture

                    textureLoaded = true;
                    _afterlifeConsole("✅ Second texture loaded and cached!");
                }
                else
                {
                    _afterlifeConsole($"❌ Failed to load second texture: {uwr2.error}");
                }
            }
            else
            {
                // If the second texture is cached, use the cached version
                loadedTextureX = cachedTexture2;
                textureLoaded = true;
                _afterlifeConsole("✅ Using cached second texture.");
            }
        }

        private static float ScalePosition(float position)
        {
            // Get the current screen width and height
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Scale the position based on the resolution (relative to the base resolution, e.g., 1920x1080)
            float baseWidth = 1920f;  // Example base resolution width
            float baseHeight = 1080f; // Example base resolution height

            // Scale position based on the ratio of the current screen size to the base resolution
            float xScale = screenWidth / baseWidth;
            float yScale = screenHeight / baseHeight;

            return position * xScale; // This scales both X and Y uniformly (if you want both to scale similarly)
        }
        public static Color ScrollerColor;

        public static void LoadMenuConfigDesign()
        {
            LoadMenuConfigDesignInternal(null, null);
        }

        public static void LoadMenuConfigDesign(string newBackground)
        {
            LoadMenuConfigDesignInternal(newBackground, null);
        }

        public static void LoadMenuConfigDesign(string newBackground, string newScrollerColor)
        {
            LoadMenuConfigDesignInternal(newBackground, newScrollerColor);
        }

        private static void LoadMenuConfigDesignInternal(string newBackground, string newScrollerColor)
        {
            string configDir = ModsDirectory;
            string configPath = Combine(configDir, "MenuConfig.txt");

            if (!File.Exists(configDir))
            {
                CreateDirectory(configDir);
                _afterlifeConsole($"📁 Created Mods directory at: {configDir}");
            }

            // Default config values
            string defaultBackground = "https://iili.io/3aYpopn.png";
            string defaultScroller = "0.0,0.1,0.0,1.0";

            // Create default config if missing
            if (!File.Exists(configPath))
            {
                WriteAllText(configPath, $"MenuBackground={defaultBackground}\nScrollerColor={defaultScroller}");
                _afterlifeConsole("📄 MenuConfig.txt was missing and has been created with default values.");
            }

            Dictionary<string, string> config = new Dictionary<string, string>();

            foreach (var line in ReadAllLines(configPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    config[key] = value;
                }
            }

            if (newBackground != null)
                config["MenuBackground"] = newBackground;

            if (newScrollerColor != null)
                config["ScrollerColor"] = newScrollerColor;

            // Save updated config
            List<string> newLines = new List<string>();
            foreach (var kvp in config)
            {
                newLines.Add(kvp.Key + "=" + kvp.Value);
            }
            WriteAllLines(configPath, newLines.ToArray());

            // Apply values
            if (config.ContainsKey("MenuBackground"))
            {
                MenuBackground = config["MenuBackground"];
            }

            if (config.ContainsKey("ScrollerColor"))
            {
                string[] rgba = config["ScrollerColor"].Split(',');
                if (rgba.Length == 4 &&
                    float.TryParse(rgba[0], out float r) &&
                    float.TryParse(rgba[1], out float g) &&
                    float.TryParse(rgba[2], out float b) &&
                    float.TryParse(rgba[3], out float a))
                {
                    ScrollerColor = new Color(r, g, b, a);
                    SetScrollerColor(r, g, b, a);
                }
            }

            _afterlifeConsole("🎨 Menu background set to: " + MenuBackground);
            _afterlifeConsole("🎨 Scroller color set to: " + ScrollerColor);
        }
        public static void LoadMenuConfigDesignWithScroller(string newScrollerColor)
        {
            LoadMenuConfigDesignInternal(null, newScrollerColor);
        }

        public static Color SetScrollerColor(float r, float g, float b, float a)
        {
            // Set and return the custom color
            customColor = new Color(r, g, b, a);
            return customColor;
        }
        public static Color SetScrollerColorX(Color color)
        {
            customColor = color;
            return color;
        }
        public static void MenuControls()
        {
            // Toggle the menu on F1 key press
            if (GetKeyDown(KeyCode.F1))
            {
                isMenuOpen = !isMenuOpen;

                if (!isCoroutineRunning && !isDownloadingTexture)
                {
                    LoadMenuConfigDesign();
                    MenuHuds(isMenuOpen);
                    _afterlifeConsole($"Starting download once. {isMenuOpen}");
                }
            }

            // Only allow navigation if not editing text
            if (!isEditingTextBox)
            {
                if (GetKeyDown(KeyCode.UpArrow) && isMenuOpen)
                {
                    MenuStartIndex = (MenuStartIndex > 0) ? MenuStartIndex - 1 : activeSubMenuCount - 1;
                }
                else if (GetKeyDown(KeyCode.DownArrow) && isMenuOpen)
                {
                    MenuStartIndex = (MenuStartIndex < activeSubMenuCount - 1) ? MenuStartIndex + 1 : 0;
                }
            }
            // Press Return to activate menu element
            if (GetKeyDown(KeyCode.Return) && isMenuOpen)
            {
                if (MenuStartIndex >= 0 && MenuStartIndex < unifiedMenuOptions.Count)
                {
                    var selectedElement = unifiedMenuOptions[MenuStartIndex];

                    if (selectedElement is MenuTextBox textBox)
                    {
                        if (!isEditingTextBox)
                        {
                            // Start typing
                            isEditingTextBox = true;
                            // Initialize currentInput based on the current value of the textbox
                            currentInput = textBox.GetDisplayText();
                        }
                        else
                        {
                            // Submit and stop typing
                            isEditingTextBox = false;

                            // Use the label as the placeholder if no input was given
                            object submittedValue = string.IsNullOrWhiteSpace(currentInput) ? textBox.Label : currentInput;

                            // Check if input is surrounded by quotes (indicating a string)
                            if (submittedValue is string strInput)
                            {
                                // Detect if the string starts and ends with quotes
                                if (strInput.StartsWith("\"") && strInput.EndsWith("\""))
                                {
                                    // Remove the quotes for a string input
                                    submittedValue = strInput.Substring(1, strInput.Length - 2); // Keep it as string but remove quotes
                                }
                                else
                                {
                                    // Handle float detection
                                    if (strInput.EndsWith("f"))
                                    {
                                        // Remove the 'f' and try parsing as float
                                        strInput = strInput.Substring(0, strInput.Length - 1);
                                    }

                                    // Try to parse the input as a float
                                    if (float.TryParse(strInput, out float floatValue))
                                    {
                                        submittedValue = floatValue;
                                    }
                                    else if (int.TryParse(strInput, out int intValue))
                                    {
                                        // If it's not a float, try parsing as an integer
                                        submittedValue = intValue;
                                    }
                                    else if (TryParseVector3(strInput, out Vector3 vectorValue))
                                    {
                                        // If the string represents a Vector3, try parsing it
                                        submittedValue = vectorValue;
                                    }
                                }
                            }

                            // Set the value of the textbox (this will trigger the callback)
                            textBox.SetValue(submittedValue);
                            _afterlifeConsole($"TextBox '{textBox.Label}' submitted: {textBox.GetDisplayText()}");

                            currentInput = "";
                        }
                    }
                    else if (selectedElement is MenuOption option)
                    {
                        // Invoke the action with the correct arguments (which are stored in option.Arguments)
                        option.ActionWithArgs?.Invoke(option.Arguments);
                    }

                }
            }
            if (isEditingTextBox && GetKeyDown(KeyCode.V) && (GetKey(KeyCode.LeftControl) || GetKey(KeyCode.RightControl)))
            {
                string pastedText = systemCopyBuffer;
                currentInput += pastedText;

                // Update visual
                if (MenuStartIndex >= 0 && MenuStartIndex < unifiedMenuOptions.Count &&
                    unifiedMenuOptions[MenuStartIndex] is MenuTextBox textBox)
                {
                    textBox.OverrideDisplay(currentInput);
                }

                _afterlifeConsole($"Pasted text: {pastedText}");
            }

            // Handle typing input while editing a textbox
            // Check for paste or Ctrl+A
            if (isEditingTextBox)
            {
                // Paste support
                if (GetKeyDown(KeyCode.V) && (GetKey(KeyCode.LeftControl) || GetKey(KeyCode.RightControl)))
                {
                    string pastedText = systemCopyBuffer;
                    currentInput = isSelectAll ? pastedText : currentInput + pastedText;
                    isSelectAll = false;
                }

                // Ctrl+A support
                if (GetKeyDown(KeyCode.A) && (GetKey(KeyCode.LeftControl) || GetKey(KeyCode.RightControl)))
                {
                    isSelectAll = true;
                    _afterlifeConsole("Select All triggered");
                }

                foreach (char c in inputString)
                {
                    _afterlifeConsole($"Char input: {c}");

                    if (c == '\b') // Backspace
                    {
                        if (currentInput.Length > 0)
                        {
                            if (isSelectAll)
                            {
                                currentInput = "";
                                isSelectAll = false;
                            }
                            else
                            {
                                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                            }
                        }
                    }
                    else if (c != '\n' && c != '\r') // Regular character
                    {
                        if (isSelectAll)
                        {
                            currentInput = c.ToString(); // Replace all
                            isSelectAll = false;
                        }
                        else
                        {
                            currentInput += c;
                        }
                    }
                }

                // Optional: visually update the MenuTextBox text while typing
                if (MenuStartIndex >= 0 && MenuStartIndex < unifiedMenuOptions.Count &&
                    unifiedMenuOptions[MenuStartIndex] is MenuTextBox textBox)
                {
                    textBox.OverrideDisplay(currentInput);
                }
            }
        }
        public static bool TryParseVector3(string input, out Vector3 result)
        {
            result = default;

            // Trim spaces and check if it contains 3 values separated by commas
            var parts = input.Trim().Split(',');

            if (parts.Length == 3)
            {
                // Try to parse each part as a float
                if (float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y) &&
                    float.TryParse(parts[2], out float z))
                {
                    result = new Vector3(x, y, z);
                    return true;
                }
            }

            return false;
        }

        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public static void MenuHuds(bool isMenuOpen)
        {
            _afterlifeCoroutinesStart(CreateRectangle(140, 140, windowRect.height, windowRect.width, MenuBackground, 0, isMenuOpen));
        }

        public static void CreateSubMenu(string submenuName, int? submenuCount)
        {
            menunameX = submenuName;
            NpcNameSubMenu = submenuName;

            if (menuMap.TryGetValue(submenuName, out var submenu))
            {
                unifiedMenuOptions = submenu;
                MenuStartIndex = 0; // Reset the scroll/index state

                // ✅ Track the submenu count globally (if valid)
                if (submenuCount.HasValue)
                {
                    if (submenuCounts.ContainsKey(submenuName))
                        submenuCounts[submenuName] = submenuCount.Value;
                    else
                        submenuCounts.Add(submenuName, submenuCount.Value);

                    // ✅ Use it to update menu states
                    activeSubMenuCount = submenuCount.Value;
                    MenuTotalIndex = activeSubMenuCount;
                }
                else
                {
                    // If submenuCount is null, set it to a default value or skip updating
                    activeSubMenuCount = unifiedMenuOptions.Count;
                    MenuTotalIndex = activeSubMenuCount;
                }
            }
            switch (submenuName)
            {
                case "Sounds Menu":
                    ModifyAudioMenu(submenuName);
                    break;
                case "Modify Cash":
                    ModifyMoneyMenu(submenuName);
                    break;
                case "Loaded Prefabs":
                    loadPrefabsMenu(submenuName);
                    break;
                default:
                    loadPlayersByNameMenu(submenuName);
                    break;
            }
        }

        public static void MenuStructure()
        {
            // Update cursor blink
            if (time - lastBlinkTime > blinkInterval)
            {
                showCursor = !showCursor;
                lastBlinkTime = time;
            }

            if (!menuInitialized)
            {
                CreateSubMenu(menuName, 21);  // Example: passing 5 as the custom submenu count
                menuInitialized = true;
            }

            CreateTitle(20, menunameX, 0, white);

            float baseY = 60;

            for (int i = 0; i < MenuMaxIndex && i < activeSubMenuCount; i++) // Use activeSubMenuCount instead of MenuTotalIndex
            {
                int wrappedIndex = (MenuStartIndex + i) % activeSubMenuCount; // Wrap around based on activeSubMenuCount
                float fixedY = baseY + (i * 20);

                var menuItem = unifiedMenuOptions[wrappedIndex];

                if (menuItem is MenuOption option)
                {
                    // Handle MenuOption type (Label + ActionWithArgs)
                    string text = option.Label ?? "Unnamed Option"; // Default text if null

                    // Convert ActionWithArgs to ButtonAction if it's not null
                    ButtonAction onClick = option.ActionWithArgs != null ? new ButtonAction(() => option.ActionWithArgs.Invoke(option.Arguments)) : null;

                    // Create text with the wrapped action
                    CreateText(fixedY, text, onClick, wrappedIndex);
                }

                else if (menuItem is MenuTextBox textBox)
                {
                    // Handle MenuTextBox type (Label + Text)
                    string displayText = textBox.GetDisplayText();
                    string cursor = showCursor && isEditingTextBox && MenuStartIndex == wrappedIndex ? "|" : " "; // Blink only for current textbox
                    string text = textBox.Label + ": " + displayText + cursor;

                    CreateText(fixedY, text, null, wrappedIndex);  // No action needed for text box
                }
            }
            CreateTitle(350, $"Created By: {name}", 0, white);
        }

        public static object ConvertInput(string input, Type type)
        {
            try
            {
                if (type == typeof(Vector3))
                {
                    string[] parts = input.Split(',');

                    if (parts.Length == 3)
                    {
                        if (float.TryParse(parts[0], out float x) &&
                            float.TryParse(parts[1], out float y) &&
                            float.TryParse(parts[2], out float z))
                        {
                            return new Vector3(x, y, z);
                        }
                        else
                        {
                            // Log the error if the parsing of the Vector3 fails
                            _afterlifeConsole($"Failed to parse Vector3 from input: {input}");
                            return Vector3.zero;  // Return default Vector3 if parsing fails
                        }
                    }
                    else
                    {
                        // Handle case where input doesn't have exactly 3 parts
                        _afterlifeConsole($"Invalid Vector3 input, must be in the format 'x,y,z': {input}");
                        return Vector3.zero;  // Return default Vector3 if the format is incorrect
                    }
                }

                if (type == typeof(float))
                    return float.TryParse(input, out var f) ? f : 0f;

                if (type == typeof(int))
                    return int.TryParse(input, out var i) ? i : 0;

                if (type == typeof(bool))
                    return bool.TryParse(input, out var b) ? b : false;

                // Handle string input, removing quotes
                if (type == typeof(string) && input.StartsWith("\"") && input.EndsWith("\""))
                    return input.Substring(1, input.Length - 2);

                // Fallback for other types
                return ChangeType(input, type);  // Fallback conversion for other types
            }
            catch (Exception ex)
            {
                _afterlifeConsole($"Error in ConvertInput: {ex.Message}");
                return input;  // Fallback to raw string if an error occurs
            }
        }
    }
}