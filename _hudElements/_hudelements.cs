using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _afterlifeMod._clientids;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using static _afterlifeScModMenu._globalVariables;
using static UnityEngine.Networking.UnityWebRequest;
using static UnityEngine.Object;
using static _afterlifeScModMenu.AfterlifeConsoleMsg;
using static UnityEngine.GUIStyle;
using static UnityEngine.GUI;
using static UnityEngine.Color;

namespace _clientids
{
    public static class _hudelements
    {
        public static IEnumerator CreateRectangle(float x, float y, float w, float h, string url, int i, bool isTextureLoaded = true)
        {
            if (isCoroutineRunning || isDownloadingTexture)
                yield break;

            isCoroutineRunning = true;
            isDownloadingTexture = true;

            try
            {
                if (isTextureLoaded)
                    showWindow = true;

                UnityWebRequest uwr = Get(url);
                uwr.downloadHandler = new DownloadHandlerBuffer();

                yield return uwr.SendWebRequest();

                if (uwr.result == Result.Success)
                {
                    byte[] imageData = uwr.downloadHandler.data;

                    if (windowBackground != null)
                        Destroy(windowBackground);

                    Texture2D newTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (newTexture.LoadImage(imageData))
                    {
                        windowBackground = newTexture;

                        // Only set position if windowRect is not initialized (e.g. width is zero)
                        if (windowRect.width == 0 && windowRect.height == 0)
                        {
                            windowRect = new Rect(x, y, w, h);
                        }

                        styleNeedsUpdate = true;
                        showWindow = true;
                    }
                    else
                    {
                        _afterlifeConsole("❌ Failed to load texture from image data.");
                    }
                }
                else
                {
                    _afterlifeConsole($"❌ Failed to download texture: {uwr.error}");
                }
            }
            finally
            {
                isDownloadingTexture = false;
                isCoroutineRunning = false;
            }
        }
        public static void CreateText(float y, string text, ButtonAction onClick, int index)
        {
            // Ensure buttons list contains only the relevant actions for the current frame
            if (buttons.Count <= index)
            {
                buttons.Add(onClick); // Add the action only if it's not already in the list
            }

            GUIStyle buttonStyle = new GUIStyle(none);
            buttonStyle.normal.textColor = white;
            buttonStyle.hover.textColor = white;
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.richText = true;

            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(20, 0, clear);
            backgroundTexture.Apply();

            Texture2D scrollbarTexture = new Texture2D(1, 1);
            scrollbarTexture.SetPixel(20, 0, clear);
            scrollbarTexture.Apply();

            buttonStyle.normal.background = backgroundTexture;
            buttonStyle.hover.background = scrollbarTexture;

            // Make sure onClick is not null before trying to invoke it
            if (Button(new Rect(20, y, 195, 20), text, buttonStyle))
            {
                if (onClick != null)
                {
                    // Execute the action when the button is clicked
                    onClick();
                }
                else
                {
                    // Optionally, log a message if onClick is null
                    _afterlifeConsole("onClick is null for button: " + text);
                }
            }

            depth = index;
        }
        public static void CreateTitle(float y, string Titletext, int i, Color? TitleColor)
        {
            GUIStyle createTitle = new GUIStyle(skin.label);
            createTitle.normal.textColor = (Color)TitleColor;
            createTitle.hover.textColor = black;
            createTitle.normal.background = null;

            Label(new Rect(20, y, windowRect.width, windowRect.height), Titletext, createTitle);
            depth = i;
        }
        public static void CreateScreenText(float x, float y, string Titletext, int i, Color? TitleColor)
        {
            GUIStyle createTitle = new GUIStyle(skin.label);
            createTitle.normal.textColor = (Color)TitleColor;
            createTitle.hover.textColor = black;
            createTitle.normal.background = null;

            Label(new Rect(x, y, windowRect.width, windowRect.height), Titletext, createTitle);
            depth = i;
        }
    }
}
