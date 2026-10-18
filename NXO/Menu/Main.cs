
using BepInEx;
using GorillaLocomotion;
using GorillaTag;
using NXO.Mods;
using NXO.Mods.Categories;
using NXO.Utilities;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

#nullable enable
namespace NXO.Menu;

public class Main : MonoBehaviour
{
  public static readonly Vector3 MenuSize = new Vector3(0.75f, 0.2875f, 0.45f);
  public static readonly Vector3 BackgroundScale = new Vector3(0.01f, 0.875f, 0.9f);
  public static readonly Vector3 BackgroundPos = new Vector3(0.05f, 0.0f, 0.025f);
  public const float FaceX = 0.07525f;
  public const float IconX = 0.07225f;
  public static readonly Vector3 SearchBarScale = new Vector3(0.01f, 0.8625f, 1f / 16f);
  public static readonly Vector3 SearchBarPos = new Vector3(0.0667f, 0.0f, 0.56f);
  public static readonly Vector3 SearchTextPos = new Vector3(0.054f, 0.0f, 0.2525f);
  public static readonly Vector3 HeaderIconScale = new Vector3(0.039375f, 0.0590625f, 0.0f);
  public const float DisconnectIconY = -0.25f;
  public const float SearchIconY = -0.35f;
  public const float HeaderIconZ = 0.45f;
  public const float FpsWidthFactor = 3.3f;
  public static readonly Vector2 TitleSize = new Vector2(0.128f, 0.0296f);
  public static readonly Vector3 AccentStripScale = new Vector3(1f / 1000f, 0.875f, 0.0045f);
  public static readonly Vector3 AccentStripPos = new Vector3(0.0715f, 0.0f, 0.4f);
  private static int? _uiLayerMask;
  public static Camera _pcCamera;
  private static MeshRenderer _clickerRenderer;
  private static Collider _clickerCollider;
  private static readonly RaycastHit[] _pcRayHits = new RaycastHit[32 /*0x20*/];
  private static string _lastSearchInput;
  public static Category _lastPageDrawn;
  private static int _lastPageIndex;
  public static int _lastDrawModVersion = -1;
  private static readonly List<ButtonHandler.Button> _pageCache = new List<ButtonHandler.Button>(7);
  private static bool _lastWasSearching;
  public const float ButtonFullWidth = 0.82f;
  public const float ButtonWidthWithGear = 0.695f;
  public const float ButtonWidthIncremental = 0.555f;
  public const float ButtonHeight = 0.08f;
  public const float ButtonDepth = 0.0075f;
  public const float ButtonTextX = 0.05975f;
  public const float ModLabelX = 0.054f;
  public const float ButtonYWithGear = 0.0625f;
  public const float ButtonBaseZ = 0.335f;
  public const float SubButtonWidth = 0.12f;
  public const float IncrementUpY = -0.3525f;
  public const float IncrementDownY = -0.22f;
  public const float GearY = -0.3525f;
  public const float ButtonTextYRatio = 0.288f;
  public const float CompactButtonY = -0.3525f;
  public const float CompactLabelX = 0.054f;
  public const float CompactLabelY = 0.035f;
  public const float CompactLabelWidth = 0.125f;
  public const float ToggleWidth = 0.15f;
  public const float ToggleY = -0.3375f;
  public const float ToggleHeight = 0.058f;
  public const float NavRowZ = -0.33f;
  public const float ReturnHeight = 0.2925f;
  public const float PageHeight = 0.25f;
  public const float PageLeftY = 0.285f;
  public const float PageRightY = -0.285f;
  public static readonly Vector2 ModTextSize = new Vector2(0.22f, 1f / 64f);
  public static readonly Vector2 IncrementalTextSize = new Vector2(0.15f, 0.0155f);
  public static readonly Vector2 SubTextSize = new Vector2(0.1f, 0.015f);
  public static readonly Vector2 PageTextSize = new Vector2(0.15f, 0.0225f);
  public static readonly Vector2 GearIconSize = new Vector2(0.1f, 0.02f);
  public static readonly Vector3 HomeIconScale = new Vector3(0.015f, 0.015f, 0.0f);
  public const float OutlineThickness = 0.0125f;
  public const float ButtonOutlineThickness = 0.0065f;
  public const float ClickerSize = 0.0035f;
  public static readonly Vector3 ClickerLocalPos = new Vector3(0.0f, -0.1f, 0.0f);
  public static List<Material> _dynamicMaterials = new List<Material>();
  public static readonly List<GameObject> _trackedObjects = new List<GameObject>();
  private static readonly List<GameObject> _pageButtonObjects = new List<GameObject>();
  private static readonly List<Material> _pageButtonMaterials = new List<Material>();
  private static readonly List<Mesh> _pageButtonMeshes = new List<Mesh>();
  private static bool _drawingPageButtons;
  private static readonly List<GameObject> _searchBarObjects = new List<GameObject>();
  private static readonly List<Material> _searchBarMaterials = new List<Material>();
  private static readonly List<Mesh> _searchBarMeshes = new List<Mesh>();
  private static bool _drawingSearchBar;
  private static Main.TrackedColorGroup _searchButtonGroup;
  private static readonly List<Main.FadeItem> _fadeItems = new List<Main.FadeItem>(96 /*0x60*/);
  private static readonly HashSet<Material> _fadeSeen = new HashSet<Material>();
  private static Coroutine _pageFadeRoutine;
  private const float PageFadeDuration = 0.24f;
  private static readonly List<Main.TrackedColorGroup> _colorGroups = new List<Main.TrackedColorGroup>(32 /*0x20*/);
  private static readonly List<ButtonHandler.Button> _newCacheScratch = new List<ButtonHandler.Button>(7);
  private static readonly List<(ButtonHandler.Button b, int score)> _scoredScratch = new List<(ButtonHandler.Button, int)>(32 /*0x20*/);
  private static bool _wasInMenuCondition;
  private static bool _wasInPcCondition;
  public static float menuScale = 0.75f;
  public static bool MenuAnimations = true;
  public static bool MenuSmoothing;
  private const float MenuSmoothingSpeed = 25f;
  private static bool _snapMenuNext;
  public static GameObject disconnectButton;
  public static GameObject fpsButton;
  public static UnityEngine.UI.Text fpsText;
  private static float _fpsSmooth;
  private static float _fpsTimer;
  public static Color pinwheelColor1 = (Color) new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static Color pinwheelColor2 = (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static float pinwheelSpeed = 0.5f;
  public static Material _cachedPinwheelBase;
  private static Material _clickerMaterial;
  private static float _lastAutoSaveTime;
  private const float AUTO_SAVE_INTERVAL = 60f;
  private static float _lastTitleUpdateTime;
  private static Settings.ColorMode _titleTextMode = ~Settings.ColorMode.Solid;
  private static Color32 _titleTextC1;
  private static Color32 _titleTextC2;
  private const float TITLE_UPDATE_INTERVAL = 0.03f;
  private static readonly StringBuilder _titleBuilder = new StringBuilder(256 /*0x0100*/);
  public static readonly List<Main.FontEntry> _fonts = new List<Main.FontEntry>(8);
  private static readonly List<Mesh> _dynamicMeshes = new List<Mesh>();
  private static readonly List<Material> _dynamicPinwheelMaterials = new List<Material>();
  private static Material _activePinwheelMaterial;
  public static readonly List<Font> _dynamicFonts = new List<Font>(4);
  private static readonly List<ButtonHandler.Button> _activeTicks = new List<ButtonHandler.Button>(32 /*0x20*/);
  private static bool _activeTicksDirty = true;
  public static int ActiveModsVersion;
  private static bool _secondaryWasDown;
  private static bool _menuToggled;
  public static float _lastKeyboardYRotation = float.MinValue;
  public static Vector3 _lastKeyboardSnapPosition = Vector3.zero;
  private static bool _colorsDirty = true;
  private static Shader _vertexColorShader;
  public static bool inRoom;
  public static bool RoomNotifications;

  private static int UILayerMask
  {
    get
    {
      int valueOrDefault = Main._uiLayerMask.GetValueOrDefault();
      if (Main._uiLayerMask.HasValue)
        return valueOrDefault;
      int uiLayerMask = 1 << LayerMask.NameToLayer("UI");
      Main._uiLayerMask = new int?(uiLayerMask);
      return uiLayerMask;
    }
  }

  public static float TextOffset
  {
    get => !(Settings.CurrentFontDescription == "Comic Sans") ? 0.15f : 0.1525f;
  }

  public static float IncrementTextOffset
  {
    get
    {
      switch (Settings.CurrentFontDescription)
      {
        case "Minecraft":
          return 0.15f;
        case "Arial":
          return 0.148f;
        case "Comic Sans":
          return 0.152f;
        default:
          return 0.155f;
      }
    }
  }

  public static float PageZOffset
  {
    get
    {
      switch (Settings.CurrentFontDescription)
      {
        case "Minecraft":
          return -0.14875f;
        case "Arial":
          return -0.14875f;
        default:
          return -0.14375f;
      }
    }
  }

  public static (float y, float z) TitleOffset
  {
    get
    {
      switch (Settings.CurrentFontDescription)
      {
        case "Comic Sans":
          return (57f / 1000f, 0.2075f);
        case "Minecraft":
          return (57f / 1000f, 0.2025f);
        default:
          return (57f / 1000f, 0.2025f);
      }
    }
  }

  private static List<GameObject> ObjectSink
  {
    get
    {
      if (Main._drawingSearchBar)
        return Main._searchBarObjects;
      return !Main._drawingPageButtons ? Main._trackedObjects : Main._pageButtonObjects;
    }
  }

  private static List<Material> MaterialSink
  {
    get
    {
      if (Main._drawingSearchBar)
        return Main._searchBarMaterials;
      return !Main._drawingPageButtons ? Main._dynamicMaterials : Main._pageButtonMaterials;
    }
  }

  private static List<Mesh> MeshSink
  {
    get
    {
      if (Main._drawingSearchBar)
        return Main._searchBarMeshes;
      return !Main._drawingPageButtons ? Main._dynamicMeshes : Main._pageButtonMeshes;
    }
  }

  public static Font CurrentFont
  {
    get => Main._fonts.Count <= 0 ? (Font) null : Main._fonts[Settings.CurrentFontIndex].Font;
  }

  private void Start()
  {
    Main._dynamicMaterials.Clear();
    Main._trackedObjects.Clear();
    Main._colorGroups.Clear();
    Variables.folderName = Path.Combine(Directory.GetCurrentDirectory(), "NXO Mod Menu");
    Directory.CreateDirectory(Variables.folderName);
    NetworkSystem instance1 = NetworkSystem.Instance;
    instance1.OnJoinedRoomEvent += new Action(Main.OnJoinRoom);
    NetworkSystem instance2 = NetworkSystem.Instance;
    instance2.OnReturnedToSinglePlayer += new Action(Main.OnLeaveRoom);
    NetworkSystem instance3 = NetworkSystem.Instance;
    instance3.OnPlayerJoined += new Action<NetPlayer>(Main.OnPlayerJoin);
    NetworkSystem instance4 = NetworkSystem.Instance;
    instance4.OnPlayerLeft += new Action<NetPlayer>(Main.OnPlayerLeave);
    Main.LoadFonts();
    Variables.taggerInstance = GorillaTagger.Instance;
    Variables.playerInstance = GTPlayer.Instance;
    Variables.thirdPersonCamera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
    Variables.cm = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera/CM vcam1");
    ButtonHandler.PreloadAllClickSounds();
    ButtonHandler.LoadCustomClickSounds();
    this.StartCoroutine(AssetHandler.LoadEmbeddedAudioClip("NXO.Resources.SteamOpen.wav", (Action<AudioClip>) (clip => Variables.menuOpenSound = clip)));
    this.StartCoroutine(AssetHandler.LoadEmbeddedAudioClip("NXO.Resources.SteamClose.wav", (Action<AudioClip>) (clip => Variables.menuCloseSound = clip)));
    this.StartCoroutine(AssetHandler.LoadEmbeddedAudioClip("NXO.Resources.NotificationSound.wav", (Action<AudioClip>) (clip => Variables.notificationSound = clip)));
    ButtonHandler.GeneratePresetButtons();
    Macros.GenerateMacroButtons();
    CustomNextbots.Initialize();
    ButtonHandler.LoadFavoritedMods();
    Settings.CaptureDefaults();
    ButtonHandler.LoadAutoSavePreference();
    ButtonHandler.LoadAutosavedStuff();
    Main._lastAutoSaveTime = Time.time;
    ButtonHandler.Button button = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.buttonText == "Auto Save"));
    if (button == null)
      return;
    button.Enabled = Variables.AutoSave;
    if (!Variables.AutoSave)
      NXOUI.RemoveMod("Auto Save");
  }

  public static void MarkActiveTicksDirty()
  {
    Main._activeTicksDirty = true;
    ++Main.ActiveModsVersion;
  }

  private static void TickActiveMods()
  {
    if (Main._activeTicksDirty)
    {
      Main._activeTicks.Clear();
      foreach (ButtonHandler.Button button in ModButtons.buttons)
      {
        if (button != null && button.Enabled && button.onEnable != null)
          Main._activeTicks.Add(button);
      }
      Main._activeTicksDirty = false;
    }
    for (int index = 0; index < Main._activeTicks.Count; ++index)
    {
      ButtonHandler.Button activeTick = Main._activeTicks[index];
      if (activeTick != null && activeTick.Enabled && activeTick.onEnable != null)
      {
        try
        {
          activeTick.onEnable();
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Button Tick Error '{activeTick.buttonText}': {ex.Message}\n{ex.StackTrace}");
        }
      }
    }
  }

  private void LateUpdate()
  {
    if (Admin.disableMenu || (UnityEngine.Object) Variables.playerInstance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null)
      return;
    if (Variables.AutoSave && (double) Time.time - (double) Main._lastAutoSaveTime >= 60.0)
    {
      Main._lastAutoSaveTime = Time.time;
      ButtonHandler.SaveAutosavedStuff();
    }
    Main.TickActiveMods();
    if (Keyboard.current != null && ((ButtonControl) Keyboard.current.rightAltKey).wasPressedThisFrame)
      NXOUI.IsGuiVisible = !NXOUI.IsGuiVisible;
    if (UnityInput.Current.GetKeyDown(Variables.PCMenuKey))
      Variables.PCMenuOpen = !Variables.PCMenuOpen;
    ControllerEmulator.Update();
    Macros.MacroPlaybackTick();
    RigManager.HandleGhostRig();
    Main.HandlePlayerActionCamera();
    Main.HandlePcMenuState();
    Main.HandleVrMenuState();
    SearchAndKeyboard.HandleKeyboardInput();
    Main.UpdateAnimatedColors();
    SearchAndKeyboard.UpdateSearchBlink();
  }

  private static void HandlePlayerActionCamera()
  {
    if (PhotonNetwork.InRoom)
    {
      if (Variables.currentPage != Category.Player_Action || PlayersActionList.selectedPlayer == null)
        return;
      PlayersActionList.UpdatePlayerCam();
    }
    else
    {
      Variables.notificationSent = false;
      PlayersActionList.ClearPlayerCam();
    }
  }

  private static void HandlePcMenuState()
  {
    bool flag = Variables.PCMenuOpen && !Variables.InMenuCondition && !InputHandler.LPrimary() && !InputHandler.RPrimary() && !Variables.menuOpen;
    if (flag && !Main._wasInPcCondition)
    {
      Variables.InPcCondition = true;
      Main._wasInPcCondition = true;
      if ((bool) (UnityEngine.Object) Variables.menuObj)
        return;
      Main.Draw(true);
      if (Variables.OpenAndCloseMenuSounds)
        AssetHandler.PlaySound(Variables.thirdPersonCamera, Variables.menuOpenSound, 1.25f);
    }
    else if (!flag && Main._wasInPcCondition)
    {
      Variables.InPcCondition = false;
      Main._wasInPcCondition = false;
      if (!((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
        return;
      if (Variables.OpenAndCloseMenuSounds)
        AssetHandler.PlaySound(Variables.thirdPersonCamera, Variables.menuCloseSound, 1.25f);
      ((MonoBehaviour) Variables.playerInstance).StartCoroutine(Main.BounceAnim(Variables.menuObj, Vector3.zero, true));
      Main.DestroyClicker();
      if (SearchAndKeyboard.isSearching)
        SearchAndKeyboard.CloseKeyboard();
    }
    else
    {
      if (!Variables.InPcCondition || !((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
        return;
      Main.AddButtonClicker(Variables.thirdPersonCamera?.transform);
      Main.AddTitleAndFPSCounter();
      Main.UpdateDisconnectIcon();
      Main.UpdateFpsCounter();
      if ((UnityEngine.Object) Variables.thirdPersonCamera != (UnityEngine.Object) null)
      {
        Transform transform = Variables.thirdPersonCamera.transform;
        Variables.menuObj.transform.SetParent(transform, true);
        Variables.menuObj.transform.position = transform.position + transform.rotation * Vector3.forward * 0.5f + Vector3.down * 0.025f;
        Variables.menuObj.transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
        Variables.menuObj.transform.Rotate(Vector3.up, -90f);
        Variables.menuObj.transform.Rotate(Vector3.right, -90f);
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
          if ((UnityEngine.Object) Main._clickerRenderer != (UnityEngine.Object) null && !Main._clickerRenderer.enabled)
            Main._clickerRenderer.enabled = true;
          if ((UnityEngine.Object) Main._pcCamera == (UnityEngine.Object) null)
            Main._pcCamera = Variables.thirdPersonCamera.GetComponent<Camera>();
          int num = Physics.RaycastNonAlloc(Main._pcCamera.ScreenPointToRay((Vector3) ((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue()), Main._pcRayHits, 100f, Main.UILayerMask);
          for (int index = 0; index < num; ++index)
          {
            ButtonHandler.BtnCollider component = ((Component) (Main._pcRayHits[index]).collider)?.GetComponent<ButtonHandler.BtnCollider>();
            if ((UnityEngine.Object) component != (UnityEngine.Object) null && (UnityEngine.Object) Main._clickerCollider != (UnityEngine.Object) null)
            {
              component.OnTriggerEnter(Main._clickerCollider);
              break;
            }
          }
        }
        else if (Mouse.current != null && !Mouse.current.leftButton.isPressed && (UnityEngine.Object) Main._clickerRenderer != (UnityEngine.Object) null && Main._clickerRenderer.enabled)
          Main._clickerRenderer.enabled = false;
      }
    }
  }

  private static void HandleVrMenuState()
  {
    bool flag1 = Variables.rightHandedMenu ? InputHandler.RSecondary() : InputHandler.LSecondary();
    bool menuToggled = Main._menuToggled;
    if (Variables.MenuToggles)
    {
      if (flag1 && !Main._secondaryWasDown)
        Main._menuToggled = !Main._menuToggled;
    }
    else
      Main._menuToggled = flag1;
    if (Main._menuToggled != menuToggled)
    {
      GorillaTagger instance = GorillaTagger.Instance;
      if ((UnityEngine.Object) instance != (UnityEngine.Object) null)
        Variables.VibrateMenuHand((float) ((double) instance.tagHapticStrength / 2.0 - (Main._menuToggled ? (double) instance.tagHapticStrength / 4.0 : (double) instance.tagHapticStrength / 3.0)), instance.tagHapticDuration);
    }
    Main._secondaryWasDown = flag1;
    bool flag2 = (Main._menuToggled || SearchAndKeyboard.isSearching) && !Variables.InPcCondition;
    if (flag2 && !Main._wasInMenuCondition)
    {
      Variables.InMenuCondition = true;
      Main._wasInMenuCondition = true;
      if (!(bool) (UnityEngine.Object) Variables.menuObj)
      {
        Main.Draw(true);
        if (Variables.OpenAndCloseMenuSounds)
          AssetHandler.PlaySound(RigManager.GetHandObject, Variables.menuOpenSound, 0.625f);
      }
      if ((bool) (UnityEngine.Object) Variables.clickerObj)
        return;
      Main.AddButtonClicker(Variables.rightHandedMenu ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform);
    }
    else if (!flag2 && Main._wasInMenuCondition)
    {
      Variables.InMenuCondition = false;
      Main._wasInMenuCondition = false;
      if (!((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
        return;
      ((MonoBehaviour) Variables.playerInstance).StartCoroutine(Main.BounceAnim(Variables.menuObj, Vector3.zero, true));
      Main.DestroyClicker();
      if (Variables.OpenAndCloseMenuSounds)
        AssetHandler.PlaySound(RigManager.GetHandObject, Variables.menuCloseSound, 0.625f);
    }
    else
    {
      if (!Variables.InMenuCondition || !((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
        return;
      if (!(bool) (UnityEngine.Object) Variables.clickerObj)
        Main.AddButtonClicker(Variables.rightHandedMenu ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform);
      if (SearchAndKeyboard.isSearching && (UnityEngine.Object) SearchAndKeyboard.keyboardObj != (UnityEngine.Object) null)
        Main.PositionMenuOnKeyboard();
      else
        Main.PositionMenuForHand();
      Main.AddTitleAndFPSCounter();
      Main.UpdateDisconnectIcon();
      Main.UpdateFpsCounter();
    }
  }

  private static void PositionMenuOnKeyboard()
  {
    if ((UnityEngine.Object) SearchAndKeyboard.keyboardObj != (UnityEngine.Object) null && (UnityEngine.Object) Variables.playerInstance != (UnityEngine.Object) null)
    {
      float y = ((Component) Variables.playerInstance.headCollider).transform.eulerAngles.y;
      Vector3 position = ((Component) Variables.playerInstance.headCollider).transform.position;
      if ((double) Main._lastKeyboardYRotation == -3.4028234663852886E+38)
      {
        Main._lastKeyboardYRotation = y;
        Main._lastKeyboardSnapPosition = position;
      }
      if (Variables.SearchFollow)
      {
        float num1 = Mathf.Abs(Mathf.DeltaAngle(Main._lastKeyboardYRotation, y));
        float num2 = Vector3.Distance(position, Main._lastKeyboardSnapPosition);
        if ((double) num1 >= 45.0 || (double) num2 >= 1.5)
        {
          Main._lastKeyboardYRotation = y;
          Main._lastKeyboardSnapPosition = position;
        }
        SearchAndKeyboard.keyboardObj.transform.position = position;
        SearchAndKeyboard.keyboardObj.transform.rotation = Quaternion.Euler(0.0f, Main._lastKeyboardYRotation, 0.0f);
        SearchAndKeyboard.keyboardObj.transform.position += SearchAndKeyboard.keyboardObj.transform.forward * 0.35f - Vector3.up * 0.35f;
        SearchAndKeyboard.keyboardObj.transform.Rotate(Vector3.right, 70f);
      }
      else
      {
        SearchAndKeyboard.keyboardObj.transform.position = Main._lastKeyboardSnapPosition;
        SearchAndKeyboard.keyboardObj.transform.rotation = Quaternion.Euler(0.0f, Main._lastKeyboardYRotation, 0.0f);
        SearchAndKeyboard.keyboardObj.transform.position += SearchAndKeyboard.keyboardObj.transform.forward * 0.35f - Vector3.up * 0.35f;
        SearchAndKeyboard.keyboardObj.transform.Rotate(Vector3.right, 70f);
      }
    }
    Transform transform = SearchAndKeyboard.keyboardObj.transform;
    Variables.menuObj.transform.position = transform.position - transform.forward * 0.17f + transform.up * 0.2f;
    Variables.menuObj.transform.rotation = transform.rotation * Quaternion.Euler(-75f, 0.0f, 0.0f);
    Variables.menuObj.transform.Rotate(Vector3.up, 90f);
    Variables.menuObj.transform.Rotate(Vector3.right, -90f);
  }

  private static void PositionMenuForHand()
  {
    if ((UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null || (UnityEngine.Object) Variables.playerInstance == (UnityEngine.Object) null)
      return;
    Transform controllerTransform = (Variables.rightHandedMenu ? Variables.playerInstance.RightHand : Variables.playerInstance.LeftHand).controllerTransform;
    Vector3 b1;
    Quaternion b2;
    if (Variables.SecondaryMenuPosition)
    {
      float num = Variables.rightHandedMenu ? -1f : 1f;
      b1 = controllerTransform.TransformPoint(0.0269999988f * num, -0.0600000024f, 0.075f);
      b2 = controllerTransform.rotation * Quaternion.Euler(-35f, 12f * num, 19f * num);
      if (Variables.rightHandedMenu)
        b2 *= Quaternion.Euler(0.0f, 0.0f, 180f);
    }
    else
    {
      b1 = controllerTransform.position - controllerTransform.up * 0.055f + controllerTransform.right * (3f / 500f);
      Vector3 eulerAngles = controllerTransform.rotation.eulerAngles;
      if (Variables.rightHandedMenu)
        eulerAngles += new Vector3(0.0f, 0.0f, 180f);
      b2 = Quaternion.Euler(eulerAngles);
    }
    if (Main.MenuSmoothing && !Main._snapMenuNext)
    {
      float t = 1f - Mathf.Exp(-25f * Time.deltaTime);
      Variables.menuObj.transform.position = Vector3.Lerp(Variables.menuObj.transform.position, b1, t);
      Variables.menuObj.transform.rotation = Quaternion.Slerp(Variables.menuObj.transform.rotation, b2, t);
    }
    else
    {
      Variables.menuObj.transform.position = b1;
      Variables.menuObj.transform.rotation = b2;
      Main._snapMenuNext = false;
    }
  }

  public static void MarkColorsDirty() => Main._colorsDirty = true;

  private static Shader VertexColorShader
  {
    get
    {
      return !((UnityEngine.Object) Main._vertexColorShader != (UnityEngine.Object) null) ? (Main._vertexColorShader = Shader.Find("Sprites/Default")) : Main._vertexColorShader;
    }
  }

  private static void ApplyGradient(Renderer r, Color c1, Color c2)
  {
    if ((UnityEngine.Object) r == (UnityEngine.Object) null || (UnityEngine.Object) Main.VertexColorShader == (UnityEngine.Object) null)
      return;
    MeshFilter component = r.GetComponent<MeshFilter>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || (UnityEngine.Object) component.sharedMesh == (UnityEngine.Object) null)
      return;
    Mesh mesh = component.mesh;
    Vector3[] vertices = mesh.vertices;
    if (vertices.Length == 0)
      return;
    Bounds bounds = mesh.bounds;
    float menuOpacity = Settings.MenuOpacity;
    Settings.GradientDirection gradientDirection = Settings.CurrentGradientDirection;
    Color[] colorArray = new Color[vertices.Length];
    for (int index = 0; index < vertices.Length; ++index)
    {
      float num1 = (double) bounds.size.y > 9.9999997473787516E-06 ? Mathf.InverseLerp(bounds.min.y, bounds.max.y, vertices[index].y) : 0.0f;
      float num2 = (double) bounds.size.z > 9.9999997473787516E-06 ? Mathf.InverseLerp(bounds.min.z, bounds.max.z, vertices[index].z) : 0.0f;
      double num3;
      switch (gradientDirection)
      {
        case Settings.GradientDirection.Vertical:
          num3 = (double) num2;
          break;
        case Settings.GradientDirection.Diagonal:
          num3 = ((double) num1 + (double) num2) * 0.5;
          break;
        default:
          num3 = (double) num1;
          break;
      }
      float t = (float) num3;
      Color color = Color.Lerp(c1, c2, t) with
      {
        a = menuOpacity
      };
      colorArray[index] = color;
    }
    mesh.colors = colorArray;
    if (!Main._dynamicMeshes.Contains(mesh))
      Main._dynamicMeshes.Add(mesh);
    if (!((UnityEngine.Object) r.sharedMaterial == (UnityEngine.Object) null) && !((UnityEngine.Object) r.sharedMaterial.shader != (UnityEngine.Object) Main.VertexColorShader))
      return;
    Material material = new Material(Main.VertexColorShader)
    {
      color = Color.white,
      renderQueue = 2460
    };
    Main._dynamicMaterials.Add(material);
    r.sharedMaterial = material;
  }

  private static void UpdateAnimatedColors()
  {
    if ((UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null && !SearchAndKeyboard.isSearching)
      return;
    bool flag1 = Settings.BackgroundMode == Settings.ColorMode.Pinwheel;
    bool flag2 = Settings.ButtonMode == Settings.ColorMode.Pinwheel;
    bool flag3 = Settings.EnabledButtonMode == Settings.ColorMode.Pinwheel;
    bool flag4 = Settings.OutlineColorMode == Settings.ColorMode.Pinwheel;
    bool flag5 = Settings.AccentStripMode == Settings.ColorMode.Pinwheel;
    bool flag6 = flag1 | flag2 | flag3 | flag4 | flag5;
    bool flag7 = Settings.BackgroundMode == Settings.ColorMode.Gradient;
    bool flag8 = Settings.ButtonMode == Settings.ColorMode.Gradient;
    bool flag9 = Settings.EnabledButtonMode == Settings.ColorMode.Gradient;
    bool flag10 = Settings.OutlineColorMode == Settings.ColorMode.Gradient;
    bool flag11 = Settings.AccentStripMode == Settings.ColorMode.Gradient;
    bool flag12 = Settings.BackgroundMode != 0;
    bool flag13 = Settings.ButtonMode != 0;
    bool flag14 = Settings.EnabledButtonMode != 0;
    bool flag15 = Settings.OutlineColorMode != 0;
    bool flag16 = Settings.AccentStripMode != 0;
    bool flag17 = Settings.TitleMode != 0;
    if (!(flag12 | flag13 | flag14 | flag15 | flag16 | flag17) && !Main._colorsDirty)
      return;
    if (flag6)
    {
      if ((UnityEngine.Object) Main._activePinwheelMaterial == (UnityEngine.Object) null)
      {
        Main._activePinwheelMaterial = Main.GetPinwheelMaterial();
        Main._dynamicPinwheelMaterials.Add(Main._activePinwheelMaterial);
      }
      else if (Main._colorsDirty)
      {
        AssetHandler.SetProperty(Main._activePinwheelMaterial, "_Speed", (object) (float) -(double) Main.pinwheelSpeed);
        AssetHandler.SetProperty(Main._activePinwheelMaterial, "_COLOR1", (object) Main.pinwheelColor1);
        AssetHandler.SetProperty(Main._activePinwheelMaterial, "_COLOR2", (object) Main.pinwheelColor2);
      }
    }
    Color color1 = flag1 ? Color.white : Settings.GetAnimatedColor(Settings.BackgroundMode, (Color) Settings.BackgroundColor, (Color) Settings.BackgroundColor2, Settings.BackgroundAnimSpeed);
    Color color2 = flag2 ? Color.white : Settings.GetAnimatedColor(Settings.ButtonMode, (Color) Settings.ButtonColor, (Color) Settings.ButtonColor2, Settings.ButtonAnimSpeed, 1);
    Color color3 = flag3 ? Color.white : Settings.GetAnimatedColor(Settings.EnabledButtonMode, (Color) Settings.EnabledButtonColor, (Color) Settings.EnabledButtonColor2, Settings.EnabledButtonAnimSpeed, 2);
    Color color4 = flag4 ? Color.white : Settings.GetAnimatedColor(Settings.OutlineColorMode, (Color) Settings.OutlineColor, (Color) Settings.OutlineColor2, Settings.OutlineAnimSpeed, 4);
    Color color5 = flag5 ? Color.white : Settings.GetAnimatedColor(Settings.AccentStripMode, (Color) Settings.AccentStripColor, (Color) Settings.AccentStripColor2, Settings.AccentStripAnimSpeed, 5);
    for (int index1 = 0; index1 < Main._colorGroups.Count; ++index1)
    {
      Main.TrackedColorGroup colorGroup = Main._colorGroups[index1];
      Color color6;
      bool flag18;
      bool flag19;
      bool flag20;
      Color c1;
      Color c2;
      switch (colorGroup.role)
      {
        case Main.ColorRole.Background:
          color6 = color1;
          flag18 = flag1;
          flag19 = flag12;
          flag20 = flag7;
          c1 = (Color) Settings.BackgroundColor;
          c2 = (Color) Settings.BackgroundColor2;
          break;
        case Main.ColorRole.Button:
          color6 = color2;
          flag18 = flag2;
          flag19 = flag13;
          flag20 = flag8;
          c1 = (Color) Settings.ButtonColor;
          c2 = (Color) Settings.ButtonColor2;
          break;
        case Main.ColorRole.EnabledButton:
          color6 = color3;
          flag18 = flag3;
          flag19 = flag14;
          flag20 = flag9;
          c1 = (Color) Settings.EnabledButtonColor;
          c2 = (Color) Settings.EnabledButtonColor2;
          break;
        case Main.ColorRole.Outline:
          color6 = color4;
          flag18 = flag4;
          flag19 = flag15;
          flag20 = flag10;
          c1 = (Color) Settings.OutlineColor;
          c2 = (Color) Settings.OutlineColor2;
          break;
        case Main.ColorRole.AccentStrip:
          color6 = color5;
          flag18 = flag5;
          flag19 = flag16;
          flag20 = flag11;
          c1 = (Color) Settings.AccentStripColor;
          c2 = (Color) Settings.AccentStripColor2;
          break;
        default:
          continue;
      }
      if (flag20)
      {
        if (Main._colorsDirty)
        {
          for (int index2 = 0; index2 < colorGroup.renderers.Count; ++index2)
            Main.ApplyGradient(colorGroup.renderers[index2], c1, c2);
        }
      }
      else if ((flag19 || Main._colorsDirty) && (!flag18 || Main._colorsDirty))
      {
        for (int index3 = 0; index3 < colorGroup.renderers.Count; ++index3)
        {
          Renderer renderer = colorGroup.renderers[index3];
          if (!((UnityEngine.Object) renderer == (UnityEngine.Object) null))
          {
            if (flag18)
            {
              if ((UnityEngine.Object) Main._activePinwheelMaterial != (UnityEngine.Object) null)
                renderer.sharedMaterial = Main._activePinwheelMaterial;
            }
            else
            {
              Color color7 = color6 with
              {
                a = Settings.MenuOpacity
              };
              if ((UnityEngine.Object) renderer.sharedMaterial == (UnityEngine.Object) Main._activePinwheelMaterial || (UnityEngine.Object) renderer.sharedMaterial == (UnityEngine.Object) null)
              {
                Material colorMaterial = Main.CreateColorMaterial(color7);
                Main._dynamicMaterials.Add(colorMaterial);
                renderer.sharedMaterial = colorMaterial;
              }
              else
                renderer.sharedMaterial.color = color7;
            }
          }
        }
      }
    }
    if ((UnityEngine.Object) Variables.title != (UnityEngine.Object) null && (flag17 || Main._colorsDirty))
    {
      Color color8 = Settings.TitleMode != Settings.ColorMode.Gradient ? Settings.GetAnimatedColor(Settings.TitleMode, (Color) Settings.TitleColor, (Color) Settings.TitleColor2, Settings.TitleAnimSpeed, 3) : Color.white;
      ((Graphic) Variables.title).color = color8;
    }
    Main._colorsDirty = false;
  }

  public static void RegisterColorGroup(
    Main.ColorRole role,
    GameObject baseObj,
    List<GameObject> extraParts)
  {
    if (role == Main.ColorRole.None || (UnityEngine.Object) baseObj == (UnityEngine.Object) null)
      return;
    Main.TrackedColorGroup trackedColorGroup = new Main.TrackedColorGroup()
    {
      role = role
    };
    Renderer component1 = baseObj.GetComponent<Renderer>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      trackedColorGroup.renderers.Add(component1);
    if (extraParts != null)
    {
      for (int index = 0; index < extraParts.Count; ++index)
      {
        if (!((UnityEngine.Object) extraParts[index] == (UnityEngine.Object) null))
        {
          Renderer component2 = extraParts[index].GetComponent<Renderer>();
          if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
            trackedColorGroup.renderers.Add(component2);
        }
      }
    }
    Main._colorGroups.Add(trackedColorGroup);
    Main._colorsDirty = true;
  }

  public static void OnJoinRoom()
  {
    if (Main.inRoom)
      return;
    Main.inRoom = true;
    NetworkingLibrary.BroadcastNXOUser();
    Room.roomCode = PhotonNetwork.CurrentRoom.Name;
    Main.NotifyRoomEvent($"Joined Code `{Room.roomCode}`");
    ((MonoBehaviour) Variables.playerInstance).StartCoroutine(PlayersActionList.DelayedPlayerButtonGeneration());
  }

  public static void OnLeaveRoom()
  {
    if (!Main.inRoom)
      return;
    Main.inRoom = false;
    MenuPatches.SerializationPatch.Override = (Func<bool>) null;
    NetworkingLibrary.ClearDetectedUsers();
    Main.NotifyRoomEvent($"Left Code `{Room.roomCode}`");
    PlayersActionList.GeneratePlayerButtons();
    Sound.ResetMic();
  }

  public static void OnPlayerJoin(NetPlayer player)
  {
    NetworkingLibrary.BroadcastNXOUser();
    if (player != NetworkSystem.Instance.LocalPlayer)
      Main.NotifyRoomEvent($"`{player.NickName}` Joined");
    if (Variables.currentPage != Category.Players)
      return;
    PlayersActionList.GeneratePlayerButtons();
  }

  public static void OnPlayerLeave(NetPlayer player)
  {
    if (player != NetworkSystem.Instance.LocalPlayer)
      Main.NotifyRoomEvent($"`{player.NickName}` Left");
    if (Variables.currentPage != Category.Players)
      return;
    PlayersActionList.GeneratePlayerButtons();
  }

  public static void NotifyRoomEvent(string message)
  {
    if (!Main.RoomNotifications)
      return;
    NotificationLib.SendNotification(NotificationLib.NotificationType.Room, message);
    if (!((UnityEngine.Object) Variables.notificationSound != (UnityEngine.Object) null) || !Variables.NotificationSounds)
      return;
    AssetHandler.PlaySound(Variables.thirdPersonCamera, Variables.notificationSound);
  }

  private static GameObject CreatePrimitive(
    PrimitiveType type,
    string name = null,
    Transform parent = null,
    bool keepCollider = false,
    bool triggerCollider = true)
  {
    GameObject primitive = GameObject.CreatePrimitive(type);
    if (name != null)
      primitive.name = name;
    Rigidbody component1 = primitive.GetComponent<Rigidbody>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) component1);
    Collider component2 = primitive.GetComponent<Collider>();
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
    {
      if (!keepCollider)
        UnityEngine.Object.Destroy((UnityEngine.Object) component2);
      else if (component2 is BoxCollider boxCollider)
        ((Collider) boxCollider).isTrigger = triggerCollider;
    }
    primitive.transform.SetParent(parent ?? Variables.menuObj?.transform, true);
    primitive.transform.localRotation = Quaternion.identity;
    Main.ObjectSink.Add(primitive);
    return primitive;
  }

  private static GameObject CreateCube(string name = null, Transform parent = null, bool keepCollider = false)
  {
    return Main.CreatePrimitive(PrimitiveType.Cube, name, parent, keepCollider);
  }

  private static GameObject CreateQuad(string name = null, Transform parent = null)
  {
    GameObject primitive = Main.CreatePrimitive(PrimitiveType.Quad, name, parent);
    ((Collider) primitive.AddComponent<BoxCollider>()).isTrigger = true;
    return primitive;
  }

  private static Material TrackMat(Material mat)
  {
    if ((UnityEngine.Object) mat != (UnityEngine.Object) null)
      Main.MaterialSink.Add(mat);
    return mat;
  }

  private static void ApplyColor(GameObject obj, Color color)
  {
    Renderer component = obj.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    component.sharedMaterial = Main.TrackMat(Main.CreateColorMaterial(color));
  }

  private static void ApplyTexture(GameObject obj, string resourcePath, Color? tint = null)
  {
    Texture2D texture2D = AssetHandler.LoadEmbeddedTexture(resourcePath);
    if ((UnityEngine.Object) texture2D == (UnityEngine.Object) null)
      return;
    Material mat = new Material(Variables.uiShader)
    {
      mainTexture = (Texture) texture2D
    };
    if (tint.HasValue)
      mat.color = tint.Value;
    obj.GetComponent<Renderer>().sharedMaterial = Main.TrackMat(mat);
  }

  private static List<GameObject> ApplyOutline(
    GameObject target,
    int requiredMode,
    float zGrow = 0.0125f,
    bool animated = true,
    float roundness = -1f,
    int seg = 5)
  {
    if (Settings.OutlineMode < requiredMode)
      return (List<GameObject>) null;
    Color color = animated ? (Color) Settings.OutlineColor : Color.Lerp(Main.pinwheelColor1, Main.pinwheelColor2, 0.5f);
    GameObject outline = Main.CreateOutline(target, Variables.menuObj.transform, zGrow, color);
    float roundness1 = (double) roundness < 0.0 ? Settings.MenuRoundness : roundness;
    List<GameObject> gameObjectList1 = (List<GameObject>) null;
    if ((double) roundness1 > 0.0)
      gameObjectList1 = Main.RoundObj(outline, roundness: roundness1, transparentQueue: 2455, seg: seg);
    if (animated)
      Main.RegisterColorGroup(Main.ColorRole.Outline, outline, gameObjectList1);
    List<GameObject> gameObjectList2 = new List<GameObject>(2)
    {
      outline
    };
    if (gameObjectList1 != null)
      gameObjectList2.AddRange((IEnumerable<GameObject>) gameObjectList1);
    return gameObjectList2;
  }

  private static void RegisterClickAnim(
    ButtonHandler.BtnCollider col,
    GameObject body,
    List<GameObject> rounded,
    List<GameObject> outline,
    params Transform[] uniform)
  {
    if ((UnityEngine.Object) col == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) body != (UnityEngine.Object) null)
      col.RegisterBody(body.transform);
    if (rounded != null)
    {
      for (int index = 0; index < rounded.Count; ++index)
      {
        if ((UnityEngine.Object) rounded[index] != (UnityEngine.Object) null)
          col.RegisterBody(rounded[index].transform);
      }
    }
    if (outline != null)
    {
      for (int index = 0; index < outline.Count; ++index)
      {
        if ((UnityEngine.Object) outline[index] != (UnityEngine.Object) null)
          col.RegisterBody(outline[index].transform);
      }
    }
    if (uniform == null)
      return;
    for (int index = 0; index < uniform.Length; ++index)
    {
      if ((UnityEngine.Object) uniform[index] != (UnityEngine.Object) null)
        col.RegisterUniform(uniform[index]);
    }
  }

  private static GameObject CreateOutline(
    GameObject target,
    Transform parent,
    float zGrow,
    Color color)
  {
    GameObject cube = Main.CreateCube(parent: parent);
    cube.transform.position = target.transform.position;
    cube.transform.rotation = target.transform.rotation;
    Vector3 localScale = target.transform.localScale;
    Vector3 lossyScale = parent.lossyScale;
    float num = zGrow * (lossyScale.z / lossyScale.y);
    cube.transform.localScale = new Vector3(localScale.x - 1f / 400f, localScale.y + num, localScale.z + zGrow);
    Main.ApplyColor(cube, color);
    return cube;
  }

  public static void Draw(bool openCall = false)
  {
    Main._snapMenuNext = true;
    Main.CreateBaseMenuObject();
    Main.CreateMenuBackground();
    Main.CreateMenuCanvasAndTitle();
    Main.CreateAccentStrip();
    Main.ToggleSearchButton();
    Main.AddSearchBar();
    Main.AddDisconnectButton();
    Main.AddFpsButton();
    Main.AddReturnButton();
    Main.AddPageButton("<", 0.285f);
    Main.AddPageButton(">", -0.285f);
    if (PlayersActionList.selectedPlayer != null && Variables.currentPage == Category.Player_Action)
      PlayersActionList.AddPlayerCam();
    Main.DrawCurrentPageButtons();
    Variables.menuObj.transform.localScale *= Main.menuScale;
    if (openCall)
    {
      Vector3 localScale = Variables.menuObj.transform.localScale;
      Variables.menuObj.transform.localScale = Vector3.zero;
      ((MonoBehaviour) Variables.playerInstance).StartCoroutine(Main.BounceAnim(Variables.menuObj, localScale));
    }
    Main.SetUILayer(Variables.menuObj);
  }

  public static void RedrawButtonList(int animate = 0)
  {
    ++NXOUI.MenuContentVersion;
    if ((UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null)
      return;
    if (Main._pageFadeRoutine != null && (UnityEngine.Object) Variables.playerInstance != (UnityEngine.Object) null)
    {
      ((MonoBehaviour) Variables.playerInstance).StopCoroutine(Main._pageFadeRoutine);
      Main._pageFadeRoutine = (Coroutine) null;
    }
    Main._fadeItems.Clear();
    for (int index = 0; index < Main._pageButtonObjects.Count; ++index)
    {
      if ((UnityEngine.Object) Main._pageButtonObjects[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._pageButtonObjects[index]);
    }
    Main._pageButtonObjects.Clear();
    for (int index = 0; index < Main._pageButtonMaterials.Count; ++index)
    {
      if ((UnityEngine.Object) Main._pageButtonMaterials[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._pageButtonMaterials[index]);
    }
    Main._pageButtonMaterials.Clear();
    for (int index = 0; index < Main._pageButtonMeshes.Count; ++index)
    {
      if ((UnityEngine.Object) Main._pageButtonMeshes[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._pageButtonMeshes[index]);
    }
    Main._pageButtonMeshes.Clear();
    Main._colorGroups.RemoveAll((Predicate<Main.TrackedColorGroup>) (g => g.renderers.Count == 0 || g.renderers.All<Renderer>((Func<Renderer, bool>) (r => (UnityEngine.Object) r == (UnityEngine.Object) null))));
    Main._lastDrawModVersion = -1;
    Main._lastWasSearching = false;
    Main._lastSearchInput = (string) null;
    Main.DrawCurrentPageButtons();
    Main.SetUILayer(Variables.menuObj);
    if (animate == 0 || !Main.MenuAnimations)
      return;
    Main.StartPageFade();
  }

  private static void StartPageFade()
  {
    if ((UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null || (UnityEngine.Object) Variables.playerInstance == (UnityEngine.Object) null)
      return;
    Main._fadeItems.Clear();
    Main._fadeSeen.Clear();
    for (int index = 0; index < Main._pageButtonObjects.Count; ++index)
    {
      GameObject pageButtonObject = Main._pageButtonObjects[index];
      if (!((UnityEngine.Object) pageButtonObject == (UnityEngine.Object) null))
      {
        UnityEngine.UI.Text component1 = pageButtonObject.GetComponent<UnityEngine.UI.Text>();
        Main.FadeItem fadeItem1;
        if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        {
          List<Main.FadeItem> fadeItems = Main._fadeItems;
          fadeItem1 = new Main.FadeItem();
          fadeItem1.txt = component1;
          fadeItem1.origA = ((Graphic) component1).color.a;
          Main.FadeItem fadeItem2 = fadeItem1;
          fadeItems.Add(fadeItem2);
          Main.SetFadeAlpha(Main._fadeItems.Count - 1, 0.0f);
        }
        else
        {
          Renderer component2 = pageButtonObject.GetComponent<Renderer>();
          Material sharedMaterial = (UnityEngine.Object) component2 != (UnityEngine.Object) null ? component2.sharedMaterial : (Material) null;
          if ((UnityEngine.Object) sharedMaterial != (UnityEngine.Object) null && Main._fadeSeen.Add(sharedMaterial))
          {
            List<Main.FadeItem> fadeItems = Main._fadeItems;
            fadeItem1 = new Main.FadeItem();
            fadeItem1.r = component2;
            fadeItem1.target = sharedMaterial.color;
            Main.FadeItem fadeItem3 = fadeItem1;
            fadeItems.Add(fadeItem3);
            Main.SetFadeAlpha(Main._fadeItems.Count - 1, 0.0f);
          }
        }
      }
    }
    if (Main._fadeItems.Count <= 0)
      return;
    Main._pageFadeRoutine = ((MonoBehaviour) Variables.playerInstance).StartCoroutine(Main.PageFadeRoutine());
  }

  private static void SetFadeAlpha(int i, float factor)
  {
    Main.FadeItem fadeItem = Main._fadeItems[i];
    if ((UnityEngine.Object) fadeItem.txt != (UnityEngine.Object) null)
    {
      Color color = ((Graphic) fadeItem.txt).color with
      {
        a = fadeItem.origA * factor
      };
      ((Graphic) fadeItem.txt).color = color;
    }
    else
    {
      if (!((UnityEngine.Object) fadeItem.r != (UnityEngine.Object) null) || !((UnityEngine.Object) fadeItem.r.sharedMaterial != (UnityEngine.Object) null))
        return;
      Color color = Color.Lerp((Color) Settings.BackgroundColor, fadeItem.target, factor) with
      {
        a = fadeItem.target.a
      };
      fadeItem.r.sharedMaterial.color = color;
    }
  }

  private static IEnumerator PageFadeRoutine()
  {
    float elapsed = 0.0f;
    while ((double) elapsed < 0.23999999463558197)
    {
      float f = elapsed / 0.24f;
      for (int i = 0; i < Main._fadeItems.Count; ++i)
        Main.SetFadeAlpha(i, f);
      elapsed += Time.deltaTime;
      yield return (object) null;
    }
    for (int i = 0; i < Main._fadeItems.Count; ++i)
      Main.SetFadeAlpha(i, 1f);
    Main._pageFadeRoutine = (Coroutine) null;
  }

  private static void DrawCurrentPageButtons()
  {
    bool flag = SearchAndKeyboard.isSearching && !SearchAndKeyboard.isTyping && !string.IsNullOrEmpty(SearchAndKeyboard.inputText);
    if (flag != Main._lastWasSearching | (flag && (SearchAndKeyboard.inputText != Main._lastSearchInput || Variables.currentCategoryPage != Main._lastPageIndex)) | (!flag && Variables.currentPage != Main._lastPageDrawn) | (!flag && Variables.currentCategoryPage != Main._lastPageIndex) | (!flag && NXOUI._modListVersion != Main._lastDrawModVersion))
    {
      List<ButtonHandler.Button> newCacheScratch = Main._newCacheScratch;
      newCacheScratch.Clear();
      if (flag)
      {
        List<(ButtonHandler.Button b, int score)> scoredScratch = Main._scoredScratch;
        scoredScratch.Clear();
        foreach (ButtonHandler.Button button in ModButtons.buttons)
        {
          if (button != null && button.Page != Category.Home)
          {
            int num = SearchAndKeyboard.FuzzyScore(button.buttonText, SearchAndKeyboard.inputText);
            if (num > 0)
              scoredScratch.Add((button, num));
          }
        }
        scoredScratch.Sort((Comparison<(ButtonHandler.Button btn, int score)>) ((a, z) => z.score.CompareTo(a.score)));
        int num1 = Variables.currentCategoryPage * Variables.ButtonsPerPage;
        for (int index = num1; index < Mathf.Min(num1 + Variables.ButtonsPerPage, scoredScratch.Count); ++index)
          newCacheScratch.Add(scoredScratch[index].b);
      }
      else
      {
        List<ButtonHandler.Button> buttonInfoByPage = ButtonHandler.GetButtonInfoByPage(Variables.currentPage);
        int num2 = 0;
        int num3 = Variables.currentCategoryPage * Variables.ButtonsPerPage;
        foreach (ButtonHandler.Button button in buttonInfoByPage)
        {
          if (button != null && Settings.ShouldShowButton(button.buttonText))
          {
            if (num2 >= num3 && newCacheScratch.Count < Variables.ButtonsPerPage)
              newCacheScratch.Add(button);
            ++num2;
          }
        }
      }
      if (newCacheScratch.Count > 0 || !flag)
      {
        Main._pageCache.Clear();
        Main._pageCache.AddRange((IEnumerable<ButtonHandler.Button>) newCacheScratch);
        Main._lastSearchInput = SearchAndKeyboard.inputText;
        Main._lastPageDrawn = Variables.currentPage;
        Main._lastPageIndex = Variables.currentCategoryPage;
        Main._lastDrawModVersion = NXOUI._modListVersion;
      }
      Main._lastWasSearching = flag;
    }
    Main._drawingPageButtons = true;
    for (int index = 0; index < Main._pageCache.Count; ++index)
      Main.AddModButtons((float) index * Variables.ButtonSpacing, Main._pageCache[index]);
    Main._drawingPageButtons = false;
  }

  private static void CreateBaseMenuObject()
  {
    Variables.menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    Variables.menuObj.name = "menu";
    UnityEngine.Object.Destroy((UnityEngine.Object) Variables.menuObj.GetComponent<Rigidbody>());
    UnityEngine.Object.Destroy((UnityEngine.Object) Variables.menuObj.GetComponent<BoxCollider>());
    UnityEngine.Object.Destroy((UnityEngine.Object) Variables.menuObj.GetComponent<Renderer>());
    Variables.menuObj.transform.localScale = Main.MenuSize;
  }

  private static void CreateMenuBackground()
  {
    Variables.background = Main.CreateCube("menucolor");
    Variables.background.transform.localScale = Main.BackgroundScale;
    Variables.background.transform.position = Main.BackgroundPos;
    Main.ApplyColor(Variables.background, (Color) Settings.BackgroundColor);
    Main.ApplyOutline(Variables.background, 1);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      extraParts = Main.RoundObj(Variables.background, roundness: Settings.MenuRoundness, transparentQueue: 2450);
    Main.RegisterColorGroup(Main.ColorRole.Background, Variables.background, extraParts);
  }

  public static void AddSearchBar()
  {
    if (!SearchAndKeyboard.isSearching)
      return;
    Main._drawingSearchBar = true;
    Variables.TopMenuButton = Main.CreateCube(keepCollider: true);
    Variables.TopMenuButton.transform.localScale = Main.SearchBarScale;
    Variables.TopMenuButton.transform.localPosition = Main.SearchBarPos;
    Main.ApplyColor(Variables.TopMenuButton, (Color) Settings.BackgroundColor);
    Main.ApplyOutline(Variables.TopMenuButton, 1);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      extraParts = Main.RoundObj(Variables.TopMenuButton, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Background, Variables.TopMenuButton, extraParts);
    SearchAndKeyboard.searchText = Main.CreateCanvasText();
    SearchAndKeyboard.searchText.alignment = (TextAnchor) 4;
    RectTransform component = ((Component) SearchAndKeyboard.searchText).GetComponent<RectTransform>();
    component.sizeDelta = new Vector2(0.2f, 0.02f);
    component.localScale = new Vector3(0.9f, 0.9f, 0.9f);
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.localPosition = Main.SearchTextPos;
    SearchAndKeyboard.searchText.text = !string.IsNullOrEmpty(SearchAndKeyboard.inputText) ? SearchAndKeyboard.inputText : SearchAndKeyboard.placeholderText;
    Main._drawingSearchBar = false;
  }

  public static void ShowSearchBar()
  {
    if ((UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null || (UnityEngine.Object) Variables.TopMenuButton != (UnityEngine.Object) null)
      return;
    Main.AddSearchBar();
    Main.SetUILayer(Variables.menuObj);
    Main.ApplySearchButtonState();
    Main.ApplyMenuScale();
  }

  public static void HideSearchBar()
  {
    for (int index = 0; index < Main._searchBarObjects.Count; ++index)
    {
      if ((UnityEngine.Object) Main._searchBarObjects[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._searchBarObjects[index]);
    }
    Main._searchBarObjects.Clear();
    for (int index = 0; index < Main._searchBarMaterials.Count; ++index)
    {
      if ((UnityEngine.Object) Main._searchBarMaterials[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._searchBarMaterials[index]);
    }
    Main._searchBarMaterials.Clear();
    for (int index = 0; index < Main._searchBarMeshes.Count; ++index)
    {
      if ((UnityEngine.Object) Main._searchBarMeshes[index] != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) Main._searchBarMeshes[index]);
    }
    Main._searchBarMeshes.Clear();
    Main._colorGroups.RemoveAll((Predicate<Main.TrackedColorGroup>) (g => g.renderers.Count == 0 || g.renderers.All<Renderer>((Func<Renderer, bool>) (r => (UnityEngine.Object) r == (UnityEngine.Object) null))));
    Variables.TopMenuButton = (GameObject) null;
    SearchAndKeyboard.searchText = (UnityEngine.UI.Text) null;
    Main.ApplySearchButtonState();
    Main.ApplyMenuScale();
    Main.RedrawButtonList();
  }

  private static void ApplySearchButtonState()
  {
    if (Main._searchButtonGroup != null)
      Main._searchButtonGroup.role = SearchAndKeyboard.isSearching ? Main.ColorRole.EnabledButton : Main.ColorRole.Button;
    Main._colorsDirty = true;
  }

  private static void ApplyMenuScale()
  {
    if (!((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
      return;
    Variables.menuObj.transform.localScale = Main.MenuSize * Main.menuScale;
  }

  public static void AddDisconnectButton()
  {
    GameObject cube = Main.CreateCube("DisconnectBg", keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, Main.HeaderIconScale.x * 2.504f, Main.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.2375f, 0.45f);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button("Disconnect Button", Category.Home, false, false, (Action) (() =>
    {
      if (PhotonNetwork.InRoom)
        Room.Disconnect();
      else
        Room.JoinRandomPublic();
    }));
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, gameObjectList);
    Main.disconnectButton = Main.CreateQuad();
    Main.disconnectButton.transform.localRotation = Quaternion.Euler(0.0f, 90f, 0.0f);
    Main.disconnectButton.transform.localScale = Main.HeaderIconScale;
    Main.disconnectButton.transform.localPosition = new Vector3(0.0795357153f, -0.2375f, 0.45f);
    Main.ApplyTexture(Main.disconnectButton, "NXO.Resources.disconnecticon.png", new Color?(Color.white));
    Main.UpdateDisconnectIcon();
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, Main.disconnectButton.transform);
  }

  public static void UpdateDisconnectIcon()
  {
    if ((UnityEngine.Object) Main.disconnectButton == (UnityEngine.Object) null)
      return;
    Renderer component = Main.disconnectButton.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || (UnityEngine.Object) component.sharedMaterial == (UnityEngine.Object) null)
      return;
    component.sharedMaterial.color = (Color) (PhotonNetwork.InRoom ? new Color32((byte) 235, (byte) 70, (byte) 60, byte.MaxValue) : new Color32((byte) 70, (byte) 210, (byte) 95, byte.MaxValue));
  }

  public static void AddFpsButton()
  {
    float num1 = Main.HeaderIconScale.x * 2.504f;
    float num2 = 0.1125f - num1;
    float y1 = num1 * 3.3f;
    float y2 = (float) ((double) num1 * 0.5 - 0.23749999701976776 + (double) num2 + (double) y1 * 0.5);
    GameObject cube = Main.CreateCube("FpsBg");
    cube.transform.localScale = new Vector3(0.0075f, y1, Main.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, y2, 0.45f);
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    Main.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      extraParts = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, extraParts);
    Main.fpsButton = cube;
    Main.fpsText = Main.CreateCanvasText();
    ((UnityEngine.Object) Main.fpsText).name = "FPS Text";
    Main.fpsText.alignment = (TextAnchor) 4;
    RectTransform component = ((Component) Main.fpsText).GetComponent<RectTransform>();
    component.localPosition = Vector3.zero;
    component.position = new Vector3(0.0795357153f * Main.MenuSize.x, y2 * Main.MenuSize.y, 0.45f * Main.MenuSize.z);
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = new Vector2(0.12f, 0.013f);
    Main.UpdateFpsCounter(true);
  }

  public static void UpdateFpsCounter(bool force = false)
  {
    if ((UnityEngine.Object) Main.fpsText == (UnityEngine.Object) null)
      return;
    Main._fpsSmooth = Mathf.Lerp(Main._fpsSmooth, 1f / Time.unscaledDeltaTime, Time.unscaledDeltaTime * 4f);
    Main._fpsTimer -= Time.unscaledDeltaTime;
    if (!force && (double) Main._fpsTimer > 0.0)
      return;
    Main._fpsTimer = 0.5f;
    Main.fpsText.text = $"v{"6.3"} | FPS: {Mathf.RoundToInt(Main._fpsSmooth)}";
  }

  public static void ToggleSearchButton()
  {
    if ((UnityEngine.Object) Variables.searchButton != (UnityEngine.Object) null)
      return;
    GameObject cube = Main.CreateCube("SearchBg", keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, Main.HeaderIconScale.x * 2.504f, Main.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.35f, 0.45f);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button("Toggle Search Button", Category.Home, false, false, (Action) null);
    Main.ApplyColor(cube, (Color) (SearchAndKeyboard.isSearching ? Settings.EnabledButtonColor : Settings.ButtonColor));
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(SearchAndKeyboard.isSearching ? Main.ColorRole.EnabledButton : Main.ColorRole.Button, cube, gameObjectList);
    Main._searchButtonGroup = Main._colorGroups.Count > 0 ? Main._colorGroups[Main._colorGroups.Count - 1] : (Main.TrackedColorGroup) null;
    Variables.searchButton = Main.CreateQuad();
    Variables.searchButton.transform.localRotation = Quaternion.Euler(0.0f, 90f, 0.0f);
    Variables.searchButton.transform.localScale = Main.HeaderIconScale;
    Variables.searchButton.transform.localPosition = new Vector3(0.0795357153f, -0.35f, 0.45f);
    Main.ApplyTexture(Variables.searchButton, "NXO.Resources.searchicon.png");
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, Variables.searchButton.transform);
  }

  private static void CreateMenuCanvasAndTitle()
  {
    Variables.canvasObj = new GameObject("canvas");
    Variables.canvasObj.transform.parent = Variables.menuObj.transform;
    Main._trackedObjects.Add(Variables.canvasObj);
    Canvas canvas = Variables.canvasObj.AddComponent<Canvas>();
    canvas.renderMode = (RenderMode) 2;
    canvas.sortingOrder = 100;
    Variables.canvasObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3000f;
    Variables.canvasObj.AddComponent<GraphicRaycaster>();
    Variables.title = Main.CreateCanvasText(4);
    Variables.title.alignment = (TextAnchor) 3;
    Variables.title.supportRichText = true;
    ((Graphic) Variables.title).color = (Color) Settings.TitleColor;
    (float y, float z) titleOffset = Main.TitleOffset;
    RectTransform component = ((Component) Variables.title).GetComponent<RectTransform>();
    component.localPosition = Vector3.zero;
    component.position = new Vector3(0.054f, titleOffset.y - 0.0065f, titleOffset.z);
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = Main.TitleSize;
  }

  private static UnityEngine.UI.Text CreateCanvasText(int fontSize = 3)
  {
    GameObject gameObject = new GameObject();
    gameObject.transform.SetParent(Variables.canvasObj.transform, false);
    Main.ObjectSink.Add(gameObject);
    UnityEngine.UI.Text canvasText = gameObject.AddComponent<UnityEngine.UI.Text>();
    canvasText.font = Main.CurrentFont;
    canvasText.fontStyle = (FontStyle) 0;
    ((Graphic) canvasText).color = (Color) (Color32) Color.white;
    canvasText.fontSize = fontSize;
    canvasText.alignment = (TextAnchor) 4;
    canvasText.resizeTextForBestFit = true;
    canvasText.resizeTextMinSize = 0;
    return canvasText;
  }

  private static void CreateAccentStrip()
  {
    if (Settings.CurrentAccentStripType == Settings.AccentStripType.Off)
      return;
    GameObject cube1 = Main.CreateCube();
    cube1.transform.localScale = Main.AccentStripScale;
    cube1.transform.localPosition = Main.AccentStripPos;
    Main.ApplyStripColor(cube1, (Color) Settings.AccentStripColor);
    Main.RegisterColorGroup(Main.ColorRole.AccentStrip, cube1, (List<GameObject>) null);
    if (Settings.CurrentAccentStripType != Settings.AccentStripType.Both)
      return;
    GameObject cube2 = Main.CreateCube();
    cube2.transform.localScale = Main.AccentStripScale;
    cube2.transform.localPosition = new Vector3(Main.AccentStripPos.x, Main.AccentStripPos.y, -0.27f);
    Main.ApplyStripColor(cube2, (Color) Settings.AccentStripColor2);
    Main.RegisterColorGroup(Main.ColorRole.AccentStrip, cube2, (List<GameObject>) null);
  }

  private static void ApplyStripColor(GameObject obj, Color color)
  {
    Renderer component = obj.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    Material mat;
    if ((double) Settings.MenuOpacity >= 1.0)
    {
      mat = new Material(Variables.uberShader)
      {
        color = color
      };
    }
    else
    {
      mat = new Material(Variables.uiShader);
      mat.color = color with { a = Settings.MenuOpacity };
      mat.renderQueue = 2460;
    }
    component.sharedMaterial = Main.TrackMat(mat);
  }

  private static void CreateButtonBackdrop(float offset)
  {
    GameObject cube = Main.CreateCube("Backdrop");
    cube.transform.localScale = new Vector3(0.0015f, 0.82f, 0.08f);
    cube.transform.localPosition = new Vector3(0.0685f, 0.0f, 0.335f - offset);
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> extraParts = Main.RoundObj(cube, roundness: 1.6f, transparentQueue: 2461, seg: 12);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, extraParts);
  }

  public static void AddModButtons(float offset, ButtonHandler.Button button)
  {
    if (button == null)
      return;
    if (Settings.ClassicButtons)
    {
      Main.AddModButtonsClassic(offset, button);
    }
    else
    {
      Main.CreateButtonBackdrop(offset);
      if (button.incremental)
      {
        Main.CreateIncrementalButton("DOWN : " + button.buttonText, button, offset, -0.22f, "<", "_DOWN");
        Main.CreateIncrementalButton("UP : " + button.buttonText, button, offset, -0.3525f, ">", "_UP");
      }
      bool flag = !button.showGear;
      GameObject gameObject = (GameObject) null;
      ButtonHandler.BtnCollider col = (ButtonHandler.BtnCollider) null;
      List<GameObject> gameObjectList = (List<GameObject>) null;
      List<GameObject> outline = (List<GameObject>) null;
      if (!button.incremental)
      {
        float y1 = button.showGear ? 0.695f : 0.15f;
        float y2 = button.showGear ? 1f / 16f : -0.3375f;
        float z = button.showGear ? 0.08f : 0.058f;
        gameObject = Main.CreateCube("Button: " + button.buttonText, keepCollider: true);
        gameObject.transform.localScale = new Vector3(0.0075f, y1, z);
        gameObject.transform.localPosition = new Vector3(0.07525f, y2, 0.335f - offset);
        col = gameObject.AddComponent<ButtonHandler.BtnCollider>();
        col.clickedButton = button;
        Main.ApplyColor(gameObject, (Color) (button.Enabled ? Settings.EnabledButtonColor : Settings.ButtonColor));
        if (button.showGear)
        {
          outline = Main.ApplyOutline(gameObject, 2, 0.0065f, false);
          if ((double) Settings.MenuRoundness > 0.0)
            gameObjectList = Main.RoundObj(gameObject, roundness: Settings.MenuRoundness);
        }
        else
        {
          outline = Main.ApplyOutline(gameObject, 2, 0.0065f, false, 1.6f, 12);
          gameObjectList = Main.RoundObj(gameObject, roundness: 1.6f, transparentQueue: 2462, seg: 12);
        }
        Main.RegisterColorGroup(button.Enabled ? Main.ColorRole.EnabledButton : Main.ColorRole.Button, gameObject, gameObjectList);
        if (button.showGear)
          Main.CreateQuickActionButton(button, offset);
        else
          Main.CreateToggleKnob(button, offset, col, gameObject, gameObjectList);
      }
      UnityEngine.UI.Text canvasText = Main.CreateCanvasText();
      ((UnityEngine.Object) canvasText).name = "Button: Text - " + button.buttonText;
      canvasText.text = ButtonHandler.FavoriteMods.Contains(button) ? "<color=blue>★</color> " + button.buttonText : button.buttonText;
      if (flag)
        canvasText.alignment = (TextAnchor) 3;
      float num = 1.2f;
      ((Component) canvasText).gameObject.transform.localScale = new Vector3(num, num, 1f);
      RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
      float y = flag ? 0.035f : (button.showGear ? 0.015f : 0.0f);
      component.localPosition = new Vector3(flag ? 0.054f : 0.054f, y, Main.TextOffset - offset / 2.225f);
      component.localRotation = Quaternion.Euler(180f, 90f, 90f);
      Vector2 modTextSize = Main.ModTextSize;
      component.sizeDelta = new Vector2(flag ? 0.125f : modTextSize.x / num, modTextSize.y);
      Transform transform = (Transform) null;
      if (!((UnityEngine.Object) col != (UnityEngine.Object) null))
        return;
      Main.RegisterClickAnim(col, gameObject, gameObjectList, outline, flag ? (Transform) null : ((Component) canvasText).transform, transform);
      if (Main.MenuAnimations && button.isToggle && button.Enabled && button.showGear)
        col.ApplyFactor(0.92f);
    }
  }

  private static void CreateToggleKnob(
    ButtonHandler.Button button,
    float offset,
    ButtonHandler.BtnCollider col,
    GameObject pill,
    List<GameObject> pillParts)
  {
    float z = 0.0464f;
    float y = z * (Main.MenuSize.z / Main.MenuSize.y);
    float num = (float) ((0.15000000596046448 - (double) y) * 0.5 - 0.0068999999202787876);
    float offY = num - 0.3375f;
    float onY = -0.3375f - num;
    GameObject cube = Main.CreateCube("Knob: " + button.buttonText);
    cube.transform.localScale = new Vector3(0.0075f, y, z);
    cube.transform.localPosition = new Vector3(0.07825f, button.Enabled ? onY : offY, 0.335f - offset);
    Color a = (Color) (button.Enabled ? Settings.EnabledButtonColor : Settings.ButtonColor);
    Main.ApplyColor(cube, Color.Lerp(a, Color.white, 0.6f));
    List<GameObject> gameObjectList = Main.RoundObj(cube, roundness: 2f, transparentQueue: 2466, seg: 14);
    Transform knob = gameObjectList == null || gameObjectList.Count <= 0 ? cube.transform : gameObjectList[0].transform;
    if (!((UnityEngine.Object) col != (UnityEngine.Object) null))
      return;
    List<Renderer> pill1 = new List<Renderer>(2);
    if (pillParts != null)
    {
      foreach (GameObject pillPart in pillParts)
      {
        Renderer component = (UnityEngine.Object) pillPart != (UnityEngine.Object) null ? pillPart.GetComponent<Renderer>() : (Renderer) null;
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          pill1.Add(component);
      }
    }
    if (pill1.Count == 0 && (UnityEngine.Object) pill != (UnityEngine.Object) null)
    {
      Renderer component = pill.GetComponent<Renderer>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        pill1.Add(component);
    }
    col.SetupPulse(knob, knob.GetComponent<Renderer>(), offY, onY, pill1, (Color) Settings.ButtonColor, (Color) Settings.EnabledButtonColor);
  }

  private static void CreateIncrementalButton(
    string name,
    ButtonHandler.Button parent,
    float offset,
    float yPos,
    string symbol,
    string suffix)
  {
    GameObject cube = Main.CreateCube(name, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, 0.335f - offset);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button(parent.buttonText + suffix, parent.Page, false, false, (Action) null);
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = Main.CreateCanvasText();
    canvasText.text = symbol;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    component.localPosition = new Vector3(0.05975f, yPos * 0.288f, Main.TextOffset - offset / 2.225f);
    component.sizeDelta = Main.SubTextSize;
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
    if (parent != ButtonHandler._bounceIncParent || suffix == "_UP" != ButtonHandler._bounceIncUp)
      return;
    ButtonHandler._bounceIncParent = (ButtonHandler.Button) null;
    col.PlayBounce();
  }

  public static void AddModButtonsClassic(float offset, ButtonHandler.Button button)
  {
    if (button.incremental)
    {
      Main.CreateIncrementalButtonClassic("DOWN : " + button.buttonText, button, offset, 0.3525f, "<", "_DOWN");
      Main.CreateIncrementalButtonClassic("UP : " + button.buttonText, button, offset, -0.3525f, ">", "_UP");
    }
    float y1 = button.incremental ? 0.555f : (button.showGear ? 0.695f : 0.82f);
    float y2 = !button.showGear || button.incremental ? 0.0f : 1f / 16f;
    GameObject cube = Main.CreateCube("Button: " + button.buttonText, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, y1, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, y2, 0.335f - offset);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = button;
    Main.ApplyColor(cube, (Color) (button.Enabled ? Settings.EnabledButtonColor : Settings.ButtonColor));
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(button.Enabled ? Main.ColorRole.EnabledButton : Main.ColorRole.Button, cube, gameObjectList);
    if (button.showGear && !button.incremental)
      Main.CreateQuickActionButton(button, offset);
    UnityEngine.UI.Text canvasText1 = Main.CreateCanvasText();
    ((UnityEngine.Object) canvasText1).name = "Button: Text - " + button.buttonText;
    canvasText1.text = ButtonHandler.FavoriteMods.Contains(button) ? "<color=blue>★</color> " + button.buttonText : button.buttonText;
    float num = 1.2f;
    ((Component) canvasText1).gameObject.transform.localScale = new Vector3(num, num, 1f);
    RectTransform component1 = ((Component) canvasText1).GetComponent<RectTransform>();
    component1.localPosition = new Vector3(0.05975f, button.showGear ? 0.015f : 0.0f, Main.TextOffset - offset / 2.225f);
    component1.localRotation = Quaternion.Euler(180f, 90f, 90f);
    Vector2 vector2 = button.incremental ? Main.IncrementalTextSize : Main.ModTextSize;
    component1.sizeDelta = new Vector2(vector2.x / num, vector2.y);
    Transform transform = (Transform) null;
    if (ButtonHandler.IsCategoryButton(button))
    {
      UnityEngine.UI.Text canvasText2 = Main.CreateCanvasText();
      ((UnityEngine.Object) canvasText2).name = "Button: Arrow - " + button.buttonText;
      canvasText2.text = button.buttonText == "Return" ? "<color=red><</color>" : $"<color=#{ColorUtility.ToHtmlStringRGB((Color) Settings.EnabledButtonColor)}>></color>";
      canvasText2.alignment = (TextAnchor) 3;
      ((Component) canvasText2).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
      RectTransform component2 = ((Component) canvasText2).GetComponent<RectTransform>();
      component2.localPosition = new Vector3(0.05975f, button.showGear ? 0.012f : -0.01125f, Main.TextOffset - offset / 2.225f);
      component2.localRotation = Quaternion.Euler(180f, 90f, 90f);
      component2.sizeDelta = button.incremental ? Main.IncrementalTextSize : Main.ModTextSize;
      transform = ((Component) canvasText2).transform;
    }
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText1).transform, transform);
    if (button != ButtonHandler._bouncePending)
      return;
    ButtonHandler._bouncePending = (ButtonHandler.Button) null;
    col.PlayBounce();
  }

  private static void CreateIncrementalButtonClassic(
    string name,
    ButtonHandler.Button parent,
    float offset,
    float yPos,
    string symbol,
    string suffix)
  {
    GameObject cube = Main.CreateCube(name, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, 0.335f - offset);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button(parent.buttonText + suffix, parent.Page, false, false, (Action) null);
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = Main.CreateCanvasText();
    canvasText.text = symbol;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    float incrementTextOffset = Main.IncrementTextOffset;
    float y = (double) yPos < 0.0 ? -0.1025f : 0.1015f;
    component.localPosition = new Vector3(0.05975f, y, incrementTextOffset - offset / 2.225f);
    component.sizeDelta = Main.SubTextSize;
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
    if (parent != ButtonHandler._bounceIncParent || suffix == "_UP" != ButtonHandler._bounceIncUp)
      return;
    ButtonHandler._bounceIncParent = (ButtonHandler.Button) null;
    col.PlayBounce();
  }

  private static void CreateQuickActionButton(ButtonHandler.Button parent, float offset)
  {
    GameObject cube = Main.CreateCube("Gear: " + parent.buttonText, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.3525f, 0.335f - offset);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button("_GEAR_" + parent.buttonText, parent.Page, false, false, (Action) (() => ButtonHandler.OpenGear_Menu(parent)));
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = Main.CreateCanvasText();
    canvasText.text = "⚙";
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    component.localPosition = new Vector3(0.05975f, -0.1055f, Main.TextOffset - offset / 2.225f);
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = Main.GearIconSize;
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
  }

  private static void AddPageButton(string direction, float yPos)
  {
    GameObject cube = Main.CreateCube(keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.25f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, -0.33f);
    ButtonHandler.BtnCollider col = cube.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button(direction, Category.Home, false, false, (Action) null);
    Main.ApplyColor(cube, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(cube, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = Main.CreateCanvasText();
    canvasText.text = direction;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    component.localPosition = Vector3.zero;
    component.sizeDelta = Main.PageTextSize;
    component.localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.localPosition = new Vector3(0.05975f, direction == "<" ? 0.083f : -0.083f, Main.PageZOffset);
    Main.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
  }

  public static void AddReturnButton()
  {
    Variables.BackToStartButton = Main.CreateCube(keepCollider: true);
    Variables.BackToStartButton.transform.localScale = new Vector3(0.0075f, 0.2925f, 0.08f);
    Variables.BackToStartButton.transform.localPosition = new Vector3(0.07525f, 0.0f, -0.33f);
    ButtonHandler.BtnCollider col = Variables.BackToStartButton.AddComponent<ButtonHandler.BtnCollider>();
    col.clickedButton = new ButtonHandler.Button("ReturnButton", Category.Home, false, false, (Action) null);
    Main.ApplyColor(Variables.BackToStartButton, (Color) Settings.ButtonColor);
    List<GameObject> outline = Main.ApplyOutline(Variables.BackToStartButton, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      gameObjectList = Main.RoundObj(Variables.BackToStartButton, roundness: Settings.MenuRoundness);
    Main.RegisterColorGroup(Main.ColorRole.Button, Variables.BackToStartButton, gameObjectList);
    GameObject quad = Main.CreateQuad(parent: Variables.canvasObj.transform);
    quad.transform.localScale = Main.HomeIconScale;
    quad.transform.localPosition = new Vector3(0.05975f, 0.0f, -0.14875f);
    quad.transform.localRotation = Quaternion.Euler(0.0f, -90f, -90f);
    Main.ApplyTexture(quad, "NXO.Resources.homeicon.png");
    Main.RegisterClickAnim(col, Variables.BackToStartButton, gameObjectList, outline, quad.transform);
  }

  public static void AddTitleAndFPSCounter()
  {
    if ((UnityEngine.Object) Variables.title == (UnityEngine.Object) null)
      return;
    Settings.ColorMode titleMode = Settings.TitleMode;
    if (titleMode == Settings.ColorMode.Gradient && Settings.TitleAnimatedGradient)
    {
      if ((double) Time.time - (double) Main._lastTitleUpdateTime < 0.029999999329447746)
        return;
      Main._lastTitleUpdateTime = Time.time;
      Variables.title.text = Main.BuildAnimatedTitle();
      Main._titleTextMode = ~Settings.ColorMode.Solid;
    }
    else if (titleMode == Settings.ColorMode.Gradient)
    {
      Color32 titleColor = Settings.TitleColor;
      Color32 titleColor2 = Settings.TitleColor2;
      if (Main._titleTextMode == titleMode && Main.SameColor(titleColor, Main._titleTextC1) && Main.SameColor(titleColor2, Main._titleTextC2))
        return;
      Variables.title.text = Main.BuildStaticGradientTitle();
      Main._titleTextMode = titleMode;
      Main._titleTextC1 = titleColor;
      Main._titleTextC2 = titleColor2;
    }
    else
    {
      if (Main._titleTextMode == titleMode)
        return;
      Variables.title.text = "<b>NXO</b>";
      Main._titleTextMode = titleMode;
    }
  }

  private static bool SameColor(Color32 a, Color32 b)
  {
    return (int) a.r == (int) b.r && (int) a.g == (int) b.g && (int) a.b == (int) b.b;
  }

  private static string BuildAnimatedTitle()
  {
    Main._titleBuilder.Clear();
    Main._titleBuilder.Append("<b>");
    string str = "NXO";
    for (int index = 0; index < str.Length; ++index)
    {
      float t = (float) (((double) Mathf.Sin((float) ((double) Time.time * (double) Settings.TitleAnimSpeed * 2.0 - (double) index * 0.5)) + 1.0) / 2.0);
      Color color = Color.Lerp((Color) Settings.TitleColor, (Color) Settings.TitleColor2, t);
      Main._titleBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(str[index]).Append("</color>");
    }
    Main._titleBuilder.Append("</b>");
    return Main._titleBuilder.ToString();
  }

  private static string BuildStaticGradientTitle()
  {
    Main._titleBuilder.Clear();
    Main._titleBuilder.Append("<b>");
    string str = "NXO";
    for (int index = 0; index < str.Length; ++index)
    {
      float t = str.Length > 1 ? (float) index / (float) (str.Length - 1) : 0.0f;
      Color color = Color.Lerp((Color) Settings.TitleColor, (Color) Settings.TitleColor2, t);
      Main._titleBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(str[index]).Append("</color>");
    }
    Main._titleBuilder.Append("</b>");
    return Main._titleBuilder.ToString();
  }

  public static void AddButtonClicker(Transform parentTransform)
  {
    if ((UnityEngine.Object) Variables.clickerObj != (UnityEngine.Object) null)
      return;
    Variables.clickerObj = new GameObject("buttonclicker");
    BoxCollider boxCollider = Variables.clickerObj.AddComponent<BoxCollider>();
    ((Collider) boxCollider).isTrigger = true;
    Main._clickerCollider = (Collider) boxCollider;
    Variables.clickerObj.layer = LayerMask.NameToLayer("UI");
    Variables.clickerObj.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
    if ((UnityEngine.Object) Main._clickerMaterial == (UnityEngine.Object) null)
      Main._clickerMaterial = new Material(Variables.uberShader)
      {
        color = Color.white
      };
    Main._clickerRenderer = Variables.clickerObj.AddComponent<MeshRenderer>();
    Main._clickerRenderer.sharedMaterial = Main._clickerMaterial;
    if ((UnityEngine.Object) parentTransform == (UnityEngine.Object) null)
      return;
    Variables.clickerObj.transform.SetParent(parentTransform);
    Variables.clickerObj.transform.localScale = Vector3.one * 0.0035f;
    Variables.clickerObj.transform.localPosition = Main.ClickerLocalPos;
  }

  private static void DestroyClicker()
  {
    if ((UnityEngine.Object) Variables.clickerObj != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) Variables.clickerObj);
      Variables.clickerObj = (GameObject) null;
    }
    Main._clickerRenderer = (MeshRenderer) null;
    Main._clickerCollider = (Collider) null;
    if (!((UnityEngine.Object) Main._clickerMaterial != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) Main._clickerMaterial);
    Main._clickerMaterial = (Material) null;
  }

  public static void CleanupMenu()
  {
    PlayersActionList.ClearPlayerCam();
    Main._colorGroups.Clear();
    Main._colorsDirty = true;
    foreach (GameObject pageButtonObject in Main._pageButtonObjects)
    {
      if ((UnityEngine.Object) pageButtonObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) pageButtonObject);
    }
    Main._pageButtonObjects.Clear();
    foreach (GameObject trackedObject in Main._trackedObjects)
    {
      if ((UnityEngine.Object) trackedObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) trackedObject);
    }
    Main._trackedObjects.Clear();
    Main.DestroyAndNullify<GameObject>(ref Variables.menuObj);
    Main.DestroyClicker();
    foreach (Material pageButtonMaterial in Main._pageButtonMaterials)
    {
      if ((UnityEngine.Object) pageButtonMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) pageButtonMaterial);
    }
    Main._pageButtonMaterials.Clear();
    foreach (Material dynamicMaterial in Main._dynamicMaterials)
    {
      if ((UnityEngine.Object) dynamicMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) dynamicMaterial);
    }
    Main._dynamicMaterials.Clear();
    foreach (Material pinwheelMaterial in Main._dynamicPinwheelMaterials)
    {
      if ((UnityEngine.Object) pinwheelMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) pinwheelMaterial);
    }
    Main._dynamicPinwheelMaterials.Clear();
    Main._activePinwheelMaterial = (Material) null;
    foreach (Mesh pageButtonMesh in Main._pageButtonMeshes)
    {
      if ((UnityEngine.Object) pageButtonMesh != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) pageButtonMesh);
    }
    Main._pageButtonMeshes.Clear();
    foreach (Mesh dynamicMesh in Main._dynamicMeshes)
    {
      if ((UnityEngine.Object) dynamicMesh != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) dynamicMesh);
    }
    Main._dynamicMeshes.Clear();
    foreach (GameObject searchBarObject in Main._searchBarObjects)
    {
      if ((UnityEngine.Object) searchBarObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) searchBarObject);
    }
    Main._searchBarObjects.Clear();
    foreach (Material searchBarMaterial in Main._searchBarMaterials)
    {
      if ((UnityEngine.Object) searchBarMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) searchBarMaterial);
    }
    Main._searchBarMaterials.Clear();
    foreach (Mesh searchBarMesh in Main._searchBarMeshes)
    {
      if ((UnityEngine.Object) searchBarMesh != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) searchBarMesh);
    }
    Main._searchBarMeshes.Clear();
    Variables.background = (GameObject) null;
    Variables.TopMenuButton = (GameObject) null;
    Variables.searchButton = (GameObject) null;
    Main.disconnectButton = (GameObject) null;
    Main.fpsButton = (GameObject) null;
    Main.fpsText = (UnityEngine.UI.Text) null;
    Variables.canvasObj = (GameObject) null;
    Variables.BackToStartButton = (GameObject) null;
    Variables.title = (UnityEngine.UI.Text) null;
    SearchAndKeyboard.searchText = (UnityEngine.UI.Text) null;
    Main._searchButtonGroup = (Main.TrackedColorGroup) null;
    Main._pageCache.Clear();
    Main._lastDrawModVersion = -1;
    Main._lastWasSearching = false;
    Main._lastSearchInput = (string) null;
    Main._drawingPageButtons = false;
    Main._drawingSearchBar = false;
  }

  public static void RefreshMenu()
  {
    ++NXOUI.MenuContentVersion;
    Main.CleanupMenu();
    Main.Draw();
    SearchAndKeyboard.ReRegisterKeyboardColorGroups();
  }

  public static void SetUILayer(GameObject obj)
  {
    if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
      return;
    obj.layer = LayerMask.NameToLayer("UI");
    foreach (Transform transform in obj.transform)
    {
      if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
        Main.SetUILayer(transform.gameObject);
    }
  }

  public static void DestroyAndNullify<T>(ref T obj, float delay = 0.0f) where T : UnityEngine.Object
  {
    if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
      return;
    if (obj is Component component && (UnityEngine.Object) component != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject, delay);
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) obj, delay);
    obj = default (T);
  }

  public static List<GameObject> RoundObj(
    GameObject toRound,
    Transform parent = null,
    List<Material> materialList = null,
    float roundness = 0.5f,
    int transparentQueue = 2460,
    int seg = 5)
  {
    List<GameObject> gameObjectList = new List<GameObject>(1);
    if ((UnityEngine.Object) toRound == (UnityEngine.Object) null)
      return gameObjectList;
    Renderer component = toRound.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return gameObjectList;
    if ((UnityEngine.Object) parent == (UnityEngine.Object) null)
      parent = Variables.menuObj.transform;
    if (materialList == null)
      materialList = Main.MaterialSink;
    Vector3 localScale = toRound.transform.localScale;
    Vector3 lossyScale = parent.lossyScale;
    float x = localScale.x * 0.5f;
    float a1 = localScale.y * 0.5f * lossyScale.y;
    float b = localScale.z * 0.5f * lossyScale.z;
    float num1 = Mathf.Min(7f / 400f * roundness, Mathf.Min(a1, b) * 0.9f);
    float num2 = (a1 - num1) / lossyScale.y;
    float num3 = (b - num1) / lossyScale.z;
    float num4 = num1 / lossyScale.y;
    float num5 = num1 / lossyScale.z;
    List<Vector2> vector2List = new List<Vector2>((seg + 1) * 4);
    float[] numArray1 = new float[4]
    {
      num2,
      -num2,
      -num2,
      num2
    };
    float[] numArray2 = new float[4]
    {
      num3,
      num3,
      -num3,
      -num3
    };
    for (int index1 = 0; index1 < 4; ++index1)
    {
      float a2 = (float) ((double) index1 * 3.1415927410125732 * 0.5);
      float num6 = numArray1[index1];
      float num7 = numArray2[index1];
      for (int index2 = 0; index2 <= seg; ++index2)
      {
        float f = Mathf.Lerp(a2, a2 + 1.57079637f, (float) index2 / (float) seg);
        vector2List.Add(new Vector2(num6 + Mathf.Cos(f) * num4, num7 + Mathf.Sin(f) * num5));
      }
    }
    int count1 = vector2List.Count;
    List<Vector3> inVertices = new List<Vector3>(count1 * 4 + 4);
    List<Vector3> inNormals = new List<Vector3>(count1 * 4 + 4);
    List<int> triangles = new List<int>(count1 * 12);
    int count2 = inVertices.Count;
    inVertices.Add(new Vector3(x, 0.0f, 0.0f));
    inNormals.Add(Vector3.right);
    int count3 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      inVertices.Add(new Vector3(x, vector2List[index].x, vector2List[index].y));
      inNormals.Add(Vector3.right);
    }
    for (int index = 0; index < count1; ++index)
    {
      triangles.Add(count2);
      triangles.Add(count3 + (index + 1) % count1);
      triangles.Add(count3 + index);
    }
    int count4 = inVertices.Count;
    inVertices.Add(new Vector3(-x, 0.0f, 0.0f));
    inNormals.Add(Vector3.left);
    int count5 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      inVertices.Add(new Vector3(-x, vector2List[index].x, vector2List[index].y));
      inNormals.Add(Vector3.left);
    }
    for (int index = 0; index < count1; ++index)
    {
      triangles.Add(count4);
      triangles.Add(count5 + index);
      triangles.Add(count5 + (index + 1) % count1);
    }
    int count6 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      Vector3 normalized = new Vector3(0.0f, vector2List[index].x, vector2List[index].y).normalized;
      inVertices.Add(new Vector3(x, vector2List[index].x, vector2List[index].y));
      inNormals.Add(normalized);
      inVertices.Add(new Vector3(-x, vector2List[index].x, vector2List[index].y));
      inNormals.Add(normalized);
    }
    for (int index = 0; index < count1; ++index)
    {
      int num8 = (index + 1) % count1;
      int num9 = count6 + index * 2;
      int num10 = num9 + 1;
      int num11 = count6 + num8 * 2;
      int num12 = num11 + 1;
      triangles.Add(num9);
      triangles.Add(num11);
      triangles.Add(num10);
      triangles.Add(num11);
      triangles.Add(num12);
      triangles.Add(num10);
    }
    int count7 = triangles.Count;
    for (int index = 0; index < count7; index += 3)
    {
      triangles.Add(triangles[index]);
      triangles.Add(triangles[index + 2]);
      triangles.Add(triangles[index + 1]);
    }
    Mesh mesh1 = new Mesh();
    mesh1.name = "RoundedMesh";
    Mesh mesh2 = mesh1;
    mesh2.SetVertices(inVertices);
    mesh2.SetNormals(inNormals);
    mesh2.SetTriangles(triangles, 0);
    mesh2.RecalculateBounds();
    Main.MeshSink.Add(mesh2);
    GameObject gameObject = new GameObject("rounded_" + toRound.name);
    gameObject.transform.SetParent(parent, false);
    gameObject.transform.localPosition = toRound.transform.localPosition;
    gameObject.transform.localRotation = toRound.transform.localRotation;
    gameObject.transform.localScale = Vector3.one;
    gameObject.AddComponent<MeshFilter>().mesh = mesh2;
    Material material;
    if ((double) Settings.MenuOpacity >= 1.0)
    {
      material = new Material(Variables.uberShader)
      {
        color = component.sharedMaterial.color
      };
    }
    else
    {
      material = new Material(Variables.uiShader);
      material.color = component.sharedMaterial.color with
      {
        a = Settings.MenuOpacity
      };
      material.renderQueue = transparentQueue;
      material.SetInt("_Cull", 0);
    }
    gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    materialList.Add(material);
    Main.ObjectSink.Add(gameObject);
    component.enabled = false;
    gameObjectList.Add(gameObject);
    return gameObjectList;
  }

  public static IEnumerator BounceAnim(GameObject obj, Vector3 target, bool cleanupAfter = false)
  {
    if (!((UnityEngine.Object) obj == (UnityEngine.Object) null))
    {
      Vector3 start = obj.transform.localScale;
      if (cleanupAfter)
      {
        if (Main.MenuAnimations)
          yield return (object) Main.ScaleOverTime(obj, start, target, 0.2f);
      }
      else if (Main.MenuAnimations)
      {
        float elapsed = 0.0f;
        while ((double) elapsed < 0.800000011920929)
        {
          if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
            yield break;
          float t = elapsed / 0.8f;
          obj.transform.localScale = Vector3.LerpUnclamped(start, target, Main.ElasticEaseOut(t));
          elapsed += Time.deltaTime;
          yield return (object) null;
        }
        if ((UnityEngine.Object) obj != (UnityEngine.Object) null)
          obj.transform.localScale = target;
      }
      else
        obj.transform.localScale = target;
      if (cleanupAfter)
      {
        Main.CleanupMenu();
        Main._wasInMenuCondition = false;
        Main._wasInPcCondition = false;
        Variables.InMenuCondition = false;
        Variables.InPcCondition = false;
      }
    }
  }

  private static float ElasticEaseOut(float t)
  {
    if ((double) t <= 0.0)
      return 0.0f;
    return (double) t >= 1.0 ? 1f : (float) (1.0499999523162842 * (double) Mathf.Pow(2f, -10f * t) * (double) Mathf.Sin((float) (((double) t - 0.075000002980232239) * 6.2831854820251465 / 0.30000001192092896)) + 1.0);
  }

  private static IEnumerator ScaleOverTime(
    GameObject obj,
    Vector3 from,
    Vector3 to,
    float duration)
  {
    float elapsed = 0.0f;
    while ((double) elapsed < (double) duration)
    {
      if ((UnityEngine.Object) obj == (UnityEngine.Object) null)
        yield break;
      float t = elapsed / duration;
      obj.transform.localScale = Vector3.Lerp(from, to, 1f - Mathf.Pow(1f - t, 3f));
      elapsed += Time.deltaTime;
      yield return (object) null;
    }
    if ((UnityEngine.Object) obj != (UnityEngine.Object) null)
      obj.transform.localScale = to;
  }

  public static Material GetPinwheelMaterial()
  {
    if ((UnityEngine.Object) Main._cachedPinwheelBase == (UnityEngine.Object) null)
      Main._cachedPinwheelBase = AssetHandler.LoadMaterial("NXO.Resources.pinwheelshader", "outline2");
    if ((UnityEngine.Object) Main._cachedPinwheelBase == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Failed to load pinwheel material");
      return (Material) null;
    }
    Material mat = new Material(Main._cachedPinwheelBase);
    AssetHandler.SetProperty(mat, "_Speed", (object) (float) -(double) Main.pinwheelSpeed);
    AssetHandler.SetProperty(mat, "_COLOR1", (object) Main.pinwheelColor1);
    AssetHandler.SetProperty(mat, "_COLOR2", (object) Main.pinwheelColor2);
    return mat;
  }

  public static void LoadFonts()
  {
    Main.CleanupFonts();
    Main.RegisterFont("Arial", Resources.GetBuiltinResource<Font>("Arial.ttf"));
    Font dynamicFontFromOsFont = Font.CreateDynamicFontFromOSFont("Comic Sans MS", 22);
    if ((UnityEngine.Object) dynamicFontFromOsFont != (UnityEngine.Object) null)
    {
      Main._dynamicFonts.Add(dynamicFontFromOsFont);
      Main.RegisterFont("Comic Sans", dynamicFontFromOsFont);
    }
    Main.RegisterFontFromBundle("Minecraft", "NXO.Resources.minecraftfont", "Minecraftia-Regular");
  }

  public static void RegisterFont(string description, Font font)
  {
    if (!((UnityEngine.Object) font != (UnityEngine.Object) null))
      return;
    Main._fonts.Add(new Main.FontEntry()
    {
      Description = description,
      Font = font
    });
  }

  public static Material CreateColorMaterial(Color color, int renderQueue = 2460)
  {
    if ((double) Settings.MenuOpacity >= 1.0)
      return new Material(Variables.uberShader)
      {
        color = color
      };
    return new Material(Variables.uiShader)
    {
      color = color with { a = Settings.MenuOpacity },
      renderQueue = renderQueue
    };
  }

  public static void RegisterFontFromBundle(string description, string bundlePath, string fontName)
  {
    AssetBundle assetBundle = AssetHandler.LoadAssetBundle(bundlePath);
    if ((UnityEngine.Object) assetBundle == (UnityEngine.Object) null)
      return;
    Font font = assetBundle.LoadAsset<Font>(fontName);
    if (!((UnityEngine.Object) font != (UnityEngine.Object) null))
      return;
    Main.RegisterFont(description, font);
  }

  public static void CleanupFonts()
  {
    Main._fonts.Clear();
    foreach (Font dynamicFont in Main._dynamicFonts)
    {
      if ((UnityEngine.Object) dynamicFont != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) dynamicFont);
    }
    Main._dynamicFonts.Clear();
  }

  private struct FadeItem
  {
    public Renderer r;
    public UnityEngine.UI.Text txt;
    public float origA;
    public Color target;
  }

  public enum ColorRole
  {
    None,
    Background,
    Button,
    EnabledButton,
    Outline,
    AccentStrip,
  }

  private class TrackedColorGroup
  {
    public Main.ColorRole role;
    public List<Renderer> renderers = new List<Renderer>(8);
  }

  public class FontEntry
  {
    public string Description;
    public Font Font;
  }
}
