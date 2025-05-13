using _clientids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MelonLoader.Utils.MelonEnvironment;
using static System.IO.Path;
using static System.IO.Directory;
using Il2CppScheduleOne.PlayerScripts;
using _afterlifeMod;
using UnityEngine;
using MelonLoader;
using static MelonLoader.MelonLogger.Instance;
using Il2CppScheduleOne.Police;
using System.Threading;
using static UnityEngine.GameObject;
using static UnityEngine.Color;
using Il2CppSystem.Net.Http;

namespace _afterlifeScModMenu
{
    public static class _globalVariables
    {
        public static string versionResponse = "";
        public static string PostOffice = "";
        public static string PostOfficeXFloat = "";
        public static string PostOfficeYFloat = "";
        public static string PostOfficeZFloat = "";
        public static string currentSongPath = null;
        public static object playingCoroutine = null;
        public static bool _npcSpawningInProgress = false;
        public static System.Random sysRand = new System.Random();
        public static string randomName = _allnpcs.allNpcCharacters[sysRand.Next(_allnpcs.allNpcCharacters.Length)];
        public static string[] namesX = { "PostOffice", "Barn" };
        public static string randomNameX = namesX[sysRand.Next(namesX.Length)];
        public static string AfterlifeModsDir = Combine(ModsDirectory, "AfterlifeMusic");
        public static string[] mp3Files = GetFiles(AfterlifeModsDir, "*.mp3");
        public static string sceneStaticName = "";
        public static bool forgeModeBool = false;
        public static bool forgeModeActive = false;
        public static bool lastForgeModeLogged = false;
        public static Player TargetPlayer;
        public static bool showGUIPlayer = false;
        public static Rect windowRect = new Rect(140f, 140f, 240f, 597f);
        public static Texture2D windowBackground;
        public static Texture2D MenuHud;
        public static bool isLoadingTexture = false;
        public static Camera GameCamera => Camera.main;
        public static bool isMenuOpen;
        public static bool AimbotIsOn;
        public static float ScrollbarYValue;
        public static int ScrollbarInitPos = 60;
        public static float Recty = 0;
        public static string menunameX = "Afterlife SC v1";
        public static bool isClicked;
        public delegate void ButtonAction();
        readonly public static Dictionary<ButtonAction, Rect> buttonRects = new Dictionary<ButtonAction, Rect>();
        readonly public static List<ButtonAction> buttons = new List<ButtonAction>();
        public static float ScrollbarXValue = 11f;
        public static int MenuStartIndex = 0;
        public static int MenuMidIndex = 6;
        public readonly static int buttonIndex = 0;
        public static int i;
        public static string P = "P";
        public static string r = "r";
        public static string o = "o";
        public static string j = "j";
        public static string e = "e";
        public static string c = "c";
        public static string t = "t";
        public static string v = "V";
        public static string oo = "o";
        public static string ii = "i";
        public static string d = "d";
        public static string vv = "v";
        public static string v1 = "1";
        public static string name = "Jaycoder";
        public static string menuName = "Afterlife SC v1";
        public static int MenuTotalIndex = 26;
        public static int MenuMaxIndex = 14;
        public static int MenuExecuteIndex = 6;
        public static bool DragWindow = true;
        public static Texture2D loadedTexture = null;
        public static Texture2D loadedTextureX = null;
        public static bool textureLoaded = false;
        public static float width = 198f;
        public static Color customColor;
        public static Dictionary<string, List<MenuElement>> menuStructure = new Dictionary<string, List<MenuElement>>();
        public static RectTransform rectTransform;
        public static Vector2 offset;
        public static bool isDragging = false;
        public static GameObject foundObject = null;
        public static bool autoDeleteEnabled = false;
        public static List<GameObject> hiddenObjects = new List<GameObject>();
        public static bool isWideFOV = false;
        public static bool isRotatingToPoliceNPC = false;
        public static bool canDoubleJump = false;
        public static bool hasDoubleJumped = false;
        public static bool flyModeOption = true;
        public static bool flyModeActive = false;
        public static bool isFollowingMouse = false;
        public static object flyModeCoroutine;
        public static string GameObjectName = "";
        public static bool MoneyGun = false;
        public static bool _isFollowing = false;
        public static CancellationTokenSource _followTokenSource;
        public static GameObject revolver = GameObject.Find("Revolver(Clone)");
        public static bool BlackHoleEnabled = false;
        public static bool isRtsModeEnabled = false;
        public static bool discoSkyEnabled = false;
        public static object discoSkyCoroutine;
        public static bool GodModeEnabled = false;
        public static CancellationTokenSource godModeTokenSource;
        public static bool DemiGodModeEnabled = false;
        public static float? originalHealth = null;
        public static bool thirdPersonEnabled = false;
        public static Vector3 originalCamPosition;
        public static Quaternion originalCamRotation;
        public static int rotationIndex = 0;
        public static Quaternion originalRotation;
        public static readonly Quaternion[] customRotations = new Quaternion[]
        {
            Quaternion.Euler(0, 0, 0),          // Will be overwritten with original
            Quaternion.Euler(0, 45, 0),
            Quaternion.Euler(0, 90, 0),
            Quaternion.Euler(30, 180, 0),
            Quaternion.Euler(-15, 270, 10),
        };
        public static bool IsFogEnabled = false;
        public static Color CustomFogColor = red;
        public static bool FogJustUpdated = false; // <- new flag
        public static bool IsGreyScale = false;
        public static float CustomContrastFloat = -50f;
        public static bool IsBlur = false;  // Whether blur is enabled or not
        public static float CustomBlurScale = 5f; // Default blur scale value
        public static Coroutine afkKillRoutine;
        public static Quaternion originalCameraRotation;
        public static bool isDrunkCameraActive = false;
        public static Coroutine drunkRoutine;
        public static bool isLoaded = false;
        public static GameObject audioObj;
        public static AudioSource source;
        public static string currentTrack = null;
        public static bool isAudioPlaying = false;
        public static List<PoliceOfficer> officers = new List<PoliceOfficer>();
        public static float launchForce = 10f;
        public static bool espEnabled = false;
        public static GameObject _espObject;
        public static bool PlayerPassout = false;
        public static bool isCrosshairVisible = true;
        public static bool isInvisible = true;
        public static string dynamicPrefix = "[Player] ";
        public static string backLabel = "Back";
        public static string returnToMenu = "Afterlife SC v1"; 
        public static string allowedMenu = "Players Menu";
        public static string messageOfTheDayString = "what is the message of the day?";
        public static string motdAISelector = "system";
        public static string actionToTake = "daily update";
        public static string failedMOTD = "Failed to load message.";
        public static bool isFadingIn = false;
        public static bool isFadingOut = false;
        public static float fadeAlpha = 0f; // Start with transparent
        public static float fadeSpeed = 20f; // How fast the fade happens
        public static string MOTD = "Loading message..."; // Default loading text
        public static bool isMOTDLoaded = false;
        public static Texture2D cachedTexture1 = null; // Cache for first texture
        public static Texture2D cachedTexture2 = null; // Cache for second texture
        public static Dictionary<string, int> submenuCounts = new Dictionary<string, int>();
        public static int activeSubMenuCount = 0;
        public static bool isEditingTextBox = false;
        public static string currentInput = "";
        public static bool isSelectAll = false;
        public static Texture2D cachedTexture = null;
        public static bool isLoadingTextureX = false;
        public static string MenuBackground = "https://iili.io/3aYpopn.png";
        public static List<MenuElement> mainModsList = new List<MenuElement>();
        public static List<MenuOption> npcMenuOptions = new List<MenuOption>();
        public static List<MenuElement> unifiedMenuOptions = new List<MenuElement>();//currentSubMenu
        public static string NpcNameSubMenu = "";
        public static bool menuInitialized = false;
        public static bool showCursor = true;
        public static float lastBlinkTime = 0f;
        public static float blinkInterval = 0.5f;
        public static readonly HttpClient httpClient = new HttpClient();
        public static readonly Dictionary<string, List<string>> responseMap = new Dictionary<string, List<string>>();
        public static readonly System.Random random = new System.Random();
        public static bool showWindow = false;
        public static bool isDownloadingTexture = false;
        //public static bool isTextureLoaded = false;
        public static GUIStyle dynamicStyle = null;
        public static bool styleNeedsUpdate = false;
        public static bool isCoroutineRunning = false;
        public static GameObject selectedObject = null;
        public static Vector3 grabOffset = Vector3.zero;
        public static float moveDistance = 5f;
        public static bool isMovingObject = false;
        public static List<GameObject> selectedObjects = new List<GameObject>();
        public static Stack<(GameObject obj, Vector3 position)> deletedObjects = new Stack<(GameObject, Vector3)>();
        public static bool ConfigLock = true;
        public static GameObject cachedPickupSource;
        public static Dictionary<string, Il2CppAssetBundle> LoadedBundles = new Dictionary<string, Il2CppAssetBundle>();
        public static Dictionary<string, GameObject> LoadedPrefabs = new Dictionary<string, GameObject>();
        public static GameObject instantiatedPrefab;
        public static List<GameObject> LoadedPrefabsIndexed = new List<GameObject>();
        public static string assetName;
        public static Dictionary<string, Quaternion> PrefabRotations = new Dictionary<string, Quaternion>();
        public static Dictionary<string, Vector3> PrefabPositions = new Dictionary<string, Vector3>();
        public static Dictionary<string, Vector3> PrefabScales = new Dictionary<string, Vector3>();
    }
    public static class BuildInfo
    {
        public const string Name = "AfterlifeModMenu";
        public const string Description = "Modded Game for Testing";
        public const string Author = "Jaycoder";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = "https://www.yourMomsBasement.com";
    }
    public static class AfterlifeConsoleMsg
    {
        public static void _afterlifeConsole(string msg)
        {
            MelonLogger.Msg(msg);
        }
    }

    public static class AfterlifeUnityMapFunctions 
    {
        public static GameObject _findAfterlifeGameObj(string gameObjectName)
        {
            return GameObject.Find(gameObjectName);
        }
    }
}
