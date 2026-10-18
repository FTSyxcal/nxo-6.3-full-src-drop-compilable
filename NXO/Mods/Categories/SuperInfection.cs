
using NXO.Utilities;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class SuperInfection
{
  private static float timer;
  public static SIPlayer siPlayer;
  public static Transform StoredPos;
  public const int WeirdGear = 1573124711;
  public const int StrangeWood = -894667703;
  public const int BouncySand = -1111610567;
  public const int FloppyMetal = -1409076879;
  public const int VibratingSpring = 1618940484;
  public const int MonkeyIdol = 1880272606;
  public const int WristJetJet = 1551901997;
  public const int WristJetPropellor = -1912435955;
  public const int StiltFixed = 1447779317;
  public const int StiltTurkey = 686793174;
  public const int StiltFixedShort = -827046453;
  public const int StiltMotorized2 = 1428761418;
  public const int StiltMotorized3 = 1996041101;
  public const int StiltFixedLong = -1906115882;
  public const int StiltExtendo = 683567723;
  public const int DashYoyo = 1799386883;
  public const int PlatformDeployer = -1236344563;
  public const int PlatformDeployerBouncy = 1657474495;
  public const int TentacleArm = 621310034;
  public const int TentacleArmCrawler = 1814413281;
  public const int TentacleArmStrider = 2060634971;
  public const int AirJuke = -1196783306;
  public const int AirGrab = -2029993207;
  public const int LaserZipline = -1581486942;
  public const int WeakBlaster = 1312505709;
  public const int ChargeBlaster = 1469243263;
  public const int MegaChargeBlaster = -1529067748;
  public const int BlastLobber = -122499862;
  public const int LongBlaster = -108912318;

  private static SuperInfectionManager SIM => SuperInfectionManager.activeSuperInfectionManager;

  public static void RemoveBlasterCooldown(SIGadgetBlaster instance_)
  {
    if ((UnityEngine.Object) instance_ == (UnityEngine.Object) null || !instance_.LocalEquippedOrActivated || !InputHandler.RTrigger() && !InputHandler.LTrigger())
      return;
    instance_.lastFired = -1f;
    instance_.projectileCount = 0;
    SuperInfection.timer += Time.deltaTime;
    while ((double) SuperInfection.timer >= 3.0000001061125658E-07)
    {
      SuperInfection.timer -= 3E-07f;
      ControllerInputPoller.instance.rightControllerIndexFloat = InputHandler.RTrigger() ? 0.0f : 0.6f;
      ControllerInputPoller.instance.leftControllerIndexFloat = InputHandler.LTrigger() ? 0.0f : 0.6f;
    }
    SuperInfection.RemoveBlasterCooldownInner(instance_);
    SuperInfection.RemoveBlasterCooldownInner(instance_);
  }

  private static void RemoveBlasterCooldownInner(SIGadgetBlaster instance_)
  {
    if ((UnityEngine.Object) instance_ == (UnityEngine.Object) null || !instance_.LocalEquippedOrActivated || !InputHandler.RTrigger() && !InputHandler.LTrigger())
      return;
    instance_.lastFired = -1f;
    instance_.projectileCount = 0;
    SuperInfection.timer += Time.deltaTime;
    while ((double) SuperInfection.timer >= 4.0000000467443897E-07)
    {
      SuperInfection.timer -= 4E-07f;
      ControllerInputPoller.instance.rightControllerIndexFloat = InputHandler.RTrigger() ? 0.0f : 0.6f;
      ControllerInputPoller.instance.leftControllerIndexFloat = InputHandler.LTrigger() ? 0.0f : 0.6f;
    }
  }

  public static void UnlockAllGadgets()
  {
    foreach (bool[] flagArray in SIProgression.Instance.unlockedTechTreeData)
    {
      for (int i = 0; i < flagArray.Length; i++)
        flagArray[i] = true;
    }
  }

  public static void CompleteAllQuests()
  {
    foreach (int activeQuestId in SIProgression.Instance.ActiveQuestIds)
    {
      RotatingQuest questById = SIProgression.Instance.questSourceList.GetQuestById(activeQuestId);
      questById.SetProgress(questById.requiredOccurenceCount);
    }
    SIProgression.Instance.SaveQuestProgress();
  }

  public static void DispenseGadget(string gadgetNodeName)
  {
    SICombinedTerminal objectOfType = UnityEngine.Object.FindObjectOfType<SICombinedTerminal>();
    if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null)
      return;
    int num = -1;
    for (int index = 0; index < objectOfType.dispenser.CurrentPage.AllNodes.Count; ++index)
    {
      if (objectOfType.dispenser.CurrentPage.AllNodes[index].Value.nickName == gadgetNodeName)
      {
        num = index;
        break;
      }
    }
    if (num == -1)
      return;
    objectOfType.dispenser._currentNode = num;
    objectOfType.dispenser.gadgetDispensePosition.position = Variables.taggerInstance.rightHandTransform.position;
    objectOfType.dispenser.AuthorityDispenseGadgetForPlayer(SIPlayer.LocalPlayer);
  }

  public static void RainbowStilts(SIGadgetStilt instance_)
  {
    Color rgb = Color.HSVToRGB((float) ((double) Time.time * 0.20000000298023224 % 1.0), 1f, 1f);
    Renderer component1;
    if (instance_.tip.TryGetComponent<Renderer>(out component1))
      component1.material.color = rgb;
    Renderer component2;
    if (!instance_.midpoint.TryGetComponent<Renderer>(out component2))
      return;
    component2.material.color = rgb;
  }

  public static void LongExtendoStilts(SIGadgetStilt instance_)
  {
    if (!instance_.TriggerToExtend)
      return;
    instance_.maxLength = 5f;
    instance_.targetLength = 5f;
  }

  public static void RemoveDashCooldown(SIGadgetDashYoyo instance_)
  {
    if (instance_._state != (SIGadgetDashYoyo.EState) 1)
      return;
    instance_._state = (SIGadgetDashYoyo.EState) 0;
  }

  public static void InfCharges(SIGadgetPlatformDeployer instance_)
  {
    instance_.remainingRechargeTime = 0.0f;
  }

  public static void AddResources(SIResource.ResourceType resourceType, int amount)
  {
    if (!SIProgression.Instance.resourceDict.ContainsKey(resourceType))
      return;
    if (resourceType == 0)
      SIProgression.Instance.resourceDict[resourceType] += amount;
    else
      SIProgression.Instance.resourceDict[resourceType] += Math.Min(amount, SIProgression.Instance.GetResourceMaxCap(resourceType) - SIProgression.Instance.resourceDict[resourceType]);
    SIProgression.Instance.AttemptIncrementResource(resourceType);
  }

  public static Vector3 PosToVector3(Vector3 currentPos, Vector3 targetPos, float speed)
  {
    return (targetPos - currentPos).normalized * speed;
  }

  public static void GunFlingTest()
  {
    if (GunLib.SetupLockOnGun())
    {
      SuperInfection.siPlayer = ((Component) GunLib.lockedTargetRig).gameObject.GetComponent<SIPlayer>();
      if (!((UnityEngine.Object) SuperInfection.siPlayer != (UnityEngine.Object) null))
        return;
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = false;
      ((Component) Variables.taggerInstance.offlineVRRig).transform.position = ((Component) GunLib.lockedTargetRig).transform.position + new Vector3(0.0f, 0.2f, 0.0f);
      Variables.taggerInstance.rightHandTransform.position = GunLib.lockedTargetRig.bodyTransform.position;
      Variables.taggerInstance.leftHandTransform.position = GunLib.lockedTargetRig.bodyTransform.position;
      Variables.taggerInstance.leftHandTransform.rotation = Quaternion.Euler(-90f, 0.0f, 0.0f);
      Variables.taggerInstance.rightHandTransform.rotation = Quaternion.Euler(-90f, 0.0f, 0.0f);
      foreach (SIGadgetBlaster instance_ in SuperInfection.GetActiveLocalPlayerGadgets().Select<SIGadget, SIGadgetBlaster>((Func<SIGadget, SIGadgetBlaster>) (x => ((Component) x).GetComponent<SIGadgetBlaster>())).Where<SIGadgetBlaster>((Func<SIGadgetBlaster, bool>) (x => (UnityEngine.Object) x != (UnityEngine.Object) null)))
      {
        SuperInfection.RemoveBlasterCooldown(instance_);
        SuperInfection.TargetPlayer(instance_, SuperInfection.siPlayer, SuperInfection.PosToVector3(Variables.taggerInstance.rightHandTransform.position, ((Component) SuperInfection.siPlayer).transform.position, 2f), true);
      }
    }
    else
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
  }

  public static void ForceBlasterSpamAtTarget(
    SIPlayer shooter,
    SIPlayer target,
    int blastersPerType = 3)
  {
    int[] numArray = new int[5]
    {
      1312505709,
      1469243263,
      -1529067748,
      -122499862,
      -108912318
    };
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    Vector3 position = target.gamePlayer.rig.headMesh.transform.position;
    foreach (int num in numArray)
    {
      for (int index = 0; index < blastersPerType; ++index)
      {
        Vector3 vector3 = position + new Vector3(UnityEngine.Random.Range(-5f, 5f), 6f, UnityEngine.Random.Range(-5f, 5f));
        Vector3 normalized = (position - vector3).normalized;
        int netId = gameEntityManager.CreateNetId(1 + gameEntityManager.FactoryGetBuiltInEntityCountById(num));
        gameEntityManager.photonView.RPC("CreateItemRPC", (RpcTarget) 0, new object[6]
        {
          (object) new int[1]{ netId },
          (object) new int[1]{ num },
          (object) new long[1]
          {
            BitPackUtils.PackWorldPosForNetwork(vector3)
          },
          (object) new int[1]
          {
            BitPackUtils.PackQuaternionForNetwork(Quaternion.LookRotation(normalized))
          },
          (object) new long[1],
          (object) new int[1]{ -1 }
        });
        gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget) 0, new object[8]
        {
          (object) netId,
          (object) false,
          (object) vector3,
          (object) Quaternion.LookRotation(normalized),
          (object) (normalized * 20f),
          (object) Vector3.zero,
          (object) shooter.gamePlayer.rig.OwningNetPlayer.GetPlayerRef(),
          (object) PhotonNetwork.Time
        });
      }
    }
    PhotonNetwork.SendAllOutgoingCommands();
  }

  public static void TargetPlayer(
    SIGadgetBlaster instance_,
    SIPlayer target,
    Vector3 velocity,
    bool Float = false)
  {
    if (instance_.activeProjectiles.Count <= 0)
      return;
    foreach (SIGadgetBlasterProjectile activeProjectile in instance_.activeProjectiles)
    {
      if (!((UnityEngine.Object) activeProjectile.parentBlaster != (UnityEngine.Object) instance_) && !((UnityEngine.Object) activeProjectile.firedByPlayer != (UnityEngine.Object) SIPlayer.LocalPlayer))
      {
        Transform bodyTransform = ((Component) target).gameObject.GetComponent<VRRig>().bodyTransform;
        instance_.firingPosition.position = bodyTransform.position - (Float ? new Vector3(0.0f, 0.3f, 0.0f) : Vector3.zero);
        instance_.firingPosition.rotation = Quaternion.LookRotation(bodyTransform.forward, bodyTransform.up);
        activeProjectile.rb.velocity = velocity;
      }
    }
  }

  public static void MaxChargeAlways(SIGadgetChargeBlaster instance_)
  {
    instance_.maxChargeDiff = 9999f;
    SIGadgetChargeBlaster gadgetChargeBlaster = instance_;
    SIGadgetChargeBlaster.BlasterChargeLevel[] chargeLevels = instance_.chargeLevels;
    double num = (double) chargeLevels[chargeLevels.Length - 1].chargeThreshold + 1.0;
    gadgetChargeBlaster.currentCharge = (float) num;
  }

  public static void ForceMaxCharge(SIGadgetChargeBlaster instance)
  {
    if ((UnityEngine.Object) instance == (UnityEngine.Object) null || instance.chargeLevels == null || instance.chargeLevels.Length == 0)
      return;
    SIGadgetChargeBlaster gadgetChargeBlaster = instance;
    SIGadgetChargeBlaster.BlasterChargeLevel[] chargeLevels = instance.chargeLevels;
    double num = (double) chargeLevels[chargeLevels.Length - 1].chargeThreshold + 1.0;
    gadgetChargeBlaster.currentCharge = (float) num;
  }

  public static void FastCharge(SIGadgetChargeBlaster instance_)
  {
    instance_.maxChargeDiff = 9999f;
    ((Component) instance_).SendMessage("OnUpdateAuthority", (object) Time.deltaTime, SendMessageOptions.DontRequireReceiver);
  }

  public static void ForceFireBlasterAtTarget(SIPlayer shooter, SIPlayer target)
  {
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    foreach (int activePlayerGadget in shooter.activePlayerGadgets)
    {
      GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(activePlayerGadget);
      if (!((UnityEngine.Object) gameEntityFromNetId == (UnityEngine.Object) null))
      {
        SIGadgetBlaster component = ((Component) gameEntityFromNetId).GetComponent<SIGadgetBlaster>();
        if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
        {
          Vector3 normalized = (target.gamePlayer.rig.headMesh.transform.position - component.firingPosition.position).normalized;
          SuperInfection.SIM.CallRPC((SuperInfectionManager.ClientToClientRPC) 3, new object[3]
          {
            (object) activePlayerGadget,
            (object) 0,
            (object) new object[3]
            {
              (object) component.NextFireId(),
              (object) component.firingPosition.position,
              (object) (normalized * 25f)
            }
          });
        }
      }
    }
  }

  public static void GiveTechPoints(SIPlayer player, int amount)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null || amount <= 0)
      return;
    SIPlayer.ProgressionData currentProgression = player.CurrentProgression;
    int[] destinationArray = new int[currentProgression.currentQuestProgresses.Length + 1];
    Array.Copy((Array) currentProgression.currentQuestProgresses, (Array) destinationArray, currentProgression.currentQuestProgresses.Length);
    int[] numArray = destinationArray;
    numArray[numArray.Length - 1] = amount;
    player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress + amount, currentProgression.currentQuestIds, destinationArray);
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void CompleteAllQuestsAndClaimPoints(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    SIPlayer.ProgressionData currentProgression = player.CurrentProgression;
    SIProgression.SIQuestsList questSourceList = SIProgression.Instance.questSourceList;
    for (int index = 0; index < currentProgression.currentQuestIds.Length; ++index)
    {
      RotatingQuest questById = questSourceList.GetQuestById(currentProgression.currentQuestIds[index]);
      if (questById != null)
      {
        currentProgression.currentQuestProgresses[index] = questById.requiredOccurenceCount;
        currentProgression.bonusProgress += 100;
        questById.questManager?.HandleQuestCompleted(questById.questID);
      }
    }
    player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress, currentProgression.currentQuestIds, currentProgression.currentQuestProgresses);
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void ResetPlayerGadgets(SIPlayer player)
  {
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    for (int index = 0; index < player.activePlayerGadgets.Count; ++index)
      gameEntityManager.RequestDestroyItem(gameEntityManager.GetEntityIdFromNetId(player.activePlayerGadgets[index]));
    player.activePlayerGadgets.Clear();
  }

  public static GameEntity SpawnGadgetAtPosition(GameObject gadgetPrefab, Vector3 position)
  {
    GameEntityManager activeManager = GameEntityManager.activeManager;
    int staticHash = StaticHashExt.GetStaticHash(gadgetPrefab.name);
    GameEntityId gameEntityId = activeManager.RequestCreateItem(staticHash, position, Quaternion.identity, 0L);
    return activeManager.GetGameEntity(gameEntityId);
  }

  public static List<SIGadget> GetActiveLocalPlayerGadgets()
  {
    List<SIGadget> localPlayerGadgets = new List<SIGadget>();
    if ((UnityEngine.Object) SIPlayer.LocalPlayer == (UnityEngine.Object) null)
      return localPlayerGadgets;
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    foreach (int activePlayerGadget in SIPlayer.LocalPlayer.activePlayerGadgets)
    {
      SIGadget component = ((Component) gameEntityManager.GetGameEntity(gameEntityManager.GetEntityIdFromNetId(activePlayerGadget)))?.GetComponent<SIGadget>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        localPlayerGadgets.Add(component);
    }
    return localPlayerGadgets;
  }

  public static void UnlockTechNode(SIPlayer player, int tierIndex, int nodeIndex)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    SIPlayer.ProgressionData currentProgression = player.CurrentProgression;
    if (currentProgression.techTreeData[tierIndex][nodeIndex])
      return;
    currentProgression.techTreeData[tierIndex][nodeIndex] = true;
    player.UpdateProgression(currentProgression.resourceArray, currentProgression.limitedDepositTimeArray, currentProgression.techTreeData, currentProgression.stashedQuests, currentProgression.stashedBonusPoints, currentProgression.bonusProgress, currentProgression.currentQuestIds, currentProgression.currentQuestProgresses);
    SIPlayer.SetAndBroadcastProgression();
  }

  public static GameEntity SpawnGadgetForPlayer(SIPlayer player, string gadgetName)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return (GameEntity) null;
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    int staticHash = StaticHashExt.GetStaticHash(gadgetName);
    if (!gameEntityManager.FactoryHasEntity(staticHash))
      return (GameEntity) null;
    GameEntityId gameEntityId = gameEntityManager.RequestCreateItem(staticHash, player.gamePlayer.rig.rightHandTransform.position, Quaternion.identity, (long) SIPlayer.LocalPlayer.ActorNr);
    return gameEntityManager.GetGameEntity(gameEntityId);
  }

  public static void SpawnGadgetOnAllPlayers(string gadgetName)
  {
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    int staticHash = StaticHashExt.GetStaticHash(gadgetName);
    if (!gameEntityManager.FactoryHasEntity(staticHash))
      return;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      SIPlayer siPlayer = SIPlayer.Get(activeRig);
      if (!((UnityEngine.Object) siPlayer == (UnityEngine.Object) null))
        gameEntityManager.RequestCreateItem(staticHash, activeRig.rightHandTransform.position, Quaternion.identity, (long) siPlayer.ActorNr);
    }
  }

  public static void SpawnGadgetAtHand(string gadgetName, bool leftHand = false)
  {
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    int staticHash = StaticHashExt.GetStaticHash(gadgetName);
    if (!gameEntityManager.FactoryHasEntity(staticHash))
      return;
    Vector3 vector3 = leftHand ? Variables.taggerInstance.leftHandTransform.position : Variables.taggerInstance.rightHandTransform.position;
    gameEntityManager.RequestCreateItem(staticHash, vector3, Quaternion.identity, (long) SIPlayer.LocalPlayer.ActorNr);
  }

  public static void SpawnAllUnlockedGadgets(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    SITechTreeSO progressionSo = SIPlayer.progressionSO;
    for (int index1 = 0; index1 < player.CurrentProgression.techTreeData.Length; ++index1)
    {
      for (int index2 = 0; index2 < player.CurrentProgression.techTreeData[index1].Length; ++index2)
      {
        if (player.CurrentProgression.techTreeData[index1][index2] && progressionSo.IsValidNode(index1, index2))
        {
          SITechTreeNode treeNode = progressionSo.GetTreeNode(index1, index2);
          if (treeNode != null && treeNode.IsDispensableGadget)
          {
            int staticHash = StaticHashExt.GetStaticHash(((Component) treeNode.unlockedGadgetPrefab).gameObject.name);
            if (gameEntityManager.FactoryHasEntity(staticHash))
              gameEntityManager.RequestCreateItem(staticHash, player.gamePlayer.rig.rightHandTransform.position + UnityEngine.Random.insideUnitSphere * 0.3f, Quaternion.identity, (long) player.ActorNr);
          }
        }
      }
    }
  }

  public static void DeleteAllGadgets()
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      SIPlayer siPlayer = SIPlayer.Get(activeRig);
      if ((UnityEngine.Object) siPlayer != (UnityEngine.Object) null)
        SuperInfection.SIM.ClearPlayerGadgets(siPlayer);
    }
  }

  public static void DeleteAllGadgetsForPlayer(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    SuperInfection.SIM.ClearPlayerGadgets(player);
  }

  public static void StealGadget(SIPlayer target)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) null || target.activePlayerGadgets.Count == 0)
      return;
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(target.activePlayerGadgets[0]);
    if ((UnityEngine.Object) gameEntityFromNetId == (UnityEngine.Object) null)
      return;
    SIGadget component = ((Component) gameEntityFromNetId).GetComponent<SIGadget>();
    bool flag = false;
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      component.FindAttachedHand(out flag);
    gameEntityManager.RequestGrabEntity(gameEntityFromNetId.id, flag, ((Component) gameEntityFromNetId).transform.localPosition, ((Component) gameEntityFromNetId).transform.localRotation);
  }

  public static void ForceDropAllGadgets(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null || (UnityEngine.Object) player.gamePlayer?.rig == (UnityEngine.Object) null)
      return;
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    if (!gameEntityManager.IsAuthority())
      return;
    foreach (int num in new List<int>((IEnumerable<int>) player.activePlayerGadgets))
    {
      GameEntity gameEntityFromNetId = gameEntityManager.GetGameEntityFromNetId(num);
      if (!((UnityEngine.Object) gameEntityFromNetId == (UnityEngine.Object) null) && gameEntityManager.IsValidNetId(num))
      {
        bool flag = player.gamePlayer.IsHoldingEntity(gameEntityFromNetId.id, true);
        Vector3 vector3 = new Vector3(UnityEngine.Random.Range(-1f, 1f), 2.5f, UnityEngine.Random.Range(-1f, 1f)).normalized * 3f;
        gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget) 0, new object[8]
        {
          (object) num,
          (object) flag,
          (object) ((Component) gameEntityFromNetId).transform.position,
          (object) ((Component) gameEntityFromNetId).transform.rotation,
          (object) vector3,
          (object) Vector3.zero,
          (object) PhotonNetwork.LocalPlayer,
          (object) PhotonNetwork.Time
        });
      }
    }
  }

  public static void ForceCreateItemForPlayer(
    SIPlayer player,
    int entityTypeId,
    Vector3 spawnPosition,
    bool shoot = false)
  {
    GameEntityManager gameEntityManager = SuperInfection.SIM.gameEntityManager;
    int netId = gameEntityManager.CreateNetId(1 + gameEntityManager.FactoryGetBuiltInEntityCountById(entityTypeId));
    gameEntityManager.photonView.RPC("CreateItemRPC", (RpcTarget) 1, new object[6]
    {
      (object) new int[1]{ netId },
      (object) new int[1]{ entityTypeId },
      (object) new long[1]
      {
        BitPackUtils.PackWorldPosForNetwork(spawnPosition)
      },
      (object) new int[1]
      {
        BitPackUtils.PackQuaternionForNetwork(Quaternion.identity)
      },
      (object) new long[1],
      (object) new int[1]{ -1 }
    });
    if (shoot)
      gameEntityManager.photonView.RPC("ThrowEntityRPC", (RpcTarget) 0, new object[8]
      {
        (object) netId,
        (object) false,
        (object) player.gamePlayer.rig.rightHandTransform.position,
        (object) player.gamePlayer.rig.rightHandTransform.rotation,
        (object) (player.gamePlayer.rig.rightHandTransform.forward * 20f),
        (object) Vector3.zero,
        (object) player.gamePlayer.rig.OwningNetPlayer.GetPlayerRef(),
        (object) PhotonNetwork.Time
      });
    else
      gameEntityManager.photonView.RPC("GrabEntityRPC", (RpcTarget) 0, new object[4]
      {
        (object) netId,
        (object) false,
        (object) BitPackUtils.PackHandPosRotForNetwork(Vector3.zero, Quaternion.identity),
        (object) player.gamePlayer.rig.OwningNetPlayer.GetPlayerRef()
      });
  }

  public static void KnockbackAllPlayers(float strength)
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      SIPlayer siPlayer = SIPlayer.Get(activeRig);
      if (!((UnityEngine.Object) siPlayer == (UnityEngine.Object) null) && !((UnityEngine.Object) siPlayer == (UnityEngine.Object) SIPlayer.LocalPlayer))
        siPlayer.PlayerKnockback((((Component) activeRig).transform.position - ((Component) VRRig.LocalRig).transform.position).normalized * strength, true, false);
    }
  }

  public static void KnockbackPlayer(SIPlayer player, Vector3 direction, float strength)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    player.PlayerKnockback(direction.normalized * strength, true, false);
  }

  public static void LaunchAllPlayersUp(float strength)
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      SIPlayer siPlayer = SIPlayer.Get(activeRig);
      if ((UnityEngine.Object) siPlayer != (UnityEngine.Object) null)
        siPlayer.PlayerKnockback(Vector3.up * strength, true, false);
    }
  }

  public static void GiveAllResourcesToMax(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    foreach (SIResource.ResourceType key in Enum.GetValues(typeof (SIResource.ResourceType)))
    {
      if (SIProgression.Instance.resourceDict.ContainsKey(key))
        SIProgression.Instance.resourceDict[key] = SIProgression.Instance.GetResourceMaxCap(key);
    }
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void UnlockFullTechTree()
  {
    bool[][] unlockedTechTreeData = SIProgression.Instance.unlockedTechTreeData;
    for (int index1 = 0; index1 < unlockedTechTreeData.Length; ++index1)
    {
      for (int index2 = 0; index2 < unlockedTechTreeData[index1].Length; ++index2)
        unlockedTechTreeData[index1][index2] = true;
    }
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void SetBonusProgress(int amount)
  {
    SIProgression.Instance.bonusProgress = amount;
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void MaxStashedQuests()
  {
    SIProgression.Instance.stashedQuests = (int) byte.MaxValue;
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void MaxStashedBonusPoints()
  {
    SIProgression.Instance.stashedBonusPoints = (int) byte.MaxValue;
    SIPlayer.SetAndBroadcastProgression();
  }

  public static void BroadcastFakeProgression(int[] fakeResources, bool[][] fakeTechTree)
  {
    SIPlayer.ProgressionData currentProgression = SIPlayer.LocalPlayer.CurrentProgression;
    SuperInfection.SIM.CallRPC((SuperInfectionManager.ClientToClientRPC) 0, new object[8]
    {
      (object) fakeResources,
      (object) new int[2],
      (object) fakeTechTree,
      (object) (int) byte.MaxValue,
      (object) (int) byte.MaxValue,
      (object) (int) byte.MaxValue,
      (object) currentProgression.currentQuestIds,
      (object) currentProgression.currentQuestProgresses
    });
  }

  public static void SendFakeIdolCelebration(Vector3 position)
  {
    SuperInfection.SIM.CallRPC((SuperInfectionManager.AuthorityToClientRPC) 5, new object[1]
    {
      (object) position
    });
  }

  public static void RequestEntityStateSpam(SIGadget gadget, long state, int times)
  {
    if ((UnityEngine.Object) gadget == (UnityEngine.Object) null)
      return;
    for (int index = 0; index < times; ++index)
      gadget.gameEntity.RequestState(gadget.gameEntity.id, state);
  }

  public static void TeleportAllGadgetsToPlayer(SIPlayer player)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    Vector3 position = ((Component) player.gamePlayer.rig).transform.position;
    foreach (GameEntity gameEntity in SuperInfection.SIM.gameEntityManager.GetGameEntities())
    {
      if (!((UnityEngine.Object) gameEntity == (UnityEngine.Object) null) && !((UnityEngine.Object) ((Component) gameEntity).GetComponent<SIGadget>() == (UnityEngine.Object) null))
        ((Component) gameEntity).transform.position = position + UnityEngine.Random.insideUnitSphere * 0.5f;
    }
  }

  public static void FreezeAllGadgets()
  {
    foreach (GameEntity gameEntity in SuperInfection.SIM.gameEntityManager.GetGameEntities())
    {
      if (!((UnityEngine.Object) gameEntity == (UnityEngine.Object) null))
      {
        Rigidbody component = ((Component) gameEntity).GetComponent<Rigidbody>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          component.isKinematic = true;
      }
    }
  }

  public static void UnfreezeAllGadgets()
  {
    foreach (Component gameEntity in SuperInfection.SIM.gameEntityManager.GetGameEntities())
    {
      Rigidbody component = gameEntity.GetComponent<Rigidbody>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        component.isKinematic = false;
    }
  }

  public static void ExplodeGadgetsOutward(float force)
  {
    Vector3 position = ((Component) VRRig.LocalRig).transform.position;
    foreach (GameEntity gameEntity in SuperInfection.SIM.gameEntityManager.GetGameEntities())
    {
      Rigidbody component = ((Component) gameEntity).GetComponent<Rigidbody>();
      component.isKinematic = false;
      component.linearVelocity = (((Component) gameEntity).transform.position - position).normalized * force;
    }
  }

  public static List<SIGadget> GetAllActiveGadgets()
  {
    List<SIGadget> allActiveGadgets = new List<SIGadget>();
    foreach (Component gameEntity in SuperInfection.SIM.gameEntityManager.GetGameEntities())
    {
      SIGadget component = gameEntity.GetComponent<SIGadget>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        allActiveGadgets.Add(component);
    }
    return allActiveGadgets;
  }

  public static void ClearExclusionZones(SIPlayer player) => player.exclusionZoneCount = 0;

  public static void ClearAllExclusionZones()
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      SIPlayer siPlayer = SIPlayer.Get(activeRig);
      if ((UnityEngine.Object) siPlayer != (UnityEngine.Object) null)
        siPlayer.exclusionZoneCount = 0;
    }
    SIPlayer.LocalPlayer.exclusionZoneCount = 0;
  }

  public static void ClearAllProjectiles(SIGadgetBlaster blaster)
  {
    foreach (SIGadgetBlasterProjectile activeProjectile in blaster.activeProjectiles)
    {
      if ((UnityEngine.Object) activeProjectile != (UnityEngine.Object) null)
        blaster.DespawnProjectile(activeProjectile);
    }
    blaster.activeProjectiles.Clear();
  }

  public static void SetProjectileVelocity(SIGadgetBlaster blaster, Vector3 velocity)
  {
    foreach (SIGadgetBlasterProjectile activeProjectile in blaster.activeProjectiles)
    {
      if ((UnityEngine.Object) activeProjectile != (UnityEngine.Object) null)
        activeProjectile.rb.linearVelocity = velocity;
    }
  }

  public static void RedirectProjectilesToTarget(SIGadgetBlaster blaster, Transform target)
  {
    foreach (SIGadgetBlasterProjectile activeProjectile in blaster.activeProjectiles)
    {
      if (!((UnityEngine.Object) activeProjectile == (UnityEngine.Object) null))
      {
        Rigidbody rb = activeProjectile.rb;
        Vector3 vector3_1 = target.position - ((Component) activeProjectile).transform.position;
        Vector3 normalized = vector3_1.normalized;
        vector3_1 = activeProjectile.rb.linearVelocity;
        double magnitude = (double) vector3_1.magnitude;
        Vector3 vector3_2 = normalized * (float) magnitude;
        rb.linearVelocity = vector3_2;
      }
    }
  }

  public enum GadgetTypes
  {
    Thrusters,
    LongArms,
    Dash,
    Platforms,
    Blasters,
  }
}
