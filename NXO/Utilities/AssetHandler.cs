
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace NXO.Utilities;

public static class AssetHandler
{
  public static readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
  private static readonly Dictionary<string, Material> _cachedMaterials = new Dictionary<string, Material>();
  private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

  public static AssetBundle LoadAssetBundle(string resourceName)
  {
    AssetBundle assetBundle1;
    if (AssetHandler._loadedBundles.TryGetValue(resourceName, out assetBundle1))
      return assetBundle1;
    Stream stream = (Stream) null;
    try
    {
      stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
      if (stream == null)
        return (AssetBundle) null;
      byte[] array;
      using (MemoryStream destination = new MemoryStream())
      {
        stream.CopyTo((Stream) destination);
        array = destination.ToArray();
      }
      AssetBundle assetBundle2 = AssetBundle.LoadFromMemory(array);
      if ((UnityEngine.Object) assetBundle2 != (UnityEngine.Object) null)
        AssetHandler._loadedBundles[resourceName] = assetBundle2;
      return assetBundle2;
    }
    finally
    {
      stream?.Dispose();
    }
  }

  public static Material LoadMaterial(string bundleName, string materialName)
  {
    string key = $"{bundleName}_{materialName}";
    Material material1;
    if (AssetHandler._cachedMaterials.TryGetValue(key, out material1) && (UnityEngine.Object) material1 != (UnityEngine.Object) null)
      return material1;
    AssetBundle assetBundle = AssetHandler.LoadAssetBundle(bundleName);
    if ((UnityEngine.Object) assetBundle == (UnityEngine.Object) null)
      return (Material) null;
    Material material2 = (Material) null;
    foreach (string allAssetName in assetBundle.GetAllAssetNames())
    {
      if (string.Equals(Path.GetFileNameWithoutExtension(allAssetName), materialName, StringComparison.OrdinalIgnoreCase))
      {
        UnityEngine.Object @object = assetBundle.LoadAsset(allAssetName);
        if (@object is Material material3)
        {
          material2 = material3;
          break;
        }
        if (@object is GameObject gameObject)
        {
          Renderer component = gameObject.GetComponent<Renderer>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null && (UnityEngine.Object) component.sharedMaterial != (UnityEngine.Object) null)
          {
            material2 = component.sharedMaterial;
            break;
          }
        }
      }
    }
    if ((UnityEngine.Object) material2 == (UnityEngine.Object) null)
    {
      foreach (string allAssetName in assetBundle.GetAllAssetNames())
      {
        switch (assetBundle.LoadAsset(allAssetName))
        {
          case Material material4:
            material2 = material4;
            goto label_21;
          case Shader shader:
            material2 = new Material(shader);
            goto label_21;
          default:
            continue;
        }
      }
label_21:;
    }
    if ((UnityEngine.Object) material2 == (UnityEngine.Object) null)
    {
      Debug.LogWarning((object) $"[NXO] No usable material for '{materialName}' in bundle '{bundleName}'");
      return (Material) null;
    }
    AssetHandler._cachedMaterials[key] = material2;
    return material2;
  }

  public static void PlaySound(GameObject target, AudioClip clip, float volume = 1f)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) null || (UnityEngine.Object) clip == (UnityEngine.Object) null)
      return;
    AudioSource audioSource = target.GetComponent<AudioSource>();
    if ((UnityEngine.Object) audioSource == (UnityEngine.Object) null)
    {
      audioSource = target.AddComponent<AudioSource>();
      ((UnityEngine.Object) audioSource).hideFlags = HideFlags.HideAndDontSave;
    }
    audioSource.clip = clip;
    audioSource.volume = volume;
    audioSource.loop = false;
    audioSource.Play();
  }

  public static IEnumerator LoadEmbeddedAudioClip(string resourceName, Action<AudioClip> callback)
  {
    string ext = Path.GetExtension(resourceName)?.ToLowerInvariant();
    if (true)
      ;
    AudioType audioType1;
    switch (ext)
    {
      case ".wav":
        audioType1 = AudioType.WAV;
        break;
      case ".mp3":
        audioType1 = AudioType.MPEG;
        break;
      default:
        audioType1 = AudioType.UNKNOWN;
        break;
    }
    if (true)
      ;
    AudioType audioType = audioType1;
    if (audioType == AudioType.UNKNOWN)
    {
      Debug.LogWarning((object) ("[NXO] Unsupported audio format: " + ext));
    }
    else
    {
      string tempPath = Path.Combine(Application.temporaryCachePath, $"{Guid.NewGuid()}_{Path.GetFileName(resourceName)}");
      bool writeSuccess = false;
      try
      {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
          if (stream == null)
          {
            Debug.LogWarning((object) ("[NXO] Could not find embedded resource: " + resourceName));
            yield break;
          }
          using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            stream.CopyTo((Stream) fs);
        }
        writeSuccess = true;
      }
      catch (Exception ex)
      {
        Debug.LogWarning((object) ("[NXO] Failed to write temp audio file: " + ex.Message));
      }
      if (!writeSuccess)
      {
        AssetHandler.TryDeleteTemp(tempPath);
      }
      else
      {
        UnityWebRequest www = (UnityWebRequest) null;
        AudioClip clip = (AudioClip) null;
        bool loadSuccess = false;
        try
        {
          www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, audioType);
          ((DownloadHandlerAudioClip) www.downloadHandler).streamAudio = true;
        }
        catch (Exception ex)
        {
          Debug.LogWarning((object) ("[NXO] Failed to create web request: " + ex.Message));
          www?.Dispose();
          AssetHandler.TryDeleteTemp(tempPath);
          yield break;
        }
        yield return (object) www.SendWebRequest();
        try
        {
          if (www.result == (UnityWebRequest.Result) 1)
          {
            clip = DownloadHandlerAudioClip.GetContent(www);
            if ((UnityEngine.Object) clip != (UnityEngine.Object) null && clip.loadState == (AudioDataLoadState) 2)
            {
              ((UnityEngine.Object) clip).name = Path.GetFileNameWithoutExtension(resourceName);
              loadSuccess = true;
            }
            else
            {
              Debug.LogWarning((object) ("[NXO] Clip loaded but data not ready: " + resourceName));
              if ((UnityEngine.Object) clip != (UnityEngine.Object) null)
              {
                UnityEngine.Object.Destroy((UnityEngine.Object) clip);
                clip = (AudioClip) null;
              }
            }
          }
          else
            Debug.LogWarning((object) $"[NXO] Failed to load audio clip '{resourceName}': {www.error}");
        }
        finally
        {
          www.Dispose();
          AssetHandler.TryDeleteTemp(tempPath);
        }
        if (loadSuccess)
        {
          Action<AudioClip> action = callback;
          if (action != null)
            action(clip);
        }
      }
    }
  }

  private static void TryDeleteTemp(string path)
  {
    if (string.IsNullOrEmpty(path) || !File.Exists(path))
      return;
    try
    {
      File.Delete(path);
    }
    catch (Exception ex)
    {
      Debug.LogWarning((object) ("[NXO] Failed to delete temp file: " + ex.Message));
    }
  }

  public static Texture2D LoadEmbeddedTexture(string fileName)
  {
    Texture2D texture2D1;
    if (AssetHandler._textureCache.TryGetValue(fileName, out texture2D1))
      return texture2D1;
    Stream stream = (Stream) null;
    try
    {
      stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);
      if (stream == null)
        return (Texture2D) null;
      byte[] array;
      using (MemoryStream destination = new MemoryStream())
      {
        stream.CopyTo((Stream) destination);
        array = destination.ToArray();
      }
      Texture2D texture2D2 = new Texture2D(2, 2, TextureFormat.RGBA32, false);
      if (!ImageConversion.LoadImage(texture2D2, array))
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) texture2D2);
        return (Texture2D) null;
      }
      AssetHandler._textureCache[fileName] = texture2D2;
      return texture2D2;
    }
    finally
    {
      stream?.Dispose();
    }
  }

  public static IEnumerator LoadTextureFromUrl(GameObject target, string url)
  {
    if (!((UnityEngine.Object) target == (UnityEngine.Object) null) && !string.IsNullOrEmpty(url))
    {
      using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
      {
        yield return (object) uwr.SendWebRequest();
        if (uwr.result == (UnityWebRequest.Result) 1 && (UnityEngine.Object) target != (UnityEngine.Object) null)
        {
          Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
          Renderer renderer = target.GetComponent<Renderer>();
          if ((UnityEngine.Object) renderer != (UnityEngine.Object) null)
          {
            if ((UnityEngine.Object) renderer.material != (UnityEngine.Object) null)
              UnityEngine.Object.Destroy((UnityEngine.Object) renderer.material);
            Material mat = new Material(Variables.uiShader)
            {
              mainTexture = (Texture) texture
            };
            renderer.material = mat;
            mat = (Material) null;
          }
          texture = (Texture2D) null;
          renderer = (Renderer) null;
        }
      }
    }
  }

  public static void SetProperty(Material mat, string property, object value)
  {
    if ((UnityEngine.Object) mat == (UnityEngine.Object) null || !mat.HasProperty(property))
      return;
    switch (value)
    {
      case float num1:
        mat.SetFloat(property, num1);
        break;
      case Color color:
        mat.SetColor(property, color);
        break;
      case int num2:
        mat.SetInt(property, num2);
        break;
      case Texture texture:
        mat.SetTexture(property, texture);
        break;
    }
  }

  public static void CleanupAssets()
  {
    foreach (KeyValuePair<string, Texture2D> keyValuePair in AssetHandler._textureCache)
    {
      if ((UnityEngine.Object) keyValuePair.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) keyValuePair.Value);
    }
    AssetHandler._textureCache.Clear();
    foreach (KeyValuePair<string, Material> cachedMaterial in AssetHandler._cachedMaterials)
    {
      if ((UnityEngine.Object) cachedMaterial.Value != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) cachedMaterial.Value);
    }
    AssetHandler._cachedMaterials.Clear();
    foreach (KeyValuePair<string, AssetBundle> loadedBundle in AssetHandler._loadedBundles)
    {
      if ((UnityEngine.Object) loadedBundle.Value != (UnityEngine.Object) null)
        loadedBundle.Value.Unload(true);
    }
    AssetHandler._loadedBundles.Clear();
  }
}
