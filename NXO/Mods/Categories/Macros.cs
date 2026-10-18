
using NXO.Menu;
using NXO.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Macros
{
  public static bool isRecordingMacro = false;
  public static bool isPlayingMacro = false;
  private static bool loopMacro = false;
  private static int macroPlaybackIndex = 0;
  private static List<Macros.MacroFrame> currentRecording = new List<Macros.MacroFrame>();
  private static List<Macros.MacroFrame> currentPlayback = (List<Macros.MacroFrame>) null;
  public static string currentOpenMacro = (string) null;
  public static Dictionary<string, List<ButtonHandler.Button>> macroCategoryButtons = new Dictionary<string, List<ButtonHandler.Button>>();
  private const int MAX_FRAMES = 10800;
  private static bool lastPrimaryState = false;

  private static string macroSavePath => Path.Combine(Variables.folderName, nameof (Macros));

  public static void MacroController()
  {
    if (Macros.isPlayingMacro)
      return;
    bool flag1 = InputHandler.RPrimary();
    bool flag2 = flag1 && !Macros.lastPrimaryState;
    Macros.lastPrimaryState = flag1;
    if (flag2)
    {
      if (!Macros.isRecordingMacro)
        Macros.StartRecording();
      else
        Macros.StopRecording();
    }
    if (!Macros.isRecordingMacro)
      return;
    Macros.RecordFrame();
  }

  public static void MacroPlaybackTick()
  {
    if (!Macros.isPlayingMacro)
      return;
    Macros.PlaybackMacroFrame();
  }

  public static void CleanupMacro()
  {
    if (Macros.isPlayingMacro)
      Macros.StopMacroPlayback();
    if (!Macros.isRecordingMacro)
      return;
    Macros.isRecordingMacro = false;
    Macros.currentRecording.Clear();
  }

  private static void StartRecording()
  {
    Macros.currentRecording.Clear();
    Macros.isRecordingMacro = true;
    NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, "Macro Recording...");
  }

  private static void StopRecording()
  {
    Macros.isRecordingMacro = false;
    if (Macros.currentRecording.Count == 0)
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "No Frames Recorded");
    }
    else
    {
      string name = $"Macro_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
      Macros.SaveMacro(name, Macros.currentRecording);
      Macros.GenerateMacroButtons();
      NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"{name} ({Macros.currentRecording.Count} frames)");
      Macros.currentRecording.Clear();
      Main.RefreshMenu();
    }
  }

  private static void RecordFrame()
  {
    if (Macros.currentRecording.Count >= 10800)
    {
      Macros.StopRecording();
    }
    else
    {
      VRRig offlineVrRig = Variables.taggerInstance.offlineVRRig;
      if ((UnityEngine.Object) offlineVrRig == (UnityEngine.Object) null)
        return;
      Macros.currentRecording.Add(new Macros.MacroFrame()
      {
        rigPosition = ((Component) offlineVrRig).transform.position,
        rigRotation = ((Component) offlineVrRig).transform.rotation,
        headPosition = offlineVrRig.head.rigTarget.position,
        headRotation = offlineVrRig.head.rigTarget.rotation,
        leftHandPosition = offlineVrRig.leftHand.rigTarget.position,
        leftHandRotation = offlineVrRig.leftHand.rigTarget.rotation,
        rightHandPosition = offlineVrRig.rightHand.rigTarget.position,
        rightHandRotation = offlineVrRig.rightHand.rigTarget.rotation,
        leftIndexT = offlineVrRig.leftIndex != null ? ((VRMap) offlineVrRig.leftIndex).calcT : 0.0f,
        leftMiddleT = offlineVrRig.leftMiddle != null ? ((VRMap) offlineVrRig.leftMiddle).calcT : 0.0f,
        leftThumbT = offlineVrRig.leftThumb != null ? ((VRMap) offlineVrRig.leftThumb).calcT : 0.0f,
        rightIndexT = offlineVrRig.rightIndex != null ? ((VRMap) offlineVrRig.rightIndex).calcT : 0.0f,
        rightMiddleT = offlineVrRig.rightMiddle != null ? ((VRMap) offlineVrRig.rightMiddle).calcT : 0.0f,
        rightThumbT = offlineVrRig.rightThumb != null ? ((VRMap) offlineVrRig.rightThumb).calcT : 0.0f
      });
    }
  }

  public static void StartMacroPlayback(string macroName)
  {
    List<Macros.MacroFrame> macroFrameList = Macros.LoadMacro(macroName);
    if (macroFrameList == null || macroFrameList.Count == 0)
    {
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Failed To Load Macro");
    }
    else
    {
      Macros.currentPlayback = macroFrameList;
      Macros.macroPlaybackIndex = 0;
      Macros.isPlayingMacro = true;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, macroName);
    }
  }

  private static void PlaybackMacroFrame()
  {
    if (Macros.currentPlayback == null || Macros.macroPlaybackIndex >= Macros.currentPlayback.Count)
    {
      if (Macros.loopMacro && Macros.currentPlayback != null)
        Macros.macroPlaybackIndex = 0;
      else
        Macros.StopMacroPlayback();
    }
    else
    {
      VRRig offlineVrRig = Variables.taggerInstance.offlineVRRig;
      ((Behaviour) offlineVrRig).enabled = false;
      Macros.MacroFrame macroFrame = Macros.currentPlayback[Macros.macroPlaybackIndex];
      ((Component) offlineVrRig).transform.SetPositionAndRotation(macroFrame.rigPosition, macroFrame.rigRotation);
      offlineVrRig.head.rigTarget.SetPositionAndRotation(macroFrame.headPosition, macroFrame.headRotation);
      offlineVrRig.leftHand.rigTarget.SetPositionAndRotation(macroFrame.leftHandPosition, macroFrame.leftHandRotation);
      offlineVrRig.rightHand.rigTarget.SetPositionAndRotation(macroFrame.rightHandPosition, macroFrame.rightHandRotation);
      if (offlineVrRig.leftIndex != null)
      {
        ((VRMap) offlineVrRig.leftIndex).calcT = macroFrame.leftIndexT;
        ((VRMap) offlineVrRig.leftIndex).LerpFinger(1f, false);
      }
      if (offlineVrRig.leftMiddle != null)
      {
        ((VRMap) offlineVrRig.leftMiddle).calcT = macroFrame.leftMiddleT;
        ((VRMap) offlineVrRig.leftMiddle).LerpFinger(1f, false);
      }
      if (offlineVrRig.leftThumb != null)
      {
        ((VRMap) offlineVrRig.leftThumb).calcT = macroFrame.leftThumbT;
        ((VRMap) offlineVrRig.leftThumb).LerpFinger(1f, false);
      }
      if (offlineVrRig.rightIndex != null)
      {
        ((VRMap) offlineVrRig.rightIndex).calcT = macroFrame.rightIndexT;
        ((VRMap) offlineVrRig.rightIndex).LerpFinger(1f, false);
      }
      if (offlineVrRig.rightMiddle != null)
      {
        ((VRMap) offlineVrRig.rightMiddle).calcT = macroFrame.rightMiddleT;
        ((VRMap) offlineVrRig.rightMiddle).LerpFinger(1f, false);
      }
      if (offlineVrRig.rightThumb != null)
      {
        ((VRMap) offlineVrRig.rightThumb).calcT = macroFrame.rightThumbT;
        ((VRMap) offlineVrRig.rightThumb).LerpFinger(1f, false);
      }
      ++Macros.macroPlaybackIndex;
    }
  }

  private static void StopMacroPlayback()
  {
    Macros.isPlayingMacro = false;
    Macros.loopMacro = false;
    if (Macros.currentPlayback != null && Macros.currentPlayback.Count > 0 && Macros.macroPlaybackIndex > 0)
      Movement.TeleportTo(Macros.currentPlayback[Macros.macroPlaybackIndex - 1].rigPosition);
    Macros.currentPlayback = (List<Macros.MacroFrame>) null;
    Macros.macroPlaybackIndex = 0;
    if ((UnityEngine.Object) Variables.taggerInstance?.offlineVRRig != (UnityEngine.Object) null)
      ((Behaviour) Variables.taggerInstance.offlineVRRig).enabled = true;
    NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, "Macro Stopped");
  }

  private static void SaveMacro(string name, List<Macros.MacroFrame> frames)
  {
    if (!Directory.Exists(Macros.macroSavePath))
      Directory.CreateDirectory(Macros.macroSavePath);
    using (BinaryWriter w = new BinaryWriter((Stream) File.Open(Path.Combine(Macros.macroSavePath, name + ".macro"), FileMode.Create)))
    {
      w.Write(frames.Count);
      foreach (Macros.MacroFrame frame in frames)
      {
        Macros.WriteVector3(w, frame.rigPosition);
        Macros.WriteQuaternion(w, frame.rigRotation);
        Macros.WriteVector3(w, frame.headPosition);
        Macros.WriteQuaternion(w, frame.headRotation);
        Macros.WriteVector3(w, frame.leftHandPosition);
        Macros.WriteQuaternion(w, frame.leftHandRotation);
        Macros.WriteVector3(w, frame.rightHandPosition);
        Macros.WriteQuaternion(w, frame.rightHandRotation);
        w.Write(frame.leftIndexT);
        w.Write(frame.leftMiddleT);
        w.Write(frame.leftThumbT);
        w.Write(frame.rightIndexT);
        w.Write(frame.rightMiddleT);
        w.Write(frame.rightThumbT);
      }
    }
  }

  private static List<Macros.MacroFrame> LoadMacro(string name)
  {
    string path = Path.Combine(Macros.macroSavePath, name + ".macro");
    if (!File.Exists(path))
      return (List<Macros.MacroFrame>) null;
    List<Macros.MacroFrame> macroFrameList = new List<Macros.MacroFrame>();
    using (BinaryReader r = new BinaryReader((Stream) File.Open(path, FileMode.Open)))
    {
      int num = r.ReadInt32();
      for (int index = 0; index < num; ++index)
        macroFrameList.Add(new Macros.MacroFrame()
        {
          rigPosition = Macros.ReadVector3(r),
          rigRotation = Macros.ReadQuaternion(r),
          headPosition = Macros.ReadVector3(r),
          headRotation = Macros.ReadQuaternion(r),
          leftHandPosition = Macros.ReadVector3(r),
          leftHandRotation = Macros.ReadQuaternion(r),
          rightHandPosition = Macros.ReadVector3(r),
          rightHandRotation = Macros.ReadQuaternion(r),
          leftIndexT = r.ReadSingle(),
          leftMiddleT = r.ReadSingle(),
          leftThumbT = r.ReadSingle(),
          rightIndexT = r.ReadSingle(),
          rightMiddleT = r.ReadSingle(),
          rightThumbT = r.ReadSingle()
        });
    }
    return macroFrameList;
  }

  private static void WriteVector3(BinaryWriter w, Vector3 v)
  {
    w.Write(v.x);
    w.Write(v.y);
    w.Write(v.z);
  }

  private static void WriteQuaternion(BinaryWriter w, Quaternion q)
  {
    w.Write(q.x);
    w.Write(q.y);
    w.Write(q.z);
    w.Write(q.w);
  }

  private static Vector3 ReadVector3(BinaryReader r)
  {
    return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
  }

  private static Quaternion ReadQuaternion(BinaryReader r)
  {
    return new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
  }

  public static void GenerateMacroButtons()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).ToList<ButtonHandler.Button>();
    list.RemoveAll((Predicate<ButtonHandler.Button>) (b => b.Page == Category.Recorded_Macros));
    list.Add(new ButtonHandler.Button("Return", Category.Recorded_Macros, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Macros)))
    {
      isCategory = true
    });
    if (Directory.Exists(Macros.macroSavePath))
    {
      foreach (string file in Directory.GetFiles(Macros.macroSavePath, "*.macro"))
      {
        string macroName = Path.GetFileNameWithoutExtension(file);
        list.Add(new ButtonHandler.Button(macroName, Category.Recorded_Macros, false, false, (Action) (() => Macros.OpenMacroOptions(macroName))));
      }
    }
    ModButtons.buttons = list.ToArray();
  }

  private static void OpenMacroOptions(string macroName)
  {
    Macros.currentOpenMacro = macroName;
    Macros.macroCategoryButtons[macroName] = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Recorded_Macros, false, false, (Action) (() =>
      {
        Macros.currentOpenMacro = (string) null;
        Main.RefreshMenu();
      }))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Play Macro", Category.Recorded_Macros, true, false, (Action) (() => Macros.StartMacroPlayback(macroName)), (Action) (() => Macros.StopMacroPlayback())),
      new ButtonHandler.Button("Loop Macro", Category.Recorded_Macros, true, false, (Action) (() =>
      {
        Macros.loopMacro = true;
        Macros.StartMacroPlayback(macroName);
      }), (Action) (() =>
      {
        Macros.loopMacro = false;
        if (!Macros.isPlayingMacro)
          return;
        Macros.StopMacroPlayback();
      })),
      new ButtonHandler.Button("Rename Macro", Category.Recorded_Macros, false, false, (Action) (() => Macros.BeginRenameMacro(macroName))),
      new ButtonHandler.Button("Delete Macro", Category.Recorded_Macros, false, false, (Action) (() => Macros.DeleteMacro(macroName)))
    };
    Main._lastPageDrawn = Category.Home;
    Main._lastDrawModVersion = -1;
    Main.RedrawButtonList();
  }

  private static void DeleteMacro(string macroName)
  {
    string path = Path.Combine(Macros.macroSavePath, macroName + ".macro");
    if (File.Exists(path))
      File.Delete(path);
    Macros.macroCategoryButtons.Remove(macroName);
    Macros.currentOpenMacro = (string) null;
    Macros.GenerateMacroButtons();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, macroName);
    Main.RefreshMenu();
  }

  private static void BeginRenameMacro(string macroName)
  {
    SearchAndKeyboard.OpenTypingKeyboard(macroName, "Enter macro name...");
    SearchAndKeyboard.onTypingComplete = (Action<string>) (newName =>
    {
      newName = newName.Trim();
      if (string.IsNullOrEmpty(newName) || newName == macroName)
        return;
      string str1 = Path.Combine(Macros.macroSavePath, macroName + ".macro");
      string str2 = Path.Combine(Macros.macroSavePath, newName + ".macro");
      if (!File.Exists(str1))
        return;
      if (File.Exists(str2))
      {
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Macro Name Already Exists");
      }
      else
      {
        File.Move(str1, str2);
        if (Macros.macroCategoryButtons.ContainsKey(macroName))
        {
          Macros.macroCategoryButtons[newName] = Macros.macroCategoryButtons[macroName];
          Macros.macroCategoryButtons.Remove(macroName);
        }
        Macros.currentOpenMacro = (string) null;
        Macros.GenerateMacroButtons();
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"Renamed {macroName} → {newName}");
        Main.RedrawButtonList();
      }
    });
  }

  public struct MacroFrame
  {
    public Vector3 rigPosition;
    public Quaternion rigRotation;
    public Vector3 headPosition;
    public Quaternion headRotation;
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;
    public Vector3 rightHandPosition;
    public Quaternion rightHandRotation;
    public float leftIndexT;
    public float leftMiddleT;
    public float leftThumbT;
    public float rightIndexT;
    public float rightMiddleT;
    public float rightThumbT;
  }
}
