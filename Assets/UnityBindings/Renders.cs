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
        RenderingComponent.textRender.Render(RNumerics.Matrix.Identity, RenderingComponent.Entity.GlobalTrans);
    }

    public override void Started()
    {
    }

    public override void Stopped()
    {
    }
}


public class UnityMeshRender : RenderLinkBase<MeshRender>
{
    public GameObject gameObject;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public Material[] materials = new Material[0];

    public override void Init()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            gameObject = new GameObject("MeshRender");
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            RenderingComponent.materials.Changed += Materials_Changed;
            RenderingComponent.colorLinear.Changed += ColorLinear_Changed;
            RenderingComponent.OrderOffset.Changed += ColorLinear_Changed;
            RenderingComponent.mesh.LoadChange += Mesh_LoadChange;
        });
    }

    private void Mesh_LoadChange(RMesh obj)
    {
        if (obj is null)
        {
            meshFilter.mesh = null;
        }
        else { 
            meshFilter.mesh = (Mesh)obj.mesh;
        }
    }

    private void Entity_GlobalTransformChange(Entity obj,bool data)
    {
            var m = RenderingComponent.Entity.GlobalTrans;
            var pos = m.Translation;
            var rot = m.Rotation;
            var scale = m.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
   }

    private void ColorLinear_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < materials.Length; i++)
            {
                var color = RenderingComponent.colorLinear.Value;
                if (materials[i] is not null)
                {
                    var mit = ((UnityMaterialHolder)RenderingComponent.materials[i].Asset?.Target).material;
                    materials[i].renderQueue = Math.Clamp(mit.renderQueue + RenderingComponent.OrderOffset.Value,-1000,5000);
                    materials[i].color = new Color(color.r, color.g, color.b, color.a);
                }
            }
        });
    }

    private void Materials_Changed(IChangeable obj)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            for (int i = 0; i < materials.Length; i++)
            {
                UnityEngine.Object.Destroy(materials[i]);
            }
            
            materials = new Material[RenderingComponent.materials.Count];
            for (int i = 0; i < RenderingComponent.materials.Count; i++)
            {
                var mit = (UnityMaterialHolder)RenderingComponent.materials[i].Asset?.Target;
                if(mit is not null)
                {
                    materials[i] = MitManager.GetMitWithOffset(RenderingComponent.materials[i].Asset, RenderingComponent.OrderOffset.Value, RenderingComponent.colorLinear.Value).material;
                }
                else
                {
                    materials[i] = null;
                }
            }
            meshRenderer.materials = materials;
        });
    }

    public override void Remove()
    {
        EngineRunner._.RunonMainThread(() =>
        {
            UnityEngine.Object.Destroy(gameObject);
            for (int i = 0; i < materials.Length; i++)
            {
                UnityEngine.Object.Destroy(materials[i]);
            }
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

    public bool firstRender = true;

    public override void Render()
    {
        if (firstRender)
        {
            Materials_Changed(null);
            ColorLinear_Changed(null);
            firstRender = false;
        }
        Entity_GlobalTransformChange(null, false);
    }
}