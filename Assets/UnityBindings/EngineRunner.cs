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
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.XR.Management;

public class EngineRunner : MonoBehaviour
{
    public FontFallBackGroup MainFont;

    [ThreadStatic]
    public static bool IsMainThread = false;

    public SafeList<Action> runonmainthread = new SafeList<Action>();

    public static EngineRunner _;


    private static Vector2f LastMousePos;
    public static Vector2f MouseDelta;

    public GameObject UserRoot;

    public GameObject UserHead;

    public Camera Camera;

    public class TextRender : IRemoveLater
    {

        GameObject gameObject;
        TextMesh Text;
        public TextRender(string id, string v, Matrix p)
        {
            gameObject = new GameObject("TextString" + id);
            gameObject.transform.parent = EngineRunner._.TextRoot.transform;
            Text = gameObject.AddComponent<TextMesh>();
            Text.fontSize = 100;
            Text.alignment = TextAlignment.Center;
            Text.anchor = TextAnchor.UpperCenter;
            Reload(v, p);
        }

        public bool UsedThisFrame { get; set; } = true;

        public void Remove()
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(Text);
        }

        internal void Reload(string v, Matrix p)
        {
            UsedThisFrame = true;
            Text.text = v;
            var pos = p.Translation;
            var rot = p.Rotation * Quaternionf.Yawed180;
            var scale = p.Scale * new Vector3f(0.002f);
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, -(float.IsNaN(scale.z) ? 0 : scale.z));
        }
    }

    public class CharRender : IRemoveLater
    {

        GameObject gameObject;
        CharRenderComp Text;
        public CharRender(string id, char v, Matrix p, Colorf color, Font instances, Vector2f textCut)
        {
            gameObject = new GameObject("TextString" + id);
            gameObject.transform.parent = EngineRunner._.TextRoot.transform;
            Text = gameObject.AddComponent<CharRenderComp>();
            Text.StartCharRender(v, instances, new Color(color.r, color.g, color.b, color.a));
        }

        public bool UsedThisFrame { get; set; } = true;

        public void Remove()
        {
            UnityEngine.Object.Destroy(gameObject);
            UnityEngine.Object.Destroy(Text);
        }

        internal void Reload(char v, Matrix p, Colorf color, Font instances, Vector2f textCut)
        {
            UsedThisFrame = true;
            Text.UpdateRender(v, instances, new Color(color.r, color.g, color.b, color.a));
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = (new Vector3(-(float.IsNaN(scale.x) ? 0 : scale.x), float.IsNaN(scale.y) ? 0 : scale.y, (float.IsNaN(scale.z) ? 0 : scale.z))) / 355f;
        }
    }


    public void AddText(string id, string v, Matrix p)
    {
        id = "Text." + id;
        if (!tempObjects.ContainsKey(id))
        {
            tempObjects.Add(id, new TextRender(id, v, p));
        }
        else
        {
            ((TextRender)tempObjects[id]).Reload(v, p);
        }
    }

    public GameObject CameraOffset;

    public GameObject Root;
    public GameObject TextRoot;

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
        runonmainthread.SafeAdd(action);
    }

    public void AddChar(string id, char c, Matrix p, Colorf color, Font instances, Vector2f textCut)
    {
        if(instances == null)
        {
            return;
        }
        id = "Char." + id;
        if (!tempObjects.ContainsKey(id))
        {
            tempObjects.Add(id, new CharRender(id, c, p, color, instances, textCut));
        }
        else
        {
            ((CharRender)tempObjects[id]).Reload(c, p, color, instances, textCut);
        }
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
        InputDevices.deviceConnected += InputDevices_Update;
        InputDevices.deviceDisconnected += InputDevices_Update;
        InputDevices_Update(new InputDevice { });
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            return true;
        }
        return false;
    }


    public Engine engine;
    public OutputCapture cap;
    public UnityEngineLink link;

    public bool isVR = false;

    public bool IsVREnabled = false;

    IEnumerator Start()
    {
        XRSettings.enabled = IsVREnabled;
        IsMainThread = true;
        _ = this;
        yield return null;

        var xrLoader = XRGeneralSettings.Instance.Manager.activeLoader;
        var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
        if (!IsVREnabled)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            xrDisplay.Stop();
        }
        var platform = Application.platform;
        if(platform == RuntimePlatform.Android)
        {
            Debug.Log("Is on Android");
        }
        isVR = isHardwarePresent() && IsVREnabled;
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

    public interface IRemoveLater
    {
        public void Remove();


        public bool UsedThisFrame { get; set; }
    }

    public class TempMesh:IRemoveLater
    {
        public bool UsedThisFrame { get; set; } = true;

        public GameObject gameObject;

        public MeshFilter meshfilter;

        public MeshRenderer meshRenderer;

        Material targetMit;


        public void Reload(Mesh mesh, Material target, Matrix p)
        {
            if(targetMit != target)
            {
                targetMit = target;
            }
            UsedThisFrame = true;
            meshRenderer.material = target;
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, float.IsNaN(scale.z) ? 0 : scale.z);
        }

        public TempMesh(string id, Mesh mesh, Material target, Matrix p)
        {
            targetMit = target;
            gameObject = new GameObject("TempMesh" + id);
            gameObject.transform.parent = EngineRunner._.Root.transform;
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = target;
            meshfilter = gameObject.AddComponent<MeshFilter>();
            meshfilter.mesh = mesh;
            var pos = p.Translation;
            var rot = p.Rotation;
            var scale = p.Scale;
            gameObject.transform.localPosition = new Vector3(float.IsNaN(pos.x) ? 0 : pos.x, float.IsNaN(pos.y) ? 0 : pos.y, float.IsNaN(pos.z) ? 0 : pos.z);
            gameObject.transform.localRotation = new Quaternion(float.IsNaN(rot.x) ? 0 : rot.x, float.IsNaN(rot.y) ? 0 : rot.y, float.IsNaN(rot.z) ? 0 : rot.z, float.IsNaN(rot.w) ? 0 : rot.w);
            gameObject.transform.localScale = new Vector3(float.IsNaN(scale.x) ? 0 : scale.x, float.IsNaN(scale.y) ? 0 : scale.y, (float.IsNaN(scale.z) ? 0 : scale.z));
        }

        public void Remove()
        {
            UnityEngine.Object.Destroy(gameObject);
        }
    }

    public Dictionary<string, IRemoveLater> tempObjects = new();

    public void Draw(string id, Mesh mesh, Material target, Matrix p)
    {
        id = "Mesh." + id;
        if (!tempObjects.ContainsKey(id))
        {
            tempObjects.Add(id, new TempMesh(id, mesh, target, p));
        }
        else
        {
            ((TempMesh)tempObjects[id]).Reload(mesh, target, p);
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

    public Shader FontShader;

    public void InputDevices_Update(InputDevice inputDevice)
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        Debug.Log($"Devices Update Count:{devices.Count}");
        for (int i = 0; i < devices.Count; i++)
        {
            Debug.Log($"Devices {i} Name:{devices[i].name} Manufacturer:{devices[i].manufacturer} Manufacturer:{devices[i].characteristics}");
            var charictes = devices[i].characteristics;
            if ((charictes & InputDeviceCharacteristics.Left) != InputDeviceCharacteristics.None)
            {
                if ((charictes & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.None)
                {
                    left = devices[i];
                }
            }
            if ((charictes & InputDeviceCharacteristics.Right) != InputDeviceCharacteristics.None)
            {
                if ((charictes & InputDeviceCharacteristics.Controller) != InputDeviceCharacteristics.None)
                {
                    right = devices[i];
                }
            }
        }
    }

    void Update()
    {
        if(engine is null)
        {
            return;
        }
        MainThreadUpdate();
        var sens = speedMultply * Time.deltaTime;
        MouseDelta = new Vector2f(Input.GetAxis("Mouse X") * sens, -Input.GetAxis("Mouse Y") * sens);
        foreach (var item in tempObjects)
        {
            item.Value.UsedThisFrame = false;
        }
        MainThreadUpdate();
        engine.Step();
        MainThreadUpdate();
        var removethisframe = new List<string>();
        foreach (var item in tempObjects)
        {
            if (!item.Value.UsedThisFrame)
            {
                removethisframe.Add(item.Key);
                item.Value.Remove();
            }
        }
        foreach (var item in removethisframe)
        {
            tempObjects.Remove(item);
        }
        MainThreadUpdate();
    }
}
