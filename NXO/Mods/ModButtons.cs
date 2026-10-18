
using GorillaLocomotion;
using GorillaNetworking;
using NXO.Menu;
using NXO.Mods.Categories;
using NXO.Utilities;
using Photon.Pun;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods;

public class ModButtons
{
  private static ButtonHandler.Button[] _buttons;

  public static ButtonHandler.Button[] buttons
  {
    get => ModButtons._buttons;
    set
    {
      ModButtons._buttons = value;
      Main.MarkActiveTicksDirty();
    }
  }

  static ModButtons()
  {
    ButtonHandler.Button[] buttonArray = new ButtonHandler.Button[506]
    {
      new ButtonHandler.Button("Settings", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Presets", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Presets)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Favorites", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Favorited)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Enabled", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Enabled)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Players List", Category.Home, false, false, (Action) (() =>
      {
        ButtonHandler.ChangePage(Category.Players);
        PlayersActionList.GeneratePlayerButtons();
      }))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Safety", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Safety)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Room", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Room)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("VRRig", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Player)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Movement", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Movement)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Visuals", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Visuals)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Audio", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Audio)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Gamemode", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Gamemode)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Projectiles", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Projectiles)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Fun", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Fun)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Macros", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Macros)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Nextbots", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Nextbots)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Overpowered", Category.Home, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Overpowered)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Open NXO Folder", Category.Settings, false, false, (Action) (() => ButtonHandler.OpenFolder(Variables.folderName))),
      new ButtonHandler.Button("Default Settings", Category.Settings, false, false, (Action) (() => Settings.ResetToDefaultSettings())),
      new ButtonHandler.Button("Auto Save", Category.Settings, true, true, (Action) (() => ButtonHandler.SetAutoSave(true)), (Action) (() => ButtonHandler.SetAutoSave(false))),
      new ButtonHandler.Button("Panic (X)", Category.Settings, true, false, (Action) (() => Safety.Panic()), (Action) (() => Safety.PanicReset())),
      new ButtonHandler.Button("Disable All Mods", Category.Settings, false, false, (Action) (() => ButtonHandler.DisableAllMods())),
      new ButtonHandler.Button("Menu", Category.Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Menu_Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Menu_Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Right Hand Menu", Category.Menu_Settings, true, false, (Action) (() => Settings.SwitchHands(true)), (Action) (() => Settings.SwitchHands(false))),
      new ButtonHandler.Button("Open & Close Sounds", Category.Menu_Settings, true, true, (Action) (() => Settings.MenuOpenAndCloseSounds(true)), (Action) (() => Settings.MenuOpenAndCloseSounds(false))),
      new ButtonHandler.Button("Dynamic Animations", Category.Menu_Settings, true, true, (Action) (() => Main.MenuAnimations = true), (Action) (() => Main.MenuAnimations = false)),
      new ButtonHandler.Button("Menu Smoothing", Category.Menu_Settings, true, false, (Action) (() => Main.MenuSmoothing = true), (Action) (() => Main.MenuSmoothing = false)),
      new ButtonHandler.Button("Menu Toggles", Category.Menu_Settings, true, false, (Action) (() => Variables.MenuToggles = true), (Action) (() => Variables.MenuToggles = false)),
      new ButtonHandler.Button("Secondary Menu Position", Category.Menu_Settings, true, false, (Action) (() => Variables.SecondaryMenuPosition = true), (Action) (() => Variables.SecondaryMenuPosition = false)),
      new ButtonHandler.Button("Haptic Feedback", Category.Menu_Settings, true, true, (Action) (() => Variables.HapticFeedback = true), (Action) (() => Variables.HapticFeedback = false)),
      new ButtonHandler.Button("In-Game Array List", Category.Menu_Settings, true, false, (Action) (() => NotificationLib.ArrayListEnabled = true), (Action) (() => NotificationLib.ArrayListEnabled = false)),
      new ButtonHandler.Button("Classic Buttons", Category.Menu_Settings, true, false, (Action) (() => Settings.SetClassicButtons(true)), (Action) (() => Settings.SetClassicButtons(false))),
      new ButtonHandler.Button("Menu Follows When Searching", Category.Menu_Settings, true, true, (Action) (() => Variables.SearchFollow = true), (Action) (() => Variables.SearchFollow = false)),
      new ButtonHandler.Button("Overlapping Custom Click Sound", Category.Menu_Settings, true, false, (Action) (() => ButtonHandler.OverlapCustomClickSounds = true), (Action) (() => ButtonHandler.OverlapCustomClickSounds = false)),
      new ButtonHandler.Button("Custom Board Colors", Category.Menu_Settings, true, true, (Action) (() => CustomBoards.SetBoardsEnabled(true)), (Action) (() => CustomBoards.SetBoardsEnabled(false))),
      Settings.CycleMenuFontButton = new ButtonHandler.Button("Menu Font : " + Settings.CurrentFontDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleFont(true)), incrementDown: (Action) (() => Settings.CycleFont(false))),
      Settings.ChangeSoundButton = new ButtonHandler.Button("Click Sound : " + Settings.ClickSoundDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.ChangeClickSound(true)), incrementDown: (Action) (() => Settings.ChangeClickSound(false))),
      Settings.outlineModeButton = new ButtonHandler.Button("Outline : " + Settings.OutlineModes[Settings.OutlineMode], Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleOutlineMode(true)), incrementDown: (Action) (() => Settings.CycleOutlineMode(false))),
      Settings.adjustMenuSizeButton = new ButtonHandler.Button("Menu Size : " + Settings.MenuSizeDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustMenuSize(true)), incrementDown: (Action) (() => Settings.AdjustMenuSize(false))),
      Settings.adjustRoundnessButton = new ButtonHandler.Button("Roundness : " + Settings.RoundnessDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustRoundness(true)), incrementDown: (Action) (() => Settings.AdjustRoundness(false))),
      Settings.adjustOpacityButton = new ButtonHandler.Button("Opacity : " + Settings.OpacityDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustOpacity(true)), incrementDown: (Action) (() => Settings.AdjustOpacity(false))),
      Settings.adjustAccentStripTypeButton = new ButtonHandler.Button("Accent Strip : " + Settings.AccentStripTypeDescription, Category.Menu_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustAccentStripType(true)), incrementDown: (Action) (() => Settings.AdjustAccentStripType(false))),
      new ButtonHandler.Button("Colors", Category.Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Color_Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Color_Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Pinwheel", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Pinwheel)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Outline", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Outline)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Background", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Background)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Buttons", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Button)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Title", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Title)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Boards", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.Boards)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Accent Strip", Category.Color_Settings, false, false, (Action) (() => Settings.OpenElementSettings(Settings.ColorElement.AccentStrip)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Notifications", Category.Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Notification_Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Notification_Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Toggle Notifications", Category.Notification_Settings, true, true, (Action) (() => Settings.ToggleNotifications(true)), (Action) (() => Settings.ToggleNotifications(false))),
      new ButtonHandler.Button("Room Notifications", Category.Notification_Settings, true, true, (Action) (() => Settings.RoomNotifications(true)), (Action) (() => Settings.RoomNotifications(false))),
      new ButtonHandler.Button("Room Notifications Sound", Category.Notification_Settings, true, true, (Action) (() => Settings.SoundRoomNotifications(true)), (Action) (() => Settings.SoundRoomNotifications(false))),
      new ButtonHandler.Button("Gun", Category.Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Gun_Settings)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Gun_Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Settings)))
      {
        isCategory = true
      },
      Settings.cycleGunAnimationButton = new ButtonHandler.Button("Gun Animation : " + Settings.GunAnimationType, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleGunAnimation(true)), incrementDown: (Action) (() => Settings.CycleGunAnimation(false))),
      Settings.gunWiggleIntensityButton = new ButtonHandler.Button("Wiggle Intensity : " + Settings.GunWiggleIntensityDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunWiggleIntensity(true)), incrementDown: (Action) (() => Settings.AdjustGunWiggleIntensity(false))),
      Settings.gunWiggleSpeedButton = new ButtonHandler.Button("Wiggle Speed : " + Settings.GunWiggleSpeedDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunWiggleSpeed(true)), incrementDown: (Action) (() => Settings.AdjustGunWiggleSpeed(false))),
      Settings.gunIdleColorButton = new ButtonHandler.Button("Gun Idle Color : " + Settings.GunIdleColorDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunIdleColor(true)), incrementDown: (Action) (() => Settings.AdjustGunIdleColor(false))),
      Settings.gunFireColorButton = new ButtonHandler.Button("Gun Fire Color : " + Settings.GunFireColorDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunFireColor(true)), incrementDown: (Action) (() => Settings.AdjustGunFireColor(false))),
      Settings.gunHoverColorButton = new ButtonHandler.Button("Gun Hover Color : " + Settings.GunHoverColorDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunHoverColor(true)), incrementDown: (Action) (() => Settings.AdjustGunHoverColor(false))),
      Settings.gunLockColorButton = new ButtonHandler.Button("Gun Lock Color : " + Settings.GunLockColorDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunLockColor(true)), incrementDown: (Action) (() => Settings.AdjustGunLockColor(false))),
      Settings.gunPointerSizeButton = new ButtonHandler.Button("Gun Pointer Size : " + Settings.GunPointerSizeDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunPointerSize(true)), incrementDown: (Action) (() => Settings.AdjustGunPointerSize(false))),
      Settings.gunLineThicknessButton = new ButtonHandler.Button("Gun Line Thickness : " + Settings.GunLineThicknessDescription, Category.Gun_Settings, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustGunLineThickness(true)), incrementDown: (Action) (() => Settings.AdjustGunLineThickness(false))),
      new ButtonHandler.Button("Gun Line", Category.Gun_Settings, true, true, (Action) (() => Settings.GunLine(true)), (Action) (() => Settings.GunLine(false))),
      new ButtonHandler.Button("Left Hand Gun", Category.Gun_Settings, true, false, (Action) (() => GunLib.LeftHandGun = true), (Action) (() => GunLib.LeftHandGun = false)),
      new ButtonHandler.Button("Open Presets Folder", Category.Presets, false, false, (Action) (() => ButtonHandler.OpenFolder(Path.Combine(Variables.folderName, "Presets")))),
      new ButtonHandler.Button("Save Preset", Category.Presets, false, false, (Action) (() => ButtonHandler.SavePreset())),
      new ButtonHandler.Button("Saved Presets", Category.Presets, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Saved_Presets)))
      {
        isCategory = true
      },
      Settings.adjustAntiReportRadiusButton = new ButtonHandler.Button("Anti Report Radius : " + Settings.AntiReportRadiusDescription, Category.Safety, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustAntiReportRadius(true)), incrementDown: (Action) (() => Settings.AdjustAntiReportRadius(false))),
      new ButtonHandler.Button("Visualize Anti Report", Category.Safety, true, false, (Action) (() => Safety.VisualizeAntiReport()), (Action) (() => Safety.DisableVisualizeAntiReport())),
      new ButtonHandler.Button("Anti Report", Category.Safety, true, false, (Action) (() => Safety.AntiReport(false, false))),
      new ButtonHandler.Button("Join Random Anti Report", Category.Safety, true, true, (Action) (() => Safety.AntiReport(true, false))),
      new ButtonHandler.Button("Reconnect Anti Report", Category.Safety, true, false, (Action) (() => Safety.AntiReport(false, true))),
      new ButtonHandler.Button("No Finger Movement", Category.Safety, true, false, (Action) (() => Player.NoFingerMovement())),
      new ButtonHandler.Button("Change Identity", Category.Safety, false, false, (Action) (() => Safety.ChangeIDENTITY())),
      new ButtonHandler.Button("Change Identity On Disconnect", Category.Safety, true, false, (Action) (() => Safety.ChangeIdentityOnDisconnect(new Action(Safety.ChangeIdentity)))),
      new ButtonHandler.Button("Name Spoof", Category.Safety, true, false, (Action) (() => Safety.NameSpoof())),
      new ButtonHandler.Button("Color Spoof", Category.Safety, true, false, (Action) (() => Safety.ColorSpoof())),
      new ButtonHandler.Button("Ranked Platform Spoof", Category.Safety, true, false, (Action) (() => Safety.SpoofRankedPlatform(true)), (Action) (() => Safety.SpoofRankedPlatform(false))),
      new ButtonHandler.Button("Ranked Badge Spoof", Category.Safety, true, false, (Action) (() => Safety.SpoofBadge(true)), (Action) (() => Safety.SpoofBadge(false))),
      new ButtonHandler.Button("Bypass Mod Checkers", Category.Safety, true, false, (Action) (() =>
      {
        MenuPatches.propertiesEnabled = true;
        Safety.BypassModCheckers();
      }), (Action) (() => MenuPatches.propertiesEnabled = false)),
      new ButtonHandler.Button("Bypass Automod", Category.Safety, true, false, (Action) (() => Safety.BypassAutomod())),
      new ButtonHandler.Button("Spoof Platform", Category.Safety, true, false, (Action) (() => Safety.SpoofPlatform(true)), (Action) (() => Safety.SpoofPlatform(false))),
      new ButtonHandler.Button("Spoof FPS", Category.Safety, true, false, (Action) (() => Safety.SpoofFps(true)), (Action) (() => Safety.SpoofFps(false))),
      new ButtonHandler.Button("Anti Stump Kick", Category.Safety, true, false, (Action) (() => MenuPatches.GroupPatch.enabled = true), (Action) (() => MenuPatches.GroupPatch.enabled = false)),
      new ButtonHandler.Button("Anti Moderator", Category.Safety, true, false, (Action) (() => Safety.AntiModerator())),
      new ButtonHandler.Button("Anti Grab App Quit", Category.Safety, true, false, (Action) (() => Room.SetQuitBoxActive(false)), (Action) (() => Room.SetQuitBoxActive(true))),
      new ButtonHandler.Button("Anti Cheat Notifications", Category.Safety, true, false, (Action) (() => MenuPatches.AntiCheatNotifications = true), (Action) (() => MenuPatches.AntiCheatNotifications = false)),
      new ButtonHandler.Button("Accept TOS", Category.Safety, true, false, (Action) (() => MenuPatches.tosEnabled = true), (Action) (() => MenuPatches.tosEnabled = false)),
      new ButtonHandler.Button("Bypass K-ID", Category.Safety, true, false, (Action) (() => MenuPatches.kidEnabled = true), (Action) (() => MenuPatches.kidEnabled = false)),
      new ButtonHandler.Button("Fake Lag (A)", Category.Safety, true, false, (Action) (() => Safety.FakeLag()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Disable Quit Box", Category.Safety, true, false, (Action) (() => Room.SetQuitBoxActive(false)), (Action) (() => Room.SetQuitBoxActive(true))),
      new ButtonHandler.Button("Quit Game", Category.Room, false, false, new Action(Application.Quit)),
      new ButtonHandler.Button("Disconnect", Category.Room, false, false, (Action) (() => Room.Disconnect())),
      new ButtonHandler.Button("Reconnect", Category.Room, false, false, (Action) (() => Room.Reconnect())),
      new ButtonHandler.Button("Join Random Public", Category.Room, false, false, (Action) (() => Room.JoinRandomPublic())),
      new ButtonHandler.Button("Create Public", Category.Room, false, false, (Action) (() => Room.CreateRoom(Room.RandomRoomName(), true))),
      new ButtonHandler.Button("Create Private", Category.Room, false, false, (Action) (() => Room.CreateRoom(Room.RandomRoomName(), false))),
      new ButtonHandler.Button("Join Specific Room", Category.Room, false, false, (Action) (() => Room.JoinRoom())),
      new ButtonHandler.Button("Unlock VIM", Category.Room, false, false, (Action) (() => Room.UnlockVim())),
      new ButtonHandler.Button("Reauthenticate", Category.Room, false, false, (Action) (() => MothershipAuthenticator.Instance.BeginLoginFlow())),
      new ButtonHandler.Button("Disable Network Triggers", Category.Room, true, false, (Action) (() => Room.SetNetworkTriggersActive(false)), (Action) (() => Room.SetNetworkTriggersActive(true))),
      new ButtonHandler.Button("Grab All IDs", Category.Room, false, false, (Action) (() => Room.GrabAllIDs())),
      new ButtonHandler.Button("Grab Own ID", Category.Room, false, false, (Action) (() => Room.GrabSelfID())),
      new ButtonHandler.Button("Anti AFK Kick", Category.Room, true, false, (Action) (() => PhotonNetworkController.Instance.disableAFKKick = true), (Action) (() => PhotonNetworkController.Instance.disableAFKKick = false)),
      new ButtonHandler.Button("US Servers", Category.Room, false, false, (Action) (() => PhotonNetwork.ConnectToRegion("us"))),
      new ButtonHandler.Button("USW Servers", Category.Room, false, false, (Action) (() => PhotonNetwork.ConnectToRegion("usw"))),
      new ButtonHandler.Button("EU Servers", Category.Room, false, false, (Action) (() => PhotonNetwork.ConnectToRegion("eu"))),
      new ButtonHandler.Button("Queue to Default", Category.Room, false, false, (Action) (() => GorillaComputer.instance.currentQueue = "DEFAULT")),
      new ButtonHandler.Button("Queue to Minigames", Category.Room, false, false, (Action) (() => GorillaComputer.instance.currentQueue = "MINIGAMES")),
      new ButtonHandler.Button("Queue to Competitive", Category.Room, false, false, (Action) (() => GorillaComputer.instance.currentQueue = "COMPETITIVE")),
      new ButtonHandler.Button("Ghost Monke (B)", Category.Player, true, false, (Action) (() => Player.GhostMonke())),
      new ButtonHandler.Button("Invisible Monke (B)", Category.Player, true, false, (Action) (() => Player.InvisibleMonke())),
      new ButtonHandler.Button("Ghost & Invisibility (A/B)", Category.Player, true, false, (Action) (() => Player.GhostInvisibleMonke())),
      new ButtonHandler.Button("Freeze Rig (B)", Category.Player, true, false, (Action) (() => Player.FreezeRig())),
      new ButtonHandler.Button("Grab Rig (RG/LG)", Category.Player, true, false, (Action) (() => Player.GrabRig())),
      new ButtonHandler.Button("Fake Body Tracking", Category.Player, true, false, (Action) (() => Player.FakeBodyTracking())),
      Settings.adjustArmLengthButton = new ButtonHandler.Button("Long Arms Length : " + Settings.ArmLengthDescription, Category.Player, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustArmLength(true)), incrementDown: (Action) (() => Settings.AdjustArmLength(false))),
      new ButtonHandler.Button("Long Arms", Category.Player, true, false, (Action) (() => Player.LongArms(true)), (Action) (() => Player.LongArms(false))),
      new ButtonHandler.Button("Size Change (LT/RT/CS)", Category.Player, true, false, (Action) (() => Player.SizeChanger())),
      new ButtonHandler.Button("Networked Size Change (LT/RT)", Category.Player, true, false, (Action) (() => Networked.SizeChanger.NetworkedSizeChanger(true)), (Action) (() => Networked.SizeChanger.NetworkedSizeChanger(false))),
      new ButtonHandler.Button("Spin Head", Category.Player, true, false, (Action) (() => Player.SetHeadRotation(spin: true)), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("Upside Head", Category.Player, true, false, (Action) (() => Player.SetHeadRotation(new Vector3?(new Vector3(180f, 0.0f, 0.0f)))), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("Backwards Head", Category.Player, true, false, (Action) (() => Player.SetHeadRotation(new Vector3?(new Vector3(0.0f, 180f, 0.0f)))), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("Snap Neck (Left)", Category.Player, true, false, (Action) (() => Player.SetHeadRotation(new Vector3?(new Vector3(0.0f, 0.0f, 90f)))), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("Snap Neck (Right)", Category.Player, true, false, (Action) (() => Player.SetHeadRotation(new Vector3?(new Vector3(0.0f, 0.0f, -90f)))), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("T-Pose (RT)", Category.Player, true, false, (Action) (() => Player.TPose()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Lay On Back (B)", Category.Player, true, false, (Action) (() => Player.LayOnBack())),
      new ButtonHandler.Button("Lay On Stomach (B)", Category.Player, true, false, (Action) (() => Player.LayOnStomach())),
      new ButtonHandler.Button("Upside Down (RT)", Category.Player, true, false, (Action) (() => Player.UpsideDown()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Backflip (RT)", Category.Player, true, false, (Action) (() => Player.Backflip()), (Action) (() => Player.DisableBackflip())),
      new ButtonHandler.Button("Frontflip (RT)", Category.Player, true, false, (Action) (() => Player.Frontflip()), (Action) (() => Player.DisableFrontflip())),
      new ButtonHandler.Button("Cartwheel (RT)", Category.Player, true, false, (Action) (() => Player.Cartwheel()), (Action) (() => Player.DisableCartwheel())),
      new ButtonHandler.Button("Griddy (RT)", Category.Player, true, false, (Action) (() => Player.GriddyMonke()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Dance Monke (RT)", Category.Player, true, false, (Action) (() => Player.DanceMonke()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Ragdoll Monke (B)", Category.Player, true, false, (Action) (() => Player.RagdollMonke()), (Action) (() => Player.DisableRagdollMonke())),
      new ButtonHandler.Button("Spaz Monke (B)", Category.Player, true, false, (Action) (() => Player.SpazMonke()), (Action) (() => Player.ResetHeadRotation())),
      new ButtonHandler.Button("Spider Monke (B)", Category.Player, true, false, (Action) (() => Player.SpiderMonke()), (Action) (() => Player.DisableSpiderMonke())),
      new ButtonHandler.Button("Glitch Monke (B)", Category.Player, true, false, (Action) (() => Player.GlitchMonke()), (Action) (() => Player.DisableGlitchMonke())),
      new ButtonHandler.Button("Wobbly Monke (B)", Category.Player, true, false, (Action) (() => Player.WobblyMonke()), (Action) (() => Player.DisableWobblyMonke())),
      new ButtonHandler.Button("Helicopter Monke (RT)", Category.Player, true, false, (Action) (() => Player.HelicopterMonke()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Ascend Monke (RT)", Category.Player, true, false, (Action) (() => Player.AscendingMonke()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Rig Gun", Category.Player, true, false, (Action) (() => Player.RigGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Chase Gun", Category.Player, true, false, (Action) (() => Player.ChasePlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Chase All", Category.Player, true, false, (Action) (() => Player.ChaseAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Fly Towards Gun", Category.Player, true, false, (Action) (() => Player.FlyTowardsGun()), (Action) (() =>
      {
        GunLib.lockedTargetRig = (VRRig) null;
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Orbit Gun", Category.Player, true, false, (Action) (() => Player.OrbitPlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Orbit All", Category.Player, true, false, (Action) (() => Player.OrbitAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Jumpscare Gun", Category.Player, true, false, (Action) (() => Player.JumpscarePlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Jumpscare All", Category.Player, true, false, (Action) (() => Player.JumpscareAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Copy Player Gun", Category.Player, true, false, (Action) (() => Player.CopyPlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Copy All", Category.Player, true, false, (Action) (() => Player.CopyAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Mirror Player Gun", Category.Player, true, false, (Action) (() => Player.MirrorPlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Mirror All", Category.Player, true, false, (Action) (() => Player.MirrorAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Backshot Player Gun", Category.Player, true, false, (Action) (() => Player.BackshotPlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Backshot All", Category.Player, true, false, (Action) (() => Player.BackshotAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Gawk Gawk Gun", Category.Player, true, false, (Action) (() => Player.GawkGawkGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Gawk Gawk All", Category.Player, true, false, (Action) (() => Player.GawkGawkAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Piggyback Player Gun", Category.Player, true, false, (Action) (() => Player.PiggybackPlayerGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Piggyback All", Category.Player, true, false, (Action) (() => Player.PiggybackAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Only Visible To Player Gun", Category.Player, true, false, (Action) (() => Player.OnlyVisibleToPlayerGun()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        GunLib.lockedTargetRig = (VRRig) null;
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Only Invisible To Player Gun", Category.Player, true, false, (Action) (() => Player.OnlyInvisibleToPlayerGun()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        GunLib.lockedTargetRig = (VRRig) null;
        GunLib.SetGunVisibility(false);
      })),
      Settings.adjustFlySpeedButton = new ButtonHandler.Button("Fly Speed : " + Settings.FlySpeedDescription, Category.Movement, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustFlySpeed(true)), incrementDown: (Action) (() => Settings.AdjustFlySpeed(false))),
      new ButtonHandler.Button("Fly (A)", Category.Movement, true, false, (Action) (() => Movement.FlyMonke(false))),
      new ButtonHandler.Button("Accel Fly (A)", Category.Movement, true, false, (Action) (() => Movement.FlyMonke(true))),
      new ButtonHandler.Button("Fly + Noclip (A)", Category.Movement, true, false, (Action) (() => Movement.FlyNoclip())),
      new ButtonHandler.Button("Joystick Fly", Category.Movement, true, false, (Action) (() => Movement.JoystickFly())),
      new ButtonHandler.Button("WASD Movement", Category.Movement, true, false, (Action) (() => Movement.WASDMovement())),
      Settings.cyclePlatformTypeButton = new ButtonHandler.Button("Platform Type : " + Settings.PlatformType, Category.Movement, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CyclePlatformType(true)), incrementDown: (Action) (() => Settings.CyclePlatformType(false))),
      new ButtonHandler.Button("Platforms (RG/LG)", Category.Movement, true, false, (Action) (() => Movement.TogglePlatforms())),
      new ButtonHandler.Button("Use Triggers For Platforms", Category.Movement, true, false, (Action) (() => Settings.TriggerPlatforms(true)), (Action) (() => Settings.TriggerPlatforms(false))),
      new ButtonHandler.Button("Frozone (RG/LG)", Category.Movement, true, false, (Action) (() => Movement.Frozone())),
      Settings.adjustSpeedBoostButton = new ButtonHandler.Button("Boost Speed : " + Settings.SpeedDescription, Category.Movement, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustSpeedBoost(true)), incrementDown: (Action) (() => Settings.AdjustSpeedBoost(false))),
      new ButtonHandler.Button("Use Grip For Speedboost", Category.Movement, true, false, (Action) (() => Settings.GripSpeedBoost(true)), (Action) (() => Settings.GripSpeedBoost(false))),
      new ButtonHandler.Button("Speed Boost", Category.Movement, true, false, (Action) (() => Movement.SpeedBoost())),
      new ButtonHandler.Button("Dash Monke (A)", Category.Movement, true, false, (Action) (() => Movement.DashAndAirJump(true, false))),
      new ButtonHandler.Button("Uncap Max Velocity", Category.Movement, true, false, (Action) (() => Variables.playerInstance.maxJumpSpeed = 9999f)),
      new ButtonHandler.Button("Noclip (RT)", Category.Movement, true, false, (Action) (() => Movement.Noclip())),
      Settings.adjustWalkWalkStrengthButton = new ButtonHandler.Button("Wall Walk Strength : " + Settings.WalkWalkStrengthDescription, Category.Movement, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustWallWalkStrength(true)), incrementDown: (Action) (() => Settings.AdjustWallWalkStrength(false))),
      new ButtonHandler.Button("Wall Walk (RG/LG)", Category.Movement, true, false, (Action) (() => Movement.WallWalk())),
      new ButtonHandler.Button("Wall Climber (RG/LG)", Category.Movement, true, false, (Action) (() => Movement.WallClimber(true)), (Action) (() => Movement.WallClimber(false))),
      new ButtonHandler.Button("Iron Monke (RG/LG)", Category.Movement, true, false, (Action) (() => Movement.IronMonke(10))),
      new ButtonHandler.Button("Pull Mod", Category.Movement, true, false, (Action) (() => Movement.PullMod())),
      Settings.teleportMapButton = new ButtonHandler.Button("TP To : " + Settings.MapDescription, Category.Movement, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleMap(true)), incrementDown: (Action) (() => Settings.CycleMap(false))),
      new ButtonHandler.Button("Teleport To Map", Category.Movement, false, false, (Action) (() => Movement.TeleportToMap())),
      new ButtonHandler.Button("Teleport Gun", Category.Movement, true, false, (Action) (() => Player.TeleportGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Checkpoint (RG/RT)", Category.Movement, true, false, (Action) (() => Movement.PlaceCheckPoint(true)), (Action) (() => Movement.PlaceCheckPoint(false))),
      new ButtonHandler.Button("Low Gravity", Category.Movement, true, false, (Action) (() => Movement.LowGravity())),
      new ButtonHandler.Button("Zero Gravity", Category.Movement, true, false, (Action) (() => Movement.AntiGravity())),
      new ButtonHandler.Button("High Gravity", Category.Movement, true, false, (Action) (() => Movement.HighGravity())),
      new ButtonHandler.Button("Reverse Gravity", Category.Movement, true, false, (Action) (() => Movement.ReverseGravity())),
      new ButtonHandler.Button("Up And Down (LT/RT)", Category.Movement, true, false, (Action) (() => Movement.UpAndDown())),
      new ButtonHandler.Button("Fast Swim Speed", Category.Movement, true, false, (Action) (() => Movement.FastSwim())),
      new ButtonHandler.Button("Slide Control", Category.Movement, true, false, (Action) (() => Movement.SlideControl(true)), (Action) (() => Movement.SlideControl(false))),
      new ButtonHandler.Button("Anti Slip", Category.Movement, true, false, (Action) (() => Movement.GrippyHands(true)), (Action) (() => Movement.GrippyHands(false))),
      new ButtonHandler.Button("Slippy Hands", Category.Movement, true, false, (Action) (() => Movement.SlippyHands(true)), (Action) (() => Movement.SlippyHands(false))),
      new ButtonHandler.Button("No Tag Freeze", Category.Movement, true, false, (Action) (() => Variables.playerInstance.disableMovement = false)),
      new ButtonHandler.Button("Force Tag Freeze", Category.Movement, true, false, (Action) (() => Variables.playerInstance.disableMovement = true), (Action) (() => Variables.playerInstance.disableMovement = false)),
      Settings.cycleNametagTypeButton = new ButtonHandler.Button("Nametag Type : " + Settings.NametagType, Category.Visuals, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleNametagType(true)), incrementDown: (Action) (() => Settings.CycleNametagType(false))),
      new ButtonHandler.Button("Name Tags", Category.Visuals, true, false, (Action) (() => Visuals.Nametags(true)), (Action) (() => Visuals.Nametags(false))),
      new ButtonHandler.Button("NXO Nametags", Category.Visuals, true, true, (Action) (() => Visuals.NXONametags(true)), (Action) (() => Visuals.NXONametags(false))),
      new ButtonHandler.Button("Team Checked ESP", Category.Visuals, true, false, (Action) (() => Settings.IsTeamChecked(true)), (Action) (() => Settings.IsTeamChecked(false))),
      new ButtonHandler.Button("Unfilled Box ESP", Category.Visuals, true, false, (Action) (() => Visuals.ESP3D(true)), (Action) (() => Visuals.ESP3D(false))),
      new ButtonHandler.Button("Unfilled Box ESP 2D", Category.Visuals, true, false, (Action) (() => Visuals.ESP2D(true)), (Action) (() => Visuals.ESP2D(false))),
      new ButtonHandler.Button("Filled Box ESP", Category.Visuals, true, false, (Action) (() => Visuals.FilledBoxESP(true)), (Action) (() => Visuals.FilledBoxESP(false))),
      new ButtonHandler.Button("Filled Box ESP 2D", Category.Visuals, true, false, (Action) (() => Visuals.FilledBoxESP2D(true)), (Action) (() => Visuals.FilledBoxESP2D(false))),
      Settings.tracerPositionButton = new ButtonHandler.Button("Tracer Position : " + Settings.TracerPosition, Category.Visuals, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustTracerPosition(true)), incrementDown: (Action) (() => Settings.AdjustTracerPosition(false))),
      new ButtonHandler.Button("Tracers ESP", Category.Visuals, true, false, (Action) (() => Visuals.TracersESP(true)), (Action) (() => Visuals.TracersESP(false))),
      new ButtonHandler.Button("Prediction ESP", Category.Visuals, true, false, (Action) (() => Visuals.PredictionESP(true)), (Action) (() => Visuals.PredictionESP(false))),
      new ButtonHandler.Button("Bone ESP", Category.Visuals, true, false, (Action) (() => Visuals.BoneESP(true)), (Action) (() => Visuals.BoneESP(false))),
      new ButtonHandler.Button("Skeleton ESP", Category.Visuals, true, false, (Action) (() => Visuals.SkeletonESP(true)), (Action) (() => Visuals.SkeletonESP(false))),
      new ButtonHandler.Button("Chams ESP", Category.Visuals, true, false, (Action) (() => Visuals.ChamsESP(true)), (Action) (() => Visuals.ChamsESP(false))),
      new ButtonHandler.Button("Trails ESP", Category.Visuals, true, false, (Action) (() => Visuals.TrailsESP(true)), (Action) (() => Visuals.TrailsESP(false))),
      new ButtonHandler.Button("Beacons ESP", Category.Visuals, true, false, (Action) (() => Visuals.BeaconsESP(true)), (Action) (() => Visuals.BeaconsESP(false))),
      Settings.adjustFOVButton = new ButtonHandler.Button("FPC FOV : " + Settings.FOVDescription, Category.Visuals, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustFOV(true)), incrementDown: (Action) (() => Settings.AdjustFOV(false))),
      new ButtonHandler.Button("First Person Cam (PC)", Category.Visuals, true, false, (Action) (() => Visuals.FPC(true)), (Action) (() => Visuals.FPC(false))),
      new ButtonHandler.Button("Free Cam (X)", Category.Visuals, true, false, (Action) (() => Visuals.Freecam()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("VR 3rd Person (X)", Category.Visuals, true, false, (Action) (() => Visuals.VrThirdPerson()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("VR 3rd Person In Front (X)", Category.Visuals, true, false, (Action) (() => Visuals.VrThirdPersonInFront()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("Drunk Cam (X)", Category.Visuals, true, false, (Action) (() => Visuals.DrunkCam()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("Spectate Gun", Category.Visuals, true, false, (Action) (() => Visuals.SpectateGun()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("Orbit Cam (X)", Category.Visuals, true, false, (Action) (() => Visuals.OrbitCam()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("Upside Down Cam (X)", Category.Visuals, true, false, (Action) (() => Visuals.UpsideDownCam()), (Action) (() => Visuals.DisableNXOCamera())),
      new ButtonHandler.Button("Monke Sense", Category.Visuals, true, false, (Action) (() => Visuals.MonkeSense(true)), (Action) (() => Visuals.MonkeSense(false))),
      new ButtonHandler.Button("Mute Monke Sense", Category.Visuals, true, false, (Action) (() => Visuals.muteMonkeSense = true), (Action) (() => Visuals.muteMonkeSense = false)),
      new ButtonHandler.Button("Ghost Rig", Category.Visuals, true, true, (Action) (() => Settings.ToggleGhostRig(true)), (Action) (() => Settings.ToggleGhostRig(false))),
      new ButtonHandler.Button("Shiny Monkes", Category.Visuals, true, false, (Action) (() => Visuals.ShinyMonkes(true)), (Action) (() => Visuals.ShinyMonkes(false))),
      new ButtonHandler.Button("Shiny Self", Category.Visuals, true, false, (Action) (() => Visuals.ShinySelf(true)), (Action) (() => Visuals.ShinySelf(false))),
      new ButtonHandler.Button("Trippy Monkes", Category.Visuals, true, false, (Action) (() => Visuals.TrippyMonkes(true)), (Action) (() => Visuals.TrippyMonkes(false))),
      new ButtonHandler.Button("FPS Boost", Category.Visuals, true, false, (Action) (() => Visuals.FPSBoost(true)), (Action) (() => Visuals.FPSBoost(false))),
      new ButtonHandler.Button("X-Ray", Category.Visuals, true, false, (Action) (() => Visuals.ToggleXray(true)), (Action) (() => Visuals.ToggleXray(false))),
      new ButtonHandler.Button("Disable Rain", Category.Visuals, true, false, (Action) (() => Fun.RainyWeather(false)), (Action) (() => Fun.RainyWeather(true))),
      new ButtonHandler.Button("Toggle Rain", Category.Visuals, true, false, (Action) (() => Fun.RainyWeather(true)), (Action) (() => Fun.RainyWeather(false))),
      new ButtonHandler.Button("Toggle Snow", Category.Visuals, true, false, (Action) (() => Visuals.ToggleSnow(true)), (Action) (() => Visuals.ToggleSnow(false))),
      Settings.adjustTimeOfDayButton = new ButtonHandler.Button("Time Of Day : " + Settings.TimeOfDayDescription, Category.Visuals, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustTimeOfDay(true)), incrementDown: (Action) (() => Settings.AdjustTimeOfDay(false))),
      new ButtonHandler.Button("Toggle Fog", Category.Visuals, true, false, (Action) (() => Visuals.ToggleFog(true)), (Action) (() => Visuals.ToggleFog(false))),
      new ButtonHandler.Button("Fuck Colors", Category.Visuals, true, false, (Action) (() => Visuals.FuckLights(true)), (Action) (() => Visuals.FuckLights(false))),
      new ButtonHandler.Button("Acid Trip", Category.Visuals, true, false, (Action) (() => Visuals.AcidTrip(true)), (Action) (() => Visuals.AcidTrip(false))),
      new ButtonHandler.Button("90 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(90))),
      new ButtonHandler.Button("72 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(72))),
      new ButtonHandler.Button("60 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(60))),
      new ButtonHandler.Button("45 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(45))),
      new ButtonHandler.Button("30 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(30))),
      new ButtonHandler.Button("15 FPS", Category.Visuals, true, false, (Action) (() => Visuals.CapFPS(15))),
      new ButtonHandler.Button("Uncap FPS", Category.Visuals, true, false, (Action) (() => Visuals.UncapFPS(true)), (Action) (() => Visuals.UncapFPS(false))),
      new ButtonHandler.Button("Enter Soundboard", Category.Audio, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Soundboard)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Soundboard, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Audio)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Load Soundboard", Category.Soundboard, false, false, (Action) (() => Soundboard.InitializeSounds())),
      new ButtonHandler.Button("Disable All Sounds", Category.Soundboard, false, false, (Action) (() => Soundboard.SoundboardSoundsActive.Keys.ToList<string>().ForEach(new Action<string>(Soundboard.StopSound)))),
      new ButtonHandler.Button("Loop Sounds", Category.Soundboard, true, false, (Action) (() => Soundboard.EnableLoop()), (Action) (() => Soundboard.DisableLoop())),
      Settings.cycleControllerBindButton = new ButtonHandler.Button("Sound Input : " + Settings.inputName, Category.Soundboard, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.CycleControllerBind()), incrementDown: (Action) (() => Settings.CycleControllerBind(false))),
      new ButtonHandler.Button("Return", Category.SFX, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Soundboard)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Trolling, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Soundboard)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Return", Category.Songs, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Soundboard)))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Reload Microphone", Category.Audio, false, false, (Action) (() => Sound.ReloadMic())),
      new ButtonHandler.Button("Hear Self", Category.Audio, true, false, (Action) (() => Variables.taggerInstance.myRecorder.DebugEchoMode = true), (Action) (() => Variables.taggerInstance.myRecorder.DebugEchoMode = false)),
      new ButtonHandler.Button("High Quality Microphone", Category.Audio, true, false, (Action) (() => Sound.SetMicQuality(65000, 48000)), (Action) (() => Sound.SetMicQuality(20000, 16000))),
      new ButtonHandler.Button("Low Quality Microphone", Category.Audio, true, false, (Action) (() => Sound.SetMicQuality(6000, 8000)), (Action) (() => Sound.SetMicQuality(20000, 16000))),
      new ButtonHandler.Button("Reverb Microphone", Category.Audio, true, false, (Action) (() => Sound.ReverbMic(true)), (Action) (() => Sound.ReverbMic(false))),
      new ButtonHandler.Button("Loud Microphone", Category.Audio, true, false, (Action) (() => Sound.LoudMic(true)), (Action) (() => Sound.LoudMic(false))),
      new ButtonHandler.Button("Mute Microphone", Category.Audio, true, false, (Action) (() => Sound.MuteMic(true)), (Action) (() => Sound.MuteMic(false))),
      new ButtonHandler.Button("High Pitch Microphone", Category.Audio, true, false, (Action) (() => Sound.SetMicPitch(1.2f)), (Action) (() => Sound.SetMicPitch(1f))),
      new ButtonHandler.Button("Low Pitch Microphone", Category.Audio, true, false, (Action) (() => Sound.SetMicPitch(0.75f)), (Action) (() => Sound.SetMicPitch(1f))),
      new ButtonHandler.Button("Echo Microphone", Category.Audio, true, false, (Action) (() => Sound.EchoVoice(true)), (Action) (() => Sound.EchoVoice(false))),
      new ButtonHandler.Button("Static Microphone", Category.Audio, true, false, (Action) (() => Sound.RadioVoice(true)), (Action) (() => Sound.RadioVoice(false))),
      new ButtonHandler.Button("Muffled Microphone", Category.Audio, true, false, (Action) (() => Sound.UnderwaterVoice(true)), (Action) (() => Sound.UnderwaterVoice(false))),
      new ButtonHandler.Button("Random Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(UnityEngine.Random.Range(0, 228)))),
      new ButtonHandler.Button("Annoying Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(248))),
      new ButtonHandler.Button("Boop Player", Category.Audio, true, false, (Action) (() => Sound.OnTouchPlayerSoundSpam(84))),
      new ButtonHandler.Button("Gong Player", Category.Audio, true, false, (Action) (() => Sound.OnTouchPlayerSoundSpam(248))),
      new ButtonHandler.Button("Slap Player", Category.Audio, true, false, (Action) (() => Sound.OnTouchPlayerSoundSpam(338))),
      new ButtonHandler.Button("Jman HELLO! Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(337))),
      new ButtonHandler.Button("Jman Slap Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(338))),
      new ButtonHandler.Button("Jman Okay Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(336))),
      new ButtonHandler.Button("Glass Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(28))),
      new ButtonHandler.Button("Metal Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(18))),
      new ButtonHandler.Button("Pop Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(84))),
      new ButtonHandler.Button("Squeaky Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(75))),
      new ButtonHandler.Button("Crystal Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(213))),
      new ButtonHandler.Button("Turkey Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(83))),
      new ButtonHandler.Button("Frog Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(91))),
      new ButtonHandler.Button("AK-47 Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(203))),
      new ButtonHandler.Button("Wolf Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(195))),
      new ButtonHandler.Button("Cat Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(236))),
      new ButtonHandler.Button("Bee Sound Spam (RT)", Category.Audio, true, false, (Action) (() => Sound.SpecificSoundSpam(191))),
      new ButtonHandler.Button("Tag All (RT)", Category.Gamemode, true, false, (Action) (() => Gamemode.TagAll())),
      new ButtonHandler.Button("Tag Gun", Category.Gamemode, true, false, (Action) (() => Gamemode.TagGun()), (Action) (() =>
      {
        Player.SetRigStatus(true);
        GunLib.SetGunVisibility(false);
      })),
      new ButtonHandler.Button("Flick Tag (RT)", Category.Gamemode, true, false, (Action) (() => Gamemode.FlickTag()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Tag Aura", Category.Gamemode, true, false, (Action) (() => Gamemode.TagAura())),
      new ButtonHandler.Button("Tag Self", Category.Gamemode, true, false, (Action) (() => Gamemode.TagSelf()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Try Un-Tag Self", Category.Gamemode, false, false, (Action) (() => Gamemode.UntagSelf())),
      new ButtonHandler.Button("Anti Tag", Category.Gamemode, true, false, (Action) (() => Gamemode.AntiTag())),
      new ButtonHandler.Button("No Tag On Join", Category.Gamemode, true, false, (Action) (() => Gamemode.NoTagOnJoin(true)), (Action) (() => Gamemode.NoTagOnJoin(false))),
      new ButtonHandler.Button("No Tag Limit", Category.Gamemode, true, false, (Action) (() => Variables.taggerInstance.maxTagDistance = float.MaxValue), (Action) (() => Variables.taggerInstance.maxTagDistance = 1.2f)),
      new ButtonHandler.Button("Untag All (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.UntagAll())),
      new ButtonHandler.Button("Untag Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.UntagGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Tag Lag (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Overpowered.TagLag(true)), (Action) (() => Overpowered.TagLag(false))),
      new ButtonHandler.Button("Auto Guardian", Category.Gamemode, true, false, (Action) (() => Gamemode.AlwaysGuardian())),
      new ButtonHandler.Button("Guardian Grab All (RG)", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianGrabAll())),
      new ButtonHandler.Button("Guardian Release All (RT)", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianReleaseAll())),
      new ButtonHandler.Button("Guardian Orbit All (RT)", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianOrbitSelfAll())),
      new ButtonHandler.Button("Guardian Fling All (RT)", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianFlingAll())),
      new ButtonHandler.Button("Guardian Fling Gun", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianFlingGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Guardian Bring All To Pointer", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianBringAllToPointer()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Guardian Self (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.GuardianSelf())),
      new ButtonHandler.Button("Guardian All (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.GuardianAll())),
      new ButtonHandler.Button("Guardian Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.GuardianGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Un-Guardian Self (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.UnGuardianSelf())),
      new ButtonHandler.Button("Un-Guardian All (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.UnGuardianAll())),
      new ButtonHandler.Button("Un-Guardian Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.UnGuardianGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Paintbrawl Aimbot", Category.Gamemode, true, false, (Action) (() => MenuPatches.SlingshotAimbot.enabled = true), (Action) (() => MenuPatches.SlingshotAimbot.enabled = false)),
      new ButtonHandler.Button("Paintbrawl Kill All", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlKillAll())),
      new ButtonHandler.Button("Paintbrawl Kill Gun", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlKillGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Paintbrawl Kill Self", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlKillSelf())),
      new ButtonHandler.Button("Paintbrawl Godmode (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlGodmode())),
      new ButtonHandler.Button("Paintbrawl No Delay (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.NoPaintBrawlDelay(true)), (Action) (() => Gamemode.NoPaintBrawlDelay(false))),
      new ButtonHandler.Button("Paintbrawl Mat All (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlMatAll())),
      new ButtonHandler.Button("Paintbrawl Mat Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlMatGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Paintbrawl End Game (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.EndPaintbrawlGame())),
      new ButtonHandler.Button("Paintbrawl Start Game (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.StartPaintbrawlGame())),
      new ButtonHandler.Button("Paintbrawl Restart Game (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.RestartPaintbrawlGame())),
      new ButtonHandler.Button("Paintbrawl Spam Balloon All (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlSpamAllBalloons())),
      new ButtonHandler.Button("Paintbrawl Spam Balloon Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlBalloonSpamGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Paintbrawl Spam Balloon Self (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlBalloonSpamSelf())),
      new ButtonHandler.Button("Paintbrawl Revive All (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.PaintbrawlReviveAll())),
      new ButtonHandler.Button("Paintbrawl Revive Gun (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => Gamemode.PaintbrawlReviveGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Paintbrawl Revive Self (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => Gamemode.PaintbrawlReviveSelf())),
      new ButtonHandler.Button("Float Gun (Blaster)", Category.Gamemode, true, false, new Action(NXO.Mods.Categories.SuperInfection.GunFlingTest)),
      new ButtonHandler.Button("Complete Quests", Category.Gamemode, false, false, new Action(NXO.Mods.Categories.SuperInfection.CompleteAllQuests)),
      new ButtonHandler.Button("Complete & Claim Quests", Category.Gamemode, false, false, (Action) (() => NXO.Mods.Categories.SuperInfection.CompleteAllQuestsAndClaimPoints(SIPlayer.LocalPlayer))),
      new ButtonHandler.Button("Add TechPoints", Category.Gamemode, false, false, (Action) (() => NXO.Mods.Categories.SuperInfection.AddResources((SIResource.ResourceType) 0, 10))),
      new ButtonHandler.Button("Give All Resources", Category.Gamemode, false, false, (Action) (() => NXO.Mods.Categories.SuperInfection.GiveAllResourcesToMax(SIPlayer.LocalPlayer))),
      new ButtonHandler.Button("Unlock All Gadgets", Category.Gamemode, false, false, new Action(NXO.Mods.Categories.SuperInfection.UnlockAllGadgets)),
      new ButtonHandler.Button("Unlock Full Tree", Category.Gamemode, false, false, new Action(NXO.Mods.Categories.SuperInfection.UnlockFullTechTree)),
      new ButtonHandler.Button("No Blaster Cooldown", Category.Gamemode, true, false, (Action) (() =>
      {
        foreach (SIGadgetBlaster instance_ in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets().Select<SIGadget, SIGadgetBlaster>((Func<SIGadget, SIGadgetBlaster>) (x => ((Component) x).GetComponent<SIGadgetBlaster>())).Where<SIGadgetBlaster>((Func<SIGadgetBlaster, bool>) (x => (UnityEngine.Object) x != (UnityEngine.Object) null)))
          NXO.Mods.Categories.SuperInfection.RemoveBlasterCooldown(instance_);
      })),
      new ButtonHandler.Button("Max Charge", Category.Gamemode, true, false, (Action) (() =>
      {
        foreach (SIGadgetChargeBlaster instance_ in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets().Select<SIGadget, SIGadgetChargeBlaster>((Func<SIGadget, SIGadgetChargeBlaster>) (x => ((Component) x).GetComponent<SIGadgetChargeBlaster>())).Where<SIGadgetChargeBlaster>((Func<SIGadgetChargeBlaster, bool>) (x => (UnityEngine.Object) x != (UnityEngine.Object) null)))
          NXO.Mods.Categories.SuperInfection.MaxChargeAlways(instance_);
      })),
      new ButtonHandler.Button("Fast Charge", Category.Gamemode, true, false, (Action) (() =>
      {
        foreach (SIGadgetChargeBlaster instance_ in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets().Select<SIGadget, SIGadgetChargeBlaster>((Func<SIGadget, SIGadgetChargeBlaster>) (x => ((Component) x).GetComponent<SIGadgetChargeBlaster>())).Where<SIGadgetChargeBlaster>((Func<SIGadgetChargeBlaster, bool>) (x => (UnityEngine.Object) x != (UnityEngine.Object) null)))
          NXO.Mods.Categories.SuperInfection.FastCharge(instance_);
      })),
      new ButtonHandler.Button("No Cooldown (Dash)", Category.Gamemode, true, false, (Action) (() =>
      {
        foreach (SIGadgetDashYoyo instance_ in NXO.Mods.Categories.SuperInfection.GetActiveLocalPlayerGadgets().Select<SIGadget, SIGadgetDashYoyo>((Func<SIGadget, SIGadgetDashYoyo>) (x => ((Component) x).GetComponent<SIGadgetDashYoyo>())).Where<SIGadgetDashYoyo>((Func<SIGadgetDashYoyo, bool>) (x => (UnityEngine.Object) x != (UnityEngine.Object) null)))
          NXO.Mods.Categories.SuperInfection.RemoveDashCooldown(instance_);
      })),
      new ButtonHandler.Button("Clear Exclusion Zones", Category.Gamemode, false, false, new Action(NXO.Mods.Categories.SuperInfection.ClearAllExclusionZones)),
      PropHunt.CyclePropButton = new ButtonHandler.Button("Prop : " + PropHunt.CurrentPropName(), Category.Gamemode, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => PropHunt.CycleProp(true)), incrementDown: (Action) (() => PropHunt.CycleProp(false))),
      new ButtonHandler.Button("Become Prop", Category.Gamemode, false, false, (Action) (() => PropHunt.BecomeProp())),
      new ButtonHandler.Button("Skip Seeker Blindfold", Category.Gamemode, true, false, (Action) (() => PropHunt.RemoveBlindFold())),
      new ButtonHandler.Button("Hiders ESP", Category.Gamemode, true, false, (Action) (() => PropHunt.RevealHiddenProps(true)), (Action) (() => PropHunt.RevealHiddenProps(false))),
      new ButtonHandler.Button("Seekers ESP", Category.Gamemode, true, false, (Action) (() => PropHunt.SeekerESP(true)), (Action) (() => PropHunt.SeekerESP(false))),
      new ButtonHandler.Button("Prop Tag All", Category.Gamemode, true, false, (Action) (() => PropHunt.PropTagAll())),
      new ButtonHandler.Button("Force Round Start (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => PropHunt.ForceRoundStart())),
      new ButtonHandler.Button("Force Round End (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => PropHunt.ForceRoundEnd())),
      new ButtonHandler.Button("Spam Gamemode (<color=red>M</color>)", Category.Gamemode, true, false, (Action) (() => PropHunt.SpamGamemode())),
      new ButtonHandler.Button("Become Seeker (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => PropHunt.BecomeSeeker())),
      new ButtonHandler.Button("Become Hider (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => PropHunt.BecomeHider())),
      new ButtonHandler.Button("Collapse Boundary (<color=red>M</color>)", Category.Gamemode, false, false, (Action) (() => PropHunt.CollapseBoundary())),
      new ButtonHandler.Button("Snowball Fling Gun", Category.Projectiles, true, false, (Action) (() => Projectile.SnowballFlingGun()), (Action) (() =>
      {
        GunLib.SetGunVisibility(false);
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Snowball Fling All", Category.Projectiles, true, false, (Action) (() => Projectile.SnowballFlingAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Touch To Snowball Fling", Category.Projectiles, true, false, (Action) (() => Projectile.TouchToSnowballFling()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Get Touched To Snowball Fling", Category.Projectiles, true, false, (Action) (() => Projectile.GetTouchedToSnowballFling()), (Action) (() => Player.SetRigStatus(true))),
      new ButtonHandler.Button("Shoot Big Snowballs (RG)", Category.Projectiles, true, false, (Action) (() => Projectile.ShootBigSnowballs())),
      new ButtonHandler.Button("Shoot Big Snowballs Gun", Category.Projectiles, true, false, (Action) (() => Projectile.ShootBigSnowballsGun()), (Action) (() => GunLib.SetGunVisibility(false))),
      new ButtonHandler.Button("Always Big Growing Snowballs", Category.Projectiles, true, false, (Action) (() => MenuPatches.autoBig = true), (Action) (() => MenuPatches.autoBig = false)),
      new ButtonHandler.Button("Touch to Snowball Effect", Category.Projectiles, true, false, (Action) (() => Projectile.TouchToSnowballEffect())),
      new ButtonHandler.Button("Get Touched to Snowball Effect", Category.Projectiles, true, false, (Action) (() => Projectile.GetTouchedToSnowballEffect())),
      new ButtonHandler.Button("Snowball Effect Gun", Category.Projectiles, true, false, (Action) (() => Projectile.SnowballEffectPlayerGun())),
      new ButtonHandler.Button("Snowball Effect All (RT)", Category.Projectiles, true, false, (Action) (() => Projectile.SnowballEffectAll()), (Action) (() =>
      {
        MenuPatches.SerializationPatch.Override = (Func<bool>) null;
        Player.SetRigStatus(true);
      })),
      new ButtonHandler.Button("Anti Knockback", Category.Projectiles, true, false, (Action) (() => MenuPatches.knockbackEnabled = true), (Action) (() => MenuPatches.knockbackEnabled = false)),
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null
    };
    string label = Fun.cosmeticName ?? "";
    ButtonHandler.Button button;
    Fun.cosmeticsAddButton = button = new ButtonHandler.Button(label, Category.Fun, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Fun.CycleCosmetics(true)), incrementDown: (Action) (() => Fun.CycleCosmetics(false)));
    buttonArray[368] = button;
    buttonArray[369] = new ButtonHandler.Button("Add Cosmetic To Cart", Category.Fun, false, false, (Action) (() => Fun.AddCosmetic()));
    buttonArray[370] = new ButtonHandler.Button("Buy Free Cosmetics", Category.Fun, false, false, (Action) (() => Fun.BuyFreeCosmetics()));
    buttonArray[371] = new ButtonHandler.Button("Jet Pack (Networked)", Category.Fun, true, false, (Action) (() => StevesPlayground.Manager(StevesPlayground.FuncType.Jetpack, true)), (Action) (() => StevesPlayground.Manager(StevesPlayground.FuncType.Jetpack, false)));
    buttonArray[372] = new ButtonHandler.Button("Grappling Hook (RT/LT)", Category.Fun, true, false, (Action) (() => Fun.GrapplingHook(true)), (Action) (() => Fun.GrapplingHook(false)));
    buttonArray[373] = new ButtonHandler.Button("Displacer Cannon (Networked)", Category.Fun, true, false, (Action) (() => StevesPlayground.Manager(StevesPlayground.FuncType.DisplacerCannon, true)), (Action) (() => StevesPlayground.Manager(StevesPlayground.FuncType.DisplacerCannon, false)));
    buttonArray[374] = new ButtonHandler.Button("Web Shooters (RT/LT)", Category.Fun, true, false, (Action) (() => Fun.Webshooters(true)), (Action) (() => Fun.Webshooters(false)));
    buttonArray[375] = new ButtonHandler.Button("Place Bomb (RG/RT)", Category.Fun, true, false, (Action) (() => Fun.PlaceBomb(true)), (Action) (() => Fun.PlaceBomb(false)));
    buttonArray[376] = new ButtonHandler.Button("Punch Mod", Category.Fun, true, false, (Action) (() => Fun.PunchMod()));
    buttonArray[377] = new ButtonHandler.Button("Draw Mod (RG/LG)", Category.Fun, true, false, (Action) (() => Fun.DrawMod(true)), (Action) (() => Fun.DrawMod(false)));
    buttonArray[378] = new ButtonHandler.Button("Disable Wind Barriers", Category.Fun, true, false, (Action) (() => Fun.DisableWindBarriers(true)), (Action) (() => Fun.DisableWindBarriers(false)));
    buttonArray[379] = new ButtonHandler.Button("Max Quest Score", Category.Fun, false, false, (Action) (() => Variables.taggerInstance.offlineVRRig.SetQuestScore(int.MaxValue)));
    buttonArray[380] = new ButtonHandler.Button("Spam Bracelet (G)", Category.Fun, true, false, (Action) (() => Fun.BraceletSpam()));
    buttonArray[381] = new ButtonHandler.Button("Remove Bracelets", Category.Fun, false, false, (Action) (() => Fun.RemoveBracelet()));
    buttonArray[382] = new ButtonHandler.Button("Loud Hand Taps", Category.Fun, true, false, (Action) (() => Fun.LoudHandTaps()), (Action) (() => Fun.FixHandTaps()));
    buttonArray[383] = new ButtonHandler.Button("Silent Hand Taps", Category.Fun, true, false, (Action) (() => Fun.SilentHandTaps()), (Action) (() => Fun.FixHandTaps()));
    buttonArray[384] = new ButtonHandler.Button("Instant Hand Taps", Category.Fun, true, false, (Action) (() => Variables.taggerInstance.tapCoolDown = 0.0f), (Action) (() => Variables.taggerInstance.tapCoolDown = 0.33f));
    buttonArray[385] = new ButtonHandler.Button("Mute Elevator", Category.Fun, true, true, (Action) (() => Fun.ElevatorBuzz(false)), (Action) (() => Fun.ElevatorBuzz(true)));
    buttonArray[386] = new ButtonHandler.Button("Solid Water", Category.Fun, true, false, (Action) (() => Fun.ModifyWater(true, false)), (Action) (() => Fun.ModifyWater(false, false)));
    buttonArray[387] = new ButtonHandler.Button("Disable Water", Category.Fun, true, false, (Action) (() => Fun.ModifyWater(false, true)), (Action) (() => Fun.ModifyWater(false, false)));
    buttonArray[388] = new ButtonHandler.Button("Air Swim", Category.Fun, true, false, (Action) (() => Fun.AirSwim(true)), (Action) (() => Fun.AirSwim(false)));
    buttonArray[389] = new ButtonHandler.Button("Water Bender (RG/LG)", Category.Fun, true, false, (Action) (() => Fun.WaterBender()));
    buttonArray[390] = new ButtonHandler.Button("Splash Gun", Category.Fun, true, false, (Action) (() => Fun.SplashGun()), (Action) (() =>
    {
      Player.SetRigStatus(true);
      GunLib.SetGunVisibility(false);
    }));
    buttonArray[391] = new ButtonHandler.Button("Water Barrage (RT)", Category.Fun, true, false, (Action) (() => Fun.WaterBarrage()));
    buttonArray[392] = new ButtonHandler.Button("Splash Aura (RT)", Category.Fun, true, false, (Action) (() => Fun.SplashAura()));
    buttonArray[393] = new ButtonHandler.Button("Splash Self (RT)", Category.Fun, true, false, (Action) (() => Fun.SplashSelf()));
    buttonArray[394] = new ButtonHandler.Button("Grab Gliders (RG)", Category.Fun, true, false, (Action) (() => Fun.GrabGliders()));
    buttonArray[395] = new ButtonHandler.Button("Orbit Gliders (RT)", Category.Fun, true, false, (Action) (() => Fun.OrbitGliders()));
    buttonArray[396] = new ButtonHandler.Button("Spaz Gliders (RT)", Category.Fun, true, false, (Action) (() => Fun.SpazGliders()));
    buttonArray[397] = new ButtonHandler.Button("Glider Gun", Category.Fun, true, false, (Action) (() => Fun.GliderGun()), (Action) (() => GunLib.SetGunVisibility(false)));
    buttonArray[398] = new ButtonHandler.Button("Glider Aura", Category.Fun, true, false, (Action) (() => Fun.GliderAura()));
    buttonArray[399] = new ButtonHandler.Button("Fast Gliders", Category.Fun, true, false, (Action) (() => Fun.GliderSpeed(0.4f, 0.5f)), (Action) (() => Fun.GliderSpeed(0.1f, 0.2f)));
    buttonArray[400] = new ButtonHandler.Button("Slowmo Gliders", Category.Fun, true, false, (Action) (() => Fun.GliderSpeed(0.04f, 0.05f)), (Action) (() => Fun.GliderSpeed(0.1f, 0.2f)));
    buttonArray[401] = new ButtonHandler.Button("Destroy Gliders", Category.Fun, false, false, (Action) (() => Fun.DestroyGliders()));
    buttonArray[402] = new ButtonHandler.Button("Respawn Gliders", Category.Fun, false, false, (Action) (() => Fun.RespawnGliders()));
    buttonArray[403] = new ButtonHandler.Button("Spawn Hoverboard", Category.Fun, false, false, (Action) (() => Fun.SpawnHoverboard()));
    buttonArray[404] = new ButtonHandler.Button("Shoot Hoverboards (RG)", Category.Fun, true, false, (Action) (() => Fun.ShootHoverboards()));
    buttonArray[405] = new ButtonHandler.Button("Orbit Hoverboards", Category.Fun, true, false, (Action) (() => Fun.OrbitHoverboards()));
    buttonArray[406] = new ButtonHandler.Button("Hoverboard Aura", Category.Fun, true, false, (Action) (() => Fun.HoverboardAura()));
    buttonArray[407] = new ButtonHandler.Button("Hoverboard Gun", Category.Fun, true, false, (Action) (() => Fun.HoverboardGun()), (Action) (() => GunLib.SetGunVisibility(false)));
    buttonArray[408] = new ButtonHandler.Button("Spaz Hoverboard", Category.Fun, true, false, (Action) (() => Fun.SpazHoverboard()));
    buttonArray[409] = new ButtonHandler.Button("Rainbow Hoverboard", Category.Fun, true, false, (Action) (() => Fun.RainbowHoverboard()));
    buttonArray[410] = new ButtonHandler.Button("Strobe Hoverboard", Category.Fun, true, false, (Action) (() => Fun.StrobeHoverboard()));
    buttonArray[411] = new ButtonHandler.Button("Become Hoverboard", Category.Fun, true, false, (Action) (() => Fun.BecomeHoverboard()), (Action) (() =>
    {
      GTPlayer.Instance.SetHoverActive(false);
      VRRig.LocalRig.hoverboardVisual.SetNotHeld();
      Player.SetRigStatus(true);
    }));
    buttonArray[412] = new ButtonHandler.Button("Open Macros Folder", Category.Macros, false, false, (Action) (() => ButtonHandler.OpenFolder(Path.Combine(Variables.folderName, "Macros"))));
    buttonArray[413] = new ButtonHandler.Button("Record Macro (A)", Category.Macros, true, false, (Action) (() => Macros.MacroController()), (Action) (() => Macros.CleanupMacro()));
    buttonArray[414] = new ButtonHandler.Button("Recorded Macros", Category.Macros, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Recorded_Macros)))
    {
      isCategory = true
    };
    buttonArray[415] = Nextbots.nextbotSpeedButton = new ButtonHandler.Button("Nextbot Speed : " + Nextbots.NextbotSpeedDescription, Category.Nextbots, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Nextbots.CycleNextbotSpeed(true)), incrementDown: (Action) (() => Nextbots.CycleNextbotSpeed(false)));
    buttonArray[416] = Nextbots.nextbotBehaviourButton = new ButtonHandler.Button($"Behaviour : {Nextbots.CurrentBehaviour}", Category.Nextbots, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Nextbots.CycleNextbotBehaviour(true)), incrementDown: (Action) (() => Nextbots.CycleNextbotBehaviour(false)));
    buttonArray[417] = new ButtonHandler.Button("Custom Nextbots", Category.Nextbots, false, false, (Action) (() =>
    {
      CustomNextbots.currentOpen = (string) null;
      ButtonHandler.ChangePage(Category.Custom_Nextbots);
    }))
    {
      isCategory = true
    };
    buttonArray[418] = new ButtonHandler.Button("Crash On Jumpscare", Category.Nextbots, true, false, (Action) (() => Nextbots.CrashOnJumpscare = true), (Action) (() => Nextbots.CrashOnJumpscare = false));
    buttonArray[419] = new ButtonHandler.Button("Clear Nextbots", Category.Nextbots, false, false, (Action) (() => Nextbots.ClearNextbots()));
    buttonArray[420] = new ButtonHandler.Button("Sanic", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_sanic.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/sanic.mp3", name: "Sanic")));
    buttonArray[421] = new ButtonHandler.Button("Obunga", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_obunga.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/obunga.mp3", name: "Obunga")));
    buttonArray[422] = new ButtonHandler.Button("Angry Munci", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_angrymunci.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/angry%20munci.mp3", name: "Angry Munci")));
    buttonArray[423] = new ButtonHandler.Button("Selene Delgado", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_delgado.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/selene%20delgado.mp3", name: "Selene Delgado")));
    buttonArray[424] = new ButtonHandler.Button("Aheno", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_aheno.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/aheno.mp3", name: "Aheno")));
    buttonArray[425] = new ButtonHandler.Button("Yoshie", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_yoshie.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/yoshie.mp3", name: "Yoshie")));
    buttonArray[426] = new ButtonHandler.Button("This Man", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_thisman.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/this%20man.mp3", name: "This Man")));
    buttonArray[427] = new ButtonHandler.Button("IShowSpeed", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_speed.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/ishowspeed.mp3", name: "Speed")));
    buttonArray[428] = new ButtonHandler.Button("Jungler", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_jungler.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/jungler.mp3", name: "Jungler")));
    buttonArray[429] = new ButtonHandler.Button("PBJ Banana", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Gif_pbj.gif", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/peanutbutterjelly.mp3", name: "PBJ Banana")));
    buttonArray[430] = new ButtonHandler.Button("Imposter", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_impostor.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/imposter.mp3", name: "Imposter")));
    buttonArray[431] = new ButtonHandler.Button("Geoffrey", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_geoffery.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/geoffrey.mp3", name: "Geoffrey")));
    buttonArray[432] = new ButtonHandler.Button("The Boiled One", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_theboiledone.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/theboiledone.mp3", name: "The Boiled One")));
    buttonArray[433] = new ButtonHandler.Button("Afton", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_afton.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/afton.mp3", name: "Afton")));
    buttonArray[434] = new ButtonHandler.Button("Trollge", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Gif_trollge.gif", "https://us-tuna-sounds-files.voicemod.net/53ede07c-9315-4f31-aa65-1daeb19a3883-1655088334406.mp3", name: "Trollge", jumpscareUrl: "https://us-tuna-sounds-files.voicemod.net/4bd5aaf2-4a7c-4730-b582-53b1ecdc5a2b-1776298464475.mp3")));
    buttonArray[435] = new ButtonHandler.Button("Glimpse", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_glimpse.png", "", name: "Glimpse", jumpscareUrl: "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/glimpsejumpscare.mp3")));
    buttonArray[436] = new ButtonHandler.Button("Happy", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_happy.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/happy.mp3", name: "Happy")));
    buttonArray[437] = new ButtonHandler.Button("Nerd Emoji", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_nerd.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/nerd.mp3", name: "Nerd Emoji")));
    buttonArray[438] = new ButtonHandler.Button("Firebrand", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_firebrand.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/firebrand.mp3", name: "Firebrand")));
    buttonArray[439] = new ButtonHandler.Button("Pinhead", Category.Nextbots, false, false, (Action) (() => Nextbots.SpawnNextbot("https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Images/Nextbot_pinhead.png", "https://raw.githubusercontent.com/NuggetGT/NXO-Resources/main/Nextbots/Nextbot%20Sounds/pinhead.mp3", name: "Pinhead")));
    buttonArray[440] = Settings.adjustLagTypeButton = new ButtonHandler.Button("Lag Type : " + Settings.LagTypeDescription, Category.Overpowered, false, false, (Action) null, incremental: true, incrementUp: (Action) (() => Settings.AdjustLagType(true)), incrementDown: (Action) (() => Settings.AdjustLagType(false)));
    buttonArray[441] = new ButtonHandler.Button("Lag All (RT)", Category.Overpowered, true, false, (Action) (() => Overpowered.LagAll()));
    buttonArray[442] = new ButtonHandler.Button("Lag Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.LagGun()));
    buttonArray[443] = new ButtonHandler.Button("Touch To Lag", Category.Overpowered, true, false, (Action) (() => Overpowered.TouchToLag()));
    buttonArray[444] = new ButtonHandler.Button("Get Touched To Lag", Category.Overpowered, true, false, (Action) (() => Overpowered.GetTouchedToLag()));
    buttonArray[445] = new ButtonHandler.Button("Lag Aura", Category.Overpowered, true, false, (Action) (() => Overpowered.LagAura()));
    buttonArray[446] = new ButtonHandler.Button("Destroy Player Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.DestroyGun()));
    buttonArray[447] = new ButtonHandler.Button("Destroy All", Category.Overpowered, true, false, (Action) (() => Overpowered.DestroyAll()));
    buttonArray[448] = new ButtonHandler.Button("Deafen Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.DeafenGun()));
    buttonArray[449] = new ButtonHandler.Button("Deafen All", Category.Overpowered, true, false, (Action) (() => Overpowered.DeafenAll()));
    buttonArray[450] = new ButtonHandler.Button("Touch To Deafen", Category.Overpowered, true, false, (Action) (() => Overpowered.TouchToDeafen()));
    buttonArray[451] = new ButtonHandler.Button("Get Touched To Deafen", Category.Overpowered, true, false, (Action) (() => Overpowered.GetTouchedToDeafen()));
    buttonArray[452] = new ButtonHandler.Button("Spy Room", Category.Overpowered, true, false, (Action) (() => Overpowered.SpyRoom(true)), (Action) (() => Overpowered.SpyRoom(false)));
    buttonArray[453] = new ButtonHandler.Button("Unban Voice Self", Category.Overpowered, false, false, (Action) (() => Overpowered.Mute(VRRig.LocalRig, false)));
    buttonArray[454] = new ButtonHandler.Button("Lock Room", Category.Overpowered, false, false, (Action) (() => Overpowered.SetRoomStatus(false)));
    buttonArray[455] = new ButtonHandler.Button("Unlock Room", Category.Overpowered, false, false, (Action) (() => Overpowered.SetRoomStatus(true)));
    buttonArray[456] = new ButtonHandler.Button("Try-On Cosmetics Anywhere", Category.Overpowered, true, false, (Action) (() => Fun.TryCosmeticAnywhere(true)), (Action) (() => Fun.TryCosmeticAnywhere(false)));
    buttonArray[457] = new ButtonHandler.Button("SS CosmetiX (City)", Category.Overpowered, true, false, (Action) (() => Fun.ServerSidedCosmetics(true)), (Action) (() => Fun.ServerSidedCosmetics(false)));
    buttonArray[458] = new ButtonHandler.Button("Grab Fling Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.GrabFlingGun()), (Action) (() =>
    {
      Overpowered.StopGrabGun();
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[459] = new ButtonHandler.Button("Grab Fling All", Category.Overpowered, true, false, (Action) (() => Overpowered.GrabFlingAll()), (Action) (() =>
    {
      Overpowered.StopGrabGun();
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[460] = new ButtonHandler.Button("Grab Shake Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.GrabShakeGun()), (Action) (() =>
    {
      Overpowered.StopGrabGun();
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[461] = new ButtonHandler.Button("Grab Shake All", Category.Overpowered, true, false, (Action) (() => Overpowered.GrabShakeAll()), (Action) (() =>
    {
      Overpowered.StopGrabGun();
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[462] = new ButtonHandler.Button("Stump Kick To Specific Room", Category.Overpowered, true, false, (Action) (() =>
    {
      if (SearchAndKeyboard.isSearching)
        return;
      Overpowered.SetKickRoom();
    }), (Action) (() => Overpowered.ClearKickRoom()));
    buttonArray[463] = new ButtonHandler.Button("Fast Stump Kick", Category.Overpowered, true, false, (Action) (() => Room.instantCreate = true), (Action) (() => Room.instantCreate = false));
    buttonArray[464] = new ButtonHandler.Button("Stump Kick Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.StumpKickGun()));
    buttonArray[465] = new ButtonHandler.Button("Stump Kick All", Category.Overpowered, false, false, (Action) (() => Overpowered.StumpKickAll()));
    buttonArray[466] = new ButtonHandler.Button("Glider Annoy All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.GliderBlindAll()));
    buttonArray[467] = new ButtonHandler.Button("Glider Annoy Gun", Category.Overpowered, true, false, (Action) (() => Fun.GliderBlindGun()), (Action) (() => GunLib.SetGunVisibility(false)));
    buttonArray[468] = new ButtonHandler.Button("Earrape Gun", Category.Overpowered, true, false, (Action) (() => Overpowered.EarrapeGun()), (Action) (() =>
    {
      Sound.Earrape(false);
      Overpowered._earrapeGunActive = false;
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
    }));
    buttonArray[469] = new ButtonHandler.Button("Earrape All", Category.Overpowered, true, false, (Action) (() => Overpowered.EarrapeAll()), (Action) (() =>
    {
      Sound.Earrape(false);
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
    }));
    buttonArray[470] = new ButtonHandler.Button("Splash Annoy Gun", Category.Overpowered, true, false, (Action) (() => Fun.SplashAnnoyGun()), (Action) (() =>
    {
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[471] = new ButtonHandler.Button("Splash Annoy All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.SplashAnnoyAll()), (Action) (() => MenuPatches.SerializationPatch.Override = (Func<bool>) null));
    buttonArray[472] = new ButtonHandler.Button("Seizure Screen Gun", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenGun((double) Time.time % 0.20000000298023224 > 0.10000000149011612 ? Color.red : Color.cyan)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[473] = new ButtonHandler.Button("Seizure Screen All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenAll((double) Time.time % 0.20000000298023224 > 0.10000000149011612 ? Color.red : Color.cyan)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      Player.SetRigStatus(true);
    }));
    buttonArray[474] = new ButtonHandler.Button("Strobe Screen Gun", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenGun(Variables.RandomColor())), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[475] = new ButtonHandler.Button("Strobe Screen All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenAll(Variables.RandomColor())), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      Player.SetRigStatus(true);
    }));
    buttonArray[476] = new ButtonHandler.Button("Flash Screen Gun", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenGun((double) Time.time % 0.20000000298023224 > 0.10000000149011612 ? Color.white : Color.black)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[477] = new ButtonHandler.Button("Flash Screen All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenAll((double) Time.time % 0.20000000298023224 > 0.10000000149011612 ? Color.white : Color.black)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      Player.SetRigStatus(true);
    }));
    buttonArray[478] = new ButtonHandler.Button("Black Screen Gun", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenGun(Color.black)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[479] = new ButtonHandler.Button("Black Screen All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenAll(Color.black)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      Player.SetRigStatus(true);
    }));
    buttonArray[480] = new ButtonHandler.Button("White Screen Gun", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenGun(Color.white)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      GunLib.SetGunVisibility(false);
      Player.SetRigStatus(true);
    }));
    buttonArray[481] = new ButtonHandler.Button("White Screen All (RT)", Category.Overpowered, true, false, (Action) (() => Fun.HoverboardScreenAll(Color.white)), (Action) (() =>
    {
      MenuPatches.SerializationPatch.Override = (Func<bool>) null;
      Player.SetRigStatus(true);
    }));
    buttonArray[482] = new ButtonHandler.Button("Enable Event (<color=red>M</color>)", Category.Overpowered, false, false, (Action) (() =>
    {
      if (!Variables.IsMaster())
        return;
      GreyZoneManager.Instance.ActivateGreyZoneAuthority();
    }));
    buttonArray[483] = new ButtonHandler.Button("Disable Event (<color=red>M</color>)", Category.Overpowered, false, false, (Action) (() =>
    {
      if (!Variables.IsMaster())
        return;
      GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
    }));
    buttonArray[484] = new ButtonHandler.Button("Spam Event (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SpamEvent()));
    buttonArray[485] = new ButtonHandler.Button("Infinite Event (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() =>
    {
      if (!Variables.IsMaster())
        return;
      GreyZoneManager.Instance.greyZoneActiveDuration = float.MaxValue;
    }), (Action) (() =>
    {
      if (!Variables.IsMaster())
        return;
      GreyZoneManager.Instance.greyZoneActiveDuration = 90f;
    }));
    buttonArray[486] = new ButtonHandler.Button("Slow All (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SlowAll()));
    buttonArray[487] = new ButtonHandler.Button("Slow Gun (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SlowGun()));
    buttonArray[488] = new ButtonHandler.Button("Slow Aura (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SlowAura()));
    buttonArray[489] = new ButtonHandler.Button("Slow On Touch (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SlowOnTouch()));
    buttonArray[490] = new ButtonHandler.Button("Vibrate All (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.VibrateAll()));
    buttonArray[491] = new ButtonHandler.Button("Vibrate Gun (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.VibrateGun()));
    buttonArray[492] = new ButtonHandler.Button("Vibrate Aura (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.VibrateAura()));
    buttonArray[493] = new ButtonHandler.Button("Vibrate On Touch (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.VibrateOnTouch()));
    buttonArray[494] = new ButtonHandler.Button("Mat Spam All (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.MatSpamAll()));
    buttonArray[495] = new ButtonHandler.Button("Mat Spam Gun (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.MatSpamGun()));
    buttonArray[496] = new ButtonHandler.Button("Touch To Mat Spam (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.TouchToMatSpam()));
    buttonArray[497] = new ButtonHandler.Button("Get Touched To Mat Spam (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.GetTouchedToMatSpam()));
    buttonArray[498] = new ButtonHandler.Button("Spaz All Targets (<color=red>M</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.SpazAllTargets()));
    buttonArray[499] = new ButtonHandler.Button("Tagged Sound (<color=red>M/RT</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.RoomSoundSpam(0)));
    buttonArray[500] = new ButtonHandler.Button("Round End Sound (<color=red>M/RT</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.RoomSoundSpam(2)));
    buttonArray[501] = new ButtonHandler.Button("Bonk Sound (<color=red>M/RT</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.RoomSoundSpam(4)));
    buttonArray[502] = new ButtonHandler.Button("Count Sound (<color=red>M/RT</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.RoomSoundSpam(1)));
    buttonArray[503] = new ButtonHandler.Button("Brawl Sound (<color=red>M/RT</color>)", Category.Overpowered, true, false, (Action) (() => Overpowered.RoomSoundSpam(7)));
    buttonArray[504] = new ButtonHandler.Button("Return", Category.Gear_Menu, false, false, (Action) (() =>
    {
      if (ButtonHandler._gearTargetButton != null)
        ButtonHandler.ChangePage(ButtonHandler._gearTargetButton.Page);
      else
        ButtonHandler.ChangePage(Category.Home);
    }))
    {
      isCategory = true
    };
    buttonArray[505] = new ButtonHandler.Button("Tooltip", Category.Gear_Menu, false, false, (Action) (() =>
    {
      if (ButtonHandler._gearTargetButton?.tooltip == null)
        return;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Info, ButtonHandler._gearTargetButton.tooltip);
    }));
    ModButtons._buttons = buttonArray;
  }
}
