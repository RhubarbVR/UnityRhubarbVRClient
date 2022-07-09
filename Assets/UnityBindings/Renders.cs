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
using static UnityMeshRender;

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
            light.cookieSize = RenderingComponent.Size.Value;
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

public static class EntityHelpers
{
    public static void SetPosFromEntity(Transform transform, Entity entity, Entity reletive = null)
    {
        Matrix m;
        if(reletive is not null)
        {
            m = reletive.GlobalToLocal(entity.GlobalTrans);
        }
        else
        {
            m = entity.GlobalTrans;
        };
        var pos = m.Translation;
        var rot = m.Rotation;
        var scale = m.Scale;
        transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
        transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
        transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
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

public class ArmatureRenderLink : RenderLinkBase<Armature>
{
    public GameObject root;

    public GameObject[] children = new GameObject[0];

    public event Action ChildrenReload;

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            root = new GameObject("Armature");
            root.transform.parent = EngineRunner._.Root.transform;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            RenderingComponent.ArmatureEntitys.Changed += ArmatureEntitys_Changed;
            ArmatureEntitys_Changed(null);
        });
    }


    public override void Render()
    {
        if (root is null)
        {
            return;
        }
        EntityHelpers.SetPosFromEntity(root.transform, RenderingComponent.Entity);
       var minval = Math.Min(RenderingComponent.ArmatureEntitys.Count, children.Length);
        for (int i = 0; i < minval; i++)
        {
            EntityHelpers.SetPosFromEntity(children[i].transform, RenderingComponent.ArmatureEntitys[i].Target ?? RenderingComponent.Entity,RenderingComponent.Entity);
        }
    }

    private void ArmatureEntitys_Changed(IChangeable obj)
    {
        RWorld.ExecuteOnStartOfFrame(this, () =>
        {
            EngineRunner._.RunonMainThread(() =>
            {
                for (int i = 0; i < children.Length; i++)
                {
                    GameObject.Destroy(children[i]);
                }
                children = new GameObject[RenderingComponent.ArmatureEntitys.Count];
                for (int i = 0; i < RenderingComponent.ArmatureEntitys.Count; i++)
                {
                    children[i] = new GameObject(RenderingComponent.ArmatureEntitys[i].Target?.name.Value ?? "Null");
                    children[i].transform.parent = root.transform;
                }
                ChildrenReload?.Invoke();
            });
        });
    }

    public override void Remove()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < children.Length; i++)
            {
                GameObject.Destroy(children[i]);
            }
            GameObject.Destroy(root);
        });
    }


    public override void Started()
    {
    }

    public override void Stopped()
    {
    }
}

public interface IUnityMeshRender
{
    public MeshRender MeshRender { get; }
    public void ReloadMitsToMaterial();
}

public class UnitySkinnedMeshRender : RenderLinkBase<SkinnedMeshRender>, IUnityMeshRender
{
    public GameObject gameObject;
    public SkinnedMeshRenderer meshRenderer;

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject = new GameObject("SkinnedMeshRender");
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            RenderingComponent.materials.Changed += Materials_Changed;
            RenderingComponent.colorLinear.Changed += Materials_Changed;
            RenderingComponent.OrderOffset.Changed += Materials_Changed;
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
            Materials_Changed(null);
            RenderingComponent.Armature.Changed += Armature_Changed;
            Armature_Changed(null);
            RenderingComponent.BoundsBox.Changed += Bounds_Changed;
            RenderingComponent.AutoBounds.Changed += Bounds_Changed;
            Bounds_Changed(null);
        });
    }

    private void Bounds_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            if (RenderingComponent.AutoBounds.Value)
            {
                meshRenderer.ResetLocalBounds();
                meshRenderer.ResetBounds();
                var server = RenderingComponent.mesh.Asset?.BoundingBox ?? AxisAlignedBox3f.Empty;
                server.Scale(new Vector3f(1.5f));
                meshRenderer.localBounds = new Bounds
                {
                    min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                    max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                };
            }
            else
            {
                var center = RenderingComponent.BoundsBox.Value.Center;
                var exstends = RenderingComponent.BoundsBox.Value.Extents;
                meshRenderer.localBounds = new Bounds(new Vector3(center.x, center.y, center.z), new Vector3(exstends.x, exstends.y, exstends.z));
            }
        });
    }

    public ArmatureRenderLink LastArmatureRenderLink;

    private void Armature_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {

            if (LastArmatureRenderLink is not null)
            {
                LastArmatureRenderLink.ChildrenReload -= UnitySkinnedMeshRender_ChildrenReload;
                return;
            }
            if (RenderingComponent.Armature.Target is null)
            {
                meshRenderer.rootBone = null;
                return;
            }
            ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).ChildrenReload += UnitySkinnedMeshRender_ChildrenReload;
            LastArmatureRenderLink = ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink);
            if(((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).children.Length == 0)
            {
                meshRenderer.rootBone = ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).root.transform;
            }
            else
            {
                meshRenderer.rootBone = ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).children[0].transform;
            }
            UnitySkinnedMeshRender_ChildrenReload();
        });
    }

    private void UnitySkinnedMeshRender_ChildrenReload()
    {
        if(meshRenderer.sharedMesh is null)
        {
            meshRenderer.bones = null;
            return;
        }
        if (RenderingComponent.Armature.Target is null)
        {
            meshRenderer.bones = null;
            return;
        }
        var newTransForms = new Transform[meshRenderer.sharedMesh.bindposes.Length];
        for (int i = 0; i < newTransForms.Length; i++)
        {
            if(i >= ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).children.Length)
            {
                newTransForms[i] = ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).root.transform;
            }
            else
            {
                newTransForms[i] = ((ArmatureRenderLink)RenderingComponent.Armature.Target.RenderLink).children[i].transform;
            }
        }
        meshRenderer.bones = newTransForms;
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
        EngineRunner._.RunonMainThread(() =>
        {
            if (obj is null)
            {
                meshRenderer.sharedMesh = null;
                var server = AxisAlignedBox3f.Empty;
                server.Scale(new Vector3f(1.5f));
                meshRenderer.localBounds = new Bounds
                {
                    min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                    max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                };
            }
            else
            {
                ((UnityMeshHolder)obj.mesh).LoadIn((mesh) =>
                {
                    meshRenderer.sharedMesh = (Mesh)mesh;
                    UnitySkinnedMeshRender_ChildrenReload();
                    var server = RenderingComponent.mesh.Asset?.BoundingBox ?? AxisAlignedBox3f.Empty;
                    server.Scale(new Vector3f(1.5f));
                    meshRenderer.localBounds = new Bounds
                    {
                        min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                        max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                    };
                });
            }
        });
    }

    private void Entity_GlobalTransformChange(Entity obj, bool data)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            if (gameObject is null)
            {
                return;
            }
            EntityHelpers.SetPosFromEntity(gameObject.transform, RenderingComponent.Entity);
        });
    }

    public MatManager[] BoundMits = new MatManager[0];

    public void ReloadMitsToMaterial()
    {
        RWorld.ExecuteOnEndOfFrame(this, () =>
        {
            var unitymits = new Material[BoundMits.Length];
            for (int i = 0; i < BoundMits.Length; i++)
            {
                if (BoundMits[i].LastHolder is null)
                {
                    unitymits[i] = null;
                }
                else
                {
                    unitymits[i] = BoundMits[i].LastHolder.material;
                }
            }
            meshRenderer.materials = unitymits;
        });
    }

    private void Materials_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < BoundMits.Length; i++)
            {
                BoundMits[i]?.Dispose();
            }
            BoundMits = new MatManager[RenderingComponent.materials.Count];
            for (int i = 0; i < RenderingComponent.materials.Count; i++)
            {
                BoundMits[i] = new MatManager(RenderingComponent.materials[i], this);
            }
        });
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

    public MeshRender MeshRender => RenderingComponent;

    public override void Render()
    {
        if (gameObject is null)
        {
            return;
        }
        Entity_GlobalTransformChange(null, false);
        var amountOnMesh = (meshRenderer.sharedMesh?.blendShapeCount ?? 0);
        var loopAmount = Math.Min(amountOnMesh, RenderingComponent.BlendShapes.Count);
        for (int i = 0; i < loopAmount; i++)
        {
            meshRenderer.SetBlendShapeWeight(i, RenderingComponent.BlendShapes[i].Weight.Value);
        }
        for (int i = 0; i < amountOnMesh - loopAmount; i++)
        {
            meshRenderer.SetBlendShapeWeight(loopAmount + i, 0);
        }
    }
}


public class UnityMeshRender : RenderLinkBase<MeshRender>, IUnityMeshRender
{
    public GameObject gameObject;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public MeshRender MeshRender => RenderingComponent;

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject = new GameObject("MeshRender");
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            RenderingComponent.materials.Changed += Materials_Changed;
            RenderingComponent.colorLinear.Changed += Materials_Changed;
            RenderingComponent.OrderOffset.Changed += Materials_Changed;
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
            Materials_Changed(null);
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
        EngineRunner._.RunonMainThread(() =>
        {
            if (obj is null)
            {
                meshFilter.mesh = null;
            }
            else
            {
                ((UnityMeshHolder)obj.mesh).LoadIn((mesh) => {
                    meshFilter.mesh = mesh;
                });
            }
        });
    }

    private void Entity_GlobalTransformChange(Entity obj, bool data)
    {
        if (gameObject is null)
        {
            return;
        }
        EntityHelpers.SetPosFromEntity(gameObject.transform, RenderingComponent.Entity);
    }

    public MatManager[] BoundMits = new MatManager[0];

    public class MatManager : IDisposable
    {
        public AssetRef<RMaterial> TargetAssetRef;
        private readonly IUnityMeshRender unityMeshRender;

        public MatManager(AssetRef<RMaterial> assetRef, IUnityMeshRender unityMeshRender)
        {
            TargetAssetRef = assetRef;
            this.unityMeshRender = unityMeshRender;
            assetRef.LoadChange += AssetRef_LoadChange;
            AssetRef_LoadChange(null);
        }
        public RMaterial LastBound = null;
        public UnityMaterialHolder LastHolder = null;

        private void AssetRef_LoadChange(RMaterial obj)
        {
            if (LastBound is not null)
            {
                LastBound.PramChanged -= PramChanged;
            }
            if (LastHolder is not null)
            {
                try
                {
                    LastHolder.OnMaterialLoadedIn -= LastHolder_OnMaterialLoadedIn;
                }
                catch { }
                LastHolder = null;
            }
            EngineRunner._.RunonMainThread(() =>
            {
                if (TargetAssetRef.Asset is null)
                {
                    unityMeshRender.ReloadMitsToMaterial();
                    return;
                }
                TargetAssetRef.Asset.PramChanged += PramChanged;
                LoadMitIn();
            });
        }

        private void LastHolder_OnMaterialLoadedIn(Material obj)
        {
            unityMeshRender.ReloadMitsToMaterial();
        }

        private void LoadMitIn()
        {
            if (TargetAssetRef.Asset.Target is null)
            {
                RLog.Err("Had no loaded target");
                unityMeshRender.ReloadMitsToMaterial();
                return;
            }
            if (TargetAssetRef.Asset.Target is UnityMaterialHolder holder)
            {
                var endHolder = MitManager.GetMitWithOffset(TargetAssetRef.Asset, unityMeshRender.MeshRender.OrderOffset.Value, unityMeshRender.MeshRender.colorLinear.Value);
                if (endHolder is not null)
                {
                    LastHolder = endHolder;
                    endHolder.LoadIn(LastHolder_OnMaterialLoadedIn);
                }
                else
                {
                    RLog.Err("Failed to GetMitWithOffset");
                    unityMeshRender.ReloadMitsToMaterial();
                    return;
                }
            }
            else
            {
                RLog.Err("Was not a UnityMaterialHolder");
                unityMeshRender.ReloadMitsToMaterial();
                return;
            }
        }

        private void PramChanged(RMaterial obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                LoadMitIn();
            });
        }

        public void Dispose()
        {
            if (LastBound is not null)
            {
                LastBound.PramChanged -= PramChanged;
            }
        }
    }

    public void ReloadMitsToMaterial()
    {
        RWorld.ExecuteOnEndOfFrame(this, () =>
        {
            var unitymits = new Material[BoundMits.Length];
            for (int i = 0; i < BoundMits.Length; i++)
            {
                if (BoundMits[i].LastHolder is null)
                {
                    unitymits[i] = null;
                }
                else
                {
                    unitymits[i] = BoundMits[i].LastHolder.material;
                }
            }
            meshRenderer.materials = unitymits;
        });
    }

    private void Materials_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < BoundMits.Length; i++)
            {
                BoundMits[i]?.Dispose();
            }
            BoundMits = new MatManager[RenderingComponent.materials.Count];
            for (int i = 0; i < RenderingComponent.materials.Count; i++)
            {
                BoundMits[i] = new MatManager(RenderingComponent.materials[i], this);
            }
        });
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