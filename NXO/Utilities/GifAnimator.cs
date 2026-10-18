
using UnityEngine;

#nullable enable
namespace NXO.Utilities;

public class GifAnimator : MonoBehaviour
{
  public GifDecoder.Frame[] frames;
  public Material mat;
  private int _i;
  private float _t;

  private void Update()
  {
    if (this.frames == null || this.frames.Length <= 1 || (Object) this.mat == (Object) null)
      return;
    this._t += Time.deltaTime;
    if ((double) this._t < (double) this.frames[this._i].delay)
      return;
    this._t = 0.0f;
    this._i = (this._i + 1) % this.frames.Length;
    this.mat.mainTexture = (Texture) this.frames[this._i].texture;
  }

  private void OnDestroy()
  {
    if (this.frames != null)
    {
      foreach (GifDecoder.Frame frame in this.frames)
      {
        if ((Object) frame.texture != (Object) null)
          Object.Destroy((Object) frame.texture);
      }
    }
    if (!((Object) this.mat != (Object) null))
      return;
    Object.Destroy((Object) this.mat);
  }
}
