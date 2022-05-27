using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using UnityEditor;
using RNumerics;

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
            var prams = EngineRunner.RunonMainThread(() =>
            {
                return MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { ((Material)tex) });
            });
            foreach (var item in prams)
            {
                var name = RMaterial.RenameFromEngine(item.displayName);
                switch (item.type)
                {
                    case MaterialProperty.PropType.Color:
                        if (name != RMaterial.MainColor)
                        {
                            yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Vector4 };
                        }
                        break;
                    case MaterialProperty.PropType.Vector:
                        yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Vector3 };
                        break;
                    case MaterialProperty.PropType.Float:
                        yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Float };
                        break;
                    case MaterialProperty.PropType.Range:
                        yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Vector2 };
                        break;
                    case MaterialProperty.PropType.Texture:
                        yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Texture };
                        break;
                    case MaterialProperty.PropType.Int:
                        yield return new RMaterial.RMatParamInfo { name = name, type = MaterialParam.Int };
                        break;
                    default:
                        break;
                }
            }
        }
    }

   
    public object Make(RShader rShader)
    {
        return EngineRunner.RunonMainThread(() =>
        {
            return new Material((Shader)(rShader.e));
        });
    }

    public void Pram(object ex, string tex, object value)
    {
        EngineRunner.RunonMainThread(() =>
        {
            tex = RMaterial.RenameFromRhubarb(tex);
            if (value is Colorb value1)
            {
                var colorGamma = new Color(value1.r, value1.g, value1.b, value1.a);
                ((Material)ex).SetColor(tex, colorGamma);
                return;
            }
            if (value is Colorf value2)
            {
                var colorGamma = new Color(value2.r, value2.g, value2.b, value2.a);
                ((Material)ex).SetColor(tex, colorGamma);
                return;
            }
            if (value is ColorHSV color)
            {
                var temp = color.ConvertToRGB();
                var colorGamma = new Color(temp.r, temp.g, temp.b, temp.a);
                ((Material)ex).SetColor(tex, colorGamma);
                return;
            }

            if (value is Vector4f)
            {
                var old = (Vector4f)value;
                ((Material)ex).SetVector(tex, new Vector4(old.x, old.y, old.z, old.w));
                return;
            }

            if (value is float)
            {
                ((Material)ex).SetFloat(tex, (float)value);
                return;
            }
            if (value is RTexture2D texer)
            {
                if (texer is null)
                {
                    ((Material)ex).SetTexture(tex, null);
                }
                if (texer.Tex is null)
                {
                    return;
                }
                Debug.Log("Loaded Texture");
                if(RMaterial.MainTexture == tex)
                {
                    ((Material)ex).mainTexture = ((Texture2D)texer.Tex);
                }
                else
                {
                    ((Material)ex).SetTexture(tex, ((Texture2D)texer.Tex));
                }
                return;
            }
            Debug.LogError(tex + " of type " + value.GetType().GetFormattedName() + " Not found");
        });
    }

    public void SetRenderQueueOffset(object mit, int tex)
    {
        ((Material)mit).renderQueue = 3000 + tex;
    }

    public int GetRenderQueueOffset(object mit)
    {
        return ((Material)mit).renderQueue - 3000;
    }
}
