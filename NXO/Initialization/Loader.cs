
using HarmonyLib;
using System;
using UnityEngine;

#nullable disable
namespace NXO.Initialization;

public class Loader
{
  public static void Load()
  {
    try
    {
      new Harmony("com.nxo.nxomodmenu.org").PatchAll();
      Debug.Log((object) "NXO v6.3 initialized.");
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Failed to initialize {"NXO"}: {ex}");
    }
  }
}
