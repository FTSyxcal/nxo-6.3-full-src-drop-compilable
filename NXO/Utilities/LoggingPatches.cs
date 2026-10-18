
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

[HarmonyPatch]
public static class LoggingPatches
{
  [HarmonyPatch(typeof (VRRig), "IncrementRPC", new Type[] {typeof (PhotonMessageInfoWrapped), typeof (string)})]
  [HarmonyPrefix]
  public static void VRRig_IncrementRPC_Wrapped(
    VRRig __instance,
    PhotonMessageInfoWrapped info,
    string sourceCall)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.RPCBlacklist.Contains(sourceCall))
      return;
    try
    {
      string str = "Unknown";
      int num = -1;
      PhotonView getView = __instance.netView?.GetView;
      if ((UnityEngine.Object) getView != (UnityEngine.Object) null && getView.Owner != null)
      {
        str = getView.Owner.NickName ?? "Unknown";
        num = getView.Owner.ActorNumber;
      }
      else if (__instance?.Creator != null)
      {
        str = __instance.Creator.NickName ?? "Unknown";
        num = __instance.Creator.ActorNumber;
      }
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[RPC] VRRig");
      stringBuilder.AppendLine("  ├─ Method: " + sourceCall);
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{num})");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (VRRig), "IncrementRPC", new Type[] {typeof (PhotonMessageInfo), typeof (string)})]
  [HarmonyPrefix]
  public static void VRRig_IncrementRPC_Info(
    VRRig __instance,
    PhotonMessageInfo info,
    string sourceCall)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.RPCBlacklist.Contains(sourceCall))
      return;
    try
    {
      string str = info.Sender?.NickName ?? "Unknown";
      Photon.Realtime.Player sender = info.Sender;
      int num = sender != null ? sender.ActorNumber : -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[RPC] VRRig");
      stringBuilder.AppendLine("  ├─ Method: " + sourceCall);
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{num})");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (MonkeAgent), "IncrementRPCCall", new Type[] {typeof (PhotonMessageInfo), typeof (string)})]
  [HarmonyPrefix]
  public static void MonkeAgent_IncrementRPCCall(PhotonMessageInfo info, string callingMethod)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.RPCBlacklist.Contains(callingMethod))
      return;
    try
    {
      string str = info.Sender?.NickName ?? "Unknown";
      Photon.Realtime.Player sender1 = info.Sender;
      int num = sender1 != null ? sender1.ActorNumber : -1;
      Photon.Realtime.Player sender2 = info.Sender;
      bool flag = sender2 != null && sender2.IsMasterClient;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[RPC] MonkeAgent");
      stringBuilder.AppendLine("  ├─ Method: " + callingMethod);
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{num}) {(flag ? (object) "[MASTER]" : (object) "")}");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (MonkeAgent), "IncrementRPCCallLocal", new Type[] {typeof (PhotonMessageInfoWrapped), typeof (string)})]
  [HarmonyPrefix]
  public static void MonkeAgent_IncrementRPCCallLocal(
    MonkeAgent __instance,
    PhotonMessageInfoWrapped infoWrapped,
    string rpcFunction)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.RPCBlacklist.Contains(rpcFunction))
      return;
    try
    {
      string str = "Unknown";
      int num = -1;
      PhotonView component = ((Component) __instance).GetComponent<PhotonView>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null && component.Owner != null)
      {
        str = component.Owner.NickName ?? "Unknown";
        num = component.Owner.ActorNumber;
      }
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[RPC] MonkeAgent.Local");
      stringBuilder.AppendLine("  ├─ Method: " + rpcFunction);
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{num})");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (LoadBalancingClient), "OnEvent")]
  [HarmonyPrefix]
  public static void OnEvent(EventData photonEvent)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.EventBlacklist.Contains(photonEvent.Code))
      return;
    try
    {
      int sender = photonEvent.Sender;
      Photon.Realtime.Player player = PhotonNetwork.CurrentRoom?.GetPlayer(sender, false);
      string str = player?.NickName ?? $"Actor{sender}";
      bool flag = player != null && player.IsMasterClient;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[EVENT] Photon IN");
      stringBuilder.AppendLine($"  ├─ Code: {photonEvent.Code}");
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{sender}) {(flag ? (object) "[MASTER]" : (object) "")}");
      if (photonEvent.CustomData != null)
        stringBuilder.AppendLine("  ├─ CustomData: " + Logger.FormatData(photonEvent.CustomData));
      if (photonEvent.Parameters != null)
      {
        foreach (KeyValuePair<byte, object> parameter in photonEvent.Parameters)
          stringBuilder.AppendLine($"  ├─ [{parameter.Key}] = {Logger.FormatData(parameter.Value)}");
      }
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (PhotonNetwork), "RaiseEvent")]
  [HarmonyPrefix]
  public static void RaiseEvent(
    byte eventCode,
    object eventContent,
    RaiseEventOptions raiseEventOptions,
    SendOptions sendOptions)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.EventBlacklist.Contains(eventCode))
      return;
    try
    {
      string str = PhotonNetwork.LocalPlayer?.NickName ?? "Local";
      Photon.Realtime.Player localPlayer = PhotonNetwork.LocalPlayer;
      int num = localPlayer != null ? localPlayer.ActorNumber : -1;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[EVENT] PhotonNetwork OUT");
      stringBuilder.AppendLine($"  ├─ Code: {eventCode}");
      stringBuilder.AppendLine($"  ├─ Sender: {str} (Actor #{num})");
      if (eventContent != null)
        stringBuilder.AppendLine("  ├─ Data: " + Logger.FormatData(eventContent));
      if (raiseEventOptions?.TargetActors != null)
        stringBuilder.AppendLine($"  ├─ Targets: [{string.Join<int>(", ", (IEnumerable<int>) raiseEventOptions.TargetActors)}]");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (PhotonPeer), "SendOperation", new Type[] {typeof (byte), typeof (Dictionary<byte, object>), typeof (SendOptions)})]
  [HarmonyPrefix]
  public static void SendOperation(
    byte operationCode,
    Dictionary<byte, object> operationParameters,
    SendOptions sendOptions)
  {
    if (!Logger.Enabled)
      return;
    try
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[OPERATION] SendOperation");
      stringBuilder.AppendLine($"  ├─ OpCode: {operationCode}");
      if (operationParameters != null)
      {
        foreach (KeyValuePair<byte, object> operationParameter in operationParameters)
          stringBuilder.AppendLine($"  ├─ [{operationParameter.Key}] = {Logger.FormatData(operationParameter.Value)}");
      }
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (PhotonView), "TransferOwnership", new Type[] {typeof (Photon.Realtime.Player)})]
  [HarmonyPrefix]
  public static void TransferOwnership(PhotonView __instance, Photon.Realtime.Player newOwner)
  {
    if (!Logger.Enabled)
      return;
    try
    {
      string str1 = __instance.Owner?.NickName ?? "None";
      string str2 = newOwner?.NickName ?? "None";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[OWNERSHIP] Transfer");
      stringBuilder.AppendLine($"  ├─ ViewID: {__instance.ViewID}");
      stringBuilder.AppendLine("  ├─ From: " + str1);
      stringBuilder.AppendLine("  ├─ To: " + str2);
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (PhotonView), "RequestOwnership")]
  [HarmonyPrefix]
  public static void RequestOwnership(PhotonView __instance)
  {
    if (!Logger.Enabled)
      return;
    try
    {
      string str = __instance.Owner?.NickName ?? "None";
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[OWNERSHIP] Request");
      stringBuilder.AppendLine($"  ├─ ViewID: {__instance.ViewID}");
      stringBuilder.AppendLine("  ├─ CurrentOwner: " + str);
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }

  [HarmonyPatch(typeof (LoadBalancingClient), "OpRaiseEvent")]
  [HarmonyPrefix]
  public static void LogOpRaiseEvent(
    byte eventCode,
    object customEventContent,
    RaiseEventOptions raiseEventOptions,
    SendOptions sendOptions)
  {
    if (!Logger.Enabled)
      return;
    if (Logger.EventBlacklist.Contains(eventCode))
      return;
    try
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      stringBuilder.AppendLine("[OP] OpRaiseEvent");
      stringBuilder.AppendLine($"  ├─ Code: {eventCode}");
      stringBuilder.AppendLine("  ├─ Data: " + Logger.FormatData(customEventContent));
      if (raiseEventOptions?.TargetActors != null)
        stringBuilder.AppendLine($"  ├─ Targets: [{string.Join<int>(", ", (IEnumerable<int>) raiseEventOptions.TargetActors)}]");
      stringBuilder.AppendLine(string.Format("  └─ Time: {0:HH:mm:ss.fff}", (object) DateTime.Now));
      stringBuilder.AppendLine("═══════════════════════════════════════════════════════════════");
      Debug.Log((object) stringBuilder.ToString());
    }
    catch
    {
    }
  }
}
