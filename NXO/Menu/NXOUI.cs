
using BepInEx;
using GorillaNetworking;
using NXO.Mods;
using NXO.Mods.Categories;
using NXO.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable
namespace NXO.Menu;

[DefaultExecutionOrder(10000)]
public class NXOUI : MonoBehaviour
{
  public static bool IsGuiVisible = true;
  public static readonly List<string> ModNotifications = new List<string>();
  public static bool freecamEnabled;
  public static bool firstPersonEnabled;
  public static bool buttonClickerEnabled;
  public static float GlobalHeight = 400f;
  public static float GlobalWidth = 540f;
  public static int _modListVersion;
  public static int MenuContentVersion;
  private static bool _resCaptured;
  private static Vector3? _oldLocalPos;
  private static readonly Dictionary<string, float> _modEntryTimes = new Dictionary<string, float>();
  private static readonly Dictionary<string, float> _modRemoveTimes = new Dictionary<string, float>();
  private static readonly List<string> _removeBuffer = new List<string>();
  private static readonly HashSet<string> _modNotificationSet = new HashSet<string>();
  private static readonly List<(string mod, float w)> _sortedMods = new List<(string, float)>(32 /*0x20*/);
  private readonly Dictionary<int, Texture2D> _modBarTexCache = new Dictionary<int, Texture2D>();
  private readonly Dictionary<int, float> _modBarTexUsed = new Dictionary<int, float>();
  private const int ModBarCacheCap = 48 /*0x30*/;
  private int _lastWatermarkFps = -1;
  private string _cachedWatermarkText;
  private float _cachedWatermarkW;
  private Texture2D _watermarkTex;
  private Texture2D _bgTex;
  private Rect _win;
  private string _inputUpper = "";
  private bool _pcPage = true;
  private int _lastW;
  private int _lastH;
  private bool _dirty = true;
  private Font _font;
  private int _btnIndex;
  private readonly Dictionary<string, Texture2D> _tex = new Dictionary<string, Texture2D>(16 /*0x10*/);
  private readonly Dictionary<long, Texture2D> _roundCache = new Dictionary<long, Texture2D>(64 /*0x40*/);
  private readonly Dictionary<long, float> _roundUsed = new Dictionary<long, float>(64 /*0x40*/);
  private const int RoundCacheCap = 256 /*0x0100*/;
  private Texture2D _pixel;
  private float _smoothFps;
  private float _fpsUpdateTimer;
  private int _displayedFps;
  private GUIStyle _sField;
  private GUIStyle _sBtn;
  private GUIStyle _sBtnOn;
  private GUIStyle _sMod;
  private GUIStyle _sWatermark;
  private GUIStyle _sTitle;
  private GUIStyle _sDim;
  private GUIStyle _sSmall;
  private GUIStyle _sCardTitle;
  private GUIStyle _sCardDesc;
  private GUIStyle _sPill;
  private readonly GUIContent _gc = new GUIContent();
  private readonly List<ButtonHandler.Button> _catsCache = new List<ButtonHandler.Button>(16 /*0x10*/);
  private readonly List<ButtonHandler.Button> _contentCache = new List<ButtonHandler.Button>(64 /*0x40*/);
  private readonly List<(ButtonHandler.Button b, int score, string label)> _searchScratch = new List<(ButtonHandler.Button, int, string)>(64 /*0x40*/);
  private int _catsKey = int.MinValue;
  private int _contentPageKey = int.MinValue;
  private string _contentSearchKey;
  private int _contentModKey = int.MinValue;
  private int _contentMenuKey = int.MinValue;
  private int _scrollDragId = -1;
  private float _scrollDragOffset;
  private int _lastSidebarPage = -1;
  private readonly Dictionary<int, float> _clickAnim = new Dictionary<int, float>();
  private readonly Dictionary<int, float> _toggleAnim = new Dictionary<int, float>();
  private readonly Dictionary<int, float> _hoverAnim = new Dictionary<int, float>();
  private const float ClickAnimDur = 0.16f;
  private const float WatermarkH = 26f;
  private const float Pad = 12f;
  private const float BtnH = 30f;
  private const float BtnGap = 6f;
  private const float SideW = 124f;
  private const float ScrollBarW = 7f;
  private float _contentScroll;
  private float _categoryScroll;
  private float _animT = 1f;
  private bool _animatingIn;
  private bool _animatingOut;
  private bool _wasVisible = true;
  private static readonly Dictionary<string, string> _labelCache = new Dictionary<string, string>(64 /*0x40*/);

  public static int originalWidth { get; private set; }

  public static int originalHeight { get; private set; }

  private static Color FlowColor(float phaseOffset = 0.0f)
  {
    return Color.Lerp(Main.pinwheelColor1, Main.pinwheelColor2, Mathf.PingPong(Time.unscaledTime * 1.5f - phaseOffset, 1f));
  }

  private void Awake()
  {
    this._font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
    texture2D.filterMode = FilterMode.Point;
    this._pixel = texture2D;
    this._pixel.SetPixels32(new Color32[1]
    {
      new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
    });
    this._pixel.Apply(false, true);
    if (!NXOUI._resCaptured)
    {
      NXOUI.originalWidth = Screen.currentResolution.width * 2;
      NXOUI.originalHeight = Screen.currentResolution.height * 2;
      NXOUI._resCaptured = true;
    }
    foreach (ButtonHandler.Button button in ModButtons.buttons)
    {
      if (button.Enabled && button.isToggle && !NXOUI._modNotificationSet.Contains(button.buttonText))
        NXOUI.AddMod(button.buttonText);
    }
    this._win = new Rect((float) ((double) NXOUI.originalWidth / 2.0 - (double) NXOUI.GlobalWidth - 8.0), 8f, NXOUI.GlobalWidth, NXOUI.GlobalHeight);
  }

  private void Update() => NXOUI.HandleActiveMods();

  private void LateUpdate() => NXOUI.HandleButtonClicker();

  private void OnGUI()
  {
    int width = Screen.width;
    int height = Screen.height;
    if (width != this._lastW || height != this._lastH)
    {
      this._lastW = width;
      this._lastH = height;
      this._dirty = true;
    }
    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * (float) ((double) width / (double) NXOUI.originalWidth * 2.0));
    if (this._dirty)
    {
      this.Rebuild();
      this._dirty = false;
    }
    if (NXOUI.IsGuiVisible && !this._wasVisible)
    {
      this._animatingIn = true;
      this._animatingOut = false;
      this._animT = 0.0f;
    }
    if (!NXOUI.IsGuiVisible && this._wasVisible)
    {
      this._animatingOut = true;
      this._animatingIn = false;
      this._animT = 1f;
    }
    this._wasVisible = NXOUI.IsGuiVisible;
    if (this._animatingIn)
    {
      this._animT = Mathf.MoveTowards(this._animT, 1f, Time.unscaledDeltaTime * 5f);
      if ((double) this._animT >= 1.0)
      {
        this._animT = 1f;
        this._animatingIn = false;
      }
    }
    if (this._animatingOut)
    {
      this._animT = Mathf.MoveTowards(this._animT, 0.0f, Time.unscaledDeltaTime * 5f);
      if ((double) this._animT <= 0.0)
      {
        this._animT = 0.0f;
        this._animatingOut = false;
      }
    }
    if (!NXOUI.IsGuiVisible && !this._animatingOut)
      return;
    float num = 1f - Mathf.Pow(1f - this._animT, 3f);
    float x1 = Mathf.Lerp(-400f, 0.0f, num);
    float x2 = Mathf.Lerp(400f, 0.0f, num);
    GUI.color = new Color(1f, 1f, 1f, num);
    Matrix4x4 matrix = GUI.matrix;
    GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(x1, 0.0f, 0.0f));
    if (NXOUI.ModNotifications.Count > 0)
      this.DrawModList();
    float y = Mathf.Lerp(-40f, 0.0f, num);
    GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(0.0f, y, 0.0f));
    this.DrawWatermark();
    this.EnsureBgTexture();
    GUI.matrix = matrix * Matrix4x4.Translate(new Vector3(x2, 0.0f, 0.0f));
    if ((double) this._win.width < 1.0)
      this._win = new Rect((float) ((double) NXOUI.originalWidth / 2.0 - (double) NXOUI.GlobalWidth - 8.0), 8f, NXOUI.GlobalWidth, NXOUI.GlobalHeight);
    this._win = GUI.Window(1, this._win, new GUI.WindowFunction(this.DrawWin), "", GUIStyle.none);
    GUI.matrix = matrix;
    GUI.color = Color.white;
    ControllerEmulator.ShowControllerHUD();
  }

  private void DrawWatermark()
  {
    this._smoothFps = Mathf.Lerp(this._smoothFps, 1f / Time.unscaledDeltaTime, Time.unscaledDeltaTime * 4f);
    this._fpsUpdateTimer -= Time.unscaledDeltaTime;
    if ((double) this._fpsUpdateTimer <= 0.0)
    {
      this._displayedFps = Mathf.RoundToInt(this._smoothFps);
      this._fpsUpdateTimer = 0.5f;
    }
    if (this._displayedFps != this._lastWatermarkFps)
    {
      this._lastWatermarkFps = this._displayedFps;
      this._cachedWatermarkText = $"{"NXO"} | v{"6.3"} | FPS: {this._displayedFps} | (L-Alt) Menu (R-Alt) GUI";
      this._gc.text = this._cachedWatermarkText;
      this._cachedWatermarkW = this._sWatermark.CalcSize(this._gc).x + 20f;
    }
    float cachedWatermarkW = this._cachedWatermarkW;
    float x = (float) ((double) NXOUI.originalWidth / 4.0 - (double) cachedWatermarkW / 2.0);
    if ((UnityEngine.Object) this._watermarkTex == (UnityEngine.Object) null || this._watermarkTex.width != (int) cachedWatermarkW)
    {
      if ((UnityEngine.Object) this._watermarkTex != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this._watermarkTex);
      this._watermarkTex = NXOUI.MakeRect((int) cachedWatermarkW, 26, new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 140), new Color32(), 0, 8);
    }
    GUI.color = Color.white;
    GUI.DrawTexture(new Rect(x, 8f, cachedWatermarkW, 26f), (Texture) this._watermarkTex);
    this._sWatermark.normal.textColor = NXOUI.FlowColor();
    GUI.Label(new Rect(x + 10f, 8f, cachedWatermarkW, 26f), this._cachedWatermarkText, this._sWatermark);
    this._sWatermark.normal.textColor = Color.white;
  }

  private void DrawWin(int id)
  {
    this.DrawRound(new Rect(0.0f, 0.0f, this._win.width, this._win.height), 14, new Color32((byte) 10, (byte) 10, (byte) 10, (byte) 236), new Color32((byte) 92, (byte) 92, (byte) 92, (byte) 150));
    float x = 12f;
    float y = 8f;
    float num1 = this._win.width - 24f;
    this.DrawSearch((float) ((double) x + 124.0 + 12.0), y, (float) ((double) num1 - 124.0 - 12.0));
    this._sTitle.normal.textColor = NXOUI.FlowColor();
    GUI.Label(new Rect(x + 6f, y, 124f, 26f), "NXO v6.3", this._sTitle);
    this._sTitle.normal.textColor = Color.white;
    float num2 = y + 34f;
    float h = (float) ((double) this._win.height - (double) num2 - 12.0);
    this.DrawSidebar(x, num2, 124f, h);
    this.DrawContent((float) ((double) x + 124.0 + 12.0), num2, (float) ((double) num1 - 124.0 - 12.0), h);
    GUI.DragWindow(new Rect(0.0f, 0.0f, 136f, num2));
  }

  private void DrawSearch(float x, float y, float w)
  {
    this.DrawRound(new Rect(x, y, w, 26f), 7, new Color32((byte) 18, (byte) 18, (byte) 18, (byte) 245), new Color32((byte) 74, (byte) 74, (byte) 74, (byte) 170));
    float width = !string.IsNullOrEmpty(this._inputUpper) ? w - 44f : w - 24f;
    string upperInvariant = GUI.TextField(new Rect(x + 12f, y + 3f, width, 20f), this._inputUpper, this._sField).ToUpperInvariant();
    if (upperInvariant != this._inputUpper)
    {
      this._inputUpper = upperInvariant;
      Variables.currentCategoryPage = 0;
      Variables.taggerInstance?.offlineVRRig?.PlayHandTapLocal(66, false, 1f);
    }
    if (string.IsNullOrWhiteSpace(this._inputUpper))
    {
      GUI.Label(new Rect(x + 12f, y + 3f, w - 24f, 20f), this._pcPage ? "Room code / name..." : "Search mods...", this._sDim);
    }
    else
    {
      if (!this.MiniButton((float) ((double) x + (double) w - 26.0), y + 4f, 18f, 18f, "×", 387660))
        return;
      this._inputUpper = "";
      Variables.currentCategoryPage = 0;
    }
  }

  private void DrawSidebar(float x, float y, float w, float h)
  {
    this.DrawRound(new Rect(x, y, w, h), 11, new Color32((byte) 15, (byte) 15, (byte) 15, (byte) 238), new Color32((byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 135));
    List<ButtonHandler.Button> sidebarCats = this.GetSidebarCats();
    float x1 = x + 7f;
    float y1 = y + 8f;
    float num1 = w - 20f;
    float num2 = h - 16f;
    float num3 = 26f;
    float num4 = Mathf.Max(0.0f, (float) (sidebarCats.Count + 1) * num3 - num2);
    if (!this._pcPage && (Category) this._lastSidebarPage != Variables.currentPage)
    {
      this._lastSidebarPage = (int) Variables.currentPage;
      int num5 = -1;
      for (int index = 0; index < sidebarCats.Count; ++index)
      {
        if (this.IsSidebarActive(sidebarCats[index]))
        {
          num5 = index + 1;
          break;
        }
      }
      if (num5 >= 0)
      {
        float num6 = (float) num5 * num3;
        if ((double) num6 < (double) this._categoryScroll)
          this._categoryScroll = num6;
        else if ((double) num6 + (double) num3 > (double) this._categoryScroll + (double) num2)
          this._categoryScroll = num6 + num3 - num2;
      }
    }
    if (new Rect(x, y, w, h).Contains(Event.current.mousePosition) && Event.current.type == (EventType) 6)
    {
      this._categoryScroll = Mathf.Clamp(this._categoryScroll + Event.current.delta.y * 14f, 0.0f, num4);
      Event.current.Use();
    }
    this._categoryScroll = Mathf.Clamp(this._categoryScroll, 0.0f, num4);
    GUI.BeginGroup(new Rect(x1, y1, num1, num2));
    float y2 = -this._categoryScroll;
    this.DrawPcSidebarButton(0.0f, ref y2, num1);
    foreach (ButtonHandler.Button b in sidebarCats)
      this.DrawSidebarButton(0.0f, ref y2, num1, b);
    GUI.EndGroup();
    if ((double) num4 <= 0.0)
      return;
    this._categoryScroll = this.DrawScrollBar(1, (float) ((double) x + (double) w - 10.0), y1, 5f, num2, this._categoryScroll, num4);
  }

  private void DrawSidebarButton(float x, ref float y, float w, ButtonHandler.Button b)
  {
    int num1 = b.buttonText.GetHashCode() * 17 + 1;
    Rect r = new Rect(x, y, w, 22f);
    bool flag = !this._pcPage && this.IsSidebarActive(b);
    bool hovered = r.Contains(Event.current.mousePosition);
    float num2 = this.ClickScale(num1);
    Matrix4x4 matrix = GUI.matrix;
    if ((double) num2 < 1.0)
      GUIUtility.ScaleAroundPivot(new Vector2(num2, num2), r.center);
    this.DrawRound(r, 7, flag ? new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 245) : new Color32((byte) 23, (byte) 23, (byte) 23, (byte) 220), flag ? new Color32((byte) 120, (byte) 120, (byte) 120, (byte) 80 /*0x50*/) : new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0), flag ? 1 : 0);
    this.DrawHoverGlow(r, 7, this.HoverT(num1, hovered) * 0.07f);
    if (flag)
    {
      GUI.color = NXOUI.FlowColor();
      GUI.DrawTexture(new Rect(x + 3f, y + 5f, 2f, 12f), (Texture) this.RoundTex(2, 12, 1, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), new Color32(), 0));
      GUI.color = Color.white;
    }
    this._sSmall.normal.textColor = flag ? Color.white : (Color) new Color32((byte) 168, (byte) 168, (byte) 168, byte.MaxValue);
    GUI.Label(new Rect(x + 11f, y + 1f, w - 21f, 20f), NXOUI.CleanLabel(b.buttonText), this._sSmall);
    if ((double) num2 < 1.0)
      GUI.matrix = matrix;
    if (GUI.Button(r, GUIContent.none, GUIStyle.none))
    {
      this._clickAnim[num1] = Time.unscaledTime;
      ButtonHandler.PlayClickSound();
      this._pcPage = false;
      this._inputUpper = "";
      GUIUtility.keyboardControl = 0;
      ButtonHandler.Toggle(b);
      this._contentScroll = 0.0f;
    }
    y += 26f;
  }

  private void DrawPcSidebarButton(float x, ref float y, float w)
  {
    int num1 = 1517;
    Rect r = new Rect(x, y, w, 22f);
    bool pcPage = this._pcPage;
    bool hovered = r.Contains(Event.current.mousePosition);
    float num2 = this.ClickScale(num1);
    Matrix4x4 matrix = GUI.matrix;
    if ((double) num2 < 1.0)
      GUIUtility.ScaleAroundPivot(new Vector2(num2, num2), r.center);
    this.DrawRound(r, 7, pcPage ? new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 245) : new Color32((byte) 23, (byte) 23, (byte) 23, (byte) 220), pcPage ? new Color32((byte) 120, (byte) 120, (byte) 120, (byte) 80 /*0x50*/) : new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0), pcPage ? 1 : 0);
    this.DrawHoverGlow(r, 7, this.HoverT(num1, hovered) * 0.07f);
    if (pcPage)
    {
      GUI.color = NXOUI.FlowColor();
      GUI.DrawTexture(new Rect(x + 3f, y + 5f, 2f, 12f), (Texture) this.RoundTex(2, 12, 1, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), new Color32(), 0));
      GUI.color = Color.white;
    }
    this._sSmall.normal.textColor = pcPage ? Color.white : (Color) new Color32((byte) 168, (byte) 168, (byte) 168, byte.MaxValue);
    GUI.Label(new Rect(x + 11f, y + 1f, w - 21f, 20f), "PC", this._sSmall);
    if ((double) num2 < 1.0)
      GUI.matrix = matrix;
    if (GUI.Button(r, GUIContent.none, GUIStyle.none))
    {
      this._clickAnim[num1] = Time.unscaledTime;
      ButtonHandler.PlayClickSound();
      this._pcPage = true;
      this._inputUpper = "";
      GUIUtility.keyboardControl = 0;
      this._contentScroll = 0.0f;
    }
    y += 26f;
  }

  private bool IsSidebarActive(ButtonHandler.Button b)
  {
    string b1 = NXOUI.CleanLabel(b.buttonText).Replace(" ", "_");
    if (b.buttonText == "Home")
      return Variables.currentPage == Category.Home;
    if (b.buttonText == "VRRig")
      return Variables.currentPage == Category.Player;
    return b.buttonText == "Favorites" ? Variables.currentPage == Category.Favorited : string.Equals(Variables.currentPage.ToString(), b1, StringComparison.OrdinalIgnoreCase);
  }

  private void DrawContent(float x, float y, float w, float h)
  {
    if (this._pcPage)
    {
      this.DrawPcContent(x, y, w, h);
    }
    else
    {
      this.DrawRound(new Rect(x, y, w, h), 11, new Color32((byte) 15, (byte) 15, (byte) 15, (byte) 238), new Color32((byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 135));
      bool searching = !string.IsNullOrWhiteSpace(this._inputUpper);
      List<ButtonHandler.Button> content = this.GetContent(searching);
      int count = content.Count;
      float y1 = y + 10f;
      string str = searching ? "Search: " + this._inputUpper : NXOUI.CleanLabel(Variables.currentPage.ToString().Replace("_", " "));
      this._sCardTitle.normal.textColor = Color.white;
      GUI.Label(new Rect(x + 12f, y1, w - 24f, 19f), str, this._sCardTitle);
      this._sDim.alignment = (TextAnchor) 5;
      GUI.Label(new Rect(x + 12f, y1, w - 24f, 19f), $"{count} items", this._sDim);
      this._sDim.alignment = (TextAnchor) 3;
      this.Sep(x + 12f, y1 + 25f, w - 24f);
      float x1 = x + 12f;
      float y2 = y1 + 36f;
      float num1 = w - 31f;
      float num2 = h - 48f;
      float num3 = Mathf.Max(0.0f, (count == 0 ? 40f : (float) ((double) count * 36.0 - 6.0)) - num2);
      if (new Rect(x, y, w, h).Contains(Event.current.mousePosition) && Event.current.type == (EventType) 6)
      {
        this._contentScroll = Mathf.Clamp(this._contentScroll + Event.current.delta.y * 22f, 0.0f, num3);
        Event.current.Use();
      }
      this._contentScroll = Mathf.Clamp(this._contentScroll, 0.0f, num3);
      GUI.BeginGroup(new Rect(x1, y2, num1, num2));
      float y3 = -this._contentScroll;
      if (count == 0)
      {
        GUI.Label(new Rect(0.0f, 8f, num1, 24f), searching ? "No mods matched your search." : "Nothing in this category.", this._sDim);
      }
      else
      {
        this._btnIndex = 0;
        for (int index = 0; index < count && (double) y3 <= (double) num2; ++index)
        {
          if ((double) y3 + 30.0 >= 0.0)
          {
            this.DrawModCard(0.0f, ref y3, num1, content[index]);
          }
          else
          {
            y3 += 36f;
            ++this._btnIndex;
          }
        }
      }
      GUI.EndGroup();
      if ((double) num3 <= 0.0)
        return;
      this._contentScroll = this.DrawScrollBar(2, (float) ((double) x + (double) w - 12.0), y2, 5f, num2, this._contentScroll, num3);
    }
  }

  private void DrawPcContent(float x, float y, float w, float h)
  {
    this.DrawRound(new Rect(x, y, w, h), 11, new Color32((byte) 15, (byte) 15, (byte) 15, (byte) 238), new Color32((byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 64 /*0x40*/, (byte) 135));
    float y1 = y + 10f;
    this._sCardTitle.normal.textColor = Color.white;
    GUI.Label(new Rect(x + 12f, y1, w - 24f, 19f), "PC", this._sCardTitle);
    this.Sep(x + 12f, y1 + 25f, w - 24f);
    float x1 = x + 12f;
    float y2 = y1 + 36f;
    float num1 = w - 31f;
    float num2 = h - 48f;
    float num3 = Mathf.Max(0.0f, 354f - num2);
    if (new Rect(x, y, w, h).Contains(Event.current.mousePosition) && Event.current.type == (EventType) 6)
    {
      this._contentScroll = Mathf.Clamp(this._contentScroll + Event.current.delta.y * 22f, 0.0f, num3);
      Event.current.Use();
    }
    this._contentScroll = Mathf.Clamp(this._contentScroll, 0.0f, num3);
    GUI.BeginGroup(new Rect(x1, y2, num1, num2));
    float y3 = -this._contentScroll;
    this.PcAction(ref y3, num1, "Join Specific Room", (Action) (() => PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(this._inputUpper, (JoinType) 0)));
    this.PcAction(ref y3, num1, "Join Random Room", (Action) (() => Room.JoinRandomPublic()));
    this.PcAction(ref y3, num1, "Change Name", (Action) (() => RigManager.ChangeName(this._inputUpper)));
    this.PcAction(ref y3, num1, "Disconnect", (Action) (() => NetworkSystem.Instance.ReturnToSinglePlayer()));
    this.PcAction(ref y3, num1, "Reconnect", (Action) (() => Room.Reconnect()));
    this.PcToggle(ref y3, num1, "Freecam", NXOUI.freecamEnabled, (Action) (() =>
    {
      NXOUI.freecamEnabled = !NXOUI.freecamEnabled;
      if (NXOUI.freecamEnabled)
        return;
      NXOUI.RemoveMod("Freecam");
    }));
    this.PcToggle(ref y3, num1, "Noclip", Movement.noclipEnabled, (Action) (() =>
    {
      Movement.noclipEnabled = !Movement.noclipEnabled;
      foreach (Collider collider in UnityEngine.Object.FindObjectsOfType<MeshCollider>())
        collider.enabled = !Movement.noclipEnabled;
      if (Movement.noclipEnabled)
        NXOUI.AddMod("Noclip");
      else
        NXOUI.RemoveMod("Noclip");
    }));
    this.PcToggle(ref y3, num1, "First Person", NXOUI.firstPersonEnabled, (Action) (() =>
    {
      NXOUI.firstPersonEnabled = !NXOUI.firstPersonEnabled;
      if (NXOUI.firstPersonEnabled)
      {
        NXOUI.AddMod("First Person");
        Visuals.FPC(true);
      }
      else
      {
        NXOUI.RemoveMod("First Person");
        Visuals.FPC(false);
      }
    }));
    this.PcToggle(ref y3, num1, "Button Clicker", NXOUI.buttonClickerEnabled, (Action) (() =>
    {
      NXOUI.buttonClickerEnabled = !NXOUI.buttonClickerEnabled;
      if (NXOUI.buttonClickerEnabled)
        return;
      NXOUI.RemoveMod("Button Clicker");
    }));
    this.PcToggle(ref y3, num1, "Controller Emulator", ControllerEmulator.EmulatorEnabled, (Action) (() => ControllerEmulator.EmulatorEnabled = !ControllerEmulator.EmulatorEnabled));
    GUI.EndGroup();
    if ((double) num3 <= 0.0)
      return;
    this._contentScroll = this.DrawScrollBar(2, (float) ((double) x + (double) w - 12.0), y2, 5f, num2, this._contentScroll, num3);
  }

  private void PcAction(ref float y, float w, string label, Action click)
  {
    if (this.RowButton(new Rect(0.0f, y, w, 30f), label, label.GetHashCode() * 31 /*0x1F*/ + 7))
    {
      ButtonHandler.PlayClickSound();
      click();
    }
    y += 36f;
  }

  private void PcToggle(ref float y, float w, string label, bool on, Action click)
  {
    if (this.RowToggle(new Rect(0.0f, y, w, 30f), label, on, label.GetHashCode() * 31 /*0x1F*/ + 9))
    {
      ButtonHandler.PlayClickSound();
      click();
    }
    y += 36f;
  }

  private List<ButtonHandler.Button> GetSidebarCats()
  {
    if (this._catsKey != NXOUI.MenuContentVersion)
    {
      this._catsKey = NXOUI.MenuContentVersion;
      this._catsCache.Clear();
      foreach (ButtonHandler.Button button in ModButtons.buttons)
      {
        if (button != null && button.Page == Category.Home && button.isCategory)
        {
          string str = NXOUI.CleanLabel(button.buttonText);
          if (!(str == "Home") && !(str == "Main"))
            this._catsCache.Add(button);
        }
      }
    }
    return this._catsCache;
  }

  private List<ButtonHandler.Button> GetContent(bool searching)
  {
    if ((Category) this._contentPageKey != Variables.currentPage || this._contentSearchKey != this._inputUpper || this._contentModKey != NXOUI._modListVersion || this._contentMenuKey != NXOUI.MenuContentVersion)
    {
      this._contentPageKey = (int) Variables.currentPage;
      this._contentSearchKey = this._inputUpper;
      this._contentModKey = NXOUI._modListVersion;
      this._contentMenuKey = NXOUI.MenuContentVersion;
      this._contentCache.Clear();
      if (searching)
      {
        this.SearchButtonsInto(this._inputUpper, this._contentCache);
      }
      else
      {
        List<ButtonHandler.Button> buttonInfoByPage = ButtonHandler.GetButtonInfoByPage(Variables.currentPage);
        for (int index = 0; index < buttonInfoByPage.Count; ++index)
        {
          ButtonHandler.Button button = buttonInfoByPage[index];
          if (button != null && Settings.ShouldShowButton(button.buttonText))
            this._contentCache.Add(button);
        }
      }
    }
    return this._contentCache;
  }

  private void SearchButtonsInto(string q, List<ButtonHandler.Button> dest)
  {
    q = q.Trim();
    this._searchScratch.Clear();
    foreach (ButtonHandler.Button button in ModButtons.buttons)
    {
      if (button != null && button.Page != Category.Home && Settings.ShouldShowButton(button.buttonText))
      {
        string text = NXOUI.CleanLabel(button.buttonText);
        int num = SearchAndKeyboard.FuzzyScore(text, q);
        if (num > 0 || text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
          this._searchScratch.Add((button, num, text));
      }
    }
    this._searchScratch.Sort((Comparison<(ButtonHandler.Button btn, int score, string label)>) ((x, y) => x.score == y.score ? string.Compare(x.label, y.label) : y.score.CompareTo(x.score)));
    for (int index = 0; index < this._searchScratch.Count; ++index)
      dest.Add(this._searchScratch[index].b);
  }

  private bool RowButton(Rect r, string label, int id)
  {
    bool hovered = r.Contains(Event.current.mousePosition);
    float num = this.ClickScale(id);
    Matrix4x4 matrix = GUI.matrix;
    if ((double) num < 1.0)
      GUIUtility.ScaleAroundPivot(new Vector2(num, num), r.center);
    this.DrawRound(r, 8, new Color32((byte) 24, (byte) 24, (byte) 24, (byte) 242), new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 130));
    this.DrawHoverGlow(r, 8, this.HoverT(id, hovered) * 0.06f);
    this._sCardTitle.normal.textColor = Color.white;
    GUI.Label(new Rect(r.x + 12f, r.y, r.width - 24f, r.height), label, this._sCardTitle);
    if ((double) num < 1.0)
      GUI.matrix = matrix;
    bool flag = GUI.Button(r, GUIContent.none, GUIStyle.none);
    if (flag)
      this._clickAnim[id] = Time.unscaledTime;
    return flag;
  }

  private bool RowToggle(Rect r, string label, bool on, int id)
  {
    bool hovered = r.Contains(Event.current.mousePosition);
    float target = on ? 1f : 0.0f;
    float num;
    if (!this._toggleAnim.TryGetValue(id, out num))
      num = target;
    if (Event.current.type == (EventType) 7)
    {
      num = Mathf.MoveTowards(num, target, Time.unscaledDeltaTime * 8f);
      this._toggleAnim[id] = num;
    }
    float t = Mathf.SmoothStep(0.0f, 1f, num);
    bool flag = (double) t >= 0.5;
    this.DrawRound(r, 8, flag ? new Color32((byte) 40, (byte) 40, (byte) 40, (byte) 248) : new Color32((byte) 20, (byte) 20, (byte) 20, (byte) 242), flag ? new Color32((byte) 120, (byte) 120, (byte) 120, (byte) 180) : new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 120));
    this.DrawHoverGlow(r, 8, this.HoverT(id, hovered) * 0.06f);
    this._sCardTitle.normal.textColor = Color.white;
    GUI.Label(new Rect(r.x + 12f, r.y, r.width - 80f, r.height), label, this._sCardTitle);
    float x = (float) ((double) r.x + (double) r.width - 58.0);
    float y = r.y + (float) (((double) r.height - 20.0) * 0.5);
    this.DrawRound(new Rect(x, y, 46f, 20f), 10, flag ? new Color32((byte) 70, (byte) 70, (byte) 70, (byte) 250) : new Color32((byte) 35, (byte) 35, (byte) 35, (byte) 245), flag ? new Color32((byte) 180, (byte) 180, (byte) 180, (byte) 220) : new Color32((byte) 92, (byte) 92, (byte) 92, (byte) 190));
    this.DrawRound(new Rect(Mathf.Lerp(x + 3f, x + 29f, t), y + 3f, 14f, 14f), 7, new Color32((byte) 235, (byte) 235, (byte) 235, byte.MaxValue), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 200));
    return GUI.Button(r, GUIContent.none, GUIStyle.none);
  }

  private void DrawModCard(float x, ref float y, float w, ButtonHandler.Button b)
  {
    Rect r = new Rect(x, y, w, 30f);
    int id = RuntimeHelpers.GetHashCode((object) b) * 17;
    string label = NXOUI.CleanLabel(b.buttonText);
    if (this.FavoriteModsSafeContains(b))
      label = "★ " + label;
    if (b.incremental)
    {
      bool flag = r.Contains(Event.current.mousePosition);
      this.DrawRound(r, 8, flag ? new Color32((byte) 30, (byte) 30, (byte) 30, (byte) 246) : new Color32((byte) 20, (byte) 20, (byte) 20, (byte) 242), new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 120));
      this._sCardTitle.normal.textColor = Color.white;
      GUI.Label(new Rect(x + 12f, y, w - 88f, 30f), label, this._sCardTitle);
      float y1 = y + 4f;
      if (this.MiniButton((float) ((double) x + (double) w - 76.0), y1, 28f, 22f, "<", id + 3))
      {
        ButtonHandler.PlayClickSound();
        ButtonHandler.Toggle(new ButtonHandler.Button(b.buttonText + "_DOWN", b.Page, false, false, (Action) null));
      }
      if (this.MiniButton((float) ((double) x + (double) w - 40.0), y1, 28f, 22f, ">", id + 4))
      {
        ButtonHandler.PlayClickSound();
        ButtonHandler.Toggle(new ButtonHandler.Button(b.buttonText + "_UP", b.Page, false, false, (Action) null));
      }
    }
    else if (b.isToggle)
    {
      if (this.RowToggle(r, label, b.Enabled, id))
      {
        ButtonHandler.PlayClickSound();
        ButtonHandler.Toggle(b);
      }
    }
    else if (this.RowButton(r, label, id))
    {
      ButtonHandler.PlayClickSound();
      ButtonHandler.Toggle(b);
    }
    y += 36f;
    ++this._btnIndex;
  }

  private bool FavoriteModsSafeContains(ButtonHandler.Button b)
  {
    try
    {
      return ButtonHandler.FavoriteMods != null && ButtonHandler.FavoriteMods.Contains(b);
    }
    catch
    {
      return false;
    }
  }

  private string GetDescription(ButtonHandler.Button b)
  {
    if (!string.IsNullOrWhiteSpace(b.tooltip))
      return NXOUI.CleanLabel(b.tooltip);
    if (b.incremental)
      return "Use the arrows to change this setting.";
    if (b.isCategory)
      return "Open this category.";
    return b.isToggle ? (b.Enabled ? "Enabled — click the switch to turn it off." : "Disabled — click the switch to turn it on.") : "Run this action.";
  }

  private float ClickScale(int id)
  {
    float num1;
    if (!this._clickAnim.TryGetValue(id, out num1))
      return 1f;
    float num2 = (float) (((double) Time.unscaledTime - (double) num1) / 0.15999999642372131);
    if ((double) num2 < 1.0)
      return (float) (1.0 - (double) Mathf.Sin(num2 * 3.14159274f) * 0.11999999731779099);
    this._clickAnim.Remove(id);
    return 1f;
  }

  private float HoverT(int id, bool hovered)
  {
    float current;
    this._hoverAnim.TryGetValue(id, out current);
    if (Event.current.type == (EventType) 7)
    {
      current = Mathf.MoveTowards(current, hovered ? 1f : 0.0f, Time.unscaledDeltaTime * 10f);
      if ((double) current <= 0.0 && !hovered)
        this._hoverAnim.Remove(id);
      else
        this._hoverAnim[id] = current;
    }
    return current;
  }

  private void DrawHoverGlow(Rect r, int radius, float alpha)
  {
    if ((double) alpha <= 0.0040000001899898052)
      return;
    Color color = GUI.color;
    GUI.color = new Color(1f, 1f, 1f, alpha);
    GUI.DrawTexture(r, (Texture) this.RoundTex(Mathf.CeilToInt(r.width), Mathf.CeilToInt(r.height), radius, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), new Color32(), 0));
    GUI.color = color;
  }

  private void DrawToggle(float x, float y, bool on, int id, Action click)
  {
    Rect r = new Rect(x, y, 52f, 20f);
    bool flag1 = r.Contains(Event.current.mousePosition);
    float target = on ? 1f : 0.0f;
    float num;
    if (!this._toggleAnim.TryGetValue(id, out num))
      num = target;
    if (Event.current.type == (EventType) 7)
    {
      num = Mathf.MoveTowards(num, target, Time.unscaledDeltaTime * 8f);
      this._toggleAnim[id] = num;
    }
    float t = Mathf.SmoothStep(0.0f, 1f, num);
    bool flag2 = (double) t >= 0.5;
    this.DrawRound(r, 10, flag2 ? new Color32((byte) 33, (byte) 82, (byte) 92, (byte) 250) : (flag1 ? new Color32((byte) 40, (byte) 40, (byte) 40, (byte) 248) : new Color32((byte) 25, (byte) 25, (byte) 25, (byte) 245)), flag2 ? new Color32((byte) 100, (byte) 226, (byte) 240 /*0xF0*/, (byte) 220) : new Color32((byte) 92, (byte) 92, (byte) 92, (byte) 190));
    this.DrawRound(new Rect(Mathf.Lerp(x + 3f, x + 35f, t), y + 3f, 14f, 14f), 7, new Color32((byte) 235, (byte) 235, (byte) 235, byte.MaxValue), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 200));
    if (!GUI.Button(r, GUIContent.none, GUIStyle.none))
      return;
    click();
  }

  private bool MiniButton(float x, float y, float w, float h, string label, int id)
  {
    Rect r = new Rect(x, y, w, h);
    bool flag1 = r.Contains(Event.current.mousePosition);
    float num = this.ClickScale(id);
    Matrix4x4 matrix = GUI.matrix;
    if ((double) num < 1.0)
      GUIUtility.ScaleAroundPivot(new Vector2(num, num), r.center);
    this.DrawRound(r, 5, flag1 ? new Color32((byte) 48 /*0x30*/, (byte) 48 /*0x30*/, (byte) 48 /*0x30*/, (byte) 245) : new Color32((byte) 30, (byte) 30, (byte) 30, (byte) 235), flag1 ? new Color32((byte) 125, (byte) 125, (byte) 125, (byte) 190) : new Color32((byte) 76, (byte) 76, (byte) 76, (byte) 150));
    GUI.Label(r, label, this._sPill);
    if ((double) num < 1.0)
      GUI.matrix = matrix;
    bool flag2 = GUI.Button(r, GUIContent.none, GUIStyle.none);
    if (flag2)
      this._clickAnim[id] = Time.unscaledTime;
    return flag2;
  }

  private float DrawScrollBar(
    int id,
    float x,
    float y,
    float w,
    float h,
    float scroll,
    float maxScroll)
  {
    this.DrawRound(new Rect(x, y, w, h), 3, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 22), new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0), 0);
    float height = Mathf.Clamp(h * (h / (h + maxScroll)), 28f, h);
    float num = h - height;
    float y1 = y + num * (scroll / Mathf.Max(1f, maxScroll));
    Event current = Event.current;
    Rect rect = new Rect(x - 3f, y1, w + 6f, height);
    if (current.type == null && rect.Contains(current.mousePosition))
    {
      this._scrollDragId = id;
      this._scrollDragOffset = current.mousePosition.y - y1;
      current.Use();
    }
    else if (this._scrollDragId == id)
    {
      if (current.type == (EventType) 3 && (double) num > 0.0)
      {
        scroll = Mathf.Clamp01((current.mousePosition.y - this._scrollDragOffset - y) / num) * maxScroll;
        y1 = y + num * (scroll / Mathf.Max(1f, maxScroll));
        current.Use();
      }
      else if (current.type == (EventType) 1)
      {
        this._scrollDragId = -1;
        current.Use();
      }
    }
    bool flag = this._scrollDragId == id;
    this.DrawRound(new Rect(x, y1, w, height), 3, flag ? new Color32((byte) 240 /*0xF0*/, (byte) 240 /*0xF0*/, (byte) 240 /*0xF0*/, (byte) 220) : new Color32((byte) 215, (byte) 215, (byte) 215, (byte) 175), new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0), 0);
    return scroll;
  }

  private static string CleanLabel(string text)
  {
    if (string.IsNullOrEmpty(text))
      return string.Empty;
    string str1;
    if (NXOUI._labelCache.TryGetValue(text, out str1))
      return str1;
    string str2 = Regex.Replace(text, "<.*?>", string.Empty);
    NXOUI._labelCache[text] = str2;
    return str2;
  }

  private void Sep(float x, float y, float w)
  {
    GUI.color = (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 20);
    GUI.DrawTexture(new Rect(x, y, w, 1f), (Texture) this._pixel);
    GUI.color = Color.white;
  }

  private long RoundKey(int w, int h, int r, int bw, Color32 fill, Color32 border)
  {
    long key = 1469598103934665603;
    Mix(w);
    Mix(h);
    Mix(r);
    Mix(bw);
    Mix((int) fill.r << 24 | (int) fill.g << 16 /*0x10*/ | (int) fill.b << 8 | (int) fill.a);
    Mix((int) border.r << 24 | (int) border.g << 16 /*0x10*/ | (int) border.b << 8 | (int) border.a);
    return key;

    void Mix(int v)
    {
      key ^= (long) v;
      key *= 1099511628211L /*0x0100000001B3*/;
    }
  }

  private Texture2D RoundTex(int w, int h, int r, Color32 fill, Color32 border, int bw)
  {
    w = Mathf.Max(1, w);
    h = Mathf.Max(1, h);
    r = Mathf.Clamp(r, 0, Mathf.Min(w, h) / 2);
    long key1 = this.RoundKey(w, h, r, bw, fill, border);
    Texture2D texture2D1;
    if (this._roundCache.TryGetValue(key1, out texture2D1) && (UnityEngine.Object) texture2D1 != (UnityEngine.Object) null)
    {
      this._roundUsed[key1] = Time.unscaledTime;
      return texture2D1;
    }
    if (this._roundCache.Count >= 256 /*0x0100*/)
    {
      long key2 = 0;
      float maxValue = float.MaxValue;
      foreach (KeyValuePair<long, float> keyValuePair in this._roundUsed)
      {
        if ((double) keyValuePair.Value < (double) maxValue)
        {
          maxValue = keyValuePair.Value;
          key2 = keyValuePair.Key;
        }
      }
      Texture2D texture2D2;
      if (this._roundCache.TryGetValue(key2, out texture2D2) && (UnityEngine.Object) texture2D2 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) texture2D2);
      this._roundCache.Remove(key2);
      this._roundUsed.Remove(key2);
    }
    Texture2D texture2D3 = NXOUI.MakeRect(w, h, fill, border, bw, r);
    this._roundCache[key1] = texture2D3;
    this._roundUsed[key1] = Time.unscaledTime;
    return texture2D3;
  }

  private void DrawRound(Rect r, int radius, Color32 fill, Color32 border, int borderWidth = 1)
  {
    Color color = GUI.color;
    GUI.color = Color.white;
    GUI.DrawTexture(r, (Texture) this.RoundTex(Mathf.CeilToInt(r.width), Mathf.CeilToInt(r.height), radius, fill, border, borderWidth));
    GUI.color = color;
  }

  private Texture2D GetModBarTex(int barW)
  {
    Texture2D modBarTex1;
    if (this._modBarTexCache.TryGetValue(barW, out modBarTex1) && (UnityEngine.Object) modBarTex1 != (UnityEngine.Object) null)
    {
      this._modBarTexUsed[barW] = Time.unscaledTime;
      return modBarTex1;
    }
    if (this._modBarTexCache.Count >= 48 /*0x30*/)
    {
      int key = 0;
      float maxValue = float.MaxValue;
      foreach (KeyValuePair<int, float> keyValuePair in this._modBarTexUsed)
      {
        if ((double) keyValuePair.Value < (double) maxValue)
        {
          maxValue = keyValuePair.Value;
          key = keyValuePair.Key;
        }
      }
      Texture2D texture2D;
      if (this._modBarTexCache.TryGetValue(key, out texture2D) && (UnityEngine.Object) texture2D != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) texture2D);
      this._modBarTexCache.Remove(key);
      this._modBarTexUsed.Remove(key);
    }
    Texture2D modBarTex2 = NXOUI.MakeRect(barW, 20, new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 180), new Color32(), 0, 6);
    this._modBarTexCache[barW] = modBarTex2;
    this._modBarTexUsed[barW] = Time.unscaledTime;
    return modBarTex2;
  }

  private void DrawModList()
  {
    float unscaledTime = Time.unscaledTime;
    float num1 = 8f;
    NXOUI._removeBuffer.Clear();
    foreach (KeyValuePair<string, float> modRemoveTime in NXOUI._modRemoveTimes)
    {
      if ((double) unscaledTime - (double) modRemoveTime.Value >= 0.40000000596046448)
        NXOUI._removeBuffer.Add(modRemoveTime.Key);
    }
    for (int index = 0; index < NXOUI._removeBuffer.Count; ++index)
    {
      NXOUI.ModNotifications.Remove(NXOUI._removeBuffer[index]);
      NXOUI._modNotificationSet.Remove(NXOUI._removeBuffer[index]);
      NXOUI._modEntryTimes.Remove(NXOUI._removeBuffer[index]);
      NXOUI._modRemoveTimes.Remove(NXOUI._removeBuffer[index]);
      ++NXOUI._modListVersion;
    }
    int count = NXOUI.ModNotifications.Count;
    if (count == 0)
      return;
    NXOUI._sortedMods.Clear();
    for (int index = 0; index < count; ++index)
    {
      string modNotification = NXOUI.ModNotifications[index];
      this._gc.text = modNotification;
      NXOUI._sortedMods.Add((modNotification, this._sMod.CalcSize(this._gc).x));
    }
    NXOUI._sortedMods.Sort((Comparison<(string name, float w)>) ((a, b) => b.w.CompareTo(a.w)));
    for (int index = 0; index < NXOUI._sortedMods.Count; ++index)
    {
      string mod = NXOUI._sortedMods[index].mod;
      float w = NXOUI._sortedMods[index].w;
      float num2;
      float num3 = NXOUI._modEntryTimes.TryGetValue(mod, out num2) ? unscaledTime - num2 : 999f;
      float num4;
      bool flag = NXOUI._modRemoveTimes.TryGetValue(mod, out num4);
      float num5 = flag ? unscaledTime - num4 : 999f;
      float num6 = 0.0f;
      float x1 = 1f;
      float a = 1f;
      if (flag && (double) num5 < 0.40000000596046448)
      {
        float num7 = num5 / 0.4f;
        num6 = num7 * 30f;
        a = 1f - num7;
      }
      else if ((double) num3 < 0.40000000596046448)
      {
        float num8 = num3 / 0.4f;
        float num9 = Mathf.Sin((float) ((double) num8 * 3.1415927410125732 * 2.5)) * (1f - num8);
        num6 = num9 * 8f;
        x1 = (float) (1.0 + (double) num9 * 0.15000000596046448);
      }
      float x2 = 6f + num6;
      float y = num1 + (float) index * 24f;
      float num10 = w + 18f;
      Matrix4x4 matrix = GUI.matrix;
      GUIUtility.ScaleAroundPivot(new Vector2(x1, 1f), new Vector2(x2, y + 10f));
      Texture2D modBarTex = this.GetModBarTex((int) num10);
      Color color = NXOUI.FlowColor((float) index * 0.15f);
      GUI.color = new Color(1f, 1f, 1f, a);
      GUI.DrawTexture(new Rect(x2, y, num10, 20f), (Texture) modBarTex);
      this._sMod.normal.textColor = new Color(color.r, color.g, color.b, a);
      GUI.Label(new Rect(x2 + 10f, y, w + 10f, 20f), mod, this._sMod);
      GUI.matrix = matrix;
      GUI.color = Color.white;
    }
    this._sMod.normal.textColor = Color.white;
  }

  public static void AddMod(string t)
  {
    if (NXOUI._modNotificationSet.Contains(t))
      return;
    NXOUI.ModNotifications.Add(t);
    NXOUI._modNotificationSet.Add(t);
    NXOUI._modEntryTimes[t] = Time.unscaledTime;
    if (NXOUI.ModNotifications.Count > 100)
      NXOUI.ModNotifications.RemoveAt(0);
    ++NXOUI._modListVersion;
  }

  public static void RemoveMod(string t)
  {
    if (!NXOUI._modNotificationSet.Contains(t))
      return;
    NXOUI._modRemoveTimes[t] = Time.unscaledTime;
    ++NXOUI._modListVersion;
  }

  public static void HandleActiveMods()
  {
    if (!NXOUI.freecamEnabled)
      return;
    NXOUI.AddMod("Freecam");
    Transform transform = ((Component) Variables.taggerInstance.headCollider).transform;
    float num = UnityInput.Current.GetKey(KeyCode.LeftShift) ? (float) ((double) Settings.FlySpeed / 2.0 + 3.0) : Settings.FlySpeed / 2f;
    ((Component) Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = new Vector3(0.0f, 0.065f, 0.0f);
    Vector3 zero = Vector3.zero;
    if (UnityInput.Current.GetKey(KeyCode.W))
      zero += transform.forward;
    if (UnityInput.Current.GetKey(KeyCode.S))
      zero -= transform.forward;
    if (UnityInput.Current.GetKey(KeyCode.A))
      zero -= transform.right;
    if (UnityInput.Current.GetKey(KeyCode.D))
      zero += transform.right;
    if (UnityInput.Current.GetKey(KeyCode.Space))
      zero += transform.up;
    if (UnityInput.Current.GetKey(KeyCode.LeftControl))
      zero -= transform.up;
    transform.position += zero.normalized * num * Time.deltaTime;
    if (UnityInput.Current.GetMouseButton(1))
    {
      Vector3 vector3 = UnityInput.Current.mousePosition - Movement.OldMousePosition;
      Variables.taggerInstance.mainCamera.transform.localEulerAngles += new Vector3((float) (-(double) vector3.y * 0.10000000149011612), vector3.x * 0.1f, 0.0f);
    }
    Movement.OldMousePosition = UnityInput.Current.mousePosition;
  }

  public static void HandleButtonClicker()
  {
    if (NXOUI.buttonClickerEnabled)
    {
      NXOUI.AddMod("Button Clicker");
      if (Mouse.current.leftButton.isPressed)
      {
        RaycastHit raycastHit;
        Physics.Raycast(((UnityEngine.Object) Main._pcCamera != (UnityEngine.Object) null ? Main._pcCamera : (Main._pcCamera = Variables.thirdPersonCamera.GetComponent<Camera>())).ScreenPointToRay((Vector3) ((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue()), out raycastHit, 512f, Variables.NoInvisLayers());
        NXOUI._oldLocalPos.GetValueOrDefault();
        if (!NXOUI._oldLocalPos.HasValue)
          NXOUI._oldLocalPos = new Vector3?(Variables.taggerInstance.rightHandTriggerCollider.transform.localPosition);
        Variables.taggerInstance.rightHandTriggerCollider.transform.position = (raycastHit).point;
      }
      else
        NXOUI.RestoreHand();
    }
    else
      NXOUI.RestoreHand();
  }

  private static void RestoreHand()
  {
    if (!NXOUI._oldLocalPos.HasValue)
      return;
    Variables.taggerInstance.rightHandTriggerCollider.transform.position = NXOUI._oldLocalPos.Value;
    NXOUI._oldLocalPos = new Vector3?();
  }

  private Texture2D Tex(string k)
  {
    Texture2D texture2D;
    return this._tex.TryGetValue(k, out texture2D) ? texture2D : this._pixel;
  }

  private void Put(string k, Texture2D t)
  {
    Texture2D texture2D;
    if (this._tex.TryGetValue(k, out texture2D) && (UnityEngine.Object) texture2D != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) texture2D);
    this._tex[k] = t;
  }

  private void Rebuild()
  {
    foreach (KeyValuePair<string, Texture2D> keyValuePair in this._tex)
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value);
    }
    this._tex.Clear();
    foreach (KeyValuePair<long, Texture2D> keyValuePair in this._roundCache)
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value);
    }
    this._roundCache.Clear();
    this._roundUsed.Clear();
    this._bgTex = (Texture2D) null;
    foreach (KeyValuePair<int, Texture2D> keyValuePair in this._modBarTexCache)
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value);
    }
    this._modBarTexCache.Clear();
    this._modBarTexUsed.Clear();
    if ((UnityEngine.Object) this._watermarkTex != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this._watermarkTex);
      this._watermarkTex = (Texture2D) null;
    }
    this._lastWatermarkFps = -1;
    this.Put("field", NXOUI.MakeRect(Mathf.Max(1, (int) ((double) NXOUI.GlobalWidth - 24.0)), 26, new Color32((byte) 18, (byte) 18, (byte) 18, (byte) 245), new Color32((byte) 70, (byte) 70, (byte) 70, (byte) 210), 1, 8));
    this.Put("panel", NXOUI.MakeRect(180, 180, new Color32((byte) 14, (byte) 14, (byte) 14, (byte) 232), new Color32((byte) 58, (byte) 58, (byte) 58, (byte) 175), 1, 14));
    this.Put("side", NXOUI.MakeRect(120, 24, new Color32((byte) 23, (byte) 23, (byte) 23, (byte) 210), new Color32(), 0, 9));
    this.Put("sideH", NXOUI.MakeRect(120, 24, new Color32((byte) 37, (byte) 37, (byte) 37, (byte) 235), new Color32(), 0, 9));
    this.Put("sideOn", NXOUI.MakeRect(120, 24, new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 245), new Color32((byte) 120, (byte) 120, (byte) 120, (byte) 80 /*0x50*/), 1, 9));
    this.Put("card", NXOUI.MakeRect(380, 30, new Color32((byte) 20, (byte) 20, (byte) 20, (byte) 236), new Color32((byte) 62, (byte) 62, (byte) 62, (byte) 155), 1, 11));
    this.Put("cardH", NXOUI.MakeRect(380, 30, new Color32((byte) 32 /*0x20*/, (byte) 32 /*0x20*/, (byte) 32 /*0x20*/, (byte) 246), new Color32((byte) 92, (byte) 92, (byte) 92, (byte) 190), 1, 11));
    this.Put("cardOn", NXOUI.MakeRect(380, 30, new Color32((byte) 36, (byte) 42, (byte) 46, (byte) 250), new Color32((byte) 110, (byte) 160 /*0xA0*/, (byte) 170, (byte) 210), 1, 11));
    this.Put("mini", NXOUI.MakeRect(62, 22, new Color32((byte) 30, (byte) 30, (byte) 30, (byte) 230), new Color32((byte) 76, (byte) 76, (byte) 76, (byte) 170), 1, 6));
    this.Put("miniH", NXOUI.MakeRect(62, 22, new Color32((byte) 48 /*0x30*/, (byte) 48 /*0x30*/, (byte) 48 /*0x30*/, (byte) 245), new Color32((byte) 125, (byte) 125, (byte) 125, (byte) 200), 1, 6));
    this.Put("toggle", NXOUI.MakeRect(52, 20, new Color32((byte) 26, (byte) 26, (byte) 26, (byte) 245), new Color32((byte) 88, (byte) 88, (byte) 88, (byte) 205), 1, 10));
    this.Put("toggleH", NXOUI.MakeRect(52, 20, new Color32((byte) 40, (byte) 40, (byte) 40, (byte) 248), new Color32((byte) 120, (byte) 120, (byte) 120, (byte) 220), 1, 10));
    this.Put("toggleOn", NXOUI.MakeRect(52, 20, new Color32((byte) 27, (byte) 75, (byte) 85, (byte) 250), new Color32((byte) 90, (byte) 225, (byte) 240 /*0xF0*/, (byte) 230), 1, 10));
    this.Put("knob", NXOUI.MakeRect(14, 14, new Color32((byte) 235, (byte) 235, (byte) 235, byte.MaxValue), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 210), 1, 7));
    this.Put("scrollTrack", NXOUI.MakeRect(5, 120, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 28), new Color32(), 0, 3));
    this.Put("scrollThumb", NXOUI.MakeRect(5, 38, new Color32((byte) 210, (byte) 210, (byte) 210, (byte) 185), new Color32(), 0, 3));
    this.BuildStyles();
  }

  private static Texture2D MakeRect(int w, int h, Color32 fill, Color32 bord, int bw, int r)
  {
    w = Mathf.Max(1, w);
    h = Mathf.Max(1, h);
    r = Mathf.Clamp(r, 0, Mathf.Min(w, h) / 2);
    bw = Mathf.Max(0, bw);
    Texture2D texture2D1 = new Texture2D(w, h, TextureFormat.RGBA32, false);
    texture2D1.filterMode = FilterMode.Bilinear;
    texture2D1.wrapMode = TextureWrapMode.Clamp;
    Texture2D texture2D2 = texture2D1;
    Color32[] colors = new Color32[w * h];
    float num1 = (float) r;
    float num2 = 1.25f;
    for (int index1 = 0; index1 < h; ++index1)
    {
      for (int index2 = 0; index2 < w; ++index2)
      {
        float num3 = (float) index2 + 0.5f;
        float num4 = (float) index1 + 0.5f;
        float a1 = Mathf.Abs(num3 - (float) w * 0.5f) - ((float) w * 0.5f - num1);
        float num5 = Mathf.Abs(num4 - (float) h * 0.5f) - ((float) h * 0.5f - num1);
        float num6 = Mathf.Max(a1, 0.0f);
        float num7 = Mathf.Max(num5, 0.0f);
        float num8 = Mathf.Clamp01((float) (0.5 - (double) (Mathf.Sqrt((float) ((double) num6 * (double) num6 + (double) num7 * (double) num7)) + Mathf.Min(Mathf.Max(a1, num5), 0.0f) - num1) / (double) num2));
        Color color = (Color) fill;
        if (bw > 0)
        {
          float num9 = Mathf.Max(0.0f, num1 - (float) bw);
          float a2 = Mathf.Abs(num3 - (float) w * 0.5f) - ((float) w * 0.5f - num1 + (float) bw);
          float num10 = Mathf.Abs(num4 - (float) h * 0.5f) - ((float) h * 0.5f - num1 + (float) bw);
          float num11 = Mathf.Max(a2, 0.0f);
          float num12 = Mathf.Max(num10, 0.0f);
          float t = Mathf.Clamp01((float) (0.5 - (double) (Mathf.Sqrt((float) ((double) num11 * (double) num11 + (double) num12 * (double) num12)) + Mathf.Min(Mathf.Max(a2, num10), 0.0f) - num9) / (double) num2));
          color = Color.Lerp((Color) bord, (Color) fill, t);
        }
        color.a *= num8;
        colors[index1 * w + index2] = (Color32) color;
      }
    }
    texture2D2.SetPixels32(colors);
    texture2D2.Apply(false, true);
    return texture2D2;
  }

  private void BuildStyles()
  {
    this._sMod = new GUIStyle()
    {
      fontSize = 13,
      font = this._font,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 3,
      wordWrap = false,
      normal = {
        textColor = Color.white
      }
    };
    this._sField = new GUIStyle(GUI.skin.textField)
    {
      fontSize = 11,
      font = this._font,
      fontStyle = (FontStyle) 0,
      alignment = (TextAnchor) 3,
      border = new RectOffset(),
      padding = new RectOffset(0, 0, 0, 0),
      margin = new RectOffset(),
      normal = {
        background = (Texture2D) null
      },
      hover = {
        background = (Texture2D) null
      },
      active = {
        background = (Texture2D) null
      },
      focused = {
        background = (Texture2D) null
      }
    };
    NXOUI.SetAllTextColor(this._sField, Color.white);
    this._sBtn = new GUIStyle(GUI.skin.button)
    {
      fontSize = 10,
      font = this._font,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 4,
      richText = true
    };
    this._sBtnOn = new GUIStyle(this._sBtn);
    this._sTitle = new GUIStyle()
    {
      fontSize = 16 /*0x10*/,
      font = this._font,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 3,
      richText = true,
      normal = {
        textColor = Color.white
      }
    };
    this._sDim = new GUIStyle()
    {
      fontSize = 10,
      font = this._font,
      alignment = (TextAnchor) 3,
      richText = true,
      normal = {
        textColor = (Color) new Color32((byte) 155, (byte) 155, (byte) 155, byte.MaxValue)
      }
    };
    this._sSmall = new GUIStyle()
    {
      fontSize = 10,
      font = this._font,
      alignment = (TextAnchor) 3,
      richText = true,
      normal = {
        textColor = (Color) new Color32((byte) 180, (byte) 180, (byte) 180, byte.MaxValue)
      }
    };
    this._sCardTitle = new GUIStyle()
    {
      fontSize = 12,
      font = this._font,
      fontStyle = (FontStyle) 1,
      alignment = (TextAnchor) 3,
      richText = true,
      normal = {
        textColor = Color.white
      }
    };
    this._sCardDesc = new GUIStyle()
    {
      fontSize = 9,
      font = this._font,
      alignment = (TextAnchor) 3,
      richText = true,
      clipping = (TextClipping) 1,
      normal = {
        textColor = (Color) new Color32((byte) 145, (byte) 145, (byte) 145, byte.MaxValue)
      }
    };
    this._sPill = new GUIStyle()
    {
      fontSize = 10,
      font = this._font,
      alignment = (TextAnchor) 4,
      richText = true,
      normal = {
        textColor = (Color) new Color32((byte) 220, (byte) 220, (byte) 220, byte.MaxValue)
      }
    };
    this._sWatermark = new GUIStyle()
    {
      fontSize = 16 /*0x10*/,
      font = this._font,
      alignment = (TextAnchor) 3,
      richText = true,
      normal = {
        textColor = Color.white
      }
    };
  }

  private static void SetAllStates(GUIStyle s, Texture2D t)
  {
    if ((UnityEngine.Object) t != (UnityEngine.Object) null)
      s.normal.background = s.active.background = s.hover.background = s.focused.background = t;
    s.normal.textColor = s.active.textColor = s.hover.textColor = s.focused.textColor = Color.white;
  }

  private static void SetAllTextColor(GUIStyle s, Color c)
  {
    s.normal.textColor = s.active.textColor = s.hover.textColor = s.focused.textColor = c;
  }

  private void EnsureBgTexture()
  {
    int w = Mathf.Max(1, (int) NXOUI.GlobalWidth);
    int h = Mathf.Max(1, (int) NXOUI.GlobalHeight);
    if ((UnityEngine.Object) this._bgTex != (UnityEngine.Object) null && this._bgTex.width == w && this._bgTex.height == h && this._tex.ContainsKey("bg"))
      return;
    if ((UnityEngine.Object) this._bgTex != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this._bgTex);
    this._bgTex = NXOUI.MakeRect(w, h, new Color32((byte) 10, (byte) 10, (byte) 10, (byte) 226), new Color32((byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 155), 1, 16 /*0x10*/);
    this._tex["bg"] = this._bgTex;
  }
}
