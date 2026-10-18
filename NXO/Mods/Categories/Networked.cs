
using NXO.Utilities;
using Photon.Pun;
using UnityEngine;

#nullable disable
namespace NXO.Mods.Categories;

public static class Networked
{
  public static class SizeChanger
  {
    private static float sizeScale = 1f;
    private static float lastSentScale = 1f;

    public static void NetworkedSizeChanger(bool enable)
    {
      if (!PhotonNetwork.InRoom)
        return;
      if (enable)
      {
        if (InputHandler.LSecondary())
          Networked.SizeChanger.sizeScale = 1f;
        if (InputHandler.LTrigger())
          Networked.SizeChanger.sizeScale -= 0.05f;
        if (InputHandler.RTrigger())
          Networked.SizeChanger.sizeScale += 0.05f;
        Networked.SizeChanger.sizeScale = Mathf.Clamp(Networked.SizeChanger.sizeScale, 0.375f, 2.75f);
        Variables.playerInstance.nativeScale = Networked.SizeChanger.sizeScale;
        if ((double) Networked.SizeChanger.sizeScale == (double) Networked.SizeChanger.lastSentScale)
          return;
        Networked.SizeChanger.lastSentScale = Networked.SizeChanger.sizeScale;
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.SizeChanger, new object[1]
        {
          (object) Networked.SizeChanger.sizeScale
        });
      }
      else
        NetworkingLibrary.SendNetworkUpdate(NetworkingLibrary.NetworkingType.SizeChanger, new object[1]
        {
          (object) 1f
        });
    }
  }
}
