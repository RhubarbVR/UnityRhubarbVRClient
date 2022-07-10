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
        public Vector2f AlbedoTextureTilling { set => YourData.Action((mit) => mit.SetTextureScale("_MainTex", new Vector2(value.x, value.y))); }
        public Vector2f AlbedoTextureOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_MainTex", new Vector2(value.x, value.y))); }
        public Vector2f MetallicTextureTilling { set => YourData.Action((mit) => mit.SetTextureScale("_MetallicGlossMap", new Vector2(value.x, value.y))); }
        public Vector2f MetallicTextureOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_MetallicGlossMap", new Vector2(value.x, value.y))); }
        public Vector2f NormalMapTilling { set => YourData.Action((mit) => mit.SetTextureScale("_BumpMap", new Vector2(value.x, value.y))); }
        public Vector2f NormalMapOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_BumpMap", new Vector2(value.x, value.y))); }
        public Vector2f HeightMapTilling { set => YourData.Action((mit) => mit.SetTextureScale("_ParallaxMap", new Vector2(value.x, value.y))); }
        public Vector2f HeightMapOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_ParallaxMap", new Vector2(value.x, value.y))); }
        public Vector2f OcclusionTilling { set => YourData.Action((mit) => mit.SetTextureScale("_OcclusionMap", new Vector2(value.x, value.y))); }
        public Vector2f OcclusionOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_OcclusionMap", new Vector2(value.x, value.y))); }
        public Vector2f DetailMaskTilling { set => YourData.Action((mit) => mit.SetTextureScale("_DetailMask", new Vector2(value.x, value.y))); }
        public Vector2f DetailMaskOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_DetailMask", new Vector2(value.x, value.y))); }
        public RTexture2D DetailAlbedo
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_DetailAlbedoMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_DetailAlbedoMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_DetailAlbedoMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_DetailAlbedoMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public Vector2f DetailAlbedoTilling { set => YourData.Action((mit) => mit.SetTextureScale("_DetailAlbedoMap", new Vector2(value.x, value.y))); }
        public Vector2f DetailAlbedoOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_DetailAlbedoMap", new Vector2(value.x, value.y))); }
        public RTexture2D DetailNormal
        {
            set => YourData.Action((mit) =>
            {
                if (value == null)
                {
                    mit.SetTexture("_DetailNormalMap", null);
                    return;
                }
                if (value.Tex == null)
                {
                    mit.SetTexture("_DetailNormalMap", EngineRunner._.LoadingTexture);
                    return;
                }
                if (((UnityTexture2DHolder)value.Tex).texture == null)
                {
                    mit.SetTexture("_DetailNormalMap", EngineRunner._.LoadingTexture);
                    return;
                }
                mit.SetTexture("_DetailNormalMap", ((UnityTexture2DHolder)value.Tex).texture);
            });
        }
        public Vector2f DetailNormalTilling { set => YourData.Action((mit) => mit.SetTextureScale("_DetailNormalMap", new Vector2(value.x, value.y))); }
        public Vector2f DetailNormalOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_DetailNormalMap", new Vector2(value.x, value.y))); }
        public float DetailNormalMapScale { set => YourData.Action((mit) => mit.SetFloat("_DetailNormalMapScale", value)); }
        public Vector2f EmissionTextureTilling { set => YourData.Action((mit) => mit.SetTextureScale("_EmissionMap", new Vector2(value.x, value.y))); }
        public Vector2f EmissionTextureOffset { set => YourData.Action((mit) => mit.SetTextureOffset("_EmissionMap", new Vector2(value.x, value.y))); }
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
        public bool dull;
        public Transparency tra;

        public Material LoadNewMit(Transparency transparency,bool dullSided)
        {
            dull = dullSided;
            tra = transparency;
            Material mit = (dull) ?
                transparency switch
                {
                    Transparency.Blend => new Material(EngineRunner._.TwoSidedUnlitTransparentBlend),
                    Transparency.Add => new Material(EngineRunner._.TwoSidedUnlitTransparentAdditive),
                    _ => new Material(EngineRunner._.TwoSidedUnlit),
                }:
                transparency switch
                {
                    Transparency.Blend => new Material(EngineRunner._.UnlitTransparentBlend),
                    Transparency.Add => new Material(EngineRunner._.UnlitTransparentAdditive),
                    _ => new Material(EngineRunner._.Unlit),
                };
            mit.color = YourData.material.color;
            mit.mainTexture = YourData.material.mainTexture;
            return mit;
        }

        public Transparency Transparency { set => YourData.Action((mit) => { YourData.material = LoadNewMit(value, dull); Object.Destroy(mit); }); }
        public Colorf Tint { set => YourData.Action((mit) => mit.color = new Color(value.r, value.g, value.b, value.a)); }
        public bool DullSided
        {
            set => YourData.Action((mit) => { YourData.material = LoadNewMit(tra, value); Object.Destroy(mit); });
        }
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