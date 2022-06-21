using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;

public class UnityText : IRText
{
    public EngineRunner EngineRunner;
    public UnityText(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public void Add(string id, string v, Matrix p)
    {
        EngineRunner.AddText(id, v, p);
    }

    public void Add(string id,string group, char c, Matrix p, Colorf color, RenderFont rFont, Vector2f textCut)
    {
        if ((Font)rFont?.Fontist is not null)
        {
            EngineRunner.AddChar(id, c, p, color, (Font)rFont.Fontist, textCut);
        }
    }

   
}
