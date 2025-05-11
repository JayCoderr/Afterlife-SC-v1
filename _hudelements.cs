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

namespace _clientids
{
    internal class _hudelements
    {
        public static bool showWindow = false;
        public static bool isDownloadingTexture = false;
        //public static bool isTextureLoaded = false;
        public static GUIStyle dynamicStyle = null;
        public static bool styleNeedsUpdate = false;
        public static bool isCoroutineRunning = false;
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

                UnityWebRequest uwr = UnityWebRequest.Get(url);
                uwr.downloadHandler = new DownloadHandlerBuffer();

                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] imageData = uwr.downloadHandler.data;

                    if (windowBackground != null)
                        UnityEngine.Object.Destroy(windowBackground);

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
                        MelonLogger.Warning("❌ Failed to load texture from image data.");
                    }
                }
                else
                {
                    MelonLogger.Warning($"❌ Failed to download texture: {uwr.error}");
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

            GUIStyle buttonStyle = new GUIStyle(GUIStyle.none);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = Color.white;
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.richText = true;

            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(20, 0, Color.clear);
            backgroundTexture.Apply();

            Texture2D scrollbarTexture = new Texture2D(1, 1);
            scrollbarTexture.SetPixel(20, 0, Color.clear);
            scrollbarTexture.Apply();

            buttonStyle.normal.background = backgroundTexture;
            buttonStyle.hover.background = scrollbarTexture;

            // Make sure onClick is not null before trying to invoke it
            if (GUI.Button(new Rect(20, y, 195, 20), text, buttonStyle))
            {
                if (onClick != null)
                {
                    // Execute the action when the button is clicked
                    onClick();
                }
                else
                {
                    // Optionally, log a message if onClick is null
                    Debug.LogWarning("onClick is null for button: " + text);
                }
            }

            GUI.depth = index;
        }
        public static void CreateTitle(float y, string Titletext, int i, Color? TitleColor)
        {
            GUIStyle createTitle = new GUIStyle(GUI.skin.label);
            createTitle.normal.textColor = (Color)TitleColor;
            createTitle.hover.textColor = Color.black;
            createTitle.normal.background = null;

            GUI.Label(new Rect(20, y, windowRect.width, windowRect.height), Titletext, createTitle);
            GUI.depth = i;
        }
        public static void CreateScreenText(float x, float y, string Titletext, int i, Color? TitleColor)
        {
            GUIStyle createTitle = new GUIStyle(GUI.skin.label);
            createTitle.normal.textColor = (Color)TitleColor;
            createTitle.hover.textColor = Color.black;
            createTitle.normal.background = null;

            GUI.Label(new Rect(x, y, windowRect.width, windowRect.height), Titletext, createTitle);
            GUI.depth = i;
        }
    }
}
