using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using System.Text;
using RhuEngine.Components;
using RhuEngine.WorldObjects.ECS;
using System.Linq;
using RhuEngine.WorldObjects;

public class UnityLight : RenderLinkBase<RhuEngine.Components.Light>
{
    public GameObject gameObject;
    public UnityEngine.Light light;

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject = new GameObject("Light");
            gameObject.transform.parent = EngineRunner._.Root.transform;
            light = gameObject.AddComponent<UnityEngine.Light>();
            RenderingComponent.LightType.Changed += LightType_Changed;
            LightType_Changed(null);
            RenderingComponent.Range.Changed += Range_Changed;
            Range_Changed(null);
            RenderingComponent.SpotAngle.Changed += SpotAngle_Changed;
            SpotAngle_Changed(null);
            RenderingComponent.Size.Changed += Size_Changed;
            Size_Changed(null);
            RenderingComponent.Intensity.Changed += Intensity_Changed;
            Intensity_Changed(null);
            RenderingComponent.IndirectMultipiler.Changed += IndirectMultipiler_Changed;
            IndirectMultipiler_Changed(null);
            RenderingComponent.Color.Changed += Color_Changed;
            Color_Changed(null);
            RenderingComponent.ShadowType.Changed += ShadowType_Changed;
            ShadowType_Changed(null);
            RenderingComponent.LightCookie.LoadChange += LightCookie_LoadChange;
            LightCookie_LoadChange(null);
            RenderingComponent.Culling.Changed += Culling_Changed;
            Culling_Changed(null);
        });
    }


    private void Culling_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.renderingLayerMask = (int)RenderingComponent.Culling.Value;
        });
    }

    private void LightCookie_LoadChange(RTexture2D obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            var value = RenderingComponent.LightCookie.Asset;
            if (value == null)
            {
                light.cookie = null;
                return;
            }
            if (value.Tex == null)
            {
                light.cookie = EngineRunner._.LoadingTexture;
                return;
            }
            if (((UnityTexture2DHolder)value.Tex).texture == null)
            {
                light.cookie = EngineRunner._.LoadingTexture;
                return;
            }
            light.cookie = ((UnityTexture2DHolder)value.Tex).texture;
        });
    }

    private void ShadowType_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.shadows = RenderingComponent.ShadowType.Value switch
            {
                ShadowMode.Hard => LightShadows.Hard,
                ShadowMode.Soft => LightShadows.Soft,
                _ => LightShadows.None,
            };
        });
    }

    private void Color_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            var color = RenderingComponent.Color.Value;
            light.color = new Color(color.r, color.g, color.b, color.a);
        });
    }

    private void IndirectMultipiler_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.bounceIntensity = RenderingComponent.IndirectMultipiler;
        });
    }

    private void Intensity_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.intensity = RenderingComponent.Intensity;
        });
    }

    private void Size_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.areaSize = new Vector2(RenderingComponent.Size.Value, RenderingComponent.Size.Value);
        });
    }

    private void SpotAngle_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.spotAngle = RenderingComponent.SpotAngle;
        });
    }

    private void Range_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.range = RenderingComponent.Range;
        });
    }

    private void LightType_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            light.type = RenderingComponent.LightType.Value switch
            {
                RLightType.Spot => LightType.Spot,
                RLightType.Directional => LightType.Directional,
                RLightType.Point => LightType.Point,
                _ => LightType.Point,
            };
        });
    }

    private void Entity_GlobalTransformChange(Entity obj, bool data)
    {
        if (gameObject is null)
        {
            return;
        }
        var m = RenderingComponent.Entity.GlobalTrans;
        var pos = m.Translation;
        var rot = m.Rotation;
        var scale = m.Scale;
        gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
        gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
        gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
    }

    public override void Remove()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            UnityEngine.Object.Destroy(gameObject);
        });
    }

    public override void Started()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject?.SetActive(true);
        });
    }

    public override void Stopped()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject?.SetActive(false);
        });
    }

    public override void Render()
    {
        Entity_GlobalTransformChange(null, false);
    }
}


public class UIRender : RenderLinkBase<UICanvas>
{
    public override void Init()
    {
    }

    public override void Remove()
    {
    }

    public override void Render()
    {
        RenderingComponent?.RenderUI();
    }

    public override void Started()
    {
    }

    public override void Stopped()
    {
    }
}

public class TextRender : RenderLinkBase<WorldText>
{
    public override void Init()
    {
    }

    public override void Remove()
    {
    }

    public override void Render()
    {
        RenderingComponent.textRender.Render(Matrix.S(0.1f), RenderingComponent.Entity.GlobalTrans, RenderLayer.Text);
    }

    public override void Started()
    {
    }

    public override void Stopped()
    {
    }
}


public class UnityMeshRender : RenderLinkBase<MeshRender>
{
    public GameObject gameObject;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public Material[] materials = new Material[0];

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject = new GameObject("MeshRender");
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            RenderingComponent.materials.Changed += Materials_Changed;
            RenderingComponent.colorLinear.Changed += ColorLinear_Changed;
            RenderingComponent.OrderOffset.Changed += ColorLinear_Changed;
            RenderingComponent.mesh.LoadChange += Mesh_LoadChange;
            RenderingComponent.renderLayer.Changed += RenderLayer_Changed;
            meshRenderer.renderingLayerMask = (uint)RenderingComponent.renderLayer.Value;
            RenderingComponent.CastShadows.Changed += CastShadows_Changed;
            meshRenderer.shadowCastingMode = RenderingComponent.CastShadows.Value switch
            {
                ShadowCast.On => UnityEngine.Rendering.ShadowCastingMode.On,
                ShadowCast.TwoSided => UnityEngine.Rendering.ShadowCastingMode.TwoSided,
                ShadowCast.ShadowsOnly => UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly,
                _ => UnityEngine.Rendering.ShadowCastingMode.Off,
            };
            RenderingComponent.RecevieShadows.Changed += RecevieShadows_Changed;
            meshRenderer.receiveShadows = RenderingComponent.RecevieShadows;
            RenderingComponent.ReflectionProbs.Changed += ReflectionProbs_Changed;
            meshRenderer.reflectionProbeUsage = (RenderingComponent.ReflectionProbs.Value) ? UnityEngine.Rendering.ReflectionProbeUsage.Off : UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            RenderingComponent.LightProbs.Changed += LightProbs_Changed;
            meshRenderer.lightProbeUsage = (RenderingComponent.LightProbs.Value) ? UnityEngine.Rendering.LightProbeUsage.Off : UnityEngine.Rendering.LightProbeUsage.BlendProbes;
        });
    }

    private void LightProbs_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            meshRenderer.lightProbeUsage = (RenderingComponent.LightProbs.Value) ? UnityEngine.Rendering.LightProbeUsage.Off : UnityEngine.Rendering.LightProbeUsage.BlendProbes;
        });
    }

    private void ReflectionProbs_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            meshRenderer.reflectionProbeUsage = (RenderingComponent.ReflectionProbs.Value) ? UnityEngine.Rendering.ReflectionProbeUsage.Off : UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        });
    }

    private void RecevieShadows_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            meshRenderer.receiveShadows = RenderingComponent.RecevieShadows;
        });
    }

    private void CastShadows_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            meshRenderer.shadowCastingMode = RenderingComponent.CastShadows.Value switch
            {
                ShadowCast.On => UnityEngine.Rendering.ShadowCastingMode.On,
                ShadowCast.TwoSided => UnityEngine.Rendering.ShadowCastingMode.TwoSided,
                ShadowCast.ShadowsOnly => UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly,
                _ => UnityEngine.Rendering.ShadowCastingMode.Off,
            };
        });
    }

    private void RenderLayer_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            meshRenderer.renderingLayerMask = (uint)RenderingComponent.renderLayer.Value;
        });
    }

    private void Mesh_LoadChange(RMesh obj)
    {
        if (obj is null)
        {
            meshFilter.mesh = null;
        }
        else
        {
            meshFilter.mesh = (Mesh)obj.mesh;
        }
    }

    private void Entity_GlobalTransformChange(Entity obj, bool data)
    {
        if (gameObject is null)
        {
            return;
        }
        var m = RenderingComponent.Entity.GlobalTrans;
        var pos = m.Translation;
        var rot = m.Rotation;
        var scale = m.Scale;
        gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
        gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
        gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
    }

    private void ColorLinear_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < materials.Length; i++)
            {
                var color = RenderingComponent.colorLinear.Value;
                if (materials[i] is not null)
                {
                    var mit = ((UnityMaterialHolder)RenderingComponent.materials[i].Asset?.Target).material;
                    materials[i].renderQueue = Math.Clamp(mit.renderQueue + RenderingComponent.OrderOffset.Value, -1000, 5000);
                    materials[i].color = new Color(color.r, color.g, color.b, color.a);
                }
            }
        });
    }

    private void Materials_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            if (meshRenderer is null)
            {
                return;
            }
            for (int i = 0; i < materials.Length; i++)
            {
                UnityEngine.Object.Destroy(materials[i]);
            }

            materials = new Material[RenderingComponent.materials.Count];
            for (int i = 0; i < RenderingComponent.materials.Count; i++)
            {
                var mit = (UnityMaterialHolder)RenderingComponent.materials[i].Asset?.Target;
                if (mit is not null)
                {
                    materials[i] = MitManager.GetMitWithOffset(RenderingComponent.materials[i].Asset, RenderingComponent.OrderOffset.Value, RenderingComponent.colorLinear.Value).material;
                }
                else
                {
                    materials[i] = null;
                }
            }
            meshRenderer.materials = materials;
        });
    }

    public override void Remove()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            UnityEngine.Object.Destroy(gameObject);
            for (int i = 0; i < materials.Length; i++)
            {
                UnityEngine.Object.Destroy(materials[i]);
            }
        });
    }

    public override void Started()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject?.SetActive(true);
        });
    }

    public override void Stopped()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject?.SetActive(false);
        });
    }

    public bool firstRender = true;

    public override void Render()
    {
        if (firstRender)
        {
            Materials_Changed(null);
            ColorLinear_Changed(null);
            firstRender = false;
        }
        Entity_GlobalTransformChange(null, false);
    }
}