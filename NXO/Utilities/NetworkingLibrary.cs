
using ExitGames.Client.Photon;
using NXO.Mods.Categories;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public class NetworkingLibrary : MonoBehaviour
{
  private static readonly Dictionary<VRRig, float> receivedScales = new Dictionary<VRRig, float>();
  private static readonly Dictionary<VRRig, GameObject> networkedDisplacerCannons = new Dictionary<VRRig, GameObject>();
  private static readonly Dictionary<VRRig, GameObject> networkedJetPacks = new Dictionary<VRRig, GameObject>();
  private static readonly List<VRRig> _pruneBuffer = new List<VRRig>();
  private static float _lastPruneTime;
  private const byte NXO_USER_EVENT = 69;
  private static readonly HashSet<string> detectedNXOUsers = new HashSet<string>();
  private static readonly HashSet<string> detectedNXOUserIds = new HashSet<string>();

  public static void SendNetworkUpdate(
    NetworkingLibrary.NetworkingType type,
    object[] data,
    int[] targets = null,
    bool broadcastToAll = true)
  {
    if (!PhotonNetwork.InRoom)
      return;
    RaiseEventOptions receivers = NetworkingLibrary.GetReceivers(broadcastToAll, targets);
    SendOptions sendOptions1 = new SendOptions();
    (sendOptions1).Reliability = true;
    SendOptions sendOptions2 = sendOptions1;
    PhotonNetwork.NetworkingClient.OpRaiseEvent((byte) type, (object) data, receivers, sendOptions2);
  }

  private static RaiseEventOptions GetReceivers(bool broadcastToAll, int[] targets)
  {
    RaiseEventOptions receivers = new RaiseEventOptions();
    if (!broadcastToAll && targets != null && targets.Length != 0)
    {
      receivers.TargetActors = targets;
      receivers.CachingOption = (EventCaching) 0;
    }
    else
    {
      receivers.Receivers = (ReceiverGroup) 0;
      receivers.CachingOption = (EventCaching) 0;
    }
    return receivers;
  }

  public static void BroadcastNXOUser()
  {
    if (!PhotonNetwork.InRoom)
      return;
    try
    {
      object[] objArray1 = new object[3]
      {
        (object) PhotonNetwork.LocalPlayer.NickName,
        (object) PhotonNetwork.LocalPlayer.UserId,
        (object) "NXO v6.3"
      };
      RaiseEventOptions raiseEventOptions1 = new RaiseEventOptions()
      {
        Receivers = (ReceiverGroup) 1,
        CachingOption = (EventCaching) 0
      };
      object[] objArray2 = objArray1;
      RaiseEventOptions raiseEventOptions2 = raiseEventOptions1;
      SendOptions sendOptions1 = new SendOptions();
      (sendOptions1).Reliability = true;
      SendOptions sendOptions2 = sendOptions1;
      PhotonNetwork.RaiseEvent((byte) 69, (object) objArray2, raiseEventOptions2, sendOptions2);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"[NXO Broadcast] Error broadcasting: {ex}");
    }
  }

  public static void ClearDetectedUsers()
  {
    NetworkingLibrary.detectedNXOUsers.Clear();
    NetworkingLibrary.detectedNXOUserIds.Clear();
  }

  public static bool IsNXOUser(string userId)
  {
    return !string.IsNullOrEmpty(userId) && NetworkingLibrary.detectedNXOUserIds.Contains(userId);
  }

  public static List<string> GetDetectedUsers()
  {
    return new List<string>((IEnumerable<string>) NetworkingLibrary.detectedNXOUsers);
  }

  private void Awake()
  {
    PhotonNetwork.NetworkingClient.EventReceived += new Action<EventData>(NetworkingLibrary.OnEvent);
  }

  private void Update()
  {
    if ((double) Time.time - (double) NetworkingLibrary._lastPruneTime >= 1.0)
    {
      NetworkingLibrary._lastPruneTime = Time.time;
      NetworkingLibrary.PruneDeadRigs();
    }
    foreach (KeyValuePair<VRRig, float> receivedScale in NetworkingLibrary.receivedScales)
    {
      if ((UnityEngine.Object) receivedScale.Key != (UnityEngine.Object) null)
        receivedScale.Key.scaleMultiplier = receivedScale.Value;
    }
  }

  private static void PruneDeadRigs()
  {
    IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
    NetworkingLibrary._pruneBuffer.Clear();
    foreach (VRRig key in NetworkingLibrary.receivedScales.Keys)
    {
      if ((UnityEngine.Object) key == (UnityEngine.Object) null || !activeRigs.Contains<VRRig>(key))
        NetworkingLibrary._pruneBuffer.Add(key);
    }
    foreach (VRRig key in NetworkingLibrary._pruneBuffer)
      NetworkingLibrary.receivedScales.Remove(key);
    NetworkingLibrary.PruneObjectDict(NetworkingLibrary.networkedDisplacerCannons, (IEnumerable<VRRig>) activeRigs);
    NetworkingLibrary.PruneObjectDict(NetworkingLibrary.networkedJetPacks, (IEnumerable<VRRig>) activeRigs);
  }

  private static void PruneObjectDict(Dictionary<VRRig, GameObject> dict, IEnumerable<VRRig> active)
  {
    NetworkingLibrary._pruneBuffer.Clear();
    foreach (KeyValuePair<VRRig, GameObject> keyValuePair in dict)
    {
      if ((UnityEngine.Object) keyValuePair.Key == (UnityEngine.Object) null || !active.Contains<VRRig>(keyValuePair.Key))
      {
        if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value);
        NetworkingLibrary._pruneBuffer.Add(keyValuePair.Key);
      }
    }
    foreach (VRRig key in NetworkingLibrary._pruneBuffer)
      dict.Remove(key);
  }

  private void OnDestroy()
  {
    PhotonNetwork.NetworkingClient.EventReceived -= new Action<EventData>(NetworkingLibrary.OnEvent);
  }

  public static void OnEvent(EventData photonEvent)
  {
    if (!PhotonNetwork.InRoom)
      return;
    if (photonEvent.Code == (byte) 69)
      NetworkingLibrary.HandleNXOBroadcast(photonEvent);
    else
      NetworkingLibrary.HandleNetworkedEvents(photonEvent);
  }

  public static void HandleNXOBroadcast(EventData photonEvent)
  {
    try
    {
      if (!(photonEvent.CustomData is object[] customData) || customData.Length < 3)
        return;
      string str1 = customData[0]?.ToString() ?? "Unknown";
      string str2 = customData[1]?.ToString() ?? "Unknown";
      if (str2 == PhotonNetwork.LocalPlayer.UserId)
        return;
      NetworkingLibrary.detectedNXOUserIds.Add(str2);
      string str3 = $"{str1}_{str2}";
      if (!NetworkingLibrary.detectedNXOUsers.Add(str3))
        return;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Alert, $"`{str1}` Is An NXO User");
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"[NXO Broadcast] Error processing event: {ex}");
    }
  }

  public static void HandleNetworkedEvents(EventData photonEvent)
  {
    if (!(photonEvent.CustomData is object[] customData))
      return;
    Photon.Realtime.Player player = PhotonNetwork.CurrentRoom?.GetPlayer(photonEvent.Sender, false);
    if (player == null)
      return;
    VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(((NetPlayer) (player)));
    if ((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null)
      return;
    switch (photonEvent.Code)
    {
      case 80 /*0x50*/:
        if (customData.Length < 1 || !(customData[0] is float num))
          break;
        NetworkingLibrary.receivedScales[rigFromNetPlayer] = Mathf.Clamp(num, 0.375f, 2.75f);
        break;
      case 81:
        NetworkingLibrary.HandleDisplacerCannonCharge(rigFromNetPlayer, customData);
        break;
      case 82:
        NetworkingLibrary.HandleDisplacerCannonShoot(rigFromNetPlayer, customData);
        break;
      case 84:
        NetworkingLibrary.HandleDisplacerCannonExplosion(customData);
        break;
      case 85:
        NetworkingLibrary.HandleDisplacerCannonEquip(rigFromNetPlayer);
        break;
      case 86:
        NetworkingLibrary.HandleDisplacerCannonUnequip(rigFromNetPlayer);
        break;
      case 87:
        NetworkingLibrary.HandleJetPackEquip(rigFromNetPlayer);
        break;
      case 88:
        NetworkingLibrary.HandleJetPackUnequip(rigFromNetPlayer);
        break;
      case 89:
        NetworkingLibrary.HandleJetPackThrust(rigFromNetPlayer, customData);
        break;
    }
  }

  public static void HandleDisplacerCannonCharge(VRRig rig, object[] data)
  {
    if (data.Length < 3 || (UnityEngine.Object) StevesPlayground.displacerCannonBoomPrefab == (UnityEngine.Object) null)
      return;
    Vector3 position = new Vector3((float) data[0], (float) data[1], (float) data[2]);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, position, Quaternion.identity);
    ParticleSystem component1 = gameObject.GetComponent<ParticleSystem>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
    {
      ParticleSystem.MainModule main = component1.main;
      (main).startColor = ((ParticleSystem.MinMaxGradient) (Color.blue));
      (main).startSize = ((ParticleSystem.MinMaxCurve) (0.3f));
      component1.Play();
    }
    AudioSource component2 = gameObject.GetComponent<AudioSource>();
    if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && StevesPlayground.displacerCannonAudioClips.ContainsKey("displacer_spin"))
    {
      component2.clip = StevesPlayground.displacerCannonAudioClips["displacer_spin"];
      component2.volume = 0.1f;
      component2.Play();
    }
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 1.5f);
  }

  public static void HandleDisplacerCannonShoot(VRRig rig, object[] data)
  {
    if (data.Length < 6)
      return;
    Vector3 position = new Vector3((float) data[0], (float) data[1], (float) data[2]);
    Vector3 forward = new Vector3((float) data[3], (float) data[4], (float) data[5]);
    if ((UnityEngine.Object) StevesPlayground.displacerCannonBoomPrefab != (UnityEngine.Object) null)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, position, Quaternion.identity);
      ParticleSystem component1 = gameObject.GetComponent<ParticleSystem>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      {
        ParticleSystem.MainModule main = component1.main;
        (main).startColor = ((ParticleSystem.MinMaxGradient) (Color.yellow));
        (main).startSize = ((ParticleSystem.MinMaxCurve) (0.5f));
        component1.Play();
      }
      AudioSource component2 = gameObject.GetComponent<AudioSource>();
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null && StevesPlayground.displacerCannonAudioClips.ContainsKey("displacer_self"))
      {
        component2.clip = StevesPlayground.displacerCannonAudioClips["displacer_self"];
        component2.volume = 0.15f;
        component2.Play();
      }
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 0.5f);
    }
    if (!((UnityEngine.Object) StevesPlayground.displacerCannonProjectilePrefab != (UnityEngine.Object) null))
      return;
    GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonProjectilePrefab, position, Quaternion.LookRotation(forward));
    gameObject1.AddComponent<NetworkingLibrary.NetworkedProjectile>();
    Rigidbody component = gameObject1.GetComponent<Rigidbody>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      component.AddForce(forward * 30f, (ForceMode) 2);
  }

  public static void HandleDisplacerCannonExplosion(object[] data)
  {
    if (data.Length < 3 || (UnityEngine.Object) StevesPlayground.displacerCannonBoomPrefab == (UnityEngine.Object) null)
      return;
    Vector3 position = new Vector3((float) data[0], (float) data[1], (float) data[2]);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, position, Quaternion.identity);
    gameObject.transform.localScale = Vector3.one * 2f;
    AudioSource component = gameObject.GetComponent<AudioSource>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
    {
      component.volume = 1f;
      component.pitch = 0.8f;
      component.Play();
    }
    UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, 4f);
  }

  public static void HandleDisplacerCannonEquip(VRRig rig)
  {
    if (NetworkingLibrary.networkedDisplacerCannons.ContainsKey(rig) || (UnityEngine.Object) StevesPlayground.displacerCannonObject == (UnityEngine.Object) null || (UnityEngine.Object) rig.rightHandTransform == (UnityEngine.Object) null)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonObject);
    gameObject.name = "NetworkedDisplacerCannon";
    foreach (UnityEngine.Object component in gameObject.GetComponents<StevesPlayground.DisplacerCannon>())
      UnityEngine.Object.Destroy(component);
    gameObject.transform.SetParent(rig.rightHandTransform, false);
    gameObject.transform.localPosition = new Vector3(0.0472f, -0.0124f, -0.0393f);
    gameObject.transform.localRotation = Quaternion.Euler(284.5566f, 0.0f, 270f);
    NetworkingLibrary.networkedDisplacerCannons[rig] = gameObject;
  }

  public static void HandleDisplacerCannonUnequip(VRRig rig)
  {
    GameObject gameObject;
    if (!NetworkingLibrary.networkedDisplacerCannons.TryGetValue(rig, out gameObject))
      return;
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    NetworkingLibrary.networkedDisplacerCannons.Remove(rig);
  }

  public static void HandleJetPackEquip(VRRig rig)
  {
    if (NetworkingLibrary.networkedJetPacks.ContainsKey(rig) || (UnityEngine.Object) StevesPlayground.jetpackPrefab == (UnityEngine.Object) null || (UnityEngine.Object) rig.bodyTransform == (UnityEngine.Object) null)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.jetpackPrefab);
    gameObject.name = "NetworkedJetPack";
    foreach (UnityEngine.Object component in gameObject.GetComponents<StevesPlayground.JetPack>())
      UnityEngine.Object.Destroy(component);
    if ((bool) (UnityEngine.Object) gameObject.GetComponent<Rigidbody>())
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject.GetComponent<Rigidbody>());
    if ((bool) (UnityEngine.Object) gameObject.GetComponent<Collider>())
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject.GetComponent<Collider>());
    gameObject.transform.SetParent(rig.bodyTransform, false);
    gameObject.transform.localPosition = new Vector3(0.0f, -0.2659f, -0.1716f);
    gameObject.transform.localRotation = Quaternion.Euler(270f, 0.0f, 0.0f);
    foreach (Renderer componentsInChild in gameObject.GetComponentsInChildren<Renderer>())
    {
      if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) null && !(componentsInChild is ParticleSystemRenderer))
      {
        foreach (Material material in componentsInChild.materials)
          material.color = Color.black;
      }
    }
    NetworkingLibrary.networkedJetPacks[rig] = gameObject;
  }

  public static void HandleJetPackUnequip(VRRig rig)
  {
    GameObject gameObject;
    if (!NetworkingLibrary.networkedJetPacks.TryGetValue(rig, out gameObject))
      return;
    if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    NetworkingLibrary.networkedJetPacks.Remove(rig);
  }

  public static void HandleJetPackThrust(VRRig rig, object[] data)
  {
    GameObject gameObject;
    if (data.Length < 4 || !NetworkingLibrary.networkedJetPacks.TryGetValue(rig, out gameObject) || (UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    float num = (float) data[3];
    ParticleSystem component1 = gameObject.transform.GetChild(1).GetComponent<ParticleSystem>();
    if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      component1.Play();
    AudioSource component2 = gameObject.GetComponent<AudioSource>();
    if (!((UnityEngine.Object) component2 != (UnityEngine.Object) null))
      return;
    component2.pitch = num;
    component2.volume = 0.2f;
    if (!component2.isPlaying)
      component2.Play();
  }

  public enum NetworkingType : byte
  {
    SizeChanger = 80, 
    DisplacerCannonCharge = 81, 
    DisplacerCannonShoot = 82, 
    DisplacerCannonProjectile = 83, 
    DisplacerCannonExplosion = 84, 
    DisplacerCannonEquip = 85, 
    DisplacerCannonUnequip = 86, 
    JetPackEquip = 87, 
    JetPackUnequip = 88, 
    JetPackThrust = 89, 
  }

  public class NetworkedProjectile : MonoBehaviour
  {
    private bool hasExploded = false;
    private static readonly int _projectileLayerMask = LayerMask.GetMask("Gorilla Object", "Default", "NoMirror");

    private void OnCollisionEnter(Collision collision)
    {
      if (this.hasExploded || (NetworkingLibrary.NetworkedProjectile._projectileLayerMask & 1 << collision.gameObject.layer) == 0)
        return;
      this.hasExploded = true;
      NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonExplosion, new object[3]
      {
        (object) this.transform.position.x,
        (object) this.transform.position.y,
        (object) this.transform.position.z
      });
      if ((UnityEngine.Object) StevesPlayground.displacerCannonBoomPrefab != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonBoomPrefab, this.transform.position, Quaternion.identity), 4f);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject, 1f / 1000f);
    }
  }
}
