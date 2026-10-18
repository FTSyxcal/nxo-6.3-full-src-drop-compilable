
using System;
using System.Collections;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public class CoroutineHelper : MonoBehaviour
{
  public static CoroutineHelper Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) CoroutineHelper.Instance != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    else
    {
      CoroutineHelper.Instance = this;
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
    }
  }

  public static void InvokeAfterDelay(float time, Action afterDelay)
  {
    if (afterDelay == null)
      Debug.LogError((object) "Delay called with null action!");
    else
      CoroutineHelper.Instance.StartCoroutine(CoroutineHelper.InvokeAfterDelayCoroutine(time, afterDelay));
  }

  private static IEnumerator InvokeAfterDelayCoroutine(float time, Action afterDelay)
  {
    yield return (object) new WaitForSeconds(time);
    try
    {
      afterDelay();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Delay coroutine caught exception: {ex}");
    }
  }

  public static IEnumerator DestroyAfter(GameObject obj, float delay)
  {
    yield return (object) new WaitForSeconds(delay);
    if ((UnityEngine.Object) obj != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) obj);
  }
}
