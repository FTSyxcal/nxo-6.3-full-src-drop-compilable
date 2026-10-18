
using NXO.Menu;
using NXO.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#nullable enable
namespace NXO.Mods.Categories;

public static class CustomNextbots
{
  public static readonly List<CustomNextbots.Entry> Entries = new List<CustomNextbots.Entry>();
  public static string currentOpen;
  private const string SeedText = "# Add one nextbot per line using this format:\r\n#\r\n# imageurl : soundurl\r\n#\r\n# There must be a space, a colon, then a space ( : ) between the two links.\r\n# Optionally give it a name:   name : imageurl : soundurl\r\n# Optionally add a jumpscare sound at the end (played when it catches you):\r\n#     imageurl : soundurl : jumpscareurl\r\n#\r\n# After editing, go in-game to Nextbots > Custom Nextbots > Reload Custom Nextbots.\r\n";
  private const string InstructionsText = "=== HOW TO ADD CUSTOM NEXTBOTS ===\r\n\r\n1. Open Nextbots.txt in this folder.\r\n\r\n2. Add one nextbot per line in this format:\r\n\r\n       imageurl : soundurl\r\n\r\n   - The image is what the nextbot looks like (a direct link to a .png / .jpg).\r\n   - The sound is the audio it plays / jumpscares with (a direct link to a .mp3 / .wav / .ogg).\r\n   - IMPORTANT: there must be a SPACE, a COLON, then a SPACE ( : ) between the two links.\r\n\r\n   Example:\r\n       https://site.com/troll.png : https://site.com/scream.mp3\r\n\r\n3. (Optional) Give it a custom name by adding one more field in front:\r\n\r\n       name : imageurl : soundurl\r\n\r\n   Example:\r\n       Troll : https://site.com/troll.png : https://site.com/scream.mp3\r\n\r\n4. (Optional) Give it a separate jumpscare sound by adding one more link at the end:\r\n\r\n       imageurl : soundurl : jumpscareurl\r\n       name : imageurl : soundurl : jumpscareurl\r\n\r\n   - The jumpscare sound plays when the nextbot catches you.\r\n   - Leave it off and it just uses its regular sound for the jumpscare instead.\r\n\r\n   Example:\r\n       Troll : https://site.com/troll.png : https://site.com/chase.mp3 : https://site.com/scream.mp3\r\n\r\n5. In-game go to: Nextbots > Custom Nextbots > Reload Custom Nextbots.\r\n\r\n6. Click a custom nextbot to Spawn, Rename, or Delete it.\r\n\r\nTIPS:\r\n- Use DIRECT image/audio links (the link should end in .png, .mp3, etc).\r\n- Renaming a nextbot in-game will save its name back to Nextbots.txt.\r\n- You can add as many as you want, one per line.\r\n";

  private static string Folder => Path.Combine(Variables.folderName, "Custom Nextbots");

  private static string ListPath => Path.Combine(CustomNextbots.Folder, "Nextbots.txt");

  private static string InstructionsPath => Path.Combine(CustomNextbots.Folder, "Instructions.txt");

  public static void Initialize()
  {
    try
    {
      Directory.CreateDirectory(CustomNextbots.Folder);
      if (!File.Exists(CustomNextbots.InstructionsPath))
        File.WriteAllText(CustomNextbots.InstructionsPath, "=== HOW TO ADD CUSTOM NEXTBOTS ===\r\n\r\n1. Open Nextbots.txt in this folder.\r\n\r\n2. Add one nextbot per line in this format:\r\n\r\n       imageurl : soundurl\r\n\r\n   - The image is what the nextbot looks like (a direct link to a .png / .jpg).\r\n   - The sound is the audio it plays / jumpscares with (a direct link to a .mp3 / .wav / .ogg).\r\n   - IMPORTANT: there must be a SPACE, a COLON, then a SPACE ( : ) between the two links.\r\n\r\n   Example:\r\n       https://site.com/troll.png : https://site.com/scream.mp3\r\n\r\n3. (Optional) Give it a custom name by adding one more field in front:\r\n\r\n       name : imageurl : soundurl\r\n\r\n   Example:\r\n       Troll : https://site.com/troll.png : https://site.com/scream.mp3\r\n\r\n4. (Optional) Give it a separate jumpscare sound by adding one more link at the end:\r\n\r\n       imageurl : soundurl : jumpscareurl\r\n       name : imageurl : soundurl : jumpscareurl\r\n\r\n   - The jumpscare sound plays when the nextbot catches you.\r\n   - Leave it off and it just uses its regular sound for the jumpscare instead.\r\n\r\n   Example:\r\n       Troll : https://site.com/troll.png : https://site.com/chase.mp3 : https://site.com/scream.mp3\r\n\r\n5. In-game go to: Nextbots > Custom Nextbots > Reload Custom Nextbots.\r\n\r\n6. Click a custom nextbot to Spawn, Rename, or Delete it.\r\n\r\nTIPS:\r\n- Use DIRECT image/audio links (the link should end in .png, .mp3, etc).\r\n- Renaming a nextbot in-game will save its name back to Nextbots.txt.\r\n- You can add as many as you want, one per line.\r\n");
      if (!File.Exists(CustomNextbots.ListPath))
        File.WriteAllText(CustomNextbots.ListPath, "# Add one nextbot per line using this format:\r\n#\r\n# imageurl : soundurl\r\n#\r\n# There must be a space, a colon, then a space ( : ) between the two links.\r\n# Optionally give it a name:   name : imageurl : soundurl\r\n# Optionally add a jumpscare sound at the end (played when it catches you):\r\n#     imageurl : soundurl : jumpscareurl\r\n#\r\n# After editing, go in-game to Nextbots > Custom Nextbots > Reload Custom Nextbots.\r\n");
      CustomNextbots.Load();
      CustomNextbots.BuildButtons();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("CustomNextbots init error: " + ex.Message));
    }
  }

  public static void Load()
  {
    CustomNextbots.Entries.Clear();
    if (!File.Exists(CustomNextbots.ListPath))
      return;
    int num1 = 1;
    foreach (string readAllLine in File.ReadAllLines(CustomNextbots.ListPath))
    {
      string str1 = readAllLine.Trim();
      if (str1.Length != 0 && !str1.StartsWith("#") && !str1.StartsWith("//"))
      {
        string[] array = ((IEnumerable<string>) str1.Split(new string[1]
        {
          " : "
        }, StringSplitOptions.None)).Select<string, string>((Func<string, string>) (p => p.Trim())).ToArray<string>();
        bool flag = !array[0].StartsWith("http", StringComparison.OrdinalIgnoreCase);
        int num2 = 0;
        CustomNextbots.Entry e = new CustomNextbots.Entry()
        {
          HasCustomName = flag
        };
        e.Name = flag ? array[num2++] : $"Custom Nextbot {num1}";
        e.Image = num2 < array.Length ? array[num2++] : "";
        e.Sound = num2 < array.Length ? array[num2++] : "";
        CustomNextbots.Entry entry = e;
        string str2;
        if (num2 >= array.Length)
        {
          str2 = "";
        }
        else
        {
          string[] strArray = array;
          int index = num2;
          int num3 = index + 1;
          str2 = strArray[index];
        }
        entry.Jumpscare = str2;
        if (!string.IsNullOrEmpty(e.Image))
        {
          while (CustomNextbots.Entries.Any<CustomNextbots.Entry>((Func<CustomNextbots.Entry, bool>) (x => x.Name == e.Name)))
            e.Name += " *";
          CustomNextbots.Entries.Add(e);
          ++num1;
        }
      }
    }
  }

  public static void Save()
  {
    try
    {
      List<string> contents = new List<string>()
      {
        "# imageurl : soundurl        (optional:  name : ...  and  ... : jumpscareurl)"
      };
      contents.AddRange(CustomNextbots.Entries.Select<CustomNextbots.Entry, string>((Func<CustomNextbots.Entry, string>) (e =>
      {
        string str1;
        if (!string.IsNullOrEmpty(e.Jumpscare))
          str1 = $"{e.Image} : {e.Sound} : {e.Jumpscare}";
        else
          str1 = $"{e.Image} : {e.Sound}";
        string str2 = str1;
        return e.HasCustomName ? $"{e.Name} : {str2}" : str2;
      })));
      File.WriteAllLines(CustomNextbots.ListPath, (IEnumerable<string>) contents);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("CustomNextbots save error: " + ex.Message));
    }
  }

  public static void Reload()
  {
    CustomNextbots.currentOpen = (string) null;
    CustomNextbots.Load();
    CustomNextbots.BuildButtons();
    Main.RefreshMenu();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Loaded, $"Custom Nextbots `{CustomNextbots.Entries.Count}`");
  }

  public static void BuildButtons()
  {
    List<ButtonHandler.Button> list = ((IEnumerable<ButtonHandler.Button>) ModButtons.buttons).Where<ButtonHandler.Button>((Func<ButtonHandler.Button, bool>) (b => b != null && b.Page != Category.Custom_Nextbots)).ToList<ButtonHandler.Button>();
    list.Add(new ButtonHandler.Button("Return", Category.Custom_Nextbots, false, false, (Action) (() => ButtonHandler.ChangePage(Category.Nextbots)))
    {
      isCategory = true
    });
    list.Add(new ButtonHandler.Button("Open Custom Nextbots Folder", Category.Custom_Nextbots, false, false, (Action) (() => ButtonHandler.OpenFolder(CustomNextbots.Folder))));
    foreach (CustomNextbots.Entry entry1 in CustomNextbots.Entries)
    {
      CustomNextbots.Entry entry = entry1;
      list.Add(new ButtonHandler.Button(entry.Name, Category.Custom_Nextbots, false, false, (Action) (() => CustomNextbots.Open(entry.Name)))
      {
        isCategory = true
      });
    }
    list.Add(new ButtonHandler.Button("Reload Custom Nextbots", Category.Custom_Nextbots, false, false, new Action(CustomNextbots.Reload)));
    ModButtons.buttons = list.ToArray();
  }

  public static List<ButtonHandler.Button> GetActionButtons()
  {
    string name = CustomNextbots.currentOpen;
    return new List<ButtonHandler.Button>()
    {
      new ButtonHandler.Button("Return", Category.Custom_Nextbots, false, false, (Action) (() =>
      {
        CustomNextbots.currentOpen = (string) null;
        Main.RedrawButtonList(-1);
      }))
      {
        isCategory = true
      },
      new ButtonHandler.Button("Spawn", Category.Custom_Nextbots, false, false, (Action) (() => CustomNextbots.Spawn(name))),
      new ButtonHandler.Button("Rename", Category.Custom_Nextbots, false, false, (Action) (() => CustomNextbots.BeginRename(name))),
      new ButtonHandler.Button("Delete", Category.Custom_Nextbots, false, false, (Action) (() => CustomNextbots.Delete(name)))
    };
  }

  private static void Open(string name)
  {
    CustomNextbots.currentOpen = name;
    ButtonHandler.ChangePage(Category.Custom_Nextbots);
  }

  private static void Spawn(string name)
  {
    CustomNextbots.Entry entry = CustomNextbots.Entries.FirstOrDefault<CustomNextbots.Entry>((Func<CustomNextbots.Entry, bool>) (x => x.Name == name));
    if (entry == null)
      return;
    Nextbots.SpawnNextbot(entry.Image, entry.Sound, name: entry.Name, jumpscareUrl: entry.Jumpscare);
  }

  private static void Delete(string name)
  {
    CustomNextbots.Entries.RemoveAll((Predicate<CustomNextbots.Entry>) (x => x.Name == name));
    CustomNextbots.Save();
    CustomNextbots.currentOpen = (string) null;
    CustomNextbots.BuildButtons();
    Main.RefreshMenu();
    NotificationLib.SendNotification(NotificationLib.NotificationType.Deleted, name);
  }

  private static void BeginRename(string name)
  {
    CustomNextbots.Entry e = CustomNextbots.Entries.FirstOrDefault<CustomNextbots.Entry>((Func<CustomNextbots.Entry, bool>) (x => x.Name == name));
    if (e == null)
      return;
    SearchAndKeyboard.OpenTypingKeyboard(name, "Enter nextbot name...");
    SearchAndKeyboard.onTypingComplete = (Action<string>) (newName =>
    {
      newName = newName.Trim();
      if (string.IsNullOrEmpty(newName) || newName == name)
        return;
      if (CustomNextbots.Entries.Any<CustomNextbots.Entry>((Func<CustomNextbots.Entry, bool>) (x => x.Name == newName)))
      {
        NotificationLib.SendNotification(NotificationLib.NotificationType.Error, "Name Already Exists");
      }
      else
      {
        e.Name = newName;
        e.HasCustomName = true;
        CustomNextbots.Save();
        CustomNextbots.currentOpen = (string) null;
        CustomNextbots.BuildButtons();
        Main.RefreshMenu();
        NotificationLib.SendNotification(NotificationLib.NotificationType.Saved, "Renamed → " + newName);
      }
    });
  }

  public class Entry
  {
    public string Name;
    public string Image;
    public string Sound;
    public string Jumpscare;
    public bool HasCustomName;
  }
}
