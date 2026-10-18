
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Player
{
  private static float orbitSpeed = 100f;
  private static float orbitDistance = 2f;
  private static bool hasTeleported = false;
  private static Vector3? cachedHeadRotation = new Vector3?();
  public static float sizeScale = 1f;
  public static float lastSentScale = -1f;
  private static bool wasRigDisabled = false;
  private static float _headSpinOffset = 0.0f;
  private static Vector3 piggybackRigOffset;
  private static bool piggybackHeld;
  private static Vector3 _layOnBackPos;
  private static Vector3 _layOnStomachPos;
  private static float _backflipAngle;
  private static float _frontflipAngle;
  private static float _cartWheelAngle;
  private static float _spider;
  private static float _spiderIdle;
  private static Vector3 _spiderLastPos;
  private static bool _spiderInit;
  private static float _glitchOffset;
  private static float _glitchTimer;
  private static Vector3 _glitchPos;
  private static float _wobbleOffset;
  private static GameObject _ragdollObj;
  private static Rigidbody _ragdollBody;
  private static BoxCollider _ragdollCollider;

  public static void GhostMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = !InputHandler.isOn;
  }

  public static void InvisibleMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (InputHandler.isOn)
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void GhostInvisibleMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RPrimary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn2, ref InputHandler.wasButtonPressed2);
    ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = !InputHandler.isOn && !InputHandler.isOn2;
    if (!InputHandler.isOn2)
      return;
    ((Component) Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
  }

  public static void LongArms(bool setActive)
  {
    ((Component) Variables.playerInstance).transform.localScale = setActive ? Settings.ArmLength : Vector3.one;
  }

  public static void SizeChanger()
  {
    if (InputHandler.LSecondary())
      Player.sizeScale = 1f;
    if (InputHandler.LTrigger())
      Player.sizeScale -= 0.05f;
    if (InputHandler.RTrigger())
      Player.sizeScale += 0.05f;
    Variables.playerInstance.nativeScale = Player.sizeScale;
  }

  public static void SetRigStatus(bool rigStatus)
  {
    ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = rigStatus;
    if (!rigStatus)
      return;
    Player.ResetHeadRotation();
  }

  public static void SetHeadRotation(Vector3? headRotation = null, bool spin = false)
  {
    if (!Player.cachedHeadRotation.HasValue)
      Player.cachedHeadRotation = new Vector3?(Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles);
    if (spin)
    {
      Player._headSpinOffset += 5f;
      Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3(0.0f, Player._headSpinOffset, 0.0f);
    }
    else
    {
      Player._headSpinOffset = 0.0f;
      if (!headRotation.HasValue)
        return;
      Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = headRotation.Value;
    }
  }

  public static void ResetHeadRotation()
  {
    if ((UnityEngine.Object) Variables.taggerInstance?.offlineVRRig?.head?.rigTarget == (UnityEngine.Object) null || !Player.cachedHeadRotation.HasValue)
      return;
    Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = Player.cachedHeadRotation.Value;
    Player.cachedHeadRotation = new Vector3?();
  }

  public static void RigGun()
  {
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      if (GunLib.GunTriggers)
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
        ((Component) Variables.taggerInstance.offlineVRRig).transform.position = (GunLib.raycastHit).point + new Vector3(0.0f, 1f, 0.0f);
      }
      else
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
      GunLib.SetGunVisibility(false);
  }

  public static void TeleportGun()
  {
    if (GunLib.GunGrips)
    {
      GunLib.SetupRaycast();
      if (GunLib.GunTriggers)
      {
        if (Player.hasTeleported || !((UnityEngine.Object) (GunLib.raycastHit).collider != (UnityEngine.Object) null))
          return;
        Movement.TeleportTo((GunLib.raycastHit).point + new Vector3(0.0f, 1f, -0.5f));
        Variables.taggerInstance.rigidbody.velocity = Vector3.zero;
        Player.hasTeleported = true;
      }
      else
        Player.hasTeleported = false;
    }
    else
      GunLib.SetGunVisibility(false);
  }

  public static void ChasePlayerGun()
  {
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
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = Vector3.MoveTowards(((Component) Variables.taggerInstance.offlineVRRig).transform.position, ((Component) GunLib.lockedTargetRig).transform.position, Time.deltaTime * 10f);
          ((Component) Variables.taggerInstance.offlineVRRig).transform.LookAt(((Component) GunLib.lockedTargetRig).transform);
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1.5f;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1.5f;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
          Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3((float) UnityEngine.Random.Range(0, 360), (float) UnityEngine.Random.Range(0, 180), (float) UnityEngine.Random.Range(0, 180));
          Variables.taggerInstance.offlineVRRig.head.rigTarget.eulerAngles = new Vector3((float) UnityEngine.Random.Range(0, 360), (float) UnityEngine.Random.Range(0, 360), (float) UnityEngine.Random.Range(0, 360));
          ((Component) Variables.taggerInstance).GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void JumpscarePlayerGun()
  {
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
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.forward * UnityEngine.Random.Range(0.1f, 0.5f);
          Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.LookAt(GunLib.lockedTargetRig.headMesh.transform.position);
          ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.right * 0.2f;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.right * -0.2f;
          Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) Variables.taggerInstance.offlineVRRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) Variables.taggerInstance.offlineVRRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void JumpscareAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = targetRig.headMesh.transform.position + targetRig.headMesh.transform.forward * UnityEngine.Random.Range(0.1f, 0.5f);
      VRRig.LocalRig.head.rigTarget.transform.LookAt(targetRig.headMesh.transform.position);
      Quaternion rotation = VRRig.LocalRig.head.rigTarget.transform.rotation;
      ((Component) VRRig.LocalRig).transform.rotation = rotation;
      VRRig.LocalRig.leftHand.rigTarget.transform.position = targetRig.headMesh.transform.position + targetRig.headMesh.transform.right * 0.2f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = targetRig.headMesh.transform.position + targetRig.headMesh.transform.right * -0.2f;
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) VRRig.LocalRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) VRRig.LocalRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
      VRRig.LocalRig.head.rigTarget.transform.rotation = rotation;
    }));
  }

  public static void OrbitPlayerGun()
  {
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
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.LookAt(((Component) GunLib.lockedTargetRig).transform.position);
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1.5f;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1.5f;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
          ((Component) Variables.taggerInstance.offlineVRRig).transform.RotateAround(((Component) GunLib.lockedTargetRig).transform.position, Vector3.up, Player.orbitSpeed * Time.deltaTime);
          Vector3 vector3 = (((Component) Variables.taggerInstance.offlineVRRig).transform.position - ((Component) GunLib.lockedTargetRig).transform.position).normalized * Player.orbitDistance + ((Component) GunLib.lockedTargetRig).transform.position;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = vector3;
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void CopyPlayerGun()
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
          Player.CopyPlayer(GunLib.lockedTargetRig);
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void CopyPlayer(VRRig target)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance?.offlineVRRig == (UnityEngine.Object) null)
      return;
    VRRig offlineVrRig = Variables.taggerInstance.offlineVRRig;
    ((Behaviour) offlineVrRig).enabled = false;
    ((Component) offlineVrRig).transform.SetPositionAndRotation(((Component) target).transform.position, ((Component) target).transform.rotation);
    Transform[] transformArray1 = new Transform[3]
    {
      offlineVrRig.head.rigTarget,
      offlineVrRig.leftHand.rigTarget,
      offlineVrRig.rightHand.rigTarget
    };
    Transform[] transformArray2 = new Transform[3]
    {
      target.head.rigTarget,
      target.leftHand.rigTarget,
      target.rightHand.rigTarget
    };
    for (int index = 0; index < transformArray1.Length; ++index)
      transformArray1[index].SetPositionAndRotation(transformArray2[index].position, transformArray2[index].rotation);
    if (target.leftIndex != null)
    {
      ((VRMap) offlineVrRig.leftIndex).calcT = ((VRMap) target.leftIndex).calcT;
      ((VRMap) offlineVrRig.leftIndex).LerpFinger(1f, false);
    }
    if (target.leftMiddle != null)
    {
      ((VRMap) offlineVrRig.leftMiddle).calcT = ((VRMap) target.leftMiddle).calcT;
      ((VRMap) offlineVrRig.leftMiddle).LerpFinger(1f, false);
    }
    if (target.leftThumb != null)
    {
      ((VRMap) offlineVrRig.leftThumb).calcT = ((VRMap) target.leftThumb).calcT;
      ((VRMap) offlineVrRig.leftThumb).LerpFinger(1f, false);
    }
    if (target.rightIndex != null)
    {
      ((VRMap) offlineVrRig.rightIndex).calcT = ((VRMap) target.rightIndex).calcT;
      ((VRMap) offlineVrRig.rightIndex).LerpFinger(1f, false);
    }
    if (target.rightMiddle != null)
    {
      ((VRMap) offlineVrRig.rightMiddle).calcT = ((VRMap) target.rightMiddle).calcT;
      ((VRMap) offlineVrRig.rightMiddle).LerpFinger(1f, false);
    }
    if (target.rightThumb == null)
      return;
    ((VRMap) offlineVrRig.rightThumb).calcT = ((VRMap) target.rightThumb).calcT;
    ((VRMap) offlineVrRig.rightThumb).LerpFinger(1f, false);
  }

  public static void BackshotPlayerGun()
  {
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
          ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.forward * (float) -(0.20000000298023224 + (double) Mathf.Sin((float) Time.frameCount / 8f) * 0.10000000149011612);
          ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.right * -0.2f + ((Component) GunLib.lockedTargetRig).transform.up * -0.4f;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.right * 0.2f + ((Component) GunLib.lockedTargetRig).transform.up * -0.4f;
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
          Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void GawkGawkGun()
  {
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
          ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.forward * (float) (0.20000000298023224 + (double) Mathf.Sin((float) Time.frameCount / 8f) * 0.10000000149011612) + ((Component) GunLib.lockedTargetRig).transform.up * -0.4f;
          ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(((Component) GunLib.lockedTargetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
          Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.right * 0.2f + ((Component) GunLib.lockedTargetRig).transform.up * -0.4f;
          Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.right * -0.2f + ((Component) GunLib.lockedTargetRig).transform.up * -0.4f;
          Transform transform1 = Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform;
          Quaternion rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
          Quaternion quaternion1 = Quaternion.Euler(rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
          transform1.rotation = quaternion1;
          Transform transform2 = Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform;
          rotation = ((Component) GunLib.lockedTargetRig).transform.rotation;
          Quaternion quaternion2 = Quaternion.Euler(rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
          transform2.rotation = quaternion2;
          Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(((Component) GunLib.lockedTargetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void PiggybackPlayerGun()
  {
    if (GunLib.SetupLockOnGun())
    {
      if (!Player.piggybackHeld)
      {
        Player.piggybackHeld = true;
        Player.piggybackRigOffset = ((Component) VRRig.LocalRig).transform.position - ((Component) Variables.taggerInstance.bodyCollider).transform.position;
      }
      Vector3 vector3 = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.up * 0.3f - ((Component) GunLib.lockedTargetRig).transform.forward * 0.4f;
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = vector3 + Player.piggybackRigOffset;
    }
    else
    {
      Player.piggybackHeld = false;
      ((Behaviour) VRRig.LocalRig).enabled = true;
    }
  }

  public static void MirrorPlayerGun()
  {
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
          if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance?.offlineVRRig == (UnityEngine.Object) null)
            return;
          VRRig offlineVrRig = Variables.taggerInstance.offlineVRRig;
          ((Behaviour) offlineVrRig).enabled = false;
          Vector3 vector3 = ((Component) GunLib.lockedTargetRig).transform.position + ((Component) GunLib.lockedTargetRig).transform.forward * 1.5f;
          ((Component) offlineVrRig).transform.position = vector3;
          ((Component) offlineVrRig).transform.rotation = Quaternion.Euler(0.0f, ((Component) GunLib.lockedTargetRig).transform.eulerAngles.y + 180f, 0.0f);
          Vector3 position1 = ((Component) GunLib.lockedTargetRig).transform.InverseTransformPoint(GunLib.lockedTargetRig.leftHand.rigTarget.position);
          Vector3 position2 = ((Component) GunLib.lockedTargetRig).transform.InverseTransformPoint(GunLib.lockedTargetRig.rightHand.rigTarget.position);
          position1.x *= -1f;
          position2.x *= -1f;
          offlineVrRig.rightHand.rigTarget.transform.position = ((Component) offlineVrRig).transform.TransformPoint(position1);
          offlineVrRig.leftHand.rigTarget.transform.position = ((Component) offlineVrRig).transform.TransformPoint(position2);
          Quaternion rotation1 = GunLib.lockedTargetRig.leftHand.rigTarget.rotation;
          Quaternion rotation2 = GunLib.lockedTargetRig.rightHand.rigTarget.rotation;
          offlineVrRig.rightHand.rigTarget.transform.rotation = new Quaternion(-rotation1.x, rotation1.y, rotation1.z, -rotation1.w);
          offlineVrRig.leftHand.rigTarget.transform.rotation = new Quaternion(-rotation2.x, rotation2.y, rotation2.z, -rotation2.w);
          offlineVrRig.head.rigTarget.rotation = Quaternion.Euler(GunLib.lockedTargetRig.head.rigTarget.eulerAngles.x, GunLib.lockedTargetRig.head.rigTarget.eulerAngles.y + 180f, -GunLib.lockedTargetRig.head.rigTarget.eulerAngles.z);
          if (GunLib.lockedTargetRig.leftIndex != null)
          {
            ((VRMap) offlineVrRig.rightIndex).calcT = ((VRMap) GunLib.lockedTargetRig.leftIndex).calcT;
            ((VRMap) offlineVrRig.rightIndex).LerpFinger(1f, false);
          }
          if (GunLib.lockedTargetRig.leftMiddle != null)
          {
            ((VRMap) offlineVrRig.rightMiddle).calcT = ((VRMap) GunLib.lockedTargetRig.leftMiddle).calcT;
            ((VRMap) offlineVrRig.rightMiddle).LerpFinger(1f, false);
          }
          if (GunLib.lockedTargetRig.leftThumb != null)
          {
            ((VRMap) offlineVrRig.rightThumb).calcT = ((VRMap) GunLib.lockedTargetRig.leftThumb).calcT;
            ((VRMap) offlineVrRig.rightThumb).LerpFinger(1f, false);
          }
          if (GunLib.lockedTargetRig.rightIndex != null)
          {
            ((VRMap) offlineVrRig.leftIndex).calcT = ((VRMap) GunLib.lockedTargetRig.rightIndex).calcT;
            ((VRMap) offlineVrRig.leftIndex).LerpFinger(1f, false);
          }
          if (GunLib.lockedTargetRig.rightMiddle != null)
          {
            ((VRMap) offlineVrRig.leftMiddle).calcT = ((VRMap) GunLib.lockedTargetRig.rightMiddle).calcT;
            ((VRMap) offlineVrRig.leftMiddle).LerpFinger(1f, false);
          }
          if (GunLib.lockedTargetRig.rightThumb != null)
          {
            ((VRMap) offlineVrRig.leftThumb).calcT = ((VRMap) GunLib.lockedTargetRig.rightThumb).calcT;
            ((VRMap) offlineVrRig.leftThumb).LerpFinger(1f, false);
          }
        }
      }
      else
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
        GunLib.lockedTargetRig = (VRRig) null;
      }
    }
    else
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
    }
  }

  public static void FlyTowardsGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    Vector3 normalized = (((Component) GunLib.lockedTargetRig).transform.position - ((Component) Variables.playerInstance).transform.position).normalized;
    ((Component) Variables.playerInstance).transform.position += normalized * Time.deltaTime * 10f;
    ((Component) Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  public static void ChaseAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = Vector3.MoveTowards(((Component) VRRig.LocalRig).transform.position, ((Component) targetRig).transform.position, Time.deltaTime * 10f);
      ((Component) VRRig.LocalRig).transform.LookAt(((Component) targetRig).transform);
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) VRRig.LocalRig).transform.right * -1.5f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) VRRig.LocalRig).transform.right * 1.5f;
      VRRig.LocalRig.leftHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
      VRRig.LocalRig.rightHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
      VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Euler((float) UnityEngine.Random.Range(0, 360), (float) UnityEngine.Random.Range(0, 360), (float) UnityEngine.Random.Range(0, 360));
    }));
  }

  public static void OrbitAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.LookAt(((Component) targetRig).transform.position);
      ((Component) VRRig.LocalRig).transform.RotateAround(((Component) targetRig).transform.position, Vector3.up, Player.orbitSpeed * Time.deltaTime);
      ((Component) VRRig.LocalRig).transform.position = (((Component) VRRig.LocalRig).transform.position - ((Component) targetRig).transform.position).normalized * Player.orbitDistance + ((Component) targetRig).transform.position;
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) VRRig.LocalRig).transform.right * -1.5f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) VRRig.LocalRig).transform.right * 1.5f;
      VRRig.LocalRig.leftHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
      VRRig.LocalRig.rightHand.rigTarget.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
    }));
  }

  public static void CopyAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.SetPositionAndRotation(((Component) targetRig).transform.position, ((Component) targetRig).transform.rotation);
      VRRig.LocalRig.head.rigTarget.SetPositionAndRotation(targetRig.head.rigTarget.position, targetRig.head.rigTarget.rotation);
      VRRig.LocalRig.leftHand.rigTarget.SetPositionAndRotation(targetRig.leftHand.rigTarget.position, targetRig.leftHand.rigTarget.rotation);
      VRRig.LocalRig.rightHand.rigTarget.SetPositionAndRotation(targetRig.rightHand.rigTarget.position, targetRig.rightHand.rigTarget.rotation);
    }));
  }

  public static void MirrorAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.forward * 1.5f;
      ((Component) VRRig.LocalRig).transform.rotation = Quaternion.Euler(0.0f, ((Component) targetRig).transform.eulerAngles.y + 180f, 0.0f);
      Vector3 position1 = ((Component) targetRig).transform.InverseTransformPoint(targetRig.leftHand.rigTarget.position);
      Vector3 position2 = ((Component) targetRig).transform.InverseTransformPoint(targetRig.rightHand.rigTarget.position);
      position1.x *= -1f;
      position2.x *= -1f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.TransformPoint(position1);
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.TransformPoint(position2);
      Quaternion rotation1 = targetRig.leftHand.rigTarget.rotation;
      Quaternion rotation2 = targetRig.rightHand.rigTarget.rotation;
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = new Quaternion(-rotation1.x, rotation1.y, rotation1.z, -rotation1.w);
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = new Quaternion(-rotation2.x, rotation2.y, rotation2.z, -rotation2.w);
      VRRig.LocalRig.head.rigTarget.rotation = Quaternion.Euler(targetRig.head.rigTarget.eulerAngles.x, targetRig.head.rigTarget.eulerAngles.y + 180f, -targetRig.head.rigTarget.eulerAngles.z);
    }));
  }

  public static void BackshotAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.forward * (float) -(0.20000000298023224 + (double) Mathf.Sin((float) Time.frameCount / 8f) * 0.10000000149011612);
      ((Component) VRRig.LocalRig).transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.right * -0.2f + ((Component) targetRig).transform.up * -0.4f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.right * 0.2f + ((Component) targetRig).transform.up * -0.4f;
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.head.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
    }));
  }

  public static void OnlyInvisibleToPlayerGun()
  {
    if (GunLib.SetupLockOnGun())
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        NetPlayer target = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
        PhotonSerializer.BroadcastViews(true, new PhotonView[1]
        {
          VRRig.LocalRig.GetPhotonViewFromVRRig()
        });
        Vector3 position = ((Component) VRRig.LocalRig).transform.position;
        PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonViewFromVRRig(), new RaiseEventOptions()
        {
          TargetActors = ((IEnumerable<Photon.Realtime.Player>) PhotonNetwork.PlayerList).Where<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (plr => plr.ActorNumber != target.ActorNumber)).Select<Photon.Realtime.Player, int>((Func<Photon.Realtime.Player, int>) (plr => plr.ActorNumber)).ToArray<int>()
        });
        ((Component) VRRig.LocalRig).transform.position = new Vector3(UnityEngine.Random.Range(-99999f, 99999f), 99999f, UnityEngine.Random.Range(-99999f, 99999f));
        PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonViewFromVRRig(), new RaiseEventOptions()
        {
          TargetActors = new int[1]{ target.ActorNumber }
        });
        Safety.RPCShield();
        ((Component) VRRig.LocalRig).transform.position = position;
        return false;
      });
    else
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
  }

  public static void OnlyVisibleToPlayerGun()
  {
    if (GunLib.SetupLockOnGun())
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        NetPlayer target = RigManager.GetNetPlayerFromVRRig(GunLib.lockedTargetRig);
        PhotonSerializer.BroadcastViews(true, new PhotonView[1]
        {
          VRRig.LocalRig.GetPhotonViewFromVRRig()
        });
        Vector3 position = ((Component) VRRig.LocalRig).transform.position;
        PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonViewFromVRRig(), new RaiseEventOptions()
        {
          TargetActors = new int[1]{ target.ActorNumber }
        });
        ((Component) VRRig.LocalRig).transform.position = new Vector3(UnityEngine.Random.Range(-99999f, 99999f), 99999f, UnityEngine.Random.Range(-99999f, 99999f));
        PhotonSerializer.SendSerialize(VRRig.LocalRig.GetPhotonViewFromVRRig(), new RaiseEventOptions()
        {
          TargetActors = ((IEnumerable<Photon.Realtime.Player>) PhotonNetwork.PlayerList).Where<Photon.Realtime.Player>((Func<Photon.Realtime.Player, bool>) (plr => plr.ActorNumber != target.ActorNumber)).Select<Photon.Realtime.Player, int>((Func<Photon.Realtime.Player, int>) (plr => plr.ActorNumber)).ToArray<int>()
        });
        Safety.RPCShield();
        ((Component) VRRig.LocalRig).transform.position = position;
        return false;
      });
    else
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
  }

  public static void PiggybackAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.up * 0.3f - ((Component) targetRig).transform.forward * 0.4f;
      ((Component) VRRig.LocalRig).transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) targetRig).transform.right * -0.3f + ((Component) targetRig).transform.up * 0.1f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) VRRig.LocalRig).transform.position + ((Component) targetRig).transform.right * 0.3f + ((Component) targetRig).transform.up * 0.1f;
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
      VRRig.LocalRig.head.rigTarget.transform.rotation = ((Component) targetRig).transform.rotation;
    }), true);
  }

  public static void GawkGawkAll()
  {
    PhotonSerializer.PerPlayerRigOverride((Action<VRRig>) (targetRig =>
    {
      ((Component) VRRig.LocalRig).transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.forward * (float) (0.20000000298023224 + (double) Mathf.Sin((float) Time.frameCount / 8f) * 0.10000000149011612) + ((Component) targetRig).transform.up * -0.4f;
      ((Component) VRRig.LocalRig).transform.rotation = Quaternion.Euler(((Component) targetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
      VRRig.LocalRig.leftHand.rigTarget.transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.right * 0.2f + ((Component) targetRig).transform.up * -0.4f;
      VRRig.LocalRig.rightHand.rigTarget.transform.position = ((Component) targetRig).transform.position + ((Component) targetRig).transform.right * -0.2f + ((Component) targetRig).transform.up * -0.4f;
      VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) targetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
      VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler(((Component) targetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
      VRRig.LocalRig.head.rigTarget.transform.rotation = Quaternion.Euler(((Component) targetRig).transform.rotation.eulerAngles + new Vector3(0.0f, 180f, 0.0f));
    }));
  }

  public static void NoFingerMovement()
  {
    VRRig offlineVrRig = Variables.taggerInstance.offlineVRRig;
    ((VRMap) offlineVrRig.leftIndex).calcT = 0.0f;
    ((VRMap) offlineVrRig.leftIndex).LerpFinger(1f, false);
    ((VRMap) offlineVrRig.leftMiddle).calcT = 0.0f;
    ((VRMap) offlineVrRig.leftMiddle).LerpFinger(1f, false);
    ((VRMap) offlineVrRig.leftThumb).calcT = 0.0f;
    ((VRMap) offlineVrRig.leftThumb).LerpFinger(1f, false);
    ((VRMap) offlineVrRig.rightIndex).calcT = 0.0f;
    ((VRMap) offlineVrRig.rightIndex).LerpFinger(1f, false);
    ((VRMap) offlineVrRig.rightMiddle).calcT = 0.0f;
    ((VRMap) offlineVrRig.rightMiddle).LerpFinger(1f, false);
    ((VRMap) offlineVrRig.rightThumb).calcT = 0.0f;
    ((VRMap) offlineVrRig.rightThumb).LerpFinger(1f, false);
  }

  private static float HeadY
  {
    get => ((Component) Variables.taggerInstance.headCollider).transform.rotation.eulerAngles.y;
  }

  private static void SetHands(Vector3 left, Vector3 right, Quaternion? rot = null)
  {
    Quaternion quaternion = rot ?? ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation;
    Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = left;
    Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = right;
    Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = quaternion;
    Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = quaternion;
  }

  private static void SetRigRotation(float x, float y, float z)
  {
    ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(x, y, z);
  }

  public static void FreezeRig()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (InputHandler.isOn)
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.SetPositionAndRotation(((Component) Variables.taggerInstance.headCollider).transform.position, ((Component) Variables.taggerInstance.headCollider).transform.rotation);
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void GrabRig()
  {
    Transform transform = !InputHandler.RGrip() || InputHandler.LGrip() ? (!InputHandler.LGrip() || InputHandler.RGrip() ? (Transform) null : Variables.taggerInstance.leftHandTransform) : Variables.taggerInstance.rightHandTransform;
    if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.SetPositionAndRotation(transform.position, transform.rotation);
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = transform.rotation;
      Player.SetHands(transform.position + transform.right * -0.3f, transform.position + transform.right * 0.3f, new Quaternion?(transform.rotation));
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void FakeBodyTracking()
  {
    ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Camera.main.transform.rotation;
    Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = Variables.playerInstance.LeftHand.handFollower.transform.position;
    Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = Variables.playerInstance.RightHand.handFollower.transform.position;
  }

  public static void TPose()
  {
    if (!InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, 0.15f, 0.0f);
      Player.SetRigRotation(0.0f, Player.HeadY, 0.0f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1.2f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1.2f);
    }
  }

  public static void LayOnBack()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (InputHandler.isOn)
    {
      if (!Player.wasRigDisabled)
      {
        Player._layOnBackPos = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, -0.15f, 0.0f);
        Player.wasRigDisabled = true;
      }
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = Player._layOnBackPos;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(-90f, 0.0f, 0.0f);
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(0.0f, 90f, -90f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
    }
    else
    {
      Player.wasRigDisabled = false;
      Player.SetRigStatus(true);
      Player.ResetHeadRotation();
    }
  }

  public static void LayOnStomach()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (InputHandler.isOn)
    {
      if (!Player.wasRigDisabled)
      {
        Player._layOnStomachPos = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, -0.15f, 0.0f);
        Player.wasRigDisabled = true;
      }
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = Player._layOnStomachPos;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(90f, 0.0f, 0.0f);
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(0.0f, 90f, 90f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.25f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * -0.5f);
    }
    else
    {
      Player.wasRigDisabled = false;
      Player.SetRigStatus(true);
      Player.ResetHeadRotation();
    }
  }

  public static void UpsideDown()
  {
    if (!InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(0.0f, Player.HeadY, 180f);
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(-((Component) Variables.taggerInstance.headCollider).transform.rotation.eulerAngles.x, ((Component) Variables.taggerInstance.headCollider).transform.rotation.eulerAngles.y, 180f);
      Vector3 position1 = Variables.playerInstance.LeftHand.controllerTransform.position;
      Vector3 position2 = Variables.playerInstance.RightHand.controllerTransform.position;
      Vector3 position3 = ((Component) Variables.taggerInstance.offlineVRRig).transform.position;
      Vector3 vector3_1 = position3 + (position1 - position3) * -1f;
      Vector3 vector3_2 = position3 + (position2 - position3) * -1f;
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = vector3_1;
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = vector3_2;
      Quaternion rotation1 = Variables.playerInstance.LeftHand.controllerTransform.rotation;
      Quaternion rotation2 = Variables.playerInstance.RightHand.controllerTransform.rotation;
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = new Quaternion(-rotation1.x, rotation1.y, rotation1.z, -rotation1.w);
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = new Quaternion(-rotation2.x, rotation2.y, rotation2.z, -rotation2.w);
    }
  }

  public static void Backflip()
  {
    if (!InputHandler.RTrigger())
    {
      Player._backflipAngle = 0.0f;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, 0.15f, 0.0f);
      Player._backflipAngle = (float) (((double) Player._backflipAngle - 360.0 * (double) Time.deltaTime + 360.0) % 360.0);
      Player.SetRigRotation(Player._backflipAngle, Player.HeadY, 0.0f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.6f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.2f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.6f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.2f);
    }
  }

  public static void DisableBackflip()
  {
    Player._backflipAngle = 0.0f;
    Player.SetRigStatus(true);
  }

  public static void Frontflip()
  {
    if (!InputHandler.RTrigger())
    {
      Player._frontflipAngle = 0.0f;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, 0.15f, 0.0f);
      Player._frontflipAngle = (float) (((double) Player._frontflipAngle + 360.0 * (double) Time.deltaTime) % 360.0);
      Player.SetRigRotation(Player._frontflipAngle, Player.HeadY, 0.0f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.6f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.2f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.6f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.2f);
    }
  }

  public static void DisableFrontflip()
  {
    Player._frontflipAngle = 0.0f;
    Player.SetRigStatus(true);
  }

  public static void Cartwheel()
  {
    if (!InputHandler.RTrigger())
    {
      Player._cartWheelAngle = 0.0f;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, 0.15f, 0.0f);
      Player._cartWheelAngle = (float) (((double) Player._cartWheelAngle + 270.0 * (double) Time.deltaTime) % 360.0);
      Player.SetRigRotation(0.0f, Player.HeadY, Player._cartWheelAngle);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1.2f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1.2f);
    }
  }

  public static void DisableCartwheel()
  {
    Player._cartWheelAngle = 0.0f;
    Player.SetRigStatus(true);
  }

  public static void GriddyMonke()
  {
    if (!InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation;
      Vector3 vector3_1 = ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.25f + ((Component) Variables.taggerInstance.offlineVRRig).transform.forward * 0.5f * Mathf.Cos((float) Time.frameCount / 10f) + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * -0.3f * Mathf.Abs(Mathf.Sin((float) Time.frameCount / 7f));
      Vector3 vector3_2 = ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.25f + ((Component) Variables.taggerInstance.offlineVRRig).transform.forward * 0.5f * Mathf.Cos((float) Time.frameCount / 10f) + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * -0.3f * Mathf.Abs(Mathf.Sin((float) Time.frameCount / 7f));
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + vector3_1, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + vector3_2);
    }
  }

  public static void DanceMonke()
  {
    if (!InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
      Player.ResetHeadRotation();
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      float f1 = Time.time * 2f;
      float f2 = Time.time * 4f;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(0.0f, Player.HeadY + Mathf.Sin(f1) * 30f, 0.0f);
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, (float) (0.15000000596046448 - (double) Mathf.Abs(Mathf.Sin(f1 * 2f)) * 0.30000001192092896), 0.0f);
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.8f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * (float) (0.30000001192092896 + (double) Mathf.Sin(f2) * 0.40000000596046448) + ((Component) Variables.taggerInstance.offlineVRRig).transform.forward * Mathf.Cos(f2) * 0.3f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.8f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * (float) (0.30000001192092896 + (double) Mathf.Sin(f2 + 3.14159274f) * 0.40000000596046448) + ((Component) Variables.taggerInstance.offlineVRRig).transform.forward * Mathf.Cos(f2 + 3.14159274f) * 0.3f);
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = Quaternion.Euler((float) (90.0 + (double) Mathf.Sin(f2) * 45.0), 0.0f, Mathf.Cos(f2) * 30f);
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = Quaternion.Euler((float) (90.0 + (double) Mathf.Sin(f2 + 3.14159274f) * 45.0), 0.0f, Mathf.Cos(f2 + 3.14159274f) * 30f);
      Player.SetHeadRotation(new Vector3?(new Vector3(Mathf.Sin(f1) * 15f, Player.HeadY, Mathf.Cos(f2) * 10f)));
    }
  }

  public static void SpazMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      Player.ResetHeadRotation();
    }
    else
    {
      Vector3 vector3 = new Vector3(UnityEngine.Random.Range(0.0f, 360f), UnityEngine.Random.Range(0.0f, 360f), UnityEngine.Random.Range(0.0f, 360f));
      Player.SetHeadRotation(new Vector3?(vector3));
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.eulerAngles = vector3;
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.eulerAngles = vector3;
    }
  }

  public static void SpiderMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      Player.DisableSpiderMonke();
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      Vector3 position = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
      if (!Player._spiderInit)
      {
        Player._spiderLastPos = position;
        Player._spiderInit = true;
      }
      float num = (position - Player._spiderLastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
      Player._spiderLastPos = position;
      Player._spider += (float) ((double) num * (double) Time.deltaTime * 1.6000000238418579);
      Player._spiderIdle += Time.deltaTime;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = position + new Vector3(0.0f, 0.02f, 0.0f);
      Player.SetRigRotation(80f, Player.HeadY, Mathf.Sin(Player._spiderIdle * 4f) * 5f);
      if ((double) num > 0.40000000596046448)
        Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(-25f, Player.HeadY + Mathf.Sin(Player._spider * 5f) * 10f, Mathf.Sin(Player._spider * 6f) * 8f);
      else
        Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = Quaternion.Euler(-20f, Player.HeadY + Mathf.Sin(Player._spiderIdle * 0.8f) * 35f, Mathf.Sin(Player._spiderIdle * 0.6f) * 10f);
      float phase1 = Player._spider % 1f;
      float phase2 = (float) (((double) Player._spider + 0.5) % 1.0);
      Player.SetCrawlHand(Variables.taggerInstance.offlineVRRig.leftHand.rigTarget, phase1, -0.45f);
      Player.SetCrawlHand(Variables.taggerInstance.offlineVRRig.rightHand.rigTarget, phase2, 0.45f);
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.rotation = ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation;
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.rotation = ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation;
    }
  }

  private static void SetCrawlHand(Transform hand, float phase, float side)
  {
    Transform transform = ((Component) Variables.taggerInstance.offlineVRRig).transform;
    float num1;
    float num2;
    if ((double) phase < 0.30000001192092896)
    {
      float t = phase / 0.3f;
      num1 = Mathf.Lerp(-0.5f, 0.7f, t);
      num2 = Mathf.Sin(t * 3.14159274f) * 0.25f;
    }
    else
    {
      num1 = Mathf.Lerp(0.7f, -0.5f, (float) (((double) phase - 0.30000001192092896) / 0.699999988079071));
      num2 = 0.0f;
    }
    hand.transform.position = transform.position + transform.forward * num1 + transform.right * side + transform.up * (num2 - 0.4f);
  }

  public static void DisableSpiderMonke()
  {
    Player._spider = 0.0f;
    Player._spiderIdle = 0.0f;
    Player._spiderInit = false;
    Player.SetRigStatus(true);
  }

  public static void GlitchMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      Player.DisableGlitchMonke();
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      Player._glitchOffset += Time.deltaTime;
      Player._glitchTimer += Time.deltaTime;
      if ((double) Player._glitchTimer > 0.05000000074505806)
      {
        Player._glitchTimer = 0.0f;
        Player._glitchPos = new Vector3(UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f), UnityEngine.Random.Range(-0.05f, 0.05f));
      }
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, 0.15f, 0.0f) + Player._glitchPos;
      Player.SetRigRotation(UnityEngine.Random.Range(-5f, 5f), Player.HeadY + UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f));
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.4f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.1f, ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.4f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * 0.1f);
    }
  }

  public static void DisableGlitchMonke()
  {
    Player._glitchOffset = 0.0f;
    Player._glitchTimer = 0.0f;
    Player._glitchPos = Vector3.zero;
    Player.SetRigStatus(true);
  }

  public static void WobblyMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      Player.DisableWobblyMonke();
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      Player._wobbleOffset += Time.deltaTime;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, (float) (0.15000000596046448 + (double) Mathf.Sin(Player._wobbleOffset * 3f) * 0.05000000074505806), 0.0f);
      Player.SetRigRotation(Mathf.Sin(Player._wobbleOffset * 2.5f) * 15f, Player.HeadY, Mathf.Cos(Player._wobbleOffset * 2f) * 15f);
      float num1 = Mathf.Sin(Player._wobbleOffset * 3f) * 0.15f;
      float num2 = Mathf.Cos(Player._wobbleOffset * 3f) * 0.15f;
      Player.SetHands(((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -0.5f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * (0.2f + num1), ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 0.5f + ((Component) Variables.taggerInstance.offlineVRRig).transform.up * (0.2f + num2));
    }
  }

  public static void DisableWobblyMonke()
  {
    Player._wobbleOffset = 0.0f;
    Player.SetRigStatus(true);
  }

  public static void HelicopterMonke()
  {
    if (InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position += new Vector3(0.0f, 0.075f, 0.0f);
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Quaternion.Euler(((Component) Variables.taggerInstance.offlineVRRig).transform.rotation.eulerAngles + new Vector3(0.0f, 10f, 0.0f));
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1f;
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1f;
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void AscendingMonke()
  {
    if (InputHandler.RTrigger())
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position += new Vector3(0.0f, 0.01f, 0.0f);
      Variables.taggerInstance.offlineVRRig.head.rigTarget.transform.rotation = ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation;
      Variables.taggerInstance.offlineVRRig.leftHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * -1f;
      Variables.taggerInstance.offlineVRRig.rightHand.rigTarget.transform.position = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + ((Component) Variables.taggerInstance.offlineVRRig).transform.right * 1f;
      Player.SetHeadRotation(new Vector3?(new Vector3(180f, 0.0f, 0.0f)));
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void RagdollMonke()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.RSecondary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      if (!((UnityEngine.Object) Player._ragdollBody != (UnityEngine.Object) null))
        return;
      Player.DisableRagdollMonke();
    }
    else
    {
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      if ((UnityEngine.Object) Player._ragdollBody == (UnityEngine.Object) null)
      {
        Player._ragdollObj = new GameObject("RagdollPhysics");
        Player._ragdollCollider = Player._ragdollObj.AddComponent<BoxCollider>();
        Player._ragdollCollider.size = new Vector3(0.4f, 0.9f, 0.4f);
        Player._ragdollCollider.center = new Vector3(0.0f, -0.3f, 0.0f);
        Player._ragdollBody = Player._ragdollObj.AddComponent<Rigidbody>();
        Player._ragdollBody.isKinematic = false;
        Player._ragdollBody.mass = 1f;
        Player._ragdollBody.drag = 0.5f;
        Player._ragdollBody.angularDrag = 0.5f;
        Player._ragdollBody.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
        Player._ragdollObj.layer = 8;
      }
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = Player._ragdollObj.transform.position;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation = Player._ragdollObj.transform.rotation;
    }
  }

  public static void DisableRagdollMonke()
  {
    if ((UnityEngine.Object) Player._ragdollObj != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) Player._ragdollObj);
      Player._ragdollObj = (GameObject) null;
      Player._ragdollBody = (Rigidbody) null;
      Player._ragdollCollider = (BoxCollider) null;
    }
    Player.SetRigStatus(true);
  }
}
