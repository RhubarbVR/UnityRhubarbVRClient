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
    public class UnityLight : WorldPositionLinked<RhuEngine.Components.Light, UnityEngine.Light>
    {
        public override string ObjectName => "Light"; 

        public override void ContinueInit()
        {
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
        }

        private void Culling_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.renderingLayerMask = (int)RenderingComponent.Culling.Value;
            });
        }

        private void LightCookie_LoadChange(RTexture2D obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                var value = RenderingComponent.LightCookie.Asset;
                if (value == null)
                {
                    UnityComponent.cookie = null;
                    return;
                }
                if (value.Tex == null)
                {
                    UnityComponent.cookie = EngineRunner._.LoadingTexture;
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    UnityComponent.cookie = EngineRunner._.LoadingTexture;
                    return;
                }
                UnityComponent.cookie = ((UnityTexture2DHolder)value.Tex).texture;
            });
        }

        private void ShadowType_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.shadows = RenderingComponent.ShadowType.Value switch
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
                UnityComponent.color = new Color(color.r, color.g, color.b, color.a);
            });
        }

        private void IndirectMultipiler_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.bounceIntensity = RenderingComponent.IndirectMultipiler;
            });
        }

        private void Intensity_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.intensity = RenderingComponent.Intensity;
            });
        }

        private void Size_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.cookieSize = RenderingComponent.Size.Value;
            });
        }

        private void SpotAngle_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.spotAngle = RenderingComponent.SpotAngle;
            });
        }

        private void Range_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.range = RenderingComponent.Range;
            });
        }

        private void LightType_Changed(IChangeable obj)
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityComponent.type = RenderingComponent.LightType.Value switch
                {
                    RLightType.Spot => LightType.Spot,
                    RLightType.Directional => LightType.Directional,
                    RLightType.Point => LightType.Point,
                    _ => LightType.Point,
                };
            });
        }
    }

}
