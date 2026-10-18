
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaTagScripts;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable
namespace NXO.Mods.Categories;

public class Gamemode
{
  public static float autoGuardianDelay;
  private static float guardianGrabDelay;
  private static float guardianReleaseDelay;
  public static float balloonSpamDelay;
  public static int paintbrawlKillIndex;
  public static readonly Dictionary<int, float> paintbrawlKillDelays = new Dictionary<int, float>();

  public static void TagAll()
  {
    if (!InputHandler.RTrigger())
      return;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      Gamemode.TagPlayer(activeRig);
  }

  public static void TagGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    Gamemode.TagPlayer(GunLib.lockedTargetRig);
  }

  public static void TagPlayer(VRRig rig)
  {
    if ((UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) rig == (UnityEngine.Object) null || (UnityEngine.Object) rig == (UnityEngine.Object) VRRig.LocalRig || RigManager.RigIsInfected(rig) || !RigManager.RigIsInfected(VRRig.LocalRig))
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) rig).transform.position;
      PhotonSerializer.ForceSerialization();
      PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget) 0, new object[1]
      {
        (object) rig.Creator.ActorNumber
      });
      PhotonNetwork.SendAllOutgoingCommands();
      ((PhotonPeer) PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
      Safety.RPCShield();
    }
  }

  public static void TagAura()
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (RigManager.RigIsInfected(VRRig.LocalRig) && !RigManager.RigIsInfected(activeRig) && (double) Vector3.Distance(activeRig.bodyTransform.position, ((Component) Variables.taggerInstance.bodyCollider).transform.position) < 4.0)
      {
        Variables.playerInstance.RightHand.controllerTransform.position = activeRig.headMesh.transform.position;
        PhotonSerializer.ForceSerialization();
        PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget) 0, new object[1]
        {
          (object) activeRig.Creator.ActorNumber
        });
        PhotonNetwork.SendAllOutgoingCommands();
        ((PhotonPeer) PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
      }
    }
  }

  public static void FlickTag()
  {
    Physics.Raycast(Mouse.current.rightButton.isPressed || Mouse.current.leftButton.isPressed ? Variables.thirdPersonCamera.GetComponent<Camera>().ScreenPointToRay((Vector3) ((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue()) : new Ray(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.RightHand.controllerTransform.forward), out GunLib.raycastHit, 100f);
    GunLib.SetupGunObjectPositions((GunLib.raycastHit).point);
    GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
    if (!InputHandler.RTrigger() || !RigManager.RigIsInfected(VRRig.LocalRig))
      return;
    Variables.playerInstance.RightHand.controllerTransform.position = (GunLib.raycastHit).point;
    PhotonSerializer.ForceSerialization();
    PunExtensions.GetPhotonView(GameObject.Find("Photon.Realtime.Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget) 0, new object[1]
    {
      (object) GunLib.potentialTargetRig.Creator.ActorNumber
    });
    PhotonNetwork.SendAllOutgoingCommands();
    ((PhotonPeer) PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
  }

  public static void AntiTag()
  {
    if (RigManager.RigIsInfected(VRRig.LocalRig))
      return;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (RigManager.RigIsInfected(activeRig))
      {
        if ((double) Vector3.Distance(((Component) Variables.taggerInstance.offlineVRRig).transform.position, ((Component) activeRig).transform.position) < 3.0)
        {
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
        }
        else
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
      }
    }
  }

  public static void TagSelf()
  {
    if (!RigManager.RigIsInfected(VRRig.LocalRig))
    {
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (RigManager.RigIsInfected(activeRig))
        {
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = activeRig.rightHandTransform.position;
          Vector3 position1 = ((Component) activeRig).transform.position;
          Vector3 position2 = Variables.taggerInstance.offlineVRRig.head.rigTarget.position;
          if ((double) Vector3.Distance(position1, position2) < 1.6670000553131104)
            Variables.playerInstance.LeftHand.controllerTransform.position = position1;
        }
      }
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void UntagSelf()
  {
    if (!PhotonNetwork.InRoom)
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Not In A Room");
    else if (!RigManager.RigIsInfected(VRRig.LocalRig))
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're Not Tagged");
    }
    else
    {
      Room.Reconnect();
      Gamemode.NoTagOnJoin(true);
    }
  }

  public static void NoTagOnJoin(bool setActive)
  {
    Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
    if (customProperties != null && ((Dictionary<object, object>) customProperties).ContainsKey((object) "didTutorial") && customProperties[(object) "didTutorial"] is bool flag && flag == !setActive)
      return;
    Hashtable hashtable = new Hashtable();
    ((Dictionary<object, object>) hashtable).Add((object) "didTutorial", (object) !setActive);
    PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, (Hashtable) null, (WebFlags) null);
    PlayerPrefs.SetString("didTutorial", setActive ? "" : "done");
    PlayerPrefs.Save();
  }

  public static void RemoveInfected(NetPlayer plr)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null)
      return;
    switch (GorillaGameManager.instance.GameType() - 1)
    {
      case 0:
      case (GorillaGameModes.GameModeType) 4:
      case (GorillaGameModes.GameModeType) 8:
      case (GorillaGameModes.GameModeType) 9:
      case (GorillaGameModes.GameModeType) 10:
        GorillaTagManager instance1 = (GorillaTagManager) GorillaGameManager.instance;
        if (instance1.isCurrentlyTag && instance1.currentIt == plr)
        {
          instance1.currentIt = (NetPlayer) null;
          break;
        }
        if (instance1.isCurrentlyTag || !instance1.currentInfected.Contains(plr))
          break;
        instance1.currentInfected.Remove(plr);
        break;
      case (GorillaGameModes.GameModeType) 2:
        ((GorillaPaintbrawlManager) GorillaGameManager.instance).playerLives[plr.ActorNumber] = 3;
        break;
      case (GorillaGameModes.GameModeType) 3:
      case (GorillaGameModes.GameModeType) 5:
        GorillaAmbushManager instance2 = (GorillaAmbushManager) GorillaGameManager.instance;
        if (((GorillaTagManager) instance2).isCurrentlyTag && ((GorillaTagManager) instance2).currentIt == plr)
        {
          ((GorillaTagManager) instance2).currentIt = (NetPlayer) null;
          break;
        }
        if (((GorillaTagManager) instance2).isCurrentlyTag || !((GorillaTagManager) instance2).currentInfected.Contains(plr))
          break;
        ((GorillaTagManager) instance2).currentInfected.Remove(plr);
        break;
    }
  }

  public static void UntagAll()
  {
    if (!Variables.IsMaster())
      return;
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
      Gamemode.RemoveInfected(((NetPlayer) (player)));
  }

  public static void UntagGun()
  {
    if (!Variables.IsMaster())
      return;
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        else
          Gamemode.RemoveInfected(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig));
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void AlwaysGuardian()
  {
    if (!PhotonNetwork.InRoom || GorillaGameManager.instance.GameType() != (GameModeType) 8)
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    else if (((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      foreach (TappableGuardianIdol tappableGuardianIdol in Variables.GetAllType<TappableGuardianIdol>())
      {
        if ((bool) (UnityEngine.Object) ((NetworkSceneObject) ((Tappable) tappableGuardianIdol).manager)?.photonView && !tappableGuardianIdol.isChangingPositions)
        {
          GorillaGuardianZoneManager zoneManager = tappableGuardianIdol.zoneManager;
          if (zoneManager.IsZoneValid())
          {
            ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
            ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) tappableGuardianIdol).transform.position + UnityEngine.Random.insideUnitSphere * 0.1f;
            Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) tappableGuardianIdol).transform.position;
            Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) tappableGuardianIdol).transform.position;
            if ((double) Time.time <= (double) Gamemode.autoGuardianDelay)
              return;
            Gamemode.autoGuardianDelay = Time.time + ((double) zoneManager._currentActivationTime >= (double) zoneManager.requiredActivationTime - 1.0 ? 0.0f : 0.2f);
            ((Tappable) tappableGuardianIdol).OnTap(UnityEngine.Random.Range(0.0f, 1f));
            Safety.RPCShield();
            return;
          }
        }
      }
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
  }

  public static void SetPlayerVelocity(NetPlayer victim, Vector3 velocity)
  {
    if (victim == null)
      return;
    velocity = (double) velocity.sqrMagnitude > 400.0 ? velocity.normalized * 20f : velocity;
    if (!((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
    }
    else
    {
      VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(victim);
      if ((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null)
        return;
      RigManager.GetNetworkViewFromVRRig(rigFromNetPlayer).SendRPC("GrabbedByPlayer", victim, new object[3]
      {
        (object) true,
        (object) false,
        (object) false
      });
      RigManager.GetNetworkViewFromVRRig(rigFromNetPlayer).SendRPC("DroppedByPlayer", victim, new object[1]
      {
        (object) velocity
      });
      Safety.RPCShield();
    }
  }

  public static void SetAllPlayersVelocity(RpcTarget target, Vector3 velocity)
  {
    velocity = (double) velocity.sqrMagnitude > 400.0 ? velocity.normalized * 20f : velocity;
    if (!((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
    }
    else
    {
      if (true)
        ;
      IEnumerable<VRRig> source;
      switch ((int) target)
      {
        case 0:
          source = (IEnumerable<VRRig>) VRRigCache.ActiveRigs;
          break;
        case 1:
          source = VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal));
          break;
        case 2:
          source = (IEnumerable<VRRig>) new VRRig[1]
          {
            RigManager.GetVRRigFromNetPlayer(NetworkSystem.Instance.MasterClient)
          };
          break;
        default:
          source = Enumerable.Empty<VRRig>();
          break;
      }
      if (true)
        ;
      foreach (VRRig vrrig in source.Where<VRRig>((Func<VRRig, bool>) (r => (UnityEngine.Object) r != (UnityEngine.Object) null)))
      {
        NetPlayer netPlayerFromVrRig = RigManager.GetNetPlayerFromVRRig(vrrig);
        if (netPlayerFromVrRig != null)
        {
          RigManager.GetNetworkViewFromVRRig(vrrig).SendRPC("GrabbedByPlayer", netPlayerFromVrRig, new object[3]
          {
            (object) true,
            (object) false,
            (object) false
          });
          RigManager.GetNetworkViewFromVRRig(vrrig).SendRPC("DroppedByPlayer", netPlayerFromVrRig, new object[1]
          {
            (object) velocity
          });
        }
      }
      Safety.RPCShield();
    }
  }

  public static void GuardianGrabAll()
  {
    if (!InputHandler.RGrip() || (double) Time.time <= (double) Gamemode.guardianGrabDelay)
      return;
    Gamemode.guardianGrabDelay = Time.time + 0.1f;
    foreach (VRRig vrrig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal)))
      Gamemode.SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(vrrig), (((Component) Variables.taggerInstance.bodyCollider).transform.position - ((Component) vrrig).transform.position).normalized * 20f);
  }

  public static void GuardianReleaseAll()
  {
    if (!InputHandler.RTrigger() || (double) Time.time <= (double) Gamemode.guardianReleaseDelay)
      return;
    Gamemode.guardianReleaseDelay = Time.time + 0.1f;
    if (!((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Guardian");
    }
    else
    {
      foreach (VRRig vrrig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal)))
        RigManager.GetNetworkViewFromVRRig(vrrig).SendRPC("DroppedByPlayer", (RpcTarget) 1, new object[1]
        {
          (object) Vector3.zero
        });
      Safety.RPCShield();
    }
  }

  public static void GuardianBringAllToPointer()
  {
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      if (GunLib.GunTriggers)
      {
        if ((double) Time.time <= (double) Gamemode.guardianReleaseDelay)
          return;
        Gamemode.guardianReleaseDelay = Time.time + 0.1f;
        foreach (VRRig vrrig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (plr => !plr.isLocal)))
        {
          Gamemode.SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(vrrig), ((GunLib.raycastHit).point - ((Component) vrrig).transform.position).normalized * 20f);
          Safety.RPCShield();
        }
      }
      else
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
      GunLib.SetGunVisibility(false);
  }

  public static void GuardianOrbitSelfAll()
  {
    if (!InputHandler.RTrigger() || (double) Time.time <= (double) Gamemode.guardianReleaseDelay)
      return;
    Gamemode.guardianReleaseDelay = Time.time + 0.2f;
    VRRig[] array = VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal)).ToArray<VRRig>();
    for (int index = 0; index < array.Length; ++index)
    {
      float f = (float) ((360.0 / (double) array.Length * (double) index + (double) Time.time) * (Math.PI / 180.0));
      Vector3 vector3_1 = new Vector3(Mathf.Cos(f) * 5f, 2f, Mathf.Sin(f) * 5f);
      Vector3 vector3_2 = ((Component) Variables.taggerInstance.headCollider).transform.position + vector3_1;
      Gamemode.SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(array[index]), vector3_2 - ((Component) array[index]).transform.position);
    }
    Safety.RPCShield();
  }

  public static void GuardianFlingAll()
  {
    if (!InputHandler.RTrigger() || (double) Time.time <= (double) Gamemode.guardianReleaseDelay)
      return;
    Gamemode.guardianReleaseDelay = Time.time + 0.1f;
    Gamemode.SetAllPlayersVelocity((RpcTarget) 1, new Vector3(0.0f, 1E+08f, 0.0f));
    Safety.RPCShield();
  }

  public static void GuardianFlingGun()
  {
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        else
          Gamemode.SetPlayerVelocity(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig), new Vector3(0.0f, 99999f, 0.0f));
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void GuardianAll()
  {
    if (!Variables.IsMaster())
      return;
    int index = 0;
    foreach (GorillaGuardianZoneManager guardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => ((Behaviour) gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid())))
    {
      guardianZoneManager.SetGuardian(((NetPlayer) (PhotonNetwork.PlayerList[index])));
      ++index;
    }
  }

  public static void GuardianSelf()
  {
    if (!Variables.IsMaster() || ((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(NetworkSystem.Instance.LocalPlayer))
      return;
    foreach (TappableGuardianIdol tappableGuardianIdol in Variables.GetAllType<TappableGuardianIdol>())
    {
      if ((bool) (UnityEngine.Object) ((Tappable) tappableGuardianIdol).manager && (bool) (UnityEngine.Object) ((NetworkSceneObject) ((Tappable) tappableGuardianIdol).manager).photonView && !tappableGuardianIdol.isChangingPositions)
      {
        GorillaGuardianZoneManager zoneManager = tappableGuardianIdol.zoneManager;
        if (zoneManager.IsZoneValid() && (bool) (UnityEngine.Object) ((Tappable) tappableGuardianIdol).manager && zoneManager.CurrentGuardian == null)
        {
          zoneManager.SetGuardian(NetworkSystem.Instance.LocalPlayer);
          break;
        }
      }
    }
  }

  public static void GuardianGun()
  {
    if (!Variables.IsMaster() || ((GorillaGuardianManager) GorillaGameManager.instance).IsPlayerGuardian(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig)))
      return;
    foreach (TappableGuardianIdol tappableGuardianIdol in Variables.GetAllType<TappableGuardianIdol>())
    {
      if ((bool) (UnityEngine.Object) ((Tappable) tappableGuardianIdol).manager && (bool) (UnityEngine.Object) ((NetworkSceneObject) ((Tappable) tappableGuardianIdol).manager).photonView && !tappableGuardianIdol.isChangingPositions)
      {
        GorillaGuardianZoneManager zoneManager = tappableGuardianIdol.zoneManager;
        if (zoneManager.IsZoneValid() && (bool) (UnityEngine.Object) ((Tappable) tappableGuardianIdol).manager && zoneManager.CurrentGuardian == null)
        {
          zoneManager.SetGuardian(RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig));
          break;
        }
      }
    }
  }

  public static void UnGuardianAll()
  {
    if (!Variables.IsMaster())
      return;
    foreach (GorillaGuardianZoneManager guardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => ((Behaviour) gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid())))
      guardianZoneManager.SetGuardian((NetPlayer) null);
  }

  public static void UnGuardianSelf()
  {
    if (!Variables.IsMaster())
      return;
    foreach (GorillaGuardianZoneManager guardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => ((Behaviour) gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid())).Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == NetworkSystem.Instance.LocalPlayer)))
      guardianZoneManager.SetGuardian((NetPlayer) null);
  }

  public static void UnGuardianGun()
  {
    if (!Variables.IsMaster() || !GunLib.SetupLockOnGun())
      return;
    foreach (GorillaGuardianZoneManager guardianZoneManager in GorillaGuardianZoneManager.zoneManagers.Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => ((Behaviour) gorillaGuardianZoneManager).enabled && gorillaGuardianZoneManager.IsZoneValid())).Where<GorillaGuardianZoneManager>((Func<GorillaGuardianZoneManager, bool>) (gorillaGuardianZoneManager => gorillaGuardianZoneManager.CurrentGuardian == RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig))))
      guardianZoneManager.SetGuardian((NetPlayer) null);
  }

  public static void PaintbrawlGodmode()
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaPaintbrawlManager) GorillaGameManager.instance).playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 4;
    Variables.playerInstance.disableMovement = false;
  }

  public static void NoPaintBrawlDelay(bool enable)
  {
    if (!Variables.IsMaster())
      return;
    GorillaPaintbrawlManager instance = (GorillaPaintbrawlManager) GorillaGameManager.instance;
    instance.hitCooldown = enable ? 0.0f : 3f;
    instance.tagCoolDown = enable ? 0.0f : 5f;
    instance.stunGracePeriod = enable ? 0.0f : 2f;
  }

  public static void StartPaintbrawlGame()
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaPaintbrawlManager) GorillaGameManager.instance).StartBattle();
  }

  public static void EndPaintbrawlGame()
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaPaintbrawlManager) GorillaGameManager.instance).BattleEnd();
  }

  public static void RestartPaintbrawlGame()
  {
    if (!Variables.IsMaster())
      return;
    GorillaPaintbrawlManager instance = (GorillaPaintbrawlManager) GorillaGameManager.instance;
    instance.BattleEnd();
    instance.StartBattle();
  }

  public static void PaintbrawlSpamBalloon(int actorNumber)
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaPaintbrawlManager) GorillaGameManager.instance).playerLives[actorNumber] = UnityEngine.Random.Range(0, 4);
  }

  public static void PaintbrawlSpamAllBalloons()
  {
    if (!Variables.IsMaster() || (double) Time.time < (double) Gamemode.balloonSpamDelay)
      return;
    Gamemode.balloonSpamDelay = Time.time + 0.1f;
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
      Gamemode.PaintbrawlSpamBalloon(player.ActorNumber);
  }

  public static void PaintbrawlBalloonSpamSelf()
  {
    if (!Variables.IsMaster() || (double) Time.time < (double) Gamemode.balloonSpamDelay)
      return;
    Gamemode.balloonSpamDelay = Time.time + 0.1f;
    Gamemode.PaintbrawlSpamBalloon(PhotonNetwork.LocalPlayer.ActorNumber);
  }

  public static void PaintbrawlBalloonSpamGun()
  {
    if (!Variables.IsMaster())
      return;
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        else if ((double) Time.time >= (double) Gamemode.balloonSpamDelay)
        {
          Gamemode.balloonSpamDelay = Time.time + 0.1f;
          Gamemode.PaintbrawlSpamBalloon(GunLib.lockedTargetRig.Creator.ActorNumber);
        }
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void PaintbrawlRevivePlayer(int actorNumber)
  {
    if (!Variables.IsMaster())
      return;
    ((GorillaPaintbrawlManager) GorillaGameManager.instance).playerLives[actorNumber] = 4;
  }

  public static void PaintbrawlReviveSelf()
  {
    Gamemode.PaintbrawlRevivePlayer(PhotonNetwork.LocalPlayer.ActorNumber);
  }

  public static void PaintbrawlReviveAll()
  {
    if (!Variables.IsMaster())
      return;
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
      Gamemode.PaintbrawlRevivePlayer(player.ActorNumber);
  }

  public static void PaintbrawlReviveGun()
  {
    if (!Variables.IsMaster())
      return;
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        else
          Gamemode.PaintbrawlRevivePlayer(GunLib.lockedTargetRig.Creator.ActorNumber);
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void PaintbrawlKillPlayer(int actorNumber, VRRig rig)
  {
    if (Variables.IsMaster())
    {
      ((GorillaPaintbrawlManager) GorillaGameManager.instance).playerLives[actorNumber] = 0;
    }
    else
    {
      float num;
      if (Gamemode.paintbrawlKillDelays.TryGetValue(actorNumber, out num) && (double) Time.time < (double) num)
        return;
      Gamemode.paintbrawlKillDelays[actorNumber] = Time.time + 3.1f;
      Photon.Realtime.Player player = ((IEnumerable<Photon.Realtime.Player>) PhotonNetwork.PlayerList).First<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (p => p.ActorNumber == actorNumber));
      for (int index = 0; index < 10; ++index)
      {
        ((GorillaWrappedSerializer) GameMode.ActiveNetworkHandler).SendRPC("RPC_ReportSlingshotHit", false, new object[3]
        {
          (object) player,
          (object) ((Component) rig).transform.position,
          (object) Gamemode.paintbrawlKillIndex
        });
        Safety.RPCShield();
        ++Gamemode.paintbrawlKillIndex;
      }
    }
  }

  public static void PaintbrawlKillSelf()
  {
    Gamemode.PaintbrawlKillPlayer(PhotonNetwork.LocalPlayer.ActorNumber, Variables.taggerInstance.offlineVRRig);
  }

  public static void PaintbrawlKillAll()
  {
    if (Variables.IsMaster())
    {
      foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
      {
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
          Gamemode.PaintbrawlKillPlayer(player.ActorNumber, (VRRig) null);
      }
    }
    else
    {
      VRRig rig = VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal)).OrderBy<VRRig, float>((Func<VRRig, float>) (r => UnityEngine.Random.value)).FirstOrDefault<VRRig>();
      if ((UnityEngine.Object) rig != (UnityEngine.Object) null)
        Gamemode.PaintbrawlKillPlayer(rig.Creator.ActorNumber, rig);
    }
  }

  public static void PaintbrawlKillGun()
  {
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        else
          Gamemode.PaintbrawlKillPlayer(GunLib.lockedTargetRig.Creator.ActorNumber, GunLib.lockedTargetRig);
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void PaintbrawlMatAll()
  {
    if (!Variables.IsMaster())
      return;
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
    {
      GorillaPaintbrawlManager instance = (GorillaPaintbrawlManager) GorillaGameManager.instance;
      instance.playerLives[player.ActorNumber] = 0;
      instance.playerLives[player.ActorNumber] = 4;
    }
  }

  public static void PaintbrawlMatGun()
  {
    if (!Variables.IsMaster())
      return;
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      GunLib.potentialTargetRig = ((Component) (GunLib.raycastHit).collider)?.GetComponentInParent<VRRig>();
      if (GunLib.GunTriggers)
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
        {
          GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        }
        else
        {
          GorillaPaintbrawlManager instance = (GorillaPaintbrawlManager) GorillaGameManager.instance;
          instance.playerLives[GunLib.lockedTargetRig.Creator.ActorNumber] = 0;
          instance.playerLives[GunLib.lockedTargetRig.Creator.ActorNumber] = 4;
        }
      }
      else
        GunLib.lockedTargetRig = (VRRig) null;
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }
}
