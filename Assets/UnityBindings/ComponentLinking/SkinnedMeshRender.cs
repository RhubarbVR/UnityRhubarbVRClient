using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using RhuEngine.WorldObjects.ECS;
using RNumerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
    public class UnitySkinnedMeshRender : WorldPositionLinked<SkinnedMeshRender, SkinnedMeshRenderer>, IUnityMeshRender
    {
        public override void ContinueInit()
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
            LinkedComp.Armature.Changed += Armature_Changed;
            Armature_Changed(null);
            LinkedComp.BoundsBox.Changed += Bounds_Changed;
            LinkedComp.AutoBounds.Changed += Bounds_Changed;
            Bounds_Changed(null);
        }

        private void Bounds_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                if (LinkedComp.AutoBounds.Value)
                {
                    UnityComponent.ResetLocalBounds();
                    UnityComponent.ResetBounds();
                    var server = LinkedComp.mesh.Asset?.BoundingBox ?? AxisAlignedBox3f.Empty;
                    server.Scale(new Vector3f(1.5f));
                    UnityComponent.localBounds = new Bounds
                    {
                        min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                        max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                    };
                }
                else
                {
                    var center = LinkedComp.BoundsBox.Value.Center;
                    var exstends = LinkedComp.BoundsBox.Value.Extents;
                    UnityComponent.localBounds = new Bounds(new Vector3(center.x, center.y, center.z), new Vector3(exstends.x, exstends.y, exstends.z));
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
                if (LinkedComp.Armature.Target is null)
                {
                    UnityComponent.rootBone = null;
                    return;
                }
                ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).ChildrenReload += UnitySkinnedMeshRender_ChildrenReload;
                LastArmatureRenderLink = ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink);
                if (((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).childs.Length == 0)
                {
                    UnityComponent.rootBone = ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).gameObject.transform;
                }
                else
                {
                    UnityComponent.rootBone = ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).childs[0].gameObject.transform;
                }
                UnitySkinnedMeshRender_ChildrenReload();
            });
        }

        private void UnitySkinnedMeshRender_ChildrenReload()
        {
            if (UnityComponent.sharedMesh is null)
            {
                UnityComponent.bones = null;
                return;
            }
            if (LinkedComp.Armature.Target is null)
            {
                UnityComponent.bones = null;
                return;
            }
            var newTransForms = new Transform[UnityComponent.sharedMesh.bindposes.Length];
            for (int i = 0; i < newTransForms.Length; i++)
            {
                if (i >= ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).childs.Length)
                {
                    newTransForms[i] = ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).gameObject.transform;
                }
                else
                {
                    newTransForms[i] = ((ArmatureRenderLink)LinkedComp.Armature.Target.WorldLink).childs[i].gameObject.transform;
                }
            }
            UnityComponent.bones = newTransForms;
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
                    UnityComponent.sharedMesh = null;
                    var server = AxisAlignedBox3f.Empty;
                    server.Scale(new Vector3f(1.5f));
                    UnityComponent.localBounds = new Bounds
                    {
                        min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                        max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                    };
                }
                else
                {
                    ((UnityMeshHolder)obj.Inst).LoadIn((mesh) =>
                    {
                        UnityComponent.sharedMesh = (Mesh)mesh;
                        UnitySkinnedMeshRender_ChildrenReload();
                        var server = LinkedComp.mesh.Asset?.BoundingBox ?? AxisAlignedBox3f.Empty;
                        server.Scale(new Vector3f(1.5f));
                        UnityComponent.localBounds = new Bounds
                        {
                            min = new Vector3(server.Min.x, server.Min.y, server.Min.z),
                            max = new Vector3(server.Max.x, server.Max.y, server.Max.z),
                        };
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

        public MeshRender MeshRender => LinkedComp;

        public override string ObjectName => "SkinnedMeshRender";

        public override void Render()
        {
            base.Render();
            var amountOnMesh = UnityComponent.sharedMesh?.blendShapeCount ?? 0;
            var loopAmount = Math.Min(amountOnMesh, LinkedComp.BlendShapes.Count);
            for (int i = 0; i < loopAmount; i++)
            {
                UnityComponent.SetBlendShapeWeight(i, LinkedComp.BlendShapes[i].Weight.Value);
            }
            for (int i = 0; i < amountOnMesh - loopAmount; i++)
            {
                UnityComponent.SetBlendShapeWeight(loopAmount + i, 0);
            }
        }
    }


}
