
using BepInEx;
using NXO.Mods.Categories;
using NXO.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
namespace NXO.Menu;

public class SearchAndKeyboard : MonoBehaviour
{
  public static Text searchText;
  public static string inputText = "";
  public static GameObject keyboardObj;
  public static bool isSearching;
  public static bool shouldShowSearchText;
  public static bool isTyping;
  private static Material _keyboardBgMaterial;
  private static Material _outlineMaterial;
  private static readonly List<Material> _keyboardMaterials = new List<Material>();
  private static readonly List<GameObject> _keyboardObjects = new List<GameObject>();
  private static readonly List<Mesh> _keyboardMeshes = new List<Mesh>();
  public static List<KeyCode> lastPressedKeys = new List<KeyCode>();
  private static readonly Dictionary<string, SearchAndKeyboard.KeyCollider> _keyColliders = new Dictionary<string, SearchAndKeyboard.KeyCollider>();
  private static readonly List<(Main.ColorRole role, GameObject obj, List<GameObject> parts)> _keyboardColorGroups = new List<(Main.ColorRole, GameObject, List<GameObject>)>();
  private const float KeyBouncePunch = 1.25f;
  private const float KeyBounceDuration = 0.18f;
  private static readonly KeyCode[] _allowedKeys = new KeyCode[39]
  {
    KeyCode.A,
    KeyCode.B,
    KeyCode.C,
    KeyCode.D,
    KeyCode.E,
    KeyCode.F,
    KeyCode.G,
    KeyCode.H,
    KeyCode.I,
    KeyCode.J,
    KeyCode.K,
    KeyCode.L,
    KeyCode.M,
    KeyCode.N,
    KeyCode.O,
    KeyCode.P,
    KeyCode.Q,
    KeyCode.R,
    KeyCode.S,
    KeyCode.T,
    KeyCode.U,
    KeyCode.V,
    KeyCode.W,
    KeyCode.X,
    KeyCode.Y,
    KeyCode.Z,
    KeyCode.Space,
    KeyCode.Backspace,
    KeyCode.Return,
    KeyCode.Alpha0,
    KeyCode.Alpha1,
    KeyCode.Alpha2,
    KeyCode.Alpha3,
    KeyCode.Alpha4,
    KeyCode.Alpha5,
    KeyCode.Alpha6,
    KeyCode.Alpha7,
    KeyCode.Alpha8,
    KeyCode.Alpha9
  };
  public static string placeholderText = "Type to search...";
  public static float _preSarchMenuScale = -1f;
  private static float _blinkTimer;
  private static bool _cursorVisible = true;
  public static Action<string> onTypingComplete;
  public static Action onTypingCancelled;
  private static Material _clickerMaterial1;
  private static Material _clickerMaterial2;

  public static KeyCode[] allowedKeys => SearchAndKeyboard._allowedKeys;

  private static void RegisterKeyboardColorGroup(
    Main.ColorRole role,
    GameObject obj,
    List<GameObject> parts)
  {
    SearchAndKeyboard._keyboardColorGroups.Add((role, obj, parts));
    Main.RegisterColorGroup(role, obj, parts);
  }

  public static void ReRegisterKeyboardColorGroups()
  {
    foreach ((Main.ColorRole role, GameObject gameObject, List<GameObject> gameObjectList) in SearchAndKeyboard._keyboardColorGroups)
    {
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        Main.RegisterColorGroup(role, gameObject, gameObjectList);
    }
  }

  public static void CleanupKeyboard()
  {
    if ((UnityEngine.Object) SearchAndKeyboard.keyboardObj != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) SearchAndKeyboard.keyboardObj);
      SearchAndKeyboard.keyboardObj = (GameObject) null;
    }
    if ((UnityEngine.Object) Variables.keyclickerObj1 != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) Variables.keyclickerObj1);
      Variables.keyclickerObj1 = (GameObject) null;
    }
    if ((UnityEngine.Object) Variables.keyclickerObj2 != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) Variables.keyclickerObj2);
      Variables.keyclickerObj2 = (GameObject) null;
    }
    foreach (Material keyboardMaterial in SearchAndKeyboard._keyboardMaterials)
    {
      if ((UnityEngine.Object) keyboardMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardMaterial);
    }
    SearchAndKeyboard._keyboardMaterials.Clear();
    foreach (GameObject keyboardObject in SearchAndKeyboard._keyboardObjects)
    {
      if ((UnityEngine.Object) keyboardObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardObject);
    }
    SearchAndKeyboard._keyboardObjects.Clear();
    foreach (Mesh keyboardMesh in SearchAndKeyboard._keyboardMeshes)
    {
      if ((UnityEngine.Object) keyboardMesh != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardMesh);
    }
    SearchAndKeyboard._keyboardMeshes.Clear();
    if ((UnityEngine.Object) SearchAndKeyboard._clickerMaterial1 != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) SearchAndKeyboard._clickerMaterial1);
      SearchAndKeyboard._clickerMaterial1 = (Material) null;
    }
    if ((UnityEngine.Object) SearchAndKeyboard._clickerMaterial2 != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) SearchAndKeyboard._clickerMaterial2);
      SearchAndKeyboard._clickerMaterial2 = (Material) null;
    }
    SearchAndKeyboard._keyboardBgMaterial = (Material) null;
    SearchAndKeyboard._outlineMaterial = (Material) null;
    SearchAndKeyboard._keyboardColorGroups.Clear();
    SearchAndKeyboard._keyColliders.Clear();
    SearchAndKeyboard.inputText = "";
  }

  public static string KeyCodeToString(KeyCode keyCode)
  {
    KeyCode keyCode1 = keyCode;
    if (true)
      ;
    string str;
    switch (keyCode1)
    {
      case KeyCode.Backspace:
        str = "BACK";
        break;
      case KeyCode.Return:
        str = "ENTER";
        break;
      case KeyCode.Space:
        str = "SPACE";
        break;
      case KeyCode.Alpha0:
      case KeyCode.Alpha1:
      case KeyCode.Alpha2:
      case KeyCode.Alpha3:
      case KeyCode.Alpha4:
      case KeyCode.Alpha5:
      case KeyCode.Alpha6:
      case KeyCode.Alpha7:
      case KeyCode.Alpha8:
      case KeyCode.Alpha9:
        str = ((int) (keyCode1 - 48 /*0x30*/)).ToString();
        break;
      default:
        str = keyCode.ToString();
        break;
    }
    if (true)
      ;
    return str;
  }

  public static void ToggleKeyboard()
  {
    if (SearchAndKeyboard.isSearching)
      SearchAndKeyboard.CloseKeyboard();
    else
      SearchAndKeyboard.OpenKeyboard(false);
  }

  public static void OpenKeyboard(bool typingMode, string prefill = "", string placeholder = "Type to search...")
  {
    SearchAndKeyboard.isSearching = true;
    SearchAndKeyboard.isTyping = typingMode;
    SearchAndKeyboard.shouldShowSearchText = true;
    SearchAndKeyboard.inputText = prefill;
    SearchAndKeyboard.placeholderText = placeholder;
    if (Variables.InMenuCondition && (double) Main.menuScale != 0.75)
    {
      SearchAndKeyboard._preSarchMenuScale = Main.menuScale;
      Main.menuScale = 0.75f;
    }
    if (!Variables.InPcCondition)
    {
      SearchAndKeyboard.DrawKeyboard();
      SearchAndKeyboard.KeyClicker1(Variables.playerInstance.RightHand.controllerTransform);
      SearchAndKeyboard.KeyClicker2(Variables.playerInstance.LeftHand.controllerTransform);
    }
    Main.ShowSearchBar();
  }

  public static void CloseKeyboard()
  {
    SearchAndKeyboard.isSearching = false;
    SearchAndKeyboard.isTyping = false;
    SearchAndKeyboard.onTypingComplete = (Action<string>) null;
    SearchAndKeyboard.onTypingCancelled = (Action) null;
    SearchAndKeyboard.shouldShowSearchText = false;
    Main._lastKeyboardYRotation = float.MinValue;
    Main._lastKeyboardSnapPosition = Vector3.zero;
    if ((double) SearchAndKeyboard._preSarchMenuScale > 0.0)
    {
      Main.menuScale = SearchAndKeyboard._preSarchMenuScale;
      SearchAndKeyboard._preSarchMenuScale = -1f;
    }
    SearchAndKeyboard.CleanupKeyboard();
    Main.HideSearchBar();
  }

  public static void UpdateSearchBlink()
  {
    if (!SearchAndKeyboard.shouldShowSearchText || (UnityEngine.Object) SearchAndKeyboard.searchText == (UnityEngine.Object) null)
      return;
    SearchAndKeyboard._blinkTimer += Time.deltaTime;
    if ((double) SearchAndKeyboard._blinkTimer < 0.5)
      return;
    SearchAndKeyboard._blinkTimer = 0.0f;
    SearchAndKeyboard._cursorVisible = !SearchAndKeyboard._cursorVisible;
    string str = string.IsNullOrEmpty(SearchAndKeyboard.inputText) ? SearchAndKeyboard.placeholderText : SearchAndKeyboard.inputText;
    SearchAndKeyboard.searchText.text = str + (SearchAndKeyboard._cursorVisible ? "|" : "");
  }

  public static void HandleKeyboardInput()
  {
    if (!Variables.InPcCondition || !SearchAndKeyboard.isSearching && !SearchAndKeyboard.isTyping)
      return;
    SearchAndKeyboard.lastPressedKeys.Clear();
    for (int index = 0; index < SearchAndKeyboard.allowedKeys.Length; ++index)
    {
      if (UnityInput.Current.GetKeyDown(SearchAndKeyboard.allowedKeys[index]))
      {
        SearchAndKeyboard.HandleKeyPress(SearchAndKeyboard.KeyCodeToString(SearchAndKeyboard.allowedKeys[index]));
        SearchAndKeyboard.lastPressedKeys.Add(SearchAndKeyboard.allowedKeys[index]);
      }
    }
  }

  public static void OpenTypingKeyboard(string prefill = "", string placeholder = "Type here...")
  {
    SearchAndKeyboard.onTypingComplete = (Action<string>) null;
    SearchAndKeyboard.onTypingCancelled = (Action) null;
    SearchAndKeyboard.isSearching = true;
    SearchAndKeyboard.isTyping = true;
    SearchAndKeyboard.shouldShowSearchText = true;
    SearchAndKeyboard.inputText = prefill;
    SearchAndKeyboard.placeholderText = placeholder;
    if (Variables.InMenuCondition && (double) Main.menuScale != 0.75)
    {
      SearchAndKeyboard._preSarchMenuScale = Main.menuScale;
      Main.menuScale = 0.75f;
    }
    if (!Variables.InPcCondition)
    {
      SearchAndKeyboard.DrawKeyboard();
      SearchAndKeyboard.KeyClicker1(Variables.playerInstance.RightHand.controllerTransform);
      SearchAndKeyboard.KeyClicker2(Variables.playerInstance.LeftHand.controllerTransform);
    }
    Main.RefreshMenu();
  }

  public static void CloseTypingKeyboard(bool cancelled = false)
  {
    string str = SearchAndKeyboard.inputText.Trim();
    SearchAndKeyboard.isSearching = false;
    SearchAndKeyboard.isTyping = false;
    SearchAndKeyboard.shouldShowSearchText = false;
    Main._lastKeyboardYRotation = float.MinValue;
    Main._lastKeyboardSnapPosition = Vector3.zero;
    if ((double) SearchAndKeyboard._preSarchMenuScale > 0.0)
    {
      Main.menuScale = SearchAndKeyboard._preSarchMenuScale;
      SearchAndKeyboard._preSarchMenuScale = -1f;
    }
    Action<string> onTypingComplete = SearchAndKeyboard.onTypingComplete;
    Action onTypingCancelled = SearchAndKeyboard.onTypingCancelled;
    SearchAndKeyboard.onTypingComplete = (Action<string>) null;
    SearchAndKeyboard.onTypingCancelled = (Action) null;
    SearchAndKeyboard.CleanupKeyboard();
    Main.RefreshMenu();
    if (cancelled)
    {
      if (onTypingCancelled == null)
        return;
      onTypingCancelled();
    }
    else
    {
      if (onTypingComplete == null)
        return;
      onTypingComplete(str);
    }
  }

  public static int FuzzyScore(string text, string query)
  {
    if (string.IsNullOrEmpty(query))
      return 0;
    if (string.IsNullOrEmpty(text))
      return int.MinValue;
    string lowerInvariant1 = text.ToLowerInvariant();
    string lowerInvariant2 = query.ToLowerInvariant();
    int num1 = lowerInvariant1.IndexOf(lowerInvariant2);
    if (num1 == 0)
      return 10000;
    if (num1 > 0)
      return 5000 - num1;
    int num2 = 0;
    int index1 = 0;
    int num3 = -1;
    int num4 = 0;
    for (int index2 = 0; index2 < lowerInvariant1.Length && index1 < lowerInvariant2.Length; ++index2)
    {
      if ((int) lowerInvariant1[index2] == (int) lowerInvariant2[index1])
      {
        if (num3 == index2 - 1)
          ++num4;
        else
          num4 = 0;
        num2 += 10 + num4 * 8;
        if (index2 == 0 || lowerInvariant1[index2 - 1] == ' ')
          num2 += 15;
        num3 = index2;
        ++index1;
      }
    }
    return index1 < lowerInvariant2.Length ? int.MinValue : num2 - (lowerInvariant1.Length - lowerInvariant2.Length);
  }

  private static List<GameObject> RoundKeyboard(GameObject panel, float roundness)
  {
    List<GameObject> gameObjectList = new List<GameObject>(1);
    if ((UnityEngine.Object) panel == (UnityEngine.Object) null)
      return gameObjectList;
    Renderer component = panel.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return gameObjectList;
    Transform parent = panel.transform.parent ?? SearchAndKeyboard.keyboardObj.transform;
    GameObject gameObject = new GameObject("rounded_" + panel.name);
    gameObject.transform.SetParent(parent, false);
    gameObject.transform.localPosition = panel.transform.localPosition;
    gameObject.transform.localRotation = panel.transform.localRotation;
    gameObject.transform.localScale = Vector3.one;
    gameObject.layer = panel.layer;
    Mesh mesh = SearchAndKeyboard.KeyboardRoundObj(panel.transform.localScale, roundness);
    SearchAndKeyboard._keyboardMeshes.Add(mesh);
    gameObject.AddComponent<MeshFilter>().mesh = mesh;
    Material material = new Material(component.sharedMaterial);
    material.SetInt("_Cull", 0);
    gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    SearchAndKeyboard._keyboardMaterials.Add(material);
    SearchAndKeyboard._keyboardObjects.Add(gameObject);
    component.enabled = false;
    gameObjectList.Add(gameObject);
    return gameObjectList;
  }

  private static Material CreateMaterial(Color color, int renderQueue)
  {
    return Main.CreateColorMaterial(color, renderQueue);
  }

  private static Material CreateOutlineMaterial()
  {
    if (Settings.OutlineColorMode == Settings.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = Main.GetPinwheelMaterial();
      if ((UnityEngine.Object) pinwheelMaterial != (UnityEngine.Object) null)
      {
        SearchAndKeyboard._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    return SearchAndKeyboard.CreateMaterial((Color) Settings.OutlineColor, 2455);
  }

  private static Material CreateBackgroundMaterial()
  {
    if (Settings.BackgroundMode == Settings.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = Main.GetPinwheelMaterial();
      if ((UnityEngine.Object) pinwheelMaterial != (UnityEngine.Object) null)
      {
        SearchAndKeyboard._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    Material material = SearchAndKeyboard.CreateMaterial((Color) Settings.BackgroundColor, 2450);
    SearchAndKeyboard._keyboardMaterials.Add(material);
    return material;
  }

  private static Material CreateButtonMaterial()
  {
    if (Settings.ButtonMode == Settings.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = Main.GetPinwheelMaterial();
      if ((UnityEngine.Object) pinwheelMaterial != (UnityEngine.Object) null)
      {
        SearchAndKeyboard._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    Material material = SearchAndKeyboard.CreateMaterial((Color) Settings.ButtonColor, 2460);
    SearchAndKeyboard._keyboardMaterials.Add(material);
    return material;
  }

  private static GameObject CreateKeyboardPrimitive(Transform parent)
  {
    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
    UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<Rigidbody>());
    UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<BoxCollider>());
    primitive.layer = LayerMask.NameToLayer("UI");
    primitive.transform.SetParent(parent, false);
    primitive.transform.localRotation = Quaternion.identity;
    SearchAndKeyboard._keyboardObjects.Add(primitive);
    return primitive;
  }

  public static void DrawKeyboard()
  {
    if ((UnityEngine.Object) SearchAndKeyboard.keyboardObj != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) SearchAndKeyboard.keyboardObj);
      SearchAndKeyboard.keyboardObj = (GameObject) null;
    }
    foreach (Material keyboardMaterial in SearchAndKeyboard._keyboardMaterials)
    {
      if ((UnityEngine.Object) keyboardMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardMaterial);
    }
    SearchAndKeyboard._keyboardMaterials.Clear();
    foreach (GameObject keyboardObject in SearchAndKeyboard._keyboardObjects)
    {
      if ((UnityEngine.Object) keyboardObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardObject);
    }
    SearchAndKeyboard._keyboardObjects.Clear();
    foreach (Mesh keyboardMesh in SearchAndKeyboard._keyboardMeshes)
    {
      if ((UnityEngine.Object) keyboardMesh != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyboardMesh);
    }
    SearchAndKeyboard._keyboardMeshes.Clear();
    SearchAndKeyboard._keyboardColorGroups.Clear();
    SearchAndKeyboard._keyColliders.Clear();
    SearchAndKeyboard.keyboardObj = new GameObject("Keyboard");
    if (Settings.OutlineMode > 0)
    {
      GameObject keyboardPrimitive = SearchAndKeyboard.CreateKeyboardPrimitive(SearchAndKeyboard.keyboardObj.transform);
      keyboardPrimitive.name = "KeyboardOutline";
      keyboardPrimitive.transform.localScale = new Vector3(0.5f, 0.255f, 0.004f);
      keyboardPrimitive.transform.localPosition = new Vector3(0.0f, 1f / 500f, 3f / 500f);
      SearchAndKeyboard._outlineMaterial = SearchAndKeyboard.CreateOutlineMaterial();
      SearchAndKeyboard._keyboardMaterials.Add(SearchAndKeyboard._outlineMaterial);
      keyboardPrimitive.GetComponent<Renderer>().sharedMaterial = SearchAndKeyboard._outlineMaterial;
      List<GameObject> parts = (List<GameObject>) null;
      if ((double) Settings.MenuRoundness > 0.0)
        parts = SearchAndKeyboard.RoundKeyboard(keyboardPrimitive, 0.015f * Settings.MenuRoundness);
      SearchAndKeyboard.RegisterKeyboardColorGroup(Main.ColorRole.Outline, keyboardPrimitive, parts);
    }
    GameObject keyboardPrimitive1 = SearchAndKeyboard.CreateKeyboardPrimitive(SearchAndKeyboard.keyboardObj.transform);
    keyboardPrimitive1.name = "KeyboardBackground";
    keyboardPrimitive1.transform.localScale = new Vector3(0.485f, 0.24f, 0.005f);
    keyboardPrimitive1.transform.localPosition = new Vector3(0.0f, 1f / 500f, 0.005f);
    SearchAndKeyboard._keyboardBgMaterial = SearchAndKeyboard.CreateBackgroundMaterial();
    keyboardPrimitive1.GetComponent<Renderer>().sharedMaterial = SearchAndKeyboard._keyboardBgMaterial;
    List<GameObject> parts1 = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      parts1 = SearchAndKeyboard.RoundKeyboard(keyboardPrimitive1, 0.015f * Settings.MenuRoundness);
    SearchAndKeyboard.RegisterKeyboardColorGroup(Main.ColorRole.Background, keyboardPrimitive1, parts1);
    string[] strArray = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    float width1 = 0.04f;
    float num1 = 0.005f;
    float y1 = 0.092f;
    foreach (string str in strArray)
    {
      float x = (float) (-(((double) str.Length * (double) width1 + (double) (str.Length - 1) * (double) num1) / 2.0) + (double) width1 / 2.0);
      foreach (char ch in str)
      {
        SearchAndKeyboard.CreateKey(ch.ToString(), new Vector3(x, y1, 0.0f), width1);
        x += width1 + num1;
      }
      y1 -= width1 + num1;
    }
    float y2 = y1;
    float width2 = width1 * 5f;
    float width3 = width1 * 2.1f;
    float width4 = width1 * 2.1f;
    float num2 = (float) -((double) (width2 + num1 + width3 + num1 + width4) / 2.0);
    SearchAndKeyboard.CreateKey("SPACE", new Vector3(num2 + width2 / 2f, y2, 0.0f), width2);
    SearchAndKeyboard.CreateKey("BACK", new Vector3((float) ((double) num2 + (double) width2 + (double) num1 + (double) width3 / 2.0), y2, 0.0f), width3);
    SearchAndKeyboard.CreateKey("ENTER", new Vector3((float) ((double) num2 + (double) width2 + (double) num1 + (double) width3 + (double) num1 + (double) width4 / 2.0), y2, 0.0f), width4);
  }

  private static void CreateKey(string key, Vector3 position, float width, float height = 0.04f)
  {
    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
    ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
    primitive.layer = LayerMask.NameToLayer("UI");
    primitive.transform.SetParent(SearchAndKeyboard.keyboardObj.transform, false);
    primitive.transform.localScale = new Vector3(width - 1f / 400f, height - 1f / 400f, 0.012f);
    primitive.transform.localPosition = position;
    primitive.transform.localRotation = Quaternion.identity;
    SearchAndKeyboard._keyboardObjects.Add(primitive);
    Rigidbody rigidbody = primitive.AddComponent<Rigidbody>();
    rigidbody.isKinematic = true;
    rigidbody.useGravity = false;
    Material buttonMaterial = SearchAndKeyboard.CreateButtonMaterial();
    primitive.GetComponent<Renderer>().sharedMaterial = buttonMaterial;
    GameObject gameObject = new GameObject("KeyCanvas");
    gameObject.transform.SetParent(primitive.transform, false);
    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.52f);
    gameObject.transform.localRotation = Quaternion.identity;
    float num1 = width - 1f / 400f;
    float num2 = height - 1f / 400f;
    float num3 = 0.01f;
    gameObject.transform.localScale = new Vector3(num3 / num1 * num2, num3, num3);
    gameObject.layer = LayerMask.NameToLayer("UI");
    SearchAndKeyboard._keyboardObjects.Add(gameObject);
    Canvas canvas = gameObject.AddComponent<Canvas>();
    canvas.renderMode = (RenderMode) 2;
    canvas.sortingOrder = 10;
    Text text = new GameObject("Label").AddComponent<Text>();
    ((Component) text).transform.SetParent(gameObject.transform, false);
    text.font = Main.CurrentFont;
    text.text = key;
    text.fontSize = 50;
    ((Graphic) text).color = Color.white;
    text.alignment = (TextAnchor) 4;
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = 50;
    RectTransform component = ((Component) text).GetComponent<RectTransform>();
    component.sizeDelta = new Vector2(200f, 200f);
    component.localPosition = Vector3.zero;
    component.localRotation = Quaternion.identity;
    if (Settings.OutlineMode >= 2)
    {
      GameObject keyboardPrimitive = SearchAndKeyboard.CreateKeyboardPrimitive(SearchAndKeyboard.keyboardObj.transform);
      keyboardPrimitive.name = "KeyOutline";
      keyboardPrimitive.transform.localScale = new Vector3((float) ((double) width - 1.0 / 400.0 + 0.0040000001899898052), (float) ((double) height - 1.0 / 400.0 + 0.0040000001899898052), 11f / 1000f);
      keyboardPrimitive.transform.localPosition = new Vector3(position.x, position.y, position.z + 0.0005f);
      Material outlineMaterial = SearchAndKeyboard.CreateOutlineMaterial();
      SearchAndKeyboard._keyboardMaterials.Add(outlineMaterial);
      keyboardPrimitive.GetComponent<Renderer>().sharedMaterial = outlineMaterial;
      List<GameObject> parts = (List<GameObject>) null;
      if ((double) Settings.MenuRoundness > 0.0)
        parts = SearchAndKeyboard.RoundKeyboard(keyboardPrimitive, 0.007f * Settings.MenuRoundness);
      SearchAndKeyboard.RegisterKeyboardColorGroup(Main.ColorRole.Outline, keyboardPrimitive, parts);
    }
    List<GameObject> parts1 = (List<GameObject>) null;
    if ((double) Settings.MenuRoundness > 0.0)
      parts1 = SearchAndKeyboard.RoundKeyboard(primitive, 0.007f * Settings.MenuRoundness);
    SearchAndKeyboard.RegisterKeyboardColorGroup(Main.ColorRole.Button, primitive, parts1);
    SearchAndKeyboard.KeyCollider keyCollider = primitive.AddComponent<SearchAndKeyboard.KeyCollider>();
    keyCollider.key = key;
    keyCollider.baseScale = primitive.transform.localScale;
    if (parts1 != null && parts1.Count > 0)
      keyCollider.roundedHolder = parts1[0];
    SearchAndKeyboard._keyColliders[key] = keyCollider;
  }

  public static void HandleKeyPress(string key)
  {
    if ((UnityEngine.Object) SearchAndKeyboard.searchText == (UnityEngine.Object) null)
      return;
    SearchAndKeyboard.KeyCollider keyCollider;
    if (SearchAndKeyboard._keyColliders.TryGetValue(key, out keyCollider) && (UnityEngine.Object) keyCollider != (UnityEngine.Object) null)
      keyCollider.TriggerBounce();
    switch (key)
    {
      case "SPACE":
        SearchAndKeyboard.inputText += " ";
        break;
      case "BACK":
        if (SearchAndKeyboard.inputText.Length > 0)
        {
          string inputText = SearchAndKeyboard.inputText;
          SearchAndKeyboard.inputText = inputText.Substring(0, inputText.Length - 1);
          break;
        }
        break;
      case "ENTER":
        if (SearchAndKeyboard.isTyping)
        {
          SearchAndKeyboard.CloseTypingKeyboard();
          return;
        }
        SearchAndKeyboard.CloseKeyboard();
        return;
      default:
        SearchAndKeyboard.inputText += key;
        break;
    }
    Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(66, true, 0.625f);
    Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(66, false, 0.625f);
    SearchAndKeyboard.searchText.text = SearchAndKeyboard.inputText;
    Variables.currentCategoryPage = 0;
    if (!SearchAndKeyboard.isSearching || SearchAndKeyboard.isTyping)
      return;
    Main.RedrawButtonList();
  }

  private static void CreateKeyClicker(
    ref GameObject keyclickerObj,
    Transform parentTransform,
    ref Material clickerMaterial)
  {
    if ((UnityEngine.Object) keyclickerObj != (UnityEngine.Object) null)
      return;
    keyclickerObj = new GameObject("keyclicker");
    ((Collider) keyclickerObj.AddComponent<BoxCollider>()).isTrigger = true;
    keyclickerObj.layer = LayerMask.NameToLayer("UI");
    keyclickerObj.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
    clickerMaterial = new Material(Variables.uberShader)
    {
      color = Color.white
    };
    keyclickerObj.AddComponent<MeshRenderer>().material = clickerMaterial;
    if (!((UnityEngine.Object) parentTransform != (UnityEngine.Object) null))
      return;
    keyclickerObj.transform.SetParent(parentTransform);
    keyclickerObj.transform.localScale = new Vector3(0.0035f, 0.0035f, 0.0035f);
    keyclickerObj.transform.localPosition = new Vector3(0.0f, -0.1f, 0.0f);
  }

  public static void KeyClicker1(Transform parentTransform)
  {
    SearchAndKeyboard.CreateKeyClicker(ref Variables.keyclickerObj1, parentTransform, ref SearchAndKeyboard._clickerMaterial1);
  }

  public static void KeyClicker2(Transform parentTransform)
  {
    SearchAndKeyboard.CreateKeyClicker(ref Variables.keyclickerObj2, parentTransform, ref SearchAndKeyboard._clickerMaterial2);
  }

  private static Mesh KeyboardRoundObj(Vector3 localScale, float r, int seg = 5)
  {
    float a1 = localScale.x * 0.5f;
    float b = localScale.y * 0.5f;
    float z = localScale.z * 0.5f;
    r = Mathf.Clamp(r, 0.0001f, Mathf.Min(a1, b) * 0.9f);
    float num1 = a1 - r;
    float num2 = b - r;
    List<Vector2> vector2List = new List<Vector2>((seg + 1) * 4);
    float[] numArray1 = new float[4]
    {
      num1,
      -num1,
      -num1,
      num1
    };
    float[] numArray2 = new float[4]
    {
      num2,
      num2,
      -num2,
      -num2
    };
    for (int index1 = 0; index1 < 4; ++index1)
    {
      float a2 = (float) ((double) index1 * 3.1415927410125732 * 0.5);
      float num3 = numArray1[index1];
      float num4 = numArray2[index1];
      for (int index2 = 0; index2 <= seg; ++index2)
      {
        float f = Mathf.Lerp(a2, a2 + 1.57079637f, (float) index2 / (float) seg);
        vector2List.Add(new Vector2(num3 + Mathf.Cos(f) * r, num4 + Mathf.Sin(f) * r));
      }
    }
    int count1 = vector2List.Count;
    List<Vector3> inVertices = new List<Vector3>(count1 * 4 + 4);
    List<Vector3> inNormals = new List<Vector3>(count1 * 4 + 4);
    List<int> triangles = new List<int>(count1 * 12);
    int count2 = inVertices.Count;
    inVertices.Add(new Vector3(0.0f, 0.0f, z));
    inNormals.Add(Vector3.forward);
    int count3 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      inVertices.Add(new Vector3(vector2List[index].x, vector2List[index].y, z));
      inNormals.Add(Vector3.forward);
    }
    for (int index = 0; index < count1; ++index)
    {
      triangles.Add(count2);
      triangles.Add(count3 + index);
      triangles.Add(count3 + (index + 1) % count1);
    }
    int count4 = inVertices.Count;
    inVertices.Add(new Vector3(0.0f, 0.0f, -z));
    inNormals.Add(Vector3.back);
    int count5 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      inVertices.Add(new Vector3(vector2List[index].x, vector2List[index].y, -z));
      inNormals.Add(Vector3.back);
    }
    for (int index = 0; index < count1; ++index)
    {
      triangles.Add(count4);
      triangles.Add(count5 + (index + 1) % count1);
      triangles.Add(count5 + index);
    }
    int count6 = inVertices.Count;
    for (int index = 0; index < count1; ++index)
    {
      Vector3 normalized = new Vector3(vector2List[index].x, vector2List[index].y, 0.0f).normalized;
      inVertices.Add(new Vector3(vector2List[index].x, vector2List[index].y, z));
      inNormals.Add(normalized);
      inVertices.Add(new Vector3(vector2List[index].x, vector2List[index].y, -z));
      inNormals.Add(normalized);
    }
    for (int index = 0; index < count1; ++index)
    {
      int num5 = (index + 1) % count1;
      int num6 = count6 + index * 2;
      int num7 = num6 + 1;
      int num8 = count6 + num5 * 2;
      int num9 = num8 + 1;
      triangles.Add(num6);
      triangles.Add(num7);
      triangles.Add(num8);
      triangles.Add(num8);
      triangles.Add(num7);
      triangles.Add(num9);
    }
    int count7 = triangles.Count;
    for (int index = 0; index < count7; index += 3)
    {
      triangles.Add(triangles[index]);
      triangles.Add(triangles[index + 2]);
      triangles.Add(triangles[index + 1]);
    }
    Mesh mesh1 = new Mesh();
    mesh1.name = "KeyboardRoundedMesh";
    Mesh mesh2 = mesh1;
    mesh2.SetVertices(inVertices);
    mesh2.SetNormals(inNormals);
    mesh2.SetTriangles(triangles, 0);
    mesh2.RecalculateBounds();
    return mesh2;
  }

  public class KeyCollider : MonoBehaviour
  {
    public string key;
    public Vector3 baseScale;
    public GameObject roundedHolder;
    private Coroutine _bounceRoutine;
    private static float _kcpsCooldown;

    private void OnTriggerEnter(Collider collider)
    {
      if (((Component) collider).gameObject.name != "keyclicker" || (double) Time.frameCount < (double) SearchAndKeyboard.KeyCollider._kcpsCooldown + 12.5)
        return;
      SearchAndKeyboard.KeyCollider._kcpsCooldown = (float) Time.frameCount;
      if ((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null)
        Variables.Vibrate((UnityEngine.Object) ((Component) collider).gameObject == (UnityEngine.Object) Variables.keyclickerObj2, Variables.taggerInstance.tagHapticStrength / 2f, Variables.taggerInstance.tagHapticDuration / 2f);
      SearchAndKeyboard.HandleKeyPress(this.key);
    }

    public void TriggerBounce()
    {
      if (!this.gameObject.activeInHierarchy)
        return;
      if (this._bounceRoutine != null)
        this.StopCoroutine(this._bounceRoutine);
      this._bounceRoutine = this.StartCoroutine(this.BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
      Transform target = (UnityEngine.Object) this.roundedHolder != (UnityEngine.Object) null ? this.roundedHolder.transform : this.transform;
      Vector3 targetBase = (UnityEngine.Object) this.roundedHolder != (UnityEngine.Object) null ? Vector3.one : this.baseScale;
      Vector3 punched = new Vector3(targetBase.x * 1.25f, targetBase.y * 1.25f, targetBase.z * 0.7f);
      float elapsed = 0.0f;
      float halfDur = 0.063f;
      while ((double) elapsed < (double) halfDur)
      {
        if ((UnityEngine.Object) this == (UnityEngine.Object) null)
          yield break;
        float t = elapsed / halfDur;
        Vector3 scale = Vector3.LerpUnclamped(targetBase, punched, 1f - Mathf.Pow(1f - t, 3f));
        target.localScale = scale;
        if ((UnityEngine.Object) this.roundedHolder != (UnityEngine.Object) null)
          this.transform.localScale = new Vector3(scale.x * this.baseScale.x, scale.y * this.baseScale.y, scale.z * this.baseScale.z);
        elapsed += Time.deltaTime;
        yield return (object) null;
      }
      elapsed = 0.0f;
      float restDur = 0.18f - halfDur;
      while ((double) elapsed < (double) restDur)
      {
        if ((UnityEngine.Object) this == (UnityEngine.Object) null)
          yield break;
        float t = elapsed / restDur;
        Vector3 scale = Vector3.LerpUnclamped(punched, targetBase, SearchAndKeyboard.KeyCollider.ElasticOut(t));
        target.localScale = scale;
        if ((UnityEngine.Object) this.roundedHolder != (UnityEngine.Object) null)
          this.transform.localScale = new Vector3(scale.x * this.baseScale.x, scale.y * this.baseScale.y, scale.z * this.baseScale.z);
        elapsed += Time.deltaTime;
        yield return (object) null;
      }
      if ((UnityEngine.Object) this != (UnityEngine.Object) null)
      {
        target.localScale = targetBase;
        this.transform.localScale = this.baseScale;
      }
      this._bounceRoutine = (Coroutine) null;
    }

    private static float ElasticOut(float t)
    {
      if ((double) t <= 0.0)
        return 0.0f;
      return (double) t >= 1.0 ? 1f : (float) ((double) Mathf.Pow(2f, -10f * t) * (double) Mathf.Sin((float) (((double) t - 0.10000000149011612) * 6.2831854820251465 / 0.40000000596046448)) + 1.0);
    }
  }
}
