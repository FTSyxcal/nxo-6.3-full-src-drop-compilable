
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public static class GifDecoder
{
  private static readonly int[] InterlaceStart = new int[4]
  {
    0,
    4,
    2,
    1
  };
  private static readonly int[] InterlaceStep = new int[4]
  {
    8,
    8,
    4,
    2
  };

  public static bool IsGif(byte[] d)
  {
    return d != null && d.Length > 5 && d[0] == (byte) 71 && d[1] == (byte) 73 && d[2] == (byte) 70;
  }

  public static List<GifDecoder.Frame> Decode(byte[] bytes, int pixelBudget = 40000000)
  {
    List<GifDecoder.Frame> frameList = new List<GifDecoder.Frame>();
    try
    {
      if (!GifDecoder.IsGif(bytes))
        return frameList;
      int p = 6;
      int num1 = GifDecoder.ReadShort(bytes, ref p);
      int num2 = GifDecoder.ReadShort(bytes, ref p);
      int num3 = (int) bytes[p++];
      bool flag1 = (num3 & 128 /*0x80*/) != 0;
      int size1 = 2 << (num3 & 7);
      p += 2;
      if (num1 <= 0 || num2 <= 0)
        return frameList;
      int num4 = Mathf.Clamp(pixelBudget / (num1 * num2), 6, 500);
      Color32[] color32Array1 = flag1 ? GifDecoder.ReadColorTable(bytes, ref p, size1) : (Color32[]) null;
      Color32 color32 = new Color32((byte) 0, (byte) 0, (byte) 0, (byte) 0);
      Color32[] color32Array2 = new Color32[num1 * num2];
      int num5 = 10;
      int transparentIndex = -1;
      int num6 = 0;
      while (p < bytes.Length)
      {
        switch (bytes[p++])
        {
          case 33:
            if (bytes[p++] == (byte) 249)
            {
              ++p;
              int num7 = (int) bytes[p++];
              num6 = num7 >> 2 & 7;
              num5 = GifDecoder.ReadShort(bytes, ref p);
              int num8 = (int) bytes[p++];
              transparentIndex = (num7 & 1) != 0 ? num8 : -1;
              ++p;
              break;
            }
            GifDecoder.SkipSubBlocks(bytes, ref p);
            break;
          case 44:
            int ix = GifDecoder.ReadShort(bytes, ref p);
            int iy = GifDecoder.ReadShort(bytes, ref p);
            int iw = GifDecoder.ReadShort(bytes, ref p);
            int ih = GifDecoder.ReadShort(bytes, ref p);
            int num9 = (int) bytes[p++];
            bool flag2 = (num9 & 128 /*0x80*/) != 0;
            bool interlace = (num9 & 64 /*0x40*/) != 0;
            int size2 = 2 << (num9 & 7);
            Color32[] palette = flag2 ? GifDecoder.ReadColorTable(bytes, ref p, size2) : color32Array1;
            int minCodeSize = (int) bytes[p++];
            byte[] idx = GifDecoder.DecodeLZW(bytes, ref p, minCodeSize, iw * ih);
            Color32[] color32Array3 = num6 == 3 ? (Color32[]) color32Array2.Clone() : (Color32[]) null;
            if (palette != null)
              GifDecoder.ApplyFrame(color32Array2, num1, num2, idx, ix, iy, iw, ih, interlace, palette, transparentIndex);
            if (frameList.Count < num4)
            {
              Texture2D texture2D = new Texture2D(num1, num2, TextureFormat.RGBA32, false);
              texture2D.SetPixels32(GifDecoder.FlipVertical(color32Array2, num1, num2));
              texture2D.wrapMode = TextureWrapMode.Clamp;
              texture2D.Apply(false);
              float num10 = (float) num5 / 100f;
              if ((double) num10 < 0.019999999552965164)
                num10 = 0.1f;
              frameList.Add(new GifDecoder.Frame()
              {
                texture = texture2D,
                delay = num10
              });
            }
            int num11;
            switch (num6)
            {
              case 2:
                for (int index1 = 0; index1 < ih; ++index1)
                {
                  for (int index2 = 0; index2 < iw; ++index2)
                  {
                    int num12 = ix + index2;
                    int num13 = iy + index1;
                    if (num12 < num1 && num13 < num2)
                      color32Array2[num13 * num1 + num12] = color32;
                  }
                }
                goto label_26;
              case 3:
                num11 = color32Array3 != null ? 1 : 0;
                break;
              default:
                num11 = 0;
                break;
            }
            if (num11 != 0)
              color32Array2 = color32Array3;
label_26:
            transparentIndex = -1;
            num6 = 0;
            if (frameList.Count < num4)
              break;
            goto label_33;
          default:
            goto label_33;
        }
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ("GIF decode error: " + ex.Message));
    }
label_33:
    return frameList;
  }

  private static int ReadShort(byte[] d, ref int p)
  {
    int num = (int) d[p] | (int) d[p + 1] << 8;
    p += 2;
    return num;
  }

  private static Color32[] ReadColorTable(byte[] d, ref int p, int size)
  {
    Color32[] color32Array = new Color32[size];
    for (int index = 0; index < size; ++index)
      color32Array[index] = new Color32(d[p++], d[p++], d[p++], byte.MaxValue);
    return color32Array;
  }

  private static void SkipSubBlocks(byte[] d, ref int p)
  {
    while (true)
    {
      byte[] numArray = d;
      int index = p++;
      int num;
      if ((num = (int) numArray[index]) != 0)
        p += num;
      else
        break;
    }
  }

  private static byte[] DecodeLZW(byte[] d, ref int p, int minCodeSize, int pixelCount)
  {
    List<byte> byteList1 = new List<byte>();
label_4:
    byte[] numArray = d;
    int index1 = p++;
    int num1;
    if ((num1 = (int) numArray[index1]) != 0)
    {
      for (int index2 = 0; index2 < num1; ++index2)
        byteList1.Add(d[p++]);
      goto label_4;
    }
    List<byte> byteList2 = new List<byte>(pixelCount);
    int clearCode = 1 << minCodeSize;
    int num2 = clearCode + 1;
    int num3 = minCodeSize + 1;
    List<int[]> dict = new List<int[]>();
    Reset();
    int num4 = 0;
    int[] sourceArray = (int[]) null;
    int num5 = byteList1.Count * 8;
    while (num4 + num3 <= num5)
    {
      int index3 = 0;
      for (int index4 = 0; index4 < num3; ++index4)
      {
        int num6 = num4 + index4;
        index3 |= ((int) byteList1[num6 >> 3] >> (num6 & 7) & 1) << index4;
      }
      num4 += num3;
      if (index3 != num2)
      {
        if (index3 == clearCode)
        {
          num3 = minCodeSize + 1;
          Reset();
          sourceArray = (int[]) null;
        }
        else
        {
          int[] destinationArray1;
          if (index3 < dict.Count && dict[index3] != null)
            destinationArray1 = dict[index3];
          else if (sourceArray != null)
          {
            destinationArray1 = new int[sourceArray.Length + 1];
            Array.Copy((Array) sourceArray, (Array) destinationArray1, sourceArray.Length);
            destinationArray1[sourceArray.Length] = sourceArray[0];
          }
          else
            break;
          for (int index5 = 0; index5 < destinationArray1.Length; ++index5)
            byteList2.Add((byte) destinationArray1[index5]);
          if (sourceArray != null)
          {
            int[] destinationArray2 = new int[sourceArray.Length + 1];
            Array.Copy((Array) sourceArray, (Array) destinationArray2, sourceArray.Length);
            destinationArray2[sourceArray.Length] = destinationArray1[0];
            dict.Add(destinationArray2);
          }
          sourceArray = destinationArray1;
          if (dict.Count == 1 << num3 && num3 < 12)
            ++num3;
        }
      }
      else
        break;
    }
    return byteList2.ToArray();

    void Reset()
    {
      dict.Clear();
      for (int index = 0; index < clearCode; ++index)
        dict.Add(new int[1]{ index });
      dict.Add((int[]) null);
      dict.Add((int[]) null);
    }
  }

  private static void ApplyFrame(
    Color32[] canvas,
    int W,
    int H,
    byte[] idx,
    int ix,
    int iy,
    int iw,
    int ih,
    bool interlace,
    Color32[] palette,
    int transparentIndex)
  {
    int src = 0;
    if (interlace)
    {
      for (int index1 = 0; index1 < 4; ++index1)
      {
        for (int index2 = GifDecoder.InterlaceStart[index1]; index2 < ih; index2 += GifDecoder.InterlaceStep[index1])
          src = GifDecoder.ApplyRow(canvas, W, H, idx, src, ix, iy + index2, iw, palette, transparentIndex);
      }
    }
    else
    {
      for (int index = 0; index < ih; ++index)
        src = GifDecoder.ApplyRow(canvas, W, H, idx, src, ix, iy + index, iw, palette, transparentIndex);
    }
  }

  private static int ApplyRow(
    Color32[] canvas,
    int W,
    int H,
    byte[] idx,
    int src,
    int ix,
    int cy,
    int iw,
    Color32[] palette,
    int transparentIndex)
  {
    for (int index1 = 0; index1 < iw && src < idx.Length; ++index1)
    {
      int index2 = (int) idx[src++];
      if (index2 != transparentIndex && index2 < palette.Length)
      {
        int num = ix + index1;
        if (num >= 0 && num < W && cy >= 0 && cy < H)
          canvas[cy * W + num] = palette[index2];
      }
    }
    return src;
  }

  private static Color32[] FlipVertical(Color32[] src, int w, int h)
  {
    Color32[] destinationArray = new Color32[src.Length];
    for (int index = 0; index < h; ++index)
      Array.Copy((Array) src, index * w, (Array) destinationArray, (h - 1 - index) * w, w);
    return destinationArray;
  }

  public class Frame
  {
    public Texture2D texture;
    public float delay;
  }
}
