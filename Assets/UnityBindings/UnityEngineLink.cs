using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RhuEngine.Physics;
using TextCopy;
using RhuEngine.Managers;

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
    public static UnityMeshHolder MakeQuad()
    {
        Mesh mesh = new();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3(0.5f,-0.5f,0),
            new Vector3(0.5f, 0.5f,0),
            new Vector3(-0.5f, 0.5f,0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            2, 1, 0,
            3, 2, 0
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };
        mesh.uv = uv;
        return new UnityMeshHolder(EngineRunner._, mesh);
    }

    public void LoadStatics()
    {
        EngineRunner.OnVRChange += (change) => VRChange?.Invoke(change);
        ClipboardService.OverRide = new UnityClipBoardOverride();
        RTexture2D.Instance = new UnityTexture2D(EngineRunner);
        RMaterial.Instance = new UnityMaterial(EngineRunner);
        RMaterial.ConstInstance = new UnityMitStatics();
        RShader.Instance = new UnityShader(EngineRunner);
        RMesh.Instance = typeof(UnityMesh);
        RRenderer.Instance = new UnityRenderer(EngineRunner);
        RTime.Instance = new UnityTime();
        StaticMaterialManager.Instanances = new UnitStaticMits(EngineRunner);
        RenderThread.ExecuteOnStartOfFrame(() => RMesh.Quad = new RMesh(new UnityMesh(MakeQuad()), false));
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

    public void LoadInput(InputManager manager)
    {
    }
}
