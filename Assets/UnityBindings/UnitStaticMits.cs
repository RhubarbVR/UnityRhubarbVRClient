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
                if(value == null)
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
}