
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using NXO.Menu;
using NXO.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

#nullable enable
namespace NXO.Mods.Categories;

public class Projectile
{
  public static int snowFlingIndex = 0;
  private static float snowballEffectDelay = 0.0f;
  public static Dictionary<Projectile.Projectiles, int> OverrideIndexPairss = new Dictionary<Projectile.Projectiles, int>()
  {
    {
      Projectile.Projectiles.SnowBall,
      32 /*0x20*/
    },
    {
      Projectile.Projectiles.BigSnowball,
      339
    },
    {
      Projectile.Projectiles.PishFood,
      333
    },
    {
      Projectile.Projectiles.WaterBalloons,
      204
    },
    {
      Projectile.Projectiles.EasterEggs,
      311
    },
    {
      Projectile.Projectiles.Books,
      312
    },
    {
      Projectile.Projectiles.Apple,
      288
    },
    {
      Projectile.Projectiles.GoldCoin,
      313
    },
    {
      Projectile.Projectiles.HotDog,
      314
    },
    {
      Projectile.Projectiles.IceCream,
      315
    },
    {
      Projectile.Projectiles.Fireworks,
      316
    },
    {
      Projectile.Projectiles.Paper,
      317
    },
    {
      Projectile.Projectiles.IceCreamScoop,
      318
    },
    {
      Projectile.Projectiles.Chips,
      322
    },
    {
      Projectile.Projectiles.RedBall,
      323
    },
    {
      Projectile.Projectiles.TacoBall,
      324
    },
    {
      Projectile.Projectiles.MashedPotatos,
      325
    },
    {
      Projectile.Projectiles.BreadBall,
      326
    },
    {
      Projectile.Projectiles.PurpleBall,
      327
    },
    {
      Projectile.Projectiles.OrangeBall,
      328
    },
    {
      Projectile.Projectiles.Stuffing,
      329
    },
    {
      Projectile.Projectiles.Turkey,
      330
    },
    {
      Projectile.Projectiles.BrokenLavaRocks,
      231
    },
    {
      Projectile.Projectiles.WorkingLavaRocks,
      287
    },
    {
      Projectile.Projectiles.Corn,
      331
    },
    {
      Projectile.Projectiles.FireStick,
      332
    },
    {
      Projectile.Projectiles.FoodBall,
      333
    },
    {
      Projectile.Projectiles.PineApple,
      334
    },
    {
      Projectile.Projectiles.PopCorn,
      335
    },
    {
      Projectile.Projectiles.FlamingNuts,
      340
    },
    {
      Projectile.Projectiles.Presents,
      240 /*0xF0*/
    },
    {
      Projectile.Projectiles.Confeti,
      249
    },
    {
      Projectile.Projectiles.GiantMeatball,
      252
    },
    {
      Projectile.Projectiles.CandyBuket,
      286
    },
    {
      Projectile.Projectiles.slingshot,
      -1
    }
  };
  public static GameObject HandObj;
  public static Projectile.Projectiles CurrentProjectile;
  private static bool ShouldRecreate = false;
  private static float lastLaunchTime = 0.0f;
  private static object[] Data1 = new object[9];
  private static object[] Data2 = new object[3];
  private static float nextLaunchTime;
  public static bool GrabbedGrowing = false;
  public static Dictionary<string, SnowballThrowable>? projlist;
  public static bool allSnowballsInitialized;
  public static RaiseEventOptions? eventOptions = (RaiseEventOptions) null;
  public static float lastSendTime;

  private static void FlingSnowballAt(VRRig rig)
  {
    if ((UnityEngine.Object) rig?.head?.rigTarget == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance.offlineVRRig?.rightHand?.rigTarget == (UnityEngine.Object) null || rig.Creator == null)
      return;
    ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
    ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) rig).transform.position + new Vector3(1f, 0.0f, 0.0f);
    for (int index = 0; index < 3; ++index)
      Projectile.SendGrowing(rig.head.rigTarget.position - new Vector3(0.0f, 0.3f, 0.0f), Projectile.PosToVector3(GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.position, ((Component) rig).transform.up, 103f), p: rig.Creator);
  }

  public static void SnowballFlingGun()
  {
    if (!GunLib.SetupLockOnGun())
      return;
    Projectile.FlingSnowballAt(GunLib.lockedTargetRig);
  }

  public static void SnowballFlingAll()
  {
    MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
    {
      PhotonView component = ((Component) Variables.taggerInstance.myVRRig).GetComponent<PhotonView>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return false;
      PhotonSerializer.BroadcastViews(true, new PhotonView[1]
      {
        component
      });
      List<NetPlayer> list = ((IEnumerable<NetPlayer>) NetworkSystem.Instance.PlayerListOthers).Where<NetPlayer>((Func<NetPlayer, bool>) (pl => (UnityEngine.Object) RigManager.GetVRRigFromNetPlayer(pl) != (UnityEngine.Object) null)).ToList<NetPlayer>();
      if (list.Count == 0)
        return false;
      Projectile.snowFlingIndex %= list.Count;
      NetPlayer netPlayer = list[Projectile.snowFlingIndex];
      Projectile.snowFlingIndex = (Projectile.snowFlingIndex + 1) % list.Count;
      VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(netPlayer);
      if ((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null || (UnityEngine.Object) rigFromNetPlayer.head?.rigTarget == (UnityEngine.Object) null || (UnityEngine.Object) VRRig.LocalRig?.rightHand?.rigTarget == (UnityEngine.Object) null)
        return false;
      Vector3 position1 = ((Component) VRRig.LocalRig).transform.position;
      Quaternion rotation1 = ((Component) VRRig.LocalRig).transform.rotation;
      Vector3 position2 = VRRig.LocalRig.leftHand.rigTarget.position;
      Quaternion rotation2 = VRRig.LocalRig.leftHand.rigTarget.rotation;
      Vector3 position3 = VRRig.LocalRig.rightHand.rigTarget.position;
      Quaternion rotation3 = VRRig.LocalRig.rightHand.rigTarget.rotation;
      ((Component) VRRig.LocalRig).transform.position = ((Component) rigFromNetPlayer).transform.position + new Vector3(1f, 0.0f, 0.0f);
      Projectile.SendGrowing(rigFromNetPlayer.head.rigTarget.position - new Vector3(0.0f, 0.3f, 0.0f), Projectile.PosToVector3(VRRig.LocalRig.rightHand.rigTarget.position, ((Component) rigFromNetPlayer).transform.up, 103f), p: netPlayer);
      PhotonSerializer.SendSerialize(component, new RaiseEventOptions()
      {
        TargetActors = new int[1]{ netPlayer.ActorNumber }
      });
      Safety.RPCShield();
      ((Component) VRRig.LocalRig).transform.position = position1;
      ((Component) VRRig.LocalRig).transform.rotation = rotation1;
      VRRig.LocalRig.leftHand.rigTarget.position = position2;
      VRRig.LocalRig.leftHand.rigTarget.rotation = rotation2;
      VRRig.LocalRig.rightHand.rigTarget.position = position3;
      VRRig.LocalRig.rightHand.rigTarget.rotation = rotation3;
      return false;
    });
  }

  public static void TouchToSnowballFling()
  {
    Transform[] hands = new Transform[2]
    {
      Variables.taggerInstance.leftHandTransform,
      Variables.taggerInstance.rightHandTransform
    };
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !activeRig.isLocal)
      {
        Transform[] targets = new Transform[2]
        {
          activeRig.headMesh.transform,
          activeRig.bodyTransform
        };
        if (RigManager.HandsTouchTargets(hands, targets))
        {
          Projectile.FlingSnowballAt(activeRig);
          break;
        }
      }
    }
  }

  public static void GetTouchedToSnowballFling()
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
          Projectile.FlingSnowballAt(activeRig);
          break;
        }
      }
    }
  }

  private static void SendSnowballEffect(int actorNumber)
  {
    NetworkSystemRaiseEvent.RaiseEvent((byte) 3, (object) new object[3]
    {
      (object) NetworkSystem.Instance.ServerTimestamp,
      (object) 6,
      (object) new object[2]
      {
        (object) actorNumber,
        (object) (PlayerEffect) 0
      }
    }, new NetEventOptions()
    {
      Reciever = (NetEventOptions.RecieverTarget) 1
    }, false);
  }

  public static void SnowballEffectPlayerGun()
  {
    if (!GunLib.SetupLockOnGun() || (double) Time.time <= (double) Projectile.snowballEffectDelay || !PhotonNetwork.InRoom || !((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null) || !((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) RigManager.GetVRRigFromNetPlayer(((NetPlayer) (PhotonNetwork.LocalPlayer)))))
      return;
    Projectile.SendSnowballEffect(GunLib.lockedTargetRig.OwningNetPlayer.ActorNumber);
    Projectile.snowballEffectDelay = Time.time + 0.1f;
  }

  public static void SnowballEffectAll()
  {
    if (InputHandler.RTrigger())
      MenuPatches.SerializationPatch.Override = (Func<bool>) (() =>
      {
        if (!PhotonNetwork.InRoom)
          return true;
        if ((double) Time.time < (double) Projectile.snowballEffectDelay)
          return false;
        Projectile.snowballEffectDelay = Time.time + 0.1f;
        PhotonSerializer.BroadcastViews(true, new PhotonView[1]
        {
          Variables.taggerInstance.myVRRig.GetView
        });
        Vector3 position = ((Component) VRRig.LocalRig).transform.position;
        ((Behaviour) VRRig.LocalRig).enabled = false;
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
            Projectile.SendSnowballEffect(playerListOther.ActorNumber);
          }
        }
        ((Component) VRRig.LocalRig).transform.position = position;
        ((Behaviour) VRRig.LocalRig).enabled = true;
        Safety.RPCShield();
        return false;
      });
    else
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
  }

  public static void TouchToSnowballEffect()
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
          if ((double) Time.time <= (double) Projectile.snowballEffectDelay)
            break;
          Projectile.SendSnowballEffect(activeRig.OwningNetPlayer.ActorNumber);
          Projectile.snowballEffectDelay = Time.time + 0.1f;
          break;
        }
      }
    }
  }

  public static void GetTouchedToSnowballEffect()
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
          if ((double) Time.time <= (double) Projectile.snowballEffectDelay)
            break;
          Projectile.SendSnowballEffect(activeRig.OwningNetPlayer.ActorNumber);
          Projectile.snowballEffectDelay = Time.time + 0.1f;
          break;
        }
      }
    }
  }

  public static void ShootBigSnowballs()
  {
    if ((double) ControllerInputPoller.instance.rightControllerGripFloat <= 0.5 && (Mouse.current == null || !Mouse.current.rightButton.isPressed))
      return;
    Transform rightHandTransform = GorillaTagger.Instance.rightHandTransform;
    Projectile.SendGrowing(rightHandTransform.position, -rightHandTransform.up * 100f);
  }

  public static void ShootBigSnowballsGun()
  {
    if (!GunLib.SetupGun())
      return;
    Vector3 position = GorillaTagger.Instance.rightHandTransform.position;
    Projectile.SendGrowing(position, ((GunLib.raycastHit).point - position).normalized * 100f);
  }

  public static string CurrentProjectileName
  {
    get => Enum.GetName(typeof (Projectile.Projectiles), (object) Projectile.CurrentProjectile);
  }

  public static void CycleProjectile(bool forward)
  {
    Projectile.Projectiles[] values = (Projectile.Projectiles[]) Enum.GetValues(typeof (Projectile.Projectiles));
    int num = Array.IndexOf<Projectile.Projectiles>(values, Projectile.CurrentProjectile);
    int index = forward ? (num + 1) % values.Length : (num - 1 + values.Length) % values.Length;
    Projectile.CurrentProjectile = values[index];
    ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (x => x.buttonText.Contains("Current Projectile"))).FirstOrDefault<ButtonHandler.Button>()?.SetText("Current Projectile : " + Projectile.CurrentProjectileName);
  }

  public static void ApplyProjectile(string name)
  {
    Projectile.Projectiles result;
    if (!Enum.TryParse<Projectile.Projectiles>(name, out result))
      return;
    Projectile.CurrentProjectile = result;
    ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (x => x.buttonText.Contains("Current Projectile"))).FirstOrDefault<ButtonHandler.Button>()?.SetText("Current Projectile : " + Projectile.CurrentProjectileName);
  }

  private static void InitObj(Projectile.Projectiles Projectile)
  {
    if ((UnityEngine.Object) NXO.Mods.Categories.Projectile.HandObj != (UnityEngine.Object) null)
      return;
    NXO.Mods.Categories.Projectile.HandObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
    NXO.Mods.Categories.Projectile.HandObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    NXO.Mods.Categories.Projectile.HandObj.transform.position = GorillaTagger.Instance.rightHandTransform.position;
    NXO.Mods.Categories.Projectile.HandObj.GetComponent<Collider>().enabled = false;
    NXO.Mods.Categories.Projectile.HandObj.AddComponent<GorillaSurfaceOverride>();
    NXO.Mods.Categories.Projectile.HandObj.GetComponent<GorillaSurfaceOverride>().overrideIndex = NXO.Mods.Categories.Projectile.OverrideIndexPairss[Projectile];
    NXO.Mods.Categories.Projectile.HandObj.AddComponent<SnowballGrabZone>();
    NXO.Mods.Categories.Projectile.HandObj.GetComponent<SnowballGrabZone>().materialIndex = NXO.Mods.Categories.Projectile.OverrideIndexPairss[Projectile];
  }

  private static bool RefreshHandObj(Projectile.Projectiles Projectile, bool shouldRecreate)
  {
    if (shouldRecreate)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) NXO.Mods.Categories.Projectile.HandObj);
      NXO.Mods.Categories.Projectile.HandObj = (GameObject) null;
      NXO.Mods.Categories.Projectile.InitObj(Projectile);
      return true;
    }
    NXO.Mods.Categories.Projectile.HandObj.transform.position = GorillaTagger.Instance.rightHandTransform.position;
    return true;
  }

  public static Vector3 PosToVector3(Vector3 currentPos, Vector3 targetPos, float speed)
  {
    return (targetPos - currentPos).normalized * speed;
  }

  public static void SpamProjectiles(Projectile.Projectiles Proj, Vector3? velocity, float delay = 0.1f)
  {
    Projectile.CurrentProjectile = Proj;
    if ((double) ControllerInputPoller.instance.rightControllerGripFloat <= 0.5 && !Mouse.current.rightButton.isPressed)
      return;
    Projectile.LaunchProj_(Projectile.CurrentProjectile, new Vector3?());
  }

  public static void LaunchProj_(Projectile.Projectiles Proj, Vector3? velocity, float delay = 0.1f)
  {
    if (Projectile.CurrentProjectile != Proj)
      Projectile.ShouldRecreate = true;
    Projectile.CurrentProjectile = Proj;
    if ((UnityEngine.Object) Projectile.HandObj == (UnityEngine.Object) null)
      Projectile.InitObj(Projectile.CurrentProjectile);
    if (!Projectile.RefreshHandObj(Projectile.CurrentProjectile, Projectile.ShouldRecreate))
      return;
    if (Projectile.ShouldRecreate)
      Projectile.ShouldRecreate = false;
    if ((double) Time.time - (double) Projectile.lastLaunchTime < (double) delay)
      return;
    Projectile.lastLaunchTime = Time.time;
    switch (Proj)
    {
      case Projectile.Projectiles.BigSnowball:
        Projectile.SendGrowing(GorillaTagger.Instance.rightHandTransform.position, Projectile.PosToVector3(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.position + ((double) ControllerInputPoller.instance.rightControllerGripFloat > 0.5 ? -GorillaTagger.Instance.rightHandTransform.up : ((Component) GorillaTagger.Instance.bodyCollider).transform.forward) * 10f, 15f));
        break;
      case Projectile.Projectiles.slingshot:
        Projectile.LaunchSlingshot();
        break;
      default:
        Projectile.LaunchStandardSnowball(Proj, velocity);
        break;
    }
  }

  private static void LaunchStandardSnowball(Projectile.Projectiles Proj, Vector3? vel = null)
  {
    if ((double) Time.time < (double) Projectile.nextLaunchTime)
      return;
    Projectile.nextLaunchTime = Time.time + 0.2f;
    int num;
    if (!Projectile.OverrideIndexPairss.TryGetValue(Proj, out num))
      return;
    if (!Projectile.allSnowballsInitialized && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.Count >= 1 && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.Count >= 1)
    {
      Projectile.allSnowballsInitialized = true;
      LinqUtils.ForEach<KeyValuePair<int, string>>((IEnumerable<KeyValuePair<int, string>>) CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft, (Action<KeyValuePair<int, string>>) (v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value)));
      LinqUtils.ForEach<KeyValuePair<int, string>>((IEnumerable<KeyValuePair<int, string>>) CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight, (Action<KeyValuePair<int, string>>) (v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value)));
    }
    SnowballMaker rightHandInstance = SnowballMaker.rightHandInstance;
    GTPlayer.HandState rightHand = GTPlayer.Instance.rightHand;
    rightHand.materialTouchIndex = num;
    GTPlayer.Instance.rightHand = rightHand;
    rightHandInstance.InitializeSnowballFromMatIndex(num);
    rightHandInstance.SetupThrowables(rightHandInstance.snowballs);
    SnowballThrowable snowballThrowable;
    if (!rightHandInstance.TryCreateSnowball(num, out snowballThrowable) || (UnityEngine.Object) snowballThrowable == (UnityEngine.Object) null)
      return;
    EquipmentInteractor.instance.ForceDropAnyEquipment();
    snowballThrowable.isLeftHanded = false;
    snowballThrowable.targetRig = VRRig.LocalRig;
    snowballThrowable.SetSnowballActiveLocal(true);
    VRRig.LocalRig.RightThrowableProjectileIndex = snowballThrowable.throwableMakerIndex;
    if (snowballThrowable.randomizeColor)
      VRRig.LocalRig.SetThrowableProjectileColor(false, (Color32) GTColor.RandomHSV(snowballThrowable.randomColorHSVRanges));
    Vector3 position = GorillaTagger.Instance.offlineVRRig.rightHand.rigTarget.position;
    Vector3 vector3 = vel.HasValue ? vel.Value : GTPlayer.Instance.GetHandVelocityTracker(false).GetAverageVelocity(false, 0.15f, false);
    float x = GorillaTagger.Instance.rightHandTransform.lossyScale.x;
    Color throwableProjectileColor = (Color) VRRig.LocalRig.GetThrowableProjectileColor(false);
    Color32 color32 = (Color32) throwableProjectileColor;
    SlingshotProjectile slingshotProjectile = snowballThrowable.LaunchSnowballLocal(position, vector3, x, snowballThrowable.randomizeColor, throwableProjectileColor);
    if (PhotonNetwork.InRoom)
    {
      try
      {
        int projectileCount = slingshotProjectile.myProjectileCount;
        RoomSystem.SendLaunchProjectile(position, vector3, (RoomSystem.ProjectileSource) 2, projectileCount, snowballThrowable.randomizeColor, color32.r, color32.g, color32.b, color32.a);
        Projectile.Data1[0] = (object) position;
        Projectile.Data1[1] = (object) vector3;
        Projectile.Data1[2] = (object) (RoomSystem.ProjectileSource) 2;
        Projectile.Data1[3] = (object) projectileCount;
        Projectile.Data1[4] = (object) snowballThrowable.randomizeColor;
        Projectile.Data1[5] = (object) color32.r;
        Projectile.Data1[6] = (object) color32.g;
        Projectile.Data1[7] = (object) color32.b;
        Projectile.Data1[8] = (object) color32.a;
        Projectile.Data2[0] = (object) NetworkSystem.Instance.ServerTimestamp;
        Projectile.Data2[1] = (object) 0;
        Projectile.Data2[2] = (object) Projectile.Data1;
        object[] data2 = Projectile.Data2;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        raiseEventOptions.Receivers = (ReceiverGroup) 0;
        SendOptions sendOptions1 = new SendOptions();
        (sendOptions1).Reliability = false;
        SendOptions sendOptions2 = sendOptions1;
        PhotonNetwork.RaiseEvent((byte) 3, (object) data2, raiseEventOptions, sendOptions2);
        Debug.Log((object) $"Launched packed ({((UnityEngine.Object) slingshotProjectile).name})");
      }
      catch
      {
      }
    }
    VRRig.LocalRig.RightThrowableProjectileIndex = -1;
    EquipmentInteractor.instance.ForceDropEquipment((IHoldableObject) snowballThrowable);
    snowballThrowable.targetRig = VRRig.LocalRig;
    snowballThrowable.isLeftHanded = false;
    Array.Clear((Array) Projectile.Data1, 0, Projectile.Data1.Length);
    Array.Clear((Array) Projectile.Data2, 0, Projectile.Data2.Length);
  }

  public static void ResetProjectiles()
  {
    Projectile.projlist = (Dictionary<string, SnowballThrowable>) null;
    Projectile.allSnowballsInitialized = false;
    Projectile.GrabbedGrowing = false;
  }

  public static void ReleaseSnowball()
  {
    try
    {
      SnowballThrowable projectile = Projectile.GetProjectile("GrowingSnowballRightAnchor");
      if (!((UnityEngine.Object) projectile != (UnityEngine.Object) null) || !((UnityEngine.Object) ((Component) projectile).gameObject != (UnityEngine.Object) null))
        return;
      ((Component) projectile).gameObject.SetActive(false);
    }
    catch
    {
    }
  }

  public static IEnumerator DisableSnowball(bool rigDisabled)
  {
    yield return (object) new WaitForSeconds(0.3f);
    if (rigDisabled)
      ((Behaviour) VRRig.LocalRig).enabled = true;
    foreach (SnowballThrowable snowball in Projectile.projlist.Values)
    {
      try
      {
        snowball.SetSnowballActiveLocal(false);
      }
      catch
      {
      }
    }
  }

  public static RaiseEventOptions GetOption(bool all)
  {
    Projectile.eventOptions = new RaiseEventOptions()
    {
      Receivers = all ? (ReceiverGroup) 1 : (ReceiverGroup) 0
    };
    return Projectile.eventOptions;
  }

  public static RaiseEventOptions GetPlayerOption(int act)
  {
    Projectile.eventOptions = new RaiseEventOptions()
    {
      TargetActors = new int[1]{ act }
    };
    return Projectile.eventOptions;
  }

  public static bool isrignull(Photon.Realtime.Player p) => p == null;

  public static void SendGrowing(Vector3 Position, Vector3 Velocity, bool Fling = false, NetPlayer? p = null)
  {
    if (!PhotonNetwork.InRoom)
      return;
    GrowingSnowballThrowable snowballThrowable = (GrowingSnowballThrowable) null;
    try
    {
      snowballThrowable = ((Component) Projectile.GetProjectile("GrowingSnowballRightAnchor"))?.GetComponent<GrowingSnowballThrowable>();
    }
    catch
    {
    }
    if ((UnityEngine.Object) snowballThrowable == (UnityEngine.Object) null)
      return;
    if (!Projectile.GrabbedGrowing)
    {
      try
      {
        ((SnowballThrowable) snowballThrowable).SetSnowballActiveLocal(true);
      }
      catch
      {
      }
      snowballThrowable.IncreaseSize(5);
      ((Component) snowballThrowable).transform.position = Position;
      Projectile.GrabbedGrowing = true;
    }
    RaiseEventOptions raiseEventOptions1 = new RaiseEventOptions();
    if (p != null)
      raiseEventOptions1.TargetActors = new int[1]
      {
        p.ActorNumber
      };
    else
      raiseEventOptions1.Receivers = (ReceiverGroup) 1;
    if (Fling && p != null && (double) Vector3.Distance(((Component) VRRig.LocalRig).transform.position, ((Component) VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (x => x.Creator.UserId == p.UserId)).FirstOrDefault<VRRig>()).transform.position) > 4.0)
    {
      ((Behaviour) VRRig.LocalRig).enabled = false;
      ((Component) VRRig.LocalRig).transform.position = ((Component) VRRigCache.ActiveRigs.Where<VRRig>((Func<VRRig, bool>) (x => x.Creator.UserId == p.UserId)).FirstOrDefault<VRRig>()).transform.position + new Vector3(0.0f, 0.2f, 0.0f);
    }
    if (Projectile.GrabbedGrowing && (double) Time.time > (double) Projectile.lastSendTime && (snowballThrowable.changeSizeEvent != (PhotonEvent) null) && (snowballThrowable.snowballThrowEvent != (PhotonEvent) null))
    {
      int num1 = 0;
      int num2;
      try
      {
        num2 = ProjectileTracker.AddAndIncrementLocalProjectile((SlingshotProjectile) null, Velocity, Position, 1f);
      }
      catch
      {
        num2 = num1 + 1;
      }
      int num3;
      try
      {
        LoopingArray<ProjectileTracker.ProjectileInfo> localProjectiles = ProjectileTracker.m_localProjectiles;
        ProjectileTracker.ProjectileInfo projectileInfo = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Velocity, Position, 1f, (SlingshotProjectile) null);
        ref ProjectileTracker.ProjectileInfo local = ref projectileInfo;
        num3 = localProjectiles.AddAndIncrement(ref local);
      }
      catch
      {
        num3 = num2 + 1;
      }
      int num4;
      try
      {
        LoopingArray<ProjectileTracker.ProjectileInfo> localProjectiles = ProjectileTracker.m_localProjectiles;
        ProjectileTracker.ProjectileInfo projectileInfo = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Velocity, Position, 1f, (SlingshotProjectile) null);
        ref ProjectileTracker.ProjectileInfo local = ref projectileInfo;
        num4 = localProjectiles.IncrementAndAdd(ref local);
      }
      catch
      {
        num4 = num3 + 1;
      }
      int num5;
      try
      {
        num5 = ProjectileTracker.m_playerProjectiles.Count<KeyValuePair<NetPlayer, LoopingArray<ProjectileTracker.ProjectileInfo>>>();
      }
      catch
      {
        num5 = num4 + 1;
      }
      int num6;
      try
      {
        num6 = ProjectileTracker.m_projectileInfoPool.m_size;
      }
      catch
      {
        num6 = num5 + 1;
      }
      object[] objArray1 = new object[2]
      {
        (object) snowballThrowable.changeSizeEvent._eventId,
        (object) 5
      };
      RaiseEventOptions raiseEventOptions2 = raiseEventOptions1;
      SendOptions sendOptions1 = new SendOptions();
      (sendOptions1).Reliability = false;
      SendOptions sendOptions2 = sendOptions1;
      PhotonNetwork.RaiseEvent((byte) 176 /*0xB0*/, (object) objArray1, raiseEventOptions2, sendOptions2);
      object[] objArray2 = new object[5]
      {
        (object) snowballThrowable.snowballThrowEvent._eventId,
        (object) Position,
        (object) Velocity,
        (object) num6,
        null
      };
      RaiseEventOptions raiseEventOptions3 = raiseEventOptions1;
      SendOptions sendOptions3 = new SendOptions();
      (sendOptions3).Reliability = false;
      SendOptions sendOptions4 = sendOptions3;
      PhotonNetwork.RaiseEvent((byte) 176 /*0xB0*/, (object) objArray2, raiseEventOptions3, sendOptions4);
      Projectile.lastSendTime = Time.time;
    }
    if (Projectile.GrabbedGrowing)
    {
      try
      {
        Projectile.ReleaseSnowball();
        ((MonoBehaviour) GorillaTagger.Instance).StartCoroutine(Projectile.DisableSnowball(true));
      }
      catch
      {
      }
      Projectile.GrabbedGrowing = false;
    }
    if (!((UnityEngine.Object) VRRig.LocalRig != (UnityEngine.Object) null) || ((Behaviour) VRRig.LocalRig).enabled)
      return;
    ((Behaviour) VRRig.LocalRig).enabled = true;
  }

  public static SnowballThrowable? GetProjectile(string projectileName)
  {
    List<string> list = ((IEnumerable<GTDirectAssetRef<CosmeticSO>>) ((AllCosmeticsArraySO) ((AssetReference) CosmeticsController.instance.v2_allCosmeticsInfoAssetRef).Asset).sturdyAssetRefs).Where<GTDirectAssetRef<CosmeticSO>>((Func<GTDirectAssetRef<CosmeticSO>, bool>) (x => (UnityEngine.Object) x.obj != (UnityEngine.Object) null && x.obj.info.isThrowable)).Select<GTDirectAssetRef<CosmeticSO>, string>((Func<GTDirectAssetRef<CosmeticSO>, string>) (x => x.obj.info.playFabID)).Distinct<string>().ToList<string>();
    if (Projectile.projlist == null || Projectile.projlist.Count != list.Count - 3)
    {
      if (!CosmeticsV2Spawner_Dirty.isPrepared || !GorillaComputer.instance.isConnectedToMaster)
        return (SnowballThrowable) null;
      if (!Projectile.allSnowballsInitialized && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft.Count >= 1 && CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight.Count >= 1)
      {
        Projectile.allSnowballsInitialized = true;
        LinqUtils.ForEach<KeyValuePair<int, string>>((IEnumerable<KeyValuePair<int, string>>) CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringLeft, (Action<KeyValuePair<int, string>>) (v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value)));
        LinqUtils.ForEach<KeyValuePair<int, string>>((IEnumerable<KeyValuePair<int, string>>) CosmeticsV2Spawner_Dirty.materialIndexToSnowballThrowablePlayfabIdStringRight, (Action<KeyValuePair<int, string>>) (v => VRRig.LocalRig.cosmeticsObjectRegistry.Cosmetic(v.Value)));
        return (SnowballThrowable) null;
      }
      Projectile.projlist = new Dictionary<string, SnowballThrowable>();
      SnowballMaker[] snowballMakerArray = new SnowballMaker[2]
      {
        SnowballMaker.leftHandInstance,
        SnowballMaker.rightHandInstance
      };
      foreach (SnowballMaker snowballMaker in snowballMakerArray)
      {
        foreach (SnowballThrowable snowball in snowballMaker.snowballs)
        {
          try
          {
            Projectile.projlist.Add(((Component) snowball).transform.parent.gameObject.name, snowball);
          }
          catch
          {
          }
        }
      }
    }
    projectileName += "(Clone)";
    SnowballThrowable snowballThrowable;
    return !Projectile.projlist.TryGetValue(projectileName, out snowballThrowable) ? (SnowballThrowable) null : snowballThrowable;
  }

  private static void LaunchSlingshot()
  {
    Slingshot component = GameObject.Find("Photon.Realtime.Player Objects/Local VRRig/Local Gorilla Player/rig/body_pivot/Slingshot Chest Snap/body_AnchorFront_StowSlot").transform.GetChild(1).gameObject.GetComponent<Slingshot>();
    ((TransferrableObject) component).currentState = (TransferrableObject.PositionState) 8;
    ((TransferrableObject) component).itemState = (TransferrableObject.ItemStates) 8;
    component.drawingHand = EquipmentInteractor.instance.rightHand;
    component.center.position = component.centerOrigin.position - Projectile.PosToVector3(GorillaTagger.Instance.rightHandTransform.position, GorillaTagger.Instance.rightHandTransform.position + ((double) ControllerInputPoller.instance.rightControllerGripFloat > 0.5 ? GorillaTagger.Instance.rightHandTransform.forward : ((Component) GorillaTagger.Instance.bodyCollider).transform.forward) * 10f, 15f) * (component.maxDraw * 1.2f);
    component.minTimeToLaunch = Time.time - 1f;
    if (!component.hasDummyProjectile)
      ((TransferrableObject) component).LateUpdateShared();
    ((ProjectileWeapon) component).LaunchProjectile();
    ((TransferrableObject) component).currentState = (TransferrableObject.PositionState) 16 /*0x10*/;
    ((TransferrableObject) component).itemState = (TransferrableObject.ItemStates) 1;
  }

  public enum Projectiles
  {
    None,
    SnowBall,
    BigSnowball,
    PishFood,
    WaterBalloons,
    EasterEggs,
    Books,
    Apple,
    GoldCoin,
    HotDog,
    IceCream,
    Fireworks,
    Paper,
    IceCreamScoop,
    Chips,
    RedBall,
    TacoBall,
    MashedPotatos,
    BreadBall,
    PurpleBall,
    OrangeBall,
    Stuffing,
    Turkey,
    BrokenLavaRocks,
    WorkingLavaRocks,
    Corn,
    FireStick,
    FoodBall,
    PineApple,
    PopCorn,
    FlamingNuts,
    Presents,
    Confeti,
    GiantMeatball,
    CandyBuket,
    slingshot,
  }
}
