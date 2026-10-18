
using NXO.Menu;
using NXO.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
namespace NXO.Utilities;

internal class NotificationLib : MonoBehaviour
{
  private static readonly Dictionary<string, float> _notificationTimestamps = new Dictionary<string, float>();
  private GameObject _hudObj;
  private GameObject _hudObj2;
  private GameObject _mainCamera;
  private Text _notificationText;
  private Material _notificationMaterial;
  private readonly List<GameObject> _trackedObjects = new List<GameObject>();
  private bool _hasInitialized;
  private float _fadeAlpha = 1f;
  private bool _isFading;
  private const float NOTIFICATION_DELAY = 3f;
  private const float FADE_DURATION = 0.5f;
  private Sprite _barSprite;
  private Material _arrayMat;
  private GameObject _arrayRoot;
  private readonly List<NotificationLib.ArrayItem> _arrayItems = new List<NotificationLib.ArrayItem>(32 /*0x20*/);
  private readonly Dictionary<string, NotificationLib.ArrayItem> _arrayByName = new Dictionary<string, NotificationLib.ArrayItem>(32 /*0x20*/);
  private readonly HashSet<string> _currentEnabledSet = new HashSet<string>(32 /*0x20*/);
  private int _lastArrayVersion = int.MinValue;
  private bool _arraySortDirty;
  private const float ArrayAnimDur = 0.4f;
  private const float ArrayRowStep = 0.125f;
  private const float ArraySlideOut = 0.15f;
  private const float ArrayBounce = 0.03f;
  private const float ArrayPadX = 8f;
  private const float ArrayPadY = 3f;
  private const float ArrayForward = 2.2f;
  private const float ArrayRight = -1.35f;
  private const float ArrayUp = 0.55f;
  public static bool inRoom;
  public static bool RoomNotifications = true;
  public static bool ArrayListEnabled;
  private static readonly Dictionary<NotificationLib.NotificationType, string> _typeColors = new Dictionary<NotificationLib.NotificationType, string>()
  {
    {
      NotificationLib.NotificationType.Enabled,
      "#00FF00"
    },
    {
      NotificationLib.NotificationType.Disabled,
      "#FF4040"
    },
    {
      NotificationLib.NotificationType.Saved,
      "#00AAFF"
    },
    {
      NotificationLib.NotificationType.Loaded,
      "#00FFFF"
    },
    {
      NotificationLib.NotificationType.Deleted,
      "#FF8C00"
    },
    {
      NotificationLib.NotificationType.Room,
      "#C040FF"
    },
    {
      NotificationLib.NotificationType.Error,
      "#FF0000"
    },
    {
      NotificationLib.NotificationType.Alert,
      "#FFD700"
    },
    {
      NotificationLib.NotificationType.Info,
      "#B0B0B0"
    }
  };

  public static string PreviousNotification { get; private set; }

  public static bool IsEnabled { get; set; } = true;

  public static NotificationLib Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) NotificationLib.Instance != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    else
    {
      NotificationLib.Instance = this;
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
    }
  }

  private void Update()
  {
    this.UpdateNotifications();
    this.UpdateArrayList();
  }

  public void Init()
  {
    if (this._hasInitialized)
      return;
    this._mainCamera = GameObject.Find("Main Camera");
    if ((UnityEngine.Object) this._mainCamera == (UnityEngine.Object) null)
      return;
    this._hudObj2 = this.CreateAndTrackHUDObject("HUD_Notification_Parent");
    this._hudObj2.transform.position = this._mainCamera.transform.position + new Vector3(-1.5f, 0.0f, -4.5f);
    this._hudObj = this.CreateAndTrackHUDObject("HUD_Notification", this._hudObj2.transform);
    Canvas canvas1 = this._hudObj.AddComponent<Canvas>();
    canvas1.renderMode = (RenderMode) 2;
    canvas1.worldCamera = this._mainCamera.GetComponent<Camera>();
    this._hudObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
    this._hudObj.AddComponent<GraphicRaycaster>();
    RectTransform component = this._hudObj.GetComponent<RectTransform>();
    component.sizeDelta = new Vector2(5f, 5f);
    component.localScale = Vector3.one;
    component.localPosition = new Vector3(0.0f, 0.0f, 1.6f);
    component.rotation = Quaternion.Euler(0.0f, -250f, 0.0f);
    this._notificationText = this.CreateTextElement("NotificationText", this._hudObj, new Vector3(-1.2f, -0.75f, 0.0f), new Vector2(300f, 70f), 7);
    this._notificationText.font = Main.CurrentFont;
    this._notificationText.fontStyle = (FontStyle) 1;
    this._notificationText.alignment = (TextAnchor) 6;
    this._notificationMaterial = new Material(Shader.Find("GUI/Text Shader"));
    ((Graphic) this._notificationText).material = this._notificationMaterial;
    this._barSprite = NotificationLib.CreateRoundedSprite(32 /*0x20*/, 12);
    this._arrayMat = new Material(Shader.Find("GUI/Text Shader"));
    this._arrayRoot = new GameObject("NXO_ArrayList");
    Canvas canvas2 = this._arrayRoot.AddComponent<Canvas>();
    canvas2.renderMode = (RenderMode) 2;
    canvas2.worldCamera = this._mainCamera.GetComponent<Camera>();
    this._arrayRoot.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
    this._arrayRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 5f);
    this._trackedObjects.Add(this._arrayRoot);
    this._hasInitialized = true;
  }

  private GameObject CreateAndTrackHUDObject(string name, Transform parent = null)
  {
    GameObject andTrackHudObject = new GameObject(name);
    if ((UnityEngine.Object) parent != (UnityEngine.Object) null)
      andTrackHudObject.transform.parent = parent;
    this._trackedObjects.Add(andTrackHudObject);
    return andTrackHudObject;
  }

  private Text CreateTextElement(
    string name,
    GameObject parent,
    Vector3 position,
    Vector2 size,
    int fontSize)
  {
    GameObject gameObject = new GameObject(name);
    gameObject.transform.parent = parent.transform;
    Text textElement = gameObject.AddComponent<Text>();
    textElement.fontSize = fontSize;
    textElement.alignment = (TextAnchor) 4;
    ((Graphic) textElement).rectTransform.sizeDelta = size;
    ((Graphic) textElement).rectTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
    ((Graphic) textElement).rectTransform.localPosition = position;
    this._trackedObjects.Add(gameObject);
    return textElement;
  }

  public void UpdateNotifications()
  {
    if (!this._hasInitialized)
      this.Init();
    if ((UnityEngine.Object) this._hudObj2 != (UnityEngine.Object) null && (UnityEngine.Object) this._mainCamera != (UnityEngine.Object) null)
      this._hudObj2.transform.SetPositionAndRotation(this._mainCamera.transform.position, this._mainCamera.transform.rotation);
    this.ProcessExpiredNotifications();
  }

  public void UpdateArrayList()
  {
    if (!this._hasInitialized)
      this.Init();
    if ((UnityEngine.Object) this._arrayRoot == (UnityEngine.Object) null)
      return;
    if (!NotificationLib.ArrayListEnabled)
    {
      if (this._arrayItems.Count <= 0)
        return;
      this.DestroyAllArrayItems();
    }
    else
    {
      if ((UnityEngine.Object) this._mainCamera != (UnityEngine.Object) null && (UnityEngine.Object) this._arrayRoot != (UnityEngine.Object) null)
      {
        Transform transform = this._mainCamera.transform;
        this._arrayRoot.transform.position = transform.position + transform.forward * 2.2f + transform.right * -1.35f + transform.up * 0.55f;
        this._arrayRoot.transform.rotation = Quaternion.LookRotation(this._arrayRoot.transform.position - transform.position, transform.up);
      }
      float unscaledTime = Time.unscaledTime;
      if (this._lastArrayVersion != Main.ActiveModsVersion)
      {
        this._lastArrayVersion = Main.ActiveModsVersion;
        this._currentEnabledSet.Clear();
        ButtonHandler.Button[] buttons = ModButtons.buttons;
        for (int index = 0; index < buttons.Length; ++index)
        {
          if (buttons[index] != null && buttons[index].Enabled)
            this._currentEnabledSet.Add(buttons[index].buttonText);
        }
        foreach (string currentEnabled in this._currentEnabledSet)
        {
          NotificationLib.ArrayItem arrayItem1;
          if (this._arrayByName.TryGetValue(currentEnabled, out arrayItem1))
          {
            if ((double) arrayItem1.removeTime >= 0.0)
            {
              arrayItem1.removeTime = -1f;
              arrayItem1.entryTime = unscaledTime;
            }
          }
          else
          {
            NotificationLib.ArrayItem arrayItem2 = this.CreateArrayItem(currentEnabled, unscaledTime);
            this._arrayItems.Add(arrayItem2);
            this._arrayByName[currentEnabled] = arrayItem2;
            this._arraySortDirty = true;
          }
        }
        for (int index = 0; index < this._arrayItems.Count; ++index)
        {
          NotificationLib.ArrayItem arrayItem = this._arrayItems[index];
          if ((double) arrayItem.removeTime < 0.0 && !this._currentEnabledSet.Contains(arrayItem.mod))
            arrayItem.removeTime = unscaledTime;
        }
      }
      for (int index = this._arrayItems.Count - 1; index >= 0; --index)
      {
        NotificationLib.ArrayItem arrayItem = this._arrayItems[index];
        if ((double) arrayItem.removeTime >= 0.0 && (double) unscaledTime - (double) arrayItem.removeTime >= 0.40000000596046448)
        {
          UnityEngine.Object.Destroy((UnityEngine.Object) arrayItem.root);
          this._arrayByName.Remove(arrayItem.mod);
          this._arrayItems.RemoveAt(index);
        }
      }
      if (this._arraySortDirty)
      {
        this._arrayItems.Sort((Comparison<NotificationLib.ArrayItem>) ((a, b) => b.width.CompareTo(a.width)));
        this._arraySortDirty = false;
      }
      for (int index = 0; index < this._arrayItems.Count; ++index)
      {
        NotificationLib.ArrayItem arrayItem = this._arrayItems[index];
        float num1 = unscaledTime - arrayItem.entryTime;
        bool flag = (double) arrayItem.removeTime >= 0.0;
        float num2 = flag ? unscaledTime - arrayItem.removeTime : 999f;
        float x = 0.0f;
        float num3 = 1f;
        float a = 1f;
        if (flag && (double) num2 < 0.40000000596046448)
        {
          float num4 = num2 / 0.4f;
          x = num4 * 0.15f;
          a = 1f - num4;
        }
        else if ((double) num1 < 0.40000000596046448)
        {
          float num5 = num1 / 0.4f;
          float num6 = Mathf.Sin((float) ((double) num5 * 3.1415927410125732 * 2.5)) * (1f - num5);
          x = num6 * 0.03f;
          num3 = (float) (1.0 + (double) num6 * 0.15000000596046448);
        }
        arrayItem.rect.localPosition = new Vector3(x, (float) -index * 0.125f, 0.0f);
        arrayItem.rect.localScale = new Vector3(0.01f * num3, 0.01f * num3, 1f);
        Color color1 = NotificationLib.FlowColor((float) index * 0.15f);
        ((Graphic) arrayItem.text).color = new Color(color1.r, color1.g, color1.b, a);
        Color color2 = ((Graphic) arrayItem.bar).color with
        {
          a = 0.7f * a
        };
        ((Graphic) arrayItem.bar).color = color2;
      }
    }
  }

  private NotificationLib.ArrayItem CreateArrayItem(string mod, float now)
  {
    GameObject gameObject1 = new GameObject("ArrayItem", new Type[1]
    {
      typeof (RectTransform)
    });
    gameObject1.transform.SetParent(this._arrayRoot.transform, false);
    RectTransform transform = (RectTransform) gameObject1.transform;
    transform.localScale = new Vector3(0.01f, 0.01f, 1f);
    GameObject gameObject2 = new GameObject("Bar");
    gameObject2.transform.SetParent(gameObject1.transform, false);
    Image image = gameObject2.AddComponent<Image>();
    image.sprite = this._barSprite;
    ((Graphic) image).material = this._arrayMat;
    image.type = (Image.Type) 1;
    ((Graphic) image).color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
    RectTransform rectTransform1 = ((Graphic) image).rectTransform;
    RectTransform rectTransform2 = rectTransform1;
    RectTransform rectTransform3 = rectTransform1;
    Vector2 vector2_1 = new Vector2(0.5f, 0.5f);
    Vector2 vector2_2 = vector2_1;
    rectTransform3.anchorMax = vector2_2;
    Vector2 vector2_3 = vector2_1;
    rectTransform2.anchorMin = vector2_3;
    rectTransform1.pivot = new Vector2(0.0f, 0.5f);
    GameObject gameObject3 = new GameObject("Text");
    gameObject3.transform.SetParent(gameObject1.transform, false);
    Text text = gameObject3.AddComponent<Text>();
    text.font = Main.CurrentFont;
    text.fontStyle = (FontStyle) 1;
    text.fontSize = 7;
    text.alignment = (TextAnchor) 3;
    text.horizontalOverflow = (HorizontalWrapMode) 1;
    text.verticalOverflow = (VerticalWrapMode) 1;
    ((Graphic) text).material = this._arrayMat;
    text.text = mod;
    RectTransform rectTransform4 = ((Graphic) text).rectTransform;
    RectTransform rectTransform5 = rectTransform4;
    RectTransform rectTransform6 = rectTransform4;
    vector2_1 = new Vector2(0.5f, 0.5f);
    Vector2 vector2_4 = vector2_1;
    rectTransform6.anchorMax = vector2_4;
    Vector2 vector2_5 = vector2_1;
    rectTransform5.anchorMin = vector2_5;
    rectTransform4.pivot = new Vector2(0.0f, 0.5f);
    float preferredWidth = text.preferredWidth;
    float preferredHeight = text.preferredHeight;
    rectTransform4.sizeDelta = new Vector2(preferredWidth, preferredHeight);
    rectTransform4.anchoredPosition = new Vector2(8f, 0.0f);
    rectTransform1.sizeDelta = new Vector2(preferredWidth + 16f, preferredHeight + 6f);
    rectTransform1.anchoredPosition = Vector2.zero;
    return new NotificationLib.ArrayItem()
    {
      mod = mod,
      root = gameObject1,
      rect = transform,
      bar = image,
      text = text,
      width = preferredWidth,
      entryTime = now,
      removeTime = -1f
    };
  }

  private void DestroyAllArrayItems()
  {
    for (int index = 0; index < this._arrayItems.Count; ++index)
    {
      if ((UnityEngine.Object) this._arrayItems[index].root != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this._arrayItems[index].root);
    }
    this._arrayItems.Clear();
    this._arrayByName.Clear();
    this._lastArrayVersion = int.MinValue;
    this._arraySortDirty = false;
  }

  private static Sprite CreateRoundedSprite(int size, int radius)
  {
    Texture2D texture2D = new Texture2D(size, size, TextureFormat.RGBA32, false);
    texture2D.filterMode = FilterMode.Bilinear;
    texture2D.wrapMode = TextureWrapMode.Clamp;
    Texture2D texture = texture2D;
    Color32[] colors = new Color32[size * size];
    float num1 = (float) radius;
    float num2 = 1.25f;
    for (int index1 = 0; index1 < size; ++index1)
    {
      for (int index2 = 0; index2 < size; ++index2)
      {
        float a = Mathf.Abs((float) ((double) index2 + 0.5 - (double) size * 0.5)) - ((float) size * 0.5f - num1);
        float num3 = Mathf.Abs((float) ((double) index1 + 0.5 - (double) size * 0.5)) - ((float) size * 0.5f - num1);
        float num4 = Mathf.Max(a, 0.0f);
        float num5 = Mathf.Max(num3, 0.0f);
        float num6 = Mathf.Clamp01((float) (0.5 - (double) (Mathf.Sqrt((float) ((double) num4 * (double) num4 + (double) num5 * (double) num5)) + Mathf.Min(Mathf.Max(a, num3), 0.0f) - num1) / (double) num2));
        colors[index1 * size + index2] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) ((double) num6 * (double) byte.MaxValue));
      }
    }
    texture.SetPixels32(colors);
    texture.Apply(false, true);
    return Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) size, (float) size), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect, new Vector4((float) radius, (float) radius, (float) radius, (float) radius));
  }

  private static Color FlowColor(float phase = 0.0f)
  {
    return Color.Lerp(Main.pinwheelColor1, Main.pinwheelColor2, Mathf.PingPong(Time.unscaledTime * 1.5f - phase, 1f));
  }

  public void ProcessExpiredNotifications()
  {
    if (NotificationLib._notificationTimestamps.Count == 0)
      return;
    float time = Time.time;
    float num1 = float.MaxValue;
    foreach (float num2 in NotificationLib._notificationTimestamps.Values)
    {
      if ((double) num2 < (double) num1)
        num1 = num2;
    }
    if ((double) (time - num1) >= 3.0)
    {
      if (this._isFading)
        return;
      this._isFading = true;
      this.StartCoroutine(this.FadeOutNotification());
    }
    else
    {
      this._fadeAlpha = 1f;
      this._isFading = false;
      this.UpdateTextAlpha();
    }
  }

  private IEnumerator FadeOutNotification()
  {
    float elapsed = 0.0f;
    while ((double) elapsed < 0.5)
    {
      elapsed += Time.deltaTime;
      this._fadeAlpha = Mathf.Lerp(1f, 0.0f, elapsed / 0.5f);
      this.UpdateTextAlpha();
      yield return (object) null;
    }
    this._fadeAlpha = 0.0f;
    this.UpdateTextAlpha();
    string oldestKey = (string) null;
    float oldestVal = float.MaxValue;
    foreach (KeyValuePair<string, float> notificationTimestamp in NotificationLib._notificationTimestamps)
    {
      KeyValuePair<string, float> kvp = notificationTimestamp;
      if ((double) kvp.Value < (double) oldestVal)
      {
        oldestVal = kvp.Value;
        oldestKey = kvp.Key;
      }
      kvp = new KeyValuePair<string, float>();
    }
    NotificationLib._notificationTimestamps.Remove(oldestKey);
    this.UpdateNotificationText();
    this._isFading = false;
    this._fadeAlpha = 1f;
  }

  public void UpdateTextAlpha()
  {
    if (!((UnityEngine.Object) this._notificationText != (UnityEngine.Object) null))
      return;
    ((Graphic) this._notificationText).color = ((Graphic) this._notificationText).color with
    {
      a = this._fadeAlpha
    };
  }

  public void UpdateNotificationText()
  {
    if (!((UnityEngine.Object) this._notificationText != (UnityEngine.Object) null))
      return;
    this._notificationText.text = string.Join(Environment.NewLine, (IEnumerable<string>) NotificationLib._notificationTimestamps.Keys);
  }

  public static void SendNotification(NotificationLib.NotificationType type, string content)
  {
    if (!Variables.toggleNotifications || !NotificationLib.IsEnabled || string.IsNullOrEmpty(content) || (UnityEngine.Object) NotificationLib.Instance?._notificationText == (UnityEngine.Object) null)
      return;
    string str;
    string key = $"<color={(NotificationLib._typeColors.TryGetValue(type, out str) ? (object) str : (object) "#FFFFFF")}>{type}</color> : {content}";
    if (key == NotificationLib.PreviousNotification)
      return;
    NotificationLib._notificationTimestamps[key] = Time.time;
    NotificationLib.PreviousNotification = key;
    NotificationLib.Instance.UpdateNotificationText();
    NotificationLib.Instance.StartCoroutine(NotificationLib.Instance.FadeInNotification());
  }

  private IEnumerator FadeInNotification()
  {
    if (!this._isFading)
    {
      float elapsed = 0.0f;
      while ((double) elapsed < 0.5)
      {
        elapsed += Time.deltaTime;
        this._fadeAlpha = Mathf.Lerp(0.0f, 1f, elapsed / 0.5f);
        this.UpdateTextAlpha();
        yield return (object) null;
      }
      this._fadeAlpha = 1f;
      this.UpdateTextAlpha();
    }
  }

  public static void ClearAllNotifications()
  {
    NotificationLib._notificationTimestamps.Clear();
    if (!((UnityEngine.Object) NotificationLib.Instance._notificationText != (UnityEngine.Object) null))
      return;
    NotificationLib.Instance.UpdateNotificationText();
  }

  private class ArrayItem
  {
    public string mod;
    public GameObject root;
    public RectTransform rect;
    public Image bar;
    public Text text;
    public float width;
    public float entryTime;
    public float removeTime;
  }

  public enum NotificationType
  {
    Enabled,
    Disabled,
    Saved,
    Loaded,
    Deleted,
    Room,
    Error,
    Alert,
    Info,
  }
}
