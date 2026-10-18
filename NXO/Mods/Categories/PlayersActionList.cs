
using ExitGames.Client.Photon;
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

public class PlayersActionList
{
  public static NetPlayer selectedPlayer;
  public static Camera PlayerCam;
  public static GameObject PlayerCamScreen;
  public static RenderTexture renderTexture;
  public static bool isCameraActive = false;
  public static GameObject PlayerCamOutline;
  public static readonly HashSet<string> IgnoredModProps = new HashSet<string>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase)
  {
    "didTutorial",
    "platform",
    "mothershipid"
  };

  public static void GeneratePlayerButtons()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).ToList<ButtonHandler.Button>();
    list.RemoveAll((Predicate<ButtonHandler.Button>) (b => b.Page == Category.Players));
    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
    {
      NetPlayer capturedPlayer = ((NetPlayer) (player));
      VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(((NetPlayer) (player)));
      string label = player.NickName;
      if ((UnityEngine.Object) rigFromNetPlayer != (UnityEngine.Object) null && (UnityEngine.Object) rigFromNetPlayer.mainSkin?.material != (UnityEngine.Object) null)
        label = $"<color=#{ColorUtility.ToHtmlStringRGB(rigFromNetPlayer.mainSkin.material.color)}>{player.NickName}</color>";
      if (RigManager.RigIsInfected(RigManager.GetVRRigFromNetPlayer(capturedPlayer)))
        label += " : <color=red>Tagged</color>";
      list.Add(new ButtonHandler.Button(label, Category.Players, false, false, (Action) (() =>
      {
        PlayersActionList.selectedPlayer = capturedPlayer;
        PlayersActionList.GeneratePlayer_ActionButtons();
        ButtonHandler.ChangePage(Category.Player_Action);
      }))
      {
        isCategory = true
      });
    }
    ModButtons.buttons = list.ToArray();
    Main.RefreshMenu();
  }

  public static void GeneratePlayer_Mods()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).ToList<ButtonHandler.Button>();
    list.RemoveAll((Predicate<ButtonHandler.Button>) (b => b.Page == Category.Player_Mods));
    RigManager.GetVRRigFromNetPlayer(PlayersActionList.selectedPlayer);
    list.Add(new ButtonHandler.Button("Return", Category.Player_Mods, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Player_Action)))
    {
      isCategory = true
    });
    foreach (DictionaryEntry customProperty in PlayersActionList.selectedPlayer.GetPlayerRef().CustomProperties)
    {
      if (!PlayersActionList.IgnoredModProps.Contains(customProperty.Key.ToString()))
        list.Add(new ButtonHandler.Button(customProperty.Key.ToString(), Category.Player_Mods, false, false, (Action) null));
    }
    ModButtons.buttons = list.ToArray();
  }

  public static void GeneratePlayer_ActionButtons()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).ToList<ButtonHandler.Button>();
    list.RemoveAll((Predicate<ButtonHandler.Button>) (b => b.Page == Category.Player_Action));
    VRRig targetRig = RigManager.GetVRRigFromNetPlayer(PlayersActionList.selectedPlayer);
    ExitGames.Client.Photon.Hashtable customProperties = PlayersActionList.selectedPlayer.GetPlayerRef().CustomProperties;
    list.Add(new ButtonHandler.Button("Return", Category.Player_Action, false, false, (Action) (() =>
    {
      PlayersActionList.ClearPlayerCam(true);
      ButtonHandler.ChangePage(Category.Players);
    }))
    {
      isCategory = true
    });
    list.Add(new ButtonHandler.Button("Name : " + PlayersActionList.selectedPlayer.NickName, Category.Player_Action, false, false, (Action) null));
    list.Add(new ButtonHandler.Button("Platform: " + Visuals.GetPlatform(targetRig), Category.Player_Action, false, false, (Action) null));
    list.Add(new ButtonHandler.Button("HZ : " + targetRig.fps.ToString(), Category.Player_Action, false, false, (Action) null));
    list.Add(new ButtonHandler.Button("ID : " + PlayersActionList.selectedPlayer.UserId.ToUpper(), Category.Player_Action, false, false, (Action) null));
    list.Add(new ButtonHandler.Button($"Color : {(targetRig.mainSkin.material.color.r * 9f).ToString()}, {(targetRig.mainSkin.material.color.g * 9f).ToString()}, {(targetRig.mainSkin.material.color.b * 9f).ToString()}", Category.Player_Action, false, false, (Action) null));
    list.Add(new ButtonHandler.Button("Mod(s) : " + ((IEnumerable<KeyValuePair<object, object>>) customProperties).Count<KeyValuePair<object, object>>((Func<KeyValuePair<object, object>, bool>) (p => !PlayersActionList.IgnoredModProps.Contains(p.Key.ToString()))).ToString(), Category.Player_Action, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Player_Mods)))
    {
      isCategory = true
    });
    list.Add(new ButtonHandler.Button("Tag Player", Category.Player_Action, true, false, (Action) (() =>
    {
      if (!((UnityEngine.Object) targetRig != (UnityEngine.Object) null))
        return;
      if (!RigManager.RigIsInfected(VRRig.LocalRig))
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Must Be Tagged");
      else if (RigManager.RigIsInfected(targetRig))
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, $"`{targetRig.Creator.NickName}` Is Already Tagged");
      else
        Gamemode.TagPlayer(targetRig);
    })));
    list.Add(new ButtonHandler.Button("Lag Player", Category.Player_Action, true, false, (Action) (() => Overpowered.LagTarget(new int[1]
    {
      targetRig.Creator.ActorNumber
    }))));
    list.Add(new ButtonHandler.Button("Deafen Player", Category.Player_Action, true, false, (Action) (() => Overpowered.DeafenPlayer((object) targetRig))));
    list.Add(new ButtonHandler.Button("Copy Player", Category.Player_Action, true, false, (Action) (() => Player.CopyPlayer(targetRig)), (Action) (() => Player.SetRigStatus(true))));
    list.Add(new ButtonHandler.Button("Piggyback Player", Category.Player_Action, true, false, (Action) (() =>
    {
      if (!((UnityEngine.Object) targetRig != (UnityEngine.Object) null))
        return;
      Movement.TeleportTo(((Component) targetRig).transform.position + ((Component) targetRig).transform.up * 0.3f - ((Component) targetRig).transform.forward * 0.4f);
    })));
    list.Add(new ButtonHandler.Button("Spectate Player (X)", Category.Player_Action, true, false, (Action) (() => PlayersActionList.SpectatePlayer()), (Action) (() => Visuals.DisableNXOCamera())));
    list.Add(new ButtonHandler.Button("Teleport To Player", Category.Player_Action, false, false, (Action) (() => Movement.TeleportTo(((Component) targetRig).transform.position)), (Action) (() => Player.SetRigStatus(true))));
    ModButtons.buttons = list.ToArray();
  }

  public static void SpectatePlayer()
  {
    InputHandler.ToggleOnButtonPress(new Func<bool>(InputHandler.LPrimary), ref InputHandler.isOn, ref InputHandler.wasButtonPressed);
    if (!InputHandler.isOn)
    {
      Visuals.DisableNXOCamera();
    }
    else
    {
      VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(PlayersActionList.selectedPlayer);
      if ((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null)
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
        Visuals.nxoCamera.transform.position = rigFromNetPlayer.bodyTransform.TransformPoint(new Vector3(0.0f, 0.8f, -1.5f));
        Visuals.nxoCamera.transform.rotation = rigFromNetPlayer.headMesh.transform.rotation;
      }
    }
  }

  public static IEnumerator DelayedPlayerButtonGeneration()
  {
    yield return (object) new WaitForSeconds(0.5f);
    if (Variables.currentPage == Category.Players)
      PlayersActionList.GeneratePlayerButtons();
  }

  public static void AddPlayerCam()
  {
    if (PlayersActionList.selectedPlayer == null || (UnityEngine.Object) Variables.menuObj == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) RigManager.GetVRRigFromNetPlayer(PlayersActionList.selectedPlayer) == (UnityEngine.Object) null)
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Players Rig Is Null, Please Refresh");
    }
    else
    {
      if ((UnityEngine.Object) PlayersActionList.PlayerCamScreen != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) PlayersActionList.PlayerCamScreen);
        PlayersActionList.PlayerCamScreen = (GameObject) null;
      }
      if ((UnityEngine.Object) PlayersActionList.PlayerCam == (UnityEngine.Object) null)
      {
        PlayersActionList.PlayerCam = new GameObject("NXO Photon.Realtime.Player Camera").AddComponent<Camera>();
        PlayersActionList.renderTexture = new RenderTexture(512 /*0x0200*/, 512 /*0x0200*/, 16 /*0x10*/, RenderTextureFormat.ARGB32);
        PlayersActionList.renderTexture.filterMode = FilterMode.Bilinear;
        PlayersActionList.renderTexture.wrapMode = TextureWrapMode.Clamp;
        PlayersActionList.renderTexture.Create();
        PlayersActionList.PlayerCam.targetTexture = PlayersActionList.renderTexture;
        PlayersActionList.PlayerCam.fieldOfView = 90f;
        PlayersActionList.PlayerCam.nearClipPlane = 0.01f;
        PlayersActionList.PlayerCam.farClipPlane = 1000f;
        PlayersActionList.PlayerCam.clearFlags = CameraClearFlags.Skybox;
        PlayersActionList.PlayerCam.enabled = true;
        PlayersActionList.PlayerCam.cullingMask = -1;
      }
      PlayersActionList.PlayerCamScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
      PlayersActionList.PlayerCamScreen.name = "NXO Photon.Realtime.Player Camera Screen";
      PlayersActionList.PlayerCamScreen.transform.SetParent(Variables.menuObj.transform, false);
      PlayersActionList.PlayerCamScreen.transform.localRotation = Quaternion.Euler(90f, 0.0f, 0.0f);
      PlayersActionList.PlayerCamScreen.transform.localScale = new Vector3(0.01f, 0.5f, 0.75f);
      PlayersActionList.PlayerCamScreen.transform.localPosition = new Vector3(0.064f, 0.85f, 0.05f);
      Renderer component1 = PlayersActionList.PlayerCamScreen.GetComponent<Renderer>();
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      {
        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = (Texture) PlayersActionList.renderTexture;
        component1.material = material;
        Main._dynamicMaterials.Add(material);
      }
      Collider component2 = PlayersActionList.PlayerCamScreen.GetComponent<Collider>();
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) component2);
      Material pinwheelMaterial = Main.GetPinwheelMaterial();
      if ((UnityEngine.Object) pinwheelMaterial != (UnityEngine.Object) null)
      {
        PlayersActionList.PlayerCamOutline = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Rigidbody component3 = PlayersActionList.PlayerCamOutline.GetComponent<Rigidbody>();
        if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) component3);
        BoxCollider component4 = PlayersActionList.PlayerCamOutline.GetComponent<BoxCollider>();
        if ((UnityEngine.Object) component4 != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) component4);
        PlayersActionList.PlayerCamOutline.transform.SetParent(Variables.menuObj.transform, false);
        PlayersActionList.PlayerCamOutline.transform.localPosition = PlayersActionList.PlayerCamScreen.transform.localPosition;
        PlayersActionList.PlayerCamOutline.transform.localRotation = PlayersActionList.PlayerCamScreen.transform.localRotation;
        Vector3 localScale = PlayersActionList.PlayerCamScreen.transform.localScale;
        PlayersActionList.PlayerCamOutline.transform.localScale = new Vector3(localScale.x - 1f / 400f, localScale.y + 0.015f, localScale.z + 0.015f);
        PlayersActionList.PlayerCamOutline.GetComponent<Renderer>().material = pinwheelMaterial;
        Main._dynamicMaterials.Add(pinwheelMaterial);
      }
      PlayersActionList.isCameraActive = true;
    }
  }

  public static void UpdatePlayerCam()
  {
    if (!PlayersActionList.isCameraActive || (UnityEngine.Object) PlayersActionList.PlayerCam == (UnityEngine.Object) null || PlayersActionList.selectedPlayer == null || Variables.currentPage != Category.Player_Action)
      return;
    VRRig rigFromNetPlayer = RigManager.GetVRRigFromNetPlayer(PlayersActionList.selectedPlayer);
    if ((UnityEngine.Object) rigFromNetPlayer == (UnityEngine.Object) null)
    {
      PlayersActionList.ClearPlayerCam(true);
    }
    else
    {
      if ((UnityEngine.Object) rigFromNetPlayer.head?.rigTarget == (UnityEngine.Object) null)
        return;
      Transform rigTarget = rigFromNetPlayer.head.rigTarget;
      Vector3 vector3 = rigTarget.position - rigTarget.forward * 1f + Vector3.up * 0.25f;
      PlayersActionList.PlayerCam.transform.position = vector3;
      PlayersActionList.PlayerCam.transform.rotation = Quaternion.LookRotation(rigTarget.forward, Vector3.up);
    }
  }

  public static void ClearPlayerCam(bool clearAll = false)
  {
    if (clearAll)
    {
      if ((UnityEngine.Object) PlayersActionList.PlayerCam != (UnityEngine.Object) null)
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) PlayersActionList.PlayerCam.gameObject);
        PlayersActionList.PlayerCam = (Camera) null;
      }
      if ((UnityEngine.Object) PlayersActionList.renderTexture != (UnityEngine.Object) null)
      {
        if (PlayersActionList.renderTexture.IsCreated())
          PlayersActionList.renderTexture.Release();
        UnityEngine.Object.Destroy((UnityEngine.Object) PlayersActionList.renderTexture);
        PlayersActionList.renderTexture = (RenderTexture) null;
      }
      PlayersActionList.selectedPlayer = (NetPlayer) null;
      PlayersActionList.isCameraActive = false;
    }
    if ((UnityEngine.Object) PlayersActionList.PlayerCamScreen != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) PlayersActionList.PlayerCamScreen);
      PlayersActionList.PlayerCamScreen = (GameObject) null;
    }
    if (!((UnityEngine.Object) PlayersActionList.PlayerCamOutline != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) PlayersActionList.PlayerCamOutline);
    PlayersActionList.PlayerCamOutline = (GameObject) null;
  }
}
