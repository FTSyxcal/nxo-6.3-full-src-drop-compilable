
using NXO.Menu;
using NXO.Utilities;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace NXO.Mods.Categories;

public static class Soundboard
{
  private static readonly Dictionary<string, bool> soundboardSoundsActive = new Dictionary<string, bool>();
  private static readonly Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
  private static readonly Dictionary<string, AudioSource> loadedSounds = new Dictionary<string, AudioSource>();
  private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
  private static readonly Dictionary<string, string> soundboardUrls = new Dictionary<string, string>()
  {
    {
      nameof (SFX),
      "https://github.com/NuggetGT/NXO-Resources/raw/refs/heads/main/Sounds/SoundEffects/SoundEffectsDownloadPath.txt"
    },
    {
      nameof (Trolling),
      "https://github.com/NuggetGT/NXO-Resources/raw/refs/heads/main/Sounds/Trolling/TrollingDownloadPath.txt"
    },
    {
      nameof (Songs),
      "https://github.com/NuggetGT/NXO-Resources/raw/refs/heads/main/Sounds/Songs/SongsDownloadPath.txt"
    }
  };
  public static Dictionary<string, string> SFX = new Dictionary<string, string>();
  public static Dictionary<string, string> Trolling = new Dictionary<string, string>();
  public static Dictionary<string, string> Songs = new Dictionary<string, string>();
  public static Dictionary<string, string> Custom = new Dictionary<string, string>();
  private static bool loop;
  private static AudioSource currentAudioSource;
  private static string currentlyPlayingSound;
  private static bool isPlaying;
  private static readonly string folderName = Path.Combine(Application.dataPath, "NXO Mod Menu");
  private static readonly string soundsPath = Path.Combine(Soundboard.folderName, "SoundBoardSounds");
  public static string CurrentlyPlaying;
  public static bool _isPlayingSound = false;
  public static List<string> sounds;
  private static GameObject soundSourceCache;
  private static List<string> SoundsPlaying = new List<string>();
  private static string CurrentlyPlayingSoundName = string.Empty;

  public static Dictionary<string, bool> SoundboardSoundsActive
  {
    get => Soundboard.soundboardSoundsActive;
  }

  public static bool SoundsInitialized { get; private set; }

  public static GameObject SoundSRC
  {
    get
    {
      return (UnityEngine.Object) Soundboard.soundSourceCache != (UnityEngine.Object) null ? Soundboard.soundSourceCache : (Soundboard.soundSourceCache = new GameObject("SoundSource"));
    }
  }

  public static void EnableLoop() => Soundboard.loop = true;

  public static void DisableLoop() => Soundboard.loop = false;

  public static void PlaySound(string soundName)
  {
    AudioSource audioSource = Soundboard.SoundSRC.GetComponent<AudioSource>() ?? Soundboard.SoundSRC.AddComponent<AudioSource>();
    if ((UnityEngine.Object) audioSource == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) ((Component) audioSource).GetComponent<Renderer>() != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) audioSource).GetComponent<Renderer>());
    if ((UnityEngine.Object) ((Component) audioSource).GetComponent<Collider>() != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) audioSource).GetComponent<Collider>());
    if (Soundboard.SoundsPlaying.Contains(soundName))
      return;
    string str = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "NXO Mod Menu", "Custom_Sounds"), soundName);
    if (!File.Exists(str))
    {
      Soundboard.SoundsPlaying.Add(soundName);
      Soundboard.CurrentlyPlayingSoundName = soundName;
      Soundboard.soundboardSoundsActive[soundName] = true;
      CoroutineHelper.Instance.StartCoroutine(Soundboard.DownloadAndPlaySound(soundName));
    }
    else
    {
      AudioType audioType = Soundboard.GetAudioType(Path.GetExtension(soundName).ToLowerInvariant());
      if (audioType == AudioType.UNKNOWN)
        return;
      Soundboard.SoundsPlaying.Add(soundName);
      Soundboard.CurrentlyPlayingSoundName = soundName;
      CoroutineHelper.Instance.StartCoroutine(Soundboard.LoadAndPlay(str, soundName, audioType));
    }
  }

  private static IEnumerator LoadAndPlay(string fullPath, string soundName, AudioType audioType)
  {
    AudioClip cached;
    if (Soundboard.clipCache.TryGetValue(fullPath, out cached) && (UnityEngine.Object) cached != (UnityEngine.Object) null)
    {
      yield return (object) Soundboard.PlayThroughRecorder(cached);
    }
    else
    {
      string uri = "file:///" + fullPath.Replace("\\", "/");
      UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
      bool flag;
      try
      {
        yield return (object) www.SendWebRequest();
        if (www.result != (UnityWebRequest.Result) 1)
        {
          flag = false;
        }
        else
        {
          AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
          if ((UnityEngine.Object) clip == (UnityEngine.Object) null)
          {
            flag = false;
          }
          else
          {
            ((UnityEngine.Object) clip).name = soundName;
            Soundboard.clipCache[fullPath] = clip;
            yield return (object) Soundboard.PlayThroughRecorder(clip);
            clip = (AudioClip) null;
            goto label_10;
          }
        }
        goto label_11;
      }
      finally
      {
        ((IDisposable) www)?.Dispose();
      }
label_10:
      www = (UnityWebRequest) null;
      yield break;
label_11:
      yield break;
    }
  }

  private static IEnumerator PlayThroughRecorder(AudioClip clip)
  {
    if (!Soundboard._isPlayingSound)
    {
      Soundboard._isPlayingSound = true;
      Soundboard.SoundSRC.transform.position = ((Component) GorillaTagger.Instance.bodyCollider).transform.position;
      AudioSource src = Soundboard.SoundSRC.GetComponent<AudioSource>();
      src.clip = clip;
      src.loop = Soundboard.loop;
      Recorder recorder = ((VoiceConnection) PhotonVoiceNetwork.Instance).PrimaryRecorder;
      recorder.StopRecording();
      recorder.SourceType = (Recorder.InputSourceType) 1;
      recorder.AudioClip = clip;
      recorder.LoopAudioClip = Soundboard.loop;
      src.Play();
      recorder.StartRecording();
      yield return (object) new WaitForSeconds(clip.length);
      recorder.SourceType = (Recorder.InputSourceType) 0;
      recorder.RestartRecording(true);
      Soundboard._isPlayingSound = false;
      src.Stop();
      Soundboard.SoundsPlaying.Remove(Soundboard.CurrentlyPlayingSoundName);
      Soundboard.CurrentlyPlayingSoundName = string.Empty;
    }
  }

  public static void InitializeSounds()
  {
    if (Soundboard.SoundsInitialized)
      return;
    if ((UnityEngine.Object) CoroutineHelper.Instance == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "CoroutineHelper.Instance is null. Make sure CoroutineHelper is initialized in the scene.");
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Coroutine Helper Not Initialized");
    }
    else
    {
      Soundboard.SoundsInitialized = true;
      Debug.Log((object) "Sounds Initialized");
      CoroutineHelper.Instance.StartCoroutine(Soundboard.GetSoundsToButtons());
      CoroutineHelper.InvokeAfterDelay(2f, new Action(Main.RefreshMenu));
      Soundboard.CreateInstructionsFile();
    }
  }

  private static void CreateInstructionsFile()
  {
    try
    {
      string path = Path.Combine(Soundboard.folderName, "Custom_Sounds", "Instructions.txt");
      if (!Directory.Exists(Path.Combine(Soundboard.folderName, "Custom_Sounds")))
        Directory.CreateDirectory(Path.Combine(Soundboard.folderName, "Custom_Sounds"));
      if (File.Exists(path))
        return;
      string contents = "=== HOW TO ADD CUSTOM SOUNDS ===\r\n\r\n1. Find an audio file (.mp3, .wav, or .ogg)\r\n   - You can download sounds from YouTube using online converters\r\n   - Make sure it's one of these file types: MP3, WAV, or OGG\r\n\r\n2. Copy your audio file into this folder (Custom_Sounds)\r\n   - The file name will be the button name in the menu\r\n   - Example: \"explosion.mp3\" will show as \"explosion\" in the menu\r\n\r\n3. In-game, go to Soundboard > Custom Sounds > Reload Custom Sounds\r\n   - This will refresh the list and add your new sounds\r\n\r\n4. Click any sound button to play it in-game\r\n   - Other players will hear it through your mic\r\n   - Press the button again to stop the sound\r\n\r\nTIPS:\r\n- Keep file names short and simple\r\n- Don't use special characters in file names\r\n- Sounds can be any length\r\n- You can add as many sounds as you want\r\n\r\nThat's it! Have fun!";
      File.WriteAllText(path, contents);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Error creating instructions file: " + ex.Message));
    }
  }

  public static IEnumerator GetSoundsToButtons()
  {
    List<ButtonHandler.Button> buttonList = new List<ButtonHandler.Button>((IEnumerable<ButtonHandler.Button>) ModButtons.buttons);
    yield return (object) Soundboard.LoadAllSoundsCoroutine();
    buttonList.Add(new ButtonHandler.Button("Custom Sounds", Category.Soundboard, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Custom_Sounds)))
    {
      isCategory = true
    });
    buttonList.Add(new ButtonHandler.Button("SFX", Category.Soundboard, false, false, (Action) (() => ButtonHandler.ChangePage(Category.SFX)))
    {
      isCategory = true
    });
    buttonList.Add(new ButtonHandler.Button("Trolling", Category.Soundboard, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Trolling)))
    {
      isCategory = true
    });
    buttonList.Add(new ButtonHandler.Button("Songs", Category.Soundboard, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Songs)))
    {
      isCategory = true
    });
    Soundboard.AddSoundButtons(buttonList, Soundboard.SFX, Category.SFX);
    Soundboard.AddSoundButtons(buttonList, Soundboard.Trolling, Category.Trolling);
    Soundboard.AddSoundButtons(buttonList, Soundboard.Songs, Category.Songs);
    buttonList.Add(new ButtonHandler.Button("Return", Category.Custom_Sounds, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Soundboard)))
    {
      isCategory = true
    });
    buttonList.Add(new ButtonHandler.Button("Open Custom Sounds Folder", Category.Custom_Sounds, false, false, (Action) (() => ButtonHandler.OpenFolder(Path.Combine(Soundboard.folderName, "Custom_Sounds")))));
    buttonList.Add(new ButtonHandler.Button("Reload Custom Sounds", Category.Custom_Sounds, false, false, new Action(Soundboard.ReloadCustom_Sounds)));
    Soundboard.AddSoundButtons(buttonList, Soundboard.Custom, Category.Custom_Sounds);
    ModButtons.buttons = buttonList.ToArray();
  }

  private static void AddSoundButtons(
    List<ButtonHandler.Button> buttonList,
    Dictionary<string, string> sounds,
    Category category)
  {
    foreach (KeyValuePair<string, string> sound in sounds)
    {
      string pathOrUrl = sound.Value;
      buttonList.Add(new ButtonHandler.Button(sound.Key, category, true, false, (Action) (() => Soundboard.PlaySound(pathOrUrl)), (Action) (() => Soundboard.StopSound(pathOrUrl))));
    }
  }

  public static void ReloadCustom_Sounds()
  {
    Soundboard.Custom.Clear();
    Soundboard.LoadCustom_Sounds();
    List<ButtonHandler.Button> buttonList = new List<ButtonHandler.Button>();
    foreach (ButtonHandler.Button button in ModButtons.buttons)
    {
      if (button.Page != Category.Custom_Sounds || !button.isToggle)
        buttonList.Add(button);
    }
    Soundboard.AddSoundButtons(buttonList, Soundboard.Custom, Category.Custom_Sounds);
    ModButtons.buttons = buttonList.ToArray();
    Main.RefreshMenu();
    int count = Soundboard.Custom.Count;
    NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, $"Custom Sounds Reloaded `{count}` `{(count != 1 ? (object) "s" : (object) "")}`");
  }

  public static IEnumerator DownloadAndPlaySound(string pathOrUrl)
  {
    AudioClip audioClip;
    if (!Soundboard.clipCache.TryGetValue(pathOrUrl, out audioClip) || (UnityEngine.Object) audioClip == (UnityEngine.Object) null)
    {
      bool isLocalFile = File.Exists(pathOrUrl);
      AudioType audioType = Soundboard.GetAudioType(pathOrUrl);
      string uri = isLocalFile ? "file://" + pathOrUrl : pathOrUrl;
      UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
      bool flag;
      try
      {
        yield return (object) request.SendWebRequest();
        if (request.result != (UnityWebRequest.Result) 1)
        {
          Debug.LogError((object) ("Failed to load audio: " + request.error));
          Soundboard.ResetPlayState(pathOrUrl);
          flag = false;
        }
        else
        {
          audioClip = DownloadHandlerAudioClip.GetContent(request);
          if ((UnityEngine.Object) audioClip == (UnityEngine.Object) null)
          {
            Debug.LogError((object) "Audio clip is null");
            Soundboard.ResetPlayState(pathOrUrl);
            flag = false;
          }
          else
          {
            Soundboard.clipCache[pathOrUrl] = audioClip;
            goto label_1;
          }
        }
        goto label_9;
      }
      finally
      {
        ((IDisposable) request)?.Dispose();
      }
label_1:
      request = (UnityWebRequest) null;
      uri = (string) null;
      goto label_8;
label_9:
      yield break;
    }
label_8:
    AudioSource stale;
    if (Soundboard.loadedSounds.TryGetValue(pathOrUrl, out stale) && (UnityEngine.Object) stale != (UnityEngine.Object) null)
    {
      stale.Stop();
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) stale).gameObject);
    }
    GameObject soundObject = new GameObject("SoundFromFile");
    AudioSource audioSource = soundObject.AddComponent<AudioSource>();
    audioSource.clip = audioClip;
    audioSource.volume = 1f;
    audioSource.loop = Soundboard.loop;
    Soundboard.loadedSounds[pathOrUrl] = audioSource;
    Soundboard.currentAudioSource = audioSource;
    Soundboard.PlayAudioSource(audioSource, pathOrUrl);
  }

  private static AudioType GetAudioType(string path)
  {
    string lower = Path.GetExtension(path).ToLower();
    if (true)
      ;
    AudioType audioType;
    switch (lower)
    {
      case ".mp3":
        audioType = AudioType.MPEG;
        break;
      case ".wav":
        audioType = AudioType.WAV;
        break;
      case ".ogg":
        audioType = AudioType.OGGVORBIS;
        break;
      default:
        audioType = AudioType.MPEG;
        break;
    }
    if (true)
      ;
    return audioType;
  }

  private static void ResetPlayState(string pathOrUrl)
  {
    Soundboard.CurrentlyPlayingSoundName = (string) null;
    Soundboard.soundboardSoundsActive[pathOrUrl] = false;
    Soundboard.isPlaying = false;
  }

  public static IEnumerator LoadAllSoundsCoroutine()
  {
    Task task = Soundboard.LoadAllSoundboardSounds();
    while (!task.IsCompleted)
      yield return (object) null;
  }

  public static async Task LoadAllSoundboardSounds()
  {
    foreach (KeyValuePair<string, string> soundboardUrl in Soundboard.soundboardUrls)
    {
      KeyValuePair<string, string> entry = soundboardUrl;
      await Soundboard.LoadSoundboardSounds(entry.Value, entry.Key);
      entry = new KeyValuePair<string, string>();
    }
    Soundboard.LoadCustom_Sounds();
  }

  public static void LoadCustom_Sounds()
  {
    try
    {
      string path1 = Path.Combine(Soundboard.folderName, "Custom_Sounds");
      if (!Directory.Exists(path1))
      {
        Directory.CreateDirectory(path1);
        Debug.Log((object) ("Created Custom_Sounds folder at: " + path1));
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Custom Sounds Folder Created");
      }
      else
      {
        string[] array = ((IEnumerable<string>) Directory.GetFiles(path1, "*.*")).Where<string>((Func<string, bool>) (file => file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))).ToArray<string>();
        int num = 0;
        foreach (string path2 in array)
        {
          string withoutExtension = Path.GetFileNameWithoutExtension(path2);
          if (!string.IsNullOrEmpty(withoutExtension))
          {
            if (Soundboard.Custom.ContainsKey(withoutExtension))
            {
              Debug.LogWarning((object) ("Duplicate custom sound: " + withoutExtension));
            }
            else
            {
              Soundboard.Custom[withoutExtension] = path2;
              ++num;
            }
          }
        }
        if (num <= 0)
          return;
        Debug.Log((object) $"Loaded {num} custom sounds from folder");
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Error loading custom sounds: " + ex.Message));
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, $"Custom Sounds Cannot Load `{ex}`");
    }
  }

  public static async Task LoadSoundboardSounds(string url, string category)
  {
    string rawData = await Soundboard.GetRawFrom(url);
    Dictionary<string, string> targetDict;
    string[] lines;
    if (string.IsNullOrEmpty(rawData))
    {
      Debug.LogError((object) ("Failed to fetch sounds for " + category));
      rawData = (string) null;
      targetDict = (Dictionary<string, string>) null;
      lines = (string[]) null;
    }
    else
    {
      if (true)
        ;
      Dictionary<string, string> dictionary;
      switch (category)
      {
        case "SFX":
          dictionary = Soundboard.SFX;
          break;
        case "Trolling":
          dictionary = Soundboard.Trolling;
          break;
        case "Songs":
          dictionary = Soundboard.Songs;
          break;
        default:
          dictionary = (Dictionary<string, string>) null;
          break;
      }
      if (true)
        ;
      targetDict = dictionary;
      if (targetDict == null)
      {
        Debug.LogError((object) ("Invalid category: " + category));
        rawData = (string) null;
        targetDict = (Dictionary<string, string>) null;
        lines = (string[]) null;
      }
      else
      {
        lines = rawData.Split(new char[2]{ '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        string[] strArray = lines;
        for (int index = 0; index < strArray.Length; ++index)
        {
          string line = strArray[index];
          string[] parts = line.Split(';');
          if (parts.Length == 2)
          {
            string name = parts[0].Trim();
            string soundUrl = parts[1].Trim();
            if (!targetDict.ContainsKey(name))
              targetDict[name] = soundUrl;
            parts = (string[]) null;
            name = (string) null;
            soundUrl = (string) null;
            line = (string) null;
          }
        }
        strArray = (string[]) null;
        rawData = (string) null;
        targetDict = (Dictionary<string, string>) null;
        lines = (string[]) null;
      }
    }
  }

  public static async Task<string> GetRawFrom(string url)
  {
    string text;
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
      TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
      ((AsyncOperation) request.SendWebRequest()).completed += (Action<AsyncOperation>) (_ => tcs.SetResult(true));
      int num = await tcs.Task ? 1 : 0;
      text = request.result == (UnityWebRequest.Result) 1 ? request.downloadHandler.text : (string) null;
    }
    return text;
  }

  private static void PlayAudioSource(AudioSource source, string pathOrUrl)
  {
    if ((UnityEngine.Object) source == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "AudioSource is null.");
    }
    else
    {
      source.Play();
      ((Component) source).transform.parent = ((Component) Variables.playerInstance).transform;
      Soundboard.SetupRecorder(source);
      if (!Soundboard.loop)
        return;
      Coroutine coroutine = CoroutineHelper.Instance.StartCoroutine(Soundboard.PlaySoundCoroutine(source, pathOrUrl));
      Soundboard.activeCoroutines[pathOrUrl] = coroutine;
    }
  }

  public static IEnumerator PlaySoundCoroutine(AudioSource source, string pathOrUrl)
  {
    if (!source.isPlaying)
      source.Play();
    while (Soundboard.loop && Soundboard.soundboardSoundsActive.ContainsKey(pathOrUrl) && Soundboard.soundboardSoundsActive[pathOrUrl])
    {
      yield return (object) new WaitForSeconds(source.clip.length);
      if (Soundboard.loop)
        source.Play();
    }
    Soundboard.StopSound(pathOrUrl);
  }

  public static void StopSound(string pathOrUrl)
  {
    Recorder primaryRecorder = ((VoiceConnection) PhotonVoiceNetwork.Instance).PrimaryRecorder;
    primaryRecorder.AudioClip = (AudioClip) null;
    primaryRecorder.SourceType = (Recorder.InputSourceType) 0;
    primaryRecorder.RestartRecording(true);
    AudioSource component = Soundboard.SoundSRC.GetComponent<AudioSource>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
      component.Stop();
    Soundboard.soundboardSoundsActive[pathOrUrl] = false;
    Coroutine routine;
    if (Soundboard.activeCoroutines.TryGetValue(pathOrUrl, out routine) && routine != null)
    {
      CoroutineHelper.Instance.StopCoroutine(routine);
      Soundboard.activeCoroutines.Remove(pathOrUrl);
    }
    AudioSource audioSource;
    if (Soundboard.loadedSounds.TryGetValue(pathOrUrl, out audioSource) && (UnityEngine.Object) audioSource != (UnityEngine.Object) null)
    {
      audioSource.Stop();
      UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) audioSource).gameObject);
      Soundboard.loadedSounds.Remove(pathOrUrl);
    }
    if (Soundboard.SoundsPlaying.Contains(pathOrUrl))
      Soundboard.SoundsPlaying.Remove(pathOrUrl);
    if (Soundboard.SoundsPlaying.Contains(Soundboard.CurrentlyPlayingSoundName))
      Soundboard.SoundsPlaying.Remove(Soundboard.CurrentlyPlayingSoundName);
    Soundboard.CurrentlyPlayingSoundName = string.Empty;
    Soundboard._isPlayingSound = false;
  }

  private static void SetupRecorder(AudioSource source)
  {
    Recorder primaryRecorder = ((VoiceConnection) PhotonVoiceNetwork.Instance).PrimaryRecorder;
    primaryRecorder.SourceType = (Recorder.InputSourceType) 1;
    primaryRecorder.AudioClip = source.clip;
    primaryRecorder.RestartRecording(true);
    primaryRecorder.LoopAudioClip = Soundboard.loop;
  }

  public static void FixRecorder()
  {
    Recorder primaryRecorder = ((VoiceConnection) PhotonVoiceNetwork.Instance).PrimaryRecorder;
    primaryRecorder.SourceType = (Recorder.InputSourceType) 0;
    primaryRecorder.AudioClip = (AudioClip) null;
    primaryRecorder.RestartRecording(true);
    primaryRecorder.LoopAudioClip = false;
    Soundboard.currentlyPlayingSound = (string) null;
    Soundboard.isPlaying = false;
  }

  public static void CleanupAllSounds()
  {
    foreach (string str in Soundboard.soundboardSoundsActive.Keys.ToList<string>())
    {
      if (Soundboard.soundboardSoundsActive[str])
        Soundboard.StopSound(str);
    }
    foreach (KeyValuePair<string, AudioSource> keyValuePair in Soundboard.loadedSounds.ToList<KeyValuePair<string, AudioSource>>())
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
      {
        keyValuePair.Value.Stop();
        if ((UnityEngine.Object) keyValuePair.Value.clip != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value.clip);
        if ((UnityEngine.Object) ((Component) keyValuePair.Value).gameObject != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) ((Component) keyValuePair.Value).gameObject);
      }
    }
    Soundboard.loadedSounds.Clear();
    Soundboard.clipCache.Clear();
    Soundboard.soundboardSoundsActive.Clear();
    Soundboard.activeCoroutines.Clear();
    Soundboard.currentAudioSource = (AudioSource) null;
    Soundboard.currentlyPlayingSound = (string) null;
    Soundboard.isPlaying = false;
  }
}
