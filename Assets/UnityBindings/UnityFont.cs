using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using TMPro;
using RNumerics;

public class UnityFont : IRFont
{
    public RFont MainFont => EngineRunner._.MainFont.Fallback;

    public bool CharExsets(RenderFont renderFont, char c)
    {
        if((Font)renderFont.Fontist is null)
        {
            return false;
        }
        return EngineRunner._.RunonMainThread(() =>
        {
            try
            {
                return ((Font)renderFont.Fontist)?.HasCharacter(c) ?? false;
            }
            catch
            {
                return false;
            }
        });
    }

    public Vector2f TextSize(RenderFont rFont, char c)
    {
        if ((Font)rFont.Fontist is null)
        {
            return Vector2f.Zero;
        }
        return EngineRunner._.RunonMainThread(() =>
        {
            ((Font)rFont.Fontist).RequestCharactersInTexture(c.ToString(), 0);
            ((Font)rFont.Fontist).GetCharacterInfo(c, out var info);
            return new Vector2f((((float)info.advance + (c == ' '?1:0)) / (float)(13f)), 1);
        });
    }
}
