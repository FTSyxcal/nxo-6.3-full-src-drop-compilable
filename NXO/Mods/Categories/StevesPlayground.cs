
using GorillaLocomotion;
using NXO.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable enable
namespace NXO.Mods.Categories;

public static class StevesPlayground
{
  public static GameObject displacerCannonObject;
  public static GameObject displacerCannonProjectilePrefab;
  public static GameObject displacerCannonBoomPrefab;
  public static GameObject jetpackPrefab;
  public static GameObject currentJetPack;
  public static GameObject webSlingPrefab;
  public static GameObject leftWebSling;
  public static GameObject rightWebSling;
  public static Dictionary<string, AudioClip> displacerCannonAudioClips = new Dictionary<string, AudioClip>();
  public static AssetBundle AssetBundle;
  private static List<Transform> boneSearchList = new List<Transform>(32 /*0x20*/);

  public static void Runnable(StevesPlayground.FuncType type)
  {
    if ((UnityEngine.Object) StevesPlayground.AssetBundle == (UnityEngine.Object) null)
    {
      StevesPlayground.AssetBundle = AssetHandler.LoadAssetBundle("NXO.Resources.nxostevestuff");
      StevesPlayground.displacerCannonObject = StevesPlayground.AssetBundle.LoadAsset<GameObject>("Displacer_P");
      StevesPlayground.displacerCannonProjectilePrefab = StevesPlayground.AssetBundle.LoadAsset<GameObject>("DisplacerShot_P");
      StevesPlayground.displacerCannonBoomPrefab = StevesPlayground.AssetBundle.LoadAsset<GameObject>("DisplacerCumshot_P");
      StevesPlayground.jetpackPrefab = StevesPlayground.AssetBundle.LoadAsset<GameObject>("JetPack_P");
      StevesPlayground.currentJetPack = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.jetpackPrefab);
      StevesPlayground.currentJetPack.name = "JetPack";
      UnityEngine.Object.Destroy((UnityEngine.Object) StevesPlayground.currentJetPack.GetComponent<Rigidbody>());
      UnityEngine.Object.Destroy((UnityEngine.Object) StevesPlayground.currentJetPack.GetComponent<Collider>());
      StevesPlayground.currentJetPack.transform.SetParent(((Component) Variables.taggerInstance.offlineVRRig.bodyRenderer).transform, true);
      StevesPlayground.currentJetPack.transform.localPosition = new Vector3(0.0f, -0.2659f, -0.1716f);
      StevesPlayground.currentJetPack.transform.localRotation = Quaternion.Euler(270f, 0.0f, 0.0f);
      StevesPlayground.currentJetPack.SetActive(false);
      StevesPlayground.currentJetPack.AddComponent<StevesPlayground.JetPack>().thrusterEffect = StevesPlayground.currentJetPack.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
      StevesPlayground.currentJetPack.GetComponent<StevesPlayground.JetPack>().source = StevesPlayground.currentJetPack.GetComponent<AudioSource>();
      StevesPlayground.displacerCannonAudioClips = new Dictionary<string, AudioClip>();
      foreach (AudioClip loadAllAsset in StevesPlayground.AssetBundle.LoadAllAssets<AudioClip>())
      {
        if (((UnityEngine.Object) loadAllAsset).name.StartsWith("displacer_"))
        {
          StevesPlayground.displacerCannonAudioClips[((UnityEngine.Object) loadAllAsset).name] = loadAllAsset;
          Debug.Log((object) ("Loaded Displacer Cannon Audio Clip: " + ((UnityEngine.Object) loadAllAsset).name));
        }
      }
      StevesPlayground.webSlingPrefab = StevesPlayground.AssetBundle.LoadAsset<GameObject>("Websling_P");
    }
    switch (type)
    {
      case StevesPlayground.FuncType.DisplacerCannon:
        if (StevesPlayground.DisplacerCannon.isActive)
          break;
        StevesPlayground.DisplacerCannon.isActive = true;
        Debug.Log((object) "Displacer Cannon Activated!");
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonEquip, new object[0]);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonObject);
        StevesPlayground.DisplacerCannon displacerCannon = gameObject.AddComponent<StevesPlayground.DisplacerCannon>();
        displacerCannon.shootSound = StevesPlayground.displacerCannonAudioClips["displacer_self"];
        displacerCannon.chargeSound = StevesPlayground.displacerCannonAudioClips["displacer_spin"];
        displacerCannon.audioSource = gameObject.AddComponent<AudioSource>();
        displacerCannon.animator = gameObject.GetComponentInChildren<Animator>();
        displacerCannon.shoot = StevesPlayground.FindBone("Launch", gameObject.transform).GetComponent<ParticleSystem>();
        displacerCannon.charge = StevesPlayground.FindBone("Charge", gameObject.transform).GetComponent<ParticleSystem>();
        displacerCannon.transform.SetParent(Variables.taggerInstance.offlineVRRig.rightHandTransform, true);
        displacerCannon.transform.localPosition = new Vector3(0.0472f, -0.0124f, -0.0393f);
        displacerCannon.transform.localRotation = Quaternion.Euler(284.5566f, 0.0f, 270f);
        break;
      case StevesPlayground.FuncType.Jetpack:
        if (StevesPlayground.JetPack.isActive)
          break;
        StevesPlayground.JetPack.isActive = true;
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackEquip, new object[0]);
        StevesPlayground.currentJetPack.SetActive(true);
        foreach (Renderer componentsInChild in StevesPlayground.currentJetPack.GetComponentsInChildren<Renderer>())
        {
          if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) null && !(componentsInChild is ParticleSystemRenderer))
          {
            foreach (Material material in componentsInChild.materials)
              material.color = Color.black;
          }
        }
        Debug.Log((object) "JetPack Activated!");
        break;
    }
  }

  public static void Removeable(StevesPlayground.FuncType type)
  {
    switch (type)
    {
      case StevesPlayground.FuncType.DisplacerCannon:
        if (!StevesPlayground.DisplacerCannon.isActive)
          break;
        StevesPlayground.DisplacerCannon.isActive = false;
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonUnequip, new object[0]);
        Debug.Log((object) "Displacer Cannon Deactivated!");
        break;
      case StevesPlayground.FuncType.Jetpack:
        if (!StevesPlayground.JetPack.isActive)
          break;
        StevesPlayground.JetPack.isActive = false;
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackUnequip, new object[0]);
        Debug.Log((object) "JetPack Deactivated!");
        break;
    }
  }

  public static void Manager(StevesPlayground.FuncType type, bool add)
  {
    if (add)
      StevesPlayground.Runnable(type);
    else
      StevesPlayground.Removeable(type);
  }

  public static Transform FindBone(string name, Transform targ)
  {
    StevesPlayground.boneSearchList.Clear();
    StevesPlayground.boneSearchList.Add(targ);
    while (StevesPlayground.boneSearchList.Count > 0)
    {
      Transform boneSearch = StevesPlayground.boneSearchList[StevesPlayground.boneSearchList.Count - 1];
      StevesPlayground.boneSearchList.RemoveAt(StevesPlayground.boneSearchList.Count - 1);
      if (boneSearch.name == name)
        return boneSearch;
      int index = 0;
      for (int childCount = boneSearch.childCount; index < childCount; ++index)
        StevesPlayground.boneSearchList.Add(boneSearch.GetChild(index));
    }
    return (Transform) null;
  }

  public class DisplacerCannon : MonoBehaviour
  {
    public static bool isActive;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip chargeSound;
    public Animator animator;
    public ParticleSystem shoot;
    public ParticleSystem charge;
    public bool isCharging = false;

    public void Update()
    {
      if (!StevesPlayground.DisplacerCannon.isActive)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
      }
      else
      {
        if (this.isCharging || !InputHandler.RTrigger() && !((ButtonControl) Keyboard.current.enterKey).isPressed)
          return;
        this.StartCoroutine(this.Charge());
      }
    }

    public void Shoot()
    {
      Transform bone = StevesPlayground.FindBone("Launch", this.transform);
      NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonShoot, new object[6]
      {
        (object) bone.position.x,
        (object) bone.position.y,
        (object) bone.position.z,
        (object) (-bone.up).x,
        (object) (-bone.up).y,
        (object) (-bone.up).z
      });
      if ((UnityEngine.Object) this.audioSource != (UnityEngine.Object) null && (UnityEngine.Object) this.shootSound != (UnityEngine.Object) null)
      {
        this.audioSource.volume = 0.15f;
        this.audioSource.PlayOneShot(this.shootSound);
      }
      if ((UnityEngine.Object) this.shoot != (UnityEngine.Object) null)
        this.shoot.Play();
      if (!((UnityEngine.Object) StevesPlayground.displacerCannonProjectilePrefab != (UnityEngine.Object) null))
        return;
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StevesPlayground.displacerCannonProjectilePrefab, bone.position, bone.rotation);
      gameObject.AddComponent<StevesPlayground.DisplacerCannon.Projectile>();
      Rigidbody component = gameObject.GetComponent<Rigidbody>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        component.AddForce(-bone.up * 30f, (ForceMode) 2);
    }

    public IEnumerator Charge()
    {
      if (!this.isCharging)
      {
        this.isCharging = true;
        Transform chargeBone = StevesPlayground.FindBone(nameof (Charge), this.transform);
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.DisplacerCannonCharge, new object[3]
        {
          (object) chargeBone.position.x,
          (object) chargeBone.position.y,
          (object) chargeBone.position.z
        });
        float duration = 1f;
        float elapsed = 0.0f;
        this.charge.Play();
        if ((UnityEngine.Object) this.animator != (UnityEngine.Object) null)
          this.animator.speed = 0.0f;
        if ((UnityEngine.Object) this.audioSource != (UnityEngine.Object) null && (UnityEngine.Object) this.chargeSound != (UnityEngine.Object) null)
        {
          this.audioSource.volume = 0.1f;
          this.audioSource.PlayOneShot(this.chargeSound);
        }
        while ((double) elapsed < (double) duration)
        {
          elapsed += Time.deltaTime;
          if ((UnityEngine.Object) this.animator != (UnityEngine.Object) null)
            this.animator.speed = Mathf.Lerp(0.0f, 1f, elapsed / duration);
          yield return (object) null;
        }
        if ((UnityEngine.Object) this.animator != (UnityEngine.Object) null)
          this.animator.speed = 1f;
        this.charge.Stop();
        this.Shoot();
        yield return (object) new WaitForSeconds(0.2f);
        this.isCharging = false;
        while ((double) elapsed > 0.0)
        {
          elapsed -= Time.deltaTime;
          if ((UnityEngine.Object) this.animator != (UnityEngine.Object) null)
            this.animator.speed = Mathf.Lerp(0.0f, 1f, elapsed / duration);
          yield return (object) null;
        }
      }
    }

    public class Projectile : MonoBehaviour
    {
      private bool hasExploded = false;

      public void OnCollisionEnter(Collision collision)
      {
        if (this.hasExploded)
          return;
        if ((LayerMask.GetMask("Gorilla Object", "Default", "NoMirror") & 1 << collision.gameObject.layer) == 0)
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

  public class JetPack : MonoBehaviour
  {
    public static bool isActive;
    public ParticleSystem thrusterEffect;
    public AudioSource source;

    public void Update()
    {
      if (!StevesPlayground.JetPack.isActive)
      {
        StevesPlayground.currentJetPack.SetActive(false);
      }
      else
      {
        StevesPlayground.currentJetPack.SetActive(true);
        if (InputHandler.LTrigger() && InputHandler.RTrigger() || ((ButtonControl) Keyboard.current.spaceKey).isPressed)
        {
          Rigidbody rigidbody = Variables.taggerInstance.rigidbody;
          if (!((UnityEngine.Object) rigidbody != (UnityEngine.Object) null))
            return;
          this.source.pitch = (float) (0.800000011920929 + (double) Variables.taggerInstance.rigidbody.velocity.y / 20.0);
          rigidbody.AddForce(Vector3.up * 1500f, (ForceMode) 0);
          this.thrusterEffect.Play();
          Vector3 position = ((Component) this.thrusterEffect).transform.position;
          NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.JetPackThrust, new object[4]
          {
            (object) position.x,
            (object) position.y,
            (object) position.z,
            (object) this.source.pitch
          });
          Vector2 joystickAxis = InputHandler.GetJoystickAxis(false);
          if ((double) joystickAxis.magnitude <= 0.10000000149011612 && Keyboard.current != null)
          {
            if (((ButtonControl) Keyboard.current.upArrowKey).isPressed)
              ++joystickAxis.y;
            if (((ButtonControl) Keyboard.current.downArrowKey).isPressed)
              --joystickAxis.y;
            if (((ButtonControl) Keyboard.current.leftArrowKey).isPressed)
              --joystickAxis.x;
            if (((ButtonControl) Keyboard.current.rightArrowKey).isPressed)
              ++joystickAxis.x;
          }
          if (!GTPlayer.Instance.IsGroundedButt && (double) joystickAxis.magnitude > 0.10000000149011612 && (UnityEngine.Object) rigidbody != (UnityEngine.Object) null)
          {
            Vector3 forward = Variables.taggerInstance.offlineVRRig.bodyTransform.forward;
            Vector3 right = Variables.taggerInstance.offlineVRRig.bodyTransform.right;
            Vector3 normalized = (forward * joystickAxis.y + right * joystickAxis.x).normalized;
            Debug.Log((object) $"JetPack Move Direction: {normalized}");
            rigidbody.AddForce(normalized * 1500f, (ForceMode) 0);
          }
        }
        else
        {
          this.thrusterEffect.Stop();
          this.source.pitch = 0.0f;
        }
      }
    }
  }

  public class WebSling : MonoBehaviour
  {
    public static bool isActive;
    public LineRenderer webLine;
    public SpringJoint? joint;
    public bool isLeftHand;
    private Vector3 hitPoint;
    private bool isExtending;
    private bool isRecoiling;
    private float currentLength;
    private float targetLength;
    private float recoilTime;

    public void Update()
    {
      if (!StevesPlayground.WebSling.isActive)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
      else if ((UnityEngine.Object) this.webLine == (UnityEngine.Object) null)
      {
        Debug.LogError((object) "WebSling: webLine is null! Make sure the prefab has a LineRenderer.");
      }
      else
      {
        bool flag = this.isLeftHand ? InputHandler.LTrigger() : InputHandler.RTrigger();
        Transform hand = this.isLeftHand ? Variables.taggerInstance.offlineVRRig.leftHandTransform : Variables.taggerInstance.offlineVRRig.rightHandTransform;
        if (flag)
        {
          if ((UnityEngine.Object) this.joint == (UnityEngine.Object) null)
          {
            RaycastHit raycastHit;
            this.hitPoint = !Physics.Raycast(hand.position, hand.forward, out raycastHit, 50f) ? hand.position + hand.forward * 16f : (raycastHit).point;
            this.joint = ((Component) Variables.taggerInstance).gameObject.AddComponent<SpringJoint>();
            ((Joint) this.joint).autoConfigureConnectedAnchor = false;
            ((Joint) this.joint).connectedAnchor = this.hitPoint;
            float num = Vector3.Distance(Variables.taggerInstance.rigidbody.position, this.hitPoint);
            this.joint.maxDistance = num * 0.8f;
            this.joint.minDistance = num * 0.25f;
            this.joint.spring = 5000f;
            this.joint.damper = 4000f;
            ((Joint) this.joint).massScale = 5f;
            this.webLine.positionCount = 20;
            this.currentLength = 0.0f;
            this.targetLength = num;
            this.isExtending = true;
            this.isRecoiling = false;
            this.recoilTime = 0.0f;
          }
          else
          {
            this.UpdateWebLine();
            try
            {
              if ((double) OVRInput.GetLocalControllerVelocity(this.isLeftHand ? (OVRInput.Controller) 1 : (OVRInput.Controller) 2).magnitude >= 2.5)
              {
                if ((UnityEngine.Object) this.joint == (UnityEngine.Object) null)
                {
                  Debug.LogWarning((object) "WebSling Joint is null during pull attempt.");
                  return;
                }
                Vector3 vector3 = (((Joint) this.joint).connectedAnchor - ((Component) Variables.taggerInstance).transform.position).normalized * 4.5f;
                Variables.taggerInstance.rigidbody.AddForce(vector3, (ForceMode) 2);
                Debug.Log((object) "WebSling Pulling Player Towards Web Point!");
              }
            }
            catch (Exception ex)
            {
              Debug.LogError((object) ("WebSling Update Error: " + ex.Message));
            }
          }
        }
        else
        {
          if ((UnityEngine.Object) this.joint != (UnityEngine.Object) null)
          {
            UnityEngine.Object.Destroy((UnityEngine.Object) this.joint);
            this.joint = (SpringJoint) null;
          }
          this.isExtending = false;
          this.isRecoiling = false;
          this.currentLength = 0.0f;
          this.targetLength = 0.0f;
          this.webLine.positionCount = 0;
        }
        if (this.isExtending)
          this.AnimateWebExtend(hand);
        if (!this.isRecoiling)
          return;
        this.AnimateRecoil(hand);
      }
    }

    public void AnimateWebExtend(Transform hand)
    {
      this.currentLength = Mathf.MoveTowards(this.currentLength, this.targetLength, Time.deltaTime * 70f);
      Vector3 normalized = (this.hitPoint - hand.position).normalized;
      Vector3 end = hand.position + normalized * this.currentLength;
      this.DrawCurvedWeb(hand.position, end);
      if ((double) Mathf.Abs(this.currentLength - this.targetLength) >= 0.05000000074505806)
        return;
      this.isExtending = false;
      this.isRecoiling = true;
      this.recoilTime = 0.0f;
    }

    public void AnimateRecoil(Transform hand)
    {
      this.recoilTime += Time.deltaTime * 10f;
      float num = (float) ((double) Mathf.Sin(this.recoilTime) * 0.20000000298023224 * (1.0 - (double) Mathf.Clamp01(this.recoilTime / 2f)));
      Vector3 end = this.hitPoint + (this.hitPoint - hand.position).normalized * num;
      this.DrawCurvedWeb(hand.position, end);
      if ((double) this.recoilTime <= 3.1415927410125732)
        return;
      this.isRecoiling = false;
      this.DrawCurvedWeb(hand.position, this.hitPoint);
    }

    public void DrawCurvedWeb(Vector3 start, Vector3 end)
    {
      int positionCount = this.webLine.positionCount;
      for (int index = 0; index < positionCount; ++index)
      {
        float t = (float) index / (float) (positionCount - 1);
        Vector3 vector3 = Vector3.down * (Mathf.Sin(t * 3.14159274f) * 0.3f * Vector3.Distance(start, end));
        Vector3 position = Vector3.Lerp(start, end, t) + vector3;
        this.webLine.SetPosition(index, position);
      }
    }

    public void UpdateWebLine()
    {
      if ((UnityEngine.Object) this.joint == (UnityEngine.Object) null || (UnityEngine.Object) this.webLine == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null || (UnityEngine.Object) Variables.taggerInstance.offlineVRRig == (UnityEngine.Object) null)
        return;
      this.DrawCurvedWeb((this.isLeftHand ? Variables.taggerInstance.offlineVRRig.leftHandTransform : Variables.taggerInstance.offlineVRRig.rightHandTransform).position, ((Joint) this.joint).connectedAnchor);
    }
  }

  public enum FuncType
  {
    DisplacerCannon,
    Jetpack,
  }
}
