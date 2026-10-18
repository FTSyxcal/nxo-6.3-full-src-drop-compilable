
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable enable
namespace NXO.Utilities;

public static class ControllerEmulator
{
  public static bool EmulatorEnabled = false;
  private static bool _virtualRTrigger = false;
  private static bool _virtualLTrigger = false;
  private static bool _virtualRGrip = false;
  private static bool _virtualLGrip = false;
  private static bool _virtualRPrimary = false;
  private static bool _virtualLPrimary = false;
  private static bool _virtualRSecondary = false;
  private static bool _virtualLSecondary = false;
  private static Texture2D _hudBackgroundTexture;
  private static GUIStyle _backgroundStyle;
  private static GUIStyle _headerStyle;
  private static GUIStyle _tableHeaderStyle;
  private static GUIStyle _rowStyle;
  private static GUIStyle _activeStyle;
  private static GUIStyle _keyCenterStyle;
  private static bool _stylesInitialized = false;
  private const float HUD_WIDTH = 280f;
  private const float HUD_HEIGHT = 210f;
  private const float ROW_HEIGHT = 20f;
  private static readonly string[] ControllerNames = new string[8]
  {
    "Right Trigger",
    "Left Trigger",
    "Right Bumper",
    "Left Bumper",
    "A Button",
    "B Button",
    "X Button",
    "Y Button"
  };
  private static readonly string[] KeyNames = new string[8]
  {
    "T",
    "R",
    "G",
    "F",
    "Y",
    "U",
    "E",
    "Q"
  };

  public static void Update()
  {
    if (!ControllerEmulator.EmulatorEnabled)
      return;
    ControllerEmulator._virtualRTrigger = ((ButtonControl) Keyboard.current[(Key) 34]).isPressed;
    ControllerEmulator._virtualLTrigger = ((ButtonControl) Keyboard.current[(Key) 32 /*0x20*/]).isPressed;
    ControllerEmulator._virtualRGrip = ((ButtonControl) Keyboard.current[(Key) 21]).isPressed;
    ControllerEmulator._virtualLGrip = ((ButtonControl) Keyboard.current[(Key) 20]).isPressed;
    ControllerEmulator._virtualRPrimary = ((ButtonControl) Keyboard.current[(Key) 39]).isPressed;
    ControllerEmulator._virtualRSecondary = ((ButtonControl) Keyboard.current[(Key) 35]).isPressed;
    ControllerEmulator._virtualLPrimary = ((ButtonControl) Keyboard.current[(Key) 19]).isPressed;
    ControllerEmulator._virtualLSecondary = ((ButtonControl) Keyboard.current[(Key) 31 /*0x1F*/]).isPressed;
  }

  public static bool GetRTrigger() => ControllerEmulator._virtualRTrigger;

  public static bool GetLTrigger() => ControllerEmulator._virtualLTrigger;

  public static bool GetRGrip() => ControllerEmulator._virtualRGrip;

  public static bool GetLGrip() => ControllerEmulator._virtualLGrip;

  public static bool GetRPrimary() => ControllerEmulator._virtualRPrimary;

  public static bool GetLPrimary() => ControllerEmulator._virtualLPrimary;

  public static bool GetRSecondary() => ControllerEmulator._virtualRSecondary;

  public static bool GetLSecondary() => ControllerEmulator._virtualLSecondary;

  public static void ShowControllerHUD()
  {
    if (!ControllerEmulator.EmulatorEnabled)
      return;
    if (!ControllerEmulator._stylesInitialized)
      ControllerEmulator.InitializeStyles();
    Rect rect = new Rect(15f, (float) ((double) Screen.height - 210.0 - 15.0), 280f, 210f);
    GUI.Box(rect, "", ControllerEmulator._backgroundStyle);
    float y1 = rect.y + 15f;
    GUI.Label(new Rect(rect.x + 20f, y1, 140f, 20f), "Controller", ControllerEmulator._tableHeaderStyle);
    GUI.Label(new Rect(rect.x + 145f, y1, 40f, 20f), "Key", ControllerEmulator._tableHeaderStyle);
    GUI.Label(new Rect(rect.x + 210f, y1, 60f, 20f), "Status", ControllerEmulator._tableHeaderStyle);
    float y2 = y1 + 22f;
    GUI.color = new Color(1f, 1f, 1f, 0.1f);
    GUI.DrawTexture(new Rect(rect.x + 20f, y2, rect.width - 40f, 1f), (Texture) Texture2D.whiteTexture);
    GUI.color = Color.white;
    float y3 = y2 + 6f;
    bool[] flagArray = new bool[8]
    {
      ControllerEmulator._virtualRTrigger,
      ControllerEmulator._virtualLTrigger,
      ControllerEmulator._virtualRGrip,
      ControllerEmulator._virtualLGrip,
      ControllerEmulator._virtualRPrimary,
      ControllerEmulator._virtualRSecondary,
      ControllerEmulator._virtualLPrimary,
      ControllerEmulator._virtualLSecondary
    };
    for (int index = 0; index < 8; ++index)
    {
      ControllerEmulator.DrawControllerRow(rect.x + 20f, y3, ControllerEmulator.ControllerNames[index], ControllerEmulator.KeyNames[index], flagArray[index]);
      y3 += 20f;
    }
  }

  private static void DrawControllerRow(
    float x,
    float y,
    string controller,
    string key,
    bool isActive)
  {
    GUI.Label(new Rect(x, y, 140f, 19f), controller, isActive ? ControllerEmulator._activeStyle : ControllerEmulator._rowStyle);
    GUI.Label(new Rect(x + 117.5f, y, 40f, 19f), key, ControllerEmulator._keyCenterStyle);
    GUI.Label(new Rect(x + 190f, y, 60f, 19f), isActive ? "ON" : "OFF", isActive ? ControllerEmulator._activeStyle : ControllerEmulator._rowStyle);
  }

  private static Texture2D CreateOptimizedHUDTexture()
  {
    int width = 280;
    int height = 210;
    int num1 = 12;
    Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false);
    texture2D.filterMode = FilterMode.Bilinear;
    Texture2D optimizedHudTexture = texture2D;
    Color32 color32_1 = new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 180);
    Color32 color32_2 = new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0);
    Color32[] colors = new Color32[width * height];
    for (int index1 = 0; index1 < height; ++index1)
    {
      for (int index2 = 0; index2 < width; ++index2)
      {
        int index3 = index1 * width + index2;
        int num2 = index2;
        int num3 = index1;
        bool flag = false;
        if (index2 < num1 && index1 >= height - num1)
        {
          num2 = num1;
          num3 = height - num1;
          flag = true;
        }
        else if (index2 >= width - num1 && index1 >= height - num1)
        {
          num2 = width - num1 - 1;
          num3 = height - num1;
          flag = true;
        }
        else if (index2 < num1 && index1 < num1)
        {
          num2 = num1;
          num3 = num1;
          flag = true;
        }
        else if (index2 >= width - num1 && index1 < num1)
        {
          num2 = width - num1 - 1;
          num3 = num1;
          flag = true;
        }
        if (flag)
        {
          float num4 = Mathf.Sqrt((float) ((index2 - num2) * (index2 - num2) + (index1 - num3) * (index1 - num3)));
          colors[index3] = (double) num4 > (double) num1 ? color32_2 : color32_1;
        }
        else
          colors[index3] = color32_1;
      }
    }
    optimizedHudTexture.SetPixels32(colors);
    optimizedHudTexture.Apply(false);
    return optimizedHudTexture;
  }

  private static void InitializeStyles()
  {
    ControllerEmulator._hudBackgroundTexture = ControllerEmulator.CreateOptimizedHUDTexture();
    ControllerEmulator._backgroundStyle = new GUIStyle()
    {
      normal = {
        background = ControllerEmulator._hudBackgroundTexture
      }
    };
    ControllerEmulator._headerStyle = new GUIStyle()
    {
      fontSize = 18,
      fontStyle = (FontStyle) 1,
      normal = {
        textColor = Color.white
      },
      alignment = (TextAnchor) 4
    };
    ControllerEmulator._tableHeaderStyle = new GUIStyle()
    {
      fontSize = 14,
      fontStyle = (FontStyle) 1,
      normal = {
        textColor = Color.white
      },
      alignment = (TextAnchor) 3
    };
    ControllerEmulator._rowStyle = new GUIStyle()
    {
      fontSize = 12,
      normal = {
        textColor = (Color) new Color32((byte) 180, (byte) 180, (byte) 180, byte.MaxValue)
      },
      alignment = (TextAnchor) 3
    };
    ControllerEmulator._activeStyle = new GUIStyle()
    {
      fontSize = 12,
      fontStyle = (FontStyle) 1,
      normal = {
        textColor = Color.white
      },
      alignment = (TextAnchor) 3
    };
    ControllerEmulator._keyCenterStyle = new GUIStyle()
    {
      fontSize = 12,
      normal = {
        textColor = (Color) new Color32((byte) 180, (byte) 180, (byte) 180, byte.MaxValue)
      },
      alignment = (TextAnchor) 4
    };
    ControllerEmulator._stylesInitialized = true;
  }

  public static void Cleanup()
  {
    if ((Object) ControllerEmulator._hudBackgroundTexture != (Object) null)
    {
      Object.DestroyImmediate((Object) ControllerEmulator._hudBackgroundTexture);
      ControllerEmulator._hudBackgroundTexture = (Texture2D) null;
    }
    ControllerEmulator._backgroundStyle = (GUIStyle) null;
    ControllerEmulator._headerStyle = (GUIStyle) null;
    ControllerEmulator._tableHeaderStyle = (GUIStyle) null;
    ControllerEmulator._rowStyle = (GUIStyle) null;
    ControllerEmulator._activeStyle = (GUIStyle) null;
    ControllerEmulator._keyCenterStyle = (GUIStyle) null;
    ControllerEmulator._stylesInitialized = false;
  }
}
