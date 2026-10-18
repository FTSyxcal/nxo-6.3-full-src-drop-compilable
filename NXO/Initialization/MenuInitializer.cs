
using GorillaLocomotion;
using HarmonyLib;
using NXO.Menu;
using NXO.Utilities;
using UnityEngine;

#nullable enable
namespace NXO.Initialization;

[HarmonyPatch(typeof (GTPlayer), "LateUpdate")]
internal class MenuInitializer
{
  private static GameObject obj;

  [HarmonyPostfix]
  private static void Postfix()
  {
    if ((Object) MenuInitializer.obj != (Object) null)
      return;
    MenuInitializer.obj = new GameObject("NXO");
    MenuInitializer.obj.AddComponent<Main>();
    MenuInitializer.obj.AddComponent<NotificationLib>();
    MenuInitializer.obj.AddComponent<CustomBoards>();
    MenuInitializer.obj.AddComponent<NXOUI>();
    MenuInitializer.obj.AddComponent<NetworkingLibrary>();
    MenuInitializer.obj.AddComponent<Admin>();
    MenuInitializer.obj.AddComponent<CoroutineHelper>();
    Object.DontDestroyOnLoad((Object) MenuInitializer.obj);
    Debug.Log((object) "NXO initialized.");
  }
}
