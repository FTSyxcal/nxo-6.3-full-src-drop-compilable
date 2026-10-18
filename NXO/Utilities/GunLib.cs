
using NXO.Mods.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable
namespace NXO.Utilities;

public class GunLib
{
  public static bool GunLineEnabled;
  public static bool LeftHandGun;
  public static VRRig lockedTargetRig;
  public static VRRig potentialTargetRig;
  public static GameObject gunPointer;
  public static RaycastHit raycastHit;
  private static GameObject gunLineObj;
  private static LineRenderer gunLineRenderer;
  private static Material gunLineMaterial;
  private static Renderer gunPointerRenderer;
  private static Material gunPointerMaterial;
  private static readonly GradientColorKey[] _rainbowKeys = new GradientColorKey[8];
  private static readonly GradientColorKey[] _solidKeys = new GradientColorKey[2];
  private static readonly GradientAlphaKey[] _alphaKeys = new GradientAlphaKey[2]
  {
    new GradientAlphaKey(1f, 0.0f),
    new GradientAlphaKey(1f, 1f)
  };
  private static readonly Gradient _lineGradient = new Gradient();
  private static bool raycastHasHit;
  private static Camera _thirdPersonCameraCache;
  private static readonly Vector3[][] _lineBuffers = new Vector3[8][];
  private static Color32 _lastLineColor;
  private static bool _lastLineRainbow;
  private static bool _lineColorApplied;
  private const float WaveDensity = 4f;
  private const float PointsPerCoil = 24f;
  private const int MaxLinePoints = 512 /*0x0200*/;
  private const int LinePointStep = 64 /*0x40*/;
  private const float EndTaper = 0.15f;
  private const float SpiralRadius = 0.05f;
  private const float SpiralCoilScale = 80f;
  private static bool playedLockHaptic;

  public static bool GunGrips
  {
    get
    {
      if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        return true;
      return !GunLib.LeftHandGun ? InputHandler.RGrip() : InputHandler.LGrip();
    }
  }

  public static bool GunTriggers
  {
    get
    {
      if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        return true;
      return !GunLib.LeftHandGun ? InputHandler.RTrigger() : InputHandler.LTrigger();
    }
  }

  private static bool IsCurrentRainbow()
  {
    if ((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null)
      return Settings.GunLockRainbow;
    if (GunLib.GunTriggers)
      return Settings.GunFireRainbow;
    return (UnityEngine.Object) GunLib.potentialTargetRig != (UnityEngine.Object) null ? Settings.GunHoverRainbow : Settings.GunIdleRainbow;
  }

  private static Color32 CurrentGunColor(float flow)
  {
    if (GunLib.IsCurrentRainbow())
      return (Color32) Color.HSVToRGB(Mathf.Repeat(Time.time * 0.4f + flow, 1f), 1f, 1f);
    if ((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null)
      return Settings.GunLockColor;
    if (GunLib.GunTriggers)
      return Settings.GunFireColor;
    return (UnityEngine.Object) GunLib.potentialTargetRig != (UnityEngine.Object) null ? Settings.GunHoverColor : Settings.GunIdleColor;
  }

  private static void ApplyLineColor()
  {
    if ((UnityEngine.Object) GunLib.gunLineRenderer == (UnityEngine.Object) null)
      return;
    if (GunLib.IsCurrentRainbow())
    {
      float num = Time.time * 0.4f;
      for (int index = 0; index < 8; ++index)
        GunLib._rainbowKeys[index] = new GradientColorKey(Color.HSVToRGB(Mathf.Repeat(num - (float) index / 8f, 1f), 1f, 1f), (float) index / 7f);
      GunLib._lineGradient.SetKeys(GunLib._rainbowKeys, GunLib._alphaKeys);
      GunLib.gunLineRenderer.colorGradient = GunLib._lineGradient;
      GunLib._lastLineRainbow = true;
      GunLib._lineColorApplied = true;
    }
    else
    {
      Color32 col = GunLib.CurrentGunColor(0.0f);
      if (GunLib._lineColorApplied && !GunLib._lastLineRainbow && (int) col.r == (int) GunLib._lastLineColor.r && (int) col.g == (int) GunLib._lastLineColor.g && (int) col.b == (int) GunLib._lastLineColor.b && (int) col.a == (int) GunLib._lastLineColor.a)
        return;
      GunLib._solidKeys[0] = new GradientColorKey((Color) col, 0.0f);
      GunLib._solidKeys[1] = new GradientColorKey((Color) col, 1f);
      GunLib._lineGradient.SetKeys(GunLib._solidKeys, GunLib._alphaKeys);
      GunLib.gunLineRenderer.colorGradient = GunLib._lineGradient;
      GunLib._lastLineColor = col;
      GunLib._lastLineRainbow = false;
      GunLib._lineColorApplied = true;
    }
  }

  private static Vector3 LineOffset(
    string style,
    float p,
    float f,
    float amp,
    Vector3 right,
    Vector3 up,
    float t,
    float spd)
  {
    switch (style)
    {
      case "Spiral":
        return right * (Mathf.Cos(p) * amp) - up * (Mathf.Sin(p) * amp);
      case "Electric":
        float num1 = Mathf.PerlinNoise(f * 15f, (float) ((double) t * (double) spd * 0.30000001192092896)) - 0.5f;
        float num2 = Mathf.PerlinNoise((float) ((double) f * 15.0 + 50.0), (float) ((double) t * (double) spd * 0.30000001192092896)) - 0.5f;
        return (right * num1 + up * num2) * (amp * 3f);
      default:
        return right * (Mathf.Sin(p) * amp);
    }
  }

  private static Transform GunHand
  {
    get
    {
      if ((UnityEngine.Object) GorillaTagger.Instance != (UnityEngine.Object) null)
        return GunLib.LeftHandGun ? GorillaTagger.Instance.leftHandTransform : GorillaTagger.Instance.rightHandTransform;
      if ((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null)
        return GunLib.LeftHandGun ? Variables.taggerInstance.leftHandTransform : Variables.taggerInstance.rightHandTransform;
      return (UnityEngine.Object) Variables.playerInstance != (UnityEngine.Object) null ? (GunLib.LeftHandGun ? Variables.playerInstance.LeftHand.controllerTransform : Variables.playerInstance.RightHand.controllerTransform) : (Transform) null;
    }
  }

  public static void SetupRaycast()
  {
    bool flag = Mouse.current != null && (Mouse.current.rightButton.isPressed || Mouse.current.leftButton.isPressed);
    if ((UnityEngine.Object) GunLib._thirdPersonCameraCache == (UnityEngine.Object) null && (UnityEngine.Object) Variables.thirdPersonCamera != (UnityEngine.Object) null)
      GunLib._thirdPersonCameraCache = Variables.thirdPersonCamera.GetComponent<Camera>();
    Transform gunHand = GunLib.GunHand;
    if (!flag && (UnityEngine.Object) gunHand == (UnityEngine.Object) null)
    {
      GunLib.SetGunVisibility(false);
    }
    else
    {
      Ray ray = !flag || !((UnityEngine.Object) GunLib._thirdPersonCameraCache != (UnityEngine.Object) null) ? new Ray(gunHand.position - gunHand.up, -gunHand.up) : GunLib._thirdPersonCameraCache.ScreenPointToRay((Vector3) ((InputControl<Vector2>) ((Pointer) Mouse.current).position).ReadValue());
      GunLib.raycastHasHit = false;
      GunLib.potentialTargetRig = (VRRig) null;
      Vector3 pos = ray.origin + ray.direction * 100f;
      GunLib.raycastHit = new RaycastHit();
      (GunLib.raycastHit).point = pos;
      using (IEnumerator<RaycastHit> enumerator = ((IEnumerable<RaycastHit>) Physics.RaycastAll(ray, 100f, Variables.NoInvisLayers())).OrderBy<RaycastHit, float>((Func<RaycastHit, float>) (x => (x).distance)).GetEnumerator())
      {
        if (enumerator.MoveNext())
        {
          RaycastHit current = enumerator.Current;
          VRRig componentInParent = (UnityEngine.Object) (current).collider != (UnityEngine.Object) null ? ((Component) (current).collider).GetComponentInParent<VRRig>() : (VRRig) null;
          GunLib.potentialTargetRig = !((UnityEngine.Object) componentInParent != (UnityEngine.Object) null) || !((UnityEngine.Object) componentInParent != (UnityEngine.Object) VRRig.LocalRig) || !((UnityEngine.Object) Variables.taggerInstance == (UnityEngine.Object) null) && !((UnityEngine.Object) componentInParent != (UnityEngine.Object) Variables.taggerInstance.offlineVRRig) || componentInParent.Creator == null ? (VRRig) null : componentInParent;
          GunLib.raycastHit = current;
          GunLib.raycastHasHit = true;
          pos = (current).point;
        }
      }
      if ((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null)
      {
        pos = ((Component) GunLib.lockedTargetRig).transform.position;
        (GunLib.raycastHit).point = pos;
      }
      GunLib.SetupGunObjectPositions(pos);
    }
  }

  public static void SetupGunObjectPositions(Vector3 pos)
  {
    if ((UnityEngine.Object) GunLib.gunPointer == (UnityEngine.Object) null)
    {
      GunLib.gunPointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      Rigidbody component1 = GunLib.gunPointer.GetComponent<Rigidbody>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component1);
      SphereCollider component2 = GunLib.gunPointer.GetComponent<SphereCollider>();
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component2);
      GunLib.gunPointerRenderer = GunLib.gunPointer.GetComponent<Renderer>();
      GunLib.gunPointerMaterial = new Material(Variables.guiShader);
      if ((UnityEngine.Object) GunLib.gunPointerRenderer != (UnityEngine.Object) null)
        GunLib.gunPointerRenderer.sharedMaterial = GunLib.gunPointerMaterial;
    }
    GunLib.gunPointer.transform.localScale = Vector3.one * Settings.GunPointerSize;
    Color32 color32 = GunLib.CurrentGunColor(0.0f);
    GunLib.gunPointer.transform.position = pos;
    if ((UnityEngine.Object) GunLib.gunPointerMaterial != (UnityEngine.Object) null)
      GunLib.gunPointerMaterial.color = (Color) color32;
    if (GunLib.GunLineEnabled)
    {
      Transform gunHand = GunLib.GunHand;
      if ((UnityEngine.Object) gunHand == (UnityEngine.Object) null)
        return;
      if ((UnityEngine.Object) GunLib.gunLineObj == (UnityEngine.Object) null)
      {
        GunLib.gunLineObj = new GameObject("GunLine");
        GunLib.gunLineRenderer = GunLib.gunLineObj.AddComponent<LineRenderer>();
        GunLib.gunLineMaterial = new Material(Shader.Find("UI/Default") ?? Shader.Find("Sprites/Default"))
        {
          color = Color.white
        };
        GunLib.gunLineMaterial.mainTexture = (Texture) Texture2D.whiteTexture;
        GunLib.gunLineMaterial.SetInt("unity_GUIZTestMode", 8);
        GunLib.gunLineMaterial.renderQueue = 4000;
        GunLib.gunLineRenderer.material = GunLib.gunLineMaterial;
        GunLib.gunLineRenderer.useWorldSpace = true;
        GunLib.gunLineRenderer.numCapVertices = 4;
        GunLib.gunLineRenderer.numCornerVertices = 4;
        GunLib.gunLineRenderer.alignment = LineAlignment.View;
        GunLib._lineColorApplied = false;
      }
      Vector3 position = gunHand.position;
      string gunAnimationType = Settings.GunAnimationType;
      float time = Time.time;
      float gunWiggleSpeed = Settings.GunWiggleSpeed;
      float gunLineThickness = Settings.GunLineThickness;
      if (gunAnimationType == "Pulse")
        gunLineThickness *= (float) (1.0 + 0.60000002384185791 * (double) Mathf.Abs(Mathf.Sin(time * gunWiggleSpeed)));
      GunLib.gunLineRenderer.startWidth = gunLineThickness;
      GunLib.gunLineRenderer.endWidth = gunLineThickness;
      if (gunAnimationType != "None" && gunAnimationType != "Pulse")
      {
        bool flag = gunAnimationType == "Spiral";
        float amp = flag ? 0.05f : Settings.GunWiggleIntensity;
        float num1 = flag ? Settings.GunWiggleIntensity * 80f : 4f;
        Vector3 normalized1 = (pos - position).normalized;
        Vector3 vector3_1 = Vector3.Cross(normalized1, Vector3.up);
        if ((double) vector3_1.sqrMagnitude < 9.9999997473787516E-05)
          vector3_1 = Vector3.Cross(normalized1, Vector3.forward);
        vector3_1.Normalize();
        Vector3 normalized2 = Vector3.Cross(normalized1, vector3_1).normalized;
        float num2 = Vector3.Distance(position, pos) * num1;
        float num3 = (float) ((double) num2 * 3.1415927410125732 * 2.0);
        int length = Mathf.Clamp((Mathf.CeilToInt(num2 * 24f) + 64 /*0x40*/ - 1) / 64 /*0x40*/ * 64 /*0x40*/, 64 /*0x40*/, 512 /*0x0200*/);
        int index1 = length / 64 /*0x40*/ - 1;
        Vector3[] positions = GunLib._lineBuffers[index1] ?? (GunLib._lineBuffers[index1] = new Vector3[length]);
        for (int index2 = 0; index2 < length; ++index2)
        {
          float num4 = (float) index2 / (float) (length - 1);
          float p = (float) ((double) time * (double) gunWiggleSpeed + (double) num4 * (double) num3);
          float num5 = Mathf.Clamp01((float) ((1.0 - (double) num4) / 0.15000000596046448));
          Vector3 vector3_2 = GunLib.LineOffset(gunAnimationType, p, num4, amp, vector3_1, normalized2, time, gunWiggleSpeed);
          positions[index2] = Vector3.Lerp(position, pos, num4) + vector3_2 * num5;
        }
        if (GunLib.gunLineRenderer.positionCount != length)
          GunLib.gunLineRenderer.positionCount = length;
        GunLib.gunLineRenderer.SetPositions(positions);
      }
      else
      {
        if (GunLib.gunLineRenderer.positionCount != 2)
          GunLib.gunLineRenderer.positionCount = 2;
        GunLib.gunLineRenderer.SetPosition(0, position);
        GunLib.gunLineRenderer.SetPosition(1, pos);
      }
      GunLib.ApplyLineColor();
      if (!GunLib.gunLineObj.activeSelf)
        GunLib.gunLineObj.SetActive(true);
    }
    else if ((UnityEngine.Object) GunLib.gunLineObj != (UnityEngine.Object) null && GunLib.gunLineObj.activeSelf)
      GunLib.gunLineObj.SetActive(false);
    GunLib.SetGunVisibility(true);
  }

  public static void SetGunVisibility(bool isVisible)
  {
    if ((UnityEngine.Object) GunLib.gunPointer != (UnityEngine.Object) null)
      GunLib.gunPointer.SetActive(isVisible);
    if (!((UnityEngine.Object) GunLib.gunLineObj != (UnityEngine.Object) null))
      return;
    GunLib.gunLineObj.SetActive(isVisible && GunLib.GunLineEnabled);
  }

  public static bool SetupGun()
  {
    if (!GunLib.GunGrips)
    {
      GunLib.SetGunVisibility(false);
      return false;
    }
    GunLib.SetupRaycast();
    return GunLib.GunTriggers;
  }

  public static bool SetupLockOnGun()
  {
    if (!GunLib.GunGrips)
    {
      GunLib.lockedTargetRig = (VRRig) null;
      GunLib.potentialTargetRig = (VRRig) null;
      GunLib.SetGunVisibility(false);
      return false;
    }
    if ((UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null && GunLib.lockedTargetRig.Creator == null)
      GunLib.lockedTargetRig = (VRRig) null;
    GunLib.SetupRaycast();
    if (GunLib.GunTriggers)
    {
      if ((UnityEngine.Object) GunLib.lockedTargetRig == (UnityEngine.Object) null && GunLib.raycastHasHit && (UnityEngine.Object) GunLib.potentialTargetRig != (UnityEngine.Object) null)
      {
        GunLib.lockedTargetRig = GunLib.potentialTargetRig;
        if (!GunLib.playedLockHaptic)
        {
          Variables.Vibrate(GunLib.LeftHandGun, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
          GunLib.playedLockHaptic = true;
        }
      }
      return (UnityEngine.Object) GunLib.lockedTargetRig != (UnityEngine.Object) null;
    }
    GunLib.lockedTargetRig = (VRRig) null;
    GunLib.playedLockHaptic = false;
    return false;
  }
}
