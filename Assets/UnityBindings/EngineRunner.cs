using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using System.IO;
using UnityEngine.XR;
using RNumerics;
using System.Threading;

public class EngineRunner : MonoBehaviour
{
    [ThreadStatic]
    public static bool IsMainThread = false;
    public class Holder<T>
    {
        public T item;
    }

    public T RunonMainThread<T>(Func<T> action)
    {
        if (IsMainThread)
        {
            return action();
        }
        var manualResetEvent = new Semaphore(0, 1);
        var datapass = new Holder<T>();
        void ThreadAction()
        {
            datapass.item = action();
            manualResetEvent.Release();
        }
        runonmainthread.SafeAdd(ThreadAction);
        manualResetEvent.WaitOne();
        manualResetEvent.Close();
        manualResetEvent.Dispose();
        return datapass.item;
    }
    public void RunonMainThread(Action action)
    {
        if (IsMainThread)
        {
            action();
            return;
        }
        var manualResetEvent = new Semaphore(0,1);
        void ThreadAction()
        {
            action();
            manualResetEvent.Release();
        }
        runonmainthread.SafeAdd(ThreadAction);
        manualResetEvent.WaitOne();
        manualResetEvent.Close();
        manualResetEvent.Dispose();
        return;
    }
    public SafeList<Action> runonmainthread = new SafeList<Action>();

    public static EngineRunner _;


    private static Vector2f LastMousePos;
    public static Vector2f MouseDelta;

    public GameObject UserRoot;

    public GameObject UserHead;

    public void AddText(string id, string v, Matrix p)
    {
    }

    public GameObject CameraOffset;

    public GameObject Root;

    public void AddChar(string id, char c, Matrix p, Font instances, RhuEngine.Linker.FontStyle fontStyle, Vector2f textCut)
    {
    }

    public GameObject LeftController;

    public GameObject RightController;

    public Shader UnlitClip;

    public Shader Unlit;

    public Shader PBR;

    public Shader PBRClip;

    public InputDevice left;

    public InputDevice right;

    public bool isHardwarePresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                var inputDevices = new List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevices(inputDevices);

                foreach (var device in inputDevices)
                {
                    Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
                    if ((device.characteristics & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.None)
                    {
                        if ((device.characteristics & InputDeviceCharacteristics.Right) != InputDeviceCharacteristics.None)
                        {
                            right = device;
                        }
                        if ((device.characteristics & InputDeviceCharacteristics.Left) != InputDeviceCharacteristics.None)
                        {
                            left = device;
                        }
                    }
                }
                return true;
            }
        }
        return false;
    }

    public Engine engine;
    public OutputCapture cap;
    public UnityEngineLink link;

    public bool isVR = false;

    void Start()
    {
        IsMainThread = true;
        _ = this;
        isVR = isHardwarePresent();
        if (!isVR)
        {
            Debug.Log("Starting RuhbarbVR ScreenMode");
            CameraOffset.transform.localPosition = Vector3.zero;
            LeftController.SetActive(false);
            RightController.SetActive(false);
        }
        else
        {
            Debug.Log("Starting RuhbarbVR VRMode");
        }
        Debug.Log("Graphics Memory Size: " + SystemInfo.graphicsMemorySize);
        cap = new OutputCapture();
        link = new UnityEngineLink(this);
        var args = Environment.GetCommandLineArgs();
        var appPath = Path.GetDirectoryName(Application.dataPath);
        engine = new Engine(link, args, cap, appPath);
        engine.Init();
    }

    public class TempMesh
    {
        public bool UsedThisFrame = true;

        public GameObject gameObject;

        public MeshFilter meshfilter;

        public MeshRenderer meshRenderer;

        public Material Parentmaterial;

        public Material material;

        public void Reload(Mesh mesh, Material target, Matrix p, Colorf tint)
        {
            UsedThisFrame = true;
            if (Parentmaterial == target)
            {
                try
                {
                    material.color = new Color(tint.r, tint.g, tint.b, tint.a);
                }
                catch { }
            }
            else
            {
                Parentmaterial = target;
                UnityEngine.Object.Destroy(material);
                material = new Material(target);
                try
                {
                    material.color = new Color(tint.r, tint.g, tint.b, tint.a);
                }
                catch { }
            }

            meshRenderer.material = material;
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
        }

        public TempMesh(string id, Mesh mesh, Material target, Matrix p, Colorf tint)
        {
            Parentmaterial = target;
            material = new Material(target);
            try
            {
                material.color = new Color(tint.r, tint.g, tint.b, tint.a);
            }
            catch { }
            gameObject = new GameObject("TempMesh" + id);
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshfilter = gameObject.AddComponent<MeshFilter>();
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
        }

        public void Remove()
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(material);
        }
    }

    public Dictionary<string, TempMesh> tempmeshes = new();

    public void Draw(string id, Mesh mesh, Material target, Matrix p, Colorf tint)
    {
        if (!tempmeshes.ContainsKey(id))
        {
            tempmeshes.Add(id, new TempMesh(id, mesh, target, p, tint));
        }
        else
        {
            tempmeshes[id].Reload(mesh, target, p, tint);
        }
    }
    void MainThreadUpdate()
    {
        runonmainthread.SafeOperation((list) =>
        {
            foreach (var item in list)
            {
                item.Invoke();
            }
            list.Clear();
        });
    }

    public float speedMultply = 200f;

    void Update()
    {
        MainThreadUpdate();
        var sens = speedMultply * Time.deltaTime;
        MouseDelta = new Vector2f(Input.GetAxis("Mouse X") * sens, -Input.GetAxis("Mouse Y") * sens);
        foreach (var item in tempmeshes)
        {
            item.Value.UsedThisFrame = false;
        }
        MainThreadUpdate();
        engine.Step();
        MainThreadUpdate();
        var removethisframe = new List<string>();
        foreach (var item in tempmeshes)
        {
            if (!item.Value.UsedThisFrame)
            {
                removethisframe.Add(item.Key);
                item.Value.Remove();
            }
        }
        MainThreadUpdate();
        foreach (var item in removethisframe)
        {
            tempmeshes.Remove(item);
        }
        MainThreadUpdate();
    }
}
