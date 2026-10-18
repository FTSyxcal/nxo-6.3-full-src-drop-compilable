
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using PlayFab.SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

#nullable enable
namespace NXO.Utilities;

[HarmonyPatch]
public class MenuPatches
{
  public static bool handtapEnabled;
  public static bool tapsEnabled = true;
  public static bool doOverride;
  public static float overrideVolume = 99999f;
  public static float overrideSpeed = 99999f;
  public static int tapMultiplier = 1;
  public static bool forceEnabled;
  public static bool bypassPositionCheck;
  public static bool AntiCheatNotifications = false;
  public static bool kidEnabled;
  public static bool knockbackEnabled;
  public static bool micEnabled;
  public static bool snowballEnabled;
  public static bool autoBig;
  public static bool propertiesEnabled;
  public static bool tosEnabled;
  private static Material _primitiveMaterial;

  [HarmonyPatch(typeof (VRRig), "SetHandEffectData")]
  [HarmonyPrefix]
  private static bool HandDataEffect(
    VRRig __instance,
    HandEffectContext effectContext,
    int audioClipIndex,
    bool isDownTap,
    bool isLeftHand,
    StiltID stiltID,
    float handTapVolume,
    float handTapSpeed,
    Vector3 dirFromHitToHand)
  {
    if (!MenuPatches.handtapEnabled || !__instance.isLocal)
      return true;
    if (MenuPatches.doOverride)
    {
      GTPlayer.MaterialData handSurfaceData = VRRig.LocalRig.GetHandSurfaceData(audioClipIndex);
      effectContext.soundFX = handSurfaceData.audio;
      effectContext.speed = MenuPatches.overrideSpeed;
      effectContext.soundVolume = MenuPatches.overrideVolume;
      if (PhotonNetwork.InRoom && MenuPatches.tapMultiplier > 1)
      {
        for (int index = 0; index < MenuPatches.tapMultiplier; ++index)
          Variables.taggerInstance.myVRRig.SendRPC("RPC_PlayHandTap", (RpcTarget) 0, new object[3]
          {
            (object) audioClipIndex,
            (object) isLeftHand,
            (object) handTapSpeed
          });
        Safety.RPCShield();
      }
      return false;
    }
    if (MenuPatches.tapsEnabled)
      return true;
    effectContext.speed = 0.0f;
    effectContext.soundVolume = 0.0f;
    Variables.taggerInstance.handTapVolume = 0.0f;
    Variables.taggerInstance.handTapSpeed = 0.0f;
    Variables.taggerInstance.audioClipIndex = -1;
    return false;
  }

  [HarmonyPatch(typeof (CosmeticsController), "PressWardrobeItemButton", new Type[] {typeof (CosmeticsController.CosmeticItem), typeof (bool), typeof (bool)})]
  [HarmonyPrefix]
  public static void PressWardrobeItemButtonPatch(
    CosmeticsController __instance,
    CosmeticsController.CosmeticItem cosmeticItem)
  {
    try
    {
      if (!Fun.serverSidedArmed || (UnityEngine.Object) __instance == (UnityEngine.Object) null || cosmeticItem.isNullItem || string.IsNullOrEmpty(cosmeticItem.itemName) || !Fun.serverSidedItems.Contains(cosmeticItem.itemName) || __instance.IsCosmeticEquipped(cosmeticItem))
        return;
      Fun.ServerSideEquip();
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (ForceVolume), "OnTriggerEnter")]
  [HarmonyPrefix]
  public static bool OnTriggerEnterPatch() => !MenuPatches.forceEnabled;

  [HarmonyPatch(typeof (ForceVolume), "OnTriggerExit")]
  [HarmonyPrefix]
  public static bool OnTriggerExitPatch() => !MenuPatches.forceEnabled;

  [HarmonyPatch(typeof (ForceVolume), "OnTriggerStay")]
  [HarmonyPrefix]
  public static bool OnTriggerStayPatch() => !MenuPatches.forceEnabled;

  [HarmonyPatch(typeof (VRRig), "IsPositionInRange")]
  [HarmonyPostfix]
  public static void Postfix(VRRig __instance, ref bool __result, Vector3 position, float range)
  {
    NetPlayer netPlayer = RigManager.GetNetPlayerFromVRRig(__instance) ?? (NetPlayer) null;
    if ((!MenuPatches.bypassPositionCheck || !__instance.isLocal) && (netPlayer == null || netPlayer != NetworkSystem.Instance.LocalPlayer))
      return;
    __result = true;
  }

  [HarmonyPatch(typeof (MonkeAgent), "SendReport")]
  [HarmonyPrefix]
  private static bool SendReportPatch(string susReason, string susId, string susNick)
  {
    if (MenuPatches.AntiCheatNotifications && susId == PhotonNetwork.LocalPlayer.UserId)
      NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, "Anti-Cheat Reported For " + susReason);
    return false;
  }

  [HarmonyPatch(typeof (MonkeAgent), "CheckReports")]
  [HarmonyPrefix]
  private static bool CheckReportsPatch() => false;

  [HarmonyPatch(typeof (MonkeAgent), "LogErrorCount")]
  [HarmonyPrefix]
  private static bool LogErrorCountPatch(string logString, string stackTrace, LogType type)
  {
    return false;
  }

  [HarmonyPatch(typeof (MonkeAgent), "DispatchReport")]
  [HarmonyPrefix]
  private static bool DispatchReportPatch() => false;

  [HarmonyPatch(typeof (GorillaNetworkPublicTestsJoin), "GracePeriod")]
  [HarmonyPrefix]
  private static bool GracePeriod1Prefix() => false;

  [HarmonyPatch(typeof (GorillaNetworkPublicTestJoin2), "GracePeriod")]
  [HarmonyPrefix]
  private static bool GracePeriod2Prefix() => false;

  [HarmonyPatch(typeof (VRRig), "IncrementRPC", new Type[] {typeof (PhotonMessageInfoWrapped), typeof (string)})]
  [HarmonyPrefix]
  private static bool IncrementRPCPrefix(PhotonMessageInfoWrapped info, string sourceCall) => false;

  [HarmonyPatch(typeof (MonkeAgent), "IncrementRPCCall", new Type[] {typeof (PhotonMessageInfo), typeof (string)})]
  [HarmonyPrefix]
  private static bool IncrementRPCCallPrefix(PhotonMessageInfo info, string callingMethod = "")
  {
    return false;
  }

  [HarmonyPatch(typeof (MonkeAgent), "IncrementRPCCallLocal")]
  [HarmonyPrefix]
  private static bool IncrementRPCCallLocalPrefix(
    PhotonMessageInfoWrapped infoWrapped,
    string rpcFunction)
  {
    return false;
  }

  [HarmonyPatch(typeof (MonkeAgent), "GetRPCCallTracker")]
  [HarmonyPrefix]
  private static bool GetRPCCallTrackerPrefix() => false;

  [HarmonyPatch(typeof (KIDManager), "HasPermissionToUseFeature")]
  [HarmonyPostfix]
  public static void HasPermissionToUseFeature(ref bool __result)
  {
    if (!MenuPatches.kidEnabled)
      return;
    __result = true;
  }

  [HarmonyPatch(typeof (GTPlayer), "ApplyKnockback")]
  [HarmonyPrefix]
  public static bool ApplyKnockback(Vector3 direction, float speed)
  {
    return !MenuPatches.knockbackEnabled;
  }

  [HarmonyPatch(typeof (GorillaSpeakerLoudness), "UpdateLoudness")]
  [HarmonyPrefix]
  private static bool MicPrefix(GorillaSpeakerLoudness __instance)
  {
    return !MenuPatches.micEnabled || ((Component) __instance).gameObject.name != "Local Gorilla Player";
  }

  [HarmonyPatch(typeof (GrowingSnowballThrowable), "OnEnable")]
  [HarmonyPostfix]
  public static void SnowballPostfix(GrowingSnowballThrowable __instance)
  {
    if (MenuPatches.autoBig)
    {
      __instance.IncreaseSize(5);
    }
    else
    {
      if (!MenuPatches.snowballEnabled)
        return;
      __instance.IncreaseSize(Settings.snowballScale);
    }
  }

  [HarmonyPatch(typeof (PlayFabDeviceUtil), "SendDeviceInfoToPlayFab")]
  [HarmonyPrefix]
  private static bool SendDeviceInfoPrefix() => false;

  [HarmonyPatch(typeof (PlayFabClientInstanceAPI), "ReportDeviceInfo")]
  [HarmonyPrefix]
  private static bool ReportDeviceInfoInstancePrefix() => false;

  [HarmonyPatch(typeof (PlayFabClientAPI), "ReportDeviceInfo")]
  [HarmonyPrefix]
  private static bool ReportDeviceInfoPrefix() => false;

  [HarmonyPatch(typeof (PlayFabDeviceUtil), "GetAdvertIdFromUnity")]
  [HarmonyPrefix]
  private static bool GetAdvertIdPrefix() => false;

  [HarmonyPatch(typeof (PlayFabClientAPI), "AttributeInstall")]
  [HarmonyPrefix]
  private static bool AttributeInstallPrefix() => false;

  [HarmonyPatch(typeof (PlayFabHttp), "InitializeScreenTimeTracker")]
  [HarmonyPrefix]
  private static bool InitializeScreenTimeTrackerPrefix() => false;

  [HarmonyPatch]
  [HarmonyPrefix]
  private static bool ReportPlayerInstancePrefix(
    ReportPlayerClientRequest request,
    Action<ReportPlayerClientResult> resultCallback,
    Action<PlayFabError> errorCallback,
    object customData = null,
    Dictionary<string, string> extraHeaders = null)
  {
    return false;
  }

  [HarmonyPatch]
  [HarmonyPrefix]
  private static bool ReportPlayerPrefix(
    ReportPlayerClientRequest request,
    Action<ReportPlayerClientResult> resultCallback,
    Action<PlayFabError> errorCallback,
    object customData = null,
    Dictionary<string, string> extraHeaders = null)
  {
    return false;
  }

  [HarmonyPatch(typeof (Photon.Realtime.Player), "SetCustomProperties")]
  [HarmonyPrefix]
  public static bool SetCustomProperties(Photon.Realtime.Player __instance, ref Hashtable propertiesToSet)
  {
    return !__instance.IsLocal || !MenuPatches.propertiesEnabled || !((IEnumerable<KeyValuePair<object, object>>) propertiesToSet).Any<KeyValuePair<object, object>>((Func<KeyValuePair<object, object>, bool>) (prop => prop.Key.ToString() != "didTutorial"));
  }

  [HarmonyPatch(typeof (Photon.Realtime.Player), "set_CustomProperties")]
  [HarmonyPrefix]
  public static bool set_CustomProperties(Photon.Realtime.Player __instance, ref Hashtable value)
  {
    return !__instance.IsLocal || !MenuPatches.propertiesEnabled || !((IEnumerable<KeyValuePair<object, object>>) value).Any<KeyValuePair<object, object>>((Func<KeyValuePair<object, object>, bool>) (prop => prop.Key.ToString() != "didTutorial"));
  }

  [HarmonyPatch(typeof (VRRig), "OnDisable")]
  [HarmonyPrefix]
  private static bool OnDisablePrefix(VRRig __instance) => !__instance.isLocal;

  [HarmonyPatch(typeof (VRRig), "Awake")]
  [HarmonyPrefix]
  private static bool AwakePrefix(VRRig __instance)
  {
    return ((Component) __instance).gameObject.name != "Local Gorilla Player(Clone)";
  }

  [HarmonyPatch(typeof (VRRig), "PostTick")]
  [HarmonyPrefix]
  private static bool PostTickPrefix(VRRig __instance)
  {
    return !__instance.isLocal || ((Behaviour) __instance).enabled;
  }

  [HarmonyPatch(typeof (GTPlayer), "GetSlidePercentage")]
  [HarmonyPostfix]
  private static void GetSlidePercentagePostfix(ref float __result)
  {
    if (Movement.GrippySurfaces)
    {
      __result = 0.0f;
    }
    else
    {
      if (!Movement.SlipperySurfaces)
        return;
      __result = 1f;
    }
  }

  [HarmonyPatch(typeof (GorillaTelemetry), "IsConnected")]
  [HarmonyPrefix]
  private static bool IsConnectedPrefix() => false;

  [HarmonyPatch]
  [HarmonyPrefix]
  private static bool AntiTeleportTechnologyPrefix() => false;

  [HarmonyPatch(typeof (LegalAgreements), "Update")]
  [HarmonyPrefix]
  private static bool LegalAgreementsUpdate(LegalAgreements __instance)
  {
    if (!MenuPatches.tosEnabled)
      return true;
    ControllerInputPoller.instance.leftControllerPrimary2DAxis.y = -1f;
    __instance.scrollSpeed = 10f;
    __instance._maxScrollSpeed = 10f;
    return false;
  }

  [HarmonyPatch(typeof (ModIOTermsOfUse_v1), "PostUpdate")]
  [HarmonyPrefix]
  private static bool ModIOTermsPostUpdate(ModIOTermsOfUse_v1 __instance)
  {
    if (!MenuPatches.tosEnabled)
      return true;
    __instance.TurnPage(999);
    ControllerInputPoller.instance.leftControllerPrimary2DAxis.y = -1f;
    __instance.holdTime = 0.1f;
    return false;
  }

  [HarmonyPatch(typeof (AgeSlider), "PostUpdate")]
  [HarmonyPrefix]
  private static bool AgeSliderPostUpdate(AgeSlider __instance)
  {
    if (!MenuPatches.tosEnabled)
      return true;
    __instance._currentAge = 21;
    __instance.holdTime = 0.1f;
    return false;
  }

  [HarmonyPatch(typeof (PrivateUIRoom), "StartOverlay")]
  [HarmonyPrefix]
  private static bool PrivateUIRoomStartOverlay() => !MenuPatches.tosEnabled;

  [HarmonyPatch(typeof (KIDManager), "UseKID")]
  [HarmonyPrefix]
  private static bool KIDManagerUseKID(ref Task<bool> __result)
  {
    if (!MenuPatches.tosEnabled)
      return true;
    __result = Task.FromResult<bool>(false);
    return false;
  }

  [HarmonyPatch]
  [HarmonyPrefix]
  private static bool QuitDelayPrefix() => false;

  [HarmonyPatch(typeof (GorillaGameManager), "ForceStopGame_DisconnectAndDestroy")]
  [HarmonyPrefix]
  private static bool ForceStopGamePrefix() => false;

  [HarmonyPatch(typeof (MonkeAgent), "ShouldDisconnectFromRoom")]
  [HarmonyPrefix]
  private static bool ShouldDisconnectFromRoomPrefix() => false;

  [HarmonyPatch]
  [HarmonyPostfix]
  private static void CreatePrimitivePostfix(GameObject __result)
  {
    Renderer component = __result.GetComponent<Renderer>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) MenuPatches._primitiveMaterial == (UnityEngine.Object) null)
    {
      Material material = new Material(Variables.uberShader);
      material.color = Color.blue;
      material.hideFlags = HideFlags.HideAndDontSave;
      MenuPatches._primitiveMaterial = material;
    }
    component.sharedMaterial = MenuPatches._primitiveMaterial;
  }

  [HarmonyPatch(typeof (VRRig), "PostTick")]
  public class RigAnimationPatch
  {
    public static Action OnPostTick;

    [HarmonyPostfix]
    private static void Postfix(VRRig __instance)
    {
      if (!__instance.isLocal)
        return;
      Action onPostTick = MenuPatches.RigAnimationPatch.OnPostTick;
      if (onPostTick == null)
        return;
      onPostTick();
    }
  }

  [HarmonyPatch(typeof (Slingshot), "GetLaunchVelocity")]
  public class SlingshotAimbot
  {
    public static bool enabled;

    [HarmonyPostfix]
    public static void Postfix(Slingshot __instance, ref Vector3 __result)
    {
      if (!MenuPatches.SlingshotAimbot.enabled || (((TransferrableObject) __instance).InLeftHand() ? InputHandler.LTrigger() : InputHandler.RTrigger()))
        return;
      List<NetPlayer> excludedPlayers = new List<NetPlayer>();
      if (PhotonNetwork.InRoom && (UnityEngine.Object) GorillaGameManager.instance != (UnityEngine.Object) null)
      {
        switch (GorillaGameManager.instance.GameType() - 1)
        {
          case 0:
          case (GorillaGameModes.GameModeType) 4:
          case (GorillaGameModes.GameModeType) 8:
          case (GorillaGameModes.GameModeType) 9:
          case (GorillaGameModes.GameModeType) 10:
            GorillaTagManager instance1 = (GorillaTagManager) GorillaGameManager.instance;
            if (instance1.isCurrentlyTag)
            {
              excludedPlayers.Add(instance1.currentIt);
              break;
            }
            excludedPlayers.AddRange((IEnumerable<NetPlayer>) instance1.currentInfected);
            break;
          case (GorillaGameModes.GameModeType) 2:
            GorillaPaintbrawlManager instance2 = (GorillaPaintbrawlManager) GorillaGameManager.instance;
            excludedPlayers.AddRange(instance2.playerLives.Where<KeyValuePair<int, int>>((Func<KeyValuePair<int, int>, bool>) (kv => kv.Value <= 0)).Select<KeyValuePair<int, int>, Photon.Realtime.Player>((Func<KeyValuePair<int, int>, Photon.Realtime.Player>) (kv => PhotonNetwork.NetworkingClient.CurrentRoom.GetPlayer(kv.Key, false))).Where<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (p => p != null)).Select<Photon.Realtime.Player, NetPlayer>((Func<Photon.Realtime.Player, NetPlayer>) (p => ((NetPlayer) (p)))));
            if (!excludedPlayers.Contains(NetworkSystem.Instance.LocalPlayer))
            {
              excludedPlayers.Add(NetworkSystem.Instance.LocalPlayer);
              break;
            }
            break;
          case (GorillaGameModes.GameModeType) 3:
          case (GorillaGameModes.GameModeType) 5:
            GorillaAmbushManager instance3 = (GorillaAmbushManager) GorillaGameManager.instance;
            if (((GorillaTagManager) instance3).isCurrentlyTag)
            {
              excludedPlayers.Add(((GorillaTagManager) instance3).currentIt);
              break;
            }
            excludedPlayers.AddRange((IEnumerable<NetPlayer>) ((GorillaTagManager) instance3).currentInfected);
            break;
        }
      }
      Transform head = ((Component) Variables.taggerInstance.headCollider).transform;
      VRRig vrRig = VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (rig => !rig.isLocal && (UnityEngine.Object) rig != (UnityEngine.Object) null)).Where<VRRig>((Func<VRRig, bool>) (rig => !excludedPlayers.Contains(RigManager.GetNetPlayerFromVRRig(rig)))).OrderBy<VRRig, float>((Func<VRRig, float>) (rig => Vector3.Angle(head.forward, (((Component) rig).transform.position - head.position).normalized) + Vector3.Distance(head.position, ((Component) rig).transform.position) * 0.1f)).FirstOrDefault<VRRig>();
      if ((UnityEngine.Object) vrRig == (UnityEngine.Object) null)
        return;
      Vector3 position1 = vrRig.headMesh.transform.position;
      Vector3 vector3_1 = vrRig.LatestVelocity();
      vector3_1.y *= 0.33f;
      Vector3 position2 = __instance.center.transform.position;
      float num1 = Vector3.Distance(position2, position1) / 20f;
      Vector3 vector3_2 = position1 + vector3_1 * num1 - position2;
      Vector3 vector3_3 = new Vector3(vector3_2.x, 0.0f, vector3_2.z);
      float magnitude = vector3_3.magnitude;
      float y = vector3_2.y;
      float num2 = -Physics.gravity.y;
      float num3 = Mathf.Sqrt(num2 * (y + Mathf.Sqrt((float) ((double) magnitude * (double) magnitude + (double) y * (double) y)))) * 2.5f;
      float num4 = num3 * num3;
      float f1 = (float) ((double) num4 * (double) num4 - (double) num2 * ((double) num2 * (double) magnitude * (double) magnitude + 2.0 * (double) y * (double) num4));
      if ((double) f1 <= 0.0)
      {
        __result = vector3_2.normalized * num3;
      }
      else
      {
        float f2 = Mathf.Atan((float) (((double) num4 - (double) Mathf.Sqrt(f1)) / ((double) num2 * (double) magnitude)));
        __result = vector3_3.normalized * Mathf.Cos(f2) * num3 + Vector3.up * Mathf.Sin(f2) * num3;
      }
    }
  }

  [HarmonyPatch(typeof (PhotonNetworkController), "OnJoinedRoom")]
  public class JoinedRoomPatch
  {
    public static bool enabled;

    [HarmonyPrefix]
    private static void Prefix()
    {
      Projectile.ResetProjectiles();
      if (!MenuPatches.JoinedRoomPatch.enabled)
        return;
      PhotonNetworkController.Instance.currentJoinType = (JoinType) 6;
    }
  }

  [HarmonyPatch(typeof (VRRig), "PackCompetitiveData")]
  public class FPSSpoofer
  {
    public static bool enabled;

    [HarmonyPostfix]
    public static void Postfix(ref short __result)
    {
      if (!MenuPatches.FPSSpoofer.enabled)
        return;
      VRRig localRig = VRRig.LocalRig;
      if (!localRig.turningCompInitialized)
      {
        localRig.GorillaSnapTurningComp = ((Component) GorillaTagger.Instance).GetComponent<GorillaSnapTurn>();
        localRig.turningCompInitialized = true;
      }
      int num = 0;
      if ((UnityEngine.Object) localRig.GorillaSnapTurningComp != (UnityEngine.Object) null)
      {
        localRig.turnFactor = localRig.GorillaSnapTurningComp.turnFactor;
        localRig.turnType = localRig.GorillaSnapTurningComp.turnType;
        switch (localRig.turnType)
        {
          case "SNAP":
            num = 1;
            break;
          case "SMOOTH":
            num = 2;
            break;
        }
        num = num * 10 + localRig.turnFactor;
      }
      __result = (short) (90 + (num << 8));
    }
  }

  [HarmonyPatch(typeof (PhotonNetworkController), "AttemptToJoinRankedPublicRoom")]
  public class RankedPatch
  {
    public static bool enabled;
    public static string targetPlatform;
    public static string targetTier;

    [HarmonyPrefix]
    public static bool Prefix(GorillaNetworkJoinTrigger triggeredTrigger, JoinType roomJoinType = (JoinType) 0)
    {
      if (!MenuPatches.RankedPatch.enabled)
        return true;
      PhotonNetworkController.Instance.AttemptToJoinRankedPublicRoomAsync(triggeredTrigger, MenuPatches.RankedPatch.targetTier ?? RankedProgressionManager.Instance.GetRankedMatchmakingTier().ToString(), MenuPatches.RankedPatch.targetPlatform ?? "PC", roomJoinType);
      return false;
    }
  }

  [HarmonyPatch(typeof (GorillaWrappedSerializer), "FailedToSpawn")]
  public class FailedToSpawn
  {
    public static bool enabled;

    [HarmonyPrefix]
    public static bool Prefix(GorillaWrappedSerializer __instance)
    {
      if (!MenuPatches.FailedToSpawn.enabled)
        return true;
      ((Component) __instance).gameObject.SetActive(false);
      return false;
    }
  }

  [HarmonyPatch(typeof (NewMapsDisplay), "UpdateSlideshow")]
  public class NewMapsDisplayPatch
  {
    [HarmonyPrefix]
    private static bool Prefix() => false;
  }

  [HarmonyPatch(typeof (PhotonNetwork), "RunViewUpdate")]
  public class SerializationPatch
  {
    public static Func<bool> Override;

    public static event Action OnSerialization;

    [HarmonyPrefix]
    private static bool Prefix()
    {
      if (!PhotonNetwork.InRoom)
        return true;
      try
      {
        Action onSerialization = MenuPatches.SerializationPatch.OnSerialization;
        if (onSerialization != null)
          onSerialization();
      }
      catch (Exception ex)
      {
        Debug.Log((object) ("SerializationPatch tick error: " + ex.Message));
      }
      if (MenuPatches.SerializationPatch.Override == null)
        return true;
      try
      {
        return MenuPatches.SerializationPatch.Override();
      }
      catch (Exception ex)
      {
        Debug.Log((object) ("SerializationPatch override error: " + ex.Message));
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        if ((UnityEngine.Object) VRRig.LocalRig != (UnityEngine.Object) null)
          ((Behaviour) VRRig.LocalRig).enabled = true;
        return true;
      }
    }
  }

  [HarmonyPatch]
  public class PlayFabRateLimitPatch
  {
    [HarmonyPatch(typeof (PlayFabHttp), "SendErrorEvent")]
    [HarmonyPrefix]
    private static bool BlockRateLimitDisconnect3(PlayFabRequestCommon request, PlayFabError error)
    {
      return error.Error != (PlayFabErrorCode) 1214;
    }

    [HarmonyPatch(typeof (LoadBalancingClient), "OnDisconnectMessageReceived")]
    public class BlockRateLimitDisconnect
    {
      [HarmonyPrefix]
      private static bool Prefix(DisconnectMessage obj) => obj.Code != (short) -35;
    }

    [HarmonyPatch(typeof (MonoBehaviourPunCallbacks), "OnDisconnected")]
    public class BlockRateLimitDisconnect1
    {
      [HarmonyPrefix]
      private static bool Prefix(DisconnectCause cause) => cause != (DisconnectCause) 12 && cause != (DisconnectCause) 6 && cause != (DisconnectCause) 7;
    }
  }

  [HarmonyPatch]
  internal static class PhotonSafeLimiter
  {
    public static int MaxCallsPerSecond = 499;
    private static readonly Queue<float> Calls = new Queue<float>();
    private static readonly object Obj = new object();

    public static bool Allow()
    {
      lock (MenuPatches.PhotonSafeLimiter.Obj)
      {
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        while (MenuPatches.PhotonSafeLimiter.Calls.Count > 0 && (double) realtimeSinceStartup - (double) MenuPatches.PhotonSafeLimiter.Calls.Peek() >= 1.0)
        {
          double num = (double) MenuPatches.PhotonSafeLimiter.Calls.Dequeue();
        }
        if (MenuPatches.PhotonSafeLimiter.Calls.Count >= MenuPatches.PhotonSafeLimiter.MaxCallsPerSecond)
          return false;
        MenuPatches.PhotonSafeLimiter.Calls.Enqueue(realtimeSinceStartup);
        return true;
      }
    }

    public static void ClearLocal()
    {
      lock (MenuPatches.PhotonSafeLimiter.Obj)
        MenuPatches.PhotonSafeLimiter.Calls.Clear();
    }
  }

  [HarmonyPatch(typeof (PhotonNetwork), "RaiseEvent", new Type[] {typeof (byte), typeof (object), typeof (RaiseEventOptions), typeof (SendOptions)})]
  internal class Patch_RaiseEvent
  {
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
      if (MenuPatches.PhotonSafeLimiter.Allow())
        return true;
      __result = false;
      return false;
    }
  }

  [HarmonyPatch(typeof (PhotonNetwork), "RaiseEventInternal", new Type[] {typeof (byte), typeof (object), typeof (RaiseEventOptions), typeof (SendOptions)})]
  internal class Patch_RaiseEventInternal
  {
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
      if (MenuPatches.PhotonSafeLimiter.Allow())
        return true;
      __result = false;
      return false;
    }
  }

  [HarmonyPatch(typeof (PhotonNetwork), "SendInstantiate")]
  internal class Patch_SendInstantiate
  {
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
      if (MenuPatches.PhotonSafeLimiter.Allow())
        return true;
      __result = false;
      return false;
    }
  }

  [HarmonyPatch]
  internal class Patch_PhotonNetwork_RPC
  {
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
      return (IEnumerable<MethodBase>) ((IEnumerable<MethodInfo>) typeof (PhotonNetwork).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)).Where<MethodInfo>((Func<MethodInfo, bool>) (x => x.Name == "RPC"));
    }

    [HarmonyPrefix]
    private static bool Prefix() => MenuPatches.PhotonSafeLimiter.Allow();
  }

  [HarmonyPatch]
  internal class Patch_PhotonView_RPC
  {
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
      return (IEnumerable<MethodBase>) ((IEnumerable<MethodInfo>) typeof (PhotonView).GetMethods(BindingFlags.Instance | BindingFlags.Public)).Where<MethodInfo>((Func<MethodInfo, bool>) (x => x.Name == "RPC" || x.Name == "RpcSecure"));
    }

    [HarmonyPrefix]
    private static bool Prefix() => MenuPatches.PhotonSafeLimiter.Allow();
  }

  [HarmonyPatch(typeof (PhotonNetwork), "OnEvent")]
  internal class InterceptPendingReports3
  {
    [HarmonyPrefix]
    private static bool Prefix(EventData photonEvent)
    {
      if (photonEvent.Code != (byte) 50 || !(photonEvent.CustomData is object[] customData) || !((string) customData[0] == PhotonNetwork.LocalPlayer.UserId))
        return true;
      customData[0] = (object) ((IEnumerable<Photon.Realtime.Player>) PhotonNetwork.PlayerListOthers).Where<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (x => x.ActorNumber == photonEvent.Sender)).Select<Photon.Realtime.Player, string>((Func<Photon.Realtime.Player, string>) (x => x.UserId)).FirstOrDefault<string>();
      NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, ((IEnumerable<Photon.Realtime.Player>) PhotonNetwork.PlayerListOthers).Where<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (x => x.ActorNumber == photonEvent.Sender)).Select<Photon.Realtime.Player, string>((Func<Photon.Realtime.Player, string>) (x => x.NickName)).FirstOrDefault<string>() + " tried to report you");
      return false;
    }
  }

  [HarmonyPatch(typeof (RoomSystem), "SearchForNearby")]
  public class GroupPatch
  {
    public static bool enabled;

    [HarmonyPrefix]
    public static bool Prefix() => !MenuPatches.GroupPatch.enabled;
  }

  [HarmonyPatch(typeof (GTPlayer), "TakeMyHand_ProcessMovement")]
  public class GrabPatch
  {
    public static bool enabled;

    [HarmonyPrefix]
    public static bool Prefix(GTPlayer __instance) => !MenuPatches.GrabPatch.enabled;
  }

  public class BuggPatches
  {
    [HarmonyPatch(typeof (GorillaKeyboardButton), "OnTriggerEnter")]
    [HarmonyPrefix]
    public static bool Prefix1(GorillaKeyboardButton __instance)
    {
      GorillaComputer instance = GorillaComputer.instance;
      switch ((int) ((GorillaKeyButton<GorillaKeyboardBindings>) __instance).Binding)
      {
        case 0:
          MenuPatches.BuggPatches.HandleCharacter(instance, "0");
          break;
        case 1:
          MenuPatches.BuggPatches.HandleCharacter(instance, "1");
          break;
        case 2:
          MenuPatches.BuggPatches.HandleCharacter(instance, "2");
          break;
        case 3:
          MenuPatches.BuggPatches.HandleCharacter(instance, "3");
          break;
        case 4:
          MenuPatches.BuggPatches.HandleCharacter(instance, "4");
          break;
        case 5:
          MenuPatches.BuggPatches.HandleCharacter(instance, "5");
          break;
        case 6:
          MenuPatches.BuggPatches.HandleCharacter(instance, "6");
          break;
        case 7:
          MenuPatches.BuggPatches.HandleCharacter(instance, "7");
          break;
        case 8:
          MenuPatches.BuggPatches.HandleCharacter(instance, "8");
          break;
        case 9:
          MenuPatches.BuggPatches.HandleCharacter(instance, "9");
          break;
        case 10:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.currentStateIndex = 2;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 3:
              instance.currentStateIndex = 3;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 4:
              instance.currentStateIndex = 4;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 5:
              instance.currentStateIndex = 0;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 6:
              instance.currentStateIndex = 6;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 7:
              instance.currentStateIndex = 7;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 8:
              instance.currentStateIndex = 8;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 9:
              instance.currentStateIndex = 9;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 10:
              instance.currentStateIndex = 10;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 11:
              instance.currentStateIndex = 9;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 15:
              instance.currentStateIndex = 15;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.currentStateIndex = 16 /*0x10*/;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 18:
              instance.currentStateIndex = 18;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 19:
              instance.currentStateIndex = 19;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
          }
          break;
        case 11:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.currentStateIndex = 19;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 3:
              instance.currentStateIndex = 4;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 4:
              instance.currentStateIndex = 6;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 5:
              instance.currentStateIndex = 2;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 6:
              instance.currentStateIndex = 16 /*0x10*/;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 7:
              instance.currentStateIndex = 8;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 8:
              instance.currentStateIndex = 9;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 9:
              instance.currentStateIndex = 9;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 10:
              instance.currentStateIndex = 15;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 11:
              instance.currentStateIndex = 18;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 15:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.currentStateIndex = 7;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 18:
              instance.currentStateIndex = 10;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 19:
              instance.currentStateIndex = 3;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
          }
          break;
        case 12:
          GorillaComputer.ComputerState currentState = instance.currentState;
          if (currentState <= (GorillaComputer.ComputerState) 5)
          {
            if (currentState != (GorillaComputer.ComputerState) 2)
            {
              if (currentState == (GorillaComputer.ComputerState) 5)
              {
                instance.ProcessRoomState((GorillaKeyboardBindings) 12);
                instance.UpdateScreen();
                break;
              }
              break;
            }
            instance.ProcessNameState((GorillaKeyboardBindings) 12);
            instance.UpdateScreen();
            break;
          }
          if (currentState != (GorillaComputer.ComputerState) 13)
          {
            if (currentState != (GorillaComputer.ComputerState) 16 /*0x10*/)
            {
              if (currentState == (GorillaComputer.ComputerState) 18)
              {
                instance.ProcessRedemptionState((GorillaKeyboardBindings) 12);
                instance.UpdateScreen();
                break;
              }
              break;
            }
            instance.ProcessTroopState((GorillaKeyboardBindings) 12);
            instance.UpdateScreen();
            break;
          }
          instance.ProcessNameWarningState((GorillaKeyboardBindings) 12);
          instance.UpdateScreen();
          break;
        case 13:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.ProcessNameState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 3:
              instance.ProcessTurnState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 4:
              instance.ProcessMicState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 5:
              instance.ProcessRoomState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 6:
              instance.ProcessQueueState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 7:
              instance.ProcessGroupState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 8:
              instance.ProcessVoiceState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 9:
              instance.ProcessAutoMuteState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 10:
              instance.ProcessCreditsState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 11:
              instance.ProcessVisualsState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 13:
              instance.ProcessNameWarningState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 15:
              instance.ProcessSupportState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.ProcessTroopState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 17:
              instance.ProcessKIdState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 18:
              instance.ProcessRedemptionState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
            case 19:
              instance.ProcessLanguageState((GorillaKeyboardBindings) 13);
              instance.UpdateScreen();
              break;
          }
          break;
        case 14:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.ProcessNameState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 3:
              instance.ProcessTurnState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 4:
              instance.ProcessMicState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 5:
              instance.ProcessRoomState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 6:
              instance.ProcessQueueState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 7:
              instance.ProcessGroupState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 8:
              instance.ProcessVoiceState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 9:
              instance.ProcessAutoMuteState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 10:
              instance.ProcessCreditsState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 11:
              instance.ProcessVisualsState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 13:
              instance.ProcessNameWarningState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 15:
              instance.ProcessSupportState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.ProcessTroopState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 17:
              instance.ProcessKIdState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 18:
              instance.ProcessRedemptionState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
            case 19:
              instance.ProcessLanguageState((GorillaKeyboardBindings) 14);
              instance.UpdateScreen();
              break;
          }
          break;
        case 15:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.ProcessNameState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 3:
              instance.ProcessTurnState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 4:
              instance.ProcessMicState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 5:
              instance.ProcessRoomState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 6:
              instance.ProcessQueueState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 7:
              instance.ProcessGroupState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 8:
              instance.ProcessVoiceState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 9:
              instance.ProcessAutoMuteState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 10:
              instance.ProcessCreditsState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 11:
              instance.ProcessVisualsState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 13:
              instance.ProcessNameWarningState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 15:
              instance.ProcessSupportState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.ProcessTroopState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 17:
              instance.ProcessKIdState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 18:
              instance.ProcessRedemptionState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
            case 19:
              instance.ProcessLanguageState((GorillaKeyboardBindings) 15);
              instance.UpdateScreen();
              break;
          }
          break;
        case 16 /*0x10*/:
          switch ((int) instance.currentState)
          {
            case 0:
              instance.currentStateIndex = 5;
              instance.SwitchState(instance.GetState(instance.currentStateIndex), true);
              instance.UpdateScreen();
              break;
            case 2:
              instance.ProcessNameState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 3:
              instance.ProcessTurnState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 4:
              instance.ProcessMicState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 5:
              instance.ProcessRoomState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 6:
              instance.ProcessQueueState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 7:
              instance.ProcessGroupState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 8:
              instance.ProcessVoiceState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 9:
              instance.ProcessAutoMuteState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 10:
              instance.ProcessCreditsState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 11:
              instance.ProcessVisualsState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 13:
              instance.ProcessNameWarningState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 15:
              instance.ProcessSupportState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 16 /*0x10*/:
              instance.ProcessTroopState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 17:
              instance.ProcessKIdState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 18:
              instance.ProcessRedemptionState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
            case 19:
              instance.ProcessLanguageState((GorillaKeyboardBindings) 16 /*0x10*/);
              instance.UpdateScreen();
              break;
          }
          break;
        case 17:
          MenuPatches.BuggPatches.HandleCharacter(instance, "A");
          break;
        case 18:
          MenuPatches.BuggPatches.HandleCharacter(instance, "B");
          break;
        case 19:
          MenuPatches.BuggPatches.HandleCharacter(instance, "C");
          break;
        case 20:
          MenuPatches.BuggPatches.HandleCharacter(instance, "D");
          break;
        case 21:
          MenuPatches.BuggPatches.HandleCharacter(instance, "E");
          break;
        case 22:
          MenuPatches.BuggPatches.HandleCharacter(instance, "F");
          break;
        case 23:
          MenuPatches.BuggPatches.HandleCharacter(instance, "G");
          break;
        case 24:
          MenuPatches.BuggPatches.HandleCharacter(instance, "H");
          break;
        case 25:
          MenuPatches.BuggPatches.HandleCharacter(instance, "I");
          break;
        case 26:
          MenuPatches.BuggPatches.HandleCharacter(instance, "J");
          break;
        case 27:
          MenuPatches.BuggPatches.HandleCharacter(instance, "K");
          break;
        case 28:
          MenuPatches.BuggPatches.HandleCharacter(instance, "L");
          break;
        case 29:
          MenuPatches.BuggPatches.HandleCharacter(instance, "M");
          break;
        case 30:
          MenuPatches.BuggPatches.HandleCharacter(instance, "N");
          break;
        case 31 /*0x1F*/:
          MenuPatches.BuggPatches.HandleCharacter(instance, "O");
          break;
        case 32 /*0x20*/:
          MenuPatches.BuggPatches.HandleCharacter(instance, "P");
          break;
        case 33:
          MenuPatches.BuggPatches.HandleCharacter(instance, "Q");
          break;
        case 34:
          MenuPatches.BuggPatches.HandleCharacter(instance, "R");
          break;
        case 35:
          MenuPatches.BuggPatches.HandleCharacter(instance, "S");
          break;
        case 36:
          MenuPatches.BuggPatches.HandleCharacter(instance, "T");
          break;
        case 37:
          MenuPatches.BuggPatches.HandleCharacter(instance, "U");
          break;
        case 38:
          MenuPatches.BuggPatches.HandleCharacter(instance, "V");
          break;
        case 39:
          MenuPatches.BuggPatches.HandleCharacter(instance, "W");
          break;
        case 40:
          MenuPatches.BuggPatches.HandleCharacter(instance, "X");
          break;
        case 41:
          MenuPatches.BuggPatches.HandleCharacter(instance, "Y");
          break;
        case 42:
          MenuPatches.BuggPatches.HandleCharacter(instance, "Z");
          break;
      }
      return false;
    }

    private static void HandleCharacter(GorillaComputer computer, string character)
    {
      GorillaComputer.ComputerState currentState = computer.currentState;
      if (currentState <= (GorillaComputer.ComputerState) 11)
      {
        switch ((int) currentState)
        {
          case 0:
            computer.currentStateIndex = 5;
            computer.SwitchState(computer.GetState(computer.currentStateIndex), true);
            computer.UpdateScreen();
            break;
          case 1:
            break;
          case 2:
            if (computer.currentName.Length < 12)
              computer.currentName += character;
            computer.UpdateScreen();
            break;
          case 3:
            break;
          case 4:
            break;
          case 5:
            if (computer.roomToJoin.Length < 10)
              computer.roomToJoin += character;
            computer.UpdateScreen();
            break;
          default:
            if (currentState == (GorillaComputer.ComputerState) 11)
              break;
            break;
        }
      }
      else if (currentState != (GorillaComputer.ComputerState) 16 /*0x10*/)
      {
        if (currentState != (GorillaComputer.ComputerState) 18)
          return;
        if (computer.redemptionCode.Length < 8)
          computer.redemptionCode += character;
        computer.UpdateScreen();
      }
      else
      {
        if (computer.troopToJoin.Length < 12)
          computer.troopToJoin += character;
        computer.UpdateScreen();
      }
    }

    [HarmonyPatch(typeof (MonoBehaviourPunCallbacks), "OnJoinedRoom")]
    [HarmonyPrefix]
    public static bool Prefix2()
    {
      VRRig.LocalRig.Creator.ReceivedSingleCallRPC((NetPlayer.SingleCallRPC) 0);
      PhotonSerializer.BroadcastViews();
      PhotonSerializer.SendSerialize(GorillaTagger.Instance.myVRRig.GetView);
      if (!((GorillaWrappedSerializer) GameMode.activeNetworkHandler).successfullInstantiate)
        GameMode.ResetGameModeSafe(GameMode.activeGameMode);
      return true;
    }
  }
}
