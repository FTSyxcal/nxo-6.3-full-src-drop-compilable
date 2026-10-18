
using BepInEx;
using GorillaLocomotion.Climbing;
using NXO.Menu;
using NXO.Utilities;
using UnityEngine;
using Valve.VR;

#nullable enable
namespace NXO.Mods.Categories;

public class Movement
{
  public static bool useGripForSpeedBoost = false;
  public static bool noclipEnabled;
  public static bool pressedPrimary = false;
  public static bool GrippySurfaces = false;
  public static float CurrentCarSpeed = 0.0f;
  public static bool SlipperySurfaces = false;
  public static Vector3 OldMousePosition;
  public static float Sensitivity = 0.3f;
  public static bool didPress = false;
  public static bool isTriggerPlatforms = false;
  private static Vector3 wallContactPoint;
  private static Vector3 wallContactNormal;
  private static GameObject checkpoint;
  private static GameObject leftPlatform;
  private static GameObject rightPlatform;
  private static GameObject leftIce;
  private static GameObject rightIce;
  private static GameObject[] leftSides;
  private static GameObject[] rightSides;
  private static Material _platformMat;
  private static readonly Vector3 _platformScale = new Vector3(0.28f, 0.015f, 0.28f);
  private static readonly Vector3 _sideX = new Vector3(0.0075f, 0.15f, 0.15f);
  private static readonly Vector3 _sideY = new Vector3(0.15f, 0.0075f, 0.15f);
  private static readonly Vector3 _sideZ = new Vector3(0.15f, 0.15f, 0.0075f);
  public static (string name, string zone, string pos)[] _maps = new (string, string, string)[16 /*0x10*/]
  {
    ("Forest", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/TreeRoomSpawnForestZone", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Forest, Tree Exit"),
    ("City", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCity", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City Front"),
    ("Canyons", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestCanyonTransition", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Canyon"),
    ("Clouds", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToSkyJungle", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Clouds From Computer"),
    ("Caves", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToCave", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Cave"),
    ("Beach", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BeachToForest", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Beach for Computer"),
    ("Mountains", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToMountain", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Mountain"),
    ("Basement", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToBasement", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Basement For Computer"),
    ("Metropolis", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MetropolisOnly", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Metropolis from Computer"),
    ("Arcade", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToArcade", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City frm Arcade"),
    ("Critters", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityCrittersTransition", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - City from Critters"),
    ("Skate Park", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/ForestToHoverboard", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Hoverboard from Forest"),
    ("Monke Blocks", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/MonkeBlocksElevatorExit", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/MonkeBlocksElevator/Triggers/JoinRoomTrigger"),
    ("Rotating", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/CityToRotating", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - Rotating Map"),
    ("Bayou", "Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/Regional Transition/BayouOnly", "Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/JoinPublicRoom - BayouComputer2"),
    ("Lava Forest", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/VIMExp1_SetZoneTrigger", "Environment Objects/05Maze_PersistentObjects/GhostReactorElevatorManager/VIMForestLavaElevator/Triggers/JoinRoomTrigger")
  };
  private static bool TouchLeft = false;
  private static bool TouchRight = false;
  private static bool rightClimbing;
  private static bool leftClimbing;
  private static GameObject climbable;

  private static bool PlatformInput(bool rightHand)
  {
    return !Movement.isTriggerPlatforms ? (!rightHand ? InputHandler.LGrip() : InputHandler.RGrip()) : (!rightHand ? InputHandler.LTrigger() : InputHandler.RTrigger());
  }

  private static void InitializeMaterial()
  {
    if (!((Object) Movement._platformMat == (Object) null))
      return;
    Movement._platformMat = new Material(Variables.uiShader);
    Movement._platformMat.color = Color.black;
  }

  private static GameObject CreatePlatform(
    Vector3 scale,
    Vector3 pos,
    Quaternion rot,
    Material mat,
    bool invisible)
  {
    GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
    primitive.transform.localScale = scale;
    primitive.transform.position = pos;
    primitive.transform.rotation = rot;
    Renderer component = primitive.GetComponent<Renderer>();
    component.material = mat;
    component.enabled = !invisible;
    return primitive;
  }

  private static void Platforms(
    ref GameObject platform,
    bool grabbing,
    Transform hand,
    bool invisible)
  {
    if (grabbing)
    {
      if (!((Object) platform == (Object) null))
        return;
      Movement.InitializeMaterial();
      platform = Movement.CreatePlatform(Movement._platformScale, hand.position + new Vector3(0.0f, -0.02f, 0.0f), hand.rotation * Quaternion.Euler(0.0f, 0.0f, -90f), Movement._platformMat, invisible);
    }
    else
    {
      if (!((Object) platform != (Object) null))
        return;
      Object.Destroy((Object) platform);
      platform = (GameObject) null;
    }
  }

  private static void StickyPlatforms(
    ref GameObject[] sides,
    bool grabbing,
    Transform hand,
    bool invisible)
  {
    if (grabbing)
    {
      if (sides != null)
        return;
      Movement.InitializeMaterial();
      sides = new GameObject[6];
      Vector3[] vector3Array1 = new Vector3[6]
      {
        hand.right * -0.075f,
        hand.right * 0.075f,
        hand.up * 0.075f,
        hand.up * -0.075f,
        hand.forward * 0.075f,
        hand.forward * -0.075f
      };
      Vector3[] vector3Array2 = new Vector3[6]
      {
        Movement._sideX,
        Movement._sideX,
        Movement._sideY,
        Movement._sideY,
        Movement._sideZ,
        Movement._sideZ
      };
      for (int index = 0; index < 6; ++index)
        sides[index] = Movement.CreatePlatform(vector3Array2[index], hand.position + vector3Array1[index], hand.rotation, Movement._platformMat, invisible);
    }
    else
    {
      if (sides == null)
        return;
      foreach (GameObject gameObject in sides)
      {
        if ((Object) gameObject != (Object) null)
          Object.Destroy((Object) gameObject);
      }
      sides = (GameObject[]) null;
    }
  }

  private static void FrozonePlatforms(ref GameObject ice, bool grabbing, Transform hand)
  {
    if (!grabbing)
      return;
    ice = GameObject.CreatePrimitive(PrimitiveType.Cube);
    ice.transform.localScale = Movement._platformScale;
    ice.transform.position = hand.position + new Vector3(0.0f, -0.06f, 0.0f);
    ice.transform.rotation = hand.rotation * Quaternion.Euler(0.0f, 0.0f, -90f);
    ice.GetComponent<Renderer>().material.color = new Color(0.525f, 0.839f, 0.847f);
    ice.AddComponent<GorillaSurfaceOverride>().overrideIndex = 59;
    Object.Destroy((Object) ice, 0.75f);
  }

  public static void TogglePlatforms()
  {
    Transform controllerTransform1 = Variables.playerInstance.LeftHand.controllerTransform;
    Transform controllerTransform2 = Variables.playerInstance.RightHand.controllerTransform;
    bool grabbing1 = Movement.PlatformInput(false);
    bool grabbing2 = Movement.PlatformInput(true);
    switch (Settings.PlatformType.ToLower())
    {
      case "normal":
        Movement.Platforms(ref Movement.leftPlatform, grabbing1, controllerTransform1, false);
        Movement.Platforms(ref Movement.rightPlatform, grabbing2, controllerTransform2, false);
        break;
      case "sticky":
        Movement.StickyPlatforms(ref Movement.leftSides, grabbing1, controllerTransform1, false);
        Movement.StickyPlatforms(ref Movement.rightSides, grabbing2, controllerTransform2, false);
        break;
      case "invisible":
        Movement.Platforms(ref Movement.leftPlatform, grabbing1, controllerTransform1, true);
        Movement.Platforms(ref Movement.rightPlatform, grabbing2, controllerTransform2, true);
        break;
      case "invisible sticky":
        Movement.StickyPlatforms(ref Movement.leftSides, grabbing1, controllerTransform1, true);
        Movement.StickyPlatforms(ref Movement.rightSides, grabbing2, controllerTransform2, true);
        break;
    }
  }

  public static void Frozone()
  {
    Movement.FrozonePlatforms(ref Movement.leftIce, Movement.PlatformInput(false), Variables.playerInstance.LeftHand.controllerTransform);
    Movement.FrozonePlatforms(ref Movement.rightIce, Movement.PlatformInput(true), Variables.playerInstance.RightHand.controllerTransform);
  }

  public static void Noclip()
  {
    bool flag = InputHandler.RTrigger();
    if (flag == Movement.noclipEnabled)
      return;
    Movement.noclipEnabled = flag;
    foreach (Collider collider in Object.FindObjectsOfType<MeshCollider>())
      collider.enabled = !Movement.noclipEnabled;
  }

  public static void TeleportToMap()
  {
    (string name, string zone, string pos) map = Movement._maps[Settings._selectedMapIndex];
    ((GorillaTriggerBox) Variables.FindObject(map.zone)?.GetComponent<GorillaSetZoneTrigger>())?.OnBoxTriggered();
    GameObject gameObject = Variables.FindObject(map.pos);
    Movement.TeleportTo(gameObject != null ? gameObject.transform.position : ((Component) VRRig.LocalRig).transform.position);
  }

  public static void TeleportTo(Vector3 position)
  {
    Vector3 vector3 = position - ((Component) Variables.taggerInstance.bodyCollider).transform.position + ((Component) Variables.taggerInstance).transform.position;
    Variables.playerInstance.TeleportTo(vector3, ((Component) Variables.playerInstance).transform.rotation, false, false);
  }

  public static void FlyMonke(bool useVelocity, float velocityMultiplier = 1.15f)
  {
    Rigidbody component = ((Component) Variables.playerInstance).GetComponent<Rigidbody>();
    if (!InputHandler.RPrimary())
      return;
    if (useVelocity)
    {
      component.linearVelocity += ((Component) Variables.playerInstance.headCollider).transform.forward * Settings.FlySpeed * velocityMultiplier * Time.deltaTime;
    }
    else
    {
      ((Component) Variables.playerInstance).transform.position += ((Component) Variables.playerInstance.headCollider).transform.forward * Time.deltaTime * Settings.FlySpeed;
      component.linearVelocity = Vector3.zero;
    }
  }

  public static void FlyNoclip()
  {
    bool flag = InputHandler.RPrimary();
    if (flag)
    {
      ((Component) Variables.playerInstance).transform.position += ((Component) Variables.playerInstance.headCollider).transform.forward * Time.deltaTime * Settings.FlySpeed;
      ((Component) Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }
    if (flag == Movement.noclipEnabled)
      return;
    Movement.noclipEnabled = flag;
    foreach (Collider collider in Object.FindObjectsOfType<MeshCollider>())
      collider.enabled = !Movement.noclipEnabled;
  }

  public static void UpAndDown()
  {
    Rigidbody component = ((Component) Variables.playerInstance).GetComponent<Rigidbody>();
    Transform transform = ((Component) Variables.playerInstance.bodyCollider).transform;
    if (InputHandler.RTrigger())
      component.velocity += transform.up * 20f * Time.deltaTime;
    if (!InputHandler.LTrigger())
      return;
    component.velocity -= transform.up * 20f * Time.deltaTime;
  }

  public static void WASDMovement()
  {
    Transform transform = ((Component) Variables.taggerInstance.headCollider).transform;
    float num1 = UnityInput.Current.GetKey(KeyCode.LeftShift) ? Settings.FlySpeed + 3f : Settings.FlySpeed;
    ((Component) Variables.playerInstance).GetComponent<Rigidbody>().linearVelocity = new Vector3(0.0f, 0.065f, 0.0f);
    Vector3 zero = Vector3.zero;
    if (UnityInput.Current.GetKey(KeyCode.W))
      zero += transform.forward;
    if (UnityInput.Current.GetKey(KeyCode.S))
      zero -= transform.forward;
    if (UnityInput.Current.GetKey(KeyCode.A))
      zero -= transform.right;
    if (UnityInput.Current.GetKey(KeyCode.D))
      zero += transform.right;
    if (UnityInput.Current.GetKey(KeyCode.Space))
      zero += transform.up;
    if (UnityInput.Current.GetKey(KeyCode.LeftControl))
      zero -= transform.up;
    ((Component) Variables.taggerInstance.headCollider).transform.position += zero.normalized * num1 * Time.deltaTime;
    if (UnityInput.Current.GetMouseButton(1))
    {
      Vector3 vector3 = UnityInput.Current.mousePosition - Movement.OldMousePosition;
      float num2 = 0.1f;
      Variables.taggerInstance.mainCamera.transform.localEulerAngles += new Vector3(-vector3.y * num2, vector3.x * num2, 0.0f);
    }
    Movement.OldMousePosition = UnityInput.Current.mousePosition;
  }

  public static void JoystickFly()
  {
    Rigidbody component = ((Component) Variables.playerInstance).GetComponent<Rigidbody>();
    Transform transform = ((Component) Variables.playerInstance.headCollider).transform;
    component.useGravity = false;
    component.linearVelocity = Vector3.zero;
    Vector2 axis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
    if ((double) axis.magnitude <= 0.10000000149011612)
      return;
    Vector3 vector3 = transform.forward * axis.y + transform.right * axis.x;
    ((Component) Variables.playerInstance).transform.position += vector3 * Time.deltaTime * Settings.FlySpeed;
  }

  public static void IronMonke(int flySpeed)
  {
    if (InputHandler.LGrip())
      ((Collider) Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce((float) flySpeed * -Variables.taggerInstance.leftHandTransform.right, (ForceMode) 5);
    if (!InputHandler.RGrip())
      return;
    ((Collider) Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce((float) flySpeed * Variables.taggerInstance.rightHandTransform.right, (ForceMode) 5);
  }

  public static void SpeedBoost()
  {
    if (Movement.useGripForSpeedBoost && !InputHandler.RGrip())
      return;
    Variables.playerInstance.maxJumpSpeed = Settings.SpeedboostSpeed;
    Variables.playerInstance.jumpMultiplier = Settings.SpeedboostMultiplier;
  }

  public static void PullMod()
  {
    if (!Variables.playerInstance.IsHandTouching(true) && Movement.TouchLeft || !Variables.playerInstance.IsHandTouching(false) && Movement.TouchRight)
    {
      Vector3 velocity = Variables.taggerInstance.rigidbody.velocity;
      ((Component) Variables.playerInstance).transform.position += new Vector3(velocity.x * 0.05f, 0.0f, velocity.z * 0.05f);
    }
    Movement.TouchLeft = Variables.playerInstance.IsHandTouching(true);
    Movement.TouchRight = Variables.playerInstance.IsHandTouching(false);
  }

  public static void DashAndAirJump(bool isDashEnabled, bool isAirJumpEnabled)
  {
    if (InputHandler.RPrimary() && !Movement.pressedPrimary)
    {
      Movement.pressedPrimary = true;
      if (isDashEnabled)
        ((Component) Variables.playerInstance).GetComponent<Rigidbody>().velocity = ((Component) Variables.playerInstance.headCollider).transform.forward * 9f;
      if (isAirJumpEnabled)
        ((Component) Variables.playerInstance).GetComponent<Rigidbody>().velocity += Vector3.up * 7f;
    }
    if (InputHandler.RPrimary())
      return;
    Movement.pressedPrimary = false;
  }

  public static void WallWalk()
  {
    if (Variables.playerInstance.LeftHand.wasColliding || Variables.playerInstance.RightHand.wasColliding)
    {
      RaycastHit lastHitInfoHand = Variables.playerInstance.lastHitInfoHand;
      Movement.wallContactPoint = (lastHitInfoHand).point;
      Movement.wallContactNormal = (lastHitInfoHand).normal;
    }
    if (!(Movement.wallContactPoint != Vector3.zero) || !InputHandler.RGrip() && !InputHandler.LGrip())
      return;
    ((Collider) Variables.playerInstance.bodyCollider).attachedRigidbody.AddForce(Movement.wallContactNormal * Settings.WallWalkStrength, (ForceMode) 5);
  }

  public static void GrippyHands(bool setActive) => Movement.GrippySurfaces = setActive;

  public static void SlippyHands(bool setActive) => Movement.SlipperySurfaces = setActive;

  public static void WallClimber(bool enable)
  {
    if (!enable)
    {
      Main.DestroyAndNullify<GameObject>(ref Movement.climbable);
    }
    else
    {
      if ((Object) Movement.climbable == (Object) null)
      {
        Movement.climbable = new GameObject("GR");
        Movement.climbable.AddComponent<GorillaClimbable>();
      }
      if (InputHandler.LGrip())
      {
        if (Variables.playerInstance.IsHandTouching(true) && !Movement.leftClimbing)
        {
          Movement.climbable.transform.position = Variables.taggerInstance.leftHandTransform.position;
          Movement.leftClimbing = true;
          Variables.playerInstance.BeginClimbing(Movement.climbable.AddComponent<GorillaClimbable>(), Variables.FindObject("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/LeftHand Controller/GorillaHandClimber").GetComponent<GorillaHandClimber>(), (GorillaClimbableRef) null);
        }
      }
      else
        Movement.leftClimbing = false;
      if (InputHandler.RGrip())
      {
        if (!Variables.playerInstance.IsHandTouching(false) || Movement.rightClimbing)
          return;
        Movement.climbable.transform.position = Variables.taggerInstance.rightHandTransform.position;
        Movement.rightClimbing = true;
        Variables.playerInstance.BeginClimbing(Movement.climbable.AddComponent<GorillaClimbable>(), Variables.FindObject("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/RightHand Controller/GorillaHandClimber").GetComponent<GorillaHandClimber>(), (GorillaClimbableRef) null);
      }
      else
        Movement.rightClimbing = false;
    }
  }

  public static void SlideControl(bool setActive)
  {
    Variables.playerInstance.slideControl = setActive ? 0.04f : 0.00425f;
  }

  public static void PlaceCheckPoint(bool setActive)
  {
    if (!setActive)
    {
      if (!((Object) Movement.checkpoint != (Object) null))
        return;
      Object.Destroy((Object) Movement.checkpoint);
      Movement.checkpoint = (GameObject) null;
    }
    else
    {
      if ((Object) Movement.checkpoint == (Object) null)
      {
        Movement.checkpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Movement.checkpoint.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Renderer component = Movement.checkpoint.GetComponent<Renderer>();
        if ((Object) component != (Object) null)
        {
          component.material.shader = Shader.Find("GUI/Text Shader");
          component.material.color = Color.red;
        }
        Movement.checkpoint.GetComponent<Collider>().enabled = false;
      }
      if (InputHandler.RGrip())
      {
        Movement.checkpoint.transform.position = Variables.playerInstance.RightHand.controllerTransform.position;
        Movement.checkpoint.GetComponent<Renderer>().material.color = Color.red;
      }
      else if (InputHandler.RTrigger())
      {
        if (!((Object) Movement.checkpoint != (Object) null))
          return;
        Renderer component = Movement.checkpoint.GetComponent<Renderer>();
        if ((Object) component != (Object) null)
          component.material.color = Color.green;
        ((Component) Variables.playerInstance).GetComponent<Rigidbody>().velocity = Vector3.zero;
        ((Component) Variables.playerInstance).transform.position = Movement.checkpoint.transform.position;
      }
      else
      {
        Renderer component = Movement.checkpoint.GetComponent<Renderer>();
        if ((Object) component != (Object) null)
          component.material.color = Color.red;
      }
    }
  }

  public static void FastSwim()
  {
    if (!Variables.playerInstance.InWater)
      return;
    ((Component) Variables.playerInstance).gameObject.GetComponent<Rigidbody>().velocity *= 1.03f;
  }

  public static void NoTagFreeze() => Variables.playerInstance.disableMovement = false;

  public static void LowGravity()
  {
    Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 6.5f, (ForceMode) 5);
  }

  public static void AntiGravity()
  {
    Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 9.81f, (ForceMode) 5);
  }

  public static void HighGravity()
  {
    Variables.taggerInstance.rigidbody.AddForce(Vector3.down * 8f, (ForceMode) 5);
  }

  public static void ReverseGravity()
  {
    Variables.taggerInstance.rigidbody.AddForce(Vector3.up * 19.62f, (ForceMode) 5);
  }
}
