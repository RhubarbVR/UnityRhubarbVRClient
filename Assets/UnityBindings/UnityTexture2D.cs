using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using UnityEngine.Experimental.Rendering;
using System.Runtime.InteropServices;

public class UnityTexture2DHolder
{
    public Texture2D texture;
    public EngineRunner EngineRunner;
    public UnityTexture2DHolder(EngineRunner engineRunner, Texture2D MakeMit)
    {
        EngineRunner = engineRunner;
        texture = MakeMit;
    }
    public UnityTexture2DHolder(EngineRunner engineRunner, Func<Texture2D> MakeMit)
    {
        EngineRunner = engineRunner;
        engineRunner.RunonMainThread(() => texture = MakeMit());
    }
    public void Action(Action<Texture2D> action)
    {
        EngineRunner.RunonMainThread(() => action(texture));
    }
}


public class UnityTexture2D : IRTexture2D
{
    public RTexture2D White => new(new UnityTexture2DHolder(EngineRunner._, Texture2D.whiteTexture));

    public EngineRunner EngineRunner { get; }

    public int GetHeight(object target)
    {
        return ((UnityTexture2DHolder)target).texture?.height ?? 0;
    }

    public int GetWidth(object target)
    {
        return ((UnityTexture2DHolder)target).texture?.width ?? 0;
    }

    public object Make(TexType dynamic, TexFormat rgba32Linear)
    {
        var unityFormat = TextureFormat.RGBAFloat;
        var linear = false;
        switch (rgba32Linear)
        {
            case TexFormat.None:
                break;
            case TexFormat.Rgba32:
                unityFormat = TextureFormat.RGBA32;
                break;
            case TexFormat.Rgba32Linear:
                unityFormat = TextureFormat.RGBA32;
                linear = true;
                break;
            case TexFormat.Bgra32:
                unityFormat = TextureFormat.BGRA32;
                break;
            case TexFormat.Bgra32Linear:
                unityFormat = TextureFormat.BGRA32;
                linear = true;
                break;
            case TexFormat.Rg11b10:
                unityFormat = TextureFormat.RGBA32;
                linear = true;
                break;
            case TexFormat.Rgb10a2:
                unityFormat = TextureFormat.RGBA32;
                linear = true;
                break;
            case TexFormat.Rgba64:
                unityFormat = TextureFormat.RGBA64;
                break;
            case TexFormat.R8:
                unityFormat = TextureFormat.R8;
                break;
            case TexFormat.R16:
                unityFormat = TextureFormat.R16;
                break;
            case TexFormat.DepthStencil:
                unityFormat = TextureFormat.R8;
                break;
            case TexFormat.Depth16:
                unityFormat = TextureFormat.R16;
                break;
            default:
                break;
        }
        return new UnityTexture2DHolder(EngineRunner, () =>
        {
            return new Texture2D(2, 2, unityFormat, true, linear);
        });
    }

    public object MakeFromColors(Colorb[] color, int width, int height, bool srgb)
    {
        return new UnityTexture2DHolder(EngineRunner, () =>
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, true, !srgb);
            tex.SetPixelData(color, 0);
            tex.Apply();
            return tex;
        });
    }

    public void SetAddressMode(object target, TexAddress value)
    {
        if (((UnityTexture2DHolder)target) == null)
        {
            return;
        }
        ((UnityTexture2DHolder)target).Action((tex) =>
        {
            tex.wrapMode = value switch
            {
                TexAddress.Wrap => TextureWrapMode.Repeat,
                TexAddress.Clamp => TextureWrapMode.Clamp,
                TexAddress.Mirror => TextureWrapMode.Mirror,
                _ => TextureWrapMode.Mirror,
            };
        });
    }

    public void SetAnisoptropy(object target, int value)
    {
        if (((UnityTexture2DHolder)target) == null)
        {
            return;
        }
        ((UnityTexture2DHolder)target).Action((tex) =>
        {
            tex.anisoLevel = value;
        });
    }

    public void SetColors(object tex, int width, int height, byte[] rgbaData)
    {
        if (((UnityTexture2DHolder)tex) == null)
        {
            return;
        }
        ((UnityTexture2DHolder)tex).Action((tex) =>
        {
            tex.Reinitialize(width, height);
            tex.SetPixelData(rgbaData, 0);
            tex.Apply();
        });
    }

    public void SetSampleMode(object target, TexSample value)
    {
        if (((UnityTexture2DHolder)target) == null)
        {
            return;
        }
        ((UnityTexture2DHolder)target).Action((tex) =>
        {
            tex.filterMode = value switch
            {
                TexSample.Linear => FilterMode.Bilinear,
                TexSample.Point => FilterMode.Point,
                TexSample.Anisotropic => FilterMode.Trilinear,
                _ => FilterMode.Bilinear,
            };
        });
    }

    public void SetSize(object tex, int width, int height)
    {
        if (((UnityTexture2DHolder)tex) == null)
        {
            return;
        }
        ((UnityTexture2DHolder)tex).Action((tex) =>
        {
            tex.Reinitialize(width, height);
        });
    }

    public UnityTexture2D(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

}
