
using NXO.Menu;
using NXO.Utilities;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

#nullable enable
namespace NXO.Mods.Categories;

public static class Nextbots
{
  public static readonly List<GameObject> ActiveNextbots = new List<GameObject>();
  private static PhysicsMaterial _slipMaterial;
  public static bool CrashOnJumpscare = false;
  public static ButtonHandler.Button nextbotSpeedButton;
  private static readonly (float speed, string desc)[] NextbotSpeedOptions = new (float, string)[5]
  {
    (2f, "Slow"),
    (3.5f, "Normal"),
    (5f, "Fast"),
    (7.5f, "Very Fast"),
    (11f, "Insane")
  };
  private static int _nbSpeedIndex = 1;
  public static float NextbotSpeed = 3.5f;
  public static string NextbotSpeedDescription = "Normal";
  public static ButtonHandler.Button nextbotBehaviourButton;
  private static readonly Nextbots.NextbotBehaviour[] BehaviourOptions = new Nextbots.NextbotBehaviour[3]
  {
    Nextbots.NextbotBehaviour.Aggressive,
    Nextbots.NextbotBehaviour.Ambusher,
    Nextbots.NextbotBehaviour.Stalker
  };
  private static int _nbBehaviourIndex = 0;
  public static Nextbots.NextbotBehaviour CurrentBehaviour = Nextbots.NextbotBehaviour.Aggressive;

  private static PhysicsMaterial SlipMaterial
  {
    get
    {
      PhysicsMaterial slipMaterial1 = Nextbots._slipMaterial;
      if (slipMaterial1 != null)
        return slipMaterial1;
      PhysicsMaterial slipMaterial2 = new PhysicsMaterial("NextbotSlip");
      slipMaterial2.dynamicFriction = 0.0f;
      slipMaterial2.staticFriction = 0.0f;
      slipMaterial2.bounciness = 0.0f;
      slipMaterial2.frictionCombine = (PhysicsMaterialCombine) 2;
      slipMaterial2.bounceCombine = (PhysicsMaterialCombine) 2;
      Nextbots._slipMaterial = slipMaterial2;
      return slipMaterial2;
    }
  }

  public static void CycleNextbotSpeed(bool forward)
  {
    Nextbots._nbSpeedIndex = forward ? (Nextbots._nbSpeedIndex + 1) % Nextbots.NextbotSpeedOptions.Length : (Nextbots._nbSpeedIndex - 1 + Nextbots.NextbotSpeedOptions.Length) % Nextbots.NextbotSpeedOptions.Length;
    (Nextbots.NextbotSpeed, Nextbots.NextbotSpeedDescription) = Nextbots.NextbotSpeedOptions[Nextbots._nbSpeedIndex];
    Nextbots.nextbotSpeedButton?.SetText("Nextbot Speed : " + Nextbots.NextbotSpeedDescription);
  }

  public static void CycleNextbotBehaviour(bool forward)
  {
    Nextbots._nbBehaviourIndex = forward ? (Nextbots._nbBehaviourIndex + 1) % Nextbots.BehaviourOptions.Length : (Nextbots._nbBehaviourIndex - 1 + Nextbots.BehaviourOptions.Length) % Nextbots.BehaviourOptions.Length;
    Nextbots.CurrentBehaviour = Nextbots.BehaviourOptions[Nextbots._nbBehaviourIndex];
    Nextbots.nextbotBehaviourButton?.SetText($"Behaviour : {Nextbots.CurrentBehaviour}");
  }

  public static void ApplyNextbotSpeed(string desc)
  {
    for (int index = 0; index < Nextbots.NextbotSpeedOptions.Length; ++index)
    {
      if (Nextbots.NextbotSpeedOptions[index].desc == desc)
      {
        Nextbots._nbSpeedIndex = index;
        (Nextbots.NextbotSpeed, Nextbots.NextbotSpeedDescription) = Nextbots.NextbotSpeedOptions[index];
        Nextbots.nextbotSpeedButton?.SetText("Nextbot Speed : " + Nextbots.NextbotSpeedDescription);
        break;
      }
    }
  }

  public static void ApplyNextbotBehaviour(string name)
  {
    for (int index = 0; index < Nextbots.BehaviourOptions.Length; ++index)
    {
      if (Nextbots.BehaviourOptions[index].ToString() == name)
      {
        Nextbots._nbBehaviourIndex = index;
        Nextbots.CurrentBehaviour = Nextbots.BehaviourOptions[index];
        Nextbots.nextbotBehaviourButton?.SetText($"Behaviour : {Nextbots.CurrentBehaviour}");
        break;
      }
    }
  }

  public static void SpawnNextbot(
    string imageUrl,
    string audioUrl,
    float speed = 3.5f,
    float size = 2f,
    string name = "",
    string jumpscareUrl = "")
  {
    if ((Object) CoroutineHelper.Instance == (Object) null)
      return;
    CoroutineHelper.Instance.StartCoroutine(Nextbots.SpawnRoutine(Nextbots.NormalizeUrl(imageUrl), Nextbots.NormalizeUrl(audioUrl), speed, size, name, Nextbots.NormalizeUrl(jumpscareUrl)));
  }

  private static string NormalizeUrl(string url)
  {
    if (string.IsNullOrEmpty(url))
      return url;
    url = url.Trim();
    if (url.Contains("github.com") && url.Contains("/blob/"))
      url = url.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");
    return url;
  }

  public static void ClearNextbots()
  {
    for (int index = 0; index < Nextbots.ActiveNextbots.Count; ++index)
    {
      if ((Object) Nextbots.ActiveNextbots[index] != (Object) null)
        Object.Destroy((Object) Nextbots.ActiveNextbots[index]);
    }
    Nextbots.ActiveNextbots.Clear();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Disabled, "Cleared Nextbots");
  }

  private static IEnumerator SpawnRoutine(
    string imageUrl,
    string audioUrl,
    float speed,
    float size,
    string name,
    string jumpscareUrl)
  {
    List<GifDecoder.Frame> frames = (List<GifDecoder.Frame>) null;
    AudioClip clip = (AudioClip) null;
    AudioClip jumpClip = (AudioClip) null;
    string label = string.IsNullOrEmpty(name) ? "" : name + " - ";
    if (!string.IsNullOrEmpty(imageUrl))
    {
      using (UnityWebRequest req = UnityWebRequest.Get(imageUrl))
      {
        yield return (object) req.SendWebRequest();
        if (req.result == (UnityWebRequest.Result) 1 && !(req.GetResponseHeader("Content-Type") ?? "").Contains("text/html"))
        {
          byte[] data = req.downloadHandler.data;
          if (GifDecoder.IsGif(data))
          {
            frames = GifDecoder.Decode(data);
          }
          else
          {
            Texture2D t = new Texture2D(2, 2);
            if (ImageConversion.LoadImage(t, data))
              frames = new List<GifDecoder.Frame>()
              {
                new GifDecoder.Frame() { texture = t, delay = 0.0f }
              };
            else
              Object.Destroy((Object) t);
            t = (Texture2D) null;
          }
          data = (byte[]) null;
        }
        if (frames == null || frames.Count == 0)
          NotificationLib.SendNotification(NotificationLib.NotificationType.Error, label + "Image link not working, double check it");
      }
    }
    if (!string.IsNullOrEmpty(audioUrl))
    {
      using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(audioUrl, Nextbots.GuessAudioType(audioUrl)))
      {
        ((DownloadHandlerAudioClip) req.downloadHandler).streamAudio = false;
        yield return (object) req.SendWebRequest();
        if (req.result == (UnityWebRequest.Result) 1 && !(req.GetResponseHeader("Content-Type") ?? "").Contains("text/html"))
          clip = DownloadHandlerAudioClip.GetContent(req);
        else
          NotificationLib.SendNotification(NotificationLib.NotificationType.Error, label + "Sound link not working, double check it");
      }
    }
    if (!string.IsNullOrEmpty(jumpscareUrl))
    {
      using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(jumpscareUrl, Nextbots.GuessAudioType(jumpscareUrl)))
      {
        ((DownloadHandlerAudioClip) req.downloadHandler).streamAudio = false;
        yield return (object) req.SendWebRequest();
        if (req.result == (UnityWebRequest.Result) 1 && !(req.GetResponseHeader("Content-Type") ?? "").Contains("text/html"))
          jumpClip = DownloadHandlerAudioClip.GetContent(req);
        else
          NotificationLib.SendNotification(NotificationLib.NotificationType.Error, label + "Jumpscare sound link not working, double check it");
      }
    }
    Nextbots.CreateNextbot(frames, clip, jumpClip, speed, size);
  }

  private static void CreateNextbot(
    List<GifDecoder.Frame> frames,
    AudioClip clip,
    AudioClip jumpClip,
    float speed,
    float size)
  {
    if ((Object) Variables.playerInstance == (Object) null)
      return;
    GameObject gameObject = new GameObject("NXO Nextbot");
    gameObject.layer = 8;
    Transform transform = ((Component) Variables.playerInstance.headCollider).transform;
    Vector3 vector3 = (Quaternion.Euler(0.0f, Random.Range(-80f, 80f), 0.0f) * transform.forward) with
    {
      y = 0.0f
    };
    if ((double) vector3.sqrMagnitude < 1.0 / 1000.0)
      vector3 = transform.forward;
    gameObject.transform.position = transform.position + vector3.normalized * 12f + Vector3.up * 4f;
    Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
    rigidbody.useGravity = true;
    rigidbody.constraints = (RigidbodyConstraints) 112 /*0x70*/;
    rigidbody.collisionDetectionMode = (CollisionDetectionMode) 2;
    rigidbody.interpolation = (RigidbodyInterpolation) 1;
    CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
    capsuleCollider.radius = size * 0.35f;
    capsuleCollider.height = size;
    ((Collider) capsuleCollider).sharedMaterial = Nextbots.SlipMaterial;
    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
    Object.DestroyImmediate((Object) primitive.GetComponent<MeshCollider>());
    primitive.transform.SetParent(gameObject.transform, false);
    primitive.transform.localScale = Vector3.one * size;
    Material material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Transparent") ?? Shader.Find("Standard"));
    Texture2D texture = frames == null || frames.Count <= 0 ? (Texture2D) null : frames[0].texture;
    if ((Object) texture != (Object) null)
      material.mainTexture = (Texture) texture;
    material.renderQueue = 4000;
    primitive.GetComponent<Renderer>().sharedMaterial = material;
    GifAnimator gifAnimator = gameObject.AddComponent<GifAnimator>();
    gifAnimator.frames = frames == null || frames.Count <= 0 ? (GifDecoder.Frame[]) null : frames.ToArray();
    gifAnimator.mat = material;
    if ((Object) clip != (Object) null || (Object) jumpClip != (Object) null)
    {
      AudioSource audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.clip = clip;
      audioSource.loop = true;
      audioSource.spatialBlend = 1f;
      audioSource.minDistance = 2f;
      audioSource.maxDistance = 60f;
      audioSource.rolloffMode = (AudioRolloffMode) 1;
      audioSource.volume = 0.1f;
      audioSource.playOnAwake = false;
    }
    bool flag = (Object) AstarPath.active != (Object) null;
    if (flag)
    {
      gameObject.AddComponent<Seeker>();
      gameObject.AddComponent<FunnelModifier>();
    }
    Nextbots.NextbotEntity nextbotEntity = gameObject.AddComponent<Nextbots.NextbotEntity>();
    nextbotEntity.visual = primitive.transform;
    nextbotEntity.behaviour = Nextbots.CurrentBehaviour;
    nextbotEntity.ambientClip = clip;
    nextbotEntity.jumpscareClip = jumpClip;
    Nextbots.ActiveNextbots.Add(gameObject);
    NotificationLib.SendNotification(NotificationLib.NotificationType.Enabled, flag ? "Nextbot Spawned (Pathfinding)" : "Nextbot Spawned");
  }

  private static AudioType GuessAudioType(string url)
  {
    string lower = url.ToLower();
    if (lower.Contains(".wav"))
      return AudioType.WAV;
    return lower.Contains(".ogg") ? AudioType.OGGVORBIS : AudioType.MPEG;
  }

  public enum NextbotBehaviour
  {
    Aggressive,
    Ambusher,
    Stalker,
  }

  public class NextbotEntity : MonoBehaviour
  {
    public Transform visual;
    public Nextbots.NextbotBehaviour behaviour = Nextbots.NextbotBehaviour.Aggressive;
    public AudioClip ambientClip;
    public AudioClip jumpscareClip;
    private Rigidbody _rb;
    private AudioSource _audio;
    private bool _caught;
    private bool _shaking;
    private GameObject _blackout;
    private Vector3 _lastPos;
    private float _stuckTime;
    private float _escapeTimer;
    private float _escapeSign = 1f;
    private bool _triggered;
    private Seeker _seeker;
    private Path _path;
    private int _waypoint;
    private bool _calculatingPath;
    private float _repathTimer;
    private bool _usePathfinding;
    private CapsuleCollider _col;
    private float _groundDist;
    private float _jumpTimer;
    private int _jumpStage;
    private const float RepathInterval = 0.3f;
    private const float WaypointRadius = 0.9f;
    private const float JumpTrigger = 5f;
    private const float JumpCooldown = 1f;
    private const float CatchRadius = 1.3f;
    private const float SeparationRadius = 3f;
    private const float SeparationWeight = 1.2f;
    private const float Accel = 32f;
    private const float AvoidRange = 4f;
    private const float AvoidWeight = 3f;
    private const float CatchUpRange = 9f;
    private const float CatchUpBoost = 1.4f;
    private const float StuckSpeed = 0.6f;
    private const float StuckTrigger = 0.3f;
    private const float EscapeTime = 1f;
    private const float AmbushRange = 5f;
    private const float StalkerBoost = 1.6f;
    private const float StalkerViewDot = 0.4f;

    private void Awake()
    {
      this._rb = this.GetComponent<Rigidbody>();
      this._audio = this.GetComponent<AudioSource>();
      this._seeker = this.GetComponent<Seeker>();
      this._usePathfinding = (Object) this._seeker != (Object) null;
      this._col = this.GetComponent<CapsuleCollider>();
      this._groundDist = (float) (((Object) this._col != (Object) null ? (double) this._col.height * 0.5 : 1.0) + 0.34999999403953552);
      this._lastPos = this.transform.position;
    }

    private void FixedUpdate()
    {
      if (this._caught || (Object) Variables.playerInstance == (Object) null || (Object) this._rb == (Object) null)
        return;
      Vector3 position = this.transform.position;
      Vector3 vector3_1 = (((Component) Variables.playerInstance.headCollider).transform.position - position) with
      {
        y = 0.0f
      };
      float magnitude1 = vector3_1.magnitude;
      if ((double) magnitude1 < 1.0 / 1000.0)
      {
        this._rb.linearVelocity = new Vector3(0.0f, this._rb.linearVelocity.y, 0.0f);
      }
      else
      {
        Vector3 vector3_2 = vector3_1 / magnitude1;
        float num1 = 1f;
        switch (this.behaviour)
        {
          case Nextbots.NextbotBehaviour.Ambusher:
            if (!this._triggered)
            {
              if ((double) magnitude1 < 5.0)
              {
                this._triggered = true;
                break;
              }
              this.Halt();
              return;
            }
            break;
          case Nextbots.NextbotBehaviour.Stalker:
            if (this.IsWatched(position))
            {
              this.HaltInstant();
              return;
            }
            num1 = 1.6f;
            break;
        }
        bool flag = false;
        if (this._usePathfinding)
        {
          this._repathTimer -= Time.fixedDeltaTime;
          if ((double) this._repathTimer <= 0.0 && !this._calculatingPath && (Object) AstarPath.active != (Object) null)
          {
            this._repathTimer = 0.3f;
            this._calculatingPath = true;
            this._seeker.StartPath(position, ((Component) Variables.playerInstance.headCollider).transform.position, new OnPathDelegate(this.OnPathComplete));
          }
          if (this.HasLineOfSight(position))
            flag = true;
          else if (this._path != null && this._path.vectorPath.Count > 0)
          {
            while (this._waypoint < this._path.vectorPath.Count - 1 && (double) Nextbots.NextbotEntity.HorizSqr(position, this._path.vectorPath[this._waypoint]) < 0.809999942779541)
              ++this._waypoint;
            Vector3 vector3_3 = (this._path.vectorPath[this._waypoint] - position) with
            {
              y = 0.0f
            };
            if ((double) vector3_3.sqrMagnitude > 9.9999997473787516E-05)
            {
              vector3_2 = vector3_3.normalized;
              flag = true;
            }
          }
        }
        Vector3 zero = Vector3.zero;
        for (int index = 0; index < Nextbots.ActiveNextbots.Count; ++index)
        {
          GameObject activeNextbot = Nextbots.ActiveNextbots[index];
          if (!((Object) activeNextbot == (Object) null) && !((Object) activeNextbot == (Object) this.gameObject))
          {
            Vector3 vector3_4 = (position - activeNextbot.transform.position) with
            {
              y = 0.0f
            };
            float magnitude2 = vector3_4.magnitude;
            if ((double) magnitude2 > 1.0 / 1000.0 && (double) magnitude2 < 3.0)
              zero += vector3_4 / (magnitude2 * magnitude2);
          }
        }
        Vector3 vector3_5;
        if ((double) this._escapeTimer > 0.0)
        {
          this._escapeTimer -= Time.fixedDeltaTime;
          Vector3 vector3_6 = Vector3.Cross(Vector3.up, vector3_2) * this._escapeSign;
          vector3_5 = vector3_2 * 0.4f + vector3_6;
        }
        else
        {
          Vector3 vector3_7 = flag ? Vector3.zero : this.Whisker(position, vector3_2, vector3_2, magnitude1) + this.Whisker(position, Quaternion.Euler(0.0f, 35f, 0.0f) * vector3_2, vector3_2, magnitude1) * 0.5f + this.Whisker(position, Quaternion.Euler(0.0f, -35f, 0.0f) * vector3_2, vector3_2, magnitude1) * 0.5f;
          vector3_5 = vector3_2 + zero * 1.2f + vector3_7 * 3f;
        }
        vector3_5.y = 0.0f;
        if ((double) vector3_5.sqrMagnitude > 9.9999997473787516E-05)
          vector3_5.Normalize();
        float num2 = (float) ((double) Nextbots.NextbotSpeed * (double) num1 * ((double) magnitude1 > 9.0 ? 1.3999999761581421 : 1.0));
        Vector3 vector3_8 = Vector3.MoveTowards(new Vector3(this._rb.linearVelocity.x, 0.0f, this._rb.linearVelocity.z), vector3_5 * num2, 32f * Time.fixedDeltaTime);
        this._rb.linearVelocity = new Vector3(vector3_8.x, this._rb.linearVelocity.y, vector3_8.z);
        float num3 = ((Component) Variables.playerInstance.headCollider).transform.position.y - position.y;
        if ((double) num3 > 5.0)
        {
          this._jumpTimer -= Time.fixedDeltaTime;
          if ((double) this._jumpTimer <= 0.0 && this.IsGrounded(position))
          {
            this._jumpTimer = 1f;
            this._jumpStage = Mathf.Min(this._jumpStage + 1, 3);
            float num4 = this._jumpStage == 1 ? 0.5f : (this._jumpStage == 2 ? 0.75f : 1f);
            float y = Mathf.Sqrt((float) (2.0 * (double) Mathf.Max(0.1f, -Physics.gravity.y) * ((double) num3 + 0.5))) * num4;
            Vector3 linearVelocity = this._rb.linearVelocity;
            this._rb.linearVelocity = new Vector3(linearVelocity.x, y, linearVelocity.z);
          }
        }
        else
          this._jumpStage = 0;
        float num5 = (position - this._lastPos).magnitude / Time.fixedDeltaTime;
        this._lastPos = position;
        if ((double) this._escapeTimer <= 0.0 && (double) num5 < 0.60000002384185791 && (double) magnitude1 > 1.7999999523162842)
        {
          this._stuckTime += Time.fixedDeltaTime;
          if ((double) this._stuckTime <= 0.30000001192092896)
            return;
          this._escapeSign = this.PickEscapeSide(position, vector3_2);
          this._escapeTimer = 1f;
          if (this._usePathfinding)
          {
            this._repathTimer = 0.0f;
            this._path = (Path) null;
          }
          this._stuckTime = 0.0f;
        }
        else
          this._stuckTime = 0.0f;
      }
    }

    private void Halt()
    {
      Vector3 linearVelocity = this._rb.linearVelocity;
      Vector3 vector3 = Vector3.MoveTowards(new Vector3(linearVelocity.x, 0.0f, linearVelocity.z), Vector3.zero, 32f * Time.fixedDeltaTime);
      this._rb.linearVelocity = new Vector3(vector3.x, linearVelocity.y, vector3.z);
      this._stuckTime = 0.0f;
      this._lastPos = this.transform.position;
    }

    private void HaltInstant()
    {
      this._rb.linearVelocity = new Vector3(0.0f, this._rb.linearVelocity.y, 0.0f);
      this._stuckTime = 0.0f;
      this._lastPos = this.transform.position;
    }

    private void UpdateAmbientAudio()
    {
      if ((Object) this._audio == (Object) null || (Object) this._audio.clip == (Object) null)
        return;
      bool flag = this.behaviour == Nextbots.NextbotBehaviour.Aggressive || this.behaviour == Nextbots.NextbotBehaviour.Ambusher && this._triggered;
      if (flag && !this._audio.isPlaying)
      {
        this._audio.Play();
      }
      else
      {
        if (flag || !this._audio.isPlaying)
          return;
        this._audio.Stop();
      }
    }

    private bool IsWatched(Vector3 pos)
    {
      Transform transform = !((Object) Variables.taggerInstance != (Object) null) || !((Object) Variables.taggerInstance.mainCamera != (Object) null) ? ((Object) Camera.main != (Object) null ? Camera.main.transform : ((Object) Variables.playerInstance != (Object) null ? ((Component) Variables.playerInstance.headCollider).transform : (Transform) null)) : Variables.taggerInstance.mainCamera.transform;
      if ((Object) transform == (Object) null)
        return false;
      Vector3 vector3 = pos - transform.position;
      float magnitude = vector3.magnitude;
      if ((double) magnitude < 0.0099999997764825821)
        return true;
      Vector3 rhs = vector3 / magnitude;
      return (double) Vector3.Dot(transform.forward, rhs) >= 0.40000000596046448;
    }

    private void OnPathComplete(Path p)
    {
      this._calculatingPath = false;
      if (p.error || p.vectorPath == null || p.vectorPath.Count == 0)
      {
        this._path = (Path) null;
      }
      else
      {
        this._path = p;
        this._waypoint = 0;
      }
    }

    private static float HorizSqr(Vector3 a, Vector3 b)
    {
      float num1 = a.x - b.x;
      float num2 = a.z - b.z;
      return (float) ((double) num1 * (double) num1 + (double) num2 * (double) num2);
    }

    private bool IsGrounded(Vector3 pos)
    {
      return Physics.Raycast(pos, Vector3.down, this._groundDist, Variables.NoInvisLayers());
    }

    private bool HasLineOfSight(Vector3 pos)
    {
      Vector3 vector3 = ((Component) Variables.playerInstance.headCollider).transform.position - pos;
      float magnitude = vector3.magnitude;
      return (double) magnitude < 0.5 || !Physics.Raycast(pos, vector3 / magnitude, magnitude - 0.5f, Variables.NoInvisLayers());
    }

    private float PickEscapeSide(Vector3 pos, Vector3 seek)
    {
      int num = Variables.NoInvisLayers();
      bool flag1 = Physics.Raycast(pos, Quaternion.Euler(0.0f, 55f, 0.0f) * seek, 4f, num);
      bool flag2 = Physics.Raycast(pos, Quaternion.Euler(0.0f, -55f, 0.0f) * seek, 4f, num);
      if (flag2 && !flag1)
        return 1f;
      return flag1 && !flag2 ? -1f : ((double) Random.value < 0.5 ? 1f : -1f);
    }

    private Vector3 Whisker(Vector3 pos, Vector3 dir, Vector3 seek, float playerDist)
    {
      float num = Mathf.Min(4f, playerDist - 1.5f);
      RaycastHit raycastHit;
      if ((double) num <= 0.10000000149011612 || !Physics.Raycast(pos, dir, out raycastHit, num, Variables.NoInvisLayers()))
        return Vector3.zero;
      Vector3 normal = (raycastHit).normal with
      {
        y = 0.0f
      };
      if ((double) normal.sqrMagnitude < 9.9999997473787516E-05)
        return Vector3.zero;
      normal.Normalize();
      Vector3 lhs = Vector3.Cross(Vector3.up, normal);
      if ((double) Vector3.Dot(lhs, seek) < 0.0)
        lhs = -lhs;
      return (lhs + normal * 0.35f) * (float) (1.0 - (double) (raycastHit).distance / 4.0);
    }

    private void Update()
    {
      if (this._caught || (Object) Variables.playerInstance == (Object) null)
        return;
      this.UpdateAmbientAudio();
      if ((double) Vector3.Distance(this.transform.position, ((Component) Variables.playerInstance.headCollider).transform.position) < 1.2999999523162842)
      {
        this.Catch();
      }
      else
      {
        if (!((Object) this.visual != (Object) null))
          return;
        Vector3 forward = ((Component) Variables.playerInstance.headCollider).transform.position - this.visual.position;
        if ((double) forward.sqrMagnitude > 9.9999997473787516E-05)
          this.visual.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0.0f, 180f, 0.0f) * Quaternion.Euler(0.0f, 0.0f, Mathf.Clamp(Vector3.Dot((Object) this._rb != (Object) null ? this._rb.linearVelocity : Vector3.zero, this.visual.right), -2f, 2f) * 6f + Mathf.Sin(Time.time * 5f) * 2.5f);
      }
    }

    private void Catch()
    {
      this._caught = true;
      if ((Object) this._rb != (Object) null)
      {
        this._rb.constraints = (RigidbodyConstraints) 0;
        this._rb.linearVelocity = Vector3.zero;
        this._rb.isKinematic = true;
      }
      Transform parent = !((Object) Variables.taggerInstance != (Object) null) || !((Object) Variables.taggerInstance.mainCamera != (Object) null) ? ((Object) Variables.playerInstance != (Object) null ? ((Component) Variables.playerInstance.headCollider).transform : (Transform) null) : Variables.taggerInstance.mainCamera.transform;
      if ((Object) parent == (Object) null)
        return;
      this._blackout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      Object.DestroyImmediate((Object) this._blackout.GetComponent<Collider>());
      this._blackout.transform.SetParent(parent, false);
      this._blackout.transform.localPosition = Vector3.zero;
      this._blackout.transform.localScale = Vector3.one * 1.6f;
      Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
      this._blackout.GetComponent<Renderer>().material = new Material(shader)
      {
        color = Color.black,
        renderQueue = 3000
      };
      if ((Object) this.visual != (Object) null)
      {
        this.visual.SetParent(parent, false);
        this.visual.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
        this.visual.localRotation = Quaternion.identity;
        this.visual.localScale = Vector3.one * 0.9f;
      }
      AudioSource audio = this._audio;
      if ((Object) audio != (Object) null)
      {
        audio.spatialBlend = 0.0f;
        audio.volume = 1f;
        audio.loop = true;
        if ((Object) this.jumpscareClip != (Object) null)
        {
          audio.Stop();
          audio.clip = this.jumpscareClip;
        }
        if ((Object) audio.clip != (Object) null && !audio.isPlaying)
          audio.Play();
      }
      this._shaking = true;
      this.StartCoroutine(this.ShakeRoutine());
      this.Invoke("EndJumpscare", 5f);
    }

    private IEnumerator ShakeRoutine()
    {
      if (!((Object) this.visual == (Object) null))
      {
        while (this._shaking)
        {
          this.visual.localPosition = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0.5f);
          this.visual.localRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-12f, 12f));
          this.visual.localScale = Vector3.one * Random.Range(0.8f, 1f);
          yield return (object) new WaitForSeconds(Random.Range(0.03f, 0.11f));
        }
      }
    }

    private void EndJumpscare()
    {
      if (Nextbots.CrashOnJumpscare)
        Process.GetCurrentProcess().Kill();
      else
        this.StartCoroutine(this.FadeOutAndDie());
    }

    private IEnumerator FadeOutAndDie()
    {
      this._shaking = false;
      GifAnimator gif = this.GetComponent<GifAnimator>();
      if ((Object) gif != (Object) null)
        gif.enabled = false;
      Renderer vr = (Object) this.visual != (Object) null ? this.visual.GetComponent<Renderer>() : (Renderer) null;
      Material vm = (Object) vr != (Object) null ? vr.sharedMaterial : (Material) null;
      if ((Object) vm != (Object) null)
        vm.color = Color.red;
      Renderer br = (Object) this._blackout != (Object) null ? this._blackout.GetComponent<Renderer>() : (Renderer) null;
      Material bm = (Object) br != (Object) null ? br.sharedMaterial : (Material) null;
      yield return (object) new WaitForSeconds(0.5f);
      float t = 0.0f;
      while ((double) t < 1.5)
      {
        t += Time.deltaTime;
        float a = Mathf.Clamp01((float) (1.0 - (double) t / 1.5));
        if ((Object) vm != (Object) null)
        {
          Color c = vm.color with { a = a };
          vm.color = c;
        }
        if ((Object) bm != (Object) null)
        {
          Color c = bm.color with { a = a };
          bm.color = c;
        }
        if ((Object) this._audio != (Object) null)
          this._audio.volume = a;
        yield return (object) null;
      }
      Object.Destroy((Object) this.gameObject);
    }

    private void OnDestroy()
    {
      Nextbots.ActiveNextbots.Remove(this.gameObject);
      if ((Object) this._blackout != (Object) null)
      {
        Renderer component = this._blackout.GetComponent<Renderer>();
        if ((Object) component != (Object) null && (Object) component.sharedMaterial != (Object) null)
          Object.Destroy((Object) component.sharedMaterial);
        Object.Destroy((Object) this._blackout);
      }
      if ((Object) this.visual != (Object) null)
        Object.Destroy((Object) this.visual.gameObject);
      if ((Object) this.ambientClip != (Object) null)
        Object.Destroy((Object) this.ambientClip);
      if (!((Object) this.jumpscareClip != (Object) null) || !((Object) this.jumpscareClip != (Object) this.ambientClip))
        return;
      Object.Destroy((Object) this.jumpscareClip);
    }
  }
}
