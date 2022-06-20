using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;

public class UnityRenderer : IRRenderer
{
    public UnityRenderer(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public EngineRunner EngineRunner { get; }

    public Matrix GetCameraRootMatrix()
    {
        var rot = EngineRunner.UserRoot.transform.localRotation;
        var pos = EngineRunner.UserRoot.transform.localPosition;
        var scale = EngineRunner.UserRoot.transform.localScale;
        return Matrix.TRS(new Vector3f(pos.x,pos.y,-pos.z),new Quaternionf(-rot.x,-rot.y,rot.z,rot.w),new Vector3f(scale.x,scale.y,scale.z));
    }

    public bool GetEnableSky()
    {
        return EngineRunner.Camera.clearFlags == CameraClearFlags.Skybox;
    }

    public void SetCameraRootMatrix(Matrix m)
    {
        var pos = m.Translation;
        var rot = m.Rotation;
        var scale = m.Scale;
        EngineRunner.UserRoot.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, (float.IsNaN(pos.z) ? 0 : -pos.z));
        EngineRunner.UserRoot.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : -rot.x, float.IsNaN(rot.y) ? 0 : -rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
        EngineRunner.UserRoot.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
    }

    public void SetEnableSky(bool e)
    {
        EngineRunner.RunonMainThread(() =>
        {
            EngineRunner.Camera.clearFlags = (e) ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        });
    }
}
