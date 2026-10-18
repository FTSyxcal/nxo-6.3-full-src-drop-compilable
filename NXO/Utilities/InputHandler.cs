
using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

#nullable enable
namespace NXO.Utilities;

public static class InputHandler
{
  public static bool isOn;
  public static bool wasButtonPressed;
  public static bool isOn2;
  public static bool wasButtonPressed2;

  public static bool RTrigger()
  {
    return !ControllerEmulator.EmulatorEnabled ? (double) ControllerInputPoller.instance.rightControllerIndexFloat > 0.10000000149011612 : ControllerEmulator.GetRTrigger();
  }

  public static bool LTrigger()
  {
    return !ControllerEmulator.EmulatorEnabled ? (double) ControllerInputPoller.instance.leftControllerIndexFloat > 0.10000000149011612 : ControllerEmulator.GetLTrigger();
  }

  public static bool RGrip()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.rightGrab : ControllerEmulator.GetRGrip();
  }

  public static bool LGrip()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.leftGrab : ControllerEmulator.GetLGrip();
  }

  public static bool RPrimary()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.rightControllerPrimaryButton : ControllerEmulator.GetRPrimary();
  }

  public static bool LPrimary()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerEmulator.GetLPrimary();
  }

  public static bool RSecondary()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.rightControllerSecondaryButton : ControllerEmulator.GetRSecondary();
  }

  public static bool LSecondary()
  {
    return !ControllerEmulator.EmulatorEnabled ? ControllerInputPoller.instance.leftControllerSecondaryButton : ControllerEmulator.GetLSecondary();
  }

  public static bool None() => true;

  public static Vector2 GetJoystickAxis(bool left)
  {
    if (SteamVR.active)
      return left ? SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis((SteamVR_Input_Sources) 1) : SteamVR_Actions.gorillaTag_RightJoystick2DAxis.GetAxis((SteamVR_Input_Sources) 2);
    Vector2 zero = Vector2.zero;
    InputDevice inputDevice = left ? ControllerInputPoller.instance.leftControllerDevice : ControllerInputPoller.instance.rightControllerDevice;
    (inputDevice).TryGetFeatureValue(CommonUsages.primary2DAxis, out zero);
    return zero;
  }

  public static bool RJoystickClick()
  {
    if (ControllerEmulator.EmulatorEnabled)
      return false;
    bool flag = false;
    (ControllerInputPoller.instance.rightControllerDevice).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out flag);
    return flag;
  }

  public static bool LJoystickClick()
  {
    if (ControllerEmulator.EmulatorEnabled)
      return false;
    bool flag = false;
    (ControllerInputPoller.instance.leftControllerDevice).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out flag);
    return flag;
  }

  public static void ToggleOnButtonPress(
    Func<bool> buttonCheck,
    ref bool toggle,
    ref bool lastState)
  {
    bool flag = buttonCheck();
    if (!lastState & flag)
      toggle = !toggle;
    lastState = flag;
  }
}
