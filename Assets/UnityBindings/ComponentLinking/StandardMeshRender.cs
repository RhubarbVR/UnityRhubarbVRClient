using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
    public class UnityMeshRender : WorldPositionLinked<MeshRender,MeshRenderer,MeshFilter>, IUnityMeshRender
    {

        public MeshRender MeshRender => LinkedComp;

        public override string ObjectName => "MeshRender";

        public override void ContInit()
        {
            LinkedComp.materials.Changed += Materials_Changed;
            LinkedComp.colorLinear.Changed += Materials_Changed;
            LinkedComp.OrderOffset.Changed += Materials_Changed;
            LinkedComp.mesh.LoadChange += Mesh_LoadChange;
            LinkedComp.renderLayer.Changed += RenderLayer_Changed;
            UnityComponent.renderingLayerMask = (uint)LinkedComp.renderLayer.Value;
            LinkedComp.CastShadows.Changed += CastShadows_Changed;
            UnityComponent.shadowCastingMode = LinkedComp.CastShadows.Value switch
            {
                ShadowCast.On => UnityEngine.Rendering.ShadowCastingMode.On,
                ShadowCast.TwoSided => UnityEngine.Rendering.ShadowCastingMode.TwoSided,
                ShadowCast.ShadowsOnly => UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly,
                _ => UnityEngine.Rendering.ShadowCastingMode.Off,
            };
            LinkedComp.RecevieShadows.Changed += RecevieShadows_Changed;
            UnityComponent.receiveShadows = LinkedComp.RecevieShadows;
            LinkedComp.ReflectionProbs.Changed += ReflectionProbs_Changed;
            UnityComponent.reflectionProbeUsage = (LinkedComp.ReflectionProbs.Value) ? UnityEngine.Rendering.ReflectionProbeUsage.Off : UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            LinkedComp.LightProbs.Changed += LightProbs_Changed;
            UnityComponent.lightProbeUsage = (LinkedComp.LightProbs.Value) ? UnityEngine.Rendering.LightProbeUsage.Off : UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            Materials_Changed(null);
        }

        private void LightProbs_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.lightProbeUsage = (LinkedComp.LightProbs.Value) ? UnityEngine.Rendering.LightProbeUsage.Off : UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            });
        }

        private void ReflectionProbs_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.reflectionProbeUsage = (LinkedComp.ReflectionProbs.Value) ? UnityEngine.Rendering.ReflectionProbeUsage.Off : UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
            });
        }

        private void RecevieShadows_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.receiveShadows = LinkedComp.RecevieShadows;
            });
        }

        private void CastShadows_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.shadowCastingMode = LinkedComp.CastShadows.Value switch
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
                UnityComponent.renderingLayerMask = (uint)LinkedComp.renderLayer.Value;
            });
        }

        private void Mesh_LoadChange(RMesh obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                if (obj is null)
                {
                    UnityComponent2.mesh = null;
                }
                else
                {
                    ((UnityMesh)obj.Inst).unityMeshHolder.LoadIn((mesh) => {
                        UnityComponent2.mesh = mesh;
                    });
                }
            });
        }

        public MatManager[] BoundMits = new MatManager[0];

        public void ReloadMitsToMaterial()
        {
            RUpdateManager.ExecuteOnEndOfFrame(this, () =>
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
                UnityComponent.materials = unitymits;
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
                BoundMits = new MatManager[LinkedComp.materials.Count];
                for (int i = 0; i < LinkedComp.materials.Count; i++)
                {
                    BoundMits[i] = new MatManager(LinkedComp.materials[i], this);
                }
            });
        }
    }
}
