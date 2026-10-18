
using ExitGames.Client.Photon;
using GorillaNetworking;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Safety : MonoBehaviourPunCallbacks
{
  private static bool isOn = false;
  private static bool wasButtonPressed = false;
  private static float lagcooldown = 0.0f;
  private static float lagcooldownduration = 0.5f;
  private static GameObject radiusSphere = (GameObject) null;
  private static readonly List<ButtonHandler.Button> _panicSnapshot = new List<ButtonHandler.Button>();
  private static bool _panicked;
  private static bool _panicPressedLast;
  private const string PanicButtonText = "Panic (X)";
  private static bool spoofedPlatform = false;
  private static string[] Allowed = new string[10]
  {
    "one",
    "two",
    "three",
    "four",
    "five",
    "six",
    "seven",
    "eight",
    "nine",
    "zero"
  };
  private static float timer_;
  private static int Original_ = -1;
  private static float lastVol;
  private static float startSilenceTime = -1f;
  private static bool reloaded;
  public static readonly string[] namePrefix = new string[11]
  {
    "EPIC",
    "REAL",
    "NOT",
    "SILLY",
    "LITTLE",
    "BIG",
    "MAYBE",
    "SUB2",
    "OG",
    "FR",
    "NOT"
  };
  public static readonly string[] nameSuffix = new string[12]
  {
    "GT",
    "VR",
    "LOL",
    "FAN",
    "XD",
    "LOL",
    "MONKE",
    "YT",
    "NOT",
    "FR",
    "LMAO",
    "GTAG"
  };
  public static readonly string[] names = new string[66]
  {
    "0",
    "PBBV",
    "J3VU",
    "BEES",
    "NEMO",
    "LEMMING",
    "BILLY",
    "TIMMY",
    "MINIGAMES",
    "JMANCURLY",
    "VMT",
    "ELLIOT",
    "DAISY09",
    "MONK",
    "MONKE",
    "MONKI",
    "MONKEY",
    "MONKIY",
    "GORILL",
    "GOORILA",
    "GORILLA",
    "TTT",
    "TTTPIG",
    "PPPTIG",
    "K9",
    "BANANA",
    "PEANUTBUTTER",
    "GHOSTMONKE",
    "STATUE",
    "NOVA",
    "LUNAR",
    "MOON",
    "SUN",
    "RANDOM",
    "UNKNOWN",
    "GLITCH",
    "BUG",
    "ERROR",
    "CODE",
    "HACKER",
    "MODDER",
    "INVIS",
    "INVISIBLE",
    "TAGGER",
    "UNTAGGED",
    "BLUE",
    "RED",
    "GREEN",
    "PURPLE",
    "YELLOW",
    "BLACK",
    "WHITE",
    "BROWN",
    "CYAN",
    "GRAY",
    "GREY",
    "BANNED",
    "LEMON",
    "PLUSHIE",
    "CHEETO",
    "TIKTOK",
    "YOUTUBE",
    "TWITCH",
    "DISCORD",
    "MODDER",
    "HACKER"
  };
  private static bool previouslyInLobby;
  private static readonly List<VRRig> nameSpoofRigs = new List<VRRig>();
  private static readonly List<VRRig> colorSpoofRigs = new List<VRRig>();
  public static int targetElo = 4000;
  public static int targetBadge = 7;

  public static void Panic()
  {
    bool flag = InputHandler.LPrimary();
    if (flag && !Safety._panicPressedLast)
    {
      if (Safety._panicked)
        Safety.PanicRestore();
      else
        Safety.PanicDisable();
    }
    Safety._panicPressedLast = flag;
  }

  public static void SpoofPlatform(bool enable)
  {
    if (enable == Safety.spoofedPlatform)
      return;
    Hashtable hashtable = new Hashtable();
    ((Dictionary<object, object>) hashtable).Add((object) "platform", enable ? (object) "Quest" : (object) "Steam");
    PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, (Hashtable) null, (WebFlags) null);
    Safety.spoofedPlatform = enable;
  }

  private static void PanicDisable()
  {
    NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Panic Enabled, Features Disabled.");
    Safety._panicSnapshot.Clear();
    foreach (ButtonHandler.Button button in ModButtons.buttons)
    {
      if (button != null && button.Enabled && !(button.buttonText == "Panic (X)"))
      {
        Safety._panicSnapshot.Add(button);
        button.Enabled = false;
        Action onDisable = button.onDisable;
        if (onDisable != null)
          onDisable();
        NXOUI.RemoveMod(button.buttonText);
      }
    }
    Safety._panicked = true;
    Main.RefreshMenu();
  }

  private static void PanicRestore()
  {
    foreach (ButtonHandler.Button button in Safety._panicSnapshot)
    {
      if (button != null)
      {
        button.Enabled = true;
        Action onEnable = button.onEnable;
        if (onEnable != null)
          onEnable();
        NXOUI.AddMod(button.buttonText);
      }
    }
    Safety._panicSnapshot.Clear();
    Safety._panicked = false;
    NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Panic Disabled, Features Restored.");
    Main.RefreshMenu();
  }

  public static void PanicReset()
  {
    if (!Safety._panicked)
      return;
    Safety.PanicRestore();
  }

  public static void AntiReport(bool autoQueue, bool reconnect)
  {
    if (!PhotonNetwork.InRoom)
      return;
    foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
    {
      if (allScoreboardLine.linePlayer == NetworkSystem.Instance.LocalPlayer && !((UnityEngine.Object) allScoreboardLine.reportButton == (UnityEngine.Object) null))
      {
        Transform transform = ((Component) allScoreboardLine.reportButton).gameObject.transform;
        float antiReportRadius = Settings.AntiReportRadius;
        foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
        {
          if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
          {
            float num1 = Vector3.Distance(activeRig.rightHandTransform.position, transform.position);
            float num2 = Vector3.Distance(activeRig.leftHandTransform.position, transform.position);
            if ((double) num1 < (double) antiReportRadius || (double) num2 < (double) antiReportRadius)
            {
              NetworkSystem.Instance.ReturnToSinglePlayer();
              NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, $"`{activeRig.Creator.NickName}` Attempted to Report You!");
              if (autoQueue)
              {
                CoroutineHelper.InvokeAfterDelay(2f, new Action(Room.JoinRandomPublic));
                return;
              }
              if (!reconnect)
                return;
              CoroutineHelper.InvokeAfterDelay(2f, new Action(Room.Reconnect));
              return;
            }
          }
        }
      }
    }
  }

  public static void VisualizeAntiReport()
  {
    if (!PhotonNetwork.InRoom)
    {
      if (!((UnityEngine.Object) Safety.radiusSphere != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) Safety.radiusSphere);
      Safety.radiusSphere = (GameObject) null;
    }
    else
    {
      foreach (GorillaPlayerScoreboardLine allScoreboardLine in GorillaScoreboardTotalUpdater.allScoreboardLines)
      {
        if (allScoreboardLine.linePlayer == NetworkSystem.Instance.LocalPlayer && !((UnityEngine.Object) allScoreboardLine.reportButton == (UnityEngine.Object) null))
        {
          Transform transform = ((Component) allScoreboardLine.reportButton).gameObject.transform;
          if ((UnityEngine.Object) Safety.radiusSphere == (UnityEngine.Object) null)
          {
            Safety.radiusSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.Destroy((UnityEngine.Object) Safety.radiusSphere.GetComponent<Collider>());
            Renderer component = Safety.radiusSphere.GetComponent<Renderer>();
            component.material.shader = Shader.Find("GUI/Text Shader");
            component.material.color = new Color(1f, 0.0f, 0.0f, 0.3f);
          }
          Safety.radiusSphere.transform.position = transform.position;
          Safety.radiusSphere.transform.localScale = Vector3.one * Settings.AntiReportRadius;
          break;
        }
      }
    }
  }

  public static void DisableVisualizeAntiReport()
  {
    if (!((UnityEngine.Object) Safety.radiusSphere != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) Safety.radiusSphere);
    Safety.radiusSphere = (GameObject) null;
  }

  public static void RPCShield()
  {
    if (!PhotonNetwork.InRoom)
      return;
    try
    {
      MonkeAgent.instance.rpcErrorMax = int.MaxValue;
      MonkeAgent.instance.rpcCallLimit = int.MaxValue;
      MonkeAgent.instance.logErrorMax = int.MaxValue;
      MonkeAgent.instance.userRPCCalls.Clear();
      PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
      PhotonNetwork.QuickResends = int.MaxValue;
      PhotonNetwork.SendAllOutgoingCommands();
    }
    catch
    {
    }
  }

  public static void AntiModerator()
  {
    if (!PhotonNetwork.InRoom)
      return;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isOfflineVRRig && activeRig.HasCosmetic("LBAAK"))
      {
        NetworkSystem.Instance.ReturnToSinglePlayer();
        NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, $"`{activeRig.Creator.NickName}` Is A Moderator");
        CoroutineHelper.InvokeAfterDelay(1.5f, new Action(Room.JoinRandomPublic));
        break;
      }
    }
  }

  public static void ChangeIDENTITY()
  {
    float num1 = UnityEngine.Random.value;
    float num2 = UnityEngine.Random.value;
    float num3 = UnityEngine.Random.value;
    ((Behaviour) GorillaTagger.Instance.offlineVRRig).enabled = false;
    PlayerPrefs.SetFloat("redValue", num1);
    PlayerPrefs.SetFloat("greenValue", num3);
    PlayerPrefs.SetFloat("blueValue", num2);
    PlayerPrefs.Save();
    ((Component) GorillaTagger.Instance.offlineVRRig).transform.position = ((Component) GorillaComputer.instance).gameObject.transform.position;
    GorillaComputer.instance.ProcessColorState((GorillaKeyboardBindings) 13);
    GorillaComputer.instance.UpdateScreen();
    GorillaComputer.instance.redValue = num1;
    GorillaComputer.instance.greenValue = num2;
    GorillaComputer.instance.blueValue = num3;
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 13);
    GorillaComputer.instance.UpdateColor(num1, num2, num3);
    GorillaComputer.instance.NameScreen();
    while (GorillaComputer.instance.currentState != (GorillaComputer.ComputerState) 2)
      GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 11);
    while (GorillaComputer.instance.currentName != "")
    {
      for (int index = 0; index < GorillaComputer.instance.currentName.Count<char>(); ++index)
        GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 12);
    }
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 23);
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 31 /*0x1F*/);
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 34);
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 28);
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 28);
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 17);
    for (int index = 0; index < 5; ++index)
      GorillaComputer.instance.PressButton((GorillaKeyboardBindings) Enum.Parse(typeof (GorillaKeyboardBindings), Safety.Allowed[UnityEngine.Random.Range(0, ((IEnumerable<string>) Safety.Allowed).Count<string>())]));
    GorillaComputer.instance.PressButton((GorillaKeyboardBindings) 13);
    NetworkSystem.Instance.SetMyNickName($"GORILLA{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}{UnityEngine.Random.Range(1, 10)}");
    CustomMapsTerminal.RequestDriverNickNameRefresh();
    PlayerPrefs.SetString("playerName", GorillaComputer.instance.currentName);
    PlayerPrefs.Save();
    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget) 0, new object[3]
    {
      (object) num1,
      (object) num2,
      (object) num3
    });
    ((Behaviour) GorillaTagger.Instance.offlineVRRig).enabled = true;
  }

  public static void FakeLag()
  {
    bool flag = InputHandler.RPrimary();
    if (!Safety.wasButtonPressed & flag)
      Safety.isOn = !Safety.isOn;
    Safety.wasButtonPressed = flag;
    if (Safety.isOn && (double) Time.time > (double) Safety.lagcooldown)
    {
      Safety.lagcooldown = Time.time + Safety.lagcooldownduration;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = !((Behaviour) Variables.taggerInstance.offlineVRRig).enabled;
      Safety.lagcooldownduration = UnityEngine.Random.Range(0.1f, 0.4f);
    }
    else
    {
      if (Safety.isOn || !((UnityEngine.Object) Variables.taggerInstance.offlineVRRig != (UnityEngine.Object) null))
        return;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
  }

  public static void SpoofFps(bool enable)
  {
    if (enable)
    {
      if ((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GorillaTagger.Instance.offlineVRRig == (UnityEngine.Object) null)
        return;
      Safety.timer_ += Time.deltaTime;
      if ((double) Safety.timer_ < 0.013888888992369175)
        return;
      Safety.timer_ -= 0.013888889f;
      if (Safety.Original_ == -1)
        Safety.Original_ = VRRig.LocalRig.fps;
      VRRig.LocalRig.fps = 72;
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Behaviour) VRRig.LocalRig).enabled = true;
    }
    else
      VRRig.LocalRig.fps = Safety.Original_;
  }

  public static void BypassAutomod()
  {
    if (!PhotonNetwork.InRoom)
      return;
    GorillaTagger.moderationMutedTime = -1f;
    if (GorillaComputer.instance.autoMuteType != "OFF")
    {
      GorillaComputer.instance.autoMuteType = "OFF";
      PlayerPrefs.SetInt("autoMute", 0);
      PlayerPrefs.Save();
    }
    Recorder primaryRecorder = NetworkSystem.Instance.VoiceConnection.PrimaryRecorder;
    if ((UnityEngine.Object) primaryRecorder == (UnityEngine.Object) null || primaryRecorder.SourceType == (Recorder.InputSourceType) 1)
      return;
    float num = 0.0f;
    GorillaSpeakerLoudness component = ((Component) VRRig.LocalRig).GetComponent<GorillaSpeakerLoudness>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      num = component.Loudness;
    if ((double) num == 0.0)
    {
      if ((double) Safety.lastVol != 0.0)
      {
        Safety.startSilenceTime = Time.time;
        Safety.reloaded = false;
      }
      if ((double) Safety.startSilenceTime > 0.0 && !Safety.reloaded && (double) Time.time - (double) Safety.startSilenceTime >= 0.25)
      {
        primaryRecorder.RestartRecording(true);
        Safety.reloaded = true;
      }
    }
    else
    {
      Safety.startSilenceTime = -1f;
      Safety.reloaded = false;
    }
    Safety.lastVol = num;
  }

  public static void BypassModCheckers()
  {
    Photon.Realtime.Player localPlayer = PhotonNetwork.LocalPlayer;
    if (localPlayer == null || localPlayer.CustomProperties == null || ((Dictionary<object, object>) localPlayer.CustomProperties).Count == 0)
      return;
    Hashtable hashtable = new Hashtable();
    foreach (string str in ((Dictionary<object, object>) localPlayer.CustomProperties).Keys.ToList<object>().Select<object, string>((Func<object, string>) (keyObj => keyObj?.ToString())).Where<string>((Func<string, bool>) (key => key != null)).Where<string>((Func<string, bool>) (key => !key.Equals("didTutorial"))))
      hashtable[(object) str] = (object) null;
    if (((Dictionary<object, object>) hashtable).Count <= 0)
      return;
    localPlayer.SetCustomProperties(hashtable, (Hashtable) null, (WebFlags) null);
  }

  public static void ChangeIdentity()
  {
    string PlayerName = "gorilla";
    for (int index = 0; index < 4; ++index)
      PlayerName += UnityEngine.Random.Range(0, 9).ToString();
    RigManager.ChangeName(PlayerName);
    RigManager.ChangeColor((Color) new Color32((byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), (byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), (byte) UnityEngine.Random.Range(0, (int) byte.MaxValue), byte.MaxValue));
  }

  public static void ChangeIdentityOnDisconnect(Action identityType)
  {
    if (!PhotonNetwork.InRoom && Safety.previouslyInLobby && identityType != null)
      identityType();
    Safety.previouslyInLobby = PhotonNetwork.InRoom;
  }

  public static void NameSpoof()
  {
    List<VRRig> vrRigList = new List<VRRig>();
    foreach (VRRig nameSpoofRig in Safety.nameSpoofRigs)
    {
      if (!VRRigCache.ActiveRigs.Contains<VRRig>(nameSpoofRig))
        vrRigList.Add(nameSpoofRig);
    }
    foreach (VRRig vrRig in vrRigList)
      Safety.nameSpoofRigs.Remove(vrRig);
    vrRigList.Clear();
    string nickName = PhotonNetwork.NickName;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isLocal && !Safety.nameSpoofRigs.Contains(activeRig))
      {
        string str1 = UnityEngine.Random.Range(0, 3) == 0 ? Safety.namePrefix[UnityEngine.Random.Range(0, Safety.namePrefix.Length)] : "";
        string str2 = UnityEngine.Random.Range(0, 3) == 0 ? Safety.nameSuffix[UnityEngine.Random.Range(0, Safety.nameSuffix.Length)] : "";
        string str3 = str1 + Safety.names[UnityEngine.Random.Range(0, Safety.names.Length)] + str2;
        RigManager.ChangeName(str3.Length > 12 ? str3.Substring(0, 12) : str3, true);
        GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RigManager.GetNetPlayerFromVRRig(activeRig), new object[3]
        {
          (object) UnityEngine.Random.Range(0.0f, 1f),
          (object) UnityEngine.Random.Range(0.0f, 1f),
          (object) UnityEngine.Random.Range(0.0f, 1f)
        });
        Safety.nameSpoofRigs.Add(activeRig);
      }
    }
    if (!(PhotonNetwork.NickName != nickName))
      return;
    PhotonNetwork.NickName = nickName;
  }

  public static void ColorSpoof()
  {
    List<VRRig> vrRigList = new List<VRRig>();
    foreach (VRRig colorSpoofRig in Safety.colorSpoofRigs)
    {
      if (!VRRigCache.ActiveRigs.Contains<VRRig>(colorSpoofRig))
        vrRigList.Add(colorSpoofRig);
    }
    foreach (VRRig vrRig in vrRigList)
      Safety.colorSpoofRigs.Remove(vrRig);
    vrRigList.Clear();
    foreach (VRRig vrrig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (rig => !rig.isLocal)).Where<VRRig>((Func<VRRig, bool>) (rig => !Safety.colorSpoofRigs.Contains(rig))))
    {
      GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RigManager.GetNetPlayerFromVRRig(vrrig), new object[3]
      {
        (object) UnityEngine.Random.Range(0.0f, 1f),
        (object) UnityEngine.Random.Range(0.0f, 1f),
        (object) UnityEngine.Random.Range(0.0f, 1f)
      });
      Safety.colorSpoofRigs.Add(vrrig);
    }
  }

  public static void SpoofRankedPlatform(bool enabled, string target = null)
  {
    MenuPatches.RankedPatch.enabled = enabled;
    MenuPatches.RankedPatch.targetPlatform = target;
  }

  public static void SpoofBadge(bool enable)
  {
    MenuPatches.RankedPatch.enabled = enable;
    if (!MenuPatches.RankedPatch.enabled || Mathf.Approximately(VRRig.LocalRig.currentRankedELO, (float) Safety.targetElo) && VRRig.LocalRig.currentRankedSubTierQuest == Safety.targetBadge && VRRig.LocalRig.currentRankedSubTierPC == Safety.targetBadge)
      return;
    VRRig.LocalRig.SetRankedInfo((float) Safety.targetElo, Safety.targetBadge, Safety.targetBadge, true);
  }
}
