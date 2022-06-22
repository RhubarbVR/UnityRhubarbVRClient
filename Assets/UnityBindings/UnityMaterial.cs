using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;

public class UnityMaterialHolder
{
    public Material material;
    public EngineRunner EngineRunner;
    public UnityMaterialHolder(EngineRunner engineRunner, Material MakeMit)
    {
        EngineRunner = engineRunner;
        material = MakeMit;
    }
    public UnityMaterialHolder(EngineRunner engineRunner,Func<Material> MakeMit)
    {
        EngineRunner = engineRunner;
        engineRunner.RunonMainThread(() => material = MakeMit());
    }
    public void Action(Action<Material> action)
    {
        EngineRunner.RunonMainThread(() => action(material));
    }
}

public class UnityMaterial : IRMaterial
{
    public EngineRunner EngineRunner { get; private set; }

    public UnityMaterial(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public IEnumerable<RMaterial.RMatParamInfo> GetAllParamInfo(object tex)
    {
        if (tex != null)
        {
            yield return new RMaterial.RMatParamInfo { name = "MAINTEXTURE_RHUBARB_CUSTOM", type = MaterialParam.Texture };
        }
    }

   
    public object Make(RShader rShader)
    {
        if(rShader is null)
        {
            return null;
        }
        return new UnityMaterialHolder(EngineRunner, () =>
        {
            return new Material((Shader)(rShader.e));
        });
    }

    public void Pram(object ex, string tex, object value)
    {
        if(((UnityMaterialHolder)ex) is null)
        {
            return;
        }
        ((UnityMaterialHolder)ex).Action((mit) =>
        {
            tex = RMaterial.RenameFromRhubarb(tex);
            if (value is Colorb value1)
            {
                var colorGamma = new Color(value1.r, value1.g, value1.b, value1.a);
                mit.SetColor(tex, colorGamma);
                return;
            }
            if (value is Colorf value2)
            {
                var colorGamma = new Color(value2.r, value2.g, value2.b, value2.a);
                mit.SetColor(tex, colorGamma);
                return;
            }
            if (value is ColorHSV color)
            {
                var temp = color.ConvertToRGB();
                var colorGamma = new Color(temp.r, temp.g, temp.b, temp.a);
                mit.SetColor(tex, colorGamma);
                return;
            }

            if (value is Vector4f)
            {
                var old = (Vector4f)value;
                mit.SetVector(tex, new Vector4(old.x, old.y, old.z, old.w));
                return;
            }

            if (value is float)
            {
                mit.SetFloat(tex, (float)value);
                return;
            }
            if (value is RTexture2D texer)
            {
                if (texer is null)
                {
                    mit.SetTexture(tex, null);
                }
                if (texer.Tex is null)
                {
                    return;
                }
                Debug.Log("Loaded Texture");
                if(RMaterial.MainTexture == tex)
                {
                    mit.mainTexture = ((UnityTexture2DHolder)texer.Tex).texture;
                }
                else
                {
                    mit.SetTexture(tex, ((UnityTexture2DHolder)texer.Tex).texture);
                }
                return;
            }
            Debug.LogError(tex + " of type " + value.GetType().GetFormattedName() + " Not found");
        });
    }

    public void SetRenderQueueOffset(object mit, int tex)
    {
        ((UnityMaterialHolder)mit).Action((mit) => mit.renderQueue = 3000 + tex);
    }
}
