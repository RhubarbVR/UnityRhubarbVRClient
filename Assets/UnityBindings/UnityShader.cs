using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;

public class UnityShader : IRShader
{
    public UnityShader(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }
    public EngineRunner EngineRunner { get; }
}
