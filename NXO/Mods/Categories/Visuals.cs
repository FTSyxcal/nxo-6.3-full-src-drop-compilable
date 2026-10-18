
using GorillaLocomotion;
using GorillaTag.Rendering;
using NXO.Menu;
using NXO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Visuals
{
  private static readonly List<VRRig> fixedMeshRigs = new List<VRRig>();
  private static Visuals.ESPData tracerData = new Visuals.ESPData();
  private static Visuals.ESPData wireframeData = new Visuals.ESPData();
  private static Visuals.ESPData twoDBoxData = new Visuals.ESPData();
  private static Visuals.ESPData boneData = new Visuals.ESPData();
  private static Visuals.ESPData beaconData = new Visuals.ESPData();
  private static Visuals.ESPData trailData = new Visuals.ESPData();
  private static Dictionary<VRRig, GameObject> nametags = new Dictionary<VRRig, GameObject>();
  private static Dictionary<VRRig, GameObject> nxoNametags = new Dictionary<VRRig, GameObject>();
  private static bool nametagsActive = false;
  private static bool nxoNametagsActive = false;
  private static readonly int[][] edgeIndices = new int[12][]
  {
    new int[2]{ 0, 1 },
    new int[2]{ 1, 2 },
    new int[2]{ 2, 3 },
    new int[2]{ 3, 0 },
    new int[2]{ 4, 5 },
    new int[2]{ 5, 6 },
    new int[2]{ 6, 7 },
    new int[2]{ 7, 4 },
    new int[2]{ 0, 4 },
    new int[2]{ 1, 5 },
    new int[2]{ 2, 6 },
    new int[2]{ 3, 7 }
  };
  public static int[] bones = new int[38]
  {
    4,
    3,
    5,
    4,
    19,
    18,
    20,
    19,
    3,
    18,
    21,
    20,
    22,
    21,
    25,
    21,
    29,
    21,
    31 /*0x1F*/,
    29,
    27,
    25,
    24,
    22,
    6,
    5,
    7,
    6,
    10,
    6,
    14,
    6,
    16 /*0x10*/,
    14,
    12,
    10,
    9,
    7
  };
  public static int TimeType = 0;
  public static bool lastState = false;
  private static bool acidApplied = false;
  private static readonly Dictionary<Renderer, Material> _acidOriginalMaterials = new Dictionary<Renderer, Material>();
  private static Visuals.ESPData predictionData = new Visuals.ESPData();
  private static Dictionary<VRRig, Vector3> lastPlayerPositions = new Dictionary<VRRig, Vector3>();
  private static Dictionary<VRRig, Vector3> smoothedVelocities = new Dictionary<VRRig, Vector3>();
  private static Visuals.ESPData filledBoxData = new Visuals.ESPData();
  private static Visuals.ESPData filledBox2DData = new Visuals.ESPData();
  private static Dictionary<VRRig, Material> originalRigMaterials = new Dictionary<VRRig, Material>();
  private static bool trippyMonkesApplied = false;
  public static GameObject nxoCamera;
  private static Vector3 CameraVelocity;
  private static bool _camLast = false;
  private static float lastFrame;
  private static readonly List<Renderer> hiddenRenderers = new List<Renderer>();
  private static List<GameObject> monkeSenseVignette = new List<GameObject>();
  private static AudioSource monkeSenseAudio = (AudioSource) null;
  private static float monkeSenseBeepCooldown = 0.0f;
  private const float SENSE_FAR = 15f;
  private const float SENSE_CLOSE = 8f;
  private const float SENSE_DANGER = 3f;
  public static bool muteMonkeSense = false;

  private static Color32 GetTeamColor(bool isInfected, bool teamChecked, VRRig rig)
  {
    if (teamChecked)
      return isInfected ? new Color32(byte.MaxValue, (byte) 0, (byte) 0, (byte) 155) : new Color32((byte) 0, byte.MaxValue, (byte) 0, (byte) 155);
    if (isInfected)
      return new Color32(byte.MaxValue, (byte) 0, (byte) 0, (byte) 155);
    Color playerColor = rig.playerColor;
    return new Color32((byte) ((double) playerColor.r * (double) byte.MaxValue), (byte) ((double) playerColor.g * (double) byte.MaxValue), (byte) ((double) playerColor.b * (double) byte.MaxValue), (byte) 155);
  }

  public static void FixRigColors(VRRig rig)
  {
    if ((UnityEngine.Object) rig == (UnityEngine.Object) null || (UnityEngine.Object) rig.mainSkin == (UnityEngine.Object) null || (UnityEngine.Object) rig.mainSkin.sharedMesh == (UnityEngine.Object) null || Visuals.fixedMeshRigs.Contains(rig))
      return;
    Visuals.fixedMeshRigs.Add(rig);
    if (rig.mainSkin.sharedMesh.colors32 != null && rig.mainSkin.sharedMesh.colors32.Length != 0)
    {
      Color32[] color32Array = new Color32[rig.mainSkin.sharedMesh.colors32.Length];
      for (int index = 0; index < color32Array.Length; ++index)
        color32Array[index] = (Color32) Color.white;
      rig.mainSkin.sharedMesh.colors32 = color32Array;
    }
    if (rig.mainSkin.sharedMesh.colors == null || rig.mainSkin.sharedMesh.colors.Length == 0)
      return;
    Color[] colorArray = new Color[rig.mainSkin.sharedMesh.colors.Length];
    for (int index = 0; index < colorArray.Length; ++index)
      colorArray[index] = Color.white;
    rig.mainSkin.sharedMesh.colors = colorArray;
  }

  private static Material EnsureMaterial(ref Material material, Shader shader)
  {
    if ((UnityEngine.Object) material == (UnityEngine.Object) null || (UnityEngine.Object) material.shader != (UnityEngine.Object) shader)
    {
      if ((UnityEngine.Object) material != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) material);
      material = new Material(shader);
    }
    return material;
  }

  private static Vector3[] CalculateCorners(Vector3 center, Vector3 halfSize, Quaternion rotation)
  {
    Vector3[] vector3Array = new Vector3[8]
    {
      new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
      new Vector3(halfSize.x, -halfSize.y, -halfSize.z),
      new Vector3(halfSize.x, -halfSize.y, halfSize.z),
      new Vector3(-halfSize.x, -halfSize.y, halfSize.z),
      new Vector3(-halfSize.x, halfSize.y, -halfSize.z),
      new Vector3(halfSize.x, halfSize.y, -halfSize.z),
      new Vector3(halfSize.x, halfSize.y, halfSize.z),
      new Vector3(-halfSize.x, halfSize.y, halfSize.z)
    };
    Vector3[] corners = new Vector3[8];
    for (int index = 0; index < 8; ++index)
      corners[index] = center + rotation * vector3Array[index];
    return corners;
  }

  private static void CleanupInactiveRigs(Visuals.ESPData data, List<VRRig> activeRigs)
  {
    List<VRRig> vrRigList = new List<VRRig>();
    foreach (VRRig key in data.singleObjects.Keys)
    {
      if ((UnityEngine.Object) key == (UnityEngine.Object) null || !activeRigs.Contains(key) || key.isOfflineVRRig)
        vrRigList.Add(key);
    }
    foreach (VRRig key in data.multiObjects.Keys)
    {
      if ((UnityEngine.Object) key == (UnityEngine.Object) null || !activeRigs.Contains(key) || key.isOfflineVRRig)
        vrRigList.Add(key);
    }
    foreach (VRRig key in vrRigList)
    {
      GameObject gameObject1;
      if (data.singleObjects.TryGetValue(key, out gameObject1))
      {
        if ((UnityEngine.Object) gameObject1 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject1);
        data.singleObjects.Remove(key);
      }
      List<GameObject> gameObjectList;
      if (data.multiObjects.TryGetValue(key, out gameObjectList))
      {
        foreach (GameObject gameObject2 in gameObjectList)
        {
          if ((UnityEngine.Object) gameObject2 != (UnityEngine.Object) null)
            UnityEngine.Object.Destroy((UnityEngine.Object) gameObject2);
        }
        data.multiObjects.Remove(key);
      }
    }
  }

  public static void TracersESP(bool enable)
  {
    if (!enable)
    {
      Visuals.tracerData.Clear();
    }
    else
    {
      Visuals.tracerData.material = Visuals.EnsureMaterial(ref Visuals.tracerData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.tracerData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isMyPlayer && !vrRig.isOfflineVRRig)
        {
          GameObject gameObject;
          if (!Visuals.tracerData.singleObjects.TryGetValue(vrRig, out gameObject) || (UnityEngine.Object) gameObject == (UnityEngine.Object) null)
          {
            gameObject = new GameObject("Tracer");
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineRenderer.endWidth = 0.015f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = Visuals.tracerData.material;
            Visuals.tracerData.singleObjects[vrRig] = gameObject;
          }
          LineRenderer component = gameObject.GetComponent<LineRenderer>();
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          component.startColor = component.endColor = (Color) teamColor;
          int tracerPositionIndex = Settings.tracerPositionIndex;
          if (true)
            ;
          Vector3 vector3;
          switch (tracerPositionIndex)
          {
            case 1:
              vector3 = Variables.taggerInstance.leftHandTransform.position;
              break;
            case 2:
              vector3 = ((Component) Variables.taggerInstance.headCollider).transform.position + new Vector3(0.0f, 0.4f, 0.0f);
              break;
            case 3:
              vector3 = ((Component) Variables.taggerInstance.bodyCollider).transform.position + new Vector3(0.0f, -0.3f, 0.0f);
              break;
            default:
              vector3 = Variables.taggerInstance.rightHandTransform.position;
              break;
          }
          if (true)
            ;
          Vector3 position = vector3;
          component.SetPosition(0, position);
          component.SetPosition(1, ((Component) vrRig).transform.position);
        }
      }
    }
  }

  public static void PredictionESP(bool enable)
  {
    if (!enable)
    {
      Visuals.predictionData.Clear();
      Visuals.lastPlayerPositions.Clear();
      Visuals.smoothedVelocities.Clear();
    }
    else
    {
      Visuals.predictionData.material = Visuals.EnsureMaterial(ref Visuals.predictionData.material, Variables.guiShader);
      List<VRRig> activeRigs = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.predictionData, activeRigs);
      foreach (VRRig key in Visuals.lastPlayerPositions.Keys.Where<VRRig>((Func<VRRig, bool>) (rig => (UnityEngine.Object) rig == (UnityEngine.Object) null || !activeRigs.Contains(rig))).ToList<VRRig>())
      {
        Visuals.lastPlayerPositions.Remove(key);
        Visuals.smoothedVelocities.Remove(key);
      }
      foreach (VRRig vrRig in activeRigs)
      {
        if (!vrRig.isMyPlayer && !vrRig.isOfflineVRRig)
        {
          Vector3 position1 = ((Component) vrRig).transform.position;
          Vector3 vector3_1 = Vector3.zero;
          Vector3 vector3_2;
          if (Visuals.lastPlayerPositions.TryGetValue(vrRig, out vector3_2))
          {
            Vector3 b = (position1 - vector3_2) / Time.deltaTime;
            if ((double) b.magnitude > 30.0)
              b = b.normalized * 30f;
            Vector3 a;
            vector3_1 = !Visuals.smoothedVelocities.TryGetValue(vrRig, out a) ? b : Vector3.Lerp(a, b, 0.3f);
            Visuals.smoothedVelocities[vrRig] = vector3_1;
          }
          Visuals.lastPlayerPositions[vrRig] = position1;
          if ((double) vector3_1.magnitude >= 0.30000001192092896)
          {
            GameObject gameObject;
            if (!Visuals.predictionData.singleObjects.TryGetValue(vrRig, out gameObject) || (UnityEngine.Object) gameObject == (UnityEngine.Object) null)
            {
              gameObject = new GameObject("PredictionLine");
              LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
              lineRenderer.positionCount = 2;
              lineRenderer.startWidth = 0.02f;
              lineRenderer.endWidth = 0.08f;
              lineRenderer.useWorldSpace = true;
              lineRenderer.material = Visuals.predictionData.material;
              Visuals.predictionData.singleObjects[vrRig] = gameObject;
            }
            LineRenderer component = gameObject.GetComponent<LineRenderer>();
            Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
            component.startColor = new Color((float) teamColor.r / (float) byte.MaxValue, (float) teamColor.g / (float) byte.MaxValue, (float) teamColor.b / (float) byte.MaxValue, 0.3f);
            component.endColor = new Color((float) teamColor.r / (float) byte.MaxValue, (float) teamColor.g / (float) byte.MaxValue, (float) teamColor.b / (float) byte.MaxValue, 0.8f);
            float num = 0.25f;
            Vector3 position2 = position1 + vector3_1 * num;
            if ((double) vector3_1.y > 0.0)
              position2 += Physics.gravity * (float) ((double) num * (double) num * 0.5);
            component.SetPosition(0, position1);
            component.SetPosition(1, position2);
          }
        }
      }
    }
  }

  public static void ESP3D(bool enable)
  {
    if (!enable)
    {
      Visuals.wireframeData.Clear();
    }
    else
    {
      Visuals.wireframeData.material = Visuals.EnsureMaterial(ref Visuals.wireframeData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.wireframeData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig)
        {
          List<GameObject> gameObjectList;
          if (!Visuals.wireframeData.multiObjects.TryGetValue(vrRig, out gameObjectList))
          {
            gameObjectList = new List<GameObject>();
            foreach (int[] edgeIndex in Visuals.edgeIndices)
            {
              GameObject gameObject = new GameObject("WireframeLine");
              LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
              lineRenderer.positionCount = 2;
              lineRenderer.startWidth = lineRenderer.endWidth = 0.06f;
              lineRenderer.useWorldSpace = true;
              lineRenderer.material = Visuals.wireframeData.material;
              gameObjectList.Add(gameObject);
            }
            Visuals.wireframeData.multiObjects[vrRig] = gameObjectList;
          }
          Vector3 halfSize = new Vector3(((Component) vrRig).transform.localScale.x * 0.375f, ((Component) vrRig).transform.localScale.y * 0.525f, ((Component) vrRig).transform.localScale.z * 0.375f);
          Vector3[] corners = Visuals.CalculateCorners(((Component) vrRig).transform.position - new Vector3(0.0f, 0.075f, 0.0f), halfSize, ((Component) vrRig).transform.rotation);
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          for (int index = 0; index < gameObjectList.Count; ++index)
          {
            LineRenderer component = gameObjectList[index].GetComponent<LineRenderer>();
            if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
            {
              component.startColor = component.endColor = (Color) teamColor;
              component.SetPositions(new Vector3[2]
              {
                corners[Visuals.edgeIndices[index][0]],
                corners[Visuals.edgeIndices[index][1]]
              });
            }
          }
        }
      }
    }
  }

  public static void ESP2D(bool enable)
  {
    if (!enable)
    {
      Visuals.twoDBoxData.Clear();
    }
    else
    {
      Visuals.twoDBoxData.material = Visuals.EnsureMaterial(ref Visuals.twoDBoxData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.twoDBoxData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig)
        {
          List<GameObject> gameObjectList;
          if (!Visuals.twoDBoxData.multiObjects.TryGetValue(vrRig, out gameObjectList))
          {
            gameObjectList = new List<GameObject>();
            for (int index = 0; index < 4; ++index)
            {
              GameObject gameObject = new GameObject("2DBoxLine");
              LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
              lineRenderer.positionCount = 2;
              lineRenderer.startWidth = lineRenderer.endWidth = 0.07f;
              lineRenderer.useWorldSpace = true;
              lineRenderer.material = Visuals.twoDBoxData.material;
              gameObjectList.Add(gameObject);
            }
            Visuals.twoDBoxData.multiObjects[vrRig] = gameObjectList;
          }
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          Vector3 position = ((Component) vrRig).transform.position;
          Quaternion quaternion = Quaternion.LookRotation(position - Camera.main.transform.position);
          float num = 0.03f;
          Vector3[] vector3Array = new Vector3[4]
          {
            position + quaternion * new Vector3(-0.5f - num, 0.5f + num, 0.0f),
            position + quaternion * new Vector3(0.5f + num, 0.5f + num, 0.0f),
            position + quaternion * new Vector3(0.5f + num, -0.5f - num, 0.0f),
            position + quaternion * new Vector3(-0.5f - num, -0.5f - num, 0.0f)
          };
          for (int index = 0; index < gameObjectList.Count; ++index)
          {
            LineRenderer component = gameObjectList[index].GetComponent<LineRenderer>();
            if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
            {
              component.startColor = component.endColor = (Color) teamColor;
              Vector3 normalized = (vector3Array[(index + 1) % 4] - vector3Array[index]).normalized;
              component.SetPositions(new Vector3[2]
              {
                vector3Array[index] - normalized * num,
                vector3Array[(index + 1) % 4] + normalized * num
              });
            }
          }
        }
      }
    }
  }

  public static void FilledBoxESP(bool enable)
  {
    if (!enable)
    {
      Visuals.filledBoxData.Clear();
    }
    else
    {
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.filledBoxData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig)
        {
          GameObject primitive;
          if (!Visuals.filledBoxData.singleObjects.TryGetValue(vrRig, out primitive) || (UnityEngine.Object) primitive == (UnityEngine.Object) null)
          {
            primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.name = "FilledBox";
            UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<Collider>());
            Material material = new Material(Variables.guiShader);
            primitive.GetComponent<Renderer>().material = material;
            Visuals.filledBoxData.singleObjects[vrRig] = primitive;
          }
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          primitive.GetComponent<Renderer>().material.color = (Color) new Color32(teamColor.r, teamColor.g, teamColor.b, (byte) 80 /*0x50*/);
          primitive.transform.position = ((Component) vrRig).transform.position - new Vector3(0.0f, 0.075f, 0.0f);
          primitive.transform.rotation = ((Component) vrRig).transform.rotation;
          primitive.transform.localScale = new Vector3(((Component) vrRig).transform.localScale.x * 0.75f, ((Component) vrRig).transform.localScale.y * 1.05f, ((Component) vrRig).transform.localScale.z * 0.75f);
        }
      }
    }
  }

  public static void FilledBoxESP2D(bool enable)
  {
    if (!enable)
    {
      Visuals.filledBox2DData.Clear();
    }
    else
    {
      Visuals.filledBox2DData.material = Visuals.EnsureMaterial(ref Visuals.filledBox2DData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.filledBox2DData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig)
        {
          GameObject primitive;
          if (!Visuals.filledBox2DData.singleObjects.TryGetValue(vrRig, out primitive) || (UnityEngine.Object) primitive == (UnityEngine.Object) null)
          {
            primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            primitive.name = "FilledBox2D";
            UnityEngine.Object.Destroy((UnityEngine.Object) primitive.GetComponent<Collider>());
            Material material = new Material(Variables.guiShader);
            primitive.GetComponent<Renderer>().material = material;
            Visuals.filledBox2DData.singleObjects[vrRig] = primitive;
          }
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          primitive.GetComponent<Renderer>().material.color = (Color) new Color32(teamColor.r, teamColor.g, teamColor.b, (byte) 80 /*0x50*/);
          Vector3 position = ((Component) vrRig).transform.position;
          primitive.transform.position = position;
          primitive.transform.rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
          primitive.transform.localScale = new Vector3(1.06f, 1.06f, 1f);
        }
      }
    }
  }

  public static void BoneESP(bool enable)
  {
    if (!enable)
    {
      Visuals.boneData.Clear();
    }
    else
    {
      Visuals.boneData.material = Visuals.EnsureMaterial(ref Visuals.boneData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.boneData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig)
        {
          List<GameObject> gameObjectList;
          if (!Visuals.boneData.multiObjects.TryGetValue(vrRig, out gameObjectList))
          {
            gameObjectList = new List<GameObject>();
            for (int index = 0; index <= Visuals.bones.Length / 2; ++index)
            {
              GameObject gameObject = new GameObject("BoneLine");
              LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
              lineRenderer.positionCount = 2;
              lineRenderer.startWidth = lineRenderer.endWidth = 0.04f;
              lineRenderer.material = Visuals.boneData.material;
              gameObjectList.Add(gameObject);
            }
            Visuals.boneData.multiObjects[vrRig] = gameObjectList;
          }
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
          LineRenderer component1 = gameObjectList[0].GetComponent<LineRenderer>();
          component1.startColor = component1.endColor = (Color) teamColor;
          component1.SetPositions(new Vector3[2]
          {
            vrRig.head.rigTarget.transform.position + new Vector3(0.0f, 0.16f, 0.0f),
            vrRig.head.rigTarget.transform.position - new Vector3(0.0f, 0.4f, 0.0f)
          });
          for (int index = 0; index < Visuals.bones.Length; index += 2)
          {
            LineRenderer component2 = gameObjectList[1 + index / 2].GetComponent<LineRenderer>();
            component2.startColor = component2.endColor = (Color) teamColor;
            component2.SetPositions(new Vector3[2]
            {
              vrRig.mainSkin.bones[Visuals.bones[index]].position,
              vrRig.mainSkin.bones[Visuals.bones[index + 1]].position
            });
          }
        }
      }
    }
  }

  public static void SkeletonESP(bool enable)
  {
    if (enable)
    {
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
        {
          Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(activeRig), Variables.teamCheckedESP, activeRig);
          ((SyncToPlayerColor) activeRig.skeleton).UpdateColor(activeRig.playerColor);
          activeRig.skeleton.renderer.sharedMaterial.shader = Shader.Find("GUI/Text Shader");
          activeRig.skeleton.renderer.sharedMaterial.color = (Color) teamColor;
          ((Behaviour) activeRig.skeleton).enabled = true;
          activeRig.skeleton.renderer.enabled = true;
        }
      }
    }
    else
    {
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
        {
          ((Behaviour) activeRig.skeleton).enabled = false;
          activeRig.skeleton.renderer.enabled = false;
        }
      }
    }
  }

  public static void BeaconsESP(bool enable)
  {
    if (!enable)
    {
      Visuals.beaconData.Clear();
    }
    else
    {
      Visuals.beaconData.material = Visuals.EnsureMaterial(ref Visuals.beaconData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.beaconData, list);
      foreach (VRRig vrRig in list)
      {
        if (!vrRig.isOfflineVRRig && !vrRig.isMyPlayer)
        {
          GameObject gameObject;
          if (!Visuals.beaconData.singleObjects.TryGetValue(vrRig, out gameObject) || (UnityEngine.Object) gameObject == (UnityEngine.Object) null)
          {
            gameObject = new GameObject("Beacon");
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineRenderer.endWidth = 0.15f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = Visuals.beaconData.material;
            Visuals.beaconData.singleObjects[vrRig] = gameObject;
          }
          LineRenderer component = gameObject.GetComponent<LineRenderer>();
          if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
          {
            Vector3 position = ((Component) vrRig).transform.position;
            Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
            component.startColor = component.endColor = (Color) teamColor;
            component.SetPositions(new Vector3[2]
            {
              position + Vector3.down * 50f,
              position + Vector3.up * 50f
            });
          }
        }
      }
    }
  }

  public static void ChamsESP(bool enable)
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig.mainSkin?.material == (UnityEngine.Object) null))
      {
        Material material = activeRig.mainSkin.material;
        if (enable)
        {
          Visuals.FixRigColors(activeRig);
          material.shader = Variables.guiShader;
          material.color = (Color) Visuals.GetTeamColor(RigManager.RigIsInfected(activeRig), Variables.teamCheckedESP, activeRig);
        }
        else if ((UnityEngine.Object) material.shader == (UnityEngine.Object) Variables.guiShader)
          material.shader = Variables.uberShader;
      }
    }
  }

  public static void TrailsESP(bool enable)
  {
    if (!enable)
    {
      Visuals.trailData.Clear();
    }
    else
    {
      Visuals.trailData.material = Visuals.EnsureMaterial(ref Visuals.trailData.material, Variables.guiShader);
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      Visuals.CleanupInactiveRigs(Visuals.trailData, list);
      foreach (VRRig vrRig in list)
      {
        if (!((UnityEngine.Object) vrRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
        {
          GameObject gameObject;
          if (!Visuals.trailData.singleObjects.TryGetValue(vrRig, out gameObject) || (UnityEngine.Object) gameObject == (UnityEngine.Object) null)
          {
            gameObject = new GameObject("Trail");
            gameObject.transform.position = ((Component) vrRig).transform.position;
            gameObject.transform.SetParent(((Component) vrRig).transform);
            TrailRenderer trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 2f;
            trailRenderer.startWidth = 0.2f;
            trailRenderer.endWidth = 0.0f;
            trailRenderer.material = Visuals.trailData.material;
            Visuals.trailData.singleObjects[vrRig] = gameObject;
          }
          gameObject.transform.position = ((Component) vrRig).transform.position;
          TrailRenderer component = gameObject.GetComponent<TrailRenderer>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            Color32 teamColor = Visuals.GetTeamColor(RigManager.RigIsInfected(vrRig), Variables.teamCheckedESP, vrRig);
            component.startColor = component.endColor = (Color) teamColor;
          }
        }
      }
    }
  }

  public static void FPSBoost(bool enable)
  {
    if (enable == Visuals.lastState)
      return;
    Visuals.lastState = enable;
    if (enable)
    {
      Screen.SetResolution(NXOUI.originalWidth / 6, NXOUI.originalHeight / 6, true);
      QualitySettings.SetQualityLevel(0, true);
      QualitySettings.pixelLightCount = 0;
      QualitySettings.shadows = ShadowQuality.Disable;
      QualitySettings.realtimeReflectionProbes = false;
      QualitySettings.softParticles = false;
      QualitySettings.lodBias = 0.5f;
      QualitySettings.antiAliasing = 0;
      QualitySettings.vSyncCount = 0;
      QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
      QualitySettings.globalTextureMipmapLimit = 2;
      QualitySettings.skinWeights = SkinWeights.OneBone;
    }
    else
    {
      Screen.SetResolution(NXOUI.originalWidth, NXOUI.originalHeight, true);
      QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
      QualitySettings.pixelLightCount = 4;
      QualitySettings.shadows = ShadowQuality.All;
      QualitySettings.realtimeReflectionProbes = true;
      QualitySettings.softParticles = true;
      QualitySettings.lodBias = 1.5f;
      QualitySettings.antiAliasing = 2;
      QualitySettings.vSyncCount = 1;
      QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
      QualitySettings.globalTextureMipmapLimit = 0;
      QualitySettings.skinWeights = SkinWeights.FourBones;
    }
  }

  public static void FuckLights(bool enable)
  {
    BetterDayNightManager.instance.AnimateLightFlash(2, enable ? 0.0f : 2f, enable ? 0.0f : 2f, 2f);
  }

  public static void AcidTrip(bool enable)
  {
    if (!enable)
    {
      if (!Visuals.acidApplied)
        return;
      foreach (KeyValuePair<Renderer, Material> originalMaterial in Visuals._acidOriginalMaterials)
      {
        if ((UnityEngine.Object) originalMaterial.Key != (UnityEngine.Object) null)
          originalMaterial.Key.sharedMaterial = originalMaterial.Value;
      }
      Visuals._acidOriginalMaterials.Clear();
      Visuals.acidApplied = false;
    }
    else
    {
      if (Visuals.acidApplied)
        return;
      Material material = AssetHandler.LoadMaterial("NXO.Resources.acidtrip", nameof (AcidTrip));
      if ((UnityEngine.Object) material == (UnityEngine.Object) null)
        return;
      foreach (Renderer key in UnityEngine.Object.FindObjectsOfType<Renderer>())
      {
        if (!((UnityEngine.Object) key == (UnityEngine.Object) null) && key.enabled && key.gameObject.activeSelf)
        {
          Visuals._acidOriginalMaterials[key] = key.sharedMaterial;
          key.sharedMaterial = material;
        }
      }
      Visuals.acidApplied = true;
    }
  }

  public static void TrippyMonkes(bool enable)
  {
    if (!enable)
    {
      if (!Visuals.trippyMonkesApplied)
        return;
      foreach (KeyValuePair<VRRig, Material> originalRigMaterial in Visuals.originalRigMaterials)
      {
        VRRig key = originalRigMaterial.Key;
        Material material = originalRigMaterial.Value;
        if ((UnityEngine.Object) key != (UnityEngine.Object) null && (UnityEngine.Object) key.mainSkin != (UnityEngine.Object) null && (UnityEngine.Object) material != (UnityEngine.Object) null)
          key.mainSkin.material = material;
      }
      Visuals.originalRigMaterials.Clear();
      Visuals.trippyMonkesApplied = false;
    }
    else
    {
      if (Visuals.trippyMonkesApplied)
        return;
      Material material = AssetHandler.LoadMaterial("NXO.Resources.acidtrip", "AcidTrip");
      if ((UnityEngine.Object) material == (UnityEngine.Object) null)
        return;
      foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
      {
        if (!((UnityEngine.Object) activeRig == (UnityEngine.Object) null) && !((UnityEngine.Object) activeRig.mainSkin == (UnityEngine.Object) null) && !((UnityEngine.Object) activeRig.mainSkin.material == (UnityEngine.Object) null) && !activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig == (UnityEngine.Object) Variables.taggerInstance.offlineVRRig))
        {
          if (!Visuals.originalRigMaterials.ContainsKey(activeRig))
            Visuals.originalRigMaterials[activeRig] = activeRig.mainSkin.material;
          activeRig.mainSkin.material = material;
        }
      }
      Visuals.trippyMonkesApplied = true;
    }
  }

  public static void FPC(bool enable)
  {
    if ((UnityEngine.Object) Variables.shoulderCamera == (UnityEngine.Object) null)
      Variables.shoulderCamera = GameObject.Find("Shoulder Camera");
    if ((UnityEngine.Object) Variables.thirdPersonCamera == (UnityEngine.Object) null)
      Variables.thirdPersonCamera = GameObject.Find("Third Person Camera");
    if (enable)
    {
      if (!Variables.didThirdPerson)
      {
        Variables.didThirdPerson = true;
        Variables.TransformCam = GameObject.Find("CM vcam1");
      }
      if ((UnityEngine.Object) Variables.TransformCam != (UnityEngine.Object) null)
        Variables.TransformCam.SetActive(false);
      if (!((UnityEngine.Object) Variables.shoulderCamera != (UnityEngine.Object) null))
        return;
      Variables.shoulderCamera.transform.SetParent(Camera.main.transform);
      Variables.shoulderCamera.transform.localPosition = Vector3.zero;
      Variables.shoulderCamera.transform.localRotation = Quaternion.identity;
      Variables.shoulderCamera.GetComponent<Camera>().fieldOfView = Settings.FOV;
    }
    else
    {
      if ((UnityEngine.Object) Variables.TransformCam != (UnityEngine.Object) null)
        Variables.TransformCam.SetActive(true);
      if ((UnityEngine.Object) Variables.shoulderCamera != (UnityEngine.Object) null && (UnityEngine.Object) Variables.thirdPersonCamera != (UnityEngine.Object) null)
      {
        Variables.shoulderCamera.transform.SetParent(Variables.thirdPersonCamera.transform);
        Variables.shoulderCamera.transform.localPosition = Vector3.zero;
        Variables.shoulderCamera.transform.localRotation = Quaternion.identity;
      }
    }
  }

  public static void Freecam()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Vector2 joystickAxis1 = InputHandler.GetJoystickAxis(true);
      Vector2 joystickAxis2 = InputHandler.GetJoystickAxis(false);
      Vector3 vector3_1 = new Vector3(joystickAxis1.x, joystickAxis2.y, joystickAxis1.y);
      Vector3 vector3_2 = GTVector3Extensions.X_Z(((Component) GTPlayer.Instance.bodyCollider).transform.forward);
      Vector3 vector3_3 = GTVector3Extensions.X_Z(((Component) GTPlayer.Instance.bodyCollider).transform.right);
      Vector3 b = (vector3_1.x * vector3_3 + vector3_1.y * Vector3.up + vector3_1.z * vector3_2) * Settings.FlySpeed;
      Visuals.CameraVelocity = Vector3.Lerp(Visuals.CameraVelocity, b, 0.12875f);
      Visuals.nxoCamera.transform.position += Visuals.CameraVelocity * Time.unscaledDeltaTime;
      Visuals.nxoCamera.transform.rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation;
    }
  }

  public static void VrThirdPerson()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.TransformPoint(new Vector3(0.0f, 0.8f, -1.5f));
      Visuals.nxoCamera.transform.rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation;
    }
  }

  public static void DrunkCam()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      float num = 15f;
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      Visuals.nxoCamera.transform.rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(Mathf.Sin(Time.time) * num, Mathf.Cos(Time.time * 0.7f) * num, Mathf.Sin(Time.time * 1.3f) * num);
    }
  }

  public static void SpectateGun()
  {
    if (GunLib.SetupLockOnGun())
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Visuals.nxoCamera.transform.position = GunLib.lockedTargetRig.bodyTransform.TransformPoint(new Vector3(0.0f, 0.8f, -1.5f));
      Visuals.nxoCamera.transform.rotation = GunLib.lockedTargetRig.headMesh.transform.rotation;
    }
    else
      Visuals.DisableNXOCamera();
  }

  public static void OrbitCam()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Vector3 vector3 = Quaternion.Euler(0.0f, Time.time * 45f, 0.0f) * new Vector3(2f, 0.5f, 0.0f);
      Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.bodyCollider).transform.position + vector3;
      Visuals.nxoCamera.transform.LookAt(((Component) Variables.taggerInstance.headCollider).transform.position);
    }
  }

  public static void VrThirdPersonInFront()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position + ((Component) Variables.taggerInstance.headCollider).transform.forward * 0.8f;
      Visuals.nxoCamera.transform.LookAt(((Component) Variables.taggerInstance.headCollider).transform.position);
    }
  }

  public static void UpsideDownCam()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref Visuals._camLast);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      if ((UnityEngine.Object) Visuals.nxoCamera == (UnityEngine.Object) null)
      {
        Visuals.nxoCamera = new GameObject("NXO_Cam");
        Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      }
      Camera camera = Visuals.nxoCamera.GetComponent<Camera>() ?? Visuals.nxoCamera.AddComponent<Camera>();
      camera.nearClipPlane = 0.01f;
      camera.cameraType = CameraType.Game;
      Visuals.nxoCamera.transform.position = ((Component) Variables.taggerInstance.headCollider).transform.position;
      Visuals.nxoCamera.transform.rotation = ((Component) Variables.taggerInstance.headCollider).transform.rotation * Quaternion.Euler(0.0f, 0.0f, 180f);
    }
  }

  public static void DisableNXOCamera()
  {
    InputHandler.isOn = false;
    if (!((UnityEngine.Object) Visuals.nxoCamera != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) Visuals.nxoCamera.GetComponent<Camera>());
    UnityEngine.Object.Destroy((UnityEngine.Object) Visuals.nxoCamera);
    Visuals.nxoCamera = (GameObject) null;
  }

  public static void Nametags(bool enable)
  {
    Visuals.nametagsActive = enable;
    if (!enable)
    {
      foreach (GameObject gameObject in Visuals.nametags.Values)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      Visuals.nametags.Clear();
    }
    else
    {
      List<VRRig> activeRigs = VRRigCache.ActiveRigs.ToList<VRRig>();
      foreach (VRRig key in Visuals.nametags.Keys.Where<VRRig>((Func<VRRig, bool>) (r => (UnityEngine.Object) r == (UnityEngine.Object) null || !activeRigs.Contains(r) || r.isOfflineVRRig)).ToList<VRRig>())
      {
        GameObject gameObject;
        if (Visuals.nametags.TryGetValue(key, out gameObject))
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
        Visuals.nametags.Remove(key);
      }
      foreach (VRRig vrRig in activeRigs)
      {
        if (!((UnityEngine.Object) vrRig == (UnityEngine.Object) null) && !vrRig.isMyPlayer && !vrRig.isOfflineVRRig && vrRig.Creator != null)
        {
          GameObject gameObject1;
          if (!Visuals.nametags.TryGetValue(vrRig, out gameObject1) || (UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
          {
            gameObject1 = new GameObject("Nametag");
            GameObject gameObject2 = new GameObject("Shadow");
            gameObject2.transform.SetParent(gameObject1.transform);
            gameObject2.transform.localPosition = new Vector3(3f / 1000f, -3f / 1000f, 1f / 1000f);
            gameObject2.transform.localScale = Vector3.one;
            TextMesh textMesh1 = gameObject2.AddComponent<TextMesh>();
            textMesh1.fontSize = 280;
            textMesh1.fontStyle = (FontStyle) 1;
            textMesh1.font = Main.CurrentFont;
            textMesh1.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
            textMesh1.anchor = (TextAnchor) 4;
            textMesh1.alignment = (TextAlignment) 1;
            ((Component) textMesh1).GetComponent<Renderer>().material = textMesh1.font.material;
            TextMesh textMesh2 = gameObject1.AddComponent<TextMesh>();
            textMesh2.fontSize = 280;
            textMesh2.fontStyle = (FontStyle) 1;
            textMesh2.font = Main.CurrentFont;
            textMesh2.anchor = (TextAnchor) 4;
            textMesh2.alignment = (TextAlignment) 1;
            ((Component) textMesh2).GetComponent<Renderer>().material = textMesh2.font.material;
            Visuals.nametags[vrRig] = gameObject1;
          }
          TextMesh component1 = gameObject1.GetComponent<TextMesh>();
          TextMesh component2 = gameObject1.transform.GetChild(0)?.GetComponent<TextMesh>();
          Color playerColor = vrRig.playerColor;
          string htmlStringRgb = ColorUtility.ToHtmlStringRGB(playerColor);
          string platform = Visuals.GetPlatform(vrRig);
          string str1 = $"({(int) ((double) playerColor.r * 9.0)}, {(int) ((double) playerColor.g * 9.0)}, {(int) ((double) playerColor.b * 9.0)})";
          int fps = vrRig.fps;
          string str2 = fps >= 90 ? "00FF00" : (fps >= 72 ? "7FFF00" : (fps >= 60 ? "FFFF00" : (fps >= 45 ? "FFA500" : "FF0000")));
          string str3 = $"<color=#{str2}>{fps}</color>";
          string str4;
          if (!Visuals.nxoNametagsActive || !NetworkingLibrary.IsNXOUser(vrRig.Creator.UserId))
            str4 = "";
          else
            str4 = $"<color=#{htmlStringRgb}>(</color><color=#00FFFF>NXO</color><color=#{htmlStringRgb}>)</color> ";
          string str5 = $"{str4}<color=#{htmlStringRgb}>{vrRig.Creator.NickName}</color>";
          int nametagTypeIndex = Settings.nametagTypeIndex;
          if (true)
            ;
          string str6;
          switch (nametagTypeIndex)
          {
            case 0:
              str6 = $"<size=320>{str5}</size>\n<size=220>{platform} | {str1} | {str3}</size>\n<size=200><color=#AAAAAA>{vrRig.Creator.UserId}</color></size>";
              break;
            case 1:
              str6 = $"<size=320>{str5}</size>\n<size=220>{platform} | {str3}</size>";
              break;
            case 2:
              str6 = $"<size=320>{str5}</size>\n<size=220>{platform}</size>";
              break;
            default:
              str6 = $"<size=320>{str5}</size>\n<size=220>{platform}</size>";
              break;
          }
          if (true)
            ;
          string str7 = str6;
          component1.text = str7;
          if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
            component2.text = str7.Replace($"<color=#{htmlStringRgb}>", "<color=#000000>").Replace("<color=#AAAAAA>", "<color=#000000>").Replace("<color=#00FFFF>", "<color=#000000>").Replace($"<color=#{str2}>", "<color=#000000>");
          float num = (float) (0.0035000001080334187 + (double) Mathf.Min(Vector3.Distance(Camera.main.transform.position, ((Component) vrRig).transform.position), 20f) * 0.00025000001187436283);
          gameObject1.transform.localScale = Vector3.one * num;
          gameObject1.transform.position = vrRig.headMesh.transform.position + Vector3.up * 0.55f;
          gameObject1.transform.LookAt(Camera.main.transform);
          gameObject1.transform.Rotate(0.0f, 180f, 0.0f);
        }
      }
    }
  }

  public static void NXONametags(bool enable)
  {
    Visuals.nxoNametagsActive = enable;
    if (!enable || Visuals.nametagsActive)
    {
      foreach (GameObject gameObject in Visuals.nxoNametags.Values)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      Visuals.nxoNametags.Clear();
    }
    else
    {
      List<VRRig> activeRigs = VRRigCache.ActiveRigs.ToList<VRRig>();
      foreach (VRRig key in Visuals.nxoNametags.Keys.Where<VRRig>((Func<VRRig, bool>) (r => (UnityEngine.Object) r == (UnityEngine.Object) null || !activeRigs.Contains(r) || r.isOfflineVRRig || !NetworkingLibrary.IsNXOUser(r.Creator?.UserId))).ToList<VRRig>())
      {
        GameObject gameObject;
        if (Visuals.nxoNametags.TryGetValue(key, out gameObject))
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
        Visuals.nxoNametags.Remove(key);
      }
      foreach (VRRig key in activeRigs)
      {
        if (!((UnityEngine.Object) key == (UnityEngine.Object) null) && !key.isMyPlayer && !key.isOfflineVRRig && key.Creator != null && NetworkingLibrary.IsNXOUser(key.Creator.UserId))
        {
          GameObject gameObject1;
          if (!Visuals.nxoNametags.TryGetValue(key, out gameObject1) || (UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
          {
            gameObject1 = new GameObject("NXONametag");
            GameObject gameObject2 = new GameObject("Shadow");
            gameObject2.transform.SetParent(gameObject1.transform);
            gameObject2.transform.localPosition = new Vector3(3f / 1000f, -3f / 1000f, 1f / 1000f);
            gameObject2.transform.localScale = Vector3.one;
            TextMesh textMesh1 = gameObject2.AddComponent<TextMesh>();
            textMesh1.fontSize = 280;
            textMesh1.fontStyle = (FontStyle) 1;
            textMesh1.font = Main.CurrentFont;
            textMesh1.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
            textMesh1.anchor = (TextAnchor) 4;
            textMesh1.alignment = (TextAlignment) 1;
            ((Component) textMesh1).GetComponent<Renderer>().material = textMesh1.font.material;
            textMesh1.text = "<size=320>NXO</size>";
            TextMesh textMesh2 = gameObject1.AddComponent<TextMesh>();
            textMesh2.fontSize = 280;
            textMesh2.fontStyle = (FontStyle) 1;
            textMesh2.font = Main.CurrentFont;
            textMesh2.anchor = (TextAnchor) 4;
            textMesh2.alignment = (TextAlignment) 1;
            ((Component) textMesh2).GetComponent<Renderer>().material = textMesh2.font.material;
            textMesh2.text = "<size=320><color=#00FFFF>NXO User</color></size>";
            Visuals.nxoNametags[key] = gameObject1;
          }
          float num = (float) (0.0035000001080334187 + (double) Mathf.Min(Vector3.Distance(Camera.main.transform.position, ((Component) key).transform.position), 20f) * 0.00025000001187436283);
          gameObject1.transform.localScale = Vector3.one * num;
          gameObject1.transform.position = key.headMesh.transform.position + Vector3.up * 0.55f;
          gameObject1.transform.LookAt(Camera.main.transform);
          gameObject1.transform.Rotate(0.0f, 180f, 0.0f);
        }
      }
    }
  }

  public static string GetPlatform(VRRig rig, bool raw = false)
  {
    object obj;
    ((Dictionary<object, object>) rig.creator.GetPlayerRef().CustomProperties).TryGetValue((object) "platform", out obj);
    if (obj != null)
    {
      string platform = obj.ToString();
      if (!raw)
        platform = obj.ToString().ToLower().Contains("steam") ? "<color=#001b96>STEAM</color>" : "<color=#FFA500>QUEST</color>";
      return platform;
    }
    if (rig.HasCosmetic("S. FIRST LOGIN"))
      return "<color=#001b96>STEAM</color>";
    return rig.HasCosmetic("FIRST LOGIN") ? "<color=#4CFF4C>PC</color>" : "<color=#FFA500>QUEST</color>";
  }

  public static void CapFPS(int fps)
  {
    float num1 = 1f / (float) fps;
    float num2 = Time.realtimeSinceStartup - Visuals.lastFrame;
    if ((double) num2 < (double) num1)
    {
      int millisecondsTimeout = Mathf.FloorToInt((float) (((double) num1 - (double) num2) * 1000.0));
      if (millisecondsTimeout > 0)
        Thread.Sleep(millisecondsTimeout);
    }
    Visuals.lastFrame = Time.realtimeSinceStartup;
  }

  public static void UncapFPS(bool enabled)
  {
    if (enabled)
    {
      QualitySettings.vSyncCount = 0;
      Application.targetFrameRate = int.MaxValue;
    }
    else
      Application.targetFrameRate = 144 /*0x90*/;
  }

  public static void ToggleXray(bool enabled)
  {
    if (enabled)
    {
      if (Visuals.hiddenRenderers.Count != 0)
        return;
      foreach (Renderer renderer in Variables.GetAllType<Renderer>())
      {
        if (!((UnityEngine.Object) renderer == (UnityEngine.Object) null) && !(renderer is SkinnedMeshRenderer) && renderer.enabled && renderer.gameObject.activeSelf)
        {
          renderer.enabled = false;
          Visuals.hiddenRenderers.Add(renderer);
        }
      }
    }
    else
    {
      foreach (Renderer hiddenRenderer in Visuals.hiddenRenderers)
      {
        if ((UnityEngine.Object) hiddenRenderer != (UnityEngine.Object) null)
          hiddenRenderer.enabled = true;
      }
      Visuals.hiddenRenderers.Clear();
    }
  }

  public static void ShinyMonkes(bool enable)
  {
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (!activeRig.isOfflineVRRig && !((UnityEngine.Object) activeRig.mainSkin?.material == (UnityEngine.Object) null))
      {
        Material material = activeRig.mainSkin.material;
        if (enable)
        {
          material.shader = Variables.shinyShader;
          material.SetFloat("_Smoothness", 0.95f);
          material.SetFloat("_Metallic", 0.85f);
        }
        else
          material.shader = Variables.uberShader;
      }
    }
  }

  public static void ShinySelf(bool enable)
  {
    Material material = Variables.taggerInstance.offlineVRRig.mainSkin.material;
    if (enable)
    {
      material.shader = Variables.shinyShader;
      material.SetFloat("_Smoothness", 0.95f);
      material.SetFloat("_Metallic", 0.85f);
    }
    else
      material.shader = Variables.uberShader;
  }

  public static void MonkeSense(bool enable)
  {
    bool flag1 = RigManager.RigIsInfected(VRRig.LocalRig);
    if (!enable)
    {
      foreach (GameObject gameObject in Visuals.monkeSenseVignette)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      Visuals.monkeSenseVignette.Clear();
      if (!((UnityEngine.Object) Visuals.monkeSenseAudio != (UnityEngine.Object) null))
        return;
      Visuals.monkeSenseAudio.Stop();
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) Visuals.monkeSenseAudio).gameObject);
      Visuals.monkeSenseAudio = (AudioSource) null;
    }
    else
    {
      List<VRRig> list = VRRigCache.ActiveRigs.ToList<VRRig>();
      List<(VRRig, float)> valueTupleList = new List<(VRRig, float)>();
      float num1 = float.MaxValue;
      foreach (VRRig rig in list)
      {
        if (!rig.isOfflineVRRig)
        {
          bool flag2 = RigManager.RigIsInfected(rig);
          if (flag1 ? !flag2 : flag2)
          {
            float num2 = Vector3.Distance(((Component) Variables.taggerInstance.bodyCollider).transform.position, ((Component) rig).transform.position);
            if ((double) num2 <= 15.0)
            {
              valueTupleList.Add((rig, num2));
              if ((double) num2 < (double) num1)
                num1 = num2;
            }
          }
        }
      }
      int length1 = 8;
      Transform transform = Camera.main.transform;
      while (Visuals.monkeSenseVignette.Count < length1)
      {
        GameObject gameObject = new GameObject("MonkeSenseArrow");
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Variables.guiShader);
        Visuals.monkeSenseVignette.Add(gameObject);
      }
      float[] numArray1 = new float[length1];
      foreach ((VRRig vrRig, float num3) in valueTupleList)
      {
        Vector3 vector3 = (((Component) vrRig).transform.position - transform.position) with
        {
          y = 0.0f
        };
        float target = -Vector3.SignedAngle((transform.forward with
        {
          y = 0.0f
        }).normalized, vector3.normalized, Vector3.up);
        float a = Mathf.Lerp(0.15f, 1f, 1f - Mathf.Clamp01((float) (((double) num3 - 3.0) / 12.0)));
        for (int index = 0; index < length1; ++index)
        {
          float num4 = Mathf.Abs(Mathf.DeltaAngle(360f / (float) length1 * (float) index, target));
          if ((double) num4 < 360.0 / (double) length1)
          {
            float num5 = Mathf.Lerp(a, 0.0f, num4 / (360f / (float) length1));
            if ((double) num5 > (double) numArray1[index])
              numArray1[index] = num5;
          }
        }
      }
      Color color = flag1 ? Color.green : Color.red;
      for (int index1 = 0; index1 < length1; ++index1)
      {
        float f = (float) ((360.0 / (double) length1 * (double) index1 + 90.0) * (Math.PI / 180.0));
        Vector3 vector3_1 = transform.right * Mathf.Cos(f) + transform.up * Mathf.Sin(f);
        Vector3 vector3_2 = transform.right * Mathf.Cos(f + 1.57079637f) + transform.up * Mathf.Sin(f + 1.57079637f);
        LineRenderer component = Visuals.monkeSenseVignette[index1].GetComponent<LineRenderer>();
        bool flag3 = (double) numArray1[index1] > 0.0;
        int num6 = 20;
        component.positionCount = num6;
        float num7 = 6f;
        float num8 = flag3 ? Mathf.Lerp(0.01f, 0.025f, numArray1[index1]) : 0.008f;
        float num9 = flag3 ? 4f : 1.5f;
        for (int index2 = 0; index2 < num6; ++index2)
        {
          float t = (float) index2 / (float) (num6 - 1);
          Vector3 vector3_3 = transform.position + transform.forward * 0.6f + vector3_1 * Mathf.Lerp(0.25f, 0.55f, t);
          float num10 = Mathf.Sin((float) ((double) t * (double) num7 * 3.1415927410125732 + (double) Time.time * (double) num9)) * num8 * Mathf.Sin(t * 3.14159274f);
          component.SetPosition(index2, vector3_3 + vector3_2 * num10);
        }
        component.startWidth = flag3 ? Mathf.Lerp(0.03f, 0.07f, numArray1[index1]) : 0.025f;
        component.endWidth = 0.0f;
        if (flag3)
        {
          component.startColor = new Color(color.r, color.g, color.b, numArray1[index1]);
          component.endColor = new Color(color.r, color.g, color.b, 0.0f);
        }
        else
        {
          component.startColor = new Color(1f, 1f, 1f, 0.4f);
          component.endColor = new Color(1f, 1f, 1f, 0.0f);
        }
        Visuals.monkeSenseVignette[index1].SetActive(true);
      }
      if (valueTupleList.Count == 0)
      {
        foreach (GameObject gameObject in Visuals.monkeSenseVignette)
        {
          if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
            gameObject.SetActive(false);
        }
        if (!((UnityEngine.Object) Visuals.monkeSenseAudio != (UnityEngine.Object) null))
          return;
        Visuals.monkeSenseAudio.Stop();
      }
      else
      {
        float num11 = (double) num1 <= 3.0 ? 0.15f : ((double) num1 <= 8.0 ? 0.4f : 1.2f);
        if (Visuals.muteMonkeSense || (double) Time.time < (double) Visuals.monkeSenseBeepCooldown)
          return;
        Visuals.monkeSenseBeepCooldown = Time.time + num11;
        if ((UnityEngine.Object) Visuals.monkeSenseAudio == (UnityEngine.Object) null)
        {
          Visuals.monkeSenseAudio = new GameObject("MonkeSenseAudio").AddComponent<AudioSource>();
          Visuals.monkeSenseAudio.spatialBlend = 0.0f;
          Visuals.monkeSenseAudio.volume = 0.6f;
        }
        int num12 = 44100;
        int length2 = num12 / 10;
        float num13 = (double) num1 <= 3.0 ? 880f : ((double) num1 <= 8.0 ? 660f : 440f);
        AudioClip audioClip = AudioClip.Create("SenseBeep", length2, 1, num12, false);
        float[] numArray2 = new float[length2];
        for (int index = 0; index < length2; ++index)
        {
          float num14 = (float) index / (float) num12;
          float num15 = (float) (1.0 - (double) index / (double) length2);
          numArray2[index] = (float) ((double) Mathf.Sin(6.28318548f * num13 * num14) * (double) num15 * 0.5);
        }
        audioClip.SetData(numArray2, 0);
        Visuals.monkeSenseAudio.PlayOneShot(audioClip);
      }
    }
  }

  public static void ToggleFog(bool enable)
  {
    if (enable)
      ZoneShaderSettings.activeInstance.SetGroundFogValue(new Color(0.9569f, 0.6941f, 0.502f, 0.1216f), 40f, 10f, 40f);
    else
      ZoneShaderSettings.activeInstance.SetGroundFogValue(Color.clear, 0.0f, 0.0f, 0.0f);
  }

  public static void ToggleSnow(bool enable)
  {
    GameObject gameObject = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/Environment/WeatherDayNight")?.transform?.Find("snow")?.gameObject;
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    gameObject.SetActive(enable);
    ((Behaviour) gameObject.GetComponent<TimeOfDayDependentAudio>()).enabled = !enable;
    Transform transform = gameObject.transform.Find("snow partic");
    if (!((UnityEngine.Object) transform != (UnityEngine.Object) null))
      return;
    transform.gameObject.SetActive(enable);
  }

  private class ESPData
  {
    public Dictionary<VRRig, GameObject> singleObjects = new Dictionary<VRRig, GameObject>();
    public Dictionary<VRRig, List<GameObject>> multiObjects = new Dictionary<VRRig, List<GameObject>>();
    public Material material;

    public void Clear()
    {
      foreach (GameObject gameObject in this.singleObjects.Values)
      {
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
      }
      this.singleObjects.Clear();
      foreach (List<GameObject> gameObjectList in this.multiObjects.Values)
      {
        foreach (GameObject gameObject in gameObjectList)
        {
          if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
            UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
        }
      }
      this.multiObjects.Clear();
      if (!((UnityEngine.Object) this.material != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.material);
      this.material = (Material) null;
    }
  }
}
