
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaNetworking;
using GorillaTagScripts;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Room : MonoBehaviourPunCallbacks
{
  public static string roomCode;
  public static bool instantCreate;

  public static void Disconnect()
  {
    if (!PhotonNetwork.InRoom)
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
    else
      NetworkSystem.Instance.ReturnToSinglePlayer();
  }

  public static void Reconnect()
  {
    if (string.IsNullOrEmpty(Room.roomCode))
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Room Code Saved");
    else if (PhotonNetwork.InRoom)
    {
      NetworkSystem.Instance.ReturnToSinglePlayer();
      CoroutineHelper.InvokeAfterDelay(1.5f, new Action(Room.Reconnect));
    }
    else
      PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(Room.roomCode, (JoinType) 0, (Action<NetJoinResult>) (result =>
      {
        if (result == (NetJoinResult) 2)
        {
          NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Room Is Full");
        }
        else
        {
          if (result != (NetJoinResult) 4)
            return;
          NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Unknown");
        }
      }));
  }

  public static void JoinRandomPublic()
  {
    if (PhotonNetwork.InRoom)
    {
      NetworkSystem.Instance.ReturnToSinglePlayer();
      CoroutineHelper.InvokeAfterDelay(1.5f, new Action(Room.JoinRandomPublic));
    }
    else
    {
      GorillaNetworkJoinTrigger networkJoinTrigger = PhotonNetworkController.Instance.currentJoinTrigger ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");
      if ((UnityEngine.Object) networkJoinTrigger == (UnityEngine.Object) null)
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Join Trigger Found");
      else
        PhotonNetworkController.Instance.AttemptToJoinPublicRoom(networkJoinTrigger, (JoinType) 0, (List<(string, string)>) null, false);
    }
  }

  public static void JoinRoom()
  {
    SearchAndKeyboard.OpenTypingKeyboard(placeholder: "Enter room code...");
    SearchAndKeyboard.onTypingComplete = (Action<string>) (code =>
    {
      if (string.IsNullOrEmpty(code))
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Room Code Entered");
      else
        PhotonNetworkController.Instance.AttemptToJoinSpecificRoomWithCallback(code, (JoinType) 0, (Action<NetJoinResult>) (result =>
        {
          if (result == (NetJoinResult) 2)
          {
            NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Room Is Full");
          }
          else
          {
            if (result != (NetJoinResult) 4)
              return;
            NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Unknown");
          }
        }));
    });
  }

  public static void UnlockVim()
  {
    Dictionary<NetPlayer, SubscriptionManager.SubscriptionDetails> subData = SubscriptionManager.Instance.subData;
    foreach (KeyValuePair<NetPlayer, SubscriptionManager.SubscriptionDetails> keyValuePair in subData.ToList<KeyValuePair<NetPlayer, SubscriptionManager.SubscriptionDetails>>())
    {
      if (keyValuePair.Key.IsLocal)
      {
        SubscriptionManager.SubscriptionDetails subscriptionDetails = keyValuePair.Value;
        subscriptionDetails.active = true;
        subscriptionDetails.tier = 1;
        subscriptionDetails.daysAccrued = 128 /*0x80*/;
        subData[keyValuePair.Key] = subscriptionDetails;
      }
    }
  }

  public static void SetNetworkTriggersActive(bool isActive)
  {
    GameObject gameObject = GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab");
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.SetActive(isActive);
  }

  public static void SetQuitBoxActive(bool isActive)
  {
    GameObject gameObject = GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox");
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.SetActive(isActive);
  }

  public static void GrabAllIDs()
  {
    if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.PlayerList == null || PhotonNetwork.PlayerList.Length == 0)
    {
      UnityEngine.Debug.LogError((object) "Failed to grab IDs: No room or players found.");
    }
    else
    {
      StringBuilder stringBuilder = new StringBuilder($"IDS GRABBED FROM NXO \nIDS GRABBED FROM ROOM: {PhotonNetwork.CurrentRoom.Name}\n\n");
      foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        stringBuilder.AppendLine($"NAME: {player.NickName} ID: {player.UserId}");
      string str = Path.Combine(Variables.folderName, "Grabbed_IDs_By_NXO.txt");
      File.WriteAllText(str, stringBuilder.ToString());
      Process.Start(new ProcessStartInfo(str)
      {
        UseShellExecute = true
      });
    }
  }

  public static void GrabSelfID()
  {
    StringBuilder stringBuilder = new StringBuilder("SELF ID GRABBED FROM NXO");
    stringBuilder.AppendLine($"NAME: {PhotonNetwork.LocalPlayer.NickName} ID: {PhotonNetwork.LocalPlayer.UserId}");
    string str = Path.Combine(Variables.folderName, "Self_ID_By_NXO.txt");
    File.WriteAllText(str, stringBuilder.ToString());
    Process.Start(new ProcessStartInfo(str)
    {
      UseShellExecute = true
    });
  }

  public static void DumpAllRPCs()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("=== ALL RPCS IN GORILLA TAG ===");
    stringBuilder.AppendLine($"Generated: {DateTime.Now}\n");
    int num = 0;
    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      try
      {
        foreach (Type type in assembly.GetTypes())
        {
          IEnumerable<MethodInfo> source = ((IEnumerable<MethodInfo>) type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)).Where<MethodInfo>((Func<MethodInfo, bool>) (m => m.GetCustomAttribute<PunRPC>() != null));
          if (source.Any<MethodInfo>())
          {
            stringBuilder.AppendLine($"\n━━━ {type.FullName} ━━━");
            foreach (MethodInfo methodInfo in source)
            {
              ParameterInfo[] parameters = methodInfo.GetParameters();
              string str = parameters.Length != 0 ? string.Join(", ", ((IEnumerable<ParameterInfo>) parameters).Select<ParameterInfo, string>((Func<ParameterInfo, string>) (p => $"{p.ParameterType.Name} {p.Name}"))) : "no parameters";
              stringBuilder.AppendLine($"  [RPC] {methodInfo.Name}({str})");
              ++num;
            }
          }
        }
      }
      catch
      {
      }
    }
    stringBuilder.AppendLine($"\n\nTotal RPCs Found: {num}");
    string str1 = Path.Combine(Variables.folderName, $"RPC_Dump_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
    File.WriteAllText(str1, stringBuilder.ToString());
    Process.Start(new ProcessStartInfo(str1)
    {
      UseShellExecute = true
    });
  }

  public static async Task ForceCreateRoom(string name, RoomConfig options)
  {
    if (NetworkSystem.Instance.InRoom)
      await NetworkSystem.Instance.ReturnToSinglePlayer();
    NetJoinResult room = await (NetworkSystem.Instance as NetworkSystemPUN).TryCreateRoom(name, options);
  }

  public static void CreateRoom(
    string roomName,
    bool isPublic,
    byte roomSize = 0,
    JoinType roomJoinType = (JoinType) 0)
  {
    if (roomSize > (byte) 10 && !roomName.StartsWith("@"))
      roomName = "@" + roomName;
    GorillaNetworkJoinTrigger networkJoinTrigger = PhotonNetworkController.Instance.currentJoinTrigger ?? GorillaComputer.instance.GetJoinTriggerForZone("forest");
    RoomConfig roomConfig = new RoomConfig();
    roomConfig.createIfMissing = true;
    roomConfig.isJoinable = true;
    roomConfig.isPublic = isPublic;
    roomConfig.MaxPlayers = roomSize == (byte) 0 ? RoomSystem.GetRoomSizeForCreate(networkJoinTrigger.zone, Enum.Parse<GameModeType>(GorillaComputer.instance.currentGameMode.Value, true), !isPublic, SubscriptionManager.IsLocalSubscribed()) : roomSize;
    Hashtable hashtable = new Hashtable();
    ((Dictionary<object, object>) hashtable).Add((object) "platform", (object) PhotonNetworkController.Instance.platformTag);
    ((Dictionary<object, object>) hashtable).Add((object) "gameMode", (object) networkJoinTrigger.GetFullDesiredGameModeString());
    ((Dictionary<object, object>) hashtable).Add((object) "language", (object) LocalisationManager.CurrentLanguage.ToString());
    ((Dictionary<object, object>) hashtable).Add((object) "fan_club", SubscriptionManager.IsLocalSubscribed() ? (object) "true" : (object) "false");
    ((Dictionary<object, object>) hashtable).Add((object) "queueName", (object) GorillaComputer.instance.currentQueue);
    roomConfig.CustomProps = hashtable;
    RoomConfig options = roomConfig;
    PhotonNetworkController.Instance.currentJoinType = roomJoinType;
    if (roomJoinType == (JoinType) 2 || roomJoinType == (JoinType) 4)
      Task.Run(new Func<Task>(PhotonNetworkController.Instance.SendPartyFollowCommands));
    switch (roomJoinType - 1)
    {
      case 0:
      case (GorillaNetworking.JoinType) 2:
        options.SetFriendIDs(PhotonNetworkController.Instance.FriendIDList);
        break;
      case (GorillaNetworking.JoinType) 1:
      case (GorillaNetworking.JoinType) 3:
        options.SetFriendIDs(FriendshipGroupDetection.Instance.PartyMemberIDs.ToList<string>());
        break;
    }
    if (Room.instantCreate)
    {
      (NetworkSystem.Instance as NetworkSystemPUN).internalState = (NetworkSystemPUN.InternalState) 16 /*0x10*/;
      Room.ForceCreateRoom(roomName, options);
    }
    else
      NetworkSystem.Instance.ConnectToRoom(roomName, options, -1);
  }

  public static string RandomString(int length = 4)
  {
    char[] chArray = new char[length];
    for (int index = 0; index < length; ++index)
      chArray[index] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)];
    return new string(chArray);
  }

  public static string RandomRoomName()
  {
    string str;
    do
    {
      str = Room.RandomString();
    }
    while (GorillaComputer.instance.CheckAutoBanListForName(str));
    return str;
  }
}
