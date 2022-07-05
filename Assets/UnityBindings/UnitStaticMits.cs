using RhuEngine.Linker;
using RNumerics;
using UnityEngine;

internal class UnitStaticMits : IStaticMaterialManager
{
    public UnitStaticMits(EngineRunner EngineRunner)
    {
        this.EngineRunner = EngineRunner;
    }

    public EngineRunner EngineRunner { get; }

    public class TextMaterial : StaticMaterialBase<UnityMaterialHolder>, ITextMaterial
    {
        public TextMaterial()
        {
            UpdateMaterial(new UnityMaterialHolder(EngineRunner._, () => new Material(EngineRunner._.TextShader)));
        }
        public RTexture2D Texture { set => YourData.Action((mit) => mit.mainTexture = ((UnityTexture2DHolder)value.Tex).texture); }
    }

    public class PBRMaterial : StaticMaterialBase<UnityMaterialHolder>, IPBRMaterial
    {
        public PBRMaterial()
        {
            UpdateMaterial(new UnityMaterialHolder(EngineRunner._, () => new Material(EngineRunner._.PBRShader)));
        }

        public BasicRenderMode RenderMode
        {
            set => YourData.Action((material) =>
            {
                switch (value)
                {
                    case BasicRenderMode.CutOut:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.EnableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 2450;
                        break;
                    case BasicRenderMode.Transparent:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        break;
                    default:
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        break;
                }
            });
        }
        public RTexture2D AlbedoTexture
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_MainTex", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_MainTex", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_MainTex", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_MainTex", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public Colorf AlbedoTint { set => YourData.Action((mit) => mit.SetColor("_Color", new Color(value.r, value.g, value.b, value.a))); }
        public float AlphaCutOut { set => YourData.Action((mit) => mit.SetFloat("_Cutoff", value)); }
        public RTexture2D MetallicTexture
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_MetallicGlossMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_MetallicGlossMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_MetallicGlossMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_MetallicGlossMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public float Metallic { set => YourData.Action((mit) => mit.SetFloat("_Metallic", value)); }
        public float Smoothness { set => YourData.Action((mit) => mit.SetFloat("_Glossiness", value)); }
        public bool SmoothnessFromAlbedo { set => YourData.Action((mit) => mit.SetFloat("_SmoothnessTextureChannel", value ? 0 : 1f)); }
        public RTexture2D NormalMap
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_BumpMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_BumpMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_BumpMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_BumpMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public RTexture2D HeightMap
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_ParallaxMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_ParallaxMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_ParallaxMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_ParallaxMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public RTexture2D Occlusion
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_OcclusionMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_OcclusionMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_OcclusionMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_OcclusionMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public RTexture2D DetailMask
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_DetailMask", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_DetailMask", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_DetailMask", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_DetailMask", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public bool Emission { get; set; }
        public RTexture2D EmissionTexture
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_EmissionMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_EmissionMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_EmissionMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_EmissionMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public Colorf EmissionTint { set => YourData.Action((mit) => mit.SetColor("_EmissionColor", new Color(value.r, value.g, value.b, value.a))); }
        public Vector2f Tilling
        {
            set => YourData.Action((mit) =>
        {
            var newvalue = new Vector2(value.x, value.y);
            mit.SetTextureScale("_MainTex", newvalue);
            mit.SetTextureScale("_MetallicGlossMap", newvalue);
            mit.SetTextureScale("_BumpMap", newvalue);
            mit.SetTextureScale("_ParallaxMap", newvalue);
            mit.SetTextureScale("_OcclusionMap", newvalue);
            mit.SetTextureScale("_EmissionMap", newvalue);
            mit.SetTextureScale("_DetailMask", newvalue);
            mit.SetTextureScale("_DetailAlbedoMap", newvalue);
            mit.SetTextureScale("_DetailNormalMap", newvalue);
        });
        }
        public Vector2f Offset
        {
            set => YourData.Action((mit) =>
        {
            var newvalue = new Vector2(value.x, value.y);
            mit.SetTextureOffset("_MainTex", newvalue);
            mit.SetTextureOffset("_MetallicGlossMap", newvalue);
            mit.SetTextureOffset("_BumpMap", newvalue);
            mit.SetTextureOffset("_ParallaxMap", newvalue);
            mit.SetTextureOffset("_OcclusionMap", newvalue);
            mit.SetTextureOffset("_EmissionMap", newvalue);
            mit.SetTextureOffset("_DetailMask", newvalue);
            mit.SetTextureOffset("_DetailAlbedoMap", newvalue);
            mit.SetTextureOffset("_DetailNormalMap", newvalue);
        });
        }
    }

    public class UnlitMaterial : StaticMaterialBase<UnityMaterialHolder>, IUnlitMaterial
    {
        public UnlitMaterial()
        {
            UpdateMaterial(new UnityMaterialHolder(EngineRunner._, () => new Material(EngineRunner._.Unlit)));
        }
        public RTexture2D Texture
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.mainTexture = null;
                    return;
                }
                if (value.Tex == null)
                {
                    mit.mainTexture = EngineRunner._.LoadingTexture;
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.mainTexture = EngineRunner._.LoadingTexture;
                    return;
                }
                mit.mainTexture = ((UnityTexture2DHolder)value.Tex).texture;
            });
        }

        public Material LoadNewMit(Transparency transparency)
        {
            Material mit = transparency switch
            {
                Transparency.Blend => new Material(EngineRunner._.UnlitTransparentBlend),
                Transparency.Add => new Material(EngineRunner._.UnlitTransparentAdditive),
                _ => new Material(EngineRunner._.Unlit),
            };
            mit.color = YourData.material.color;
            mit.mainTexture = YourData.material.mainTexture;
            return mit;
        }

        public Transparency Transparency { set => YourData.Action((mit) => { YourData.material = LoadNewMit(value); Object.Destroy(mit); }); }
        public Colorf Tint { set => YourData.Action((mit) => mit.color = new Color(value.r, value.g, value.b, value.a)); }
    }

    public ITextMaterial CreateTextMaterial()
    {
        return new TextMaterial();
    }

    public IUnlitMaterial CreateUnlitMaterial()
    {
        return new UnlitMaterial();
    }

    public IPBRMaterial CreatePBRMaterial()
    {
        return new PBRMaterial();
    }

    public IToonMaterial CreateToonMaterial()
    {
        throw new System.NotImplementedException();
    }
}