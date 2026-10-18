
using NXO.Menu;
using NXO.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public class Settings
{
  public static bool ClassicButtons;
  public static ButtonHandler.Button gunPointerSizeButton;
  public static ButtonHandler.Button gunLineThicknessButton;
  private static readonly (float v, string d)[] GunPointerSizeOptions = new (float, string)[5]
  {
    (0.06f, "Tiny"),
    (0.1f, "Small"),
    (0.15f, "Medium"),
    (0.2f, "Large"),
    (0.3f, "Huge")
  };
  private static int gunPointerSizeIndex = 1;
  public static float GunPointerSize = 0.1f;
  public static string GunPointerSizeDescription = "Small";
  private static readonly (float v, string d)[] GunLineThicknessOptions = new (float, string)[4]
  {
    (0.015f, "Thin"),
    (0.03f, "Normal"),
    (0.05f, "Thick"),
    (0.08f, "Chunky")
  };
  private static int gunLineThicknessIndex = 1;
  public static float GunLineThickness = 0.03f;
  public static string GunLineThicknessDescription = "Normal";
  public static readonly string[] ColorModeNames = new string[6]
  {
    "Solid",
    "Lerp",
    "Rainbow",
    "Strobe",
    "Pinwheel",
    "Gradient"
  };
  public static Settings.ColorMode BackgroundMode = Settings.ColorMode.Solid;
  public static Settings.ColorMode ButtonMode = Settings.ColorMode.Solid;
  public static Settings.ColorMode EnabledButtonMode = Settings.ColorMode.Solid;
  public static Settings.ColorMode TitleMode = Settings.ColorMode.Gradient;
  public static Settings.ColorMode OutlineColorMode = Settings.ColorMode.Pinwheel;
  public static Settings.ColorMode AccentStripMode = Settings.ColorMode.Solid;
  public static ButtonHandler.Button backgroundModeButton;
  public static ButtonHandler.Button buttonModeButton;
  public static ButtonHandler.Button enabledModeButton;
  public static ButtonHandler.Button titleModeButton;
  public static ButtonHandler.Button outlineColorModeButton;
  public static Settings.GradientDirection CurrentGradientDirection = Settings.GradientDirection.Horizontal;
  private static readonly string[] GradientDirectionNames = new string[3]
  {
    "Horizontal",
    "Vertical",
    "Diagonal"
  };
  private static int gradientDirectionIndex = 0;
  public static string GradientDirectionDescription = "Horizontal";
  public static ButtonHandler.Button gradientDirectionButton;
  public static Settings.ColorElement _currentColorElement;
  private static readonly Color32[] CustomColors = new Color32[90]
  {
    new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 230, (byte) 230, (byte) 230, byte.MaxValue),
    new Color32((byte) 180, (byte) 180, (byte) 180, byte.MaxValue),
    new Color32((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/, byte.MaxValue),
    new Color32((byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 80 /*0x50*/, byte.MaxValue),
    new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue),
    new Color32((byte) 15, (byte) 15, (byte) 15, byte.MaxValue),
    new Color32((byte) 0, (byte) 0, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 0, (byte) 0, byte.MaxValue),
    new Color32((byte) 220, (byte) 20, (byte) 60, byte.MaxValue),
    new Color32((byte) 178, (byte) 34, (byte) 34, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 99, (byte) 99, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 105, (byte) 180, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 20, (byte) 147, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 182, (byte) 193, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 192 /*0xC0*/, (byte) 203, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 0, (byte) 127 /*0x7F*/, byte.MaxValue),
    new Color32((byte) 231, (byte) 84, (byte) 128 /*0x80*/, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 127 /*0x7F*/, (byte) 80 /*0x50*/, byte.MaxValue),
    new Color32((byte) 250, (byte) 128 /*0x80*/, (byte) 114, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 165, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 140, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 69, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 215, (byte) 80 /*0x50*/, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 99, (byte) 71, byte.MaxValue),
    new Color32((byte) 210, (byte) 105, (byte) 30, byte.MaxValue),
    new Color32((byte) 139, (byte) 69, (byte) 19, byte.MaxValue),
    new Color32((byte) 160 /*0xA0*/, (byte) 82, (byte) 45, byte.MaxValue),
    new Color32(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 220, (byte) 80 /*0x50*/, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 215, (byte) 0, byte.MaxValue),
    new Color32((byte) 240 /*0xF0*/, (byte) 230, (byte) 140, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 250, (byte) 205, byte.MaxValue),
    new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue),
    new Color32((byte) 50, (byte) 205, (byte) 50, byte.MaxValue),
    new Color32((byte) 34, (byte) 139, (byte) 34, byte.MaxValue),
    new Color32((byte) 0, (byte) 128 /*0x80*/, (byte) 0, byte.MaxValue),
    new Color32((byte) 124, (byte) 252, (byte) 0, byte.MaxValue),
    new Color32((byte) 173, byte.MaxValue, (byte) 47, byte.MaxValue),
    new Color32((byte) 152, (byte) 251, (byte) 152, byte.MaxValue),
    new Color32((byte) 60, (byte) 179, (byte) 113, byte.MaxValue),
    new Color32((byte) 46, (byte) 204, (byte) 113, byte.MaxValue),
    new Color32((byte) 85, (byte) 107, (byte) 47, byte.MaxValue),
    new Color32((byte) 0, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 64 /*0x40*/, (byte) 224 /*0xE0*/, (byte) 208 /*0xD0*/, byte.MaxValue),
    new Color32((byte) 72, (byte) 209, (byte) 204, byte.MaxValue),
    new Color32((byte) 0, (byte) 206, (byte) 209, byte.MaxValue),
    new Color32((byte) 127 /*0x7F*/, byte.MaxValue, (byte) 212, byte.MaxValue),
    new Color32((byte) 0, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/, byte.MaxValue),
    new Color32((byte) 176 /*0xB0*/, (byte) 224 /*0xE0*/, (byte) 230, byte.MaxValue),
    new Color32((byte) 0, (byte) 0, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 30, (byte) 144 /*0x90*/, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 0, (byte) 191, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 135, (byte) 206, (byte) 235, byte.MaxValue),
    new Color32((byte) 70, (byte) 130, (byte) 180, byte.MaxValue),
    new Color32((byte) 100, (byte) 149, (byte) 237, byte.MaxValue),
    new Color32((byte) 25, (byte) 25, (byte) 112 /*0x70*/, byte.MaxValue),
    new Color32((byte) 65, (byte) 105, (byte) 225, byte.MaxValue),
    new Color32((byte) 180, (byte) 220, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 128 /*0x80*/, (byte) 0, (byte) 128 /*0x80*/, byte.MaxValue),
    new Color32((byte) 186, (byte) 85, (byte) 211, byte.MaxValue),
    new Color32((byte) 218, (byte) 112 /*0x70*/, (byte) 214, byte.MaxValue),
    new Color32((byte) 221, (byte) 160 /*0xA0*/, (byte) 221, byte.MaxValue),
    new Color32((byte) 147, (byte) 112 /*0x70*/, (byte) 219, byte.MaxValue),
    new Color32((byte) 138, (byte) 43, (byte) 226, byte.MaxValue),
    new Color32((byte) 75, (byte) 0, (byte) 130, byte.MaxValue),
    new Color32((byte) 139, (byte) 0, (byte) 139, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 0, byte.MaxValue, byte.MaxValue),
    new Color32((byte) 199, (byte) 21, (byte) 133, byte.MaxValue),
    new Color32((byte) 72, (byte) 61, (byte) 139, byte.MaxValue),
    new Color32((byte) 57, byte.MaxValue, (byte) 20, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 0, (byte) 234, byte.MaxValue),
    new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 244, (byte) 0, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 92, (byte) 0, byte.MaxValue),
    new Color32((byte) 191, (byte) 0, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 20, (byte) 100, byte.MaxValue),
    new Color32((byte) 0, (byte) 100, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 209, (byte) 220, byte.MaxValue),
    new Color32((byte) 200, (byte) 245, (byte) 220, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 253, (byte) 184, byte.MaxValue),
    new Color32((byte) 196, (byte) 197, byte.MaxValue, byte.MaxValue),
    new Color32(byte.MaxValue, (byte) 228, (byte) 196, byte.MaxValue),
    new Color32((byte) 200, (byte) 235, (byte) 245, byte.MaxValue),
    new Color32((byte) 245, (byte) 240 /*0xF0*/, (byte) 230, byte.MaxValue),
    new Color32((byte) 192 /*0xC0*/, (byte) 192 /*0xC0*/, (byte) 192 /*0xC0*/, byte.MaxValue),
    new Color32((byte) 212, (byte) 175, (byte) 55, byte.MaxValue),
    new Color32((byte) 176 /*0xB0*/, (byte) 141, (byte) 87, byte.MaxValue),
    new Color32((byte) 184, (byte) 115, (byte) 51, byte.MaxValue),
    new Color32((byte) 229, (byte) 228, (byte) 226, byte.MaxValue)
  };
  private static readonly string[] CustomColorNames = new string[90]
  {
    "White",
    "Off-White",
    "Light Gray",
    "Gray",
    "Medium Gray",
    "Dark Gray",
    "Near Black",
    "Black",
    "Red",
    "Crimson",
    "Firebrick",
    "Salmon",
    "Hot Pink",
    "Deep Pink",
    "Light Pink",
    "Pink",
    "Bubblegum",
    "Rose",
    "Coral",
    "Salmon Pink",
    "Orange",
    "Dark Orange",
    "Orange Red",
    "Tangerine",
    "Tomato",
    "Chocolate",
    "Saddle Brown",
    "Sienna",
    "Yellow",
    "Sunny Yellow",
    "Gold",
    "Khaki",
    "Cream",
    "Green",
    "Lime Green",
    "Forest Green",
    "Dark Green",
    "Lawn Green",
    "Green Yellow",
    "Pale Green",
    "Medium Sea Green",
    "Emerald",
    "Dark Olive",
    "Cyan",
    "Turquoise",
    "Medium Turquoise",
    "Dark Turquoise",
    "Aquamarine",
    "Teal",
    "Powder Blue",
    "Blue",
    "Dodger Blue",
    "Deep Sky Blue",
    "Sky Blue",
    "Steel Blue",
    "Cornflower",
    "Midnight Blue",
    "Royal Blue",
    "Pastel Blue",
    "Purple",
    "Medium Orchid",
    "Orchid",
    "Plum",
    "Medium Purple",
    "Blue Violet",
    "Indigo",
    "Dark Magenta",
    "Magenta",
    "Medium Violet Red",
    "Dark Slate Blue",
    "Neon Green",
    "Neon Pink",
    "Neon Cyan",
    "Neon Yellow",
    "Neon Orange",
    "Neon Purple",
    "Neon Red",
    "Neon Blue",
    "Pastel Pink",
    "Pastel Mint",
    "Pastel Yellow",
    "Pastel Lavender",
    "Pastel Peach",
    "Pastel Sky",
    "Pastel Cream",
    "Silver",
    "Antique Gold",
    "Bronze",
    "Copper",
    "Platinum"
  };
  public static ButtonHandler.Button pinwheelColor1Button;
  private static int pinwheelColor1Index = 72;
  public static Color32 PinwheelColor1 = new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static string PinwheelColor1Name = "Neon Cyan";
  public static ButtonHandler.Button pinwheelColor2Button;
  private static int pinwheelColor2Index = 0;
  public static Color32 PinwheelColor2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static string PinwheelColor2Name = "White";
  public static ButtonHandler.Button pinwheelSpeedButton;
  private static readonly (float speed, string desc)[] PinwheelSpeedOptions = new (float, string)[5]
  {
    (0.25f, "Slow"),
    (0.5f, "Normal"),
    (1f, "Fast"),
    (2f, "Very Fast"),
    (4f, "Strobe")
  };
  private static int pinwheelSpeedIndex = 1;
  public static string PinwheelSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustAccentStripAnimSpeedButton;
  private static int accentStripAnimSpeedIndex = 2;
  public static float AccentStripAnimSpeed = 1f;
  public static string AccentStripAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button accentStripModeButton;
  public static ButtonHandler.Button adjustAccentStripColorButton;
  public static ButtonHandler.Button adjustAccentStripColor2Button;
  private static int accentStripColorIndex = 5;
  private static int accentStripColor2Index = 1;
  public static Color32 AccentStripColor = new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue);
  public static Color32 AccentStripColor2 = new Color32((byte) 230, (byte) 230, (byte) 230, byte.MaxValue);
  public static string AccentStripColorDescription = "Dark Gray";
  public static string AccentStripColor2Description = "Off-White";
  private static readonly (float speed, string desc)[] AnimationSpeedOptions = new (float, string)[5]
  {
    (0.25f, "Very Slow"),
    (0.5f, "Slow"),
    (1f, "Normal"),
    (2f, "Fast"),
    (4f, "Very Fast")
  };
  public static ButtonHandler.Button adjustOutlineAnimSpeedButton;
  private static int outlineAnimSpeedIndex = 2;
  public static float OutlineAnimSpeed = 1f;
  public static string OutlineAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustBackgroundAnimSpeedButton;
  private static int backgroundAnimSpeedIndex = 2;
  public static float BackgroundAnimSpeed = 1f;
  public static string BackgroundAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustButtonAnimSpeedButton;
  private static int buttonAnimSpeedIndex = 2;
  public static float ButtonAnimSpeed = 1f;
  public static string ButtonAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustEnabledButtonAnimSpeedButton;
  private static int enabledButtonAnimSpeedIndex = 2;
  public static float EnabledButtonAnimSpeed = 1f;
  public static string EnabledButtonAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustTitleAnimSpeedButton;
  private static int titleAnimSpeedIndex = 2;
  public static float TitleAnimSpeed = 1f;
  public static string TitleAnimSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustOutlineColorButton;
  public static ButtonHandler.Button adjustOutlineColor2Button;
  private static int outlineColorIndex = 7;
  private static int outlineColor2Index = 1;
  public static Color32 OutlineColor = new Color32((byte) 0, (byte) 0, (byte) 0, byte.MaxValue);
  public static Color32 OutlineColor2 = new Color32((byte) 230, (byte) 230, (byte) 230, byte.MaxValue);
  public static string OutlineColorDescription = "Black";
  public static string OutlineColor2Description = "Off-White";
  public static int OutlineMode = 1;
  public static ButtonHandler.Button outlineModeButton;
  public static readonly string[] OutlineModes = new string[3]
  {
    "None",
    "Minimal",
    "Everything"
  };
  public static ButtonHandler.Button adjustMenuSizeButton;
  private static readonly (float scale, string desc)[] MenuSizeOptions = new (float, string)[5]
  {
    (0.45f, "Tiny"),
    (0.6f, "Small"),
    (0.75f, "Medium"),
    (0.9f, "Large"),
    (1.1f, "Massive")
  };
  private static int menuSizeIndex = 2;
  public static string MenuSizeDescription = "Medium";
  public static ButtonHandler.Button adjustRoundnessButton;
  private static readonly (float value, string desc)[] RoundnessOptions = new (float, string)[5]
  {
    (0.0f, "None"),
    (0.3f, "Low"),
    (0.5f, "Medium"),
    (0.75f, "High"),
    (1f, "Roundest")
  };
  private static int roundnessIndex = 4;
  public static float MenuRoundness = 1f;
  public static string RoundnessDescription = "Roundest";
  public static ButtonHandler.Button adjustBoardsAnimSpeedButton;
  private static int boardsAnimSpeedIndex = 2;
  public static float BoardsAnimSpeed = 1f;
  public static string BoardsAnimSpeedDescription = "Normal";
  public static Settings.ColorMode BoardsMode = Settings.ColorMode.Solid;
  public static ButtonHandler.Button boardsModeButton;
  public static ButtonHandler.Button adjustBoardsColorButton;
  public static ButtonHandler.Button adjustBoardsColor2Button;
  private static int boardsColorIndex = 72;
  private static int boardsColor2Index = 1;
  public static Color32 BoardsColor = new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static Color32 BoardsColor2 = new Color32((byte) 230, (byte) 230, (byte) 230, byte.MaxValue);
  public static string BoardsColorDescription = "Neon Cyan";
  public static string BoardsColor2Description = "Off-White";
  public static ButtonHandler.Button adjustAccentStripTypeButton;
  public static Settings.AccentStripType CurrentAccentStripType = Settings.AccentStripType.Both;
  private static readonly string[] AccentStripTypeNames = new string[3]
  {
    "Off",
    "Top",
    "Both"
  };
  private static int accentStripTypeIndex = 2;
  public static string AccentStripTypeDescription = "Both";
  public static ButtonHandler.Button adjustBackgroundColorButton;
  public static ButtonHandler.Button adjustBackgroundColor2Button;
  private static int backgroundColorIndex = 6;
  private static int backgroundColor2Index = 5;
  public static Color32 BackgroundColor = new Color32((byte) 15, (byte) 15, (byte) 15, byte.MaxValue);
  public static Color32 BackgroundColor2 = new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue);
  public static string BackgroundColorDescription = "Near Black";
  public static string BackgroundColor2Description = "Dark Gray";
  public static ButtonHandler.Button adjustButtonColorButton;
  public static ButtonHandler.Button adjustButtonColor2Button;
  private static int buttonColorIndex = 5;
  private static int buttonColor2Index = 4;
  public static Color32 ButtonColor = new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue);
  public static Color32 ButtonColor2 = new Color32((byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 80 /*0x50*/, byte.MaxValue);
  public static string ButtonColorDescription = "Dark Gray";
  public static string ButtonColor2Description = "Medium Gray";
  public static ButtonHandler.Button adjustEnabledButtonColorButton;
  public static ButtonHandler.Button adjustEnabledButtonColor2Button;
  private static int enabledButtonColorIndex = 4;
  private static int enabledButtonColor2Index = 3;
  public static Color32 EnabledButtonColor = new Color32((byte) 80 /*0x50*/, (byte) 80 /*0x50*/, (byte) 80 /*0x50*/, byte.MaxValue);
  public static Color32 EnabledButtonColor2 = new Color32((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/, byte.MaxValue);
  public static string EnabledButtonColorDescription = "Medium Gray";
  public static string EnabledButtonColor2Description = "Gray";
  public static ButtonHandler.Button titleColorButton;
  public static ButtonHandler.Button titleColor2Button;
  private static int titleColorIndex = 72;
  private static int titleColor2Index = 0;
  public static Color32 TitleColor = new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static Color32 TitleColor2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
  public static string TitleColorDescription = "Neon Cyan";
  public static string TitleColor2Description = "White";
  public static bool TitleAnimatedGradient = true;
  public static ButtonHandler.Button titleAnimatedGradientButton;
  public static ButtonHandler.Button tracerPositionButton;
  private static readonly string[] TracerPositions = new string[4]
  {
    "Right Hand",
    "Left Hand",
    "Head",
    "Button"
  };
  public static int tracerPositionIndex = 0;
  public static string TracerPosition = Settings.TracerPositions[0];
  public static ButtonHandler.Button adjustSpeedBoostButton;
  private static readonly (float speed, string desc)[] SpeedOptions = new (float, string)[6]
  {
    (10f, "Medium"),
    (14f, "Fast"),
    (18f, "Very Fast"),
    (7.5f, "Mosa"),
    (7f, "Comp"),
    (6f, "Slow")
  };
  private static int currentSpeedIndex = 0;
  public static float SpeedboostSpeed = 10f;
  public static float SpeedboostMultiplier = 2f;
  public static string SpeedDescription = "Medium";
  public static ButtonHandler.Button adjustFlySpeedButton;
  private static readonly (float speed, string desc)[] FlySpeedOptions = new (float, string)[5]
  {
    (12f, "Medium"),
    (16f, "Fast"),
    (20f, "Very Fast"),
    (4f, "Very Slow"),
    (8f, "Slow")
  };
  private static int flySpeedIndex = 0;
  public static float FlySpeed = 12f;
  public static string FlySpeedDescription = "Medium";
  public static ButtonHandler.Button adjustFOVButton;
  private static readonly (float value, string desc)[] FOVOptions = new (float, string)[5]
  {
    (100f, "100"),
    (110f, "110"),
    (120f, "120"),
    (80f, "80"),
    (90f, "90")
  };
  private static int FOVIndex = 0;
  public static float FOV = 100f;
  public static string FOVDescription = "100";
  public static ButtonHandler.Button adjustWalkWalkStrengthButton;
  private static readonly (float strength, string desc)[] WallWalkOptions = new (float, string)[3]
  {
    (-7.5f, "Medium"),
    (-12.5f, "Strong"),
    (-3f, "Weak")
  };
  private static int WallWalkStrengthIndex = 0;
  public static float WallWalkStrength = -7.5f;
  public static string WalkWalkStrengthDescription = "Medium";
  public static ButtonHandler.Button adjustTimeOfDayButton;
  private static int timeOfDayIndex = 0;
  private static readonly string[] TimeOfDayNames = new string[10]
  {
    "Dawn",
    "Morning",
    "Late Morning",
    "Midday",
    "Afternoon",
    "Late Afternoon",
    "Dusk",
    "Evening",
    "Night",
    "Late Night"
  };
  public static string TimeOfDayDescription = "Dawn";
  public static ButtonHandler.Button adjustArmLengthButton;
  private static readonly (Vector3 length, string desc)[] ArmLengthOptions = Settings.BuildArmLengths();
  private static int ArmLengthIndex = 3;
  public static Vector3 ArmLength = new Vector3(1.2f, 1.2f, 1.2f);
  public static string ArmLengthDescription = "1.2";
  public static ButtonHandler.Button cyclePlatformTypeButton;
  private static readonly string[] PlatformTypes = new string[4]
  {
    "Normal",
    "Sticky",
    "Invisible",
    "Invisible Sticky"
  };
  private static int PlatformTypeIndex = 0;
  public static string PlatformType = "Normal";
  public static int currentIndex = 0;
  public static Settings.ControllerInput currentControllerInput = new Settings.ControllerInput(InputHandler.None);
  public static string inputName = "None";
  public static Settings.ControllerInput[] controllerInputs = new Settings.ControllerInput[9]
  {
    new Settings.ControllerInput(InputHandler.None),
    new Settings.ControllerInput(InputHandler.RPrimary),
    new Settings.ControllerInput(InputHandler.LPrimary),
    new Settings.ControllerInput(InputHandler.RSecondary),
    new Settings.ControllerInput(InputHandler.LSecondary),
    new Settings.ControllerInput(InputHandler.RTrigger),
    new Settings.ControllerInput(InputHandler.LTrigger),
    new Settings.ControllerInput(InputHandler.RGrip),
    new Settings.ControllerInput(InputHandler.LGrip)
  };
  public static ButtonHandler.Button cycleControllerBindButton;
  public static ButtonHandler.Button adjustAntiReportRadiusButton;
  private static readonly (float radius, string desc)[] AntiReportRadiusOptions = new (float, string)[4]
  {
    (0.55f, "Default"),
    (0.65f, "Large"),
    (0.75f, "Very Large"),
    (0.45f, "Small")
  };
  private static int antiReportRadiusIndex = 0;
  public static float AntiReportRadius = 0.55f;
  public static string AntiReportRadiusDescription = "Default";
  public static ButtonHandler.Button adjustProjectileSpeedButton;
  public static ButtonHandler.Button adjustProjectileSizeButton;
  private static readonly (float speed, string desc)[] ProjectileSpeedOptions = new (float, string)[5]
  {
    (10f, "Slow"),
    (25f, "Medium"),
    (50f, "Fast"),
    (75f, "Very Fast"),
    (150f, "Extreme")
  };
  private static readonly (int scale, string desc)[] ProjectileSizeOptions = new (int, string)[3]
  {
    (0, "Regular"),
    (3, "Large"),
    (5, "Max")
  };
  private static int ProjectileSpeedIndex = 1;
  private static int ProjectileSizeIndex = 2;
  public static float ProjectileSpeed = 25f;
  public static int snowballScale = 5;
  public static string ProjectileSpeedDescription = "Medium";
  public static string ProjectileSizeDescription = "Max";
  public static ButtonHandler.Button CycleMenuFontButton;
  public static ButtonHandler.Button adjustLagTypeButton;
  private static readonly (int packets, float cooldown, string desc)[] LagTypeOptions = new (int, float, string)[4]
  {
    (450, 1f, "Regular"),
    (1350, 3f, "Heavy"),
    (3600, 8f, "Spike"),
    (225, 0.5f, "Light")
  };
  public static int lagTypeIndex = 0;
  public static int LagPackets = 450;
  public static float LagCooldown = 1f;
  public static string LagTypeDescription = "Regular";
  public static ButtonHandler.Button cycleNametagTypeButton;
  private static readonly string[] NametagTypes = new string[3]
  {
    "Full",
    "Medium",
    "Minimal"
  };
  public static int nametagTypeIndex = 0;
  public static string NametagType = "Full";
  public static ButtonHandler.Button ChangeSoundButton;
  public static int _currentSoundIndex;
  public static int _selectedMapIndex = 0;
  public static ButtonHandler.Button teleportMapButton;
  public static ButtonHandler.Button cycleGunAnimationButton;
  private static readonly string[] GunAnimations = new string[5]
  {
    "None",
    "Wiggly",
    "Spiral",
    "Pulse",
    "Electric"
  };
  private static int gunAnimationIndex = 0;
  public static string GunAnimationType = "None";
  public static ButtonHandler.Button gunIdleColorButton;
  public static ButtonHandler.Button gunFireColorButton;
  public static ButtonHandler.Button gunHoverColorButton;
  public static ButtonHandler.Button gunLockColorButton;
  private static int gunIdleColorIndex = 8;
  private static int gunFireColorIndex = 33;
  private static int gunHoverColorIndex = 50;
  private static int gunLockColorIndex = Settings.CustomColors.Length;
  public static Color32 GunIdleColor = new Color32(byte.MaxValue, (byte) 0, (byte) 0, byte.MaxValue);
  public static Color32 GunFireColor = new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue);
  public static Color32 GunHoverColor = new Color32((byte) 0, (byte) 0, byte.MaxValue, byte.MaxValue);
  public static Color32 GunLockColor = new Color32((byte) 0, (byte) 0, byte.MaxValue, byte.MaxValue);
  public static bool GunIdleRainbow;
  public static bool GunFireRainbow;
  public static bool GunHoverRainbow;
  public static bool GunLockRainbow = true;
  public static string GunIdleColorDescription = "Red";
  public static string GunFireColorDescription = "Green";
  public static string GunHoverColorDescription = "Blue";
  public static string GunLockColorDescription = "Rainbow";
  public static ButtonHandler.Button gunWiggleIntensityButton;
  public static ButtonHandler.Button gunWiggleSpeedButton;
  private static readonly (float v, string d)[] GunWiggleIntensityOptions = new (float, string)[4]
  {
    (0.02f, "Subtle"),
    (0.05f, "Normal"),
    (0.09f, "Intense"),
    (0.15f, "Extreme")
  };
  private static int gunWiggleIntensityIndex = 1;
  public static float GunWiggleIntensity = 0.05f;
  public static string GunWiggleIntensityDescription = "Normal";
  private static readonly (float v, string d)[] GunWiggleSpeedOptions = new (float, string)[4]
  {
    (4f, "Slow"),
    (7.5f, "Normal"),
    (12f, "Fast"),
    (20f, "Insane")
  };
  private static int gunWiggleSpeedIndex = 1;
  public static float GunWiggleSpeed = 7.5f;
  public static string GunWiggleSpeedDescription = "Normal";
  public static ButtonHandler.Button adjustOpacityButton;
  private static readonly (float value, string desc)[] OpacityOptions = new (float, string)[5]
  {
    (1f, "Solid"),
    (0.25f, "Glass"),
    (0.35f, "Low"),
    (0.45f, "Medium"),
    (0.55f, "High")
  };
  private static int opacityIndex = 0;
  public static float MenuOpacity = 1f;
  public static string OpacityDescription = "Solid";
  private static string[] _defaultSettings;

  public static void SetClassicButtons(bool setActive)
  {
    if (Settings.ClassicButtons == setActive)
      return;
    Settings.ClassicButtons = setActive;
    if (!((UnityEngine.Object) Variables.menuObj != (UnityEngine.Object) null))
      return;
    Main.RefreshMenu();
  }

  public static void SwitchHands(bool setActive) => Variables.rightHandedMenu = setActive;

  public static void ToggleNotifications(bool setActive)
  {
    Variables.toggleNotifications = setActive;
  }

  public static void GripSpeedBoost(bool setActive) => Movement.useGripForSpeedBoost = setActive;

  public static void TriggerPlatforms(bool setActive) => Movement.isTriggerPlatforms = setActive;

  public static void MenuOpenAndCloseSounds(bool setActive)
  {
    Variables.OpenAndCloseMenuSounds = setActive;
  }

  public static void IsTeamChecked(bool setActive) => Variables.teamCheckedESP = setActive;

  public static void RoomNotifications(bool setActive) => Main.RoomNotifications = setActive;

  public static void SoundRoomNotifications(bool setActive)
  {
    Variables.NotificationSounds = setActive;
  }

  public static void GunLine(bool setActive) => GunLib.GunLineEnabled = setActive;

  public static void AdjustGunPointerSize(bool forward)
  {
    Settings.gunPointerSizeIndex = forward ? (Settings.gunPointerSizeIndex + 1) % Settings.GunPointerSizeOptions.Length : (Settings.gunPointerSizeIndex - 1 + Settings.GunPointerSizeOptions.Length) % Settings.GunPointerSizeOptions.Length;
    (Settings.GunPointerSize, Settings.GunPointerSizeDescription) = Settings.GunPointerSizeOptions[Settings.gunPointerSizeIndex];
    Settings.gunPointerSizeButton?.SetText("Gun Pointer Size : " + Settings.GunPointerSizeDescription);
  }

  public static void AdjustGunLineThickness(bool forward)
  {
    Settings.gunLineThicknessIndex = forward ? (Settings.gunLineThicknessIndex + 1) % Settings.GunLineThicknessOptions.Length : (Settings.gunLineThicknessIndex - 1 + Settings.GunLineThicknessOptions.Length) % Settings.GunLineThicknessOptions.Length;
    (Settings.GunLineThickness, Settings.GunLineThicknessDescription) = Settings.GunLineThicknessOptions[Settings.gunLineThicknessIndex];
    Settings.gunLineThicknessButton?.SetText("Gun Line Thickness : " + Settings.GunLineThicknessDescription);
  }

  public static void ToggleGhostRig(bool setActive) => RigManager.ghostRigEnabled = setActive;

  public static Color GetAnimatedColor(
    Settings.ColorMode mode,
    Color c1,
    Color c2,
    float speed = 1f,
    int seed = 0)
  {
    float num = Time.time * speed;
    switch (mode)
    {
      case Settings.ColorMode.Solid:
        return c1;
      case Settings.ColorMode.Lerp:
        return Color.Lerp(c1, c2, (float) (((double) Mathf.Sin(num + (float) seed * 0.5f) + 1.0) * 0.5));
      case Settings.ColorMode.Rainbow:
        return Color.HSVToRGB((float) (((double) num * 0.10000000149011612 + (double) seed * 0.10000000149011612) % 1.0), 1f, 1f);
      case Settings.ColorMode.Strobe:
        return (double) Mathf.Sin(num * 4f + (float) seed) > 0.0 ? c1 : c2;
      case Settings.ColorMode.Pinwheel:
        return Color.Lerp(Main.pinwheelColor1, Main.pinwheelColor2, (float) (((double) Mathf.Sin(num + (float) seed * 0.5f) + 1.0) * 0.5));
      case Settings.ColorMode.Gradient:
        return c1;
      default:
        return c1;
    }
  }

  public static void CycleBackgroundMode(bool forward)
  {
    Settings.CycleMode(ref Settings.BackgroundMode, (Action<string>) (v => Settings.backgroundModeButton?.SetText("Background Mode : " + v)), forward);
  }

  public static void CycleButtonMode(bool forward)
  {
    Settings.CycleMode(ref Settings.ButtonMode, (Action<string>) (v => Settings.buttonModeButton?.SetText("Button Mode : " + v)), forward);
  }

  public static void CycleEnabledMode(bool forward)
  {
    Settings.CycleMode(ref Settings.EnabledButtonMode, (Action<string>) (v => Settings.enabledModeButton?.SetText("Enabled Mode : " + v)), forward);
  }

  public static void CycleTitleMode(bool forward)
  {
    int length = Settings.ColorModeNames.Length;
    int index = (int) Settings.TitleMode;
    do
    {
      index = forward ? (index + 1) % length : (index - 1 + length) % length;
    }
    while (index == 4);
    Settings.TitleMode = (Settings.ColorMode) index;
    Settings.titleModeButton?.SetText("Title Mode : " + Settings.ColorModeNames[index]);
  }

  public static void CycleOutlineColorMode(bool forward)
  {
    Settings.CycleMode(ref Settings.OutlineColorMode, (Action<string>) (v => Settings.outlineColorModeButton?.SetText("Outline Mode : " + v)), forward);
  }

  public static void CycleAccentStripMode(bool forward)
  {
    Settings.CycleMode(ref Settings.AccentStripMode, (Action<string>) (v => Settings.accentStripModeButton?.SetText("Accent Mode : " + v)), forward);
  }

  private static void CycleMode(ref Settings.ColorMode mode, Action<string> setText, bool forward = true)
  {
    int length = Settings.ColorModeNames.Length;
    int num = (int) mode;
    Settings.ColorMode colorMode = mode;
    int index = forward ? (num + 1) % length : (num - 1 + length) % length;
    mode = (Settings.ColorMode) index;
    setText(Settings.ColorModeNames[index]);
    if (colorMode != Settings.ColorMode.Gradient && mode != Settings.ColorMode.Gradient)
      return;
    Main.RefreshMenu();
  }

  public static void AdjustGradientDirection(bool forward)
  {
    Settings.gradientDirectionIndex = forward ? (Settings.gradientDirectionIndex + 1) % Settings.GradientDirectionNames.Length : (Settings.gradientDirectionIndex - 1 + Settings.GradientDirectionNames.Length) % Settings.GradientDirectionNames.Length;
    Settings.CurrentGradientDirection = (Settings.GradientDirection) Settings.gradientDirectionIndex;
    Settings.GradientDirectionDescription = Settings.GradientDirectionNames[Settings.gradientDirectionIndex];
    Settings.gradientDirectionButton?.SetText("Gradient Direction : " + Settings.GradientDirectionDescription);
    Main.RefreshMenu();
  }

  public static void OpenElementSettings(Settings.ColorElement element)
  {
    Settings._currentColorElement = element;
    ButtonHandler.ChangePage(Category.Element_Settings);
  }

  public static List<ButtonHandler.Button> GenerateElementButtons()
  {
    List<ButtonHandler.Button> list = new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Element_Settings, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Color_Settings)))
      {
        isCategory = true
      }
    };
    switch (Settings._currentColorElement)
    {
      case Settings.ColorElement.Pinwheel:
        Settings.pinwheelColor1Button = Settings.Incremental("Pinwheel Color 1 : " + Settings.PinwheelColor1Name, (Action) (() => Settings.AdjustPinwheelColor1(true)), (Action) (() => Settings.AdjustPinwheelColor1(false)));
        Settings.pinwheelColor2Button = Settings.Incremental("Pinwheel Color 2 : " + Settings.PinwheelColor2Name, (Action) (() => Settings.AdjustPinwheelColor2(true)), (Action) (() => Settings.AdjustPinwheelColor2(false)));
        Settings.pinwheelSpeedButton = Settings.Incremental("Pinwheel Speed : " + Settings.PinwheelSpeedDescription, (Action) (() => Settings.AdjustPinwheelSpeed(true)), (Action) (() => Settings.AdjustPinwheelSpeed(false)));
        list.Add(Settings.pinwheelColor1Button);
        list.Add(Settings.pinwheelColor2Button);
        list.Add(Settings.pinwheelSpeedButton);
        break;
      case Settings.ColorElement.Outline:
        Settings.outlineColorModeButton = Settings.Incremental("Outline Mode : " + Settings.ColorModeNames[(int) Settings.OutlineColorMode], (Action) (() => Settings.CycleOutlineColorMode(true)), (Action) (() => Settings.CycleOutlineColorMode(false)));
        Settings.adjustOutlineColorButton = Settings.Incremental("Outline Color : " + Settings.OutlineColorDescription, (Action) (() => Settings.AdjustOutlineColor(true)), (Action) (() => Settings.AdjustOutlineColor(false)));
        Settings.adjustOutlineColor2Button = Settings.Incremental("Outline Color 2 : " + Settings.OutlineColor2Description, (Action) (() => Settings.AdjustOutlineColor2(true)), (Action) (() => Settings.AdjustOutlineColor2(false)));
        list.Add(Settings.outlineColorModeButton);
        if (Settings.OutlineColorMode == Settings.ColorMode.Lerp || Settings.OutlineColorMode == Settings.ColorMode.Rainbow || Settings.OutlineColorMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustOutlineAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.OutlineAnimSpeedDescription, (Action) (() => Settings.AdjustOutlineAnimSpeed(true)), (Action) (() => Settings.AdjustOutlineAnimSpeed(false)));
          list.Add(Settings.adjustOutlineAnimSpeedButton);
        }
        list.Add(Settings.adjustOutlineColorButton);
        list.Add(Settings.adjustOutlineColor2Button);
        Settings.AddGradientDir(list, Settings.OutlineColorMode == Settings.ColorMode.Gradient);
        break;
      case Settings.ColorElement.Background:
        Settings.backgroundModeButton = Settings.Incremental("Background Mode : " + Settings.ColorModeNames[(int) Settings.BackgroundMode], (Action) (() => Settings.CycleBackgroundMode(true)), (Action) (() => Settings.CycleBackgroundMode(false)));
        Settings.adjustBackgroundColorButton = Settings.Incremental("Background Color : " + Settings.BackgroundColorDescription, (Action) (() => Settings.AdjustBackgroundColor(true)), (Action) (() => Settings.AdjustBackgroundColor(false)));
        Settings.adjustBackgroundColor2Button = Settings.Incremental("Background Color 2 : " + Settings.BackgroundColor2Description, (Action) (() => Settings.AdjustBackgroundColor2(true)), (Action) (() => Settings.AdjustBackgroundColor2(false)));
        list.Add(Settings.backgroundModeButton);
        if (Settings.BackgroundMode == Settings.ColorMode.Lerp || Settings.BackgroundMode == Settings.ColorMode.Rainbow || Settings.BackgroundMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustBackgroundAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.BackgroundAnimSpeedDescription, (Action) (() => Settings.AdjustBackgroundAnimSpeed(true)), (Action) (() => Settings.AdjustBackgroundAnimSpeed(false)));
          list.Add(Settings.adjustBackgroundAnimSpeedButton);
        }
        list.Add(Settings.adjustBackgroundColorButton);
        list.Add(Settings.adjustBackgroundColor2Button);
        Settings.AddGradientDir(list, Settings.BackgroundMode == Settings.ColorMode.Gradient);
        break;
      case Settings.ColorElement.Button:
        Settings.buttonModeButton = Settings.Incremental("Button Mode : " + Settings.ColorModeNames[(int) Settings.ButtonMode], (Action) (() => Settings.CycleButtonMode(true)), (Action) (() => Settings.CycleButtonMode(false)));
        Settings.adjustButtonColorButton = Settings.Incremental("Button Color : " + Settings.ButtonColorDescription, (Action) (() => Settings.AdjustButtonColor(true)), (Action) (() => Settings.AdjustButtonColor(false)));
        Settings.adjustButtonColor2Button = Settings.Incremental("Button Color 2 : " + Settings.ButtonColor2Description, (Action) (() => Settings.AdjustButtonColor2(true)), (Action) (() => Settings.AdjustButtonColor2(false)));
        Settings.enabledModeButton = Settings.Incremental("Enabled Mode : " + Settings.ColorModeNames[(int) Settings.EnabledButtonMode], (Action) (() => Settings.CycleEnabledMode(true)), (Action) (() => Settings.CycleEnabledMode(false)));
        Settings.adjustEnabledButtonColorButton = Settings.Incremental("Enabled Button Color : " + Settings.EnabledButtonColorDescription, (Action) (() => Settings.AdjustEnabledButtonColor(true)), (Action) (() => Settings.AdjustEnabledButtonColor(false)));
        Settings.adjustEnabledButtonColor2Button = Settings.Incremental("Enabled Button Color 2 : " + Settings.EnabledButtonColor2Description, (Action) (() => Settings.AdjustEnabledButtonColor2(true)), (Action) (() => Settings.AdjustEnabledButtonColor2(false)));
        list.Add(Settings.buttonModeButton);
        if (Settings.ButtonMode == Settings.ColorMode.Lerp || Settings.ButtonMode == Settings.ColorMode.Rainbow || Settings.ButtonMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustButtonAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.ButtonAnimSpeedDescription, (Action) (() => Settings.AdjustButtonAnimSpeed(true)), (Action) (() => Settings.AdjustButtonAnimSpeed(false)));
          list.Add(Settings.adjustButtonAnimSpeedButton);
        }
        list.Add(Settings.adjustButtonColorButton);
        list.Add(Settings.adjustButtonColor2Button);
        list.Add(Settings.enabledModeButton);
        if (Settings.EnabledButtonMode == Settings.ColorMode.Lerp || Settings.EnabledButtonMode == Settings.ColorMode.Rainbow || Settings.EnabledButtonMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustEnabledButtonAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.EnabledButtonAnimSpeedDescription, (Action) (() => Settings.AdjustEnabledButtonAnimSpeed(true)), (Action) (() => Settings.AdjustEnabledButtonAnimSpeed(false)));
          list.Add(Settings.adjustEnabledButtonAnimSpeedButton);
        }
        list.Add(Settings.adjustEnabledButtonColorButton);
        list.Add(Settings.adjustEnabledButtonColor2Button);
        Settings.AddGradientDir(list, Settings.ButtonMode == Settings.ColorMode.Gradient || Settings.EnabledButtonMode == Settings.ColorMode.Gradient);
        break;
      case Settings.ColorElement.Title:
        Settings.titleModeButton = Settings.Incremental("Title Mode : " + Settings.ColorModeNames[(int) Settings.TitleMode], (Action) (() => Settings.CycleTitleMode(true)), (Action) (() => Settings.CycleTitleMode(false)));
        Settings.titleColorButton = Settings.Incremental("Title Color : " + Settings.TitleColorDescription, (Action) (() => Settings.AdjustTitleColor(true)), (Action) (() => Settings.AdjustTitleColor(false)));
        Settings.titleColor2Button = Settings.Incremental("Title Color 2 : " + Settings.TitleColor2Description, (Action) (() => Settings.AdjustTitleColor2(true)), (Action) (() => Settings.AdjustTitleColor2(false)));
        list.Add(Settings.titleModeButton);
        if (Settings.TitleMode == Settings.ColorMode.Gradient)
        {
          Settings.titleAnimatedGradientButton = Settings.Incremental("Animated Gradient : " + (Settings.TitleAnimatedGradient ? "On" : "Off"), (Action) (() => Settings.ToggleTitleAnimatedGradient()), (Action) (() => Settings.ToggleTitleAnimatedGradient()));
          list.Add(Settings.titleAnimatedGradientButton);
        }
        if (Settings.TitleMode == Settings.ColorMode.Lerp || Settings.TitleMode == Settings.ColorMode.Rainbow || Settings.TitleMode == Settings.ColorMode.Strobe || Settings.TitleMode == Settings.ColorMode.Gradient && Settings.TitleAnimatedGradient)
        {
          Settings.adjustTitleAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.TitleAnimSpeedDescription, (Action) (() => Settings.AdjustTitleAnimSpeed(true)), (Action) (() => Settings.AdjustTitleAnimSpeed(false)));
          list.Add(Settings.adjustTitleAnimSpeedButton);
        }
        list.Add(Settings.titleColorButton);
        list.Add(Settings.titleColor2Button);
        break;
      case Settings.ColorElement.AccentStrip:
        Settings.accentStripModeButton = Settings.Incremental("Accent Mode : " + Settings.ColorModeNames[(int) Settings.AccentStripMode], (Action) (() => Settings.CycleAccentStripMode(true)), (Action) (() => Settings.CycleAccentStripMode(false)));
        Settings.adjustAccentStripColorButton = Settings.Incremental("Accent Color : " + Settings.AccentStripColorDescription, (Action) (() => Settings.AdjustAccentStripColor(true)), (Action) (() => Settings.AdjustAccentStripColor(false)));
        Settings.adjustAccentStripColor2Button = Settings.Incremental("Accent Color 2 : " + Settings.AccentStripColor2Description, (Action) (() => Settings.AdjustAccentStripColor2(true)), (Action) (() => Settings.AdjustAccentStripColor2(false)));
        list.Add(Settings.accentStripModeButton);
        if (Settings.AccentStripMode == Settings.ColorMode.Lerp || Settings.AccentStripMode == Settings.ColorMode.Rainbow || Settings.AccentStripMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustAccentStripAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.AccentStripAnimSpeedDescription, (Action) (() => Settings.AdjustAccentStripAnimSpeed(true)), (Action) (() => Settings.AdjustAccentStripAnimSpeed(false)));
          list.Add(Settings.adjustAccentStripAnimSpeedButton);
        }
        list.Add(Settings.adjustAccentStripColorButton);
        list.Add(Settings.adjustAccentStripColor2Button);
        Settings.AddGradientDir(list, Settings.AccentStripMode == Settings.ColorMode.Gradient);
        break;
      case Settings.ColorElement.Boards:
        Settings.boardsModeButton = Settings.Incremental("Boards Mode : " + Settings.ColorModeNames[(int) Settings.BoardsMode], (Action) (() => Settings.CycleBoardsMode(true)), (Action) (() => Settings.CycleBoardsMode(false)));
        Settings.adjustBoardsColorButton = Settings.Incremental("Boards Color : " + Settings.BoardsColorDescription, (Action) (() => Settings.AdjustBoardsColor(true)), (Action) (() => Settings.AdjustBoardsColor(false)));
        Settings.adjustBoardsColor2Button = Settings.Incremental("Boards Color 2 : " + Settings.BoardsColor2Description, (Action) (() => Settings.AdjustBoardsColor2(true)), (Action) (() => Settings.AdjustBoardsColor2(false)));
        list.Add(Settings.boardsModeButton);
        if (Settings.BoardsMode == Settings.ColorMode.Lerp || Settings.BoardsMode == Settings.ColorMode.Rainbow || Settings.BoardsMode == Settings.ColorMode.Strobe)
        {
          Settings.adjustBoardsAnimSpeedButton = Settings.Incremental("Animation Speed : " + Settings.BoardsAnimSpeedDescription, (Action) (() => Settings.AdjustBoardsAnimSpeed(true)), (Action) (() => Settings.AdjustBoardsAnimSpeed(false)));
          list.Add(Settings.adjustBoardsAnimSpeedButton);
        }
        list.Add(Settings.adjustBoardsColorButton);
        list.Add(Settings.adjustBoardsColor2Button);
        break;
    }
    return list;
  }

  private static ButtonHandler.Button Incremental(string text, Action up, Action down)
  {
    return new ButtonHandler.Button(text, Category.Element_Settings, false, false, (Action) null, incremental: true, incrementUp: up, incrementDown: down);
  }

  private static void AddGradientDir(List<ButtonHandler.Button> list, bool show)
  {
    if (!show)
      return;
    Settings.gradientDirectionButton = Settings.Incremental("Gradient Direction : " + Settings.GradientDirectionDescription, (Action) (() => Settings.AdjustGradientDirection(true)), (Action) (() => Settings.AdjustGradientDirection(false)));
    list.Add(Settings.gradientDirectionButton);
  }

  public static bool ShouldShowButton(string buttonText)
  {
    if (string.IsNullOrEmpty(buttonText))
      return true;
    if (buttonText.StartsWith("Background Color 2"))
      return Settings.BackgroundMode == Settings.ColorMode.Lerp || Settings.BackgroundMode == Settings.ColorMode.Strobe || Settings.BackgroundMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Button Color 2"))
      return Settings.ButtonMode == Settings.ColorMode.Lerp || Settings.ButtonMode == Settings.ColorMode.Strobe || Settings.ButtonMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Enabled Button Color 2"))
      return Settings.EnabledButtonMode == Settings.ColorMode.Lerp || Settings.EnabledButtonMode == Settings.ColorMode.Strobe || Settings.EnabledButtonMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Title Color 2"))
      return Settings.TitleMode == Settings.ColorMode.Lerp || Settings.TitleMode == Settings.ColorMode.Strobe || Settings.TitleMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Outline Color 2"))
      return Settings.OutlineColorMode == Settings.ColorMode.Lerp || Settings.OutlineColorMode == Settings.ColorMode.Strobe || Settings.OutlineColorMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Accent Color 2"))
      return Settings.AccentStripMode == Settings.ColorMode.Lerp || Settings.AccentStripMode == Settings.ColorMode.Strobe || Settings.AccentStripMode == Settings.ColorMode.Gradient;
    if (buttonText.StartsWith("Boards Color 2"))
      return Settings.BoardsMode == Settings.ColorMode.Lerp || Settings.BoardsMode == Settings.ColorMode.Strobe;
    if (buttonText.StartsWith("Outline Color "))
      return Settings.OutlineColorMode != Settings.ColorMode.Rainbow && Settings.OutlineColorMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Background Color "))
      return Settings.BackgroundMode != Settings.ColorMode.Rainbow && Settings.BackgroundMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Button Color "))
      return Settings.ButtonMode != Settings.ColorMode.Rainbow && Settings.ButtonMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Enabled Button Color "))
      return Settings.EnabledButtonMode != Settings.ColorMode.Rainbow && Settings.EnabledButtonMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Title Color "))
      return Settings.TitleMode != Settings.ColorMode.Rainbow && Settings.TitleMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Accent Color "))
      return Settings.AccentStripMode != Settings.ColorMode.Rainbow && Settings.AccentStripMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Boards Color "))
      return Settings.BoardsMode != Settings.ColorMode.Rainbow && Settings.BoardsMode != Settings.ColorMode.Pinwheel;
    if (buttonText.StartsWith("Pinwheel Speed"))
      return Settings.OutlineMode != 0;
    if (buttonText.StartsWith("Wiggle Intensity"))
      return Settings.GunAnimationType != "None" && Settings.GunAnimationType != "Pulse";
    return !buttonText.StartsWith("Wiggle Speed") || Settings.GunAnimationType != "None";
  }

  private static void CycleColor(
    ref int index,
    ref Color32 color,
    ref string desc,
    ButtonHandler.Button btn,
    string prefix,
    bool forward)
  {
    index = forward ? (index + 1) % Settings.CustomColors.Length : (index - 1 + Settings.CustomColors.Length) % Settings.CustomColors.Length;
    color = Settings.CustomColors[index];
    desc = Settings.CustomColorNames[index];
    btn?.SetText($"{prefix} : {desc}");
    Main.MarkColorsDirty();
  }

  public static void AdjustPinwheelColor1(bool forward)
  {
    Settings.CycleColor(ref Settings.pinwheelColor1Index, ref Settings.PinwheelColor1, ref Settings.PinwheelColor1Name, Settings.pinwheelColor1Button, "Pinwheel Color 1", forward);
    Main.pinwheelColor1 = (Color) Settings.PinwheelColor1;
  }

  public static void AdjustPinwheelColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.pinwheelColor2Index, ref Settings.PinwheelColor2, ref Settings.PinwheelColor2Name, Settings.pinwheelColor2Button, "Pinwheel Color 2", forward);
    Main.pinwheelColor2 = (Color) Settings.PinwheelColor2;
  }

  public static void AdjustPinwheelSpeed(bool forward)
  {
    Settings.pinwheelSpeedIndex = forward ? (Settings.pinwheelSpeedIndex + 1) % Settings.PinwheelSpeedOptions.Length : (Settings.pinwheelSpeedIndex - 1 + Settings.PinwheelSpeedOptions.Length) % Settings.PinwheelSpeedOptions.Length;
    (Main.pinwheelSpeed, Settings.PinwheelSpeedDescription) = Settings.PinwheelSpeedOptions[Settings.pinwheelSpeedIndex];
    Settings.pinwheelSpeedButton?.SetText("Pinwheel Speed : " + Settings.PinwheelSpeedDescription);
  }

  public static void AdjustAccentStripAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.accentStripAnimSpeedIndex, ref Settings.AccentStripAnimSpeed, ref Settings.AccentStripAnimSpeedDescription, Settings.adjustAccentStripAnimSpeedButton, forward);
  }

  public static void AdjustAccentStripColor(bool forward)
  {
    Settings.CycleColor(ref Settings.accentStripColorIndex, ref Settings.AccentStripColor, ref Settings.AccentStripColorDescription, Settings.adjustAccentStripColorButton, "Accent Color", forward);
  }

  public static void AdjustAccentStripColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.accentStripColor2Index, ref Settings.AccentStripColor2, ref Settings.AccentStripColor2Description, Settings.adjustAccentStripColor2Button, "Accent Color 2", forward);
  }

  public static void AdjustOutlineAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.outlineAnimSpeedIndex, ref Settings.OutlineAnimSpeed, ref Settings.OutlineAnimSpeedDescription, Settings.adjustOutlineAnimSpeedButton, forward);
  }

  public static void AdjustBackgroundAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.backgroundAnimSpeedIndex, ref Settings.BackgroundAnimSpeed, ref Settings.BackgroundAnimSpeedDescription, Settings.adjustBackgroundAnimSpeedButton, forward);
  }

  public static void AdjustButtonAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.buttonAnimSpeedIndex, ref Settings.ButtonAnimSpeed, ref Settings.ButtonAnimSpeedDescription, Settings.adjustButtonAnimSpeedButton, forward);
  }

  public static void AdjustEnabledButtonAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.enabledButtonAnimSpeedIndex, ref Settings.EnabledButtonAnimSpeed, ref Settings.EnabledButtonAnimSpeedDescription, Settings.adjustEnabledButtonAnimSpeedButton, forward);
  }

  public static void AdjustTitleAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.titleAnimSpeedIndex, ref Settings.TitleAnimSpeed, ref Settings.TitleAnimSpeedDescription, Settings.adjustTitleAnimSpeedButton, forward);
  }

  private static void CycleAnimSpeed(
    ref int index,
    ref float speed,
    ref string desc,
    ButtonHandler.Button btn,
    bool forward)
  {
    index = forward ? (index + 1) % Settings.AnimationSpeedOptions.Length : (index - 1 + Settings.AnimationSpeedOptions.Length) % Settings.AnimationSpeedOptions.Length;
    (speed, desc) = Settings.AnimationSpeedOptions[index];
    btn?.SetText("Animation Speed : " + desc);
  }

  public static void AdjustOutlineColor(bool forward)
  {
    Settings.CycleColor(ref Settings.outlineColorIndex, ref Settings.OutlineColor, ref Settings.OutlineColorDescription, Settings.adjustOutlineColorButton, "Outline Color", forward);
  }

  public static void AdjustOutlineColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.outlineColor2Index, ref Settings.OutlineColor2, ref Settings.OutlineColor2Description, Settings.adjustOutlineColor2Button, "Outline Color 2", forward);
  }

  public static void CycleOutlineMode(bool forward)
  {
    Settings.OutlineMode = forward ? (Settings.OutlineMode + 1) % Settings.OutlineModes.Length : (Settings.OutlineMode - 1 + Settings.OutlineModes.Length) % Settings.OutlineModes.Length;
    Settings.outlineModeButton?.SetText("Outline : " + Settings.OutlineModes[Settings.OutlineMode]);
    Main.RefreshMenu();
  }

  public static void AdjustMenuSize(bool forward)
  {
    Settings.menuSizeIndex = forward ? (Settings.menuSizeIndex + 1) % Settings.MenuSizeOptions.Length : (Settings.menuSizeIndex - 1 + Settings.MenuSizeOptions.Length) % Settings.MenuSizeOptions.Length;
    (Main.menuScale, Settings.MenuSizeDescription) = Settings.MenuSizeOptions[Settings.menuSizeIndex];
    Settings.adjustMenuSizeButton?.SetText("Menu Size : " + Settings.MenuSizeDescription);
    Main.RefreshMenu();
  }

  public static void AdjustRoundness(bool forward)
  {
    Settings.roundnessIndex = forward ? (Settings.roundnessIndex + 1) % Settings.RoundnessOptions.Length : (Settings.roundnessIndex - 1 + Settings.RoundnessOptions.Length) % Settings.RoundnessOptions.Length;
    (Settings.MenuRoundness, Settings.RoundnessDescription) = Settings.RoundnessOptions[Settings.roundnessIndex];
    Settings.adjustRoundnessButton?.SetText("Roundness : " + Settings.RoundnessDescription);
    Main.RefreshMenu();
  }

  public static void CycleBoardsMode(bool forward)
  {
    Settings.CycleMode(ref Settings.BoardsMode, (Action<string>) (v => Settings.boardsModeButton?.SetText("Boards Mode : " + v)), forward);
  }

  public static void AdjustBoardsAnimSpeed(bool forward)
  {
    Settings.CycleAnimSpeed(ref Settings.boardsAnimSpeedIndex, ref Settings.BoardsAnimSpeed, ref Settings.BoardsAnimSpeedDescription, Settings.adjustBoardsAnimSpeedButton, forward);
  }

  public static void AdjustBoardsColor(bool forward)
  {
    Settings.CycleColor(ref Settings.boardsColorIndex, ref Settings.BoardsColor, ref Settings.BoardsColorDescription, Settings.adjustBoardsColorButton, "Boards Color", forward);
  }

  public static void AdjustBoardsColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.boardsColor2Index, ref Settings.BoardsColor2, ref Settings.BoardsColor2Description, Settings.adjustBoardsColor2Button, "Boards Color 2", forward);
  }

  public static void AdjustAccentStripType(bool forward)
  {
    Settings.accentStripTypeIndex = forward ? (Settings.accentStripTypeIndex + 1) % Settings.AccentStripTypeNames.Length : (Settings.accentStripTypeIndex - 1 + Settings.AccentStripTypeNames.Length) % Settings.AccentStripTypeNames.Length;
    Settings.CurrentAccentStripType = (Settings.AccentStripType) Settings.accentStripTypeIndex;
    Settings.AccentStripTypeDescription = Settings.AccentStripTypeNames[Settings.accentStripTypeIndex];
    Settings.adjustAccentStripTypeButton?.SetText("Accent Strip : " + Settings.AccentStripTypeDescription);
    Main.RefreshMenu();
  }

  public static void AdjustBackgroundColor(bool forward)
  {
    Settings.CycleColor(ref Settings.backgroundColorIndex, ref Settings.BackgroundColor, ref Settings.BackgroundColorDescription, Settings.adjustBackgroundColorButton, "Background Color", forward);
  }

  public static void AdjustBackgroundColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.backgroundColor2Index, ref Settings.BackgroundColor2, ref Settings.BackgroundColor2Description, Settings.adjustBackgroundColor2Button, "Background Color 2", forward);
  }

  public static void AdjustButtonColor(bool forward)
  {
    Settings.CycleColor(ref Settings.buttonColorIndex, ref Settings.ButtonColor, ref Settings.ButtonColorDescription, Settings.adjustButtonColorButton, "Button Color", forward);
  }

  public static void AdjustButtonColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.buttonColor2Index, ref Settings.ButtonColor2, ref Settings.ButtonColor2Description, Settings.adjustButtonColor2Button, "Button Color 2", forward);
  }

  public static void AdjustEnabledButtonColor(bool forward)
  {
    Settings.CycleColor(ref Settings.enabledButtonColorIndex, ref Settings.EnabledButtonColor, ref Settings.EnabledButtonColorDescription, Settings.adjustEnabledButtonColorButton, "Enabled Button Color", forward);
  }

  public static void AdjustEnabledButtonColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.enabledButtonColor2Index, ref Settings.EnabledButtonColor2, ref Settings.EnabledButtonColor2Description, Settings.adjustEnabledButtonColor2Button, "Enabled Button Color 2", forward);
  }

  public static void AdjustTitleColor(bool forward)
  {
    Settings.CycleColor(ref Settings.titleColorIndex, ref Settings.TitleColor, ref Settings.TitleColorDescription, Settings.titleColorButton, "Title Color", forward);
  }

  public static void AdjustTitleColor2(bool forward)
  {
    Settings.CycleColor(ref Settings.titleColor2Index, ref Settings.TitleColor2, ref Settings.TitleColor2Description, Settings.titleColor2Button, "Title Color 2", forward);
  }

  public static void ToggleTitleAnimatedGradient()
  {
    Settings.TitleAnimatedGradient = !Settings.TitleAnimatedGradient;
    Settings.titleAnimatedGradientButton?.SetText("Animated Gradient : " + (Settings.TitleAnimatedGradient ? "On" : "Off"));
  }

  public static void AdjustTracerPosition(bool forward)
  {
    Settings.tracerPositionIndex = forward ? (Settings.tracerPositionIndex + 1) % Settings.TracerPositions.Length : (Settings.tracerPositionIndex - 1 + Settings.TracerPositions.Length) % Settings.TracerPositions.Length;
    Settings.TracerPosition = Settings.TracerPositions[Settings.tracerPositionIndex];
    Settings.tracerPositionButton?.SetText("Tracer Position : " + Settings.TracerPosition);
  }

  public static void AdjustSpeedBoost(bool forward)
  {
    Settings.currentSpeedIndex = forward ? (Settings.currentSpeedIndex + 1) % Settings.SpeedOptions.Length : (Settings.currentSpeedIndex - 1 + Settings.SpeedOptions.Length) % Settings.SpeedOptions.Length;
    (Settings.SpeedboostSpeed, Settings.SpeedDescription) = Settings.SpeedOptions[Settings.currentSpeedIndex];
    Settings.SpeedboostMultiplier = Settings.SpeedboostSpeed / 5f;
    Settings.adjustSpeedBoostButton?.SetText("Boost Speed : " + Settings.SpeedDescription);
  }

  public static void AdjustFlySpeed(bool forward)
  {
    Settings.flySpeedIndex = forward ? (Settings.flySpeedIndex + 1) % Settings.FlySpeedOptions.Length : (Settings.flySpeedIndex - 1 + Settings.FlySpeedOptions.Length) % Settings.FlySpeedOptions.Length;
    (Settings.FlySpeed, Settings.FlySpeedDescription) = Settings.FlySpeedOptions[Settings.flySpeedIndex];
    Settings.adjustFlySpeedButton?.SetText("Fly Speed : " + Settings.FlySpeedDescription);
  }

  public static void AdjustFOV(bool forward)
  {
    Settings.FOVIndex = forward ? (Settings.FOVIndex + 1) % Settings.FOVOptions.Length : (Settings.FOVIndex - 1 + Settings.FOVOptions.Length) % Settings.FOVOptions.Length;
    (Settings.FOV, Settings.FOVDescription) = Settings.FOVOptions[Settings.FOVIndex];
    Settings.adjustFOVButton?.SetText("FPC FOV : " + Settings.FOVDescription);
  }

  public static void AdjustWallWalkStrength(bool forward)
  {
    Settings.WallWalkStrengthIndex = forward ? (Settings.WallWalkStrengthIndex + 1) % Settings.WallWalkOptions.Length : (Settings.WallWalkStrengthIndex - 1 + Settings.WallWalkOptions.Length) % Settings.WallWalkOptions.Length;
    (Settings.WallWalkStrength, Settings.WalkWalkStrengthDescription) = Settings.WallWalkOptions[Settings.WallWalkStrengthIndex];
    Settings.adjustWalkWalkStrengthButton?.SetText("Wall Walk Strength : " + Settings.WalkWalkStrengthDescription);
  }

  public static void AdjustTimeOfDay(bool forward)
  {
    Settings.timeOfDayIndex = forward ? (Settings.timeOfDayIndex + 1) % Settings.TimeOfDayNames.Length : (Settings.timeOfDayIndex - 1 + Settings.TimeOfDayNames.Length) % Settings.TimeOfDayNames.Length;
    Settings.TimeOfDayDescription = Settings.TimeOfDayNames[Settings.timeOfDayIndex];
    BetterDayNightManager.instance.SetTimeOfDay(Settings.timeOfDayIndex, false);
    Settings.adjustTimeOfDayButton?.SetText("Time Of Day : " + Settings.TimeOfDayDescription);
  }

  private static (Vector3 length, string desc)[] BuildArmLengths()
  {
    (Vector3, string)[] valueTupleArray = new (Vector3, string)[20];
    for (int index = 0; index < 20; ++index)
    {
      float num = Mathf.Round((float) ((double) (21 + index) * 0.05000000074505806 * 1000.0)) / 1000f;
      valueTupleArray[index] = (new Vector3(num, num, num), num.ToString("0.0##", (IFormatProvider) CultureInfo.InvariantCulture));
    }
    return valueTupleArray;
  }

  public static void AdjustArmLength(bool forward)
  {
    Settings.ArmLengthIndex = forward ? (Settings.ArmLengthIndex + 1) % Settings.ArmLengthOptions.Length : (Settings.ArmLengthIndex - 1 + Settings.ArmLengthOptions.Length) % Settings.ArmLengthOptions.Length;
    (Settings.ArmLength, Settings.ArmLengthDescription) = Settings.ArmLengthOptions[Settings.ArmLengthIndex];
    Settings.adjustArmLengthButton?.SetText("Long Arms Length : " + Settings.ArmLengthDescription);
  }

  public static void CyclePlatformType(bool forward)
  {
    Settings.PlatformTypeIndex = forward ? (Settings.PlatformTypeIndex + 1) % Settings.PlatformTypes.Length : (Settings.PlatformTypeIndex - 1 + Settings.PlatformTypes.Length) % Settings.PlatformTypes.Length;
    Settings.PlatformType = Settings.PlatformTypes[Settings.PlatformTypeIndex];
    Settings.cyclePlatformTypeButton?.SetText("Platform Type : " + Settings.PlatformType);
  }

  public static void CycleControllerBind(bool forward = true)
  {
    Settings.currentIndex = Array.IndexOf<Settings.ControllerInput>(Settings.controllerInputs, Settings.currentControllerInput);
    if (Settings.currentIndex == -1)
    {
      Debug.LogError((object) "Error: Current input is not found in the array!");
    }
    else
    {
      Settings.currentIndex = forward ? (Settings.currentIndex + 1) % Settings.controllerInputs.Length : (Settings.currentIndex - 1 + Settings.controllerInputs.Length) % Settings.controllerInputs.Length;
      Settings.currentControllerInput = Settings.controllerInputs[Settings.currentIndex];
      Settings.inputName = Settings.currentControllerInput.Method.Name;
      Settings.cycleControllerBindButton?.SetText("Sound Input : " + Settings.inputName);
    }
  }

  public static void AdjustAntiReportRadius(bool forward)
  {
    Settings.antiReportRadiusIndex = forward ? (Settings.antiReportRadiusIndex + 1) % Settings.AntiReportRadiusOptions.Length : (Settings.antiReportRadiusIndex - 1 + Settings.AntiReportRadiusOptions.Length) % Settings.AntiReportRadiusOptions.Length;
    (Settings.AntiReportRadius, Settings.AntiReportRadiusDescription) = Settings.AntiReportRadiusOptions[Settings.antiReportRadiusIndex];
    Settings.adjustAntiReportRadiusButton?.SetText("Anti Report Radius : " + Settings.AntiReportRadiusDescription);
  }

  public static void AdjustProjectileSpeed(bool forward)
  {
    Settings.ProjectileSpeedIndex = forward ? (Settings.ProjectileSpeedIndex + 1) % Settings.ProjectileSpeedOptions.Length : (Settings.ProjectileSpeedIndex - 1 + Settings.ProjectileSpeedOptions.Length) % Settings.ProjectileSpeedOptions.Length;
    (Settings.ProjectileSpeed, Settings.ProjectileSpeedDescription) = Settings.ProjectileSpeedOptions[Settings.ProjectileSpeedIndex];
    Settings.adjustProjectileSpeedButton?.SetText("Projectile Speed : " + Settings.ProjectileSpeedDescription);
  }

  public static void AdjustProjectileSize(bool forward)
  {
    Settings.ProjectileSizeIndex = forward ? (Settings.ProjectileSizeIndex + 1) % Settings.ProjectileSizeOptions.Length : (Settings.ProjectileSizeIndex - 1 + Settings.ProjectileSizeOptions.Length) % Settings.ProjectileSizeOptions.Length;
    (Settings.snowballScale, Settings.ProjectileSizeDescription) = Settings.ProjectileSizeOptions[Settings.ProjectileSizeIndex];
    Settings.adjustProjectileSizeButton?.SetText("Projectile Size : " + Settings.ProjectileSizeDescription);
  }

  public static int CurrentFontIndex { get; private set; }

  public static string CurrentFontDescription
  {
    get => Main._fonts.Count <= 0 ? "Arial" : Main._fonts[Settings.CurrentFontIndex].Description;
  }

  public static void CycleFont(bool forward)
  {
    if (Main._fonts.Count == 0)
      return;
    Settings.CurrentFontIndex = forward ? (Settings.CurrentFontIndex + 1) % Main._fonts.Count : (Settings.CurrentFontIndex - 1 + Main._fonts.Count) % Main._fonts.Count;
    Settings.CycleMenuFontButton?.SetText("Menu Font : " + Settings.CurrentFontDescription);
    Main.RefreshMenu();
  }

  public static void AdjustLagType(bool forward)
  {
    Settings.lagTypeIndex = forward ? (Settings.lagTypeIndex + 1) % Settings.LagTypeOptions.Length : (Settings.lagTypeIndex - 1 + Settings.LagTypeOptions.Length) % Settings.LagTypeOptions.Length;
    (Settings.LagPackets, Settings.LagCooldown, Settings.LagTypeDescription) = Settings.LagTypeOptions[Settings.lagTypeIndex];
    Settings.adjustLagTypeButton?.SetText("Lag Type : " + Settings.LagTypeDescription);
  }

  public static void CycleNametagType(bool forward)
  {
    Settings.nametagTypeIndex = forward ? (Settings.nametagTypeIndex + 1) % Settings.NametagTypes.Length : (Settings.nametagTypeIndex - 1 + Settings.NametagTypes.Length) % Settings.NametagTypes.Length;
    Settings.NametagType = Settings.NametagTypes[Settings.nametagTypeIndex];
    Settings.cycleNametagTypeButton?.SetText("Nametag Type : " + Settings.NametagType);
  }

  public static string ClickSoundDescription
  {
    get => ButtonHandler.SoundSequence[Settings._currentSoundIndex].Description;
  }

  public static void ChangeClickSound(bool forward)
  {
    Settings._currentSoundIndex = forward ? (Settings._currentSoundIndex + 1) % ButtonHandler.SoundSequence.Count : (Settings._currentSoundIndex - 1 + ButtonHandler.SoundSequence.Count) % ButtonHandler.SoundSequence.Count;
    Settings.ChangeSoundButton?.SetText("Click Sound : " + Settings.ClickSoundDescription);
  }

  public static string MapDescription => Movement._maps[Settings._selectedMapIndex].name;

  public static void CycleMap(bool forward)
  {
    Settings._selectedMapIndex = forward ? (Settings._selectedMapIndex + 1) % Movement._maps.Length : (Settings._selectedMapIndex - 1 + Movement._maps.Length) % Movement._maps.Length;
    Settings.teleportMapButton?.SetText("TP To : " + Movement._maps[Settings._selectedMapIndex].name);
  }

  public static void CycleGunAnimation(bool forward)
  {
    Settings.gunAnimationIndex = forward ? (Settings.gunAnimationIndex + 1) % Settings.GunAnimations.Length : (Settings.gunAnimationIndex - 1 + Settings.GunAnimations.Length) % Settings.GunAnimations.Length;
    Settings.GunAnimationType = Settings.GunAnimations[Settings.gunAnimationIndex];
    Settings.cycleGunAnimationButton?.SetText("Gun Animation : " + Settings.GunAnimationType);
  }

  public static void AdjustGunIdleColor(bool forward)
  {
    Settings.CycleGunColor(ref Settings.gunIdleColorIndex, ref Settings.GunIdleColor, ref Settings.GunIdleColorDescription, ref Settings.GunIdleRainbow, Settings.gunIdleColorButton, "Gun Idle Color", forward);
  }

  public static void AdjustGunFireColor(bool forward)
  {
    Settings.CycleGunColor(ref Settings.gunFireColorIndex, ref Settings.GunFireColor, ref Settings.GunFireColorDescription, ref Settings.GunFireRainbow, Settings.gunFireColorButton, "Gun Fire Color", forward);
  }

  public static void AdjustGunHoverColor(bool forward)
  {
    Settings.CycleGunColor(ref Settings.gunHoverColorIndex, ref Settings.GunHoverColor, ref Settings.GunHoverColorDescription, ref Settings.GunHoverRainbow, Settings.gunHoverColorButton, "Gun Hover Color", forward);
  }

  public static void AdjustGunLockColor(bool forward)
  {
    Settings.CycleGunColor(ref Settings.gunLockColorIndex, ref Settings.GunLockColor, ref Settings.GunLockColorDescription, ref Settings.GunLockRainbow, Settings.gunLockColorButton, "Gun Lock Color", forward);
  }

  private static void CycleGunColor(
    ref int index,
    ref Color32 color,
    ref string desc,
    ref bool rainbow,
    ButtonHandler.Button btn,
    string prefix,
    bool forward)
  {
    int num = Settings.CustomColors.Length + 1;
    index = forward ? (index + 1) % num : (index - 1 + num) % num;
    rainbow = index == Settings.CustomColors.Length;
    if (rainbow)
    {
      desc = "Rainbow";
    }
    else
    {
      color = Settings.CustomColors[index];
      desc = Settings.CustomColorNames[index];
    }
    btn?.SetText($"{prefix} : {desc}");
  }

  public static void AdjustGunWiggleIntensity(bool forward)
  {
    Settings.gunWiggleIntensityIndex = forward ? (Settings.gunWiggleIntensityIndex + 1) % Settings.GunWiggleIntensityOptions.Length : (Settings.gunWiggleIntensityIndex - 1 + Settings.GunWiggleIntensityOptions.Length) % Settings.GunWiggleIntensityOptions.Length;
    (Settings.GunWiggleIntensity, Settings.GunWiggleIntensityDescription) = Settings.GunWiggleIntensityOptions[Settings.gunWiggleIntensityIndex];
    Settings.gunWiggleIntensityButton?.SetText("Wiggle Intensity : " + Settings.GunWiggleIntensityDescription);
  }

  public static void AdjustGunWiggleSpeed(bool forward)
  {
    Settings.gunWiggleSpeedIndex = forward ? (Settings.gunWiggleSpeedIndex + 1) % Settings.GunWiggleSpeedOptions.Length : (Settings.gunWiggleSpeedIndex - 1 + Settings.GunWiggleSpeedOptions.Length) % Settings.GunWiggleSpeedOptions.Length;
    (Settings.GunWiggleSpeed, Settings.GunWiggleSpeedDescription) = Settings.GunWiggleSpeedOptions[Settings.gunWiggleSpeedIndex];
    Settings.gunWiggleSpeedButton?.SetText("Wiggle Speed : " + Settings.GunWiggleSpeedDescription);
  }

  public static void AdjustOpacity(bool forward)
  {
    Settings.opacityIndex = forward ? (Settings.opacityIndex + 1) % Settings.OpacityOptions.Length : (Settings.opacityIndex - 1 + Settings.OpacityOptions.Length) % Settings.OpacityOptions.Length;
    (Settings.MenuOpacity, Settings.OpacityDescription) = Settings.OpacityOptions[Settings.opacityIndex];
    Settings.adjustOpacityButton?.SetText("Opacity : " + Settings.OpacityDescription);
    Main.RefreshMenu();
  }

  private static int ColorIndexByName(string name)
  {
    int num = Array.IndexOf<string>(Settings.CustomColorNames, name);
    return num >= 0 ? num : -1;
  }

  private static void LoadGunColor(
    string value,
    ref int index,
    ref Color32 color,
    ref string desc,
    ref bool rainbow,
    ButtonHandler.Button btn,
    string prefix)
  {
    if (value == "Rainbow")
    {
      index = Settings.CustomColors.Length;
      rainbow = true;
      desc = "Rainbow";
    }
    else
    {
      int index1 = Settings.ColorIndexByName(value);
      if (index1 < 0)
        return;
      index = index1;
      color = Settings.CustomColors[index1];
      rainbow = false;
      desc = Settings.CustomColorNames[index1];
    }
    btn?.SetText($"{prefix} : {desc}");
  }

  private static int ModeIndexByName(string name)
  {
    int num = Array.IndexOf<string>(Settings.ColorModeNames, name);
    return num >= 0 ? num : -1;
  }

  public static string SettingsFilePath => Path.Combine(Variables.folderName, "Settings.txt");

  public static void SaveSettings() => Settings.SaveSettings(Settings.SettingsFilePath);

  public static void SaveSettings(string path)
  {
    try
    {
      File.WriteAllLines(path, Settings.BuildSettingsLines());
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Error saving settings: " + ex.Message));
    }
  }

  private static string[] BuildSettingsLines()
  {
    return new string[67]
    {
      "Pinwheel Speed : " + Settings.PinwheelSpeedDescription,
      "Tracer Position : " + Settings.TracerPosition,
      "Boost Speed : " + Settings.SpeedDescription,
      "Fly Speed : " + Settings.FlySpeedDescription,
      "FPC FOV : " + Settings.FOVDescription,
      "Wall Walk Strength : " + Settings.WalkWalkStrengthDescription,
      "Long Arms Length : " + Settings.ArmLengthDescription,
      "Platform Type : " + Settings.PlatformType,
      "Sound Input : " + Settings.inputName,
      "Anti Report Radius : " + Settings.AntiReportRadiusDescription,
      "Projectile Speed : " + Settings.ProjectileSpeedDescription,
      "Projectile Size : " + Settings.ProjectileSizeDescription,
      "Menu Font : " + Settings.CurrentFontDescription,
      "Click Sound : " + Settings.ClickSoundDescription,
      "Lag Type : " + Settings.LagTypeDescription,
      "Nametag Type : " + Settings.NametagType,
      "Gun Animation : " + Settings.GunAnimationType,
      "Gun Idle Color : " + Settings.GunIdleColorDescription,
      "Gun Fire Color : " + Settings.GunFireColorDescription,
      "Gun Hover Color : " + Settings.GunHoverColorDescription,
      "Gun Lock Color : " + Settings.GunLockColorDescription,
      "Gun Pointer Size : " + Settings.GunPointerSizeDescription,
      "Gun Line Thickness : " + Settings.GunLineThicknessDescription,
      "Gun Wiggle Intensity : " + Settings.GunWiggleIntensityDescription,
      "Gun Wiggle Speed : " + Settings.GunWiggleSpeedDescription,
      "Nextbot Speed : " + Nextbots.NextbotSpeedDescription,
      $"Nextbot Behaviour : {Nextbots.CurrentBehaviour}",
      "Current Projectile : " + Projectile.CurrentProjectileName,
      "Background Color : " + Settings.BackgroundColorDescription,
      "Background Color 2 : " + Settings.BackgroundColor2Description,
      "Pinwheel Color 1 : " + Settings.PinwheelColor1Name,
      "Pinwheel Color 2 : " + Settings.PinwheelColor2Name,
      "Button Color : " + Settings.ButtonColorDescription,
      "Button Color 2 : " + Settings.ButtonColor2Description,
      "Enabled Button Color : " + Settings.EnabledButtonColorDescription,
      "Enabled Button Color 2 : " + Settings.EnabledButtonColor2Description,
      "Outline : " + Settings.OutlineModes[Settings.OutlineMode],
      "Title Color : " + Settings.TitleColorDescription,
      "Title Color 2 : " + Settings.TitleColor2Description,
      "Title Animated Gradient : " + (Settings.TitleAnimatedGradient ? "On" : "Off"),
      "Background Mode : " + Settings.ColorModeNames[(int) Settings.BackgroundMode],
      "Button Mode : " + Settings.ColorModeNames[(int) Settings.ButtonMode],
      "Enabled Mode : " + Settings.ColorModeNames[(int) Settings.EnabledButtonMode],
      "Title Mode : " + Settings.ColorModeNames[(int) Settings.TitleMode],
      "Outline Color : " + Settings.OutlineColorDescription,
      "Outline Color 2 : " + Settings.OutlineColor2Description,
      "Outline Mode : " + Settings.ColorModeNames[(int) Settings.OutlineColorMode],
      "Outline Anim Speed : " + Settings.OutlineAnimSpeedDescription,
      "Background Anim Speed : " + Settings.BackgroundAnimSpeedDescription,
      "Button Anim Speed : " + Settings.ButtonAnimSpeedDescription,
      "Enabled Button Anim Speed : " + Settings.EnabledButtonAnimSpeedDescription,
      "Title Anim Speed : " + Settings.TitleAnimSpeedDescription,
      "Accent Color : " + Settings.AccentStripColorDescription,
      "Accent Color 2 : " + Settings.AccentStripColor2Description,
      "Accent Mode : " + Settings.ColorModeNames[(int) Settings.AccentStripMode],
      "Accent Anim Speed : " + Settings.AccentStripAnimSpeedDescription,
      "Menu Size : " + Settings.MenuSizeDescription,
      "Roundness : " + Settings.RoundnessDescription,
      "Accent Strip Type : " + Settings.AccentStripTypeDescription,
      "Time Of Day : " + Settings.TimeOfDayDescription,
      "Opacity : " + Settings.OpacityDescription,
      "Boards Color : " + Settings.BoardsColorDescription,
      "Boards Color 2 : " + Settings.BoardsColor2Description,
      "Boards Mode : " + Settings.ColorModeNames[(int) Settings.BoardsMode],
      "Boards Anim Speed : " + Settings.BoardsAnimSpeedDescription,
      "Gradient Direction : " + Settings.GradientDirectionDescription,
      "TP Map : " + Settings.MapDescription
    };
  }

  public static void CaptureDefaults()
  {
    if (Settings._defaultSettings != null)
      return;
    Settings._defaultSettings = Settings.BuildSettingsLines();
  }

  public static void ResetToDefaultSettings()
  {
    if (Settings._defaultSettings == null)
      return;
    foreach (string defaultSetting in Settings._defaultSettings)
    {
      string[] source = defaultSetting.Split(':');
      if (source.Length >= 2)
        Settings.ApplyKeyValue(source[0].Trim(), string.Join(":", ((IEnumerable<string>) source).Skip<string>(1)).Trim());
    }
    Settings.SaveSettings();
    Main.RefreshMenu();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, "Default Settings");
  }

  public static void LoadSettings() => Settings.LoadSettings(Settings.SettingsFilePath);

  public static void LoadSettings(string path)
  {
    if (!File.Exists(path))
      return;
    try
    {
      foreach (string readAllLine in File.ReadAllLines(path))
      {
        string[] source = readAllLine.Split(':');
        if (source.Length >= 2)
          Settings.ApplyKeyValue(source[0].Trim(), string.Join(":", ((IEnumerable<string>) source).Skip<string>(1)).Trim());
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("Error loading settings: " + ex.Message));
    }
  }

  private static void ApplyKeyValue(string key, string value)
  {
    string s = key;
    switch (s)
    {
      case "Nextbot Behaviour":
        Nextbots.ApplyNextbotBehaviour(value);
        break;
      case "Sound Input":
        for (int index = 0; index < Settings.controllerInputs.Length; ++index)
        {
          if (Settings.controllerInputs[index].Method.Name == value)
          {
            Settings.currentIndex = index;
            Settings.currentControllerInput = Settings.controllerInputs[index];
            Settings.inputName = value;
            Settings.cycleControllerBindButton?.SetText("Sound Input : " + Settings.inputName);
            break;
          }
        }
        break;
      case "Outline":
        Settings.OutlineMode = Array.IndexOf<string>(Settings.OutlineModes, value);
        if (Settings.OutlineMode < 0)
          Settings.OutlineMode = 1;
        Settings.outlineModeButton?.SetText("Outline : " + Settings.OutlineModes[Settings.OutlineMode]);
        break;
      case "Boards Color":
        int index1 = Settings.ColorIndexByName(value);
        if (index1 < 0)
          break;
        Settings.boardsColorIndex = index1;
        Settings.BoardsColor = Settings.CustomColors[index1];
        Settings.BoardsColorDescription = Settings.CustomColorNames[index1];
        Settings.adjustBoardsColorButton?.SetText("Boards Color : " + Settings.BoardsColorDescription);
        break;
      case "Enabled Mode":
        int index2 = Settings.ModeIndexByName(value);
        if (index2 < 0)
          break;
        Settings.EnabledButtonMode = (Settings.ColorMode) index2;
        Settings.enabledModeButton?.SetText("Enabled Mode : " + Settings.ColorModeNames[index2]);
        break;
      case "Menu Font":
        for (int index3 = 0; index3 < Main._fonts.Count; ++index3)
        {
          if (Main._fonts[index3].Description == value)
          {
            Settings.CurrentFontIndex = index3;
            Settings.CycleMenuFontButton?.SetText("Menu Font : " + Settings.CurrentFontDescription);
            break;
          }
        }
        break;
      case "Lag Type":
        for (int index4 = 0; index4 < Settings.LagTypeOptions.Length; ++index4)
        {
          if (Settings.LagTypeOptions[index4].desc == value)
          {
            Settings.lagTypeIndex = index4;
            (Settings.LagPackets, Settings.LagCooldown, Settings.LagTypeDescription) = Settings.LagTypeOptions[index4];
            Settings.adjustLagTypeButton?.SetText("Lag Type : " + Settings.LagTypeDescription);
            break;
          }
        }
        break;
      case "Gun Lock Color":
        Settings.LoadGunColor(value, ref Settings.gunLockColorIndex, ref Settings.GunLockColor, ref Settings.GunLockColorDescription, ref Settings.GunLockRainbow, Settings.gunLockColorButton, "Gun Lock Color");
        break;
      case "FPC FOV":
        for (int index5 = 0; index5 < Settings.FOVOptions.Length; ++index5)
        {
          if (Settings.FOVOptions[index5].desc == value)
          {
            Settings.FOVIndex = index5;
            (Settings.FOV, Settings.FOVDescription) = Settings.FOVOptions[index5];
            Settings.adjustFOVButton?.SetText("FPC FOV : " + Settings.FOVDescription);
            break;
          }
        }
        break;
      case "Boards Color 2":
        int index6 = Settings.ColorIndexByName(value);
        if (index6 < 0)
          break;
        Settings.boardsColor2Index = index6;
        Settings.BoardsColor2 = Settings.CustomColors[index6];
        Settings.BoardsColor2Description = Settings.CustomColorNames[index6];
        Settings.adjustBoardsColor2Button?.SetText("Boards Color 2 : " + Settings.BoardsColor2Description);
        break;
      case "Enabled Button Anim Speed":
        for (int index7 = 0; index7 < Settings.AnimationSpeedOptions.Length; ++index7)
        {
          if (Settings.AnimationSpeedOptions[index7].desc == value)
          {
            Settings.enabledButtonAnimSpeedIndex = index7;
            (Settings.EnabledButtonAnimSpeed, Settings.EnabledButtonAnimSpeedDescription) = Settings.AnimationSpeedOptions[index7];
            Settings.adjustEnabledButtonAnimSpeedButton?.SetText("Animation Speed : " + Settings.EnabledButtonAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Gradient Direction":
        Settings.gradientDirectionIndex = Array.IndexOf<string>(Settings.GradientDirectionNames, value);
        if (Settings.gradientDirectionIndex < 0)
          Settings.gradientDirectionIndex = 0;
        Settings.CurrentGradientDirection = (Settings.GradientDirection) Settings.gradientDirectionIndex;
        Settings.GradientDirectionDescription = Settings.GradientDirectionNames[Settings.gradientDirectionIndex];
        Settings.gradientDirectionButton?.SetText("Gradient Direction : " + Settings.GradientDirectionDescription);
        break;
      case "Title Mode":
        int num = Settings.ModeIndexByName(value);
        if (num < 0)
          break;
        Settings.TitleMode = num == 4 ? Settings.ColorMode.Gradient : (Settings.ColorMode) num;
        Settings.titleModeButton?.SetText("Title Mode : " + Settings.ColorModeNames[(int) Settings.TitleMode]);
        break;
      case "Background Mode":
        int index8 = Settings.ModeIndexByName(value);
        if (index8 < 0)
          break;
        Settings.BackgroundMode = (Settings.ColorMode) index8;
        Settings.backgroundModeButton?.SetText("Background Mode : " + Settings.ColorModeNames[index8]);
        break;
      case "Nextbot Speed":
        Nextbots.ApplyNextbotSpeed(value);
        break;
      case "Title Color":
        int index9 = Settings.ColorIndexByName(value);
        if (index9 < 0)
          break;
        Settings.titleColorIndex = index9;
        Settings.TitleColor = Settings.CustomColors[index9];
        Settings.TitleColorDescription = Settings.CustomColorNames[index9];
        Settings.titleColorButton?.SetText("Title Color : " + Settings.TitleColorDescription);
        break;
      case "TP Map":
        int index10 = Array.FindIndex<(string, string, string)>(Movement._maps, (Predicate<(string, string, string)>) (m => m.Item1 == value));
        if (index10 >= 0)
          Settings._selectedMapIndex = index10;
        Settings.teleportMapButton?.SetText("TP To : " + Movement._maps[Settings._selectedMapIndex].name);
        break;
      case "Pinwheel Color 1":
        int index11 = Settings.ColorIndexByName(value);
        if (index11 < 0)
          break;
        Settings.pinwheelColor1Index = index11;
        Settings.PinwheelColor1 = Settings.CustomColors[index11];
        Settings.PinwheelColor1Name = Settings.CustomColorNames[index11];
        Settings.pinwheelColor1Button?.SetText("Pinwheel Color 1 : " + Settings.PinwheelColor1Name);
        Main.pinwheelColor1 = (Color) Settings.PinwheelColor1;
        break;
      case "Pinwheel Color 2":
        int index12 = Settings.ColorIndexByName(value);
        if (index12 < 0)
          break;
        Settings.pinwheelColor2Index = index12;
        Settings.PinwheelColor2 = Settings.CustomColors[index12];
        Settings.PinwheelColor2Name = Settings.CustomColorNames[index12];
        Settings.pinwheelColor2Button?.SetText("Pinwheel Color 2 : " + Settings.PinwheelColor2Name);
        Main.pinwheelColor2 = (Color) Settings.PinwheelColor2;
        break;
      case "Anti Report Radius":
        for (int index13 = 0; index13 < Settings.AntiReportRadiusOptions.Length; ++index13)
        {
          if (Settings.AntiReportRadiusOptions[index13].desc == value)
          {
            Settings.antiReportRadiusIndex = index13;
            (Settings.AntiReportRadius, Settings.AntiReportRadiusDescription) = Settings.AntiReportRadiusOptions[index13];
            Settings.adjustAntiReportRadiusButton?.SetText("Anti Report Radius : " + Settings.AntiReportRadiusDescription);
            break;
          }
        }
        break;
      case "Boost Speed":
        for (int index14 = 0; index14 < Settings.SpeedOptions.Length; ++index14)
        {
          if (Settings.SpeedOptions[index14].desc == value)
          {
            Settings.currentSpeedIndex = index14;
            (Settings.SpeedboostSpeed, Settings.SpeedDescription) = Settings.SpeedOptions[index14];
            Settings.SpeedboostMultiplier = Settings.SpeedboostSpeed / 5f;
            Settings.adjustSpeedBoostButton?.SetText("Boost Speed : " + Settings.SpeedDescription);
            break;
          }
        }
        break;
      case "Projectile Size":
        for (int index15 = 0; index15 < Settings.ProjectileSizeOptions.Length; ++index15)
        {
          if (Settings.ProjectileSizeOptions[index15].desc == value)
          {
            Settings.ProjectileSizeIndex = index15;
            (Settings.snowballScale, Settings.ProjectileSizeDescription) = Settings.ProjectileSizeOptions[index15];
            Settings.adjustProjectileSizeButton?.SetText("Projectile Size : " + Settings.ProjectileSizeDescription);
            break;
          }
        }
        break;
      case "Gun Wiggle Speed":
        for (int index16 = 0; index16 < Settings.GunWiggleSpeedOptions.Length; ++index16)
        {
          if (Settings.GunWiggleSpeedOptions[index16].d == value)
          {
            Settings.gunWiggleSpeedIndex = index16;
            (Settings.GunWiggleSpeed, Settings.GunWiggleSpeedDescription) = Settings.GunWiggleSpeedOptions[index16];
            Settings.gunWiggleSpeedButton?.SetText("Wiggle Speed : " + Settings.GunWiggleSpeedDescription);
            break;
          }
        }
        break;
      case "Background Color 2":
        int index17 = Settings.ColorIndexByName(value);
        if (index17 < 0)
          break;
        Settings.backgroundColor2Index = index17;
        Settings.BackgroundColor2 = Settings.CustomColors[index17];
        Settings.BackgroundColor2Description = Settings.CustomColorNames[index17];
        Settings.adjustBackgroundColor2Button?.SetText("Background Color 2 : " + Settings.BackgroundColor2Description);
        break;
      case "Enabled Button Color 2":
        int index18 = Settings.ColorIndexByName(value);
        if (index18 < 0)
          break;
        Settings.enabledButtonColor2Index = index18;
        Settings.EnabledButtonColor2 = Settings.CustomColors[index18];
        Settings.EnabledButtonColor2Description = Settings.CustomColorNames[index18];
        Settings.adjustEnabledButtonColor2Button?.SetText("Enabled Button Color 2 : " + Settings.EnabledButtonColor2Description);
        break;
      case "Gun Hover Color":
        Settings.LoadGunColor(value, ref Settings.gunHoverColorIndex, ref Settings.GunHoverColor, ref Settings.GunHoverColorDescription, ref Settings.GunHoverRainbow, Settings.gunHoverColorButton, "Gun Hover Color");
        break;
      case "Accent Color 2":
        int index19 = Settings.ColorIndexByName(value);
        if (index19 < 0)
          break;
        Settings.accentStripColor2Index = index19;
        Settings.AccentStripColor2 = Settings.CustomColors[index19];
        Settings.AccentStripColor2Description = Settings.CustomColorNames[index19];
        Settings.adjustAccentStripColor2Button?.SetText("Accent Color 2 : " + Settings.AccentStripColor2Description);
        break;
      case "Accent Anim Speed":
        for (int index20 = 0; index20 < Settings.AnimationSpeedOptions.Length; ++index20)
        {
          if (Settings.AnimationSpeedOptions[index20].desc == value)
          {
            Settings.accentStripAnimSpeedIndex = index20;
            (Settings.AccentStripAnimSpeed, Settings.AccentStripAnimSpeedDescription) = Settings.AnimationSpeedOptions[index20];
            Settings.adjustAccentStripAnimSpeedButton?.SetText("Animation Speed : " + Settings.AccentStripAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Platform Type":
        Settings.PlatformTypeIndex = Array.IndexOf<string>(Settings.PlatformTypes, value);
        if (Settings.PlatformTypeIndex < 0)
          break;
        Settings.PlatformType = Settings.PlatformTypes[Settings.PlatformTypeIndex];
        Settings.cyclePlatformTypeButton?.SetText("Platform Type : " + Settings.PlatformType);
        break;
      case "Projectile Speed":
        for (int index21 = 0; index21 < Settings.ProjectileSpeedOptions.Length; ++index21)
        {
          if (Settings.ProjectileSpeedOptions[index21].desc == value)
          {
            Settings.ProjectileSpeedIndex = index21;
            (Settings.ProjectileSpeed, Settings.ProjectileSpeedDescription) = Settings.ProjectileSpeedOptions[index21];
            Settings.adjustProjectileSpeedButton?.SetText("Projectile Speed : " + Settings.ProjectileSpeedDescription);
            break;
          }
        }
        break;
      case "Button Color 2":
        int index22 = Settings.ColorIndexByName(value);
        if (index22 < 0)
          break;
        Settings.buttonColor2Index = index22;
        Settings.ButtonColor2 = Settings.CustomColors[index22];
        Settings.ButtonColor2Description = Settings.CustomColorNames[index22];
        Settings.adjustButtonColor2Button?.SetText("Button Color 2 : " + Settings.ButtonColor2Description);
        break;
      case "Pinwheel Speed":
        for (int index23 = 0; index23 < Settings.PinwheelSpeedOptions.Length; ++index23)
        {
          if (Settings.PinwheelSpeedOptions[index23].desc == value)
          {
            Settings.pinwheelSpeedIndex = index23;
            (Main.pinwheelSpeed, Settings.PinwheelSpeedDescription) = Settings.PinwheelSpeedOptions[index23];
            Settings.pinwheelSpeedButton?.SetText("Pinwheel Speed : " + Settings.PinwheelSpeedDescription);
            break;
          }
        }
        break;
      case "Title Color 2":
        int index24 = Settings.ColorIndexByName(value);
        if (index24 < 0)
          break;
        Settings.titleColor2Index = index24;
        Settings.TitleColor2 = Settings.CustomColors[index24];
        Settings.TitleColor2Description = Settings.CustomColorNames[index24];
        Settings.titleColor2Button?.SetText("Title Color 2 : " + Settings.TitleColor2Description);
        break;
      case "Accent Mode":
        int index25 = Settings.ModeIndexByName(value);
        if (index25 < 0)
          break;
        Settings.AccentStripMode = (Settings.ColorMode) index25;
        Settings.accentStripModeButton?.SetText("Accent Mode : " + Settings.ColorModeNames[index25]);
        break;
      case "Button Color":
        int index26 = Settings.ColorIndexByName(value);
        if (index26 < 0)
          break;
        Settings.buttonColorIndex = index26;
        Settings.ButtonColor = Settings.CustomColors[index26];
        Settings.ButtonColorDescription = Settings.CustomColorNames[index26];
        Settings.adjustButtonColorButton?.SetText("Button Color : " + Settings.ButtonColorDescription);
        break;
      case "Outline Color":
        int index27 = Settings.ColorIndexByName(value);
        if (index27 < 0)
          break;
        Settings.outlineColorIndex = index27;
        Settings.OutlineColor = Settings.CustomColors[index27];
        Settings.OutlineColorDescription = Settings.CustomColorNames[index27];
        Settings.adjustOutlineColorButton?.SetText("Outline Color : " + Settings.OutlineColorDescription);
        break;
      case "Tracer Position":
        Settings.tracerPositionIndex = Array.IndexOf<string>(Settings.TracerPositions, value);
        if (Settings.tracerPositionIndex < 0)
          break;
        Settings.TracerPosition = Settings.TracerPositions[Settings.tracerPositionIndex];
        Settings.tracerPositionButton?.SetText("Tracer Position : " + Settings.TracerPosition);
        break;
      case "Nametag Type":
        Settings.nametagTypeIndex = Array.IndexOf<string>(Settings.NametagTypes, value);
        if (Settings.nametagTypeIndex < 0)
          break;
        Settings.NametagType = Settings.NametagTypes[Settings.nametagTypeIndex];
        Settings.cycleNametagTypeButton?.SetText("Nametag Type : " + Settings.NametagType);
        break;
      case "Outline Color 2":
        int index28 = Settings.ColorIndexByName(value);
        if (index28 < 0)
          break;
        Settings.outlineColor2Index = index28;
        Settings.OutlineColor2 = Settings.CustomColors[index28];
        Settings.OutlineColor2Description = Settings.CustomColorNames[index28];
        Settings.adjustOutlineColor2Button?.SetText("Outline Color 2 : " + Settings.OutlineColor2Description);
        break;
      case "Fly Speed":
        for (int index29 = 0; index29 < Settings.FlySpeedOptions.Length; ++index29)
        {
          if (Settings.FlySpeedOptions[index29].desc == value)
          {
            Settings.flySpeedIndex = index29;
            (Settings.FlySpeed, Settings.FlySpeedDescription) = Settings.FlySpeedOptions[index29];
            Settings.adjustFlySpeedButton?.SetText("Fly Speed : " + Settings.FlySpeedDescription);
            break;
          }
        }
        break;
      case "Long Arms Length":
        for (int index30 = 0; index30 < Settings.ArmLengthOptions.Length; ++index30)
        {
          if (Settings.ArmLengthOptions[index30].desc == value)
          {
            Settings.ArmLengthIndex = index30;
            (Settings.ArmLength, Settings.ArmLengthDescription) = Settings.ArmLengthOptions[index30];
            Settings.adjustArmLengthButton?.SetText("Long Arms Length : " + Settings.ArmLengthDescription);
            break;
          }
        }
        break;
      case "Current Projectile":
        Projectile.ApplyProjectile(value);
        break;
      case "Outline Anim Speed":
        for (int index31 = 0; index31 < Settings.AnimationSpeedOptions.Length; ++index31)
        {
          if (Settings.AnimationSpeedOptions[index31].desc == value)
          {
            Settings.outlineAnimSpeedIndex = index31;
            (Settings.OutlineAnimSpeed, Settings.OutlineAnimSpeedDescription) = Settings.AnimationSpeedOptions[index31];
            Settings.adjustOutlineAnimSpeedButton?.SetText("Animation Speed : " + Settings.OutlineAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Gun Line Thickness":
        for (int index32 = 0; index32 < Settings.GunLineThicknessOptions.Length; ++index32)
        {
          if (Settings.GunLineThicknessOptions[index32].d == value)
          {
            Settings.gunLineThicknessIndex = index32;
            (Settings.GunLineThickness, Settings.GunLineThicknessDescription) = Settings.GunLineThicknessOptions[index32];
            Settings.gunLineThicknessButton?.SetText("Gun Line Thickness : " + Settings.GunLineThicknessDescription);
            break;
          }
        }
        break;
      case "Menu Size":
        for (int index33 = 0; index33 < Settings.MenuSizeOptions.Length; ++index33)
        {
          if (Settings.MenuSizeOptions[index33].desc == value)
          {
            Settings.menuSizeIndex = index33;
            (Main.menuScale, Settings.MenuSizeDescription) = Settings.MenuSizeOptions[index33];
            Settings.adjustMenuSizeButton?.SetText("Menu Size : " + Settings.MenuSizeDescription);
            break;
          }
        }
        break;
      case "Background Color":
        int index34 = Settings.ColorIndexByName(value);
        if (index34 < 0)
          break;
        Settings.backgroundColorIndex = index34;
        Settings.BackgroundColor = Settings.CustomColors[index34];
        Settings.BackgroundColorDescription = Settings.CustomColorNames[index34];
        Settings.adjustBackgroundColorButton?.SetText("Background Color : " + Settings.BackgroundColorDescription);
        break;
      case "Accent Color":
        int index35 = Settings.ColorIndexByName(value);
        if (index35 < 0)
          break;
        Settings.accentStripColorIndex = index35;
        Settings.AccentStripColor = Settings.CustomColors[index35];
        Settings.AccentStripColorDescription = Settings.CustomColorNames[index35];
        Settings.adjustAccentStripColorButton?.SetText("Accent Color : " + Settings.AccentStripColorDescription);
        break;
      case "Title Animated Gradient":
        Settings.TitleAnimatedGradient = value == "On";
        Settings.titleAnimatedGradientButton?.SetText("Animated Gradient : " + (Settings.TitleAnimatedGradient ? "On" : "Off"));
        break;
      case "Roundness":
        for (int index36 = 0; index36 < Settings.RoundnessOptions.Length; ++index36)
        {
          if (Settings.RoundnessOptions[index36].desc == value)
          {
            Settings.roundnessIndex = index36;
            (Settings.MenuRoundness, Settings.RoundnessDescription) = Settings.RoundnessOptions[index36];
            Settings.adjustRoundnessButton?.SetText("Roundness : " + Settings.RoundnessDescription);
            break;
          }
        }
        break;
      case "Button Mode":
        int index37 = Settings.ModeIndexByName(value);
        if (index37 < 0)
          break;
        Settings.ButtonMode = (Settings.ColorMode) index37;
        Settings.buttonModeButton?.SetText("Button Mode : " + Settings.ColorModeNames[index37]);
        break;
      case "Enabled Button Color":
        int index38 = Settings.ColorIndexByName(value);
        if (index38 < 0)
          break;
        Settings.enabledButtonColorIndex = index38;
        Settings.EnabledButtonColor = Settings.CustomColors[index38];
        Settings.EnabledButtonColorDescription = Settings.CustomColorNames[index38];
        Settings.adjustEnabledButtonColorButton?.SetText("Enabled Button Color : " + Settings.EnabledButtonColorDescription);
        break;
      case "Time Of Day":
        Settings.timeOfDayIndex = Array.IndexOf<string>(Settings.TimeOfDayNames, value);
        if (Settings.timeOfDayIndex < 0)
          break;
        Settings.TimeOfDayDescription = Settings.TimeOfDayNames[Settings.timeOfDayIndex];
        BetterDayNightManager.instance?.SetTimeOfDay(Settings.timeOfDayIndex, false);
        Settings.adjustTimeOfDayButton?.SetText("Time Of Day : " + Settings.TimeOfDayDescription);
        break;
      case "Click Sound":
        bool flag = false;
        for (int index39 = 0; index39 < ButtonHandler.SoundSequence.Count; ++index39)
        {
          if (ButtonHandler.SoundSequence[index39].Description == value)
          {
            Settings._currentSoundIndex = index39;
            Settings.ChangeSoundButton?.SetText("Click Sound : " + Settings.ClickSoundDescription);
            flag = true;
            break;
          }
        }
        if (flag)
          break;
        Settings._currentSoundIndex = 0;
        Settings.ChangeSoundButton?.SetText("Click Sound : " + Settings.ClickSoundDescription);
        break;
      case "Background Anim Speed":
        for (int index40 = 0; index40 < Settings.AnimationSpeedOptions.Length; ++index40)
        {
          if (Settings.AnimationSpeedOptions[index40].desc == value)
          {
            Settings.backgroundAnimSpeedIndex = index40;
            (Settings.BackgroundAnimSpeed, Settings.BackgroundAnimSpeedDescription) = Settings.AnimationSpeedOptions[index40];
            Settings.adjustBackgroundAnimSpeedButton?.SetText("Animation Speed : " + Settings.BackgroundAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Accent Strip Type":
        Settings.accentStripTypeIndex = Array.IndexOf<string>(Settings.AccentStripTypeNames, value);
        if (Settings.accentStripTypeIndex < 0)
          break;
        Settings.CurrentAccentStripType = (Settings.AccentStripType) Settings.accentStripTypeIndex;
        Settings.AccentStripTypeDescription = Settings.AccentStripTypeNames[Settings.accentStripTypeIndex];
        Settings.adjustAccentStripTypeButton?.SetText("Accent Strip : " + Settings.AccentStripTypeDescription);
        break;
      case "Button Anim Speed":
        for (int index41 = 0; index41 < Settings.AnimationSpeedOptions.Length; ++index41)
        {
          if (Settings.AnimationSpeedOptions[index41].desc == value)
          {
            Settings.buttonAnimSpeedIndex = index41;
            (Settings.ButtonAnimSpeed, Settings.ButtonAnimSpeedDescription) = Settings.AnimationSpeedOptions[index41];
            Settings.adjustButtonAnimSpeedButton?.SetText("Animation Speed : " + Settings.ButtonAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Outline Mode":
        int index42 = Settings.ModeIndexByName(value);
        if (index42 < 0)
          break;
        Settings.OutlineColorMode = (Settings.ColorMode) index42;
        Settings.outlineColorModeButton?.SetText("Outline Mode : " + Settings.ColorModeNames[index42]);
        break;
      case "Wall Walk Strength":
        for (int index43 = 0; index43 < Settings.WallWalkOptions.Length; ++index43)
        {
          if (Settings.WallWalkOptions[index43].desc == value)
          {
            Settings.WallWalkStrengthIndex = index43;
            (Settings.WallWalkStrength, Settings.WalkWalkStrengthDescription) = Settings.WallWalkOptions[index43];
            Settings.adjustWalkWalkStrengthButton?.SetText("Wall Walk Strength : " + Settings.WalkWalkStrengthDescription);
            break;
          }
        }
        break;
      case "Opacity":
        for (int index44 = 0; index44 < Settings.OpacityOptions.Length; ++index44)
        {
          if (Settings.OpacityOptions[index44].desc == value)
          {
            Settings.opacityIndex = index44;
            (Settings.MenuOpacity, Settings.OpacityDescription) = Settings.OpacityOptions[index44];
            Settings.adjustOpacityButton?.SetText("Opacity : " + Settings.OpacityDescription);
            break;
          }
        }
        break;
      case "Gun Pointer Size":
        for (int index45 = 0; index45 < Settings.GunPointerSizeOptions.Length; ++index45)
        {
          if (Settings.GunPointerSizeOptions[index45].d == value)
          {
            Settings.gunPointerSizeIndex = index45;
            (Settings.GunPointerSize, Settings.GunPointerSizeDescription) = Settings.GunPointerSizeOptions[index45];
            Settings.gunPointerSizeButton?.SetText("Gun Pointer Size : " + Settings.GunPointerSizeDescription);
            break;
          }
        }
        break;
      case "Gun Idle Color":
        Settings.LoadGunColor(value, ref Settings.gunIdleColorIndex, ref Settings.GunIdleColor, ref Settings.GunIdleColorDescription, ref Settings.GunIdleRainbow, Settings.gunIdleColorButton, "Gun Idle Color");
        break;
      case "Boards Anim Speed":
        for (int index46 = 0; index46 < Settings.AnimationSpeedOptions.Length; ++index46)
        {
          if (Settings.AnimationSpeedOptions[index46].desc == value)
          {
            Settings.boardsAnimSpeedIndex = index46;
            (Settings.BoardsAnimSpeed, Settings.BoardsAnimSpeedDescription) = Settings.AnimationSpeedOptions[index46];
            Settings.adjustBoardsAnimSpeedButton?.SetText("Animation Speed : " + Settings.BoardsAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Gun Fire Color":
        Settings.LoadGunColor(value, ref Settings.gunFireColorIndex, ref Settings.GunFireColor, ref Settings.GunFireColorDescription, ref Settings.GunFireRainbow, Settings.gunFireColorButton, "Gun Fire Color");
        break;
      case "Boards Mode":
        int index47 = Settings.ModeIndexByName(value);
        if (index47 < 0)
          break;
        Settings.BoardsMode = (Settings.ColorMode) index47;
        Settings.boardsModeButton?.SetText("Boards Mode : " + Settings.ColorModeNames[index47]);
        break;
      case "Gun Wiggle Intensity":
        for (int index48 = 0; index48 < Settings.GunWiggleIntensityOptions.Length; ++index48)
        {
          if (Settings.GunWiggleIntensityOptions[index48].d == value)
          {
            Settings.gunWiggleIntensityIndex = index48;
            (Settings.GunWiggleIntensity, Settings.GunWiggleIntensityDescription) = Settings.GunWiggleIntensityOptions[index48];
            Settings.gunWiggleIntensityButton?.SetText("Wiggle Intensity : " + Settings.GunWiggleIntensityDescription);
            break;
          }
        }
        break;
      case "Title Anim Speed":
        for (int index49 = 0; index49 < Settings.AnimationSpeedOptions.Length; ++index49)
        {
          if (Settings.AnimationSpeedOptions[index49].desc == value)
          {
            Settings.titleAnimSpeedIndex = index49;
            (Settings.TitleAnimSpeed, Settings.TitleAnimSpeedDescription) = Settings.AnimationSpeedOptions[index49];
            Settings.adjustTitleAnimSpeedButton?.SetText("Animation Speed : " + Settings.TitleAnimSpeedDescription);
            break;
          }
        }
        break;
      case "Gun Animation":
        Settings.gunAnimationIndex = Array.IndexOf<string>(Settings.GunAnimations, value);
        if (Settings.gunAnimationIndex < 0)
          break;
        Settings.GunAnimationType = Settings.GunAnimations[Settings.gunAnimationIndex];
        Settings.cycleGunAnimationButton?.SetText("Gun Animation : " + Settings.GunAnimationType);
        break;
    }
  }

  public enum ColorMode
  {
    Solid,
    Lerp,
    Rainbow,
    Strobe,
    Pinwheel,
    Gradient,
  }

  public enum GradientDirection
  {
    Horizontal,
    Vertical,
    Diagonal,
  }

  public enum ColorElement
  {
    Pinwheel,
    Outline,
    Background,
    Button,
    Title,
    AccentStrip,
    Boards,
  }

  public enum AccentStripType
  {
    Off,
    Top,
    Both,
  }

  public delegate bool ControllerInput();
}
