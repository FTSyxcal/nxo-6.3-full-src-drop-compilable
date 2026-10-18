
using GorillaNetworking;
using NXO.Menu;
using NXO.Mods.Categories;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable enable
namespace NXO.Utilities;

public class CustomBoards : MonoBehaviour
{
  private const string TreeRoomPath = "Environment Objects/LocalObjects_Prefab/TreeRoom";
  private const string ForestPath = "Environment Objects/LocalObjects_Prefab/Forest";
  private const string MotdUrl = "https://api.github.com/repos/NuggetGT/NXO-Resources/contents/NXO-Menu-Status.txt";
  private const string ImageUrl = "https://github.com/NuggetGT/NXO-Resources/blob/main/NXO%20is%20sooooo%20awesome.png?raw=true";
  private const int ForestBoardIndex = 6;
  private const float BoardBrightness = 0.75f;
  private static readonly int SpeedId = Shader.PropertyToID("_Speed");
  private static readonly int Color1Id = Shader.PropertyToID("_COLOR1");
  private static readonly int Color2Id = Shader.PropertyToID("_COLOR2");
  private static bool _enabled = true;
  private Material _colorMat;
  private Material _pinwheelMat;
  private Material _current;
  private bool _applied;
  private bool _boardsFound;
  private bool _joinScreensFound;
  private readonly Dictionary<Renderer, Material> _boards = new Dictionary<Renderer, Material>();
  private readonly Dictionary<string, Renderer> _scenePlanes = new Dictionary<string, Renderer>();
  private readonly List<CustomBoards.JoinScreen> _joinScreens = new List<CustomBoards.JoinScreen>();
  private Texture2D _imageTex;
  private Material _imageMat;
  private Renderer _imageRenderer;
  private static readonly Dictionary<string, CustomBoards.BoardConfig> SceneBoards = new Dictionary<string, CustomBoards.BoardConfig>()
  {
    ["Canyon2"] = new CustomBoards.BoardConfig("Canyon/CanyonScoreboardAnchor/GorillaScoreBoard", new Vector3(-24.5019f, -28.7746f, 0.1f), new Vector3(270f, 0.0f, 0.0f), new Vector3(21.5946f, 1f, 22.1782f)),
    ["Skyjungle"] = new CustomBoards.BoardConfig("skyjungle/UI/Scoreboard/GorillaScoreBoard", new Vector3(-21.2764f, -32.1928f, 0.0f), new Vector3(270.2987f, 0.2f, 359.9f), new Vector3(21.6f, 0.1f, 20.4909f)),
    ["Mountain"] = new CustomBoards.BoardConfig("Mountain/MountainScoreboardAnchor/GorillaScoreBoard", Vector3.zero, Vector3.zero, Vector3.one),
    ["Metropolis"] = new CustomBoards.BoardConfig("MetroMain/ComputerArea/Scoreboard/GorillaScoreBoard", new Vector3(-25.1f, -31f, 0.1502f), new Vector3(270.1958f, 0.2086f, 0.0f), new Vector3(21f, 102.9727f, 21.4f)),
    ["Bayou"] = new CustomBoards.BoardConfig("BayouMain/ComputerArea/GorillaScoreBoardPhysical", new Vector3(-28.3419f, -26.851f, 0.3f), new Vector3(270f, 0.0f, 0.0f), new Vector3(21.3636f, 38f, 21f)),
    ["Beach"] = new CustomBoards.BoardConfig("BeachScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0.0f, 0.0f), new Vector3(21.2f, 2f, 21.6f)),
    ["Cave"] = new CustomBoards.BoardConfig("Cave_Main_Prefab/CrystalCaveScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0.0f, 0.0f), new Vector3(21.2f, 2f, 21.6f)),
    ["Rotating"] = new CustomBoards.BoardConfig("RotatingPermanentEntrance/UI (1)/RotatingScoreboard/RotatingScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -33.7126f, 0.1f), new Vector3(270.056f, 0.0f, 0.0f), new Vector3(21.2f, 2f, 21.6f)),
    ["MonkeBlocks"] = new CustomBoards.BoardConfig("Environment Objects/MonkeBlocksRoomPersistent/AtticScoreBoard/AtticScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -24.5091f, 0.57f), new Vector3(270.1856f, 0.1f, 0.0f), new Vector3(21.6f, 1.2f, 20.8f)),
    ["Basement"] = new CustomBoards.BoardConfig("Basement/BasementScoreboardAnchor/GorillaScoreBoard/", new Vector3(-22.1964f, -24.5091f, 0.57f), new Vector3(270.1856f, 0.1f, 0.0f), new Vector3(21.6f, 1.2f, 20.8f)),
    ["City"] = new CustomBoards.BoardConfig("City_Pretty/CosmeticsScoreboardAnchor/GorillaScoreBoard", new Vector3(-22.1964f, -34.9f, 0.57f), new Vector3(270f, 0.0f, 0.0f), new Vector3(21.6f, 2.4f, 22f))
  };

  public static void SetBoardsEnabled(bool enabled) => CustomBoards._enabled = enabled;

  private void Awake()
  {
    this._colorMat = new Material(Shader.Find("GorillaTag/UberShader"));
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
    SceneManager.sceneUnloaded += new UnityAction<Scene>(this.OnSceneUnloaded);
    this.StartCoroutine(this.SetupTreeRoom());
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
    SceneManager.sceneUnloaded -= new UnityAction<Scene>(this.OnSceneUnloaded);
    if ((Object) this._colorMat != (Object) null)
      Object.Destroy((Object) this._colorMat);
    if ((Object) this._pinwheelMat != (Object) null)
      Object.Destroy((Object) this._pinwheelMat);
    if ((Object) this._imageMat != (Object) null)
      Object.Destroy((Object) this._imageMat);
    if (!((Object) this._imageTex != (Object) null))
      return;
    Object.Destroy((Object) this._imageTex);
  }

  private void Update()
  {
    if ((Object) this._imageRenderer != (Object) null && (Object) this._imageRenderer.sharedMaterial != (Object) this._imageMat)
      this._imageRenderer.sharedMaterial = this._imageMat;
    if (!CustomBoards._enabled)
    {
      if (!this._applied)
        return;
      this.Revert();
    }
    else
    {
      if (!this._boardsFound)
        this.FindBoards();
      if (!this._joinScreensFound)
        this.FindJoinScreens();
      this._current = this.GetBoardMaterial();
      foreach (Renderer key in this._boards.Keys)
      {
        if ((Object) key != (Object) null && (Object) key.sharedMaterial != (Object) this._current)
          key.sharedMaterial = this._current;
      }
      foreach (Renderer renderer in this._scenePlanes.Values)
      {
        if (!((Object) renderer == (Object) null))
        {
          if ((Object) renderer.sharedMaterial != (Object) this._current)
            renderer.sharedMaterial = this._current;
          if (!this._applied)
            renderer.gameObject.SetActive(true);
        }
      }
      bool flag = false;
      foreach (CustomBoards.JoinScreen joinScreen in this._joinScreens)
      {
        if (!((Object) joinScreen.Template == (Object) null) && !((Object) joinScreen.Template.ScreenBG_AlreadyInRoom == (Object) this._current))
        {
          CustomBoards.SetScreens(joinScreen.Template, this._current);
          flag = true;
        }
      }
      if (flag)
        PhotonNetworkController.Instance?.UpdateTriggerScreens();
      this._applied = true;
    }
  }

  private Material GetBoardMaterial()
  {
    if (Settings.BoardsMode == Settings.ColorMode.Pinwheel)
    {
      if ((Object) this._pinwheelMat == (Object) null)
        this._pinwheelMat = Main.GetPinwheelMaterial();
      if ((Object) this._pinwheelMat != (Object) null)
      {
        this._pinwheelMat.SetFloat(CustomBoards.SpeedId, -Main.pinwheelSpeed);
        this._pinwheelMat.SetColor(CustomBoards.Color1Id, Main.pinwheelColor1);
        this._pinwheelMat.SetColor(CustomBoards.Color2Id, Main.pinwheelColor2);
        return this._pinwheelMat;
      }
    }
    else if ((Object) this._pinwheelMat != (Object) null)
    {
      Object.Destroy((Object) this._pinwheelMat);
      this._pinwheelMat = (Material) null;
    }
    Color animatedColor = Settings.GetAnimatedColor(Settings.BoardsMode, (Color) Settings.BoardsColor, (Color) Settings.BoardsColor2, Settings.BoardsAnimSpeed, 6);
    this._colorMat.color = new Color(animatedColor.r * 0.75f, animatedColor.g * 0.75f, animatedColor.b * 0.75f, animatedColor.a);
    return this._colorMat;
  }

  private void Revert()
  {
    foreach (KeyValuePair<Renderer, Material> board in this._boards)
    {
      if ((Object) board.Key != (Object) null)
        board.Key.sharedMaterial = board.Value;
    }
    foreach (Renderer renderer in this._scenePlanes.Values)
    {
      if ((Object) renderer != (Object) null)
        renderer.gameObject.SetActive(false);
    }
    foreach (CustomBoards.JoinScreen joinScreen in this._joinScreens)
    {
      if ((Object) joinScreen.Template != (Object) null)
        CustomBoards.RestoreScreens(joinScreen);
    }
    if (this._joinScreens.Count > 0)
      PhotonNetworkController.Instance?.UpdateTriggerScreens();
    this._applied = false;
  }

  private void FindBoards()
  {
    Transform transform1 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom")?.transform;
    if ((Object) transform1 == (Object) null)
      return;
    this.PinTempFileNear(transform1, transform1.Find("COCBodyText_TitleData"));
    this.Pin(transform1.Find("TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen")?.GetComponent<Renderer>());
    Transform transform2 = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest")?.transform;
    if ((Object) transform2 != (Object) null)
      this.PinNthTempFile(transform2, 6);
    this._boardsFound = true;
  }

  private void FindJoinScreens()
  {
    List<GorillaNetworkJoinTrigger> allJoinTriggers = PhotonNetworkController.Instance?.allJoinTriggers;
    if (allJoinTriggers == null)
      return;
    foreach (GorillaNetworkJoinTrigger networkJoinTrigger in allJoinTriggers)
    {
      JoinTriggerUITemplate template = !((Object) networkJoinTrigger != (Object) null) || !((Object) networkJoinTrigger.ui != (Object) null) ? (JoinTriggerUITemplate) null : networkJoinTrigger.ui.template;
      if (!((Object) template == (Object) null))
        this._joinScreens.Add(new CustomBoards.JoinScreen()
        {
          Template = template,
          Original = new Material[8]
          {
            template.ScreenBG_AbandonPartyAndSoloJoin,
            template.ScreenBG_AlreadyInRoom,
            template.ScreenBG_ChangingGameModeSoloJoin,
            template.ScreenBG_Error,
            template.ScreenBG_InPrivateRoom,
            template.ScreenBG_LeaveRoomAndGroupJoin,
            template.ScreenBG_LeaveRoomAndSoloJoin,
            template.ScreenBG_NotConnectedSoloJoin
          }
        });
    }
    this._joinScreensFound = true;
  }

  private static void SetScreens(JoinTriggerUITemplate t, Material m)
  {
    t.ScreenBG_AbandonPartyAndSoloJoin = m;
    t.ScreenBG_AlreadyInRoom = m;
    t.ScreenBG_ChangingGameModeSoloJoin = m;
    t.ScreenBG_Error = m;
    t.ScreenBG_InPrivateRoom = m;
    t.ScreenBG_LeaveRoomAndGroupJoin = m;
    t.ScreenBG_LeaveRoomAndSoloJoin = m;
    t.ScreenBG_NotConnectedSoloJoin = m;
  }

  private static void RestoreScreens(CustomBoards.JoinScreen j)
  {
    JoinTriggerUITemplate template = j.Template;
    Material[] original = j.Original;
    template.ScreenBG_AbandonPartyAndSoloJoin = original[0];
    template.ScreenBG_AlreadyInRoom = original[1];
    template.ScreenBG_ChangingGameModeSoloJoin = original[2];
    template.ScreenBG_Error = original[3];
    template.ScreenBG_InPrivateRoom = original[4];
    template.ScreenBG_LeaveRoomAndGroupJoin = original[5];
    template.ScreenBG_LeaveRoomAndSoloJoin = original[6];
    template.ScreenBG_NotConnectedSoloJoin = original[7];
  }

  private void Pin(Renderer r)
  {
    if (!((Object) r != (Object) null) || this._boards.ContainsKey(r))
      return;
    this._boards.Add(r, r.sharedMaterial);
  }

  private void PinNthTempFile(Transform parent, int n)
  {
    int num = 0;
    foreach (Transform transform in parent)
    {
      if (transform.name.Contains("UnityTempFile") && num++ == n)
      {
        this.Pin(transform.GetComponent<Renderer>());
        break;
      }
    }
  }

  private void PinTempFileNear(Transform parent, Transform anchor)
  {
    if ((Object) anchor == (Object) null)
      return;
    Renderer r = (Renderer) null;
    float num = float.MaxValue;
    foreach (Transform transform in parent)
    {
      if (transform.name.Contains("UnityTempFile"))
      {
        Renderer component = transform.GetComponent<Renderer>();
        if (!((Object) component == (Object) null) && (double) component.bounds.SqrDistance(anchor.position) <= 0.25)
        {
          float sqrMagnitude = component.bounds.size.sqrMagnitude;
          if ((double) sqrMagnitude < (double) num)
          {
            r = component;
            num = sqrMagnitude;
          }
        }
      }
    }
    this.Pin(r);
  }

  private IEnumerator SetupTreeRoom()
  {
    string motd;
    using (UnityWebRequest req = UnityWebRequest.Get("https://api.github.com/repos/NuggetGT/NXO-Resources/contents/NXO-Menu-Status.txt"))
    {
      req.SetRequestHeader("Accept", "application/vnd.github.v3.raw");
      yield return (object) req.SendWebRequest();
      motd = req.result == (UnityWebRequest.Result) 1 ? req.downloadHandler.text.Trim() : "THIS VERSION IS OUTDATED! CHECK DISCORD.GG/NXOONTOP FOR UPDATES.";
    }
    int players;
    using (UnityWebRequest req = UnityWebRequest.Get(Variables.nxouri + "/api/ping"))
    {
      yield return (object) req.SendWebRequest();
      if (req.result != (UnityWebRequest.Result) 1 || !int.TryParse(req.downloadHandler.text, out players))
        players = -1;
    }
    yield return (object) new WaitForSeconds(2f);
    this.SetupCoCText(motd, players);
    yield return (object) this.ApplyCustomImage();
  }

  private void SetupCoCText(string motd, int players)
  {
    TextMeshPro component1 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText")?.GetComponent<TextMeshPro>();
    if ((Object) component1 != (Object) null)
    {
      ((TMP_Text) component1).text = "NXO PAID 6.3";
      ((Graphic) component1).color = Color.white;
      ((TMP_Text) component1).richText = true;
    }
    TextMeshPro component2 = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData")?.GetComponent<TextMeshPro>();
    if ((Object) component2 == (Object) null)
      return;
    PlayFabTitleDataTextDisplay component3 = ((Component) component2).GetComponent<PlayFabTitleDataTextDisplay>();
    if ((Object) component3 != (Object) null)
      ((Behaviour) component3).enabled = false;
    ((Graphic) component2).color = Color.white;
    ((TMP_Text) component2).richText = true;
    ((TMP_Text) component2).text = $"THANK YOU FOR BUYING NXO PAID!\n\nNXO IS NOT RESPONSIBLE FOR ANY ACTIONS\nTAKEN AGAINST YOUR ACCOUNT.\n\n{(players >= 0 ? $"NXO Users Online: {players}\n\n" : "")}{motd}\n\nCREATED BY: NUGGET\nDEVELOPERS: CATLICKER AND LIEX\nDISCORD.GG/NXOONTOP";
  }

  private IEnumerator ApplyCustomImage()
  {
    Renderer r = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/UnityTempFile-6e28226cf10e6734d804215b5deb7d7b (combined by EdMeshCombiner)")?.GetComponent<Renderer>();
    if (!((Object) r == (Object) null))
    {
      using (UnityWebRequest req = UnityWebRequestTexture.GetTexture("https://github.com/NuggetGT/NXO-Resources/blob/main/NXO%20is%20sooooo%20awesome.png?raw=true"))
      {
        yield return (object) req.SendWebRequest();
        if (req.result != (UnityWebRequest.Result) 1)
          yield break;
        this._imageTex = CustomBoards.RotateClockwise(DownloadHandlerTexture.GetContent(req));
      }
      this._imageMat = new Material(Shader.Find("Unlit/Texture"))
      {
        mainTexture = (Texture) this._imageTex,
        mainTextureScale = new Vector2(9.65f, 11.11f)
      };
      this._imageRenderer = r;
    }
  }

  private static Texture2D RotateClockwise(Texture2D tex)
  {
    int width = tex.width;
    int height = tex.height;
    Color32[] pixels32 = tex.GetPixels32();
    Color32[] colors = new Color32[pixels32.Length];
    for (int index1 = 0; index1 < height; ++index1)
    {
      for (int index2 = 0; index2 < width; ++index2)
        colors[(width - 1 - index2) * height + index1] = pixels32[index1 * width + index2];
    }
    Object.Destroy((Object) tex);
    Texture2D texture2D = new Texture2D(height, width, TextureFormat.RGBA32, false);
    texture2D.SetPixels32(colors);
    texture2D.Apply(false, true);
    return texture2D;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    CustomBoards.BoardConfig boardConfig;
    if (!CustomBoards.SceneBoards.TryGetValue(scene.name, out boardConfig))
      return;
    this.RemovePlane(scene.name);
    GameObject gameObject = GameObject.Find(boardConfig.Path);
    if ((Object) gameObject == (Object) null)
      return;
    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
    Object.Destroy((Object) primitive.GetComponent<Collider>());
    primitive.transform.SetParent(gameObject.transform, false);
    primitive.transform.localPosition = boardConfig.Pos;
    primitive.transform.localRotation = Quaternion.Euler(boardConfig.Rot);
    primitive.transform.localScale = boardConfig.Scale;
    Renderer component = primitive.GetComponent<Renderer>();
    component.sharedMaterial = (Object) this._current != (Object) null ? this._current : this._colorMat;
    primitive.SetActive(CustomBoards._enabled);
    this._scenePlanes[scene.name] = component;
  }

  private void OnSceneUnloaded(Scene scene) => this.RemovePlane(scene.name);

  private void RemovePlane(string sceneName)
  {
    Renderer renderer;
    if (!this._scenePlanes.TryGetValue(sceneName, out renderer))
      return;
    if ((Object) renderer != (Object) null)
      Object.Destroy((Object) renderer.gameObject);
    this._scenePlanes.Remove(sceneName);
  }

  private struct JoinScreen
  {
    public JoinTriggerUITemplate Template;
    public Material[] Original;
  }

  private struct BoardConfig(string p, Vector3 pos, Vector3 rot, Vector3 s)
  {
    public readonly string Path = p;
    public readonly Vector3 Pos = pos;
    public readonly Vector3 Rot = rot;
    public readonly Vector3 Scale = s;
  }
}
