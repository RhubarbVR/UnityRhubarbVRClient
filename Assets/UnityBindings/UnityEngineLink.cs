using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RhuEngine.Physics;
using TextCopy;

public class UnityEngineLink : IEngineLink
{
    public UnityEngineLink(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public bool SpawnPlayer => true;

    public bool CanRender => true;

    public bool CanAudio => false;

    public bool CanInput => true;

    public string BackendID => "Unity 2021";

    public EngineRunner EngineRunner { get; }

    public bool ForceLibLoad => true;

    public bool InVR => EngineRunner.isInVR;

    public bool LiveVRChange => true;

    public Type RenderSettingsType => typeof(UnityRenderSettings);

    public event Action<bool> VRChange;

    RhuEngine.Engine Engine;

    public void BindEngine(RhuEngine.Engine engine)
    {
        Engine = engine;
        RLog.Instance = new UnityLoger();
    }

    public void ChangeVR(bool value)
    {
        EngineRunner.ChangeVR(value);
    }

    public void LoadStatics()
    {
        EngineRunner.OnVRChange += (change) => VRChange?.Invoke(change);
        ClipboardService.OverRide = new UnityClipBoardOverride();
        RTexture2D.Instance = new UnityTexture2D(EngineRunner);
        RMaterial.Instance = new UnityMaterial(EngineRunner);
        RMaterial.ConstInstance = new UnityMitStatics();
        RShader.Instance = new UnityShader(EngineRunner);
        RMesh.Instance = new UnityMesh(EngineRunner);
        RRenderer.Instance = new UnityRenderer(EngineRunner);
        RTime.Instance = new UnityTime();
        RInput.Instance = new UnityInput(EngineRunner);
        StaticMaterialManager.Instanances = new UnitStaticMits(EngineRunner);
        //Use bypass to load libs
        new RBullet.BulletPhsyicsLink(true).RegisterPhysics();
    }

    public void Start()
    {
        EngineRunner.ChangeVR(!Engine._forceFlatscreen);
    }

    public void ChangeVRState(bool isInVR)
    {
        VRChange?.Invoke(isInVR);
    }
}
