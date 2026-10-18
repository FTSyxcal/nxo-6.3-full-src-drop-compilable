
using ExitGames.Client.Photon;
using GorillaNetworking;
using GorillaTagScripts;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public static class Overpowered
{
  private static float _lagCooldown = 0.0f;
  private static readonly Dictionary<VRRig, int> materialState = new Dictionary<VRRig, int>();
  public static float materialDelay;
  private static float slowDelay;
  private static float vibrateDelay;
  private static float lastToggleTime = 0.0f;
  public static float roomSoundDelay;
  public static bool _earrapeGunActive = false;
  public static string specificRoom;
  private static float kickDelay;
  private static bool _kickRoomSet;
  public static float grabTimer;
  private static VRRig grabSyncTarget;
  private static Vector3 grabSyncPos;
  public static readonly List<LineRenderer> grabLines = new List<LineRenderer>();

  public static void LagTarget(int[] targetActors)
  {
    if (!PhotonNetwork.InRoom || (double) Time.time < (double) Overpowered._lagCooldown)
      return;
    Overpowered._lagCooldown = Time.time + Settings.LagCooldown;
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions()
    {
      TargetActors = targetActors,
      CachingOption = (EventCaching) 2
    };
    SendOptions sendOptions1 = new SendOptions();
    (sendOptions1).Reliability = false;
    sendOptions1.DeliveryMode = (DeliveryMode) 0;
    sendOptions1.Encrypt = true;
    SendOptions sendOptions2 = sendOptions1;
    if (Settings.lagTypeIndex == 2)
    {
      for (int index = 0; index < Settings.LagPackets; ++index)
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent((byte) 202, (object) new object[1]
        {
          (object) float.NaN
        }, raiseEventOptions, sendOptions2);
    }
    else
    {
      for (int index = 0; index < Settings.LagPackets; ++index)
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent((byte) 186, (object) new object[1]
        {
          (object) float.NaN
        }, raiseEventOptions, sendOptions2);
    }
    ((PhotonPeer) PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendOutgoingCommands();
    Safety.RPCShield();
  }

  public static void LagAll()
  {
    if (!InputHandler.RTrigger())
      return;
    List<int> intList = new List<int>();
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
        intList.Add(activeRig.Creator.ActorNumber);
    }
    if (intList.Count > 0)
      Overpowered.LagTarget(intList.ToArray());
  }

  public static void LagGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    Overpowered.LagTarget(new int[1]
    {
      GunLib.lockedTargetRig.Creator.ActorNumber
    });
  }

  public static void TouchToLag()
  {
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
      {
        Transform[] targets = new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        };
        if (RigManager.HandsTouchTargets(hands, targets))
        {
          Overpowered.LagTarget(new int[1]
          {
            activeRig.Creator.ActorNumber
          });
          break;
        }
      }
    }
  }

  public static void GetTouchedToLag()
  {
    Transform[] targets = new Transform[2]
    {
      Variables.taggerInstance.offlineVRRig.headMesh.transform,
      Variables.taggerInstance.offlineVRRig.bodyTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isMyPlayer)
      {
        if (RigManager.HandsTouchTargets(new Transform[2]
        {
          activeRig.leftHandTransform,
          activeRig.rightHandTransform
        }, targets))
        {
          Overpowered.LagTarget(new int[1]
          {
            activeRig.Creator.ActorNumber
          });
          break;
        }
      }
    }
  }

  public static void LagAura()
  {
    if (!PhotonNetwork.InRoom)
      return;
    List<int> intList = new List<int>();
    Vector3 position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal && (double) Vector3.Distance(position, ((Component) activeRig).transform.position) < 4.0)
        intList.Add(activeRig.Creator.ActorNumber);
    }
    if (intList.Count <= 0)
      return;
    Overpowered.LagTarget(intList.ToArray());
  }

  public static void ElevatorKickAll()
  {
    ((NetworkView) GRElevatorManager._instance).SendRPC("RemoteElevatorButtonPress", (RpcTarget) 2, new object[1]
    {
      (object) new int[2]
      {
        3,
        (int) GRElevatorManager._instance.currentLocation
      }
    });
    Safety.RPCShield();
  }

  public static void ElevatorKickGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    ((NetworkView) GRElevatorManager._instance).SendRPC("RemoteElevatorButtonPress", (RpcTarget) 2, new object[1]
    {
      (object) new int[2]
      {
        3,
        (int) GRElevatorManager._instance.currentLocation
      }
    });
    Safety.RPCShield();
  }

  public static void TouchToElevatorKick()
  {
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
      {
        Transform[] targets = new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        };
        if (RigManager.HandsTouchTargets(hands, targets))
        {
          ((NetworkView) GRElevatorManager._instance).SendRPC("RemoteElevatorButtonPress", (RpcTarget) 2, new object[1]
          {
            (object) new int[2]
            {
              3,
              (int) GRElevatorManager._instance.currentLocation
            }
          });
          Safety.RPCShield();
          break;
        }
      }
    }
  }

  public static void GetTouchedToElevatorKick()
  {
    Transform[] targets = new Transform[2]
    {
      Variables.taggerInstance.offlineVRRig.headMesh.transform,
      Variables.taggerInstance.offlineVRRig.bodyTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isMyPlayer)
      {
        if (RigManager.HandsTouchTargets(new Transform[2]
        {
          activeRig.leftHandTransform,
          activeRig.rightHandTransform
        }, targets))
        {
          ((NetworkView) GRElevatorManager._instance).SendRPC("RemoteElevatorButtonPress", (RpcTarget) 2, new object[1]
          {
            (object) new int[2]
            {
              3,
              (int) GRElevatorManager._instance.currentLocation
            }
          });
          Safety.RPCShield();
          break;
        }
      }
    }
  }

  public static void SetRoomStatus(bool status)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
    ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
    hashtable.Add((byte) 253, (object) status);
    hashtable.Add((byte) 254, (object) status);
    hashtable.Add(byte.MaxValue, (object) (status ? (int) PhotonNetwork.CurrentRoom.MaxPlayers : 0));
    dictionary.Add((byte) 251, (object) hashtable);
    dictionary.Add((byte) 250, (object) true);
    dictionary.Add((byte) 231, (object) null);
    ((PhotonPeer) PhotonNetwork.CurrentRoom.LoadBalancingClient.LoadBalancingPeer).SendOperation((byte) 252, dictionary, SendOptions.SendReliable);
    GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
  }

  public static void Mute(VRRig rig, bool mute)
  {
    RigContainer rigContainer = rig.rigContainer;
    rigContainer.SetMuted((RigContainer.MuteReason) 4, mute);
    rigContainer.SetMuted((RigContainer.MuteReason) 1, mute);
    rigContainer.SetMuted((RigContainer.MuteReason) 2, mute);
  }

  public static void DeafenPlayer(object player)
  {
    RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
    ReceiverGroup receiverGroup = default;
    int num;
    if (player is ReceiverGroup)
    {
      receiverGroup = (ReceiverGroup) player;
      num = 1;
    }
    else
      num = 0;
    if (num != 0)
    {
      raiseEventOptions.Receivers = receiverGroup;
    }
    else
    {
      if (!(player is int[] numArray))
        return;
      raiseEventOptions.TargetActors = numArray;
    }
    SendOptions sendOptions1 = new SendOptions();
    (sendOptions1).Reliability = true;
    sendOptions1.Channel = (byte) 0;
    SendOptions sendOptions2 = sendOptions1;
    ((LoadBalancingClient) ((VoiceConnection) PhotonVoiceNetwork.Instance).Client).OpRaiseEvent((byte) 202, (object) new object[3]
    {
      (object) (byte) 0,
      (object) (byte) 1,
      (object) new object[1]
      {
        (object) new Dictionary<byte, object>()
        {
          {
            (byte) 1,
            (object) (int) byte.MaxValue
          },
          {
            (byte) 2,
            (object) 48000
          },
          {
            (byte) 3,
            (object) 2
          },
          {
            (byte) 4,
            (object) 20000
          },
          {
            (byte) 5,
            (object) 30000
          },
          {
            (byte) 10,
            (object) null
          },
          {
            (byte) 11,
            (object) (byte) 0
          },
          {
            (byte) 12,
            (object) (byte) 11
          }
        }
      }
    }, raiseEventOptions, sendOptions2);
  }

  public static void DeafenAll()
  {
    for (int index = 0; index < 2; ++index)
      Overpowered.DeafenPlayer((object) (ReceiverGroup) 0);
  }

  public static void DeafenGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    for (int index = 0; index < 2; ++index)
      Overpowered.DeafenPlayer((object) new int[1]
      {
        GunLib.lockedTargetRig.Creator.ActorNumber
      });
  }

  public static void TouchToDeafen()
  {
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
      {
        Transform[] targets = new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        };
        if (RigManager.HandsTouchTargets(hands, targets))
        {
          Overpowered.DeafenPlayer((object) new int[1]
          {
            activeRig.Creator.ActorNumber
          });
          break;
        }
      }
    }
  }

  public static void GetTouchedToDeafen()
  {
    Transform[] targets = new Transform[2]
    {
      Variables.taggerInstance.offlineVRRig.headMesh.transform,
      Variables.taggerInstance.offlineVRRig.bodyTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isMyPlayer)
      {
        if (RigManager.HandsTouchTargets(new Transform[2]
        {
          activeRig.leftHandTransform,
          activeRig.rightHandTransform
        }, targets))
        {
          Overpowered.DeafenPlayer((object) new int[1]
          {
            activeRig.Creator.ActorNumber
          });
          break;
        }
      }
    }
  }

  public static void DestroyGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(GunLib.lockedTargetRig.Creator.ActorNumber);
  }

  public static void DestroyAll()
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if ((UnityEngine.Object) activeRig == (UnityEngine.Object) null || activeRig.isOfflineVRRig || activeRig.isLocal)
        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(activeRig.Creator.ActorNumber);
    }
  }

  public static void AddInfected(NetPlayer plr)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null || plr == null)
      return;
    switch (GorillaGameManager.instance.GameType() - 1)
    {
      case 0:
      case (GorillaGameModes.GameModeType) 4:
      case (GorillaGameModes.GameModeType) 8:
      case (GorillaGameModes.GameModeType) 9:
      case (GorillaGameModes.GameModeType) 10:
        if (!(GorillaGameManager.instance is GorillaTagManager instance1))
          break;
        if (instance1.isCurrentlyTag)
          instance1.ChangeCurrentIt(plr, true);
        else if (instance1.currentInfected != null && !instance1.currentInfected.Contains(plr))
          instance1.AddInfectedPlayer(plr, true);
        break;
      case (GorillaGameModes.GameModeType) 2:
        if (!(GorillaGameManager.instance is GorillaPaintbrawlManager instance2) || instance2.playerLives == null)
          break;
        instance2.playerLives[plr.ActorNumber] = 0;
        break;
      case (GorillaGameModes.GameModeType) 3:
      case (GorillaGameModes.GameModeType) 5:
        if (!(GorillaGameManager.instance is GorillaAmbushManager instance3))
          break;
        if (((GorillaTagManager) instance3).isCurrentlyTag)
          ((GorillaTagManager) instance3).ChangeCurrentIt(plr, true);
        else if (((GorillaTagManager) instance3).currentInfected != null && !((GorillaTagManager) instance3).currentInfected.Contains(plr))
          ((GorillaTagManager) instance3).AddInfectedPlayer(plr, true);
        break;
    }
  }

  public static void RemoveInfected(NetPlayer plr)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null || plr == null)
      return;
    switch (GorillaGameManager.instance.GameType() - 1)
    {
      case 0:
      case (GorillaGameModes.GameModeType) 4:
      case (GorillaGameModes.GameModeType) 8:
      case (GorillaGameModes.GameModeType) 9:
      case (GorillaGameModes.GameModeType) 10:
        if (!(GorillaGameManager.instance is GorillaTagManager instance1))
          break;
        if (instance1.isCurrentlyTag)
        {
          if (instance1.currentIt == plr)
            instance1.currentIt = (NetPlayer) null;
        }
        else if (instance1.currentInfected != null && instance1.currentInfected.Contains(plr))
          instance1.currentInfected.Remove(plr);
        break;
      case (GorillaGameModes.GameModeType) 2:
        if (!(GorillaGameManager.instance is GorillaPaintbrawlManager instance2) || instance2.playerLives == null)
          break;
        instance2.playerLives[plr.ActorNumber] = 3;
        break;
      case (GorillaGameModes.GameModeType) 3:
      case (GorillaGameModes.GameModeType) 5:
        if (!(GorillaGameManager.instance is GorillaAmbushManager instance3))
          break;
        if (((GorillaTagManager) instance3).isCurrentlyTag)
        {
          if (((GorillaTagManager) instance3).currentIt == plr)
            ((GorillaTagManager) instance3).currentIt = (NetPlayer) null;
        }
        else if (((GorillaTagManager) instance3).currentInfected != null && ((GorillaTagManager) instance3).currentInfected.Contains(plr))
          ((GorillaTagManager) instance3).currentInfected.Remove(plr);
        break;
    }
  }

  public static void AddRock(NetPlayer plr)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null || plr == null)
      return;
    switch (GorillaGameManager.instance.GameType() - 1)
    {
      case 0:
      case (GorillaGameModes.GameModeType) 4:
      case (GorillaGameModes.GameModeType) 8:
      case (GorillaGameModes.GameModeType) 9:
      case (GorillaGameModes.GameModeType) 10:
        if (!(GorillaGameManager.instance is GorillaTagManager instance1))
          break;
        instance1.ChangeCurrentIt(plr, true);
        break;
      case (GorillaGameModes.GameModeType) 2:
        if (!(GorillaGameManager.instance is GorillaPaintbrawlManager instance2) || instance2.playerLives == null)
          break;
        instance2.playerLives[plr.ActorNumber] = 0;
        break;
      case (GorillaGameModes.GameModeType) 3:
      case (GorillaGameModes.GameModeType) 5:
        if (!(GorillaGameManager.instance is GorillaAmbushManager instance3))
          break;
        ((GorillaTagManager) instance3).ChangeCurrentIt(plr, true);
        break;
    }
  }

  public static void RemoveRock(NetPlayer plr)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null || plr == null)
      return;
    switch (GorillaGameManager.instance.GameType() - 1)
    {
      case 0:
      case (GorillaGameModes.GameModeType) 4:
      case (GorillaGameModes.GameModeType) 8:
      case (GorillaGameModes.GameModeType) 9:
      case (GorillaGameModes.GameModeType) 10:
        if (!(GorillaGameManager.instance is GorillaTagManager instance1) || instance1.currentIt != plr)
          break;
        instance1.ChangeCurrentIt((NetPlayer) null, true);
        break;
      case (GorillaGameModes.GameModeType) 2:
        if (!(GorillaGameManager.instance is GorillaPaintbrawlManager instance2) || instance2.playerLives == null)
          break;
        instance2.playerLives[plr.ActorNumber] = 3;
        break;
      case (GorillaGameModes.GameModeType) 3:
      case (GorillaGameModes.GameModeType) 5:
        if (!(GorillaGameManager.instance is GorillaAmbushManager instance3) || ((GorillaTagManager) instance3).currentIt != plr)
          break;
        ((GorillaTagManager) instance3).ChangeCurrentIt((NetPlayer) null, true);
        break;
    }
  }

  public static void MaterialTarget(VRRig rig)
  {
    if (!NetworkSystem.Instance.IsMasterClient)
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're Not Master Client");
    }
    else
    {
      NetPlayer netPlayerFromVrRig = RigManager.GetNetPlayerFromVRRig(rig);
      int num;
      Overpowered.materialState.TryGetValue(rig, out num);
      ++num;
      num %= 4;
      if (GorillaGameManager.instance.GameType() == 0 && num < 4)
        num = 4;
      Overpowered.materialState[rig] = num;
      switch (num)
      {
        case 0:
          Overpowered.AddInfected(netPlayerFromVrRig);
          break;
        case 1:
          Overpowered.RemoveInfected(netPlayerFromVrRig);
          break;
        case 2:
          Overpowered.AddRock(netPlayerFromVrRig);
          break;
        case 3:
          Overpowered.RemoveRock(netPlayerFromVrRig);
          break;
      }
    }
  }

  public static void MatSpamGun()
  {
    if (!Variables.IsMaster() || !GunLib.SetupLockOnGun() || (double) Time.time <= (double) Overpowered.materialDelay)
      return;
    Overpowered.materialDelay = Time.time + 0.1f;
    Overpowered.MaterialTarget(GunLib.lockedTargetRig);
  }

  public static void MatSpamAll()
  {
    if (!Variables.IsMaster() || (double) Time.time <= (double) Overpowered.materialDelay)
      return;
    Overpowered.materialDelay = Time.time + 0.1f;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      Overpowered.MaterialTarget(activeRig);
  }

  public static void TouchToMatSpam()
  {
    if (!Variables.IsMaster())
      return;
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        if (RigManager.HandsTouchTargets(hands, new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        }))
        {
          if ((double) Time.time <= (double) Overpowered.materialDelay)
            break;
          Overpowered.materialDelay = Time.time + 0.1f;
          Overpowered.MaterialTarget(activeRig);
          break;
        }
      }
    }
  }

  public static void GetTouchedToMatSpam()
  {
    if (!Variables.IsMaster())
      return;
    Transform[] targets = new Transform[2]
    {
      Variables.taggerInstance.offlineVRRig.headMesh.transform,
      Variables.taggerInstance.offlineVRRig.bodyTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isMyPlayer)
      {
        if (RigManager.HandsTouchTargets(new Transform[2]
        {
          activeRig.leftHandTransform,
          activeRig.rightHandTransform
        }, targets))
        {
          if ((double) Time.time <= (double) Overpowered.materialDelay)
            break;
          Overpowered.materialDelay = Time.time + 0.1f;
          Overpowered.MaterialTarget(activeRig);
          break;
        }
      }
    }
  }

  private static void SendStatusEffect(
    RoomSystem.StatusEffects statusEffect,
    RaiseEventOptions options)
  {
    if (!Variables.IsMaster())
      return;
    object[] objArray = new object[1]
    {
      (object) (int) statusEffect
    };
    PhotonNetwork.RaiseEvent((byte) 3, (object) new object[3]
    {
      (object) NetworkSystem.Instance.ServerTimestamp,
      (object) (byte) 2,
      (object) objArray
    }, options, SendOptions.SendUnreliable);
    Safety.RPCShield();
  }

  public static void SlowGun()
  {
    if (!Variables.IsMaster() || !GunLib.SetupLockOnGun() || (double) Time.time <= (double) Overpowered.slowDelay)
      return;
    Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 0, new RaiseEventOptions()
    {
      TargetActors = new int[1]
      {
        GunLib.lockedTargetRig.Creator.ActorNumber
      }
    });
    Overpowered.slowDelay = Time.time + 1f;
  }

  public static void SlowAll()
  {
    if (!Variables.IsMaster() || !InputHandler.RTrigger() || (double) Time.time <= (double) Overpowered.slowDelay)
      return;
    Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 0, new RaiseEventOptions()
    {
      Receivers = (ReceiverGroup) 0
    });
    Overpowered.slowDelay = Time.time + 1f;
  }

  public static void SlowAura()
  {
    if (!Variables.IsMaster() || !PhotonNetwork.InRoom || (double) Time.time < (double) Overpowered.slowDelay)
      return;
    Vector3 position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal && (double) Vector3.Distance(position, ((Component) activeRig).transform.position) < 4.0)
      {
        Overpowered.slowDelay = Time.time + 1f;
        Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 0, new RaiseEventOptions()
        {
          TargetActors = new int[1]
          {
            activeRig.Creator.ActorNumber
          }
        });
        break;
      }
    }
  }

  public static void SlowOnTouch()
  {
    if (!Variables.IsMaster() || !PhotonNetwork.InRoom || (double) Time.time < (double) Overpowered.slowDelay)
      return;
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        if (RigManager.HandsTouchTargets(hands, new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        }))
        {
          Overpowered.slowDelay = Time.time + 1f;
          Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 0, new RaiseEventOptions()
          {
            TargetActors = new int[1]
            {
              activeRig.Creator.ActorNumber
            }
          });
          break;
        }
      }
    }
  }

  public static void VibrateGun()
  {
    if (!Variables.IsMaster() || !GunLib.SetupLockOnGun() || (double) Time.time <= (double) Overpowered.vibrateDelay)
      return;
    Overpowered.vibrateDelay = Time.time + 0.5f;
    Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 1, new RaiseEventOptions()
    {
      TargetActors = new int[1]
      {
        GunLib.lockedTargetRig.Creator.ActorNumber
      }
    });
  }

  public static void VibrateAll()
  {
    if (!Variables.IsMaster() || !InputHandler.RTrigger() || (double) Time.time <= (double) Overpowered.vibrateDelay)
      return;
    Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 1, new RaiseEventOptions()
    {
      Receivers = (ReceiverGroup) 0
    });
    Overpowered.vibrateDelay = Time.time + 0.5f;
  }

  public static void VibrateAura()
  {
    if (!Variables.IsMaster() || !PhotonNetwork.InRoom || (double) Time.time < (double) Overpowered.vibrateDelay)
      return;
    Vector3 position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal && (double) Vector3.Distance(position, ((Component) activeRig).transform.position) < 4.0)
      {
        Overpowered.vibrateDelay = Time.time + 0.5f;
        Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 1, new RaiseEventOptions()
        {
          TargetActors = new int[1]
          {
            activeRig.Creator.ActorNumber
          }
        });
        break;
      }
    }
  }

  public static void VibrateOnTouch()
  {
    if (!Variables.IsMaster() || !PhotonNetwork.InRoom || (double) Time.time < (double) Overpowered.vibrateDelay)
      return;
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        if (RigManager.HandsTouchTargets(hands, new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        }))
        {
          Overpowered.vibrateDelay = Time.time + 0.5f;
          Overpowered.SendStatusEffect((RoomSystem.StatusEffects) 1, new RaiseEventOptions()
          {
            TargetActors = new int[1]
            {
              activeRig.Creator.ActorNumber
            }
          });
          break;
        }
      }
    }
  }

  public static void SpazAllTargets()
  {
    if (!Variables.IsMaster())
      return;
    foreach (HitTargetNetworkState targetNetworkState in Variables.GetAllType<HitTargetNetworkState>())
    {
      targetNetworkState.hitCooldownTime = 0;
      targetNetworkState.TargetHit(Vector3.zero, Vector3.zero);
    }
  }

  public static void SpyRoom(bool enable)
  {
    GorillaTagger.Instance.myRecorder.IsRecording = !enable;
    if (!enable || !PhotonNetwork.InRoom)
      return;
    ((Component) VRRig.LocalRig).transform.transform.position = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
  }

  public static void SpamEvent()
  {
    if (!Variables.IsMaster() || (double) Time.time - (double) Overpowered.lastToggleTime < 0.10000000149011612)
      return;
    Overpowered.lastToggleTime = Time.time;
    if (GreyZoneManager.Instance.greyZoneActive)
      GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
    else
      GreyZoneManager.Instance.ActivateGreyZoneAuthority();
  }

  public static void TagLag(bool enable)
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaTagManager) GorillaGameManager.instance).tagCoolDown = enable ? float.MaxValue : 5f;
  }

  public static void RoomSoundSpam(int id)
  {
    if (!Variables.IsMaster() || !InputHandler.RTrigger() || (double) Time.time <= (double) Overpowered.roomSoundDelay)
      return;
    object[] objArray1 = new object[3]
    {
      (object) PhotonNetwork.ServerTimestamp,
      (object) (byte) 3,
      (object) new object[3]
      {
        (object) id,
        (object) 99999f,
        (object) false
      }
    };
    try
    {
      object[] objArray2 = objArray1;
      RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
      raiseEventOptions.Receivers = (ReceiverGroup) 1;
      SendOptions sendUnreliable = SendOptions.SendUnreliable;
      PhotonNetwork.RaiseEvent((byte) 3, (object) objArray2, raiseEventOptions, sendUnreliable);
    }
    catch
    {
    }
    Safety.RPCShield();
    Overpowered.roomSoundDelay = Time.time + 0.1f;
  }

  public static void EarrapeGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    if (!Overpowered._earrapeGunActive)
    {
      Sound.Earrape(true);
      Overpowered._earrapeGunActive = true;
    }
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal && activeRig.Creator.ActorNumber != GunLib.lockedTargetRig.Creator.ActorNumber)
        Overpowered.DeafenPlayer((object) new int[1]
        {
          activeRig.Creator.ActorNumber
        });
    }
    MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
    {
      if (!PhotonNetwork.InRoom)
        return true;
      PhotonView component = !((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null) || !((UnityEngine.Object) Variables.taggerInstance.myVRRig != (UnityEngine.Object) null) ? (PhotonView) null : ((Component) Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || (UnityEngine.Object) VRRig.LocalRig == (UnityEngine.Object) null)
        return true;
      if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null || GunLib.lockedTargetRig.Creator == null)
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        return true;
      }
      PhotonSerializer.BroadcastViews(true, new PhotonView[1]
      {
        component
      });
      PhotonSerializer.RigPose rigPose = PhotonSerializer.RigPose.Capture(VRRig.LocalRig);
      try
      {
        ((Component) VRRig.LocalRig).transform.SetPositionAndRotation(((Component) GunLib.lockedTargetRig).transform.position, ((Component) GunLib.lockedTargetRig).transform.rotation);
        VRRig.LocalRig.head.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.head.rigTarget.position, GunLib.lockedTargetRig.head.rigTarget.rotation);
        VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.leftHand.rigTarget.position, GunLib.lockedTargetRig.leftHand.rigTarget.rotation);
        VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(GunLib.lockedTargetRig.rightHand.rigTarget.position, GunLib.lockedTargetRig.rightHand.rigTarget.rotation);
        PhotonSerializer.SendSerialize(component, new RaiseEventOptions()
        {
          TargetActors = new int[1]
          {
            GunLib.lockedTargetRig.Creator.ActorNumber
          }
        });
        Safety.RPCShield();
      }
      finally
      {
        rigPose.Restore(VRRig.LocalRig);
      }
      return false;
    });
  }

  public static void EarrapeAll()
  {
    Sound.Earrape(true);
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.SetPositionAndRotation(((Component) targetRig).transform.position, ((Component) targetRig).transform.rotation);
      VRRig.LocalRig.head.rigTarget.SetPositionAndRotation(targetRig.head.rigTarget.position, targetRig.head.rigTarget.rotation);
      VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(targetRig.leftHand.rigTarget.position, targetRig.leftHand.rigTarget.rotation);
      VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(targetRig.rightHand.rigTarget.position, targetRig.rightHand.rigTarget.rotation);
    }));
  }

  public static void BetaNearbyFollowCommand(GorillaFriendCollider friendCollider, Photon.Realtime.Player player)
  {
    PhotonNetworkController.Instance.FriendIDList.Add(player.UserId);
    object[] objArray = new object[2]
    {
      (object) PhotonNetworkController.Instance.shuffler,
      (object) PhotonNetworkController.Instance.keyStr
    };
    NetEventOptions netEventOptions = new NetEventOptions()
    {
      TargetActors = new int[1]{ player.ActorNumber }
    };
    if (friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && friendCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
    {
      RoomSystem.SendEvent((byte) 4, objArray, ref netEventOptions, false);
    }
    else
    {
      if (friendCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
        return;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In Stump");
    }
  }

  public static IEnumerator StumpKickDelay(
    Action action,
    Action action2,
    float extraDelay = 0.0f,
    bool changeQueue = false)
  {
    PhotonNetworkController.Instance.FriendIDList.Clear();
    yield return (object) new WaitForSeconds(extraDelay);
    bool joinedRoomPatchEnabled = MenuPatches.JoinedRoomPatch.enabled;
    string queueArchive = GorillaComputer.instance.currentQueue;
    if (changeQueue)
      GorillaComputer.instance.currentQueue = Room.RandomString();
    Action action1 = action;
    if (action1 != null)
      action1();
    yield return (object) new WaitForSeconds(0.3f);
    Action action3 = action2;
    if (action3 != null)
      action3();
    yield return (object) new WaitForSeconds(1f);
    if (changeQueue)
      GorillaComputer.instance.currentQueue = queueArchive;
    yield return (object) new WaitForSeconds(15f);
    MenuPatches.JoinedRoomPatch.enabled = joinedRoomPatchEnabled;
  }

  public static void StumpKickGun()
  {
    if (!GunLib.GunGrips)
    {
      GunLib.SetGunVisibility(false);
      GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers && (double) Time.time > (double) Overpowered.kickDelay)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
        {
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        }
        else
        {
          NetPlayer player = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
          Overpowered.kickDelay = Time.time + 0.5f;
          if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId))
            NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Photon.Realtime.Player Must Be In Stump");
          else if (!NetworkSystem.Instance.SessionIsPrivate)
            NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In A Private Room");
          else
            CoroutineHelper.Instance.StartCoroutine(Overpowered.StumpKickDelay((Action) (() =>
            {
              PhotonNetworkController instance = PhotonNetworkController.Instance;
              int num = UnityEngine.Random.Range(0, 99);
              string str1 = num.ToString().PadLeft(2, '0');
              num = UnityEngine.Random.Range(0, 99999999);
              string str2 = num.ToString().PadLeft(8, '0');
              string str3 = str1 + str2;
              instance.shuffler = str3;
              PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
              Overpowered.BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, RigManager.NetPlayerToPhotonPlayer(player));
              Safety.RPCShield();
            }), (Action) (() => Room.CreateRoom(Overpowered.specificRoom ?? Room.RandomString(), false, roomJoinType: (JoinType) 1))));
        }
      }
      else
      {
        if (GunLib.GunTriggers)
          return;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
  }

  public static void StumpKickAll()
  {
    if (PhotonNetwork.InRoom)
    {
      if (!NetworkSystem.Instance.SessionIsPrivate)
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You Must Be In A Private Room");
      else
        CoroutineHelper.Instance.StartCoroutine(Overpowered.StumpKickDelay((Action) (() =>
        {
          PhotonNetworkController instance = PhotonNetworkController.Instance;
          int num = UnityEngine.Random.Range(0, 99);
          string str1 = num.ToString().PadLeft(2, '0');
          num = UnityEngine.Random.Range(0, 99999999);
          string str2 = num.ToString().PadLeft(8, '0');
          string str3 = str1 + str2;
          instance.shuffler = str3;
          PhotonNetworkController.Instance.keyStr = UnityEngine.Random.Range(0, 99999999).ToString().PadLeft(8, '0');
          foreach (VRRig vrrig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (rig => !rig.IsRigLocal() && GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(rig.netView.Owner.UserId))))
            Overpowered.BetaNearbyFollowCommand(GorillaComputer.instance.friendJoinCollider, RigManager.NetPlayerToPhotonPlayer(RigManager.GetNetPlayerFromVRRig(vrrig)));
          Safety.RPCShield();
        }), (Action) (() => Room.CreateRoom(Overpowered.specificRoom ?? Room.RandomString(), false, roomJoinType: (JoinType) 1))));
    }
    else
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
  }

  public static void SetKickRoom()
  {
    if (SearchAndKeyboard.isSearching || Overpowered._kickRoomSet)
      return;
    SearchAndKeyboard.OpenTypingKeyboard(Overpowered.specificRoom ?? "", "Enter room code...");
    SearchAndKeyboard.onTypingComplete = (Action<string>) (code =>
    {
      Overpowered._kickRoomSet = true;
      ButtonHandler.Button button = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && (b.buttonText?.StartsWith("Kick To Specific Room")).GetValueOrDefault()));
      if (string.IsNullOrEmpty(code))
      {
        Overpowered.specificRoom = (string) null;
        button?.SetText("Kick To Specific Room");
        NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Kick Room Cleared");
      }
      else
      {
        Overpowered.specificRoom = code;
        button?.SetText("Kick To Specific Room : " + code);
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"Kick Room Set To `{code}`");
      }
    });
    SearchAndKeyboard.onTypingCancelled = (Action) (() =>
    {
      ButtonHandler.Button button = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && (b.buttonText?.StartsWith("Kick To Specific Room")).GetValueOrDefault()));
      if (button != null)
        button.Enabled = false;
      Overpowered.specificRoom = (string) null;
      Overpowered._kickRoomSet = false;
      button?.SetText("Kick To Specific Room");
    });
  }

  public static void ClearKickRoom()
  {
    Overpowered.specificRoom = (string) null;
    Overpowered._kickRoomSet = false;
    if (SearchAndKeyboard.isSearching)
      SearchAndKeyboard.CloseTypingKeyboard(true);
    ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && (b.buttonText?.StartsWith("Kick To Specific Room")).GetValueOrDefault()))?.SetText("Kick To Specific Room");
    NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, "Kick Room Cleared");
  }

  private static void EnableGrabSync(VRRig target, Vector3 pos)
  {
    Overpowered.grabSyncTarget = target;
    Overpowered.grabSyncPos = pos;
    MenuPatches.SerializationPatch.Override = new Func<bool>(Overpowered.GrabSyncOverride);
  }

  public static void StopGrabGun()
  {
    Overpowered.grabSyncTarget = (VRRig) null;
    if (MenuPatches.SerializationPatch.Override == new Func<bool>(Overpowered.GrabSyncOverride))
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
    MenuPatches.GrabPatch.enabled = false;
    if ((UnityEngine.Object) VRRig.LocalRig != (UnityEngine.Object) null)
    {
      VRRig.LocalRig.BreakHandLinks();
      ((Behaviour) VRRig.LocalRig).enabled = true;
    }
    Overpowered.ClearGrabLines();
  }

  private static bool GrabSyncOverride()
  {
    if (!PhotonNetwork.InRoom)
      return true;
    if (!MenuPatches.GrabPatch.enabled || (UnityEngine.Object) Overpowered.grabSyncTarget == (UnityEngine.Object) null || Overpowered.grabSyncTarget.Creator == null || (UnityEngine.Object) Variables.taggerInstance.myVRRig == (UnityEngine.Object) null)
    {
      Overpowered.grabSyncTarget = (VRRig) null;
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      return true;
    }
    PhotonSerializer.BroadcastViews(true, new PhotonView[1]
    {
      Variables.taggerInstance.myVRRig.GetView
    });
    Vector3 position = ((Component) VRRig.LocalRig).transform.position;
    bool enabled = ((Behaviour) VRRig.LocalRig).enabled;
    ((Behaviour) VRRig.LocalRig).enabled = false;
    ((Component) VRRig.LocalRig).transform.position = Overpowered.grabSyncPos;
    PhotonSerializer.SendSerialize(Variables.taggerInstance.myVRRig.GetView, new RaiseEventOptions()
    {
      TargetActors = new int[1]
      {
        Overpowered.grabSyncTarget.Creator.ActorNumber
      }
    });
    ((Component) VRRig.LocalRig).transform.position = position;
    ((Behaviour) VRRig.LocalRig).enabled = enabled;
    Safety.RPCShield();
    return false;
  }

  public static void ForceGrabTarget(VRRig target, Vector3 pos)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) null || target.isLocal)
      return;
    bool flag = target.leftHandLink.CanBeGrabbed();
    if (!flag && !target.rightHandLink.CanBeGrabbed())
    {
      MenuPatches.GrabPatch.enabled = false;
      VRRig.LocalRig.BreakHandLinks();
      ((Behaviour) VRRig.LocalRig).enabled = true;
    }
    else
    {
      MenuPatches.GrabPatch.enabled = true;
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = pos;
      Overpowered.EnableGrabSync(target, pos);
      TakeMyHand_HandLink takeMyHandHandLink1 = flag ? target.leftHandLink : target.rightHandLink;
      TakeMyHand_HandLink takeMyHandHandLink2 = flag ? VRRig.LocalRig.leftHandLink : VRRig.LocalRig.rightHandLink;
      if (takeMyHandHandLink1.grabbedPlayer == NetworkSystem.Instance.LocalPlayer)
        return;
      if ((double) Overpowered.grabTimer <= 0.0)
        Overpowered.grabTimer = (double) takeMyHandHandLink1.rejectGrabsUntilTimestamp > (double) Time.time ? takeMyHandHandLink1.rejectGrabsUntilTimestamp : Time.time + 0.2f;
      if ((double) Time.time <= (double) Overpowered.grabTimer)
        return;
      ((Component) VRRig.LocalRig).transform.position = target.syncPos;
      takeMyHandHandLink2.TentacleTryCreateLink(takeMyHandHandLink1);
      Overpowered.grabTimer = (double) takeMyHandHandLink1.rejectGrabsUntilTimestamp > (double) Time.time ? takeMyHandHandLink1.rejectGrabsUntilTimestamp : Time.time + 0.2f;
    }
  }

  public static void IsGrabbableTracer()
  {
    if ((UnityEngine.Object) GorillaTagger.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GorillaTagger.Instance.offlineVRRig == (UnityEngine.Object) null)
    {
      foreach (LineRenderer grabLine in Overpowered.grabLines)
      {
        if ((UnityEngine.Object) grabLine != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) grabLine.gameObject);
      }
      Overpowered.grabLines.Clear();
    }
    else
    {
      Vector3 position = ((Component) GorillaTagger.Instance.offlineVRRig).transform.position;
      int index1 = 0;
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isMyPlayer && !activeRig.isOfflineVRRig && (activeRig.leftHandLink.CanBeGrabbed() || activeRig.rightHandLink.CanBeGrabbed()))
        {
          while (Overpowered.grabLines.Count <= index1)
          {
            LineRenderer lineRenderer = new GameObject("GrabLine").AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Variables.guiShader);
            lineRenderer.startWidth = lineRenderer.endWidth = 0.015f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            Overpowered.grabLines.Add(lineRenderer);
          }
          LineRenderer grabLine = Overpowered.grabLines[index1];
          grabLine.enabled = true;
          grabLine.startColor = grabLine.endColor = Color.blue;
          grabLine.SetPosition(0, position);
          grabLine.SetPosition(1, ((Component) activeRig).transform.position);
          ++index1;
        }
      }
      for (int index2 = index1; index2 < Overpowered.grabLines.Count; ++index2)
      {
        if ((UnityEngine.Object) Overpowered.grabLines[index2] != (UnityEngine.Object) null)
          Overpowered.grabLines[index2].enabled = false;
      }
    }
  }

  public static void ClearGrabLines()
  {
    foreach (LineRenderer grabLine in Overpowered.grabLines)
    {
      if ((UnityEngine.Object) grabLine != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) grabLine.gameObject);
    }
    Overpowered.grabLines.Clear();
  }

  public static bool NotGrabbing()
  {
    return !GunLib.GunGrips && !GunLib.GunTriggers && !InputHandler.RGrip() && !InputHandler.LGrip() && !InputHandler.RTrigger() && !InputHandler.LTrigger();
  }

  private static Vector3 BringPos(VRRig r)
  {
    return ((Component) VRRig.LocalRig).transform.position - ((Component) VRRig.LocalRig).transform.forward * 10f;
  }

  private static Vector3 FlingPos(VRRig r)
  {
    return ((Component) r).transform.position + new Vector3(4f, 0.0f, 0.0f);
  }

  private static Vector3 ShakePos(VRRig r)
  {
    return ((Component) r).transform.position + new Vector3(UnityEngine.Random.Range(0, 2) == 1 ? 4f : -4f, 0.0f, 0.0f);
  }

  private static Vector3 CrashPos(VRRig r) => Vector3.down * (float) int.MaxValue;

  private static Vector3 MetroPos(VRRig r)
  {
    Vector3 normalized = (new Vector3(137.0344f, 66.6208f, -42.8385f) - ((Component) r).transform.position).normalized;
    return ((Component) r).transform.position + Vector3.up * 200f + normalized * 400f;
  }

  private static Vector3 BreakMovePos(VRRig r) => r.syncPos;

  private static void RunGrabGun(Func<VRRig, Vector3> posFor)
  {
    Overpowered.IsGrabbableTracer();
    if (GunLib.SetupLockOnGun())
      Overpowered.ForceGrabTarget(GunLib.lockedTargetRig, posFor(GunLib.lockedTargetRig));
    else
      ((Behaviour) VRRig.LocalRig).enabled = true;
    if (!Overpowered.NotGrabbing())
      return;
    MenuPatches.GrabPatch.enabled = false;
    VRRig.LocalRig.BreakHandLinks();
    ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  private static void RunGrabAll(Func<VRRig, Vector3> posFor)
  {
    Overpowered.IsGrabbableTracer();
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isMyPlayer && !activeRig.isOfflineVRRig && (activeRig.leftHandLink.CanBeGrabbed() || activeRig.rightHandLink.CanBeGrabbed()))
      {
        Overpowered.ForceGrabTarget(activeRig, posFor(activeRig));
        return;
      }
    }
    MenuPatches.GrabPatch.enabled = false;
    VRRig.LocalRig.BreakHandLinks();
    ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  public static void GrabBringGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.BringPos));
  }

  public static void GrabBringAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.BringPos));
  }

  public static void GrabFlingGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.FlingPos));
  }

  public static void GrabFlingAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.FlingPos));
  }

  public static void GrabShakeGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.ShakePos));
  }

  public static void GrabShakeAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.ShakePos));
  }

  public static void TryGrabCrashGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.CrashPos));
  }

  public static void GrabCrashAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.CrashPos));
  }

  public static void GrabMetroAppQuitGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.MetroPos));
  }

  public static void GrabMetroCrashAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.MetroPos));
  }

  public static void GrabBreakMovementGun()
  {
    Overpowered.RunGrabGun(new Func<VRRig, Vector3>(Overpowered.BreakMovePos));
  }

  public static void GrabBreakMovementAll()
  {
    Overpowered.RunGrabAll(new Func<VRRig, Vector3>(Overpowered.BreakMovePos));
  }
}
