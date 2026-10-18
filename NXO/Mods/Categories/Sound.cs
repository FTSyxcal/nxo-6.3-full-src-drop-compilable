
using GorillaExtensions;
using NXO.Utilities;
using Photon.Pun;
using Photon.Voice;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using POpusCodec.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Sound
{
  public static bool haschangedint = false;
  public static bool haschangedint2 = false;
  public static int sound = 0;
  private static bool lastLeftTouch = false;
  private static bool lastRightTouch = false;
  private static float soundCooldown = 0.175f;

  public static string GetSoundName(int index)
  {
    switch (index)
    {
      case 0:
        return "Default";
      case 1:
        return "Rock Wall";
      case 2:
        return "Canyon";
      case 3:
        return "Pillow White";
      case 4:
        return "Pillow Blue";
      case 5:
        return "Pillow Yellow";
      case 6:
        return "Pillow Pink";
      case 7:
        return "Pit Ground";
      case 8:
        return "Bark";
      case 9:
        return "Log Low Resolution";
      case 10:
        return "Table";
      case 11:
        return "Dark Brown Panel";
      case 12:
        return "Log Support";
      case 13:
        return "Wooden Support";
      case 14:
        return "Mine Wall";
      case 15:
        return "Picnic Table";
      case 16 /*0x10*/:
        return "Crate";
      case 17:
        return "Cart Texture";
      case 18:
        return "Metal Bare";
      case 19:
        return "Metal Pattern";
      case 20:
        return "Crystal Light Pink";
      case 21:
        return "Crystal Light Purple";
      case 22:
        return "Crystal Light Teal";
      case 23:
        return "Crystal Light Yellow";
      case 24:
        return "Bronze";
      case 25:
        return "Red Panel";
      case 26:
        return "Rusty";
      case 27:
        return "Cactus";
      case 28:
        return "Light";
      case 29:
        return "Window";
      case 30:
        return "Mirror Center";
      case 31 /*0x1F*/:
        return "Pit Ground Fall";
      case 32 /*0x20*/:
        return "Pit Ground Winter";
      case 33:
        return "Holiday White";
      case 34:
        return "Holiday White Off";
      case 35:
        return "Holiday Red";
      case 36:
        return "Holiday Green";
      case 37:
        return "Holiday Yellow";
      case 38:
        return "Holiday Blue";
      case 39:
        return "Christmas Tree";
      case 40:
        return "Crystal Red Root 2 Down";
      case 41:
        return "Crystal Orange Second 2 Down";
      case 42:
        return "Crystal Yellow Third 2 Down";
      case 43:
        return "Crystal Teal Fifth 2 Down";
      case 44:
        return "Crystal Dark Blue Sixth 2 Down";
      case 45:
        return "Crystal Red Root 1 Down";
      case 46:
        return "Crystal Orange Second 1 Down";
      case 47:
        return "Crystal Yellow Third 1 Down";
      case 48 /*0x30*/:
        return "Crystal Teal Fifth 1 Down";
      case 49:
        return "Crystal Dark Blue Sixth 1 Down";
      case 50:
        return "Crystal Red Root";
      case 51:
        return "Crystal Orange Second";
      case 52:
        return "Crystal Yellow Third";
      case 53:
        return "Crystal Teal Fifth";
      case 54:
        return "Crystal Dark Blue Sixth";
      case 55:
        return "Shiny Hat";
      case 56:
        return "Whee";
      case 57:
        return "Bronze Slide";
      case 58:
        return "Metal Slide";
      case 59:
        return "Ice Ground";
      case 60:
        return "Purple Plastic Inside";
      case 61:
        return "Mountain Ice";
      case 62:
        return "Cosmetics Room Atlas Slippery";
      case 63 /*0x3F*/:
        return "Rock Wall 3";
      case 64 /*0x40*/:
        return "Umbrella Open";
      case 65:
        return "Umbrella Close";
      case 66:
        return "Keyboard Key";
      case 67:
        return "Button Press";
      case 68:
        return "Giant Drum";
      case 69:
        return "Large Drum";
      case 70:
        return "Medium Drum";
      case 71:
        return "Small Drum";
      case 72:
        return "Cymbal";
      case 73:
        return "Big Bongo";
      case 74:
        return "Small Bongo";
      case 75:
        return "Squeak Squeeze";
      case 76:
        return "Squeak Release";
      case 77:
        return "Rattling Bone";
      case 78:
        return "Tombstone Hit";
      case 79:
        return "Inner Cauldron";
      case 80 /*0x50*/:
        return "Outer Cauldron";
      case 81:
        return "Pumpkin Hit";
      case 82:
        return "Web Touch";
      case 83:
        return "Turkey Squeeze";
      case 84:
        return "Food Pop";
      case 85:
        return "Bite 1";
      case 86:
        return "Bite 2";
      case 87:
        return "Bite 3";
      case 88:
        return "Hay Bale";
      case 89:
        return "Rope Creak";
      case 90:
        return "Plant Crunch";
      case 91:
        return "Frog Squeeze";
      case 92:
        return "Vine";
      case 93:
        return "Cloud Floor";
      case 94:
        return "Platform";
      case 95:
        return "Car Tire";
      case 96 /*0x60*/:
        return "Storefront Fruit";
      case 97:
        return "Branches";
      case 98:
        return "Clothes Dryer";
      case 99:
        return "Leaf";
      case 100:
        return "Gear Platform";
      case 101:
        return "Pivot Platform";
      case 102:
        return "Wall Jump";
      case 103:
        return "Gear Platform";
      case 104:
        return "Wall Jump";
      case 105:
        return "Hut";
      case 106:
        return "Firefly Jar";
      case 107:
        return "Bean Bag Chair";
      case 108:
        return "Round Pillow";
      case 109:
        return "Square Pillow";
      case 110:
        return "Small Storefront";
      case 111:
        return "Large Storefront";
      case 112 /*0x70*/:
        return "Small Storefront";
      case 113:
        return "Large Storefront";
      case 114:
        return "Wooden Shelf";
      case 115:
        return "Hut";
      case 116:
        return "Crane";
      case 117:
        return "Crane";
      case 118:
        return "Square Rug";
      case 119:
        return "Round Rug";
      case 120:
        return "Window";
      case 121:
        return "Giant Ornament Long";
      case 122:
        return "Christmas Tree";
      case 123:
        return "Giant Ornament Round";
      case 124:
        return "Giant Ornament Star";
      case 125:
        return "Giant Ornament Snowflake";
      case 126:
        return "Stocking Empty";
      case (int) sbyte.MaxValue:
        return "Stocking Full";
      case 128 /*0x80*/:
        return "Shiny Atlas";
      case 129:
        return "Neon Blue";
      case 130:
        return "Neon Red";
      case 131:
        return "Neon Green";
      case 132:
        return "Neon Purple";
      case 133:
        return "Neon Yellow";
      case 134:
        return "Rectangle Gift Box";
      case 135:
        return "Rectangle Gift Box Full";
      case 136:
        return "Square Gift Box";
      case 137:
        return "Square Gift Box Full";
      case 138:
        return "Neon Yellow";
      case 139:
        return "Hat Atlas";
      case 140:
        return "Toy Gorilla Squeeze";
      case 141:
        return "Toy Gorilla Release";
      case 142:
        return "Gear Platform";
      case 143:
        return "Material";
      case 144 /*0x90*/:
        return "Stool";
      case 145:
        return "Table";
      case 146:
        return "TV";
      case 147:
        return "Snowman";
      case 148:
        return "Giant Candy Cane";
      case 149:
        return "Toy Train";
      case 150:
        return "Giant Snowman";
      case 151:
        return "Hut Container";
      case 152:
        return "Tree Topper";
      case 153:
        return "Shiny Hat Atlas";
      case 154:
        return "Penguin Squeeze";
      case 155:
        return "Penguin Release";
      case 156:
        return "Wolf Squeeze";
      case 157:
        return "Wolf Release";
      case 158:
        return "Basement Couch";
      case 159:
        return "Shag Carpet";
      case 160 /*0xA0*/:
        return "Small Basement Box";
      case 161:
        return "Medium Basement Box";
      case 162:
        return "Large Basement Box";
      case 163:
        return "Basement Couch Pillow";
      case 164:
        return "Basement Rug";
      case 165:
        return "Large Basement Rug";
      case 166:
        return "Basement Shelf";
      case 167:
        return "Small Basement Shelf";
      case 168:
        return "Basement Stool";
      case 169:
        return "Basement Table";
      case 170:
        return "Basement Heater";
      case 171:
        return "Basement Lava Lamp";
      case 172:
        return "Lumber";
      case 173:
        return "Wall Panel";
      case 174:
        return "Basement Comic Box";
      case 175:
        return "Basement Comic Books";
      case 176 /*0xB0*/:
        return "Basement Lamp";
      case 177:
        return "Basement Lamp";
      case 178:
        return "Basement Book";
      case 179:
        return "Basement Book";
      case 180:
        return "Basement Book";
      case 181:
        return "Basement Book";
      case 182:
        return "Basement Book Stack";
      case 183:
        return "Basement Book Stack";
      case 184:
        return "Basement Light";
      case 185:
        return "Basement Light";
      case 186:
        return "Basement Pipes";
      case 187:
        return "Monkey Squeeze";
      case 188:
        return "Dragon Squeeze";
      case 189:
        return "Basement Concrete";
      case 190:
        return "Basement Concrete Floor";
      case 191:
        return "Bee Toy Squeeze";
      case 192 /*0xC0*/:
        return "Bee Toy Release";
      case 193:
        return "Sponge Squeeze";
      case 194:
        return "Sponge Release";
      case 195:
        return "Coyote Toy Squeeze";
      case 196:
        return "Diving Board Bounce";
      case 197:
        return "Beach Sand";
      case 198:
        return "Palm Tree Bark";
      case 199:
        return "Palm Tree Leaf";
      case 200:
        return "Shark Squeeze";
      case 201:
        return "Shark Release";
      case 202:
        return "Beach Tent";
      case 203:
        return "Firework Mortars Bucket Pile";
      case 204:
        return "Water Balloon Bucket Pile";
      case 205:
        return "Summer Slip Slide";
      case 206:
        return "Dolphin Squeeze";
      case 207:
        return "Dolphin Release";
      case 208 /*0xD0*/:
        return "Bug Spray Squeeze";
      case 209:
        return "Bug Spray Release";
      case 210:
        return "Trampoline Bounce";
      case 211:
        return "Button Split Down";
      case 212:
        return "Button Split Up";
      case 213:
        return "Huge Crystal";
      case 214:
        return "Purple Crystal Seventh";
      case 215:
        return "Purple Crystal Seventh 1 Up";
      case 216:
        return "Purple Crystal Seventh 1 Down";
      case 217:
        return "Purple Crystal Seventh 2 Down";
      case 218:
        return "Green Crystal Fourth";
      case 219:
        return "Green Crystal Fourth 1 Down";
      case 220:
        return "Green Crystal Fourth 1 Up";
      case 221:
        return "Green Crystal Fourth 2 Down";
      case 222:
        return "Eel Squeeze";
      case 223:
        return "Eel Release";
      case 224 /*0xE0*/:
        return "Red Crystal Root 1 Up";
      case 225:
        return "Orange Crystal Second 1 Up";
      case 226:
        return "Yellow Crystal Third 1 Up";
      case 227:
        return "Teal Crystal Fifth 1 Up";
      case 228:
        return "Dark Blue Crystal Sixth 1 Up";
      default:
        return "Unknown Sound";
    }
  }

  public static void SpecificSoundSpam(int SoundIndex)
  {
    if ((double) Time.time < (double) Variables.rpcCooldown)
      return;
    Variables.rpcCooldown = Time.time + Sound.soundCooldown;
    Safety.RPCShield();
    if (!InputHandler.RTrigger())
      return;
    if (PhotonNetwork.InRoom)
      Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget) 0, new object[3]
      {
        (object) SoundIndex,
        (object) Variables.rightHandedMenu,
        (object) 99999f
      });
    else
      Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(SoundIndex, Variables.rightHandedMenu, 1f);
  }

  public static void OnTouchPlayerSoundSpam(int SoundIndex)
  {
    if ((double) Time.time < (double) Variables.rpcCooldown)
      return;
    bool flag1 = false;
    bool flag2 = false;
    float num = 0.325f;
    foreach (VRRig activeRig in (IEnumerable<VRRig>) VRRigCache.ActiveRigs)
    {
      if (RigManager.IsOtherPlayer(activeRig))
      {
        if (!flag1)
          flag1 = (double) Vector3.Distance(Variables.taggerInstance.leftHandTransform.position, activeRig.headMesh.transform.position) < (double) num;
        if (!flag2)
          flag2 = (double) Vector3.Distance(Variables.taggerInstance.rightHandTransform.position, activeRig.headMesh.transform.position) < (double) num;
        if (flag1 & flag2)
          break;
      }
    }
    if (flag1 && !Sound.lastLeftTouch)
    {
      Variables.rpcCooldown = Time.time + Sound.soundCooldown;
      Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget) 0, new object[3]
      {
        (object) SoundIndex,
        (object) true,
        (object) 99999f
      });
      Safety.RPCShield();
    }
    if (flag2 && !Sound.lastRightTouch)
    {
      Variables.rpcCooldown = Time.time + Sound.soundCooldown;
      Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget) 0, new object[3]
      {
        (object) SoundIndex,
        (object) false,
        (object) 99999f
      });
      Safety.RPCShield();
    }
    Sound.lastLeftTouch = flag1;
    Sound.lastRightTouch = flag2;
  }

  public static void Custom_Soundspam()
  {
    if ((double) Time.time < (double) Variables.rpcCooldown)
      return;
    Variables.rpcCooldown = Time.time + Sound.soundCooldown;
    Safety.RPCShield();
    if (InputHandler.RTrigger())
    {
      if (PhotonNetwork.InRoom)
        Variables.taggerInstance.myVRRig.GetView.RPC("RPC_PlayHandTap", (RpcTarget) 0, new object[3]
        {
          (object) Sound.sound,
          (object) Variables.rightHandedMenu,
          (object) 99999f
        });
      else
        Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(Sound.sound, Variables.rightHandedMenu, 99999f);
    }
    if (InputHandler.RPrimary() && !Sound.haschangedint)
    {
      ++Sound.sound;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Info, $"Set Index To: {Sound.GetSoundName(Sound.sound)} [{Sound.sound.ToString()}]");
      Sound.haschangedint = true;
    }
    if (!InputHandler.RPrimary())
      Sound.haschangedint = false;
    if (InputHandler.RSecondary() && !Sound.haschangedint2)
    {
      --Sound.sound;
      NotificationLib.SendNotification(NotificationLib.NotificationType.Info, $"Set Index To: {Sound.GetSoundName(Sound.sound)} [{Sound.sound.ToString()}]");
      Sound.haschangedint2 = true;
    }
    if (InputHandler.RSecondary())
      return;
    Sound.haschangedint2 = false;
  }

  public static void SetMicQuality(int bitrate, int samplingRate)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (myRecorder.SamplingRate == (SamplingRate) samplingRate && myRecorder.Bitrate == bitrate)
      return;
    myRecorder.SamplingRate = (SamplingRate) samplingRate;
    myRecorder.Bitrate = bitrate;
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void LoudMic(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>() != (UnityEngine.Object) null)
        return;
      MicAmplifier orAddComponent = GTExt.GetOrAddComponent<MicAmplifier>(((Component) myRecorder).gameObject);
      orAddComponent.AmplificationFactor = 13f;
      orAddComponent.AmplificationFactor = 13f;
    }
    else if ((bool) (UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>())
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>() == (UnityEngine.Object) null)
        return;
      ((Behaviour) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>()).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>());
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void MuteMic(bool mute)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (myRecorder.IsRecording != mute)
      return;
    myRecorder.IsRecording = !mute;
  }

  public static void SetMicPitch(float pitch)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (!Mathf.Approximately(pitch, 1f))
    {
      Sound.MicPitchShifter component = ((Component) myRecorder).gameObject.GetComponent<Sound.MicPitchShifter>();
      if ((UnityEngine.Object) component != (UnityEngine.Object) null && Mathf.Approximately(component.PitchFactor, pitch))
        return;
      GTExt.GetOrAddComponent<Sound.MicPitchShifter>(((Component) myRecorder).gameObject).PitchFactor = pitch;
    }
    else
    {
      if (!(bool) (UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.MicPitchShifter>())
        return;
      ((Behaviour) ((Component) myRecorder).gameObject.GetComponent<Sound.MicPitchShifter>()).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.MicPitchShifter>());
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void EchoVoice(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if ((UnityEngine.Object) myRecorder == (UnityEngine.Object) null)
      return;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.EchoEffect>() != (UnityEngine.Object) null)
        return;
      Sound.EchoEffect orAddComponent = GTExt.GetOrAddComponent<Sound.EchoEffect>(((Component) myRecorder).gameObject);
      orAddComponent.echoDelay = 0.3f;
      orAddComponent.echoDecay = 0.6f;
      myRecorder.VoiceDetection = false;
      myRecorder.VoiceDetectionThreshold = 0.0f;
      myRecorder.TransmitEnabled = true;
    }
    else
    {
      Sound.EchoEffect component = ((Component) myRecorder).gameObject.GetComponent<Sound.EchoEffect>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      ((Behaviour) component).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) component);
      myRecorder.VoiceDetection = true;
      myRecorder.VoiceDetectionThreshold = 0.02f;
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void RadioVoice(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.RadioEffect>() != (UnityEngine.Object) null)
        return;
      GTExt.GetOrAddComponent<Sound.RadioEffect>(((Component) myRecorder).gameObject);
    }
    else
    {
      if (!(bool) (UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.RadioEffect>())
        return;
      ((Behaviour) ((Component) myRecorder).gameObject.GetComponent<Sound.RadioEffect>()).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.RadioEffect>());
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void UnderwaterVoice(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.UnderwaterEffect>() != (UnityEngine.Object) null)
        return;
      GTExt.GetOrAddComponent<Sound.UnderwaterEffect>(((Component) myRecorder).gameObject);
    }
    else
    {
      if (!(bool) (UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.UnderwaterEffect>())
        return;
      ((Behaviour) ((Component) myRecorder).gameObject.GetComponent<Sound.UnderwaterEffect>()).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.UnderwaterEffect>());
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void ReloadMic()
  {
    if (!((UnityEngine.Object) GorillaTagger.Instance?.myRecorder != (UnityEngine.Object) null))
      return;
    GorillaTagger.Instance.myRecorder.RestartRecording(true);
  }

  public static void ResetMic()
  {
    Recorder myRecorder = Variables.taggerInstance?.myRecorder;
    if ((UnityEngine.Object) myRecorder == (UnityEngine.Object) null)
      return;
    Soundboard.CleanupAllSounds();
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.MicPitchShifter>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.EchoEffect>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.ReverbEffect>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.RadioEffect>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.UnderwaterEffect>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.SquareWaveEffect>());
    UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<MicAmplifier>());
    myRecorder.SourceType = (Recorder.InputSourceType) 0;
    myRecorder.AudioClip = (AudioClip) null;
    myRecorder.LoopAudioClip = false;
    myRecorder.IsRecording = true;
    myRecorder.VoiceDetection = true;
    myRecorder.VoiceDetectionThreshold = 0.02f;
    myRecorder.TransmitEnabled = true;
    myRecorder.Bitrate = 20000;
    myRecorder.SamplingRate = (SamplingRate) 16000;
    myRecorder.RestartRecording(true);
  }

  private static IEnumerator DelayReloadMic()
  {
    yield return (object) new WaitForSeconds(0.25f);
    Sound.ReloadMic();
  }

  public static void ReverbMic(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if ((UnityEngine.Object) myRecorder == (UnityEngine.Object) null)
      return;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.ReverbEffect>() != (UnityEngine.Object) null)
        return;
      GTExt.GetOrAddComponent<Sound.ReverbEffect>(((Component) myRecorder).gameObject);
      myRecorder.VoiceDetection = false;
      myRecorder.VoiceDetectionThreshold = 0.0f;
      myRecorder.TransmitEnabled = true;
    }
    else
    {
      Sound.ReverbEffect component = ((Component) myRecorder).gameObject.GetComponent<Sound.ReverbEffect>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      ((Behaviour) component).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) component);
      myRecorder.VoiceDetection = true;
      myRecorder.VoiceDetectionThreshold = 0.02f;
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public static void Earrape(bool enable)
  {
    if (!PhotonNetwork.InRoom)
      return;
    Recorder myRecorder = GorillaTagger.Instance.myRecorder;
    if (enable)
    {
      if ((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.SquareWaveEffect>() != (UnityEngine.Object) null)
        return;
      myRecorder.TransmitEnabled = true;
      myRecorder.VoiceDetection = false;
      myRecorder.VoiceDetectionThreshold = 0.0f;
      GTExt.GetOrAddComponent<Sound.SquareWaveEffect>(((Component) myRecorder).gameObject);
    }
    else
    {
      if (!(bool) (UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.SquareWaveEffect>())
        return;
      myRecorder.VoiceDetection = true;
      myRecorder.VoiceDetectionThreshold = 0.01f;
      ((Behaviour) ((Component) myRecorder).gameObject.GetComponent<Sound.SquareWaveEffect>()).enabled = false;
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) myRecorder).gameObject.GetComponent<Sound.SquareWaveEffect>());
    }
    CoroutineHelper.Instance.StartCoroutine(Sound.DelayReloadMic());
  }

  public class MicPitchShifter : VoiceComponent
  {
    public float PitchFactor = 1.5f;
    public Sound.MicPitchShifter.PitchProcessor floatProcessor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.floatProcessor = new Sound.MicPitchShifter.PitchProcessor(this.PitchFactor);
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.floatProcessor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class PitchProcessor : IProcessor<float>, IDisposable
    {
      private readonly float pitch;

      public PitchProcessor(float pitchFactor) => this.pitch = Mathf.Clamp(pitchFactor, 0.5f, 2f);

      public float[] Process(float[] buf)
      {
        int length = buf.Length;
        float[] numArray = new float[length];
        float f = 0.0f;
        for (int index1 = 0; index1 < length; ++index1)
        {
          int index2 = Mathf.FloorToInt(f);
          int index3 = Mathf.Min(index2 + 1, length - 1);
          float t = f - (float) index2;
          float num = Mathf.Lerp(buf[index2], buf[index3], t);
          numArray[index1] = num;
          f += this.pitch;
          if ((double) f >= (double) (length - 1))
            break;
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class EchoEffect : VoiceComponent
  {
    public float echoDelay = 0.3f;
    public float echoDecay = 0.6f;
    private Sound.EchoEffect.EchoProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.EchoEffect.EchoProcessor(this.echoDelay, this.echoDecay);
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class EchoProcessor : IProcessor<float>, IDisposable
    {
      private readonly float decay;
      private readonly float[] buffer;
      private int bufferIndex = 0;
      private const int sampleRate = 48000;

      public EchoProcessor(float delaySeconds, float decayAmount)
      {
        this.decay = Mathf.Clamp01(decayAmount);
        this.buffer = new float[Mathf.CeilToInt(48000f * delaySeconds)];
      }

      public float[] Process(float[] buf)
      {
        float[] numArray = new float[buf.Length];
        for (int index = 0; index < buf.Length; ++index)
        {
          float num1 = this.buffer[this.bufferIndex];
          float num2 = buf[index] + num1 * this.decay;
          numArray[index] = Mathf.Clamp(num2, -1f, 1f);
          this.buffer[this.bufferIndex] = num2;
          this.bufferIndex = (this.bufferIndex + 1) % this.buffer.Length;
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class RadioEffect : VoiceComponent
  {
    private Sound.RadioEffect.RadioProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.RadioEffect.RadioProcessor();
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class RadioProcessor : IProcessor<float>, IDisposable
    {
      private float phase = 0.0f;
      private const int sampleRate = 48000;

      public float[] Process(float[] buf)
      {
        float[] numArray = new float[buf.Length];
        for (int index = 0; index < buf.Length; ++index)
        {
          float num1 = buf[index];
          if (index > 0)
            num1 -= buf[index - 1] * 0.9f;
          float num2 = (float) (((double) UnityEngine.Random.value - 0.5) * 0.05000000074505806);
          numArray[index] = Mathf.Clamp(num1 + num2, -1f, 1f);
          this.phase += 2.08333331E-05f;
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class UnderwaterEffect : VoiceComponent
  {
    private Sound.UnderwaterEffect.UnderwaterProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.UnderwaterEffect.UnderwaterProcessor();
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class UnderwaterProcessor : IProcessor<float>, IDisposable
    {
      private float[] lowPassBuffer = new float[5];
      private int bufferIndex = 0;

      public float[] Process(float[] buf)
      {
        float[] numArray = new float[buf.Length];
        for (int index1 = 0; index1 < buf.Length; ++index1)
        {
          this.lowPassBuffer[this.bufferIndex] = buf[index1];
          float num = 0.0f;
          for (int index2 = 0; index2 < this.lowPassBuffer.Length; ++index2)
            num += this.lowPassBuffer[index2];
          numArray[index1] = (float) ((double) num / (double) this.lowPassBuffer.Length * 0.699999988079071);
          this.bufferIndex = (this.bufferIndex + 1) % this.lowPassBuffer.Length;
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class ReverbEffect : VoiceComponent
  {
    public float reverbAmount = 0.5f;
    private Sound.ReverbEffect.ReverbProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.ReverbEffect.ReverbProcessor(this.reverbAmount);
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class ReverbProcessor : IProcessor<float>, IDisposable
    {
      private readonly float amount;
      private float[] buffer1;
      private float[] buffer2;
      private float[] buffer3;
      private int index1 = 0;
      private int index2 = 0;
      private int index3 = 0;

      public ReverbProcessor(float reverbAmount)
      {
        this.amount = Mathf.Clamp01(reverbAmount);
        this.buffer1 = new float[1789];
        this.buffer2 = new float[2357];
        this.buffer3 = new float[3137];
      }

      public float[] Process(float[] buf)
      {
        float[] numArray = new float[buf.Length];
        for (int index = 0; index < buf.Length; ++index)
        {
          float num1 = this.buffer1[this.index1];
          float num2 = this.buffer2[this.index2];
          float num3 = this.buffer3[this.index3];
          float num4 = (float) (((double) num1 + (double) num2 + (double) num3) / 3.0) * this.amount;
          numArray[index] = buf[index] + num4 * 0.4f;
          this.buffer1[this.index1] = buf[index] + num1 * 0.5f;
          this.buffer2[this.index2] = buf[index] + num2 * 0.6f;
          this.buffer3[this.index3] = buf[index] + num3 * 0.7f;
          this.index1 = (this.index1 + 1) % this.buffer1.Length;
          this.index2 = (this.index2 + 1) % this.buffer2.Length;
          this.index3 = (this.index3 + 1) % this.buffer3.Length;
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class SquareWaveEffect : VoiceComponent
  {
    private Sound.SquareWaveEffect.SquareProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.SquareWaveEffect.SquareProcessor();
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class SquareProcessor : IProcessor<float>, IDisposable
    {
      private float phase = 0.0f;
      private const float SAMPLE_RATE = 48000f;
      private const float FREQUENCY = 3500f;

      public float[] Process(float[] buf)
      {
        float[] numArray = new float[buf.Length];
        for (int index = 0; index < buf.Length; ++index)
        {
          this.phase += 0.0729166642f;
          if ((double) this.phase >= 1.0)
            --this.phase;
          numArray[index] = (float) (((double) this.phase < 0.5 ? 1.0 : -1.0) * 0.5);
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }

  public class AutotuneEffect : VoiceComponent
  {
    private Sound.AutotuneEffect.AutotuneProcessor processor;

    public void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
    {
      if (!(p.Voice is LocalVoiceAudioFloat voice))
        return;
      this.processor = new Sound.AutotuneEffect.AutotuneProcessor();
      IProcessor<float>[] iprocessorArray = new IProcessor<float>[1]
      {
        (IProcessor<float>) this.processor
      };
      ((LocalVoiceFramed<float>) voice).AddPostProcessor(iprocessorArray);
    }

    public class AutotuneProcessor : IProcessor<float>, IDisposable
    {
      private const int SAMPLE_RATE = 48000;
      private const float MIN_FREQ = 80f;
      private const float MAX_FREQ = 1100f;
      private readonly float[] pitchBuffer = new float[2048 /*0x0800*/];
      private static readonly float[] Notes = Sound.AutotuneEffect.AutotuneProcessor.GenerateNotes();

      private static float[] GenerateNotes()
      {
        float[] notes = new float[128 /*0x80*/];
        for (int index = 0; index < 128 /*0x80*/; ++index)
          notes[index] = 440f * Mathf.Pow(2f, (float) (index - 69) / 12f);
        return notes;
      }

      private float DetectPitch(float[] buf)
      {
        int num1 = 43;
        int num2 = Mathf.Min(600, buf.Length / 2);
        float num3 = -1f;
        int num4 = num1;
        for (int index1 = num1; index1 < num2; ++index1)
        {
          float num5 = 0.0f;
          float num6 = 0.0f;
          for (int index2 = 0; index2 < buf.Length - index1; ++index2)
          {
            num5 += buf[index2] * buf[index2 + index1];
            num6 += (float) ((double) buf[index2] * (double) buf[index2] + (double) buf[index2 + index1] * (double) buf[index2 + index1]);
          }
          if ((double) num6 > 0.0)
            num5 = 2f * num5 / num6;
          if ((double) num5 > (double) num3)
          {
            num3 = num5;
            num4 = index1;
          }
        }
        return (double) num3 > 0.40000000596046448 ? 48000f / (float) num4 : -1f;
      }

      private float SnapToNote(float freq)
      {
        float note1 = Sound.AutotuneEffect.AutotuneProcessor.Notes[0];
        float num1 = Mathf.Abs(freq - Sound.AutotuneEffect.AutotuneProcessor.Notes[0]);
        foreach (float note2 in Sound.AutotuneEffect.AutotuneProcessor.Notes)
        {
          float num2 = Mathf.Abs(freq - note2);
          if ((double) num2 < (double) num1)
          {
            num1 = num2;
            note1 = note2;
          }
        }
        return note1;
      }

      public float[] Process(float[] buf)
      {
        float freq = this.DetectPitch(buf);
        if ((double) freq < 0.0)
          return buf;
        float num = Mathf.Clamp(this.SnapToNote(freq) / freq, 0.5f, 2f);
        float[] numArray = new float[buf.Length];
        for (int index1 = 0; index1 < buf.Length; ++index1)
        {
          float f = (float) index1 * num;
          int index2 = Mathf.FloorToInt(f);
          int index3 = Mathf.Min(index2 + 1, buf.Length - 1);
          numArray[index1] = index2 < buf.Length ? Mathf.Lerp(buf[index2], buf[index3], f - (float) index2) : buf[buf.Length - 1];
        }
        return numArray;
      }

      public void Dispose()
      {
      }
    }
  }
}
