
using ExitGames.Client.Photon;
using GorillaTag.CosmeticSystem;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

internal class PropHunt
{
  private static int _propIndex;
  private static string[] _propIds;
  public static ButtonHandler.Button CyclePropButton;

  public static GorillaPropHuntGameManager GetPropHunt()
  {
    return (GorillaPropHuntGameManager) GorillaGameManager.instance;
  }

  public static string GetPropDisplayName(string cosmeticId)
  {
    CosmeticSO cosmeticSo;
    return PropHuntPools.propCosmeticId_to_cosmeticSO.TryGetValue(cosmeticId, out cosmeticSo) ? cosmeticSo.info.displayName : cosmeticId;
  }

  public static string CurrentPropName()
  {
    return PropHunt._propIds == null || PropHunt._propIds.Length == 0 ? "None" : PropHunt.GetPropDisplayName(PropHunt._propIds[PropHunt._propIndex]);
  }

  public static void BecomeProp()
  {
    if (((GorillaTagManager) PropHunt.GetPropHunt()).IsInfected(NetworkSystem.Instance.LocalPlayer))
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "You're A Seeker");
    }
    else
    {
      if (PropHunt._propIds == null || PropHunt._propIds.Length == 0)
        PropHunt._propIds = PropHuntPools.AllPropCosmeticIds;
      PropHunt.TurnIntoProp(PropHunt._propIds[PropHunt._propIndex]);
    }
  }

  public static void CycleProp(bool forward)
  {
    if (PropHunt._propIds == null || PropHunt._propIds.Length == 0)
      PropHunt._propIds = PropHuntPools.AllPropCosmeticIds;
    PropHunt._propIndex = !forward ? (PropHunt._propIndex - 1 + PropHunt._propIds.Length) % PropHunt._propIds.Length : (PropHunt._propIndex + 1) % PropHunt._propIds.Length;
    PropHunt.CyclePropButton?.SetText("Prop : " + PropHunt.CurrentPropName());
  }

  public static void TurnIntoProp(string playfabId)
  {
    if (!PropHuntPools.IsReady || string.IsNullOrEmpty(playfabId))
      return;
    PropHuntGrabbableProp huntGrabbableProp;
    PropHuntPools.TryGetGrabbableProp(playfabId, out huntGrabbableProp);
    PropHuntTaggableProp huntTaggableProp;
    PropHuntPools.TryGetTaggableProp(playfabId, out huntTaggableProp);
    if ((Object) huntGrabbableProp == (Object) null)
      return;
    PropHuntHandFollower huntHandFollower = VRRig.LocalRig.propHuntHandFollower;
    huntHandFollower.DestroyProp();
    huntHandFollower._grabbableProp = huntGrabbableProp;
    huntHandFollower._taggableProp = huntTaggableProp;
    huntHandFollower._prop = ((Component) huntGrabbableProp).gameObject;
    huntHandFollower._propOffset = huntGrabbableProp.offset;
    huntHandFollower._hasProp = true;
    huntGrabbableProp.handFollower = huntHandFollower;
    ((Component) huntGrabbableProp).gameObject.SetActive(true);
    for (int index = 0; index < huntGrabbableProp.interactionPoints.Count; ++index)
      huntGrabbableProp.interactionPoints[index].OnSpawn(VRRig.LocalRig);
  }

  public static void RemoveBlindFold()
  {
    PropHunt.GetPropHunt()._ph_blindfold_forCamera_1p?.SetActive(false);
    PropHunt.GetPropHunt()._ph_blindfold_forCamera_3p?.SetActive(false);
  }

  public static void RevealHiddenProps(bool enable)
  {
    GorillaPropHuntGameManager propHunt = PropHunt.GetPropHunt();
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isOfflineVRRig)
      {
        if (enable)
        {
          if (!((GorillaTagManager) propHunt).IsInfected(activeRig.Creator))
          {
            activeRig.SetInvisibleToLocalPlayer(false);
            activeRig.bodyRenderer.SetSkeletonBodyActive(true);
            activeRig.skeleton.renderer.sharedMaterial.shader = Variables.guiShader;
            activeRig.skeleton.renderer.sharedMaterial.color = new Color(0.4f, 1f, 0.6f, 0.35f);
            activeRig.skeleton.renderer.sharedMaterial.renderQueue = 4000;
            ((Behaviour) activeRig.skeleton).enabled = true;
            activeRig.skeleton.renderer.enabled = true;
          }
        }
        else
        {
          ((Behaviour) activeRig.skeleton).enabled = false;
          activeRig.skeleton.renderer.enabled = false;
          activeRig.skeleton.renderer.sharedMaterial.shader = Variables.uberShader;
        }
      }
    }
  }

  public static void SeekerESP(bool enable)
  {
    GorillaPropHuntGameManager propHunt = PropHunt.GetPropHunt();
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isOfflineVRRig)
      {
        bool flag = ((GorillaTagManager) propHunt).IsInfected(activeRig.Creator);
        if (enable)
        {
          if (!flag)
          {
            ((Behaviour) activeRig.skeleton).enabled = false;
            activeRig.skeleton.renderer.enabled = false;
          }
          else
          {
            activeRig.mainSkin.material.shader = Variables.guiShader;
            activeRig.mainSkin.material.color = new Color(1f, 0.15f, 0.15f, 0.85f);
            activeRig.mainSkin.material.renderQueue = 4000;
            ((Behaviour) activeRig.skeleton).enabled = false;
            activeRig.skeleton.renderer.enabled = false;
          }
        }
        else if (flag)
          activeRig.mainSkin.material.shader = Variables.uberShader;
      }
    }
  }

  public static void ForceRoundStart()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    PropHunt.GetPropHunt().InfectionRoundStartCheck();
  }

  public static void ForceRoundEnd()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    PropHunt.GetPropHunt().PH_OnRoundEnd();
    PropHunt.GetPropHunt().InfectionRoundEndCheck();
  }

  public static void PropTagAll()
  {
    GorillaPropHuntGameManager propHunt = PropHunt.GetPropHunt();
    if (propHunt._ph_gameState != (GorillaPropHuntGameManager.EPropHuntGameState) 6)
      return;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((Object) activeRig == (Object) VRRig.LocalRig) && !((GorillaTagManager) propHunt).IsInfected(activeRig.Creator))
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
        ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) activeRig).transform.position;
        PhotonSerializer.ForceSerialization();
        PunExtensions.GetPhotonView(GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)")).RPC("RPC_ReportTag", (RpcTarget) 0, new object[1]
        {
          (object) activeRig.Creator.ActorNumber
        });
        PhotonNetwork.SendAllOutgoingCommands();
        ((PhotonPeer) PhotonNetwork.NetworkingClient.LoadBalancingPeer).SendAcksOnly();
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
      }
    }
  }

  public static void BecomeSeeker()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    ((GorillaTagManager) PropHunt.GetPropHunt()).AddInfectedPlayer(NetworkSystem.Instance.LocalPlayer, true);
  }

  public static void BecomeHider()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    GorillaPropHuntGameManager propHunt = PropHunt.GetPropHunt();
    while (((GorillaTagManager) propHunt).currentInfected.Contains(NetworkSystem.Instance.LocalPlayer))
      ((GorillaTagManager) propHunt).currentInfected.Remove(NetworkSystem.Instance.LocalPlayer);
    ((GorillaTagManager) propHunt).UpdateInfectionState();
  }

  public static void CollapseBoundary()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    PropHunt.GetPropHunt()._ph_playBoundary.radiusScale = 1f / 1000f;
  }

  public static void SpamGamemode()
  {
    if (!PhotonNetwork.IsMasterClient)
      return;
    GorillaPropHuntGameManager propHunt = PropHunt.GetPropHunt();
    propHunt.PH_OnRoundEnd();
    propHunt.InfectionRoundEndCheck();
    propHunt.InfectionRoundStartCheck();
  }
}
