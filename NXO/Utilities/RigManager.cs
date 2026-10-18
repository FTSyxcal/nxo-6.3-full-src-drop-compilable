
using GorillaNetworking;
using GorillaTagScripts;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public static class RigManager
{
  private static VRRig ghostRig;
  private static Material ghostRigMaterial;
  public static bool ghostRigEnabled = true;

  public static GameObject GetHandObject
  {
    get
    {
      return !Variables.rightHandedMenu ? ((Component) Variables.taggerInstance.offlineVRRig.leftHandPlayer).gameObject : ((Component) Variables.taggerInstance.offlineVRRig.rightHandPlayer).gameObject;
    }
  }

  public static NetPlayer GetNetPlayerFromVRRig(VRRig vrrig)
  {
    return vrrig.Creator ?? vrrig.OwningNetPlayer ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(((Component) vrrig.rigSerializer).gameObject));
  }

  public static VRRig GetVRRigFromNetPlayer(NetPlayer netPlayer)
  {
    return netPlayer != null ? GorillaGameManager.StaticFindRigForPlayer(netPlayer) : (VRRig) null;
  }

  public static PhotonView GetPhotonViewFromVRRig(this VRRig rig) => rig.netView?.GetView;

  public static NetworkView GetNetworkViewFromVRRig(VRRig vrrig)
  {
    return !((UnityEngine.Object) vrrig == (UnityEngine.Object) null) ? vrrig.netView : (NetworkView) null;
  }

  public static Photon.Realtime.Player NetPlayerToPhotonPlayer(NetPlayer p) => p.GetPlayerRef();

  public static bool IsRigLocal(this VRRig rig)
  {
    if (!((UnityEngine.Object) rig != (UnityEngine.Object) null))
      return false;
    if (rig.isLocal)
      return true;
    return (UnityEngine.Object) RigManager.ghostRig != (UnityEngine.Object) null && (UnityEngine.Object) rig == (UnityEngine.Object) RigManager.ghostRig;
  }

  public static bool IsOtherPlayer(VRRig rig)
  {
    return (UnityEngine.Object) rig != (UnityEngine.Object) null && !rig.isOfflineVRRig && (UnityEngine.Object) rig != (UnityEngine.Object) VRRig.LocalRig;
  }

  public static bool RigIsInfected(VRRig rig)
  {
    if ((UnityEngine.Object) rig == (UnityEngine.Object) null || !PhotonNetwork.InRoom || (UnityEngine.Object) GorillaGameManager.instance == (UnityEngine.Object) null)
      return false;
    NetPlayer netPlayerFromVrRig = RigManager.GetNetPlayerFromVRRig(rig);
    if (netPlayerFromVrRig == null)
      return false;
    switch ((int) GorillaGameManager.instance.GameType())
    {
      case 0:
        return false;
      case 1:
      case 5:
      case 9:
      case 10:
      case 11:
        if (GorillaGameManager.instance is GorillaTagManager instance1)
        {
          int num;
          if (!instance1.isCurrentlyTag)
          {
            List<NetPlayer> currentInfected = instance1.currentInfected;
            num = currentInfected != null ? ((currentInfected.Contains(netPlayerFromVrRig)) ? 1 : 0) : 0;
          }
          else
            num = instance1.currentIt == netPlayerFromVrRig ? 1 : 0;
          return num != 0;
        }
        break;
      case 3:
        if (GorillaGameManager.instance is GorillaPaintbrawlManager instance2 && instance2.playerLives != null)
        {
          int num;
          return instance2.playerLives.TryGetValue(netPlayerFromVrRig.ActorNumber, out num) && num <= 0;
        }
        break;
      case 4:
      case 6:
        if (GorillaGameManager.instance is GorillaAmbushManager instance3)
        {
          int num;
          if (!((GorillaTagManager) instance3).isCurrentlyTag)
          {
            List<NetPlayer> currentInfected = ((GorillaTagManager) instance3).currentInfected;
            num = currentInfected != null ? ((currentInfected.Contains(netPlayerFromVrRig)) ? 1 : 0) : 0;
          }
          else
            num = ((GorillaTagManager) instance3).currentIt == netPlayerFromVrRig ? 1 : 0;
          return num != 0;
        }
        break;
    }
    if ((UnityEngine.Object) rig.mainSkin?.material == (UnityEngine.Object) null)
      return false;
    string name = rig.mainSkin.material.name;
    return name.Contains("fected") || name.Contains("It");
  }

  public static bool HandsTouchTargets(Transform[] hands, Transform[] targets)
  {
    if (hands == null || targets == null)
      return false;
    foreach (Transform hand in hands)
    {
      if (!((UnityEngine.Object) hand == (UnityEngine.Object) null))
      {
        foreach (Transform target in targets)
        {
          if ((UnityEngine.Object) target != (UnityEngine.Object) null && (double) Vector3.Distance(hand.position, target.position) < 0.25)
            return true;
        }
      }
    }
    return false;
  }

  public static void HandleGhostRig()
  {
    if (!RigManager.ghostRigEnabled)
    {
      if (!((UnityEngine.Object) RigManager.ghostRig != (UnityEngine.Object) null))
        return;
      ((Component) RigManager.ghostRig).gameObject.SetActive(false);
    }
    else
    {
      try
      {
        if (!((Behaviour) Variables.taggerInstance.offlineVRRig).enabled)
        {
          if ((UnityEngine.Object) RigManager.ghostRig == (UnityEngine.Object) null)
          {
            RigManager.ghostRig = UnityEngine.Object.Instantiate<VRRig>(Variables.taggerInstance.offlineVRRig);
            RigManager.ghostRig.headBodyOffset = Vector3.zero;
            ((Component) RigManager.ghostRig).transform.SetParent(((Component) Variables.taggerInstance.offlineVRRig).transform.parent);
            ((Component) RigManager.ghostRig).transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio")?.gameObject.SetActive(false);
            ((Component) RigManager.ghostRig).transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio")?.gameObject.SetActive(false);
            ((Component) RigManager.ghostRig).transform.Find("GorillaPlayerNetworkedRigAnchor/rig/bodySlideAudio")?.gameObject.SetActive(false);
            Visuals.FixRigColors(RigManager.ghostRig);
          }
          if ((UnityEngine.Object) RigManager.ghostRigMaterial == (UnityEngine.Object) null)
          {
            RigManager.ghostRigMaterial = new Material(Variables.guiShader);
            RigManager.ghostRigMaterial.color = (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte) 50);
          }
          if (!((Component) RigManager.ghostRig).gameObject.activeSelf)
            ((Component) RigManager.ghostRig).gameObject.SetActive(true);
          if (!((Behaviour) RigManager.ghostRig).enabled)
            ((Behaviour) RigManager.ghostRig).enabled = true;
          RigManager.ghostRig.mainSkin.material = RigManager.ghostRigMaterial;
          RigManager.ghostRig.headConstraint.transform.SetPositionAndRotation(((Component) Variables.playerInstance.headCollider).transform.position, ((Component) Variables.playerInstance.headCollider).transform.rotation);
          RigManager.ghostRig.leftHandTransform.SetPositionAndRotation(Variables.playerInstance.LeftHand.controllerTransform.position, Variables.playerInstance.LeftHand.controllerTransform.rotation);
          RigManager.ghostRig.rightHandTransform.SetPositionAndRotation(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.RightHand.controllerTransform.rotation);
        }
        else
        {
          if (!((UnityEngine.Object) RigManager.ghostRig != (UnityEngine.Object) null) || !((Component) RigManager.ghostRig).gameObject.activeSelf)
            return;
          ((Component) RigManager.ghostRig).gameObject.SetActive(false);
        }
      }
      catch (Exception ex)
      {
        Debug.LogError((object) $"Error updating ghost rig: {ex}");
      }
    }
  }

  public static void ChangeName(string PlayerName, bool noColor = false)
  {
    GorillaComputer.instance.currentName = PlayerName;
    GorillaComputer.instance.SetLocalNameTagText(GorillaComputer.instance.currentName);
    GorillaComputer.instance.savedName = GorillaComputer.instance.currentName;
    PlayerPrefs.SetString("playerName", GorillaComputer.instance.currentName);
    PlayerPrefs.Save();
    PhotonNetwork.LocalPlayer.NickName = PlayerName;
    if (noColor)
      return;
    try
    {
      if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && !CosmeticWardrobeProximityDetector.IsUserNearWardrobe(PhotonNetwork.LocalPlayer.ActorNumber))
        return;
      GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget) 0, new object[3]
      {
        (object) VRRig.LocalRig.playerColor.r,
        (object) VRRig.LocalRig.playerColor.g,
        (object) VRRig.LocalRig.playerColor.b
      });
      Safety.RPCShield();
    }
    catch
    {
    }
  }

  public static void ChangeColor(Color color, object target = null)
  {
    PlayerPrefs.SetFloat("redValue", Mathf.Clamp(color.r, 0.0f, 1f));
    PlayerPrefs.SetFloat("greenValue", Mathf.Clamp(color.g, 0.0f, 1f));
    PlayerPrefs.SetFloat("blueValue", Mathf.Clamp(color.b, 0.0f, 1f));
    GorillaTagger.Instance.UpdateColor(color.r, color.g, color.b);
    PlayerPrefs.Save();
    try
    {
      object obj = target;
      switch (obj)
      {
        case null:
          GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget) 0, new object[3]
          {
            (object) color.r,
            (object) color.g,
            (object) color.b
          });
          break;
        case NetPlayer netPlayer:
          NetworkView myVrRig = GorillaTagger.Instance.myVRRig;
          object[] objArray = new object[3]
          {
            (object) color.r,
            (object) color.g,
            (object) color.b
          };
          myVrRig.SendRPC("RPC_InitializeNoobMaterial", netPlayer, objArray);
          break;
        case RpcTarget _:
          GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", (RpcTarget) obj, new object[3]
          {
            (object) color.r,
            (object) color.g,
            (object) color.b
          });
          break;
      }
      Safety.RPCShield();
    }
    catch
    {
    }
  }
}
