
using GorillaLocomotion;
using NXO.Menu;
using NXO.Mods;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
namespace NXO.Utilities;

public class Variables
{
  public static GameObject menuObj;
  public static GameObject background;
  public static GameObject canvasObj;
  public static GameObject clickerObj;
  public static GameObject BackToStartButton;
  public static GameObject keyclickerObj1;
  public static GameObject keyclickerObj2;
  public static GameObject searchButton;
  public static GameObject TopMenuButton;
  public static Text title;
  public static Text subtitle;
  private static Category _currentPage;
  public static int currentCategoryPage = 0;
  public static int ButtonsPerPage = 7;
  public static float ButtonSpacing = 0.09f;
  public static bool toggledisconnectButton = true;
  public static bool SearchFollow = true;
  public static bool rightHandedMenu = false;
  public static bool toggleNotifications = true;
  public static bool AutoSave;
  public static bool notificationSent = false;
  public static bool teamCheckedESP = false;
  public static bool SearchEnabled = false;
  public static bool PCMenuOpen = false;
  public static KeyCode PCMenuKey = KeyCode.LeftAlt;
  public static bool openMenu;
  public static bool menuOpen;
  public static bool InMenuCondition;
  public static bool InPcCondition;
  public static bool OpenAndCloseMenuSounds = true;
  public static bool NotificationSounds = true;
  public static bool MenuToggles = false;
  public static bool SecondaryMenuPosition = false;
  public static bool HapticFeedback = true;
  public static AudioClip menuOpenSound;
  public static AudioClip menuCloseSound;
  public static AudioClip notificationSound;
  public static string folderName;
  public static GTPlayer playerInstance;
  public static GorillaTagger taggerInstance;
  public static GameObject thirdPersonCamera;
  public static GameObject shoulderCamera;
  public static GameObject TransformCam;
  public static GameObject cm;
  public static bool didThirdPerson = false;
  public static float rpcCooldown = 0.0f;
  public static float lastClearTime = 0.0f;
  public static int fps;
  public static float lastFPSTime = 0.0f;
  public static string nxouri = "https://nxo.lol/";
  private static readonly Dictionary<Type, Array> cache = new Dictionary<Type, Array>();
  public static readonly Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();
  public static Shader guiShader = Shader.Find("GUI/Text Shader");
  public static Shader uberShader = Shader.Find("GorillaTag/UberShader");
  public static Shader uiShader = Shader.Find("UI/Default");
  public static Shader shinyShader = Shader.Find("Universal Render Pipeline/Lit");
  private static int? noInvisLayers;

  public static Category currentPage
  {
    get => Variables._currentPage;
    set
    {
      Variables._currentPage = value;
      Main._lastPageDrawn = Category.Home;
      Main._lastDrawModVersion = -1;
    }
  }

  public static void Vibrate(bool leftController, float strength, float duration)
  {
    if (!Variables.HapticFeedback)
      return;
    GorillaTagger instance = GorillaTagger.Instance;
    if (!((UnityEngine.Object) instance != (UnityEngine.Object) null))
      return;
    instance.StartVibration(leftController, strength, duration);
  }

  public static void VibrateMenuHand(float strength, float duration)
  {
    Variables.Vibrate(!Variables.rightHandedMenu, strength, duration);
  }

  public static void VibrateClickHand(float strength, float duration)
  {
    Variables.Vibrate(Variables.rightHandedMenu, strength, duration);
  }

  public static bool GetGamemode(string gamemodeName)
  {
    return GorillaGameManager.instance.GameModeName().IndexOf(gamemodeName, StringComparison.OrdinalIgnoreCase) >= 0;
  }

  public static bool IsMaster() => PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;

  public static string GetHTMode()
  {
    Room currentRoom = PhotonNetwork.CurrentRoom;
    return ((RoomInfo) currentRoom)?.CustomProperties == null || !((Dictionary<object, object>) ((RoomInfo) currentRoom).CustomProperties).ContainsKey((object) "gameMode") ? "ERROR" : ((RoomInfo) currentRoom).CustomProperties[(object) "gameMode"].ToString();
  }

  public static T[] GetAllType<T>(bool refresh = false) where T : UnityEngine.Object
  {
    Array allType;
    if (!refresh && Variables.cache.TryGetValue(typeof (T), out allType))
      return (T[]) allType;
    Variables.cache.Remove(typeof (T));
    T[] objectsOfType = UnityEngine.Object.FindObjectsOfType<T>(true);
    Variables.cache[typeof (T)] = (Array) objectsOfType;
    return objectsOfType;
  }

  public static GameObject FindObject(string find)
  {
    GameObject gameObject1;
    if (Variables.cachedObjects.TryGetValue(find, out gameObject1) && (UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
      return gameObject1;
    Variables.cachedObjects.Remove(find);
    GameObject gameObject2 = GameObject.Find(find);
    if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
      Variables.cachedObjects[find] = gameObject2;
    return gameObject2;
  }

  public static Color GetRainbowColor(float hue) => Color.HSVToRGB(hue % 1f, 1f, 1f);

  public static int NoInvisLayers()
  {
    int valueOrDefault = Variables.noInvisLayers.GetValueOrDefault();
    int num1;
    if (!Variables.noInvisLayers.HasValue)
    {
      int num2 = ~(1 << LayerMask.NameToLayer("TransparentFX") | 1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Zone") | 1 << LayerMask.NameToLayer("Gorilla Trigger") | 1 << LayerMask.NameToLayer("Gorilla Boundary") | 1 << LayerMask.NameToLayer("GorillaCosmetics") | 1 << LayerMask.NameToLayer("GorillaParticle"));
      Variables.noInvisLayers = new int?(num2);
      num1 = num2;
    }
    else
      num1 = valueOrDefault;
    return num1;
  }

  public static Vector3 RandomVector3(float range = 1f) => UnityEngine.Random.insideUnitSphere * range;

  public static Quaternion RandomQuaternion() => UnityEngine.Random.rotationUniform;

  public static Color RandomColor()
  {
    return (Color) new Color32((byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), (byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), (byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), byte.MaxValue);
  }
}
