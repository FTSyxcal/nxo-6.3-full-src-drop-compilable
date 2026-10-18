
using NXO.Mods;
using NXO.Mods.Categories;
using NXO.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace NXO.Menu;

public class ButtonHandler
{
  private static float _lastClickTime;
  private static Coroutine _loadingCoroutine;
  private const float CLICK_COOLDOWN = 0.25f;
  public static List<string> favoriteMods = new List<string>();
  public static List<ButtonHandler.Button> FavoriteMods = new List<ButtonHandler.Button>();
  public static readonly List<ButtonHandler.SoundEntry> SoundSequence = new List<ButtonHandler.SoundEntry>()
  {
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.AssetBundle, "NXO.Resources.destiny.click", "destiny", "Destiny"),
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.AssetBundle, "NXO.Resources.thocky", "thocky", "Thocky"),
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.AssetBundle, "NXO.Resources.click1", "click", "Pop"),
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.AssetBundle, "NXO.Resources.minecraftclick", "minecraftclick", "Minecraft"),
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.HandTap, 67, "OG Button"),
    new ButtonHandler.SoundEntry(ButtonHandler.SoundType.HandTap, 66, "Key Switch")
  };
  private static readonly Dictionary<string, AudioClip> _customClickClips = new Dictionary<string, AudioClip>();
  private static string _currentOpenPreset;
  private static string _currentViewingPreset;
  private static readonly Dictionary<string, List<ButtonHandler.Button>> _presetCategoryButtons = new Dictionary<string, List<ButtonHandler.Button>>();
  public static ButtonHandler.Button _gearTargetButton;
  public static ButtonHandler.Button _bouncePending;
  public static ButtonHandler.Button _bounceIncParent;
  public static bool _bounceIncUp;
  private const string AutoSavePrefKey = "NXO_AutoSave";
  public static bool OverlapCustomClickSounds = false;
  private static readonly Dictionary<string, AudioClip> _preloadedClips = new Dictionary<string, AudioClip>();
  private static readonly List<AssetBundle> _loadedBundles = new List<AssetBundle>();

  private static string CustomClickSoundsFolderPath
  {
    get => Path.Combine(Variables.folderName, "Custom Click Sounds");
  }

  public static void LoadCustomClickSounds()
  {
    foreach (KeyValuePair<string, AudioClip> customClickClip in ButtonHandler._customClickClips)
    {
      if ((UnityEngine.Object) customClickClip.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) customClickClip.Value);
    }
    ButtonHandler._customClickClips.Clear();
    ButtonHandler.SoundSequence.RemoveAll((Predicate<ButtonHandler.SoundEntry>) (s => s.Type == ButtonHandler.SoundType.CustomFile));
    if (!Directory.Exists(ButtonHandler.CustomClickSoundsFolderPath))
    {
      Directory.CreateDirectory(ButtonHandler.CustomClickSoundsFolderPath);
      ButtonHandler.CreateCustomClickInstructions();
    }
    string[] array = ((IEnumerable<string>) Directory.GetFiles(ButtonHandler.CustomClickSoundsFolderPath, "*.*")).Where<string>((Func<string, bool>) (f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))).ToArray<string>();
    if (array.Length == 0)
    {
      ButtonHandler.SoundSequence.Add(new ButtonHandler.SoundEntry(ButtonHandler.SoundType.CustomFile, "", "", "Custom Placeholder"));
    }
    else
    {
      foreach (string str in array)
      {
        string withoutExtension = Path.GetFileNameWithoutExtension(str);
        ButtonHandler.SoundSequence.Add(new ButtonHandler.SoundEntry(ButtonHandler.SoundType.CustomFile, str, "", withoutExtension));
      }
    }
  }

  private static void CreateCustomClickInstructions()
  {
    string path = Path.Combine(ButtonHandler.CustomClickSoundsFolderPath, "Instructions.txt");
    if (File.Exists(path))
      return;
    File.WriteAllText(path, "=== HOW TO ADD CUSTOM CLICK SOUNDS ===\r\n\r\n1. Find an audio file (.mp3 or .wav)\r\n   - You can download sounds from YouTube using online converters\r\n   - Make sure it's one of these file types: MP3 or WAV\r\n\r\n2. Copy your audio file into this folder (Custom Click Sounds)\r\n   - The file name will be shown as the sound name in the menu\r\n   - Example: \"thock.wav\" will show as \"thock\" in the menu\r\n\r\n3. In-game, go to Settings > Click Sound and cycle to your custom sound\r\n   - It will appear at the end of the sound list\r\n\r\nTIPS:\r\n- Keep file names short and simple\r\n- Short sounds work best as click sounds\r\n- Don't use special characters in file names\r\n- You can add as many sounds as you want, each file becomes its own option\r\n\r\nThat's it! Have fun!");
  }

  public static IEnumerator LoadCustomClickClipCoroutine(
    string filePath,
    Action<AudioClip> callback)
  {
    AudioClip cached;
    if (ButtonHandler._customClickClips.TryGetValue(filePath, out cached))
    {
      Action<AudioClip> action = callback;
      if (action != null)
        action(cached);
    }
    else
    {
      string ext = Path.GetExtension(filePath).ToLowerInvariant();
      AudioType audioType = ext == ".wav" ? AudioType.WAV : AudioType.MPEG;
      using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, audioType))
      {
        yield return (object) www.SendWebRequest();
        if (www.result == (UnityWebRequest.Result) 1)
        {
          AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
          if ((UnityEngine.Object) clip != (UnityEngine.Object) null)
          {
            ((UnityEngine.Object) clip).name = Path.GetFileNameWithoutExtension(filePath);
            ButtonHandler._customClickClips[filePath] = clip;
            Action<AudioClip> action = callback;
            if (action != null)
              action(clip);
          }
          clip = (AudioClip) null;
        }
      }
    }
  }

  public static void CleanupCustomClickSounds()
  {
    foreach (KeyValuePair<string, AudioClip> customClickClip in ButtonHandler._customClickClips)
    {
      if ((UnityEngine.Object) customClickClip.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) customClickClip.Value);
    }
    ButtonHandler._customClickClips.Clear();
  }

  private static string AutoSaveDirectoryPath => Path.Combine(Variables.folderName, "Auto Save");

  private static string FavoriteModsFilePath
  {
    get => Path.Combine(Variables.folderName, "FavoriteMods.txt");
  }

  private static string PresetsDirectoryPath => Path.Combine(Variables.folderName, "Presets");

  public static void Toggle(ButtonHandler.Button button)
  {
    if (button == null)
      return;
    if (button.buttonText.EndsWith("_DOWN"))
    {
      string str = button.buttonText.Substring(0, button.buttonText.Length - 5);
      ButtonHandler.Button inCurrentContext = ButtonHandler.FindButtonInCurrentContext(str);
      if (inCurrentContext?.down == null)
        return;
      ButtonHandler.SafeInvoke(inCurrentContext.down, str);
      Main.RedrawButtonList();
    }
    else if (button.buttonText.EndsWith("_UP"))
    {
      string str = button.buttonText.Substring(0, button.buttonText.Length - 3);
      ButtonHandler.Button inCurrentContext = ButtonHandler.FindButtonInCurrentContext(str);
      if (inCurrentContext?.up == null)
        return;
      ButtonHandler.SafeInvoke(inCurrentContext.up, str);
      Main.RedrawButtonList();
    }
    else
    {
      switch (button.buttonText)
      {
        case "<":
          ButtonHandler.NavigatePage(false);
          break;
        case ">":
          ButtonHandler.NavigatePage(true);
          break;
        case "ReturnButton":
          ButtonHandler.ReturnToMainPage();
          break;
        case "Toggle Search Button":
          SearchAndKeyboard.ToggleKeyboard();
          break;
        case "Disconnect Button":
          ButtonHandler.SafeInvoke(button.onEnable, button.buttonText);
          break;
        default:
          ButtonHandler.ToggleButton(button);
          break;
      }
    }
  }

  public static void InvokeIncrement(ButtonHandler.Button button)
  {
    if (button == null)
      return;
    if (button.buttonText.EndsWith("_DOWN"))
    {
      string str = button.buttonText.Substring(0, button.buttonText.Length - 5);
      ButtonHandler.Button inCurrentContext = ButtonHandler.FindButtonInCurrentContext(str);
      if (inCurrentContext?.down == null)
        return;
      ButtonHandler.SafeInvoke(inCurrentContext.down, str);
    }
    else
    {
      if (!button.buttonText.EndsWith("_UP"))
        return;
      string str = button.buttonText.Substring(0, button.buttonText.Length - 3);
      ButtonHandler.Button inCurrentContext = ButtonHandler.FindButtonInCurrentContext(str);
      if (inCurrentContext?.up != null)
        ButtonHandler.SafeInvoke(inCurrentContext.up, str);
    }
  }

  private static ButtonHandler.Button FindButtonInCurrentContext(string targetName)
  {
    if (Variables.currentPage == Category.Element_Settings)
    {
      ButtonHandler.Button inCurrentContext = Settings.GenerateElementButtons().FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.buttonText == targetName));
      if (inCurrentContext != null)
        return inCurrentContext;
    }
    return ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.buttonText == targetName));
  }

  public static void NavigatePage(bool forward)
  {
    int num1 = (!SearchAndKeyboard.isSearching || string.IsNullOrWhiteSpace(SearchAndKeyboard.inputText) ? (IEnumerable<ButtonHandler.Button>) ButtonHandler.GetCurrentButtons() : (IEnumerable<ButtonHandler.Button>) ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.buttonText.Contains(SearchAndKeyboard.inputText, StringComparison.OrdinalIgnoreCase))).ToList<ButtonHandler.Button>()).Count<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && Settings.ShouldShowButton(b.buttonText)));
    if (num1 == 0)
      return;
    int num2 = Mathf.CeilToInt((float) num1 / (float) Variables.ButtonsPerPage);
    Variables.currentCategoryPage = forward ? (Variables.currentCategoryPage + 1) % num2 : (Variables.currentCategoryPage - 1 + num2) % num2;
    Main.RedrawButtonList();
  }

  private static void ReturnToMainPage()
  {
    Variables.currentPage = Category.Home;
    Variables.currentCategoryPage = 0;
    PlayersActionList.ClearPlayerCam(true);
    Main.RedrawButtonList(-1);
  }

  public static void ChangePage(Category newPage)
  {
    Variables.currentPage = newPage;
    Variables.currentCategoryPage = 0;
    Main._lastPageDrawn = Category.Home;
    Main._lastDrawModVersion = -1;
    int num;
    switch (newPage)
    {
      case Category.Player_Action:
        num = PlayersActionList.selectedPlayer != null ? 1 : 0;
        break;
      case Category.Players:
        PlayersActionList.GeneratePlayerButtons();
        goto label_8;
      default:
        num = 0;
        break;
    }
    if (num != 0)
    {
      PlayersActionList.GeneratePlayer_ActionButtons();
      PlayersActionList.AddPlayerCam();
    }
    else if (newPage == Category.Player_Mods && PlayersActionList.selectedPlayer != null)
      PlayersActionList.GeneratePlayer_Mods();
label_8:
    Main.RedrawButtonList(1);
  }

  private static void SafeInvoke(Action action, string label)
  {
    if (action == null)
      return;
    try
    {
      action();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"NXO action error '{label}': {ex.Message}\n{ex.StackTrace}");
      NotificationLib.SendNotification(NotificationLib.NotificationType.Error, $"`{label}` failed : {ex.Message}");
    }
  }

  public static void ToggleButton(ButtonHandler.Button button, bool redraw = true)
  {
    if (button == null)
      return;
    if (!button.isToggle)
    {
      ButtonHandler.SafeInvoke(button.onEnable, button.buttonText);
      NotificationLib.SendNotification(button.Page == Variables.currentPage ? NotificationLib.NotificationType.Enabled : NotificationLib.NotificationType.Info, button.buttonText);
    }
    else
    {
      button.Enabled = !button.Enabled;
      if (button.Enabled)
      {
        ButtonHandler.SafeInvoke(button.onEnable, button.buttonText);
        NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, button.buttonText);
        NXOUI.AddMod(button.buttonText);
      }
      else
      {
        ButtonHandler.SafeInvoke(button.onDisable, button.buttonText);
        NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, button.buttonText);
        NXOUI.RemoveMod(button.buttonText);
      }
      if (!redraw)
        return;
      Main.RedrawButtonList();
    }
  }

  public static List<ButtonHandler.Button> GetCurrentButtons()
  {
    return !SearchAndKeyboard.isSearching || string.IsNullOrWhiteSpace(SearchAndKeyboard.inputText) ? ButtonHandler.GetButtonInfoByPage(Variables.currentPage) : ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Page != Category.Home && b.buttonText.Contains(SearchAndKeyboard.inputText, StringComparison.OrdinalIgnoreCase))).ToList<ButtonHandler.Button>();
  }

  public static List<ButtonHandler.Button> GetButtonInfoByPage(Category page)
  {
    if (page == Category.Element_Settings)
      return Settings.GenerateElementButtons();
    if (page == Category.Saved_Presets)
    {
      string key = ButtonHandler._currentViewingPreset ?? ButtonHandler._currentOpenPreset;
      if (key != null)
      {
        List<ButtonHandler.Button> buttonList;
        return ButtonHandler._presetCategoryButtons.TryGetValue(key, out buttonList) ? buttonList : new List<ButtonHandler.Button>();
      }
    }
    if (page == Category.Recorded_Macros && Macros.currentOpenMacro != null)
    {
      List<ButtonHandler.Button> buttonList;
      return Macros.macroCategoryButtons.TryGetValue(Macros.currentOpenMacro, out buttonList) ? buttonList : new List<ButtonHandler.Button>();
    }
    if (page == Category.Custom_Nextbots && CustomNextbots.currentOpen != null)
      return CustomNextbots.GetActionButtons();
    if (true)
      ;
    List<ButtonHandler.Button> buttonInfoByPage;
    switch (page)
    {
      case Category.Enabled:
        buttonInfoByPage = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Enabled)).ToList<ButtonHandler.Button>();
        break;
      case Category.Favorited:
        buttonInfoByPage = ButtonHandler.GetFavoritedButtons();
        break;
      default:
        buttonInfoByPage = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Page == page)).ToList<ButtonHandler.Button>();
        break;
    }
    if (true)
      ;
    return buttonInfoByPage;
  }

  private static List<ButtonHandler.Button> GetFavoritedButtons()
  {
    List<ButtonHandler.Button> favoritedButtons = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("<color=blue>LG On Press To Favorite</color>", Category.Favorited, false, false, (Action) null)
    };
    favoritedButtons.AddRange(ButtonHandler.FavoriteMods.Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null)));
    return favoritedButtons;
  }

  public static void DisableAllMods()
  {
    foreach (ButtonHandler.Button button in ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Enabled)))
    {
      button.Enabled = false;
      Action onDisable = button.onDisable;
      if (onDisable != null)
        onDisable();
      NXOUI.RemoveMod(button.buttonText);
    }
    Main.RefreshMenu();
  }

  public static void SaveAutosavedStuff()
  {
    Directory.CreateDirectory(ButtonHandler.AutoSaveDirectoryPath);
    List<string> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Enabled)).Select<ButtonHandler.Button, string>((Func<ButtonHandler.Button, string>) (b => b.buttonText)).ToList<string>();
    File.WriteAllLines(Path.Combine(ButtonHandler.AutoSaveDirectoryPath, "Mods.txt"), (IEnumerable<string>) list);
    Settings.SaveSettings(Path.Combine(ButtonHandler.AutoSaveDirectoryPath, "Settings.txt"));
  }

  public static void SetAutoSave(bool on)
  {
    Variables.AutoSave = on;
    PlayerPrefs.SetInt("NXO_AutoSave", on ? 1 : 0);
    PlayerPrefs.Save();
  }

  public static void SetMenuAnimations(bool on) => Main.MenuAnimations = on;

  public static void LoadAutoSavePreference()
  {
    Variables.AutoSave = PlayerPrefs.GetInt("NXO_AutoSave", 1) == 1;
  }

  public static void LoadAutosavedStuff()
  {
    if (!Variables.AutoSave)
      return;
    string path1 = Path.Combine(ButtonHandler.AutoSaveDirectoryPath, "Mods.txt");
    string path2 = Path.Combine(ButtonHandler.AutoSaveDirectoryPath, "Settings.txt");
    if (File.Exists(path1))
    {
      HashSet<string> hashSet = ((IEnumerable<string>) File.ReadAllLines(path1)).ToHashSet<string>();
      foreach (ButtonHandler.Button button in ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null)))
      {
        if (button.Enabled && !hashSet.Contains(button.buttonText))
        {
          button.Enabled = false;
          Action onDisable = button.onDisable;
          if (onDisable != null)
            onDisable();
          NXOUI.RemoveMod(button.buttonText);
        }
        else if (!button.Enabled && hashSet.Contains(button.buttonText))
        {
          button.Enabled = true;
          Action onEnable = button.onEnable;
          if (onEnable != null)
            onEnable();
          NXOUI.AddMod(button.buttonText);
        }
      }
    }
    if (!File.Exists(path2))
      return;
    Settings.LoadSettings(path2);
  }

  public static void SaveFavoriteMods()
  {
    Directory.CreateDirectory(Variables.folderName);
    File.WriteAllLines(ButtonHandler.FavoriteModsFilePath, (IEnumerable<string>) ButtonHandler.favoriteMods);
    NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Favorite");
  }

  public static void LoadFavoritedMods()
  {
    if (!File.Exists(ButtonHandler.FavoriteModsFilePath))
      return;
    ButtonHandler.favoriteMods = ((IEnumerable<string>) File.ReadAllLines(ButtonHandler.FavoriteModsFilePath)).ToList<string>();
    foreach (string favoriteMod in ButtonHandler.favoriteMods)
    {
      string modName = favoriteMod;
      ButtonHandler.Button button = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).FirstOrDefault<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.buttonText == modName));
      if (button != null && !ButtonHandler.FavoriteMods.Contains(button))
        ButtonHandler.FavoriteMods.Add(button);
    }
  }

  public static void PlayClickSound()
  {
    if (Settings._currentSoundIndex < 0 || Settings._currentSoundIndex >= ButtonHandler.SoundSequence.Count)
      return;
    ButtonHandler.SoundEntry soundEntry = ButtonHandler.SoundSequence[Settings._currentSoundIndex];
    if (soundEntry.Type == ButtonHandler.SoundType.AssetBundle)
    {
      AudioClip clip;
      if (ButtonHandler._preloadedClips.TryGetValue(soundEntry.ClipName, out clip))
        AssetHandler.PlaySound(RigManager.GetHandObject, clip, soundEntry.ClipName == "destiny" ? 5f / 16f : 0.625f);
      else
        Debug.LogError((object) $"[NXO] Clip not preloaded: '{soundEntry.ClipName}'");
    }
    else if (soundEntry.Type == ButtonHandler.SoundType.CustomFile)
    {
      if (string.IsNullOrEmpty(soundEntry.AssetPath))
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Custom Sound Is Empty. To Add One Go To `Gorilla Tag > NXO Mod Menu > Custom Click Sounds`, Paste Your Sound There And Restart Your Game.");
      else if (ButtonHandler.OverlapCustomClickSounds)
      {
        if (!((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null))
          return;
        ((MonoBehaviour) Variables.taggerInstance).StartCoroutine(ButtonHandler.LoadCustomClickClipCoroutine(soundEntry.AssetPath, (Action<AudioClip>) (clip =>
        {
          if ((UnityEngine.Object) clip == (UnityEngine.Object) null || (UnityEngine.Object) RigManager.GetHandObject == (UnityEngine.Object) null)
            return;
          GameObject gameObject = new GameObject("CustomClickTemp");
          gameObject.transform.SetParent(RigManager.GetHandObject.transform);
          gameObject.transform.localPosition = Vector3.zero;
          AudioSource audioSource = gameObject.AddComponent<AudioSource>();
          audioSource.clip = clip;
          audioSource.volume = 0.45f;
          audioSource.spatialBlend = 1f;
          audioSource.Play();
          ((MonoBehaviour) Variables.taggerInstance).StartCoroutine(CoroutineHelper.DestroyAfter(gameObject, clip.length + 0.1f));
        })));
      }
      else
      {
        if (ButtonHandler._loadingCoroutine != null)
          ((MonoBehaviour) Variables.taggerInstance)?.StopCoroutine(ButtonHandler._loadingCoroutine);
        if ((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null)
          ButtonHandler._loadingCoroutine = ((MonoBehaviour) Variables.taggerInstance).StartCoroutine(ButtonHandler.LoadCustomClickClipCoroutine(soundEntry.AssetPath, (Action<AudioClip>) (clip =>
          {
            ButtonHandler._loadingCoroutine = (Coroutine) null;
            if (!((UnityEngine.Object) clip != (UnityEngine.Object) null))
              return;
            AssetHandler.PlaySound(RigManager.GetHandObject, clip, 0.625f);
          })));
      }
    }
    else if (soundEntry.Type == ButtonHandler.SoundType.EmbeddedWav)
    {
      if (ButtonHandler._loadingCoroutine != null)
        ((MonoBehaviour) Variables.taggerInstance)?.StopCoroutine(ButtonHandler._loadingCoroutine);
      if (!((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null))
        return;
      ButtonHandler._loadingCoroutine = ((MonoBehaviour) Variables.taggerInstance).StartCoroutine(AssetHandler.LoadEmbeddedAudioClip(soundEntry.AssetPath, (Action<AudioClip>) (clip =>
      {
        ButtonHandler._loadingCoroutine = (Coroutine) null;
        if (!((UnityEngine.Object) clip != (UnityEngine.Object) null))
          return;
        AssetHandler.PlaySound(RigManager.GetHandObject, clip);
      })));
    }
    else
    {
      if (!((UnityEngine.Object) Variables.taggerInstance?.offlineVRRig != (UnityEngine.Object) null))
        return;
      Variables.taggerInstance.offlineVRRig.PlayHandTapLocal(soundEntry.HandTapIndex, !Variables.rightHandedMenu, 0.625f);
    }
  }

  public static void PreloadAllClickSounds()
  {
    foreach (ButtonHandler.SoundEntry soundEntry in ButtonHandler.SoundSequence)
    {
      if (soundEntry.Type == 0 && !ButtonHandler._preloadedClips.ContainsKey(soundEntry.ClipName))
      {
        AssetBundle assetBundle = AssetHandler.LoadAssetBundle(soundEntry.AssetPath);
        if ((UnityEngine.Object) assetBundle == (UnityEngine.Object) null)
        {
          Debug.LogError((object) $"[NXO] Failed to preload bundle: '{soundEntry.AssetPath}'");
        }
        else
        {
          AudioClip audioClip = assetBundle.LoadAsset<AudioClip>(soundEntry.ClipName);
          if ((UnityEngine.Object) audioClip != (UnityEngine.Object) null)
          {
            ButtonHandler._preloadedClips[soundEntry.ClipName] = audioClip;
            ButtonHandler._loadedBundles.Add(assetBundle);
          }
          else
          {
            Debug.LogError((object) $"[NXO] Failed to preload clip '{soundEntry.ClipName}'");
            assetBundle.Unload(true);
          }
        }
      }
    }
  }

  public static void SavePreset()
  {
    Directory.CreateDirectory(ButtonHandler.PresetsDirectoryPath);
    string path2 = $"Preset_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
    string str = Path.Combine(ButtonHandler.PresetsDirectoryPath, path2);
    Directory.CreateDirectory(str);
    List<string> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Enabled)).Select<ButtonHandler.Button, string>((Func<ButtonHandler.Button, string>) (b => b.buttonText)).ToList<string>();
    File.WriteAllLines(Path.Combine(str, "Mods.txt"), (IEnumerable<string>) list);
    Settings.SaveSettings(Path.Combine(str, "Settings.txt"));
    NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"Preset `{path2}`");
    ButtonHandler.GeneratePresetButtons();
  }

  public static void GeneratePresetButtons()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b.Page != Category.Saved_Presets)).ToList<ButtonHandler.Button>();
    list.Add(new ButtonHandler.Button("Return", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Presets)))
    {
      isCategory = true
    });
    if (Directory.Exists(ButtonHandler.PresetsDirectoryPath))
    {
      foreach (string directory in Directory.GetDirectories(ButtonHandler.PresetsDirectoryPath))
      {
        string presetName = Path.GetFileName(directory);
        list.Add(new ButtonHandler.Button(presetName, Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.OpenPresetCategory(presetName))));
      }
    }
    ModButtons.buttons = list.ToArray();
    Main.MarkActiveTicksDirty();
  }

  private static void OpenPresetCategory(string presetName)
  {
    ButtonHandler._currentOpenPreset = presetName;
    ButtonHandler._currentViewingPreset = (string) null;
    if (!ButtonHandler._presetCategoryButtons.ContainsKey(presetName))
      ButtonHandler.GeneratePresetCategoryButtons(presetName);
    Variables.currentPage = Category.Saved_Presets;
    Main._lastPageDrawn = Category.Home;
    Main._lastDrawModVersion = -1;
    Main.RedrawButtonList();
  }

  private static void GeneratePresetCategoryButtons(string presetName)
  {
    ButtonHandler._presetCategoryButtons[presetName] = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Saved_Presets, false, false, (Action) (() =>
      {
        ButtonHandler._currentOpenPreset = (string) null;
        ButtonHandler._currentViewingPreset = (string) null;
        Main._lastPageDrawn = Category.Home;
        Main._lastDrawModVersion = -1;
        Main.RedrawButtonList();
      }))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Load This Preset", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.LoadSpecificPreset(presetName))),
      new ButtonHandler.Button("Rename Preset", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.BeginRenamePreset(presetName))),
      new ButtonHandler.Button("Delete This Preset", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.DeleteSpecificPreset(presetName))),
      new ButtonHandler.Button("View Saved Mods", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.ViewPresetMods(presetName))),
      new ButtonHandler.Button("View Saved Settings", Category.Saved_Presets, false, false, (Action) (() => ButtonHandler.ViewPresetSettings(presetName)))
    };
  }

  private static void BeginRenamePreset(string presetName)
  {
    SearchAndKeyboard.OpenTypingKeyboard(presetName, "Enter preset name...");
    SearchAndKeyboard.onTypingComplete = (Action<string>) (newName =>
    {
      newName = newName.Trim();
      if (string.IsNullOrEmpty(newName) || newName == presetName)
        return;
      string str1 = Path.Combine(ButtonHandler.PresetsDirectoryPath, presetName);
      string str2 = Path.Combine(ButtonHandler.PresetsDirectoryPath, newName);
      if (!Directory.Exists(str1))
        return;
      if (Directory.Exists(str2))
      {
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Preset Name Already Exists");
      }
      else
      {
        Directory.Move(str1, str2);
        ButtonHandler._presetCategoryButtons.Remove(presetName);
        ButtonHandler._presetCategoryButtons.Remove(newName);
        ButtonHandler._currentOpenPreset = (string) null;
        ButtonHandler._currentViewingPreset = (string) null;
        ButtonHandler.GeneratePresetButtons();
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"Renamed {presetName} → {newName}");
        Main.RedrawButtonList();
      }
    });
  }

  private static void ViewPresetMods(string presetName)
  {
    ButtonHandler._currentViewingPreset = presetName + "_Mods";
    string path = Path.Combine(ButtonHandler.PresetsDirectoryPath, presetName, "Mods.txt");
    List<ButtonHandler.Button> buttonList = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Saved_Presets, false, false, (Action) (() =>
      {
        ButtonHandler._currentViewingPreset = (string) null;
        ButtonHandler.GeneratePresetCategoryButtons(presetName);
        Main._lastPageDrawn = Category.Home;
        Main._lastDrawModVersion = -1;
        Main.RedrawButtonList();
      }))
      {
        isCategory = true
      }
    };
    if (File.Exists(path))
    {
      foreach (string readAllLine in File.ReadAllLines(path))
        buttonList.Add(new ButtonHandler.Button(readAllLine, Category.Saved_Presets, false, false, (Action) null));
    }
    ButtonHandler._presetCategoryButtons[ButtonHandler._currentViewingPreset] = buttonList;
    Variables.currentPage = Category.Saved_Presets;
    Main._lastPageDrawn = Category.Home;
    Main._lastDrawModVersion = -1;
    Main.RedrawButtonList();
  }

  private static void ViewPresetSettings(string presetName)
  {
    ButtonHandler._currentViewingPreset = presetName + "_Settings";
    string path = Path.Combine(ButtonHandler.PresetsDirectoryPath, presetName, "Settings.txt");
    List<ButtonHandler.Button> buttonList = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Saved_Presets, false, false, (Action) (() =>
      {
        ButtonHandler._currentViewingPreset = (string) null;
        ButtonHandler.GeneratePresetCategoryButtons(presetName);
        Main._lastPageDrawn = Category.Home;
        Main._lastDrawModVersion = -1;
        Main.RedrawButtonList();
      }))
      {
        isCategory = true
      }
    };
    if (File.Exists(path))
    {
      foreach (string readAllLine in File.ReadAllLines(path))
        buttonList.Add(new ButtonHandler.Button(readAllLine, Category.Saved_Presets, false, false, (Action) null));
    }
    ButtonHandler._presetCategoryButtons[ButtonHandler._currentViewingPreset] = buttonList;
    Variables.currentPage = Category.Saved_Presets;
    Main._lastPageDrawn = Category.Home;
    Main._lastDrawModVersion = -1;
    Main.RedrawButtonList();
  }

  private static void LoadSpecificPreset(string presetName)
  {
    string path1 = Path.Combine(ButtonHandler.PresetsDirectoryPath, presetName);
    string path2 = Path.Combine(path1, "Mods.txt");
    string path3 = Path.Combine(path1, "Settings.txt");
    if (File.Exists(path2))
    {
      HashSet<string> hashSet = ((IEnumerable<string>) File.ReadAllLines(path2)).ToHashSet<string>();
      foreach (ButtonHandler.Button button in ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null)))
      {
        if (button.Enabled && !hashSet.Contains(button.buttonText))
        {
          button.Enabled = false;
          Action onDisable = button.onDisable;
          if (onDisable != null)
            onDisable();
          NXOUI.RemoveMod(button.buttonText);
        }
        else if (!button.Enabled && hashSet.Contains(button.buttonText))
        {
          button.Enabled = true;
          Action onEnable = button.onEnable;
          if (onEnable != null)
            onEnable();
          NXOUI.AddMod(button.buttonText);
        }
      }
    }
    if (File.Exists(path3))
      Settings.LoadSettings(path3);
    Main.RefreshMenu();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, $"Preset `{presetName}`");
  }

  private static void DeleteSpecificPreset(string presetName)
  {
    string path = Path.Combine(ButtonHandler.PresetsDirectoryPath, presetName);
    if (!Directory.Exists(path))
      return;
    Directory.Delete(path, true);
    ButtonHandler._presetCategoryButtons.Remove(presetName);
    ButtonHandler._currentOpenPreset = (string) null;
    ButtonHandler._currentViewingPreset = (string) null;
    ButtonHandler.GeneratePresetButtons();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, $"Preset `{presetName}`");
    Main.RedrawButtonList();
  }

  public static void OpenFolder(string path)
  {
    Directory.CreateDirectory(path);
    Application.OpenURL("file:///" + path.Replace("\\", "/"));
  }

  public static void OpenGear_Menu(ButtonHandler.Button button)
  {
    ButtonHandler._gearTargetButton = button;
    ButtonHandler.ChangePage(Category.Gear_Menu);
  }

  public static bool IsCategoryButton(ButtonHandler.Button button) => button.isCategory;

  public class Button
  {
    private bool _enabled;

    public string buttonText { get; set; }

    public bool isToggle { get; set; }

    public bool Enabled
    {
      get => this._enabled;
      set
      {
        if (this._enabled == value)
          return;
        this._enabled = value;
        Main.MarkActiveTicksDirty();
      }
    }

    public Action onEnable { get; set; }

    public Action onDisable { get; set; }

    public bool incremental { get; set; }

    public Action up { get; set; }

    public Action down { get; set; }

    public Category Page { get; set; }

    public bool showGear { get; set; }

    public string tooltip { get; set; }

    public bool isCategory { get; set; }

    public Button(
      string label,
      Category page,
      bool isToggle,
      bool isActive,
      Action onClick,
      Action onDisable = null,
      bool incremental = false,
      Action incrementUp = null,
      Action incrementDown = null)
    {
      this.buttonText = label;
      this.isToggle = isToggle;
      this.onEnable = onClick;
      this.Page = page;
      this.onDisable = onDisable;
      this.Enabled = isActive;
      this.incremental = incremental;
      this.up = incrementUp;
      this.down = incrementDown;
    }

    public void SetText(string newText) => this.buttonText = newText;
  }

  public class BtnCollider : MonoBehaviour
  {
    public ButtonHandler.Button clickedButton;
    public readonly List<Transform> animBody = new List<Transform>(6);
    public readonly List<Vector3> animBodyBase = new List<Vector3>(6);
    public readonly List<Transform> animUniform = new List<Transform>(2);
    public readonly List<Vector3> animUniformBase = new List<Vector3>(2);
    private Coroutine _pressRoutine;
    private float _factor = 1f;
    private Transform _pulseKnob;
    private Renderer _pulseKnobRend;
    private readonly List<Renderer> _pulsePill = new List<Renderer>(2);
    private float _pulseOffY;
    private float _pulseOnY;
    private Color _pulsePillOff;
    private Color _pulsePillOn;
    private bool _hasPulse;
    public const float ToggleScale = 0.92f;
    private const float PressScale = 0.95f;
    private const float ShrinkDuration = 0.1f;
    private const float ReleaseDuration = 0.12f;

    public void SetupPulse(
      Transform knob,
      Renderer knobRend,
      float offY,
      float onY,
      List<Renderer> pill,
      Color pillOff,
      Color pillOn)
    {
      this._pulseKnob = knob;
      this._pulseKnobRend = knobRend;
      this._pulseOffY = offY;
      this._pulseOnY = onY;
      this._pulsePill.Clear();
      if (pill != null)
        this._pulsePill.AddRange((IEnumerable<Renderer>) pill);
      this._pulsePillOff = pillOff;
      this._pulsePillOn = pillOn;
      this._hasPulse = (UnityEngine.Object) knob != (UnityEngine.Object) null;
    }

    public void RegisterBody(Transform t)
    {
      if ((UnityEngine.Object) t == (UnityEngine.Object) null)
        return;
      this.animBody.Add(t);
      this.animBodyBase.Add(t.localScale);
    }

    public void RegisterUniform(Transform t)
    {
      if ((UnityEngine.Object) t == (UnityEngine.Object) null)
        return;
      this.animUniform.Add(t);
      this.animUniformBase.Add(t.localScale);
    }

    public void ApplyFactor(float f)
    {
      this._factor = f;
      for (int index = 0; index < this.animBody.Count; ++index)
      {
        Transform transform = this.animBody[index];
        if (!((UnityEngine.Object) transform == (UnityEngine.Object) null))
        {
          Vector3 vector3 = this.animBodyBase[index];
          transform.localScale = new Vector3(vector3.x, vector3.y * f, vector3.z * f);
        }
      }
      for (int index = 0; index < this.animUniform.Count; ++index)
      {
        Transform transform = this.animUniform[index];
        if (!((UnityEngine.Object) transform == (UnityEngine.Object) null))
          transform.localScale = this.animUniformBase[index] * f;
      }
    }

    public void OnTriggerEnter(Collider collider)
    {
      if ((UnityEngine.Object) ((Component) collider)?.gameObject == (UnityEngine.Object) null || ((Component) collider).gameObject.name != "buttonclicker" || (double) Time.time - (double) ButtonHandler._lastClickTime < 0.25)
        return;
      ButtonHandler._lastClickTime = Time.time;
      if ((UnityEngine.Object) Variables.taggerInstance != (UnityEngine.Object) null)
        Variables.VibrateClickHand(Variables.taggerInstance.tagHapticStrength / 2f, Variables.taggerInstance.tagHapticDuration / 2f);
      ButtonHandler.PlayClickSound();
      if (InputHandler.LGrip())
      {
        ButtonHandler.BtnCollider.TryAddModToFavorites(this.clickedButton);
        ButtonHandler.Toggle(this.clickedButton);
      }
      else if (!Main.MenuAnimations || this.animBody.Count == 0)
      {
        ButtonHandler.Toggle(this.clickedButton);
      }
      else
      {
        if (this._pressRoutine != null)
          this.StopCoroutine(this._pressRoutine);
        this._pressRoutine = this.StartCoroutine(this.PressRoutine());
      }
    }

    private IEnumerator PressRoutine()
    {
      ButtonHandler.Button btn = this.clickedButton;
      if (btn != null && btn.isToggle)
      {
        if (this._hasPulse)
        {
          bool wasOn = btn.Enabled;
          ButtonHandler.ToggleButton(btn, false);
          this._pressRoutine = (Coroutine) null;
          yield return (object) this.PulseLerp(wasOn ? 1f : 0.0f, wasOn ? 0.0f : 1f, 0.15f);
          Main.RedrawButtonList();
        }
        else if (Settings.ClassicButtons)
        {
          this._pressRoutine = (Coroutine) null;
          ButtonHandler._bouncePending = btn;
          ButtonHandler.Toggle(btn);
        }
        else
        {
          yield return (object) this.ScaleTo(btn.Enabled ? 1f : 0.92f, 0.1f);
          this._pressRoutine = (Coroutine) null;
          ButtonHandler.Toggle(btn);
        }
      }
      else if (btn != null && (btn.buttonText.EndsWith("_UP") || btn.buttonText.EndsWith("_DOWN")))
      {
        bool up = btn.buttonText.EndsWith("_UP");
        ButtonHandler._bounceIncParent = ButtonHandler.FindButtonInCurrentContext(btn.buttonText.Substring(0, btn.buttonText.Length - (up ? 3 : 5)));
        ButtonHandler._bounceIncUp = up;
        this._pressRoutine = (Coroutine) null;
        ButtonHandler.Toggle(btn);
      }
      else
      {
        this._pressRoutine = (Coroutine) null;
        ButtonHandler.Toggle(btn);
        if (this._hasPulse)
          this.StartCoroutine(this.PulseRoutine());
        yield return (object) this.ScaleTo(0.95f, 0.1f);
        yield return (object) this.ScaleTo(1f, 0.12f, true);
      }
    }

    public void PlayBounce()
    {
      if (!Main.MenuAnimations || this.animBody.Count == 0)
        return;
      if (this._pressRoutine != null)
        this.StopCoroutine(this._pressRoutine);
      this._pressRoutine = this.StartCoroutine(this.BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
      yield return (object) this.ScaleTo(0.95f, 0.1f);
      yield return (object) this.ScaleTo(1f, 0.12f, true);
      this._pressRoutine = (Coroutine) null;
    }

    private IEnumerator PulseRoutine()
    {
      yield return (object) this.PulseLerp(0.0f, 1f, 0.12f);
      yield return (object) new WaitForSecondsRealtime(0.1f);
      yield return (object) this.PulseLerp(1f, 0.0f, 0.16f);
    }

    private IEnumerator PulseLerp(float from, float to, float dur)
    {
      float e = 0.0f;
      while ((double) e < (double) dur)
      {
        this.ApplyPulse(Mathf.Lerp(from, to, e / dur));
        e += Time.unscaledDeltaTime;
        yield return (object) null;
      }
      this.ApplyPulse(to);
    }

    private void ApplyPulse(float p)
    {
      if ((UnityEngine.Object) this._pulseKnob != (UnityEngine.Object) null)
        this._pulseKnob.localPosition = this._pulseKnob.localPosition with
        {
          y = Mathf.Lerp(this._pulseOffY, this._pulseOnY, p)
        };
      Color color1 = Color.Lerp(this._pulsePillOff, this._pulsePillOn, p);
      for (int index = 0; index < this._pulsePill.Count; ++index)
      {
        Renderer renderer = this._pulsePill[index];
        if (!((UnityEngine.Object) renderer == (UnityEngine.Object) null) && !((UnityEngine.Object) renderer.sharedMaterial == (UnityEngine.Object) null))
        {
          Color color2 = color1 with
          {
            a = renderer.sharedMaterial.color.a
          };
          renderer.sharedMaterial.color = color2;
        }
      }
      if (!((UnityEngine.Object) this._pulseKnobRend != (UnityEngine.Object) null) || !((UnityEngine.Object) this._pulseKnobRend.sharedMaterial != (UnityEngine.Object) null))
        return;
      this._pulseKnobRend.sharedMaterial.color = Color.Lerp(Color.Lerp(this._pulsePillOff, Color.white, 0.6f), Color.Lerp(this._pulsePillOn, Color.white, 0.6f), p) with
      {
        a = this._pulseKnobRend.sharedMaterial.color.a
      };
    }

    private IEnumerator ScaleTo(float target, float duration, bool overshoot = false)
    {
      float from = this._factor;
      float e = 0.0f;
      while ((double) e < (double) duration)
      {
        float t = e / duration;
        float k = overshoot ? ButtonHandler.BtnCollider.EaseOutBack(t) : (float) (1.0 - (1.0 - (double) t) * (1.0 - (double) t) * (1.0 - (double) t));
        this.ApplyFactor(Mathf.LerpUnclamped(from, target, k));
        e += Time.unscaledDeltaTime;
        yield return (object) null;
      }
      this.ApplyFactor(target);
    }

    private static float EaseOutBack(float t)
    {
      float num = t - 1f;
      return (float) (1.0 + 4.0 * (double) num * (double) num * (double) num + 3.0 * (double) num * (double) num);
    }

    public static void TryAddModToFavorites(ButtonHandler.Button button)
    {
      if (button == null || string.IsNullOrEmpty(button.buttonText) || button.isCategory || !button.isToggle && button.onEnable == null)
        return;
      if (ButtonHandler.FavoriteMods.Contains(button))
      {
        ButtonHandler.FavoriteMods.Remove(button);
        ButtonHandler.favoriteMods.Remove(button.buttonText);
        Variables.taggerInstance?.offlineVRRig?.PlayHandTapLocal(28, !Variables.rightHandedMenu, 1f);
        NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, $"Unfavorited `{button.buttonText}`");
        ButtonHandler.SaveFavoriteMods();
        Main.RefreshMenu();
      }
      else
      {
        ButtonHandler.FavoriteMods.Add(button);
        ButtonHandler.favoriteMods.Add(button.buttonText);
        Variables.taggerInstance?.offlineVRRig?.PlayHandTapLocal(28, !Variables.rightHandedMenu, 1f);
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, $"Favorited `{button.buttonText}`");
        ButtonHandler.SaveFavoriteMods();
        Main.RefreshMenu();
      }
    }
  }

  public enum SoundType
  {
    AssetBundle,
    HandTap,
    EmbeddedWav,
    CustomFile,
  }

  public class SoundEntry
  {
    public ButtonHandler.SoundType Type;
    public string AssetPath;
    public string ClipName;
    public string Description;
    public int HandTapIndex;

    public SoundEntry(
      ButtonHandler.SoundType type,
      string resourcePath,
      string clipName,
      string description)
    {
      this.Type = type;
      this.AssetPath = resourcePath;
      this.ClipName = clipName;
      this.Description = description;
    }

    public SoundEntry(ButtonHandler.SoundType type, int index, string desc)
    {
      this.Type = type;
      this.HandTapIndex = index;
      this.Description = desc;
    }
  }
}
