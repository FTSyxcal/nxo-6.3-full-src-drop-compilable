
using ExitGames.Client.Photon;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

internal class PhotonSerializer
{
  public static void PerPlayerRigOverride(Action<VRRig> poseForTarget, bool disableRig = false)
  {
    MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
    {
      if (!PhotonNetwork.InRoom)
        return true;
      VRRig localRig = VRRig.LocalRig;
      if ((UnityEngine.Object) localRig == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance.myVRRig == (UnityEngine.Object) null)
        return true;
      PhotonView component = ((Component) Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return true;
      PhotonSerializer.BroadcastViews(true, new PhotonView[1]
      {
        component
      });
      PhotonSerializer.RigPose rigPose = PhotonSerializer.RigPose.Capture(localRig);
      bool enabled = ((Behaviour) localRig).enabled;
      if (disableRig)
        ((Behaviour) localRig).enabled = false;
      try
      {
        foreach (NetPlayer playerListOther in NetworkSystem.Instance.PlayerListOthers)
        {
          VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(playerListOther);
          if (!((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null))
          {
            try
            {
              poseForTarget(rigFromNetPlayer);
              PhotonSerializer.SendSerialize(component, new RaiseEventOptions()
              {
                TargetActors = new int[1]
                {
                  playerListOther.ActorNumber
                }
              });
            }
            catch
            {
            }
          }
        }
        Safety.RPCShield();
      }
      finally
      {
        rigPose.Restore(localRig);
        ((Behaviour) localRig).enabled = enabled;
      }
      return false;
    });
  }

  public static void SendSerialize(PhotonView pv, RaiseEventOptions options = null)
  {
    if (!PhotonNetwork.InRoom || (UnityEngine.Object) pv == (UnityEngine.Object) null)
      return;
    List<object> objectList = PhotonNetwork.OnSerializeWrite(pv);
    if (objectList == null)
      return;
    PhotonNetwork.RaiseEventBatch key = new PhotonNetwork.RaiseEventBatch();
    key.Reliable = pv.Synchronization == (ViewSynchronization) 1 || pv.mixedModeIsReliable;
    key.Group = pv.Group;
    IDictionary serializeViewBatches = (IDictionary) PhotonNetwork.serializeViewBatches;
    PhotonNetwork.SerializeViewBatch serializeViewBatch = new PhotonNetwork.SerializeViewBatch(key, 2);
    if (!serializeViewBatches.Contains((object) key))
      serializeViewBatches[(object) key] = (object) serializeViewBatch;
    serializeViewBatch.Add(objectList);
    RaiseEventOptions serializeRaiseEvOptions = PhotonNetwork.serializeRaiseEvOptions;
    RaiseEventOptions raiseEventOptions1;
    if (options == null)
    {
      raiseEventOptions1 = serializeRaiseEvOptions;
    }
    else
    {
      raiseEventOptions1 = new RaiseEventOptions();
      raiseEventOptions1.CachingOption = serializeRaiseEvOptions.CachingOption;
      raiseEventOptions1.Flags = serializeRaiseEvOptions.Flags;
      raiseEventOptions1.InterestGroup = serializeRaiseEvOptions.InterestGroup;
      raiseEventOptions1.TargetActors = options.TargetActors;
      raiseEventOptions1.Receivers = options.Receivers;
    }
    RaiseEventOptions raiseEventOptions2 = raiseEventOptions1;
    bool reliable = serializeViewBatch.Batch.Reliable;
    List<object> objectUpdates = serializeViewBatch.ObjectUpdates;
    objectUpdates[0] = (object) PhotonNetwork.ServerTimestamp;
    objectUpdates[1] = PhotonNetwork.currentLevelPrefix != (byte) 0 ? (object) PhotonNetwork.currentLevelPrefix : (object) null;
    PhotonNetwork.NetworkingClient.OpRaiseEvent(reliable ? (byte) 206 : (byte) 201, (object) objectUpdates, raiseEventOptions2, reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable);
    serializeViewBatch.Clear();
  }

  public static void BroadcastViews(bool exclude = false, PhotonView[] viewFilter = null)
  {
    if (!PhotonNetwork.InRoom)
      return;
    if (viewFilter == null)
      viewFilter = Array.Empty<PhotonView>();
    List<int> list = ((IEnumerable<PhotonView>) viewFilter).Select<PhotonView, int>((Func<PhotonView, int>) (v => v.ViewID)).ToList<int>();
    foreach (PhotonView pv in PhotonNetwork.photonViewList.Values)
    {
      if (pv.IsMine && pv.Synchronization != null && ((Behaviour) pv).isActiveAndEnabled && !PhotonNetwork.blockedSendingGroups.Contains(pv.Group) && (exclude ? !list.Contains(pv.ViewID) : list.Contains(pv.ViewID)))
        PhotonSerializer.SendSerialize(pv);
    }
  }

  public static void ForceSerialization() => PhotonNetwork.RunViewUpdate();

  public struct RigPose
  {
    private Vector3 pos;
    private Quaternion rot;
    private Vector3 lPos;
    private Quaternion lRot;
    private Vector3 rPos;
    private Quaternion rRot;
    private Quaternion headRot;

    public static PhotonSerializer.RigPose Capture(VRRig r)
    {
      return new PhotonSerializer.RigPose()
      {
        pos = ((Component) r).transform.position,
        rot = ((Component) r).transform.rotation,
        lPos = r.leftHand.rigTarget.position,
        lRot = r.leftHand.rigTarget.rotation,
        rPos = r.rightHand.rigTarget.position,
        rRot = r.rightHand.rigTarget.rotation,
        headRot = r.head.rigTarget.transform.rotation
      };
    }

    public void Restore(VRRig r)
    {
      ((Component) r).transform.position = this.pos;
      ((Component) r).transform.rotation = this.rot;
      r.leftHand.rigTarget.position = this.lPos;
      r.leftHand.rigTarget.rotation = this.lRot;
      r.rightHand.rigTarget.position = this.rPos;
      r.rightHand.rigTarget.rotation = this.rRot;
      r.head.rigTarget.transform.rotation = this.headRot;
    }
  }
}
