using RhuEngine.Components;
using RhuEngine.Linker;
using RhuEngine.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UnityBindings
{
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

    public interface IUnityMeshRender
    {
        public MeshRender MeshRender { get; }
        public void ReloadMitsToMaterial();
    }

}
