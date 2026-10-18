
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public static class Logger
{
  public static bool Enabled = false;
  public static HashSet<byte> EventBlacklist = new HashSet<byte>()
  {
    (byte) 3,
    (byte) 200,
    (byte) 201,
    (byte) 203,
    (byte) 206
  };
  public static HashSet<string> RPCBlacklist = new HashSet<string>()
  {
    "OnHandTapRPCShared",
    "TransferOwnershipFromToRPC",
    "PieceDestroyedRPC",
    "TriggerAttractAnim"
  };

  public static void EnableAllLogging(bool enabled)
  {
    if (Logger.Enabled == enabled)
      return;
    Logger.Enabled = enabled;
    Debug.Log((object) ("[UltimateLogger] Logging " + (enabled ? "ENABLED" : "DISABLED")));
  }

  public static string FormatData(object data)
  {
    if (data == null)
      return "null";
    try
    {
      if (data is Dictionary<byte, object> source1)
        return $"{{{string.Join(", ", source1.Select<KeyValuePair<byte, object>, string>((Func<KeyValuePair<byte, object>, string>) (kvp => $"[{kvp.Key}]={Logger.FormatData(kvp.Value)}")))}}}";
      if (data is object[] source2)
      {
        string str = $"[{string.Join(", ", ((IEnumerable<object>) source2).Take<object>(10).Select<object, string>((Func<object, string>) (x => Logger.FormatData(x))))}]";
        if (source2.Length > 10)
          str += $" +{source2.Length - 10}";
        return str;
      }
      Hashtable hash = data as Hashtable;
      if (hash != null)
        return $"{{{string.Join(", ", ((Dictionary<object, object>) hash).Keys.Cast<object>().Select<object, string>((Func<object, string>) (k => $"{k}={Logger.FormatData(hash[k])}")))}}}";
      return data is byte[] numArray ? $"byte[{numArray.Length}]" : data.ToString();
    }
    catch
    {
      return data?.ToString() ?? "null";
    }
  }
}
