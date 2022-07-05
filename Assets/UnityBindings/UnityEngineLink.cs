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

    public string BackendID => "Unity";

    public EngineRunner EngineRunner { get; }

    public bool ForceLibLoad => true;

    public bool InVR => EngineRunner.isInVR;

    public bool LiveVRChange { get; set; }

    public Type RenderSettingsType => typeof(UnityRenderSettings);

    public SupportedFancyFeatures SupportedFeatures => (
                SupportedFancyFeatures.Basic |
                SupportedFancyFeatures.Lighting |
                SupportedFancyFeatures.GlobalIllumination |
                SupportedFancyFeatures.MeshRenderShadowSettings |
                SupportedFancyFeatures.LightCookie |
                SupportedFancyFeatures.LightHalo |
                SupportedFancyFeatures.NativeSkinnedMesheRender |
                SupportedFancyFeatures.Camera |
                SupportedFancyFeatures.ReflectionProbes |
                SupportedFancyFeatures.BasicParticleSystem |
                SupportedFancyFeatures.AdvancedParticleSystem |
                SupportedFancyFeatures.PhysicalCamera |
                SupportedFancyFeatures.CalledCameraRender |
                SupportedFancyFeatures.LightProbeGroup
        );

    public event Action<bool> VRChange;

    Engine Engine;

    public void BindEngine(Engine engine)
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
    }

    public void ChangeVRState(bool isInVR)
    {
        VRChange?.Invoke(isInVR);
    }

    public void LoadArgs()
    {
        if (Engine._forceFlatscreen)
        {
            EngineRunner.ChangeVR(false);
        }
        else
        {
            EngineRunner.ChangeVR(false);
            EngineRunner.ChangeVR(true);
        }
    }
}
