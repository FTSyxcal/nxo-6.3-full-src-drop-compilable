
using BepInEx;

#nullable disable
namespace NXO.Initialization;

[BepInPlugin("com.nxo.nxomodmenu.org", "NXO", "6.3")]
public class MenuPlugin : BaseUnityPlugin
{
  private void Awake() => Loader.Load();
}
