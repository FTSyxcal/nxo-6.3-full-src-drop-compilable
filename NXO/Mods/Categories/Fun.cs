
using GorillaExtensions;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using GorillaNetworking;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Fun
{
  private static readonly Vector3[] lastRight = new Vector3[10];
  private static readonly Vector3[] lastLeft = new Vector3[10];
  private static GameObject rightPointer;
  private static GameObject leftPointer;
  private static readonly HashSet<GameObject> drawings = new HashSet<GameObject>();
  private static bool colorChangerCooldown = false;
  private static int currentColor = 0;
  private static Color drawColor = Color.white;
  private static GameObject bomb;
  private static float orbitSpeed = 100f;
  private static float orbitRadius = 1.5f;
  private static float orbitAngle = 0.0f;
  private static bool becomingEntity;
  private static bool togglePressed;
  private static float hoverboardDelay;
  private static Coroutine hoverboardCoroutine;
  public static Coroutine BugCoroutine;
  private static bool lastBecomeWasNull;
  private static int currentCosmetics = 0;
  public static CosmeticsController.CosmeticItem currentCosmeticItem;
  public static string cosmeticName = "No Cosmetic Selected";
  public static ButtonHandler.Button cosmeticsAddButton;
  private static float hoverboardAuraDelay;
  private static bool strobeState;
  private static float strobeDelay;
  private static BetterDayNightManager.WeatherType[] originalWeatherCycle;
  private static GameObject[] elevatorAmbientObjects;
  private static float splashCooldown = 0.25f;
  private static float rpcCooldown;
  private static WaterVolume[] cachedWater;
  public static GameObject airSwimPart;
  private static int[] archiveCosmetics;
  private static int[] cachedLmajuCosmetics;
  private static bool cosmeticsEnabled;
  public static readonly HashSet<string> serverSidedItems = new HashSet<string>();
  public static bool serverSidedArmed;
  private static readonly Vector3 tryOnRoomPos = new Vector3(-51.7836f, 16.7865f, -119.6697f);
  private static bool serverSideBusy;
  private static float braceletSpamDelay;
  private static GameObject rightaimer;
  private static GameObject leftaimer;
  private static LineRenderer lr;
  private static LineRenderer leftlr;
  private static SpringJoint joint;
  private static SpringJoint leftjoint;
  private static bool cangrapple = true;
  private static bool canleftgrapple = true;
  private static bool wackstart = false;
  private const float maxDistance = 100f;
  private const float Spring = 5000f;
  private const float Damper = 4000f;
  private const float MassScale = 6f;
  private const float pullspeed = 3f;
  private const float speedtopull = 2.5f;
  private static GameObject rightGrappleAimer;
  private static GameObject leftGrappleAimer;
  private static LineRenderer grappleLR;
  private static LineRenderer leftGrappleLR;
  private static Vector3 rightGrapplePoint;
  private static Vector3 leftGrapplePoint;
  private static bool rightGrappleAttached;
  private static bool leftGrappleAttached;
  private static bool grappleStart;

  private static void DestroyAllDrawings()
  {
    foreach (GameObject drawing in Fun.drawings)
    {
      if ((UnityEngine.Object) drawing != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) drawing);
    }
    Fun.drawings.Clear();
  }

  public static IEnumerator ReturnRig()
  {
    yield return (object) new WaitForSeconds(0.2f);
    ((Behaviour) VRRig.LocalRig).enabled = true;
    Fun.BugCoroutine = (Coroutine) null;
  }

  private static IEnumerator EnableRig()
  {
    yield return (object) new WaitForSeconds(0.3f);
    ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  private static IEnumerator ResetHoverboard()
  {
    yield return (object) new WaitForSeconds(0.3f);
    GTPlayer.Instance.SetHoverActive(false);
    VRRig.LocalRig.hoverboardVisual.SetNotHeld();
  }

  public static void GrabEntity(string name)
  {
    if (!InputHandler.RGrip())
      return;
    GameObject gameObject = GameObject.Find(name);
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.transform.position = Variables.taggerInstance.rightHandTransform.position;
    gameObject.transform.rotation = Variables.taggerInstance.rightHandTransform.rotation;
  }

  public static void EntityGun(string name)
  {
    if (!GunLib.SetupGun())
      return;
    GameObject gameObject = GameObject.Find(name);
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      gameObject.transform.position = (GunLib.raycastHit).point + Vector3.up;
  }

  public static void EntityOrbit(string name, float offset = 0.0f)
  {
    GameObject gameObject = GameObject.Find(name);
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position + new Vector3(MathF.Cos(offset + (float) Time.frameCount / 30f), 1f, MathF.Sin(offset + (float) Time.frameCount / 30f));
  }

  public static void SpazEntity(string name)
  {
    GameObject gameObject = GameObject.Find(name);
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.transform.rotation = Variables.RandomQuaternion();
  }

  public static void FreezeEntity(string name)
  {
    GameObject gameObject = GameObject.Find(name);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    ThrowableBug component = gameObject.GetComponent<ThrowableBug>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    component.bugRotationalVelocity = Quaternion.identity;
    component.targetVelocity = Vector3.zero;
    component.thrownVeloicity = Vector3.zero;
    component.thrownYVelocity = 0.0f;
    component.reliableState.travelingDirection = Vector3.zero;
  }

  public static void SetEntitySpeed(string name, float speed)
  {
    GameObject gameObject = GameObject.Find(name);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    ThrowableBug component = gameObject.GetComponent<ThrowableBug>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    component.maxNaturalSpeed = speed;
  }

  public static void RideEntity(string name)
  {
    GameObject gameObject = GameObject.Find(name);
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    Movement.TeleportTo(gameObject.transform.position);
    ((Component) Variables.taggerInstance).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  public static void BecomeEntity(string name)
  {
    GameObject gameObject = GameObject.Find(name);
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
    {
      Player.SetRigStatus(false);
      ((Component) VRRig.LocalRig).transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position - Vector3.up * 99999f;
      gameObject.transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
      gameObject.transform.rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation;
    }
    else if (Fun.lastBecomeWasNull)
      Player.SetRigStatus(true);
    Fun.lastBecomeWasNull = (UnityEngine.Object) gameObject != (UnityEngine.Object) null;
  }

  public static void DestroyEntity(string name)
  {
    GameObject gameObject = GameObject.Find(name);
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.transform.position = new Vector3(99999f, 99999f, 99999f);
  }

  private static string ToTitleCase(string text)
  {
    if (string.IsNullOrEmpty(text))
      return text;
    string[] strArray = text.ToLower().Split(' ');
    for (int index = 0; index < strArray.Length; ++index)
    {
      if (strArray[index].Length > 0)
        strArray[index] = char.ToUpper(strArray[index][0]).ToString() + strArray[index].Substring(1);
    }
    return string.Join(" ", strArray);
  }

  public static void CycleCosmetics(bool forward)
  {
    List<CosmeticsController.CosmeticItem> allCosmetics = CosmeticsController.instance._allCosmetics;
    int currentCosmetics = Fun.currentCosmetics;
    do
    {
      Fun.currentCosmetics += forward ? 1 : -1;
      if (Fun.currentCosmetics >= allCosmetics.Count)
        Fun.currentCosmetics = 0;
      if (Fun.currentCosmetics < 0)
        Fun.currentCosmetics = allCosmetics.Count - 1;
    }
    while ((!allCosmetics[Fun.currentCosmetics].canTryOn || allCosmetics[Fun.currentCosmetics].isNullItem) && Fun.currentCosmetics != currentCosmetics);
    Fun.currentCosmeticItem = allCosmetics[Fun.currentCosmetics];
    Fun.cosmeticName = Fun.ToTitleCase(CosmeticsController.instance.GetItemDisplayName(Fun.currentCosmeticItem));
    Fun.cosmeticsAddButton?.SetText($"{Fun.cosmeticName} ({Fun.currentCosmeticItem.cost})");
  }

  public static void AddCosmetic()
  {
    CosmeticsController.instance.currentCart.Insert(0, Fun.currentCosmeticItem);
    CosmeticsController.instance.UpdateShoppingCart();
  }

  public static void BuyFreeCosmetics()
  {
    foreach (CosmeticsController.CosmeticItem allCosmetic in CosmeticsController.instance._allCosmetics)
    {
      if (allCosmetic.canTryOn)
      {
        CosmeticsController.instance.itemToBuy = allCosmetic;
        CosmeticsController.instance.currentPurchaseItemStage = (CosmeticsController.PurchaseItemStages) 4;
        CosmeticsController.instance.PurchaseItem();
      }
    }
    CosmeticsController.instance.UpdateWardrobeModelsAndButtons();
  }

  public static void DropHoverboard(
    Vector3 pos,
    Quaternion rot,
    Vector3 vel,
    Vector3 angVel,
    Color color)
  {
    if ((double) Vector3.Distance(((Component) Variables.taggerInstance.bodyCollider).transform.position, pos) > 5.0)
    {
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = pos + Vector3.down * 4f;
      if (Fun.hoverboardCoroutine != null)
        CoroutineHelper.Instance.StopCoroutine(Fun.hoverboardCoroutine);
      Fun.hoverboardCoroutine = CoroutineHelper.Instance.StartCoroutine(Fun.EnableRig());
    }
    FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, angVel, color);
    Safety.RPCShield();
  }

  public static void SpawnHoverboard()
  {
    Fun.DropHoverboard(((Component) VRRig.LocalRig).transform.position, ((Component) VRRig.LocalRig).transform.rotation, Vector3.zero, Vector3.zero, Variables.RandomColor());
    GTPlayer.Instance.isHoverAllowed = true;
  }

  public static void ShootHoverboards()
  {
    if (!InputHandler.RGrip() || (double) Time.time < (double) Fun.hoverboardDelay)
      return;
    Fun.hoverboardDelay = Time.time + 0.5f;
    Transform rightHandTransform = Variables.taggerInstance.rightHandTransform;
    Fun.DropHoverboard(rightHandTransform.position, rightHandTransform.rotation, rightHandTransform.forward * 10f, Vector3.zero, Variables.RandomColor());
  }

  public static void OrbitHoverboards()
  {
    if ((double) Time.time < (double) Fun.hoverboardDelay)
      return;
    Fun.hoverboardDelay = Time.time + 0.25f;
    Vector3 position = ((Component) Variables.taggerInstance.headCollider).transform.position;
    float num1 = (float) Time.frameCount / 30f;
    for (int index = 0; index < 2; ++index)
    {
      float num2 = (float) index * 180f;
      float num3 = num2 - 25f;
      Vector3 vector3_1 = new Vector3(MathF.Cos(num2 + num1) * 2f, 1f, MathF.Sin(num2 + num1) * 2f);
      Vector3 vector3_2 = new Vector3(MathF.Cos(num3 + num1) * 2f, 1f, MathF.Sin(num3 + num1) * 2f);
      Fun.DropHoverboard(position + vector3_1, Quaternion.Euler((position - vector3_1).normalized), (vector3_2 - vector3_1).normalized * 6.5f, new Vector3(0.0f, 360f, 0.0f), Variables.RandomColor());
    }
  }

  public static void HoverboardGun()
  {
    if (!GunLib.GunGrips)
    {
      GunLib.SetGunVisibility(false);
    }
    else
    {
      GunLib.SetupRaycast();
      if (!GunLib.GunTriggers || (double) Time.time <= (double) Fun.hoverboardDelay)
        return;
      Fun.hoverboardDelay = Time.time + 0.25f;
      Fun.DropHoverboard((GunLib.raycastHit).point + Vector3.up, Variables.RandomQuaternion(), Vector3.zero, Vector3.zero, Variables.RandomColor());
    }
  }

  public static void HoverboardAura()
  {
    if ((double) Time.time < (double) Fun.hoverboardAuraDelay)
      return;
    Fun.hoverboardAuraDelay = Time.time + 0.25f;
    for (int index = 0; index < 2; ++index)
      Fun.DropHoverboard(((Component) Variables.taggerInstance.headCollider).transform.position + Variables.RandomVector3(), Variables.RandomQuaternion(), Variables.RandomVector3(20f), Variables.RandomVector3(20f), Variables.RandomColor());
  }

  public static void SpazHoverboard()
  {
    if (!((UnityEngine.Object) VRRig.LocalRig.hoverboardVisual != (UnityEngine.Object) null) || !VRRig.LocalRig.hoverboardVisual.IsHeld)
      return;
    VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, Variables.RandomQuaternion(), Variables.RandomColor());
  }

  public static void RainbowHoverboard()
  {
    if ((UnityEngine.Object) VRRig.LocalRig.hoverboardVisual == (UnityEngine.Object) null || !VRRig.LocalRig.hoverboardVisual.IsHeld)
      return;
    VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, VRRig.LocalRig.hoverboardVisual.NominalLocalRotation, Color.HSVToRGB((float) ((double) Time.frameCount / 180.0 % 1.0), 1f, 1f));
  }

  public static void StrobeHoverboard()
  {
    if ((UnityEngine.Object) VRRig.LocalRig.hoverboardVisual == (UnityEngine.Object) null || !VRRig.LocalRig.hoverboardVisual.IsHeld)
      return;
    if ((double) Time.time > (double) Fun.strobeDelay)
    {
      Fun.strobeDelay = Time.time + 0.1f;
      Fun.strobeState = !Fun.strobeState;
    }
    VRRig.LocalRig.hoverboardVisual.SetIsHeld(VRRig.LocalRig.hoverboardVisual.IsLeftHanded, VRRig.LocalRig.hoverboardVisual.NominalLocalPosition, VRRig.LocalRig.hoverboardVisual.NominalLocalRotation, Fun.strobeState ? Color.white : Color.black);
  }

  public static void BecomeHoverboard()
  {
    Vector3 position = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
    Quaternion rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation;
    ((Behaviour) VRRig.LocalRig).enabled = false;
    ((Component) VRRig.LocalRig).transform.position = position - Vector3.up;
    GTPlayer.Instance.isHoverAllowed = true;
    GTPlayer.Instance.SetHoverActive(true);
    HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
    Transform nominalParentTransform = hoverboardVisual.NominalParentTransform;
    hoverboardVisual.SetIsHeld(true, nominalParentTransform.InverseTransformPoint(position), GTExt.InverseTransformRotation(nominalParentTransform, rotation), VRRig.LocalRig.playerColor);
    hoverboardVisual.interpolatedLocalPosition = hoverboardVisual.NominalLocalPosition;
    hoverboardVisual.interpolatedLocalRotation = hoverboardVisual.NominalLocalRotation;
    GTPlayer.Instance.SetHoverboardPosRot(position, rotation);
  }

  public static void HoverboardScreenTarget(VRRig rig, Color color)
  {
    if (Fun.hoverboardCoroutine != null)
      CoroutineHelper.Instance.StopCoroutine(Fun.hoverboardCoroutine);
    Fun.hoverboardCoroutine = CoroutineHelper.Instance.StartCoroutine(Fun.ResetHoverboard());
    Vector3 angularVelocity = GTExt.GetOrAddComponent<GorillaVelocityEstimator>(rig.headMesh).angularVelocity;
    Vector3 position = rig.headMesh.transform.TransformPoint(-0.3f, 0.1f, 0.3725f) + rig.LatestVelocity() * 0.5f;
    Quaternion quaternion = rig.headMesh.transform.rotation * Quaternion.Euler(angularVelocity * 5.729578f) * Quaternion.Euler(0.0f, 90f, 270f);
    ((Behaviour) VRRig.LocalRig).enabled = false;
    ((Component) VRRig.LocalRig).transform.position = position - Vector3.up * 0.5f;
    GTPlayer.Instance.isHoverAllowed = true;
    HoverboardVisual hoverboardVisual = VRRig.LocalRig.hoverboardVisual;
    Transform nominalParentTransform = hoverboardVisual.NominalParentTransform;
    hoverboardVisual.SetIsHeld(true, nominalParentTransform.InverseTransformPoint(position), GTExt.InverseTransformRotation(nominalParentTransform, quaternion), color);
    GTPlayer.Instance.SetHoverActive(false);
    hoverboardVisual.interpolatedLocalPosition = hoverboardVisual.NominalLocalPosition;
    hoverboardVisual.interpolatedLocalRotation = hoverboardVisual.NominalLocalRotation;
    GTPlayer.Instance.SetHoverboardPosRot(position, quaternion);
  }

  public static void HoverboardScreenAll(Color color)
  {
    if (InputHandler.RTrigger())
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        if (!PhotonNetwork.InRoom)
          return true;
        PhotonSerializer.BroadcastViews(true, new PhotonView[1]
        {
          Variables.taggerInstance.myVRRig.GetView
        });
        Vector3 position = ((Component) VRRig.LocalRig).transform.position;
        foreach (NetPlayer playerListOther in NetworkSystem.Instance.PlayerListOthers)
        {
          VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(playerListOther);
          if (!((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null))
          {
            Fun.HoverboardScreenTarget(rigFromNetPlayer, color);
            PhotonSerializer.SendSerialize(Variables.taggerInstance.myVRRig.GetView, new RaiseEventOptions()
            {
              TargetActors = new int[1]
              {
                playerListOther.ActorNumber
              }
            });
          }
        }
        Safety.RPCShield();
        ((Behaviour) VRRig.LocalRig).enabled = true;
        ((Component) VRRig.LocalRig).transform.position = position;
        return false;
      });
    else
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
  }

  public static void HoverboardScreenGun(Color color)
  {
    if (GunLib.SetupLockOnGun())
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null)
          return false;
        Fun.HoverboardScreenTarget(GunLib.lockedTargetRig, color);
        PhotonSerializer.SendSerialize(Variables.taggerInstance.myVRRig.GetView);
        return false;
      });
    }
    else
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      ((Behaviour) VRRig.LocalRig).enabled = true;
    }
  }

  public static void GrabGliders()
  {
    if (!InputHandler.RGrip())
      return;
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
      {
        ((Component) gliderHoldable).gameObject.transform.position = Variables.taggerInstance.rightHandTransform.position;
        ((Component) gliderHoldable).gameObject.transform.rotation = Variables.taggerInstance.rightHandTransform.rotation;
      }
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void SpazGliders()
  {
    if (!InputHandler.RTrigger())
      return;
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
        ((Component) gliderHoldable).gameObject.transform.rotation = Variables.RandomQuaternion();
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void OrbitGliders()
  {
    if (!InputHandler.RTrigger())
      return;
    GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>();
    int num1 = 0;
    foreach (GliderHoldable gliderHoldable in allType)
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
      {
        float num2 = 360f / (float) allType.Length * (float) num1;
        ((Component) gliderHoldable).gameObject.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position + new Vector3(MathF.Cos(num2 + (float) Time.frameCount / 30f) * 5f, 2f, MathF.Sin(num2 + (float) Time.frameCount / 30f) * 5f);
      }
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
      ++num1;
    }
  }

  public static void GliderGun()
  {
    if (!GunLib.SetupGun())
      return;
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
        ((Component) gliderHoldable).gameObject.transform.position = (GunLib.raycastHit).point + Vector3.up;
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void GliderSpeed(float pull, float drag)
  {
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      gliderHoldable.pullUpLiftBonus = pull;
      gliderHoldable.dragVsSpeedDragFactor = drag;
    }
  }

  public static void GliderBlindAll()
  {
    GliderHoldable[] allType = Variables.GetAllType<GliderHoldable>();
    int index = 0;
    if (!InputHandler.RTrigger())
      return;
    foreach (VRRig vrRig in VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (r => !r.isLocal)))
    {
      if (index < allType.Length)
      {
        GliderHoldable gliderHoldable = allType[index];
        if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
        {
          ((Component) gliderHoldable).gameObject.transform.position = vrRig.headMesh.transform.position;
          ((Component) gliderHoldable).gameObject.transform.rotation = Variables.RandomQuaternion();
        }
        else
          ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
        ++index;
      }
      else
        break;
    }
  }

  public static void GliderBlindGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
      {
        ((Component) gliderHoldable).gameObject.transform.position = GunLib.lockedTargetRig.headMesh.transform.position;
        ((Component) gliderHoldable).gameObject.transform.rotation = Variables.RandomQuaternion();
      }
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void DestroyGliders()
  {
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
        ((Component) gliderHoldable).gameObject.transform.position = new Vector3(99999f, 99999f, 99999f);
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void RespawnGliders()
  {
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
        gliderHoldable.Respawn();
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void GliderAura()
  {
    foreach (GliderHoldable gliderHoldable in Variables.GetAllType<GliderHoldable>())
    {
      if (((NetworkView) gliderHoldable).GetView.Owner == PhotonNetwork.LocalPlayer)
      {
        ((Component) gliderHoldable).gameObject.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position + Variables.RandomVector3();
        ((Component) gliderHoldable).gameObject.transform.rotation = Variables.RandomQuaternion();
      }
      else
        ((NetworkHoldableObject) gliderHoldable).OnHover((InteractionPoint) null, (GameObject) null);
    }
  }

  public static void GrabObject(string name)
  {
    if (!InputHandler.RGrip())
      return;
    Variables.FindObject(name).transform.position = Variables.taggerInstance.rightHandTransform.position;
  }

  public static void ObjectGun(string name)
  {
    if (!GunLib.GunGrips)
    {
      GunLib.SetGunVisibility(false);
    }
    else
    {
      GunLib.SetupRaycast();
      if (!GunLib.GunTriggers)
        return;
      Variables.FindObject(name).transform.position = (GunLib.raycastHit).point;
    }
  }

  public static void ObjectOrbit(string name)
  {
    Fun.orbitAngle += Fun.orbitSpeed * Time.deltaTime;
    float f = Fun.orbitAngle * ((float) Math.PI / 180f);
    Variables.FindObject(name).transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position + new Vector3(Mathf.Cos(f), 0.0f, Mathf.Sin(f)) * Fun.orbitRadius;
  }

  public static void BecomeObject(string name)
  {
    bool flag = InputHandler.RPrimary();
    if (flag && !Fun.togglePressed)
    {
      Fun.becomingEntity = !Fun.becomingEntity;
      if (Fun.becomingEntity)
      {
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
        ((Component) Variables.taggerInstance.offlineVRRig).transform.position = new Vector3(999f, 999f, 999f);
      }
      else
        ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
      Fun.togglePressed = true;
    }
    else if (!flag)
      Fun.togglePressed = false;
    if (!Fun.becomingEntity)
      return;
    Transform transform = ((Component) Variables.taggerInstance.headCollider).transform;
    Variables.FindObject(name).transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f));
  }

  public static void RainyWeather(bool setActive)
  {
    if (setActive)
    {
      Fun.originalWeatherCycle = (BetterDayNightManager.WeatherType[]) BetterDayNightManager.instance.weatherCycle.Clone();
      for (int index = 1; index < BetterDayNightManager.instance.weatherCycle.Length; ++index)
        BetterDayNightManager.instance.weatherCycle[index] = (BetterDayNightManager.WeatherType) 1;
    }
    else
    {
      if (Fun.originalWeatherCycle != null)
        BetterDayNightManager.instance.weatherCycle = Fun.originalWeatherCycle;
      else
        BetterDayNightManager.instance.GenerateWeatherEventTimes();
      Fun.originalWeatherCycle = (BetterDayNightManager.WeatherType[]) null;
    }
  }

  public static void ElevatorBuzz(bool enable)
  {
    if (Fun.elevatorAmbientObjects == null)
      Fun.elevatorAmbientObjects = new GameObject[3]
      {
        GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/GhostReactorElevator/HasVisualComponents/Audio/Ambient"),
        GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/CityElevator/HasVisualComponents/Audio/Ambient"),
        GameObject.Find("Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/StumpElevator/HasVisualComponents/Audio/Ambient")
      };
    foreach (GameObject elevatorAmbientObject in Fun.elevatorAmbientObjects)
    {
      if ((UnityEngine.Object) elevatorAmbientObject != (UnityEngine.Object) null)
        elevatorAmbientObject.SetActive(enable);
    }
  }

  public static void AddBarrelToCart()
  {
    CosmeticsController.instance.currentCart.Insert(0, CosmeticsController.instance.GetItemFromDict("LMAPE."));
  }

  public static void DisableWindBarriers(bool enable)
  {
    MenuPatches.forceEnabled = enable;
    Variables.FindObject("Environment Objects/LocalObjects_Prefab/Forest/Environment/Forest_ForceVolumes/").SetActive(!enable);
    Variables.FindObject("Environment Objects/LocalObjects_Prefab/ForestToHoverboard/TurnOnInForestAndHoverboard/ForestDome_CollisionOnly").SetActive(!enable);
  }

  public static void PlaySplashEffect(Vector3 pos, Quaternion rot, float size)
  {
    Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlaySplashEffect", (RpcTarget) 0, new object[6]
    {
      (object) pos,
      (object) rot,
      (object) size,
      (object) size,
      (object) true,
      (object) true
    });
  }

  public static void WaterBender()
  {
    if ((double) Time.time < (double) Fun.rpcCooldown)
      return;
    Fun.rpcCooldown = Time.time + Fun.splashCooldown;
    Safety.RPCShield();
    if (InputHandler.RGrip() && InputHandler.LGrip())
    {
      Vector3 pos = (Variables.playerInstance.RightHand.controllerTransform.position + Variables.playerInstance.LeftHand.controllerTransform.position) / 2f;
      float num = Vector3.Distance(Variables.playerInstance.RightHand.controllerTransform.position, Variables.playerInstance.LeftHand.controllerTransform.position);
      Fun.PlaySplashEffect(pos, Quaternion.Lerp(Variables.playerInstance.RightHand.controllerTransform.rotation, Variables.playerInstance.LeftHand.controllerTransform.rotation, 0.5f), Mathf.Clamp(num * 100f, 75f, 500f));
    }
    else
    {
      if (InputHandler.RGrip())
        Fun.PlaySplashEffect(Variables.playerInstance.RightHand.controllerTransform.position, ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
      if (InputHandler.LGrip())
        Fun.PlaySplashEffect(Variables.playerInstance.LeftHand.controllerTransform.position, ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
    }
    if (!InputHandler.RPrimary())
      return;
    Fun.PlaySplashEffect(Variables.taggerInstance.offlineVRRig.headMesh.transform.position + Vector3.up * 0.5f, Quaternion.Euler(90f, 0.0f, 0.0f), 125f);
  }

  public static void SplashAnnoyGun()
  {
    if (GunLib.SetupLockOnGun())
    {
      if ((double) Time.time < (double) Fun.rpcCooldown)
        return;
      Fun.rpcCooldown = Time.time + Fun.splashCooldown;
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = ((Component) GunLib.lockedTargetRig).transform.position;
      Fun.PlaySplashEffect(GunLib.lockedTargetRig.headMesh.transform.position + GunLib.lockedTargetRig.headMesh.transform.forward * 0.2f, GunLib.lockedTargetRig.headMesh.transform.rotation, 999f);
      ((Behaviour) VRRig.LocalRig).enabled = true;
      Safety.RPCShield();
    }
    else
      ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  public static void SplashAnnoyAll()
  {
    if (InputHandler.RTrigger())
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        if (!PhotonNetwork.InRoom)
          return true;
        if ((double) Time.time < (double) Fun.rpcCooldown)
          return false;
        Fun.rpcCooldown = Time.time + Fun.splashCooldown;
        PhotonSerializer.BroadcastViews(true, new PhotonView[1]
        {
          Variables.taggerInstance.myVRRig.GetView
        });
        Vector3 position = ((Component) VRRig.LocalRig).transform.position;
        foreach (NetPlayer playerListOther in NetworkSystem.Instance.PlayerListOthers)
        {
          VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(playerListOther);
          if (!((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null))
          {
            ((Component) VRRig.LocalRig).transform.position = ((Component) rigFromNetPlayer).transform.position;
            PhotonSerializer.SendSerialize(Variables.taggerInstance.myVRRig.GetView, new RaiseEventOptions()
            {
              TargetActors = new int[1]
              {
                playerListOther.ActorNumber
              }
            });
            Variables.taggerInstance.myVRRig.SendRPC("RPC_PlaySplashEffect", playerListOther, new object[6]
            {
              (object) (rigFromNetPlayer.headMesh.transform.position + rigFromNetPlayer.headMesh.transform.forward * 0.2f),
              (object) rigFromNetPlayer.headMesh.transform.rotation,
              (object) 999f,
              (object) 999f,
              (object) true,
              (object) true
            });
          }
        }
        ((Component) VRRig.LocalRig).transform.position = position;
        Safety.RPCShield();
        return false;
      });
    else
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
  }

  public static void SplashSelf()
  {
    if (!InputHandler.RTrigger() || (double) Time.time < (double) Fun.rpcCooldown)
      return;
    Fun.rpcCooldown = Time.time + Fun.splashCooldown;
    Safety.RPCShield();
    Fun.PlaySplashEffect(((Component) Variables.taggerInstance.offlineVRRig).transform.position, ((Component) Variables.taggerInstance.offlineVRRig).transform.rotation, 125f);
  }

  public static void SplashGun()
  {
    if (GunLib.SetupGun())
    {
      if ((double) Time.time < (double) Fun.rpcCooldown)
        return;
      Fun.rpcCooldown = Time.time + Fun.splashCooldown;
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = (GunLib.raycastHit).point - Vector3.up * 2f;
      Fun.PlaySplashEffect((GunLib.raycastHit).point, (GunLib.raycastHit).normal != Vector3.zero ? Quaternion.LookRotation((GunLib.raycastHit).normal) : Quaternion.identity, 125f);
      ((Behaviour) VRRig.LocalRig).enabled = true;
      Safety.RPCShield();
    }
    else
      ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  public static void WaterBarrage()
  {
    if (!InputHandler.RTrigger() || (double) Time.time < (double) Fun.rpcCooldown)
      return;
    Fun.rpcCooldown = Time.time + Fun.splashCooldown;
    Safety.RPCShield();
    Vector3 vector3 = UnityEngine.Random.insideUnitSphere.normalized * UnityEngine.Random.Range(1.5f, 1.6f);
    Vector3 pos = ((Component) Variables.taggerInstance.offlineVRRig).transform.position + vector3;
    Fun.PlaySplashEffect(pos, Quaternion.LookRotation(((Component) Variables.taggerInstance.offlineVRRig).transform.position - pos), 125f);
  }

  public static void SplashAura()
  {
    if (!InputHandler.RTrigger() || (double) Time.time < (double) Fun.rpcCooldown)
      return;
    Fun.rpcCooldown = Time.time + Fun.splashCooldown;
    Safety.RPCShield();
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig) && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.myVRRig))
        Fun.PlaySplashEffect(((Component) activeRig).transform.position, ((Component) activeRig).transform.rotation, 125f);
    }
  }

  public static void ModifyWater(bool solid, bool transparent)
  {
    if (Fun.cachedWater == null)
      Fun.cachedWater = UnityEngine.Object.FindObjectsOfType<WaterVolume>();
    int layer = LayerMask.NameToLayer(solid ? "Default" : (transparent ? "TransparentFX" : "Water"));
    if (layer == -1)
      return;
    foreach (WaterVolume waterVolume in Fun.cachedWater)
    {
      if ((UnityEngine.Object) waterVolume != (UnityEngine.Object) null)
        ((Component) waterVolume).gameObject.layer = layer;
    }
  }

  public static void AirSwim(bool active)
  {
    if (active)
    {
      if ((UnityEngine.Object) Fun.airSwimPart == (UnityEngine.Object) null)
      {
        GameObject original = Variables.FindObject("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/ForestToBeach_Geo/CaveWaterVolume");
        if ((UnityEngine.Object) original == (UnityEngine.Object) null)
          return;
        Fun.airSwimPart = UnityEngine.Object.Instantiate<GameObject>(original);
        Fun.airSwimPart.transform.localScale = new Vector3(5f, 5f, 5f);
        Fun.airSwimPart.GetComponent<Renderer>().enabled = false;
      }
      Variables.playerInstance.audioManager.UnsetMixerSnapshot(0.1f);
      Fun.airSwimPart.transform.position = ((Component) GorillaTagger.Instance.headCollider).transform.position + new Vector3(0.0f, 2.5f, 0.0f);
    }
    else if ((UnityEngine.Object) Fun.airSwimPart != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.airSwimPart);
      Fun.airSwimPart = (GameObject) null;
    }
  }

  public static void FixHandTaps()
  {
    MenuPatches.handtapEnabled = false;
    MenuPatches.tapsEnabled = true;
    MenuPatches.doOverride = false;
    MenuPatches.overrideVolume = 0.1f;
    MenuPatches.tapMultiplier = 1;
    Variables.taggerInstance.handTapVolume = 0.1f;
  }

  public static void LoudHandTaps()
  {
    MenuPatches.handtapEnabled = true;
    MenuPatches.tapsEnabled = true;
    MenuPatches.doOverride = true;
    MenuPatches.overrideVolume = 99999f;
    MenuPatches.tapMultiplier = 10;
    Variables.taggerInstance.handTapVolume = 99999f;
  }

  public static void SilentHandTaps()
  {
    MenuPatches.handtapEnabled = true;
    MenuPatches.tapsEnabled = false;
    MenuPatches.doOverride = false;
    MenuPatches.overrideVolume = 0.0f;
    MenuPatches.tapMultiplier = 0;
    Variables.taggerInstance.handTapVolume = 0.0f;
  }

  public static void TryCosmeticAnywhere(bool enable)
  {
    if (enable == Fun.cosmeticsEnabled || (UnityEngine.Object) CosmeticsController.instance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance.myVRRig == (UnityEngine.Object) null)
      return;
    Fun.cosmeticsEnabled = enable;
    int[] numArray = enable ? Fun.cachedLmajuCosmetics ?? (Fun.cachedLmajuCosmetics = Fun.PackedCosmetics(Enumerable.Repeat<string>("LMAJU.", 16 /*0x10*/).ToArray<string>())) : Fun.archiveCosmetics;
    if (enable)
      Fun.archiveCosmetics = CosmeticsController.instance.currentWornSet.ToPackedIDArray();
    CosmeticsController.CosmeticSet cosmeticSet = new CosmeticsController.CosmeticSet(numArray, CosmeticsController.instance);
    CosmeticsController.instance.currentWornSet = cosmeticSet;
    Variables.taggerInstance.offlineVRRig.cosmeticSet = cosmeticSet;
    Variables.taggerInstance.myVRRig.SendRPC("RPC_UpdateCosmeticsWithTryonPacked", (RpcTarget) 0, new object[3]
    {
      (object) numArray,
      (object) CosmeticsController.instance.tryOnSet.ToPackedIDArray(),
      (object) false
    });
    Safety.RPCShield();
  }

  public static int[] PackedCosmetics(string[] cosmetics)
  {
    return new CosmeticsController.CosmeticSet(cosmetics, CosmeticsController.instance).ToPackedIDArray();
  }

  public static void ServerSideEquip()
  {
    if (Fun.serverSideBusy || (UnityEngine.Object) CoroutineHelper.Instance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance.bodyCollider == (UnityEngine.Object) null)
      return;
    Fun.serverSideBusy = true;
    CoroutineHelper.Instance.StartCoroutine(Fun.ServerSideEquipRoutine());
  }

  private static IEnumerator ServerSideEquipRoutine()
  {
    try
    {
      if (!((UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null) && !((UnityEngine.Object) Variables.taggerInstance.bodyCollider == (UnityEngine.Object) null))
      {
        Vector3 origin = ((Component) Variables.taggerInstance.bodyCollider).transform.position;
        GTZone[] savedZones = (GTZone[]) null;
        if ((UnityEngine.Object) ZoneManagement.instance != (UnityEngine.Object) null && !ZoneManagement.IsInZone((GTZone) 1))
        {
          savedZones = ZoneManagement.instance.activeZones.ToArray();
          List<GTZone> withCity = new List<GTZone>((IEnumerable<GTZone>) ZoneManagement.instance.activeZones)
          {
            (GTZone) 1
          };
          ZoneManagement.SetActiveZones(withCity.ToArray());
          float timeout = Time.time + 5f;
          while (!ZoneManagement.IsZoneLoaded((GTZone) 1) && (double) Time.time < (double) timeout)
            yield return (object) null;
          yield return (object) new WaitForSeconds(0.5f);
          withCity = (List<GTZone>) null;
        }
        Movement.TeleportTo(Fun.tryOnRoomPos);
        yield return (object) new WaitForSeconds(1f);
        Fun.cosmeticsEnabled = false;
        Fun.TryCosmeticAnywhere(true);
        yield return (object) new WaitForSeconds(0.5f);
        if ((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null && (UnityEngine.Object) Variables.taggerInstance.bodyCollider != (UnityEngine.Object) null)
          Movement.TeleportTo(origin);
        if (savedZones != null)
        {
          yield return (object) new WaitForSeconds(0.5f);
          ZoneManagement.SetActiveZones(savedZones);
        }
        savedZones = (GTZone[]) null;
      }
    }
    finally
    {
      Fun.serverSideBusy = false;
    }
  }

  public static void ServerSidedCosmetics(bool enable)
  {
    Fun.serverSidedArmed = enable;
    if (!enable)
      return;
    CosmeticsController instance = CosmeticsController.instance;
    if ((UnityEngine.Object) instance == (UnityEngine.Object) null || instance.allCosmetics == null || instance.unlockedCosmetics == null)
      return;
    HashSet<string> stringSet = new HashSet<string>();
    foreach (CosmeticsController.CosmeticItem unlockedCosmetic in instance.unlockedCosmetics)
      stringSet.Add(unlockedCosmetic.itemName);
    foreach (CosmeticsController.CosmeticItem allCosmetic in instance.allCosmetics)
    {
      if (!allCosmetic.isNullItem && allCosmetic.canTryOn && !string.IsNullOrEmpty(allCosmetic.itemName) && stringSet.Add(allCosmetic.itemName))
      {
        instance.unlockedCosmetics.Add(allCosmetic);
        Fun.serverSidedItems.Add(allCosmetic.itemName);
        switch (allCosmetic.itemCategory - 1)
        {
          case 0:
            instance.unlockedHats.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 1:
            instance.unlockedBadges.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 2:
            instance.unlockedFaces.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 3:
            if (allCosmetic.isThrowable)
            {
              instance.unlockedThrowables.Add(allCosmetic);
              break;
            }
            instance.unlockedPaws.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 4:
            instance.unlockedChests.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 5:
            instance.unlockedFurs.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 6:
            instance.unlockedShirts.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 7:
            instance.unlockedBacks.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 8:
            instance.unlockedArms.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 9:
            instance.unlockedPants.Add(allCosmetic);
            break;
          case (GorillaNetworking.CosmeticsController.CosmeticCategory) 10:
            instance.unlockedTagFX.Add(allCosmetic);
            break;
        }
      }
    }
    instance.UpdateWardrobeModelsAndButtons();
  }

  private static void SetBraceletState(bool enable, bool left)
  {
    Variables.taggerInstance.myVRRig.SendRPC("EnableNonCosmeticHandItemRPC", (RpcTarget) 0, new object[2]
    {
      (object) enable,
      (object) left
    });
  }

  public static void GetBracelet(bool state)
  {
    if (InputHandler.LGrip())
    {
      Fun.SetBraceletState(false, false);
      Fun.SetBraceletState(state, true);
    }
    if (InputHandler.RGrip())
    {
      Fun.SetBraceletState(state, false);
      Fun.SetBraceletState(false, true);
    }
    if (!InputHandler.LGrip() && !InputHandler.RGrip())
      return;
    Safety.RPCShield();
  }

  public static void BraceletSpam()
  {
    if ((double) Time.time <= (double) Fun.braceletSpamDelay)
      return;
    Fun.GetBracelet(Time.frameCount % 2 == 0);
    Fun.braceletSpamDelay = Time.time + 0.1f;
  }

  public static void RemoveBracelet()
  {
    Fun.SetBraceletState(false, true);
    Fun.SetBraceletState(false, false);
    Safety.RPCShield();
  }

  public static void PunchMod()
  {
    int index = 0;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
      {
        Rigidbody component = ((Component) Variables.playerInstance).GetComponent<Rigidbody>();
        Vector3 position1 = ((Component) Variables.taggerInstance.offlineVRRig).transform.position;
        Vector3 position2 = activeRig.rightHandTransform.position;
        if ((double) Vector3.Distance(position2, position1) < 0.25)
          component.AddForce(Vector3.Normalize(position2 - Fun.lastRight[index]) * 9f + Vector3.up * 2f, (ForceMode) 2);
        Fun.lastRight[index] = position2;
        Vector3 position3 = activeRig.leftHandTransform.position;
        if ((double) Vector3.Distance(position3, position1) < 0.25)
          component.AddForce(Vector3.Normalize(position3 - Fun.lastLeft[index]) * 9f + Vector3.up * 2f, (ForceMode) 2);
        Fun.lastLeft[index] = position3;
        ++index;
      }
    }
  }

  public static void DrawMod(bool active)
  {
    if (active)
    {
      if ((UnityEngine.Object) Fun.rightPointer == (UnityEngine.Object) null)
      {
        Fun.rightPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Fun.rightPointer.transform.localScale = Vector3.one * 0.1f;
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.rightPointer.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.rightPointer.GetComponent<SphereCollider>());
      }
      if ((UnityEngine.Object) Fun.leftPointer == (UnityEngine.Object) null)
      {
        Fun.leftPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Fun.leftPointer.transform.localScale = Vector3.one * 0.1f;
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.leftPointer.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.leftPointer.GetComponent<SphereCollider>());
      }
      Fun.rightPointer.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
      Fun.leftPointer.transform.position = Variables.playerInstance.LeftHand.controllerTransform.position;
      Fun.rightPointer.GetComponent<Renderer>().material.color = Fun.drawColor;
      Fun.leftPointer.GetComponent<Renderer>().material.color = Fun.drawColor;
      if (InputHandler.RGrip())
      {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitive.transform.localScale = Vector3.one * 0.1f;
        primitive.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
        primitive.GetComponent<Renderer>().material.color = Fun.drawColor;
        UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<SphereCollider>());
        Fun.drawings.Add(primitive);
      }
      if (InputHandler.LGrip())
      {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        primitive.transform.localScale = Vector3.one * 0.1f;
        primitive.transform.position = Variables.playerInstance.LeftHand.controllerTransform.position;
        primitive.GetComponent<Renderer>().material.color = Fun.drawColor;
        UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<SphereCollider>());
        Fun.drawings.Add(primitive);
      }
      if (InputHandler.RPrimary() && !Fun.colorChangerCooldown)
      {
        Fun.currentColor = (Fun.currentColor + 1) % 13;
        int currentColor = Fun.currentColor;
        if (true)
          ;
        Color color;
        switch (currentColor)
        {
          case 0:
            color = Color.white;
            break;
          case 1:
            color = Color.blue;
            break;
          case 2:
            color = Color.red;
            break;
          case 3:
            color = Color.green;
            break;
          case 4:
            color = (Color) new Color32((byte) 76, (byte) 0, (byte) 164, byte.MaxValue);
            break;
          case 5:
            color = Color.black;
            break;
          case 6:
            color = Color.cyan;
            break;
          case 7:
            color = (Color) new Color32((byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 80 /*0x50*/, byte.MaxValue);
            break;
          case 8:
            color = Color.yellow;
            break;
          case 9:
            color = (Color) new Color32(byte.MaxValue, (byte) 0, byte.MaxValue, byte.MaxValue);
            break;
          case 10:
            color = (Color) new Color32((byte) 0, byte.MaxValue, byte.MaxValue, byte.MaxValue);
            break;
          case 11:
            color = (Color) new Color32((byte) 150, (byte) 75, (byte) 0, byte.MaxValue);
            break;
          case 12:
            color = (Color) new Color32(byte.MaxValue, (byte) 165, (byte) 0, byte.MaxValue);
            break;
          default:
            color = Color.white;
            break;
        }
        if (true)
          ;
        Fun.drawColor = color;
        Fun.colorChangerCooldown = true;
      }
      else
      {
        if (InputHandler.RPrimary())
          return;
        Fun.colorChangerCooldown = false;
      }
    }
    else
    {
      if ((UnityEngine.Object) Fun.rightPointer != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.rightPointer);
        Fun.rightPointer = (GameObject) null;
      }
      if ((UnityEngine.Object) Fun.leftPointer != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.leftPointer);
        Fun.leftPointer = (GameObject) null;
      }
      Fun.DestroyAllDrawings();
    }
  }

  public static void PlaceBomb(bool active)
  {
    if (active)
    {
      if (InputHandler.RGrip())
      {
        if ((UnityEngine.Object) Fun.bomb == (UnityEngine.Object) null)
        {
          Fun.bomb = GameObject.CreatePrimitive(PrimitiveType.Cube);
          Fun.bomb.transform.localScale = Vector3.one * 0.2f;
          Fun.bomb.GetComponent<Renderer>().material.color = Color.red;
          UnityEngine.Object.Destroy((UnityEngine.Object) Fun.bomb.GetComponent<BoxCollider>());
        }
        Fun.bomb.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
      }
      else
      {
        if (!InputHandler.RTrigger() || !((UnityEngine.Object) Fun.bomb != (UnityEngine.Object) null))
          return;
        ((Collider) Variables.playerInstance.bodyCollider).attachedRigidbody.AddExplosionForce(50000f, Fun.bomb.transform.position, 5f);
        UnityEngine.Object.Destroy((UnityEngine.Object) Fun.bomb);
        Fun.bomb = (GameObject) null;
      }
    }
    else
    {
      if (!((UnityEngine.Object) Fun.bomb != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.bomb);
      Fun.bomb = (GameObject) null;
    }
  }

  public static void SetName(string name)
  {
    GorillaComputer.instance.currentName = name;
    PhotonNetwork.LocalPlayer.NickName = name;
    GorillaComputer.instance.SetLocalNameTagText(name);
    GorillaComputer.instance.savedName = name;
    PlayerPrefs.SetString("playerName", name);
    PlayerPrefs.Save();
  }

  private static void CleanupWebshooters()
  {
    if ((UnityEngine.Object) Fun.lr != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.lr.material);
    if ((UnityEngine.Object) Fun.leftlr != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.leftlr.material);
    Main.DestroyAndNullify<GameObject>(ref Fun.rightaimer);
    Main.DestroyAndNullify<GameObject>(ref Fun.leftaimer);
    Main.DestroyAndNullify<LineRenderer>(ref Fun.lr);
    Main.DestroyAndNullify<LineRenderer>(ref Fun.leftlr);
    Main.DestroyAndNullify<SpringJoint>(ref Fun.joint);
    Main.DestroyAndNullify<SpringJoint>(ref Fun.leftjoint);
    Fun.wackstart = false;
    Fun.cangrapple = true;
    Fun.canleftgrapple = true;
  }

  public static void Webshooters(bool active)
  {
    if (!active)
    {
      Fun.CleanupWebshooters();
    }
    else
    {
      if (!Fun.wackstart)
      {
        if ((UnityEngine.Object) Fun.lr == (UnityEngine.Object) null)
        {
          GameObject gameObject = new GameObject("RightWebLine");
          Fun.lr = gameObject.AddComponent<LineRenderer>();
          Fun.lr.material = new Material(Shader.Find("Sprites/Default"));
          Fun.lr.startColor = Fun.lr.endColor = Color.white;
          Fun.lr.startWidth = Fun.lr.endWidth = 0.02f;
          Fun.lr.positionCount = 0;
          gameObject.transform.SetParent(((Component) Variables.playerInstance).transform);
        }
        if ((UnityEngine.Object) Fun.leftlr == (UnityEngine.Object) null)
        {
          GameObject gameObject = new GameObject("LeftWebLine");
          Fun.leftlr = gameObject.AddComponent<LineRenderer>();
          Fun.leftlr.material = new Material(Shader.Find("Sprites/Default"));
          Fun.leftlr.startColor = Fun.leftlr.endColor = Color.white;
          Fun.leftlr.startWidth = Fun.leftlr.endWidth = 0.02f;
          Fun.leftlr.positionCount = 0;
          gameObject.transform.SetParent(((Component) Variables.playerInstance).transform);
        }
        Fun.wackstart = true;
      }
      Fun.HandleWebshooter(ref Fun.joint, ref Fun.lr, ref Fun.rightaimer, ref Fun.cangrapple, Variables.playerInstance.RightHand.controllerTransform, ControllerInputPoller.instance.rightControllerIndexFloat, (OVRInput.Controller) 2, false);
      Fun.HandleWebshooter(ref Fun.leftjoint, ref Fun.leftlr, ref Fun.leftaimer, ref Fun.canleftgrapple, Variables.playerInstance.LeftHand.controllerTransform, ControllerInputPoller.instance.leftControllerIndexFloat, (OVRInput.Controller) 1, true);
    }
  }

  private static void HandleWebshooter(
    ref SpringJoint jnt,
    ref LineRenderer line,
    ref GameObject aim,
    ref bool canGrap,
    Transform hand,
    float trigger,
    OVRInput.Controller ctrl,
    bool isRight)
  {
    RaycastHit raycastHit;
    bool flag = Physics.Raycast(hand.position, hand.forward, out raycastHit, 100f);
    if (flag)
    {
      if ((UnityEngine.Object) aim == (UnityEngine.Object) null)
      {
        aim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UnityEngine.Object.Destroy((UnityEngine.Object) aim.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) aim.GetComponent<SphereCollider>());
        aim.transform.localScale = Vector3.one * 0.2f;
        aim.GetComponent<Renderer>().material.color = Color.green;
      }
      aim.SetActive(true);
      aim.transform.position = (raycastHit).point;
    }
    if ((double) trigger > 0.10000000149011612)
    {
      if (canGrap & flag)
      {
        jnt = ((Component) Variables.playerInstance).gameObject.AddComponent<SpringJoint>();
        ((Joint) jnt).autoConfigureConnectedAnchor = false;
        ((Joint) jnt).connectedAnchor = (raycastHit).point;
        float num = Vector3.Distance(((Component) Variables.playerInstance).transform.position, (raycastHit).point);
        jnt.maxDistance = num * 0.8f;
        jnt.minDistance = num * 0.25f;
        jnt.spring = 5000f;
        jnt.damper = 4000f;
        ((Joint) jnt).massScale = 6f;
        line.positionCount = 2;
        line.SetPosition(1, (raycastHit).point);
        Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(82, isRight, 1f);
        canGrap = false;
      }
      if (!((UnityEngine.Object) jnt != (UnityEngine.Object) null))
        return;
      line.positionCount = 2;
      line.SetPosition(0, hand.position);
      line.SetPosition(1, ((Joint) jnt).connectedAnchor);
      if ((UnityEngine.Object) aim != (UnityEngine.Object) null)
        aim.transform.position = ((Joint) jnt).connectedAnchor;
      if ((double) OVRInput.GetLocalControllerVelocity(ctrl).magnitude >= 2.5)
        ((Component) Variables.playerInstance).GetComponent<Rigidbody>().AddForce((((Joint) jnt).connectedAnchor - ((Component) Variables.playerInstance).transform.position).normalized * 3f, (ForceMode) 2);
    }
    else
    {
      if ((UnityEngine.Object) jnt != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) jnt);
        jnt = (SpringJoint) null;
      }
      line.positionCount = 0;
      canGrap = true;
    }
  }

  private static void CleanupGrapple()
  {
    if ((UnityEngine.Object) Fun.grappleLR != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.grappleLR.material);
    if ((UnityEngine.Object) Fun.leftGrappleLR != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) Fun.leftGrappleLR.material);
    Main.DestroyAndNullify<GameObject>(ref Fun.rightGrappleAimer);
    Main.DestroyAndNullify<GameObject>(ref Fun.leftGrappleAimer);
    Main.DestroyAndNullify<LineRenderer>(ref Fun.grappleLR);
    Main.DestroyAndNullify<LineRenderer>(ref Fun.leftGrappleLR);
    int num;
    Fun.grappleStart = (num = 0) != 0;
    Fun.leftGrappleAttached = num != 0;
    Fun.rightGrappleAttached = num != 0;
  }

  public static void GrapplingHook(bool active)
  {
    if (!active)
    {
      Fun.CleanupGrapple();
    }
    else
    {
      if (!Fun.grappleStart)
      {
        if ((UnityEngine.Object) Fun.grappleLR == (UnityEngine.Object) null)
        {
          GameObject gameObject = new GameObject("RGrappleLine");
          Fun.grappleLR = gameObject.AddComponent<LineRenderer>();
          Fun.grappleLR.material = new Material(Shader.Find("Sprites/Default"));
          Fun.grappleLR.startColor = Fun.grappleLR.endColor = Color.white;
          Fun.grappleLR.startWidth = Fun.grappleLR.endWidth = 0.02f;
          Fun.grappleLR.positionCount = 0;
          gameObject.transform.SetParent(((Component) Variables.playerInstance).transform);
        }
        if ((UnityEngine.Object) Fun.leftGrappleLR == (UnityEngine.Object) null)
        {
          GameObject gameObject = new GameObject("LGrappleLine");
          Fun.leftGrappleLR = gameObject.AddComponent<LineRenderer>();
          Fun.leftGrappleLR.material = new Material(Shader.Find("Sprites/Default"));
          Fun.leftGrappleLR.startColor = Fun.leftGrappleLR.endColor = Color.white;
          Fun.leftGrappleLR.startWidth = Fun.leftGrappleLR.endWidth = 0.02f;
          Fun.leftGrappleLR.positionCount = 0;
          gameObject.transform.SetParent(((Component) Variables.playerInstance).transform);
        }
        Fun.grappleStart = true;
      }
      Fun.HandleGrapple(ref Fun.rightGrappleAimer, ref Fun.grappleLR, ref Fun.rightGrapplePoint, ref Fun.rightGrappleAttached, Variables.playerInstance.RightHand.controllerTransform, ControllerInputPoller.instance.rightControllerIndexFloat, false);
      Fun.HandleGrapple(ref Fun.leftGrappleAimer, ref Fun.leftGrappleLR, ref Fun.leftGrapplePoint, ref Fun.leftGrappleAttached, Variables.playerInstance.LeftHand.controllerTransform, ControllerInputPoller.instance.leftControllerIndexFloat, true);
    }
  }

  private static void HandleGrapple(
    ref GameObject aim,
    ref LineRenderer line,
    ref Vector3 grapPt,
    ref bool attached,
    Transform hand,
    float trigger,
    bool isRight)
  {
    RaycastHit raycastHit;
    bool flag = Physics.Raycast(hand.position, hand.forward, out raycastHit, 100f);
    if (flag)
    {
      if ((UnityEngine.Object) aim == (UnityEngine.Object) null)
      {
        aim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        UnityEngine.Object.Destroy((UnityEngine.Object) aim.GetComponent<Rigidbody>());
        UnityEngine.Object.Destroy((UnityEngine.Object) aim.GetComponent<SphereCollider>());
        aim.transform.localScale = Vector3.one * 0.2f;
        aim.GetComponent<Renderer>().material.color = Color.green;
      }
      aim.SetActive(true);
      aim.transform.position = (raycastHit).point;
    }
    if ((double) trigger > 0.10000000149011612)
    {
      if (!attached & flag)
      {
        grapPt = (raycastHit).point;
        attached = true;
        line.positionCount = 2;
        line.SetPosition(1, grapPt);
        Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(82, isRight, 1f);
      }
      if (!attached)
        return;
      line.positionCount = 2;
      line.SetPosition(0, hand.position);
      line.SetPosition(1, grapPt);
      if ((UnityEngine.Object) aim != (UnityEngine.Object) null)
        aim.transform.position = grapPt;
      Vector3 normalized = (grapPt - ((Component) Variables.playerInstance.bodyCollider).transform.position).normalized;
      if ((double) Vector3.Distance(((Component) Variables.playerInstance.bodyCollider).transform.position, grapPt) > 1.0)
        ((Component) Variables.playerInstance).GetComponent<Rigidbody>().velocity = Vector3.Lerp(((Component) Variables.playerInstance).GetComponent<Rigidbody>().velocity, normalized * 15f, Time.deltaTime * 10f);
    }
    else
    {
      attached = false;
      line.positionCount = 0;
    }
  }
}
